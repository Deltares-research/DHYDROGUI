using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    [Entity]
    public partial class WaterFlowFMModel : TimeDependentModelBase, 
                                            IDisposable, 
                                            IHydroModel, 
                                            IHydFileModel, 
                                            IDimrModel, 
                                            IWaterFlowFMModel, 
                                            ISedimentModelData, 
                                            ICoupledModel
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (WaterFlowFMModel));
        
        public const string GridPropertyName = "Grid";
        
        private readonly FileSystem fileSystem;

        private DepthLayerDefinition depthLayerDefinition;
        private WaterFlowFMModelDefinition modelDefinition;
        private bool disposing;
        private bool updatingGroupName;
        private IHydroCoupling dimrCoupling;
        private IHydroCoupling hydroCoupling;

        private IList<ModelFeatureCoordinateData<FixedWeir>> allFixedWeirsAndCorrespondingProperties;
        private IEventedList<SourceAndSink> sourcesAndSinks;
        private IEventedList<ISedimentFraction> sedimentFractions;
        private IEventedList<BoundaryConditionSet> boundaryConditionSets;
        private List<Model1DBoundaryNodeData> boundaryConditionDataList;
        private IDataItem areaDataItem;
        private IDataItem networkDataItem;

        private readonly Dictionary<IFeature, List<IDataItem>> areaDataItems = new Dictionary<IFeature, List<IDataItem>>();
        private bool useLocalApi;

        public WaterFlowFMModel() : this(null)
        {
        }
        
        public WaterFlowFMModel(string mduFilePath, ImportProgressChangedDelegate progressChanged = null) : base("FlowFM")
        {
            InitializeModelProperties();
            
            AddNetworkToModel();
            AddAreaToModel();

            var hydroAreaParent = Area.Parent;
            var hydroNetworkParent = Network.Parent;

            fileSystem = new FileSystem();
            fmRegion = new HydroRegion{Name = Name, SubRegions = new EventedList<IRegion>{ Area, Network}};

            Area.Parent = hydroAreaParent;
            Network.Parent = hydroNetworkParent;
            
            DimrRunner = new DimrRunner(this);
            DimrRunner.FileExportService.RegisterFileExporter(new FMModelFileExporter());

            ((INotifyCollectionChanged) this).CollectionChanged += OnFMModelCollectionChanged;
            ((INotifyPropertyChanged) this).PropertyChanged += OnFMModelPropertyChanged;

            if (!string.IsNullOrEmpty(mduFilePath))
            {
                ReadFromMdu(mduFilePath, progressChanged);
                return;
            }

            ModelDefinition = new WaterFlowFMModelDefinition();
            ModelDefinition.SetModelProperty(KnownProperties.NetFile, Name + NetFile.FullExtension);
            SynchronizeModelDefinitions();

            Grid = new UnstructuredGrid();
            InitializeUnstructuredGridCoverages();

            AddSpatialDataItems();
            RenameSubFilesIfApplicable();

            UpdateRoughnessSections();
        }

        ~WaterFlowFMModel()
        {
            Dispose(false);
        }

        private void AddNetworkToModel()
        {
            // network
            var network = new HydroNetwork { Name = WaterFlowFMModelDataSet.NetworkTag };
            AddDataItem(network, DataItemRole.Input, WaterFlowFMModelDataSet.NetworkTag);
            networkDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag);
            SubscribeToNetwork(network);
            NetworkDiscretization = new Discretization
            {
                Network = network, 
                Name = DiscretizationObjectName, 
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered
            };
            
            // q's supplied by externals
            AddInflowsDataItem();
        }

        private void AddAreaToModel()
        {
            var area = new HydroArea();
            AddDataItem(area, DataItemRole.Input, WaterFlowFMModelDataSet.HydroAreaTag);
            areaDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.HydroAreaTag);
            SubscribeToEvents(area);
        }

        private void SubscribeToEvents(HydroArea area)
        {
            ((INotifyCollectionChanged) area).CollectionChanged += HydroAreaCollectionChanged;
            ((INotifyPropertyChanged) area).PropertyChanged += HydroAreaPropertyChanged;
        }
        
        private void InitializeModelProperties()
        {
            SedimentModelDataItem = new SedimentModelDataItem();
            SnapVersion = 0;
            ValidateBeforeRun = true;
            DisableFlowNodeRenumbering = false;
            TracerDefinitions = new EventedList<string>();
            SourcesAndSinks = new EventedList<SourceAndSink>();
            SedimentFractions = new EventedList<ISedimentFraction>();

            allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            BridgePillarsDataModel = new List<ModelFeatureCoordinateData<BridgePillar>>();

            SedimentOverallProperties = SedimentFractionHelper.GetSedimentationOverAllProperties();
            Links = new EventedList<ILink1D2D>();
            BoundaryConditions1D = new EventedList<Model1DBoundaryNodeData>();
            LateralSourcesData = new EventedList<Model1DLateralSourceData>();
            ChannelFrictionDefinitions = new EventedList<ChannelFrictionDefinition>();
            PipeFrictionDefinitions = new EventedList<PipeFrictionDefinition>();
            ChannelInitialConditionDefinitions = new EventedList<ChannelInitialConditionDefinition>();
            RoughnessSections = new EventedList<RoughnessSection>();
        }
        
        public WaterFlowFMModelDefinition ModelDefinition
        {
            get { return modelDefinition; }
            private set
            {
                if (modelDefinition != null)
                {
                    ((INotifyPropertyChanged) (modelDefinition.Properties)).PropertyChanged -= OnModelDefinitionPropertyChanged;
                }

                modelDefinition = value;

                OnModelDefinitionChanged();

                if (modelDefinition != null)
                {
                    ((INotifyPropertyChanged) (modelDefinition.Properties)).PropertyChanged += OnModelDefinitionPropertyChanged;
                }
            }
        }
        
        public bool DisableFlowNodeRenumbering { get; set; }
        
        public IList<ModelFeatureCoordinateData<BridgePillar>> BridgePillarsDataModel { get; private set; }

        public IEventedList<ILink1D2D> Links
        {
            get { return links; }
            set
            {
                if (links != null)
                {
                    ((INotifyPropertyChanged)(links)).PropertyChanged -= OnWaterFlowFm1D2DLinkPropertyChanged;
                }

                links = value;

                if (links != null)
                {
                    ((INotifyPropertyChanged)(links)).PropertyChanged += OnWaterFlowFm1D2DLinkPropertyChanged;
                }
            }
        }

        private void RefreshMappings()
        {
            Links1D2DHelper.SetIndexes1D2DLinks(Links, NetworkDiscretization, Grid);

            var linksWithout2dCell = Links.Where(l => l.FaceIndex == -1).ToArray();
            var linksWithout1dCompPoint = Links.Where(l => l.DiscretisationPointIndex == -1).ToArray();
            
            RemoveLinks(linksWithout2dCell, "Can not find the cell for the following links");
            RemoveLinks(linksWithout1dCompPoint, "Can not find the computation point for the following links");
        }

        private void RemoveLinks(ILink1D2D[] linksToRemove, string reason)
        {
            if (!linksToRemove.Any()) return;

            var linkInformation = linksToRemove.Select(l => $"{l.Name } ({l.Geometry})");

            Log.Warn($"{reason} (removing links) {Environment.NewLine}" +
                      $" {string.Join(Environment.NewLine, linkInformation)}");

            linksToRemove.ForEach(l => Links.Remove(l));
        }

        public DepthLayerDefinition DepthLayerDefinition
        {
            get { return depthLayerDefinition; }
            set
            {
                BeginEdit("Changing layer definition");
                depthLayerDefinition = value;
                ModelDefinition.Kmx = depthLayerDefinition.UseLayers ? depthLayerDefinition.NumLayers : 0;
                EndEdit();
            }
        }

        /// <summary>
        /// The reference date as a DateTime. The time part is always 0. The setter will extract only the date part.
        /// </summary>
        public DateTime ReferenceTime
        {
            get
            {
                return modelDefinition.GetReferenceDateAsDateTime();
            }
            set
            {
                modelDefinition.SetReferenceDateFromDatePartOfDateTime(value);
            }
        }

        private int CdType
        {
            get { return Convert.ToInt32(ModelDefinition.GetModelProperty(KnownProperties.ICdtyp).Value); }
        }

        public double MinimumSegmentLength
        {
            get
            {
                return Convert.ToDouble(ModelDefinition.GetModelProperty(KnownProperties.Dxmin1D).Value);
            }
        }

        public IEventedList<IWindField> WindFields { get; private set; }

        public IEventedList<IFmMeteoField> FmMeteoFields { get; private set; }

        public IList<IUnsupportedFileBasedExtForceFileItem> UnsupportedFileBasedExtForceFileItems { get; private set; }

        public HeatFluxModelType HeatFluxModelType
        {
            get { return heatFluxModelType; }
            private set{
                if (value != heatFluxModelType)
                {
                    ToggleTemperature(value != HeatFluxModelType.None);
                    heatFluxModelType = value;
                }}
        }

        public IList<ModelFeatureCoordinateData<FixedWeir>> FixedWeirsProperties
        {
            get { return allFixedWeirsAndCorrespondingProperties; }
        }

        public bool UseDepthLayers
        {
            get { return ModelDefinition.Kmx != 0; }
        }

        public bool UseSalinity
        {
            get { return (bool)ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value; }
        }

        public bool UseSecondaryFlow
        {
            get { return (bool)ModelDefinition.GetModelProperty(KnownProperties.SecondaryFlow).Value; }
        }

        public bool UseTemperature
        {
            get { return (HeatFluxModelType)ModelDefinition.GetModelProperty(KnownProperties.Temperature).Value != HeatFluxModelType.None; }
        }

        /// <summary>
        /// Whether this model uses spatial infiltration data. 
        /// </summary>
        public bool UseInfiltration => (int) ModelDefinition.GetModelProperty(KnownProperties.InfiltrationModel).Value == 2;

        private void ToggleTemperature(bool useTemperature)
        {
            if (UseTemperature == useTemperature) return;
            BoundaryConditions1D.ForEach(bc => bc.UseTemperature = useTemperature);
            LateralSourcesData.ForEach(lat => lat.UseTemperature = useTemperature);
        }

        public bool UseMorSed
        {
            get { return ModelDefinition.UseMorphologySediment; }
        }

        public bool WriteSnappedFeatures
        {
            get { return ModelDefinition.WriteSnappedFeatures; }
        }

        [PropertyGrid]
        [DisplayName("Validate before run")]
        [Category("Run mode")]
        public bool ValidateBeforeRun { get; set; }

        [PropertyGrid]
        [DisplayName("Show model run console")]
        [Category("Run mode")]
        public bool ShowModelRunConsole { get; set; }

        [PropertyGrid]
        [DisplayName("Use RPC")]
        [Description("For development only--remove at release")]
        [Category("Run mode")]
        public bool UseRPC
        {
            get { return !useLocalApi; }
            set { useLocalApi = !value; }
        }

        private void SynchronizeModelDefinitions()
        {
            HeatFluxModelType = ModelDefinition.HeatFluxModel.Type; // sync the heat flux model
            Boundaries = ModelDefinition.Boundaries;
            BoundaryConditionSets = ModelDefinition.BoundaryConditionSets;
            WindFields = ModelDefinition.WindFields;
            FmMeteoFields = ModelDefinition.FmMeteoFields;
            UnsupportedFileBasedExtForceFileItems = ModelDefinition.UnsupportedFileBasedExtForceFileItems;
            Pipes = ModelDefinition.Pipes;
            SourcesAndSinks = ModelDefinition.SourcesAndSinks;
            Inflows = ModelDefinition.Inflows;

            // read depth layer definition
            DepthLayerDefinition = ModelDefinition.Kmx == 0
                ? new DepthLayerDefinition(DepthLayerType.Single)
                : new DepthLayerDefinition(ModelDefinition.Kmx);

            syncers.Add(new FeatureDataSyncer<Feature2D, BoundaryConditionSet>(Boundaries, BoundaryConditionSets, CreateBoundaryCondition));
            syncers.Add(new FeatureDataSyncer<Feature2D, SourceAndSink>(Pipes, SourcesAndSinks, CreateSourceAndSink));
        }

        private void AddToIntialFractions(string spatiallyVaryingName)
        {
            if ( InitialFractions == null ) return;
            var t = DataItems.FirstOrDefault(di => di.Name == spatiallyVaryingName);
            if (t == null)
            {
                var unstructuredGridCellCoverage = CreateUnstructuredGridCellCoverage(spatiallyVaryingName, Grid);
                InitialFractions.Add(unstructuredGridCellCoverage);
            }
            else
            {
                var unstrGridCellCoverage = t.Value as UnstructuredGridCellCoverage;
                if (unstrGridCellCoverage == null)
                {
                    t.Value = CreateUnstructuredGridCellCoverage(spatiallyVaryingName, Grid);
                    InitialFractions.Add((UnstructuredGridCellCoverage) t.Value);
                    /* DELFT3DFM-1077 
                     * Apparently the spatial operation is not being executed after being added (which should be)
                     * We can force it here.
                     */
                    var spOperationSet = t.ValueConverter as SpatialOperationSetValueConverter;
                    if (spOperationSet != null) spOperationSet.SpatialOperationSet.Execute();
                }
                else
                {
                    if (!InitialFractions.Contains(unstrGridCellCoverage))
                    {
                        InitialFractions.Add(unstrGridCellCoverage);
                    }
                }
            }
        }

        private void SyncInitialFractions(ISedimentFraction sedimentFraction)
        {
            foreach (var layerName in sedimentFraction.GetAllActiveSpatiallyVaryingPropertyNames())
            {
                if (InitialFractions.FirstOrDefault(fr => fr.Name.Equals(layerName)) == null)
                {
                    AddToIntialFractions(layerName);
                }
            }
        }

        private void AddSedimentFractionToFlowBoundaryConditionFunction(string name)
        {
            foreach (var set in BoundaryConditionSets)
            {
                foreach (var bc in set.BoundaryConditions)
                {
                    var flowCondition = bc as FlowBoundaryCondition;
                    if (flowCondition != null
                        && flowCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport)
                    {
                        foreach (var point in bc.PointData)
                        {
                            flowCondition.AddSedimentFractionToFunction(point, name);
                        }
                    }
                }
            }
        }

        private void RemoveSedimentFractionFromBoundaryConditionSets(string name)
        {
            foreach (var set in BoundaryConditionSets)
            {
                set.BoundaryConditions.RemoveAllWhere(bc =>
                {
                    var flowCondition = bc as FlowBoundaryCondition;

                    if (flowCondition != null &&
                        flowCondition.FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration &&
                        Equals(flowCondition.SedimentFractionName, name))
                    {
                        return true;
                    }

                    return false;
                });

                foreach (var bc in set.BoundaryConditions)
                {
                    var flowCondition = bc as FlowBoundaryCondition;
                    if (flowCondition != null
                        && flowCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport)
                    {
                        foreach (var point in bc.PointData)
                        {
                            flowCondition.RemoveSedimentFractionFromFunction(point, name);
                        }
                    }
                }


                set.BoundaryConditions.RemoveAllWhere(bc =>
                {
                    var flowCondition = bc as FlowBoundaryCondition;

                    if (flowCondition != null &&
                        flowCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport
                        && (flowCondition.SedimentFractionNames == null || flowCondition.SedimentFractionNames.Count == 0))
                    {
                        return true;
                    }

                    return false;
                });
            }
        }
        
        private void RemoveAllSedimentFractionsFromBoundaryConditionSets()
        {
            foreach (var set in BoundaryConditionSets)
            {
                set.BoundaryConditions.RemoveAllWhere(bc =>
                {
                    var flowCondition = bc as FlowBoundaryCondition;
                    return flowCondition != null &&
                           (flowCondition.FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration
                            || flowCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport);
                });
            }
        }
        
        private IEventedList<IDataItem> AddSpatialDataItems()
        {
            AddOrRenameDataItem(Bathymetry, WaterFlowFMModelDefinition.BathymetryDataItemName);

            // Backwards compatibility
            // BedLevel dataitem value used to be exclusively UnstructuredGridVertexCoverages, now it needs to be more generic
            var bedLevelDataItem = DataItems.FirstOrDefault(di => di.Name == WaterFlowFMModelDefinition.BathymetryDataItemName);
            if (bedLevelDataItem != null) bedLevelDataItem.ValueType = typeof(UnstructuredGridCoverage);
            var initialWaterQuantityNameType = (InitialConditionQuantity)(int)ModelDefinition
                .GetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D).Value;
            AddOrRenameDataItem(InitialWaterLevel, initialWaterQuantityNameType == InitialConditionQuantity.WaterLevel
                ? WaterFlowFMModelDefinition.InitialWaterLevelDataItemName
                : WaterFlowFMModelDefinition.InitialWaterDepthDataItemName);

            AddOrRenameDataItem(Roughness, WaterFlowFMModelDefinition.RoughnessDataItemName);
            AddOrRenameDataItem(Viscosity, WaterFlowFMModelDefinition.ViscosityDataItemName);
            AddOrRenameDataItem(Diffusivity, WaterFlowFMModelDefinition.DiffusivityDataItemName);
            AddOrRenameDataItem(Infiltration, WaterFlowFMModelDefinition.InfiltrationDataItemName);
            AddOrRenameDataItem(InitialTemperature, WaterFlowFMModelDefinition.InitialTemperatureDataItemName);
            AddOrRenameDataItems(InitialSalinity, WaterFlowFMModelDefinition.InitialSalinityDataItemName);
            AddOrRenameTracerDataItems();
            AddOrRenameFractionDataItems();

            return DataItems;
        }

        private void AddOrRenameTracerDataItems()
        {
            foreach (var initialTracer in InitialTracers)
            {
                AddOrRenameDataItem(initialTracer, initialTracer.Name);
            }
        }

        private void AddOrRenameFractionDataItems()
        {
            foreach (var initialFraction in InitialFractions)
            {
                AddOrRenameDataItem(initialFraction, initialFraction.Name);
            }
        }

        private void AddOrRenameDataItem(ICoverage coverage, string name)
        {
            var existingDataItem = GetDataItemByValue(coverage);
            if (existingDataItem == null)
            {
                DataItems.Add(new DataItem(coverage, name) {Role = DataItemRole.Input});
            }
            else
            {
                if (existingDataItem.Name != name)
                {
                    existingDataItem.Name = name;
                }
            }
        }

        #region IHasCoordinateSystem

        public ICoordinateSystem CoordinateSystem
        {
            get { return ModelDefinition.CoordinateSystem; }
            set
            {
                if (Equals(ModelDefinition.CoordinateSystem, value))
                    return;

                ModelDefinition.CoordinateSystem = value;
                
                if (Area != null)
                {
                    Area.CoordinateSystem = value;
                }

                if (Network != null)
                {
                    Network.CoordinateSystem = value;
                }

                if (fmRegion != null)
                {
                    fmRegion.CoordinateSystem = value;
                }

                if (Grid != null)
                {
                    Grid.CoordinateSystem = value;
                }

                if (OutputHisFileStore != null)
                {
                    OutputHisFileStore.CoordinateSystem = value;
                }

                if (OutputMapFileStore != null)
                {
                    OutputMapFileStore.SetCoordinateSystem(value);
                }

                if (Output1DFileStore != null)
                {
                    Output1DFileStore.CoordinateSystem = value;
                }
                
                if (OutputClassMapFileStore != null)
                {
                    OutputClassMapFileStore.CoordinateSystem = value;
                }
                
                if (Network != null && Network.CoordinateSystem != value)
                {
                    Network.CoordinateSystem = value;
                }
                
                // coverages are handled via the feature collections.

                InvalidateSnapping();
            }
        }

        public bool CanSetCoordinateSystem(ICoordinateSystem potentialCoordinateSystem)
        {
            return WaterFlowFMModelCoordinateConversion.CanAssignCoordinateSystem(this, potentialCoordinateSystem);
        }

        public void TransformCoordinates(ICoordinateTransformation transformation)
        {
            BeginEdit("Converting model coordinates");

            WaterFlowFMModelCoordinateConversion.ConvertModel(this, transformation);

            
            EndEdit();
        }

        public static bool IsValidCoordinateSystem(ICoordinateSystem coordinateSystem)
        {
            return !coordinateSystem.IsGeographic || coordinateSystem.Name == "WGS 84";
        }

        #endregion

        #region Area

        private readonly IList<IDisposable> syncers = new List<IDisposable>();

        public HydroArea Area
        {
            get
            {
                if (areaDataItem == null)
                    areaDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.HydroAreaTag);

                return (HydroArea) GetDataItemValueByTag(WaterFlowFMModelDataSet.HydroAreaTag);
            }
            set
            {
                var areaItem = GetDataItemByTag(WaterFlowFMModelDataSet.HydroAreaTag);
                
                if (areaItem.Value != null) 
                {
                    ((INotifyCollectionChanged)areaItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
                    ((INotifyPropertyChanged)value).PropertyChanged -= HydroAreaPropertyChanged;
                }

                allFixedWeirsAndCorrespondingProperties.Clear();
                BridgePillarsDataModel.Clear();

                areaItem.Value = value;

                if (value != null)
                {
                    value.FixedWeirs.ForEach(fw => allFixedWeirsAndCorrespondingProperties.Add(CreateModelFeatureCoordinateDataFor(fw)));
                    value.BridgePillars.ForEach( bp => BridgePillarsDataModel.Add(CreateModelFeatureCoordinateDataFor(bp)));

                    ((INotifyCollectionChanged)value).CollectionChanged += HydroAreaCollectionChanged;
                    ((INotifyPropertyChanged) value).PropertyChanged += HydroAreaPropertyChanged;
                }
            }
        }
        
        public IEventedList<Feature2D> Boundaries { get; private set; }

        public IEventedList<BoundaryConditionSet> BoundaryConditionSets
        {
            get { return boundaryConditionSets; }
            private set
            {
                if (boundaryConditionSets != null)
                {
                    BoundaryConditionSets.CollectionChanged -= BoundaryConditionSetsCollectionChanged;
                }

                boundaryConditionSets = value;

                if (boundaryConditionSets != null)
                {
                    BoundaryConditionSets.CollectionChanged += BoundaryConditionSetsCollectionChanged;
                }
            }
        }

        public IEventedList<Feature2D> Pipes { get; private set; }

        public IEventedList<SourceAndSink> SourcesAndSinks
        {
            get { return sourcesAndSinks; }
            set
            {
                if (sourcesAndSinks != null)
                {
                    SourcesAndSinks.CollectionChanged -= SourcesAndSinksCollectionChanged;
                }
                sourcesAndSinks = value;
                if (sourcesAndSinks != null)
                {
                    SourcesAndSinks.CollectionChanged += SourcesAndSinksCollectionChanged;
                }
            }
        }

        public virtual IEnumerable<IBoundaryCondition> BoundaryConditions
        {
            get { return ModelDefinition.BoundaryConditions; }
        }

        private static BoundaryConditionSet CreateBoundaryCondition(Feature2D feature)
        {
            return new BoundaryConditionSet {Feature = feature};
        }

        private static SourceAndSink CreateSourceAndSink(Feature2D feature)
        {
            return new SourceAndSink {Feature = feature};
        }

        #endregion

        #region Spatial data

        public bool InitialCoverageSetChanged { get; set; }

        #endregion

        #region Mdu file

        public bool WriteHisFile
        {
            get { return (bool) ModelDefinition.GetModelProperty(GuiProperties.WriteHisFile).Value; }
        }

        public bool SpecifyHisStart
        {
            get { return (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyHisStart).Value; }
        }

        public bool SpecifyHisStop
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyHisStop).Value; }
        }

        public bool WriteMapFile
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value; }
        }

        public bool SpecifyMapStart
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyMapStart).Value; }
        }

        public bool SpecifyMapStop
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyMapStop).Value; }
        }

        public bool WriteRstFile
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value; }
        }

        public bool SpecifyRstStart
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyRstStart).Value; }
        }

        public bool SpecifyRstStop
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyRstStop).Value; }
        }

        public bool SpecifyWaqOutputInterval
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).Value; }
        }

        public bool SpecifyWaqOutputStartTime
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStartTime).Value; }
        }

        public bool SpecifyWaqOutputStopTime
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStopTime).Value; }
        }
        
        public object WaveModel
        {
            // cannot actually return anything, because it's a dynamic enum
            get { return null; }
        }

        public bool WriteClassMapFile
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.WriteClassMapFile).Value; }
        }

        #endregion

        private void AddOrRenameDataItems(CoverageDepthLayersList coverageDepthLayersList, string name)
        {
            var i = 1;
            var uniform = coverageDepthLayersList.VerticalProfile.Type == VerticalProfileType.Uniform;

            foreach (var coverage in coverageDepthLayersList.Coverages)
            {
                var numberedName = uniform ? name : (name + "_" + (i++));
                AddOrRenameDataItem(coverage, numberedName);
            }
        }
        
        private IEventedList<string> tracerDefinitions;
        private bool isLoading;
        private IEventedList<ILink1D2D> links;
        private HeatFluxModelType heatFluxModelType;
        private IHydroRegion fmRegion;
        private ValidationReport report;

        public const int TOTALSTEPS = 32;
        private int currentStep = 0;

        #region Coupling

        private void CreatePointFeatureOfThisLeveeBreach(ILeveeBreach leveeFeature, LeveeBreachPointLocationType leveeBreachPointLocationType, IGeometry leveeFeatureBreachLocation)
        {
            if (((IEventedList<Feature2D>)Area.LeveeBreaches).SingleOrDefault(lpf =>
                    lpf.Attributes != null &&
                    lpf.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE) &&
                    lpf.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE) &&
                    lpf.Attributes[LeveeBreach.LEVEE_BREACH_FEATURE].Equals(leveeFeature) &&
                    (LeveeBreachPointLocationType)lpf.Attributes[LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE] ==
                    leveeBreachPointLocationType) == null)
            {
                var feature2DPoint = new Feature2DPoint
                {
                    Name = leveeFeature.Name + " : " + leveeBreachPointLocationType.GetDescription(),
                    Geometry = leveeFeatureBreachLocation,
                    Attributes = new DictionaryFeatureAttributeCollection()
                    {
                        {LeveeBreach.LEVEE_BREACH_FEATURE, leveeFeature},
                        {LeveeBreach.LEVEE_BREACH_POINT_LOCATION_TYPE, leveeBreachPointLocationType}
                    }
                };
                feature2DPoint.PropertyChanged += Feature2DPointOnPropertyChanged;
                Area.LeveeBreaches.Add(feature2DPoint);
            }
        }
        
        private ModelFeatureCoordinateData<FixedWeir> CreateModelFeatureCoordinateDataFor(FixedWeir fixedWeir)
        {
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<FixedWeir> {Feature = fixedWeir};
            var scheme = ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).GetValueAsString();

            modelFeatureCoordinateData.UpdateDataColumns(scheme);
            return modelFeatureCoordinateData;
        }

        private ModelFeatureCoordinateData<BridgePillar> CreateModelFeatureCoordinateDataFor(BridgePillar bridgePillar)
        {
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar> { Feature = bridgePillar };
            modelFeatureCoordinateData.UpdateDataColumns();

            return modelFeatureCoordinateData;
        }

        public virtual string GetFeatureCategory(IFeature feature)
        {
            if (feature is IGate)
            {
                return Model1DParametersCategories.Gates;
            }
            if (Area.ObservationCrossSections.Contains(feature))
            {
                return Model1DParametersCategories.CrossSections;
            }
            if (SourcesAndSinks?.Any(ss => ss.Feature.Equals(feature)) ?? false)
            {
                return Model1DParametersCategories.SourceSinks;
            }
            if (feature is IPump)
            {
                return Model1DParametersCategories.Pumps;
            }
            if (feature is IWeir weir)
            {
                if (weir is Orifice)
                {
                    return Model1DParametersCategories.Orifices;
                }

                if (weir.WeirFormula is GeneralStructureWeirFormula)
                {
                    return Model1DParametersCategories.GeneralStructures;
                }
                if (weir.WeirFormula is GatedWeirFormula)
                {
                    return Model1DParametersCategories.Gates;
                }
                return Model1DParametersCategories.Weirs;
            }
            if (feature is ICulvert)
            {
                return Model1DParametersCategories.Culverts;
            }
            if (feature is IObservationPoint || Area.ObservationPoints.Contains(feature))
            {
                return Model1DParametersCategories.ObservationPoints;
            }
            if (feature is IRetention)
            {
                return Model1DParametersCategories.Retentions;
            }
            if (feature is ILateralSource)
            {
                return Model1DParametersCategories.Laterals;
            }
            if (feature is IHydroNode)
            {
                return Model1DParametersCategories.BoundaryConditions;
            }
            if (feature is LeveeBreach leveeBreach && Area.LeveeBreaches.Contains(leveeBreach))
            {
                return Model1DParametersCategories.LeveeBreaches;
            }

            return null;
        }

        #endregion

        #region IHydroModel

        public IHydroRegion Region
        {
            get { return fmRegion; }
        }
        
        public IEventedList<string> TracerDefinitions
        {
            get { return tracerDefinitions; }
            private set
            {
                if (tracerDefinitions != null)
                {
                    TracerDefinitions.CollectionChanged -= TracerDefinitionsCollectionChanged;
                }

                tracerDefinitions = value;

                if (tracerDefinitions != null)
                {
                    TracerDefinitions.CollectionChanged += TracerDefinitionsCollectionChanged;
                }
            }
        }

        public IEventedList<ISedimentProperty> SedimentOverallProperties { get; set; }
        
        public IEventedList<ISedimentFraction> SedimentFractions
        {
            get { return sedimentFractions; }
            set
            {
                if (sedimentFractions != null)
                {
                    ((INotifyPropertyChanged) SedimentFractions).PropertyChanged -= SedimentFractionPropertyChanged;
                    SedimentFractions.CollectionChanged -= SedimentFractionsCollectionChanged;
                }
                sedimentFractions = value;
                if (sedimentFractions != null)
                {
                    ((INotifyPropertyChanged)SedimentFractions).PropertyChanged += SedimentFractionPropertyChanged;
                    SedimentFractions.CollectionChanged += SedimentFractionsCollectionChanged;
                }
            }
        }
        #endregion

        #region ISedimentModelData implementation
        private SedimentModelDataItem SedimentModelDataItem { get; set; }

        public SedimentModelDataItem GetSedimentDataItem()
        {
            SedimentModelDataItem.SpacialVariableNames = SedimentFractions
                .SelectMany(s => s.GetAllActiveSpatiallyVaryingPropertyNames())
                .Where(n => !n.EndsWith("SedConc")).ToList();

            var dataItemsFound = SedimentModelDataItem.SpacialVariableNames.SelectMany(spaceVarName => DataItems.Where(di => di.Name.Equals(spaceVarName))).ToArray();
            var dataItemsWithConverter = dataItemsFound.Where(d => d.ValueConverter is SpatialOperationSetValueConverter).ToList();
            var dataItemsWithOutConverter = dataItemsFound.Except(dataItemsWithConverter).ToList();

            SedimentModelDataItem.SpatialOperation = GetSpatialOperationsLookupTable(dataItemsWithConverter);
            SedimentModelDataItem.Coverages = dataItemsWithOutConverter.Select(di => di.Value)
                .OfType<UnstructuredGridCoverage>()
                .GroupBy(c => c.GetType())
                .ToList();
            SedimentModelDataItem.DataItemNameLookup = dataItemsWithOutConverter.ToDictionary(di => di.Value, di => di.Name);

            return SedimentModelDataItem;
        }

        public Dictionary<string, IList<ISpatialOperation>> GetSpatialOperationsLookupTable(List<IDataItem> dataItemsWithConverter)
        {
            var spatialOperationsLookupTable = new Dictionary<string, IList<ISpatialOperation>>();
            foreach (var dataItem in dataItemsWithConverter)
            {
                var spatialOperationValueConverter = (SpatialOperationSetValueConverter)dataItem.ValueConverter;
                if (
                    spatialOperationValueConverter.SpatialOperationSet.Operations.All(
                        WaterFlowFMModelDefinition.SupportedByExtForceFile))
                {
                    // put in everything except spatial operation sets,
                    // because we only use interpolate commands that will grab the importsamplesoperation via the input parameters.
                    var spatialOperation = spatialOperationValueConverter.SpatialOperationSet.GetOperationsRecursive()
                        .Where(s => !(s is ISpatialOperationSet))
                        .Select(WaterFlowFMModelDefinition.ConvertSpatialOperation)
                        .ToList();

                    spatialOperationsLookupTable.Add(dataItem.Name, spatialOperation);
                }
                // null check to see if it has a final coverage. It could be that there are only point clouds in the set.
                else if (spatialOperationValueConverter.SpatialOperationSet.Output.Provider != null)
                {
                    // unsupported operations are converted to sample operations that are saved with an xyz file via the model definition.
                    var coverage =
                        spatialOperationValueConverter.SpatialOperationSet.Output.Provider.Features[0] as
                            UnstructuredGridCoverage;

                    // In the event that the coverage is comprised entirely of non-data values, ignore it and continue
                    // (This can happen when exporting spatial operations that comprise of added points but no interpolation
                    // - we're not interested in these for the mdu, they will be saved as dataitems to the dsproj)
                    if (coverage == null || (coverage.Components[0].NoDataValues != null &&
                                             coverage.GetValues<double>()
                                                 .All(v => coverage.Components[0].NoDataValues.Contains(v))))
                    {
                        continue;
                    }

                    var newOperation = new AddSamplesOperation(false)
                    {
                        Name = spatialOperationValueConverter.SpatialOperationSet.Name
                    };
                    newOperation.SetInputData(AddSamplesOperation.SamplesInputName,
                        new PointCloudFeatureProvider
                        {
                            PointCloud = coverage.ToPointCloud(0, true),
                        });

                    spatialOperationsLookupTable.Add(dataItem.Name, new[] { newOperation });
                }
            }
            return spatialOperationsLookupTable;
        }
        #endregion

        private void CacheTimes()
        {
            cachedStartTime = StartTime;
            cachedEndTime = StopTime;
        }

        private void CleanCacheTimes()
        {
            cachedStartTime = null;
            cachedEndTime = null;
        }

        public IEnumerable<IDataItem> GetDataItemsUsedForCouplingModel(DataItemRole role)
        {
            return GetChildDataItemLocations(role).SelectMany(GetChildDataItems);
        }
        
        /// <summary>
        /// The hydro model coupling for this <see cref="WaterFlowFMModel"/>.
        /// </summary>
        /// <remarks>
        /// Always returns an up-to-date <see cref="IHydroCoupling"/>.
        /// Does not return <c>null</c>.
        /// </remarks>
        public IHydroCoupling HydroCoupling
        {
            get
            {
                if (hydroCoupling == null || hydroCoupling.HasEnded)
                {
                    hydroCoupling = new HydroCoupling();
                }

                return hydroCoupling;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            this.disposing = disposing;

            if (disposing)
            {
                ((INotifyCollectionChanged) this).CollectionChanged -= OnFMModelCollectionChanged;
                ((INotifyPropertyChanged) this).PropertyChanged -= OnFMModelPropertyChanged;
                
                var points2DFeatures = Area?.LeveeBreaches?.Where(f2d =>
                                                                      f2d.Attributes != null &&
                                                                      f2d.Attributes.ContainsKey(LeveeBreach.LEVEE_BREACH_FEATURE));
                if (points2DFeatures != null)
                    foreach (var points2DFeature in points2DFeatures)
                    {
                        points2DFeature.PropertyChanged -= Feature2DPointOnPropertyChanged;
                    }

                // also disposes grid snap api, so if you remove this, at least make sure you dispose that one (holds remote instance in the air):
                Grid = null;
                DisposeSnapApi();
                DimrRunner?.Dispose();
                syncers.ForEach(s => s.Dispose());
                syncers.Clear();

                allFixedWeirsAndCorrespondingProperties.ForEach(d => d.Dispose());
                BridgePillarsDataModel.ForEach(d => d.Dispose());
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}