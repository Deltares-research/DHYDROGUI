using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using BasicModelInterface;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Editing;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.Utils;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Helpers.CopyHandlers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Spatial;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
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
        private readonly DimrRunner runner;
        public const string CellsToFeaturesName = "CellsToFeatures";
        
        public const string DisableFlowNodeRenumberingPropertyName = "DisableFlowNodeRenumbering";
        public const string GridPropertyName = "Grid";
        private DepthLayerDefinition depthLayerDefinition;
        private WaterFlowFMModelDefinition modelDefinition;
        private bool disposing;
        private bool updatingGroupName;
        private IDimrCoupling dimrCoupling;

        private IList<ModelFeatureCoordinateData<FixedWeir>> allFixedWeirsAndCorrespondingProperties;
        private IEventedList<SourceAndSink> sourcesAndSinks;
        private IEventedList<ISedimentFraction> sedimentFractions;
        private IEventedList<BoundaryConditionSet> boundaryConditionSets;
        private List<Model1DBoundaryNodeData> boundaryConditionDataList;
        private IDataItem areaDataItem;
        private IDataItem networkDataItem;

        private readonly Dictionary<IFeature, List<IDataItem>> areaDataItems = new Dictionary<IFeature, List<IDataItem>>();
        private double previousProgress;
        private string progressText;
        private bool useLocalApi;

        private CacheFile cacheFile = null;

        public WaterFlowFMModel() : this(null)
        {
        }
        
        public WaterFlowFMModel(string mduFilePath, ImportProgressChangedDelegate progressChanged = null) : base("FlowFM")
        {
            runner = new DimrRunner(this, new DimrApiFactory());
            
            InitializeModelProperties();
            
            AddNetworkToModel();
            AddAreaToModel();

            var hydroAreaParent = Area.Parent;
            var hydroNetworkParent = Network.Parent;
            
            fmRegion = new HydroRegion{Name = Name, SubRegions = new EventedList<IRegion>{ Area, Network}};

            Area.Parent = hydroAreaParent;
            Network.Parent = hydroNetworkParent;

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

        private void CreateDataItemsNotAvailableInPreviousVersion()
        {
            if (GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag) == null)
            {
                AddNetworkToModel();
            }
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
            ((INotifyCollectionChanged) this).CollectionChanged += (s, e) => { MarkDirty(); };
            ((INotifyPropertyChanged) this).PropertyChanged += OnFMModelPropertyChanged;
        }

        private void InitializeModelProperties()
        {
            SedimentModelDataItem = new SedimentModelDataItem();
            SnapVersion = 0;
            ValidateBeforeRun = true;
            DisableFlowNodeRenumbering = false;
            TracerDefinitions = new EventedList<string>();
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

        public Func<string> WorkingDirectoryPathFunc { get; set; } = () => Path.Combine(DefaultModelSettings.DefaultDeltaShellWorkingDirectory);

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

        private void LoadLinks()
        {
            if (!File.Exists(NetFilePath)) return;
            var loadedLinks = UGridFileHelper.Read1D2DLinks(NetFilePath);

            if (NetworkDiscretization == null || Grid == null) return;
            Links1D2DHelper.SetGeometry1D2DLinks(loadedLinks, NetworkDiscretization.Locations, Grid.Cells);
            Links = new EventedList<ILink1D2D>(loadedLinks);
        }

        public override IBasicModelInterface BMIEngine
        {
            get { return runner.Api; }
        }

        public DepthLayerDefinition DepthLayerDefinition
        {
            get { return depthLayerDefinition; }
            set
            {
                BeginEdit(new DefaultEditAction("Changing layer definition"));
                depthLayerDefinition = value;
                ModelDefinition.Kmx = depthLayerDefinition.UseLayers ? depthLayerDefinition.NumLayers : 0;
                EndEdit();
            }
        }

        public DateTime ReferenceTime
        {
            get
            {
                return (DateTime) ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value;
            }
            set { ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value = value; }
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

        protected override void OnAfterDataItemsSet()
        {
            base.OnAfterDataItemsSet();

            var areaDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.HydroAreaTag);
            if (areaDataItem != null)
            {
                ((INotifyCollectionChange) areaDataItem.Value).CollectionChanged += HydroAreaCollectionChanged;
                ((INotifyPropertyChanged) areaDataItem.Value).PropertyChanged += HydroAreaPropertyChanged;
            }
            networkDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag);
            if (networkDataItem != null)
            {
                SubscribeToNetwork(networkDataItem.Value as IHydroNetwork);
            }
        }

        protected override void OnBeforeDataItemsSet()
        {
            base.OnBeforeDataItemsSet();

            areaDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.HydroAreaTag);
            if (areaDataItem != null)
            {
                ((INotifyCollectionChange)areaDataItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
                ((INotifyPropertyChanged)areaDataItem.Value).PropertyChanged -= HydroAreaPropertyChanged;
            }
            networkDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag);
            if (networkDataItem != null)
            {
                UnSubscribeFromNetwork(networkDataItem.Value as IHydroNetwork);
            }
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

        private void SyncFractionsAndTracers(SourceAndSink sourceAndSink)
        {
            SedimentFractions.ForEach(sf => sourceAndSink.SedimentFractionNames.Add(sf.Name));

            BoundaryConditionSets.ForEach(bcs =>
            {
                bcs.BoundaryConditions.ForEach(bc =>
                {
                    var flowCondition = bc as FlowBoundaryCondition;
                    if (flowCondition != null && flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                    {
                        var tracerName = flowCondition.TracerName;
                        if (!sourceAndSink.TracerNames.Contains(tracerName))
                            sourceAndSink.TracerNames.Add(tracerName);
                    }
                });
            });
        }

        private void RemoveTracerFromSourcesAndSink(string name)
        {
            if(BoundaryConditions.OfType<FlowBoundaryCondition>().All(bc => bc.TracerName != name))
                SourcesAndSinks.ForEach(ss => ss.TracerNames.Remove(name));
        }

        private void AddTracerToSourcesAndSink(string name)
        {
            SourcesAndSinks.ForEach(ss =>
            {
                if(!ss.TracerNames.Contains(name))
                    ss.TracerNames.Add(name);
            });
        }

        private void AssembleTracerDefinitions()
        {
            foreach (var boundaryCondition in BoundaryConditions)
            {
                var flowCondition = boundaryCondition as FlowBoundaryCondition;
                if (flowCondition != null && flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                {
                    if(!TracerDefinitions.Contains(flowCondition.TracerName))
                    {
                        TracerDefinitions.Add(flowCondition.TracerName);
                    }
                    AddTracerToSourcesAndSink(flowCondition.TracerName);
                }
            }
            var sp = SedimentFractions.SelectMany(sf => sf.GetAllActiveSpatiallyVaryingPropertyNames()).Distinct();
            foreach (var quantity in ModelDefinition.SpatialOperations.Keys.Except(WaterFlowFMModelDefinition.SpatialDataItemNames).Except(sp))
            {
                if (!TracerDefinitions.Contains(quantity))
                {
                    TracerDefinitions.Add(quantity);
                }
            }
        }

        private void AssembleSpatiallyVaryingSedimentProperties()
        {
            var spatiallyVaryingSedimentProperties = SedimentFractions.SelectMany(f => f.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().Where(sp => sp.IsSpatiallyVarying)).ToList();
            spatiallyVaryingSedimentProperties.AddRange(SedimentFractions.Where(f=> f.CurrentFormulaType != null ).SelectMany(f => f.CurrentFormulaType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().Where(sp => sp.IsSpatiallyVarying)));
            foreach (var spatiallyVaryingSedimentProperty in spatiallyVaryingSedimentProperties)
            {
                AddToIntialFractions(spatiallyVaryingSedimentProperty.SpatiallyVaryingName);
            }
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

        private void ImportSpatialOperationsAfterCreating(IEventedList<IDataItem> modelDataItems)
        {
            foreach (var spatialOperation in ModelDefinition.SpatialOperations)
            {
                var dataItemName = spatialOperation.Key;
                var spatialOperationList = spatialOperation.Value;
                var dataItem = modelDataItems.FirstOrDefault(di => di.Name == dataItemName);

                if (!spatialOperationList.Any()) continue;

                if (dataItem == null)
                {
                    Log.Error("No data item found with name " + dataItemName);
                    continue;
                }

                if (dataItem.ValueConverter == null)
                {
                    dataItem.ValueConverter = SpatialOperationValueConverterFactory.Create(dataItem.Value, dataItem.ValueType);
                }
                var valueConverter = dataItem.ValueConverter as SpatialOperationSetValueConverter;
                if (valueConverter == null) continue;

                valueConverter.SpatialOperationSet.Operations.Clear();

                foreach (var operation in spatialOperationList)
                {
                    // samples should directly be applied to the coverage with an interpolate operation
                    var importSamplesSpatialOperationExtension = operation as ImportSamplesOperationImportData;
                    if (importSamplesSpatialOperationExtension != null)
                    {
                        var operations = importSamplesSpatialOperationExtension.CreateOperations();
                        valueConverter.SpatialOperationSet.AddOperation(operations.First);
                        valueConverter.SpatialOperationSet.AddOperation(operations.Second);
                    }
                    else
                    {
                        valueConverter.SpatialOperationSet.AddOperation(operation);
                    }
                }
                
                SpatialOperationHelper.MakeNamesUniquePerSet(valueConverter.SpatialOperationSet);
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

        public override IEnumerable<IDataItem> AllDataItems
        {
            get
            {
                var lateralDataItems = LateralSourcesData.Select(d => d.SeriesDataItem);

                return base.AllDataItems.Concat(areaDataItems.Values.SelectMany(v => v)).Concat(lateralDataItems);
            }
        }

        /// <summary>
        /// Sync properties that are both in the model and the model definition.
        /// </summary>
        [EditAction]
        private void OnModelDefinitionChanged()
        {
            HeatFluxModelType = ModelDefinition.HeatFluxModel.Type;
            WindFields = ModelDefinition.WindFields;
            UnsupportedFileBasedExtForceFileItems = ModelDefinition.UnsupportedFileBasedExtForceFileItems;
        }

        private void InitializeRunTimeGridOperationApi()
        {
            if (runTimeGridOperationApi != null)
            {
                runTimeGridOperationApi.Dispose();
            }
            runTimeGridOperationApi = new UnstrucGridOperationApi(this);
        }
        #region TimedependentModelBase

        public override IEnumerable<object> GetDirectChildren()
        {
            foreach (var item in base.GetDirectChildren())
                yield return item;

            foreach (var boundary in Boundaries)
            {
                yield return boundary;
            }
            foreach (var model1DBoundaryNodeData in BoundaryConditions1D)
            {
                yield return model1DBoundaryNodeData;
            }
            foreach (var model1DLateralSourceData in LateralSourcesData)
            {
                yield return model1DLateralSourceData;
            }
            foreach (var pipe in Pipes)
            {
                yield return pipe;
            }

            foreach (var boundaryConditionSet in BoundaryConditionSets)
            {
                yield return boundaryConditionSet;
            }

            foreach (var sourcesAndSink in SourcesAndSinks)
            {
                yield return sourcesAndSink;
            }

            if (ModelDefinition.HeatFluxModel.MeteoData != null)
            {
                yield return ModelDefinition.HeatFluxModel;
            }

            yield return WindFields;

            foreach (var windField in WindFields)
            {
                yield return windField;
            }

            yield return Links;

            foreach (var link in Links)
            {
                yield return link;
            }

            yield return InitialSalinity;
            yield return Viscosity;
            yield return Diffusivity;
            yield return Roughness;
            yield return Infiltration;
            yield return InitialWaterLevel;
            yield return InitialTemperature;
            yield return InitialTracers;
            yield return InitialFractions;
            yield return Network;

            // for QueryTimeSeries tool:
            if (OutputHisFileStore != null)
                foreach (var featureCoverage in OutputHisFileStore.Functions)
                    yield return featureCoverage;

            if (OutputMapFileStore != null)
                foreach (var function in OutputMapFileStore.Functions)
                    yield return function;
            if (Output1DFileStore != null)
                foreach (var function in Output1DFileStore.Functions)
                    yield return function;
            if (OutputClassMapFileStore != null)
            {
                foreach (IFunction function in OutputClassMapFileStore.Functions)
                {
                    yield return function;
                }
            }
        }

        public override IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
        {
            if ((role & DataItemRole.Input) == DataItemRole.Input)
            {
                foreach (var inputFeature2D in InputFeatureCollections.OfType<IList>().SelectMany(l => l.OfType<IFeature>()).ToArray())
                {
                    yield return inputFeature2D;
                }
            }

            if ((role & DataItemRole.Output) == DataItemRole.Output)
            {
                foreach (var outputFeature2D in OutputFeatureCollections.OfType<IList>()
                    .SelectMany(l => l.OfType<IFeature>()))
                {
                    yield return outputFeature2D;
                }
            }

            foreach (var feature1D in Get1DChildDataItemLocations(role).ToArray())
            {
                yield return feature1D;
            }
                        
        }

        private IEnumerable<IFeature> Get1DChildDataItemLocations(DataItemRole role)
        {
            if ((role & DataItemRole.Input) == DataItemRole.Input)
            {
                foreach (var weir in Network.Weirs)
                {
                    yield return weir;
                }
                foreach (var gate in Network.Gates)
                {
                    yield return gate;
                }
                foreach (var culvert in Network.Culverts)
                {
                    yield return culvert;
                }
                foreach (var pump in Network.Pumps)
                {
                    yield return pump;
                }
                foreach (var lateralSource in Network.LateralSources)
                {
                    yield return lateralSource;
                }
                foreach (var hydroNode in Network.HydroNodes.Where(hn => !hn.IsConnectedToMultipleBranches))
                {
                    yield return hydroNode;
                }
            }
            if ((role & DataItemRole.Output) == DataItemRole.Output)
            {
                foreach (var location in Network.ObservationPoints)
                {
                    yield return location;
                }
                foreach (var location in Network.Retentions)
                {
                    yield return location;
                }
            }
        }

        public override IEnumerable<IDataItem> GetChildDataItems(IFeature location)
        {
            if (location == null) yield break;

            areaDataItems.TryGetValue(location, out List<IDataItem> items);

            if (items != null)
            {
                foreach (var di in items)
                {
                    yield return di;
                }
            }

            if (location.Geometry is Point)
            {
                var networkDataItem = GetDataItemByValue(Network);
                // Engine parameters that can be set by RTC
                foreach (var engineParameter in GetEngineParametersForLocation(location))
                {
                    // search it first in existing data items
                    var existingDataItem = networkDataItem.Children
                                                          .FirstOrDefault(di => di.ValueType == typeof(double)
                                                                                && di.ValueConverter is Model1DBranchFeatureValueConverter valueConverter
                                                                                && IsValueConverterForEngineParameter(location, valueConverter, engineParameter));

                    if (existingDataItem != null)
                    {
                        yield return existingDataItem;
                    }
                    else
                    {
                        yield return new DataItem
                        {
                            Name = location + " - " + engineParameter.Name, //todo: clean this up
                            Role = engineParameter.Role,
                            ValueType = typeof(double),
                            Parent = networkDataItem,
                            ShouldBeRemovedAfterUnlink = true,
                            ValueConverter =
                                new Model1DBranchFeatureValueConverter(
                                    this,
                                    location,
                                    engineParameter.Name,
                                    engineParameter.QuantityType,
                                    engineParameter.ElementSet,
                                    engineParameter.Role,
                                    engineParameter.Unit.Symbol)
                        };
                    }
                }
            }
        }

        private static bool IsValueConverterForEngineParameter(IFeature location, Model1DBranchFeatureValueConverter valueConverter, EngineParameter engineParameter)
        {
            return valueConverter.ParameterName == engineParameter.Name
                   && valueConverter.Role == engineParameter.Role
                   && valueConverter.ElementSet == engineParameter.ElementSet 
                   && valueConverter.QuantityType == engineParameter.QuantityType
                   && Equals(valueConverter.Location, location);
        }

        private IEnumerable<EngineParameter> GetEngineParametersForLocation(IFeature location)
        {
            if (location is IHydroNode)
            {
                var boundary = BoundaryConditions1D.FirstOrDefault(boundaryNodeData => boundaryNodeData.Node.Equals(location));
                if (boundary == null) yield break;

                switch (boundary.DataType)
                {
                    case Model1DBoundaryNodeDataType.WaterLevelConstant:
                    case Model1DBoundaryNodeDataType.WaterLevelTimeSeries:
                        yield return new EngineParameter(QuantityType.WaterLevel, ElementSet.HBoundaries,
                            DataItemRole.Input, FunctionAttributes.StandardNames.WaterLevel,
                            new Unit("Meter above reference level", "m AD"));
                        yield return new EngineParameter(QuantityType.Discharge, ElementSet.HBoundaries,
                            DataItemRole.Output, FunctionAttributes.StandardNames.WaterDischarge,
                            new Unit("Cubic meter", Resources.WaterFlowFMModel_GetEngineParametersForLocation_CubicMeter));
                        break;
                    case Model1DBoundaryNodeDataType.FlowConstant:
                    case Model1DBoundaryNodeDataType.FlowTimeSeries:
                        yield return new EngineParameter(QuantityType.Discharge, ElementSet.QBoundaries,
                            DataItemRole.Input, FunctionAttributes.StandardNames.WaterDischarge,
                            new Unit("Cubic meter", Resources.WaterFlowFMModel_GetEngineParametersForLocation_CubicMeter));
                        yield return new EngineParameter(QuantityType.Discharge, ElementSet.QBoundaries,
                            DataItemRole.Output, FunctionAttributes.StandardNames.WaterLevel,
                            new Unit("Meter above reference level", "m AD"));
                        break;
                }
            }
            else
            {
                foreach (EngineParameter exchangableParameter in EngineParameters.GetExchangableParameters(EngineParameters.EngineMapping(), location))
                {
                    yield return exchangableParameter;
                }
            }
        }

        [NoNotifyPropertyChange]
        public override DateTime StartTime
        {
            get
            {
                return (DateTime)ModelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
            }
            set
            {
                ModelDefinition.GetModelProperty(GuiProperties.StartTime).Value = value;
                // This base model setting is made to make the base logic right
                base.StartTime = value;
            }
        }

        [NoNotifyPropertyChange]
        public override DateTime StopTime
        {
            get { return (DateTime)ModelDefinition.GetModelProperty(GuiProperties.StopTime).Value; }
            set
            {
                ModelDefinition.GetModelProperty(GuiProperties.StopTime).Value = value;
                // This base model setting is made to make the base logic right
                base.StopTime = value;
            }
        }

        public override TimeSpan TimeStep
        {
            get { return (TimeSpan) ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value; }
            set
            {
                ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value = value;
                // This base model setting is made to make the base logic right
                base.TimeStep = value;
            }
        }

        protected override void OnClearOutput() => DisconnectOutput();

        private void ClearFunctionStore(PropertyInfo property)
        {
            if (!(property.GetValue(this) is IFunctionStore functionStore))
            {
                return;
            }

            // FunctionStores are cleared, but NetworkCoverages still listen to Network changes,
            // so the Network should be set to null.
            foreach (INetworkCoverage function in functionStore.Functions.OfType<INetworkCoverage>())
            {
                function.Network = null;
            }
            functionStore.Functions.Clear();

            if (functionStore is IFileBased fileBasedFunctionStore)
            {
                fileBasedFunctionStore.Close();
            }

            property.SetValue(this, null);
        }

        public override IProjectItem DeepClone()
        {
            var tempDir = FileUtils.CreateTempDirectory();
            var mduFileName = MduFilePath != null ? Path.GetFileName(MduFilePath) : "some_temp.mdu";
            var tempFilePath = Path.Combine(tempDir, mduFileName);
            ExportTo(tempFilePath, false);

            return new WaterFlowFMModel(tempFilePath);
        }

        #endregion

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
            BeginEdit(new DefaultEditAction("Converting model coordinates"));

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

        public virtual string WorkingDirectory
        {
            get { return Path.Combine(WorkingDirectoryPathFunc(), Name); }
        }

        public string HydFilePath
        {
            get
            {
                var projectName = Path.GetFileNameWithoutExtension(MduFilePath);
                return Path.Combine(WorkingDirectory, string.Format("DFM_DELWAQ_{0}", projectName),String.Format("{0}.hyd", projectName));
            }
        }

        public bool HydFileOutput { get; set; } // always on ??

        #region Spatial data

        public bool InitialCoverageSetChanged { get; set; }

        #endregion

        #region Mdu file

        private readonly MduFile mduFile = new MduFile();

        public string MduSavePath
        {
            get { return GetMduPathFromDeltaShellPath(RecursivelyGetModelDirectoryPathFromMduFile()); }
        }

        private string RecursivelyGetModelDirectoryPathFromMduFile()
        {
            if (string.IsNullOrEmpty(MduFilePath))
            {
                return Name;
            }

            string modelDirectoryName = Path.GetFileNameWithoutExtension(MduFilePath);
            var modelDir = new DirectoryInfo(MduFilePath);
            while (modelDir != null && modelDir.Name != modelDirectoryName)
            {
                modelDir = modelDir.Parent;
            }

            return modelDir?.Parent == null // should never happen, unless the file-based repository is corrupted
                       ? Path.GetDirectoryName(
                           Path.GetDirectoryName(MduFilePath)) // default behaviour (e.g. model renamed)
                       : modelDir.FullName;
        }

        public string HisSavePath
        {
            get
            {
                if (ModelDefinition == null) return null;
                if (ModelDefinition.ModelName.Equals(Name))
                    return HisFilePath;
                return Name + "_his.nc";
            }
        }

        public string MapSavePath
        {
            get
            {
                if (ModelDefinition == null) return null;
                if (ModelDefinition.ModelName.Equals(Name))
                    return MapFilePath;
                return Name + "_map.nc";
            }
        }

        public IEnumerable<KeyValuePair<WaterFlowFMProperty, string>> SubFiles
        {
            get
            {
                if (ModelDefinition == null) yield break;

                var modelDefinitionName = ModelDefinition.ModelName;

                var modelNameBasedFiles = new Dictionary<string, string>
                {
                    {KnownProperties.NetFile, NetFile.FullExtension},
                    {KnownProperties.ExtForceFile, ExtForceFile.Extension},
                    {KnownProperties.BndExtForceFile, ExtForceFile.Extension},
                    {KnownProperties.LandBoundaryFile, MduFile.LandBoundariesExtension},
                    {KnownProperties.ThinDamFile, MduFile.ThinDamExtension},
                    {KnownProperties.FixedWeirFile, MduFile.FixedWeirExtension},
                    {KnownProperties.StructuresFile, MduFile.StructuresExtension},
                    {KnownProperties.ObsFile, MduFile.ObsExtension},
                    {KnownProperties.ObsCrsFile, MduFile.ObsCrossExtension},
                    {KnownProperties.DryPointsFile, MduFile.DryPointExtension}
                };

                foreach (var pair in modelNameBasedFiles)
                {
                    var property = ModelDefinition.GetModelProperty(pair.Key);
                    var propertyValue = property.GetValueAsString();
                    if (pair.Key != KnownProperties.NetFile && pair.Key != KnownProperties.ExtForceFile &&
                        string.IsNullOrEmpty(propertyValue)) //skip default (empty) paths
                    {
                        continue;
                    }
                    var currentFileName = Path.GetFileName(propertyValue);
                    if (modelDefinitionName == null ||
                        (modelDefinitionName + pair.Value).Equals(currentFileName, StringComparison.InvariantCultureIgnoreCase) && pair.Key != KnownProperties.NetFile)
                    {
                        propertyValue = Name + pair.Value;
                    }
                    yield return new KeyValuePair<WaterFlowFMProperty, string>(property, propertyValue);
                }
            }
        }

        public virtual string MduFilePath { get; set; }

        public MduFile MduFile { get { return mduFile; } }

        public string ExtFilePath
        {
            get
            {
                if (MduFilePath != null && ModelDefinition.ContainsProperty(KnownProperties.ExtForceFile))
                    return MduFileHelper.GetSubfilePath(MduFilePath,
                        ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile));
                return null;
            }
        }

        public string BndExtFilePath
        {
            get
            {
                if (MduFilePath != null && ModelDefinition.ContainsProperty(KnownProperties.BndExtForceFile))
                    return MduFileHelper.GetSubfilePath(MduFilePath,
                        ModelDefinition.GetModelProperty(KnownProperties.BndExtForceFile));
                return null;
            }
        }

        public string NetFilePath
        {
            get
            {
                if (MduFilePath != null && ModelDefinition.ContainsProperty(KnownProperties.NetFile))
                    return MduFileHelper.GetSubfilePath(MduFilePath,
                        ModelDefinition.GetModelProperty(KnownProperties.NetFile));
                return null;
            }
        }

        public string StorageNodeFilePath
        {
            get
            {
                if (MduFilePath != null && ModelDefinition.ContainsProperty(KnownProperties.StorageNodeFile))
                    return MduFileHelper.GetSubfilePath(MduFilePath,
                        ModelDefinition.GetModelProperty(KnownProperties.StorageNodeFile));
                return null;
            }
        }

        private string MapFilePath
        {
            get
            {
                return !String.IsNullOrEmpty(MduFilePath)
                    ? Path.Combine(Path.GetDirectoryName(MduFilePath), ModelDefinition.RelativeMapFilePath)
                    : null;
            }
        }

        public string MorFilePath
        {
            get
            {
                if (MduFilePath != null && ModelDefinition.ContainsProperty(KnownProperties.MorFile))
                    return MduFileHelper.GetSubfilePath(MduFilePath,
                        ModelDefinition.GetModelProperty(KnownProperties.MorFile));
                return null;
            }
        }

        public string SedFilePath
        {
            get
            {
                if (MduFilePath != null && ModelDefinition.ContainsProperty(KnownProperties.SedFile))
                    return MduFileHelper.GetSubfilePath(MduFilePath,
                        ModelDefinition.GetModelProperty(KnownProperties.SedFile));
                return null;
            }
        }

        private string HisFilePath
        {
            get
            {
                return !String.IsNullOrEmpty(MduFilePath)
                    ? Path.Combine(Path.GetDirectoryName(MduFilePath), ModelDefinition.RelativeHisFilePath)
                    : null;
            }
        }

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

        private void LoadOutputStateFromMdu(string mduFilePath)
        {
            if(!File.Exists(mduFilePath)) return;
            string existingOutputDirectory = RetrieveOutputDirectory(mduFilePath);
            ReconnectOutputFiles(existingOutputDirectory);
        }

        public string ModelDirectoryPath => Path.GetDirectoryName(Path.GetDirectoryName(MduFilePath));

        public string PersistentOutputDirectoryPath => Path.Combine(ModelDirectoryPath, DirectoryNameConstants.OutputDirectoryName);

        private string RetrieveOutputDirectory(string mduFilePath)
        {
            currentOutputDirectoryPath = PersistentOutputDirectoryPath;

            if (ModelDefinition.ContainsProperty(KnownProperties.OutDir))
            {
                string mduOutputDir =
                    ModelDefinition.GetModelProperty(KnownProperties.OutDir).GetValueAsString()?.Trim();

                if (!string.IsNullOrEmpty(mduOutputDir))
                {
                    // We currently assume all OutputDirectoryNames are relative.
                    string mduOutputDirPath = Path.Combine(Path.GetDirectoryName(mduFilePath), mduOutputDir);
                    if (Directory.Exists(mduOutputDirPath))
                    {
                        currentOutputDirectoryPath = mduOutputDirPath;
                    }
                }
            }

            string existingOutputDirectory = Directory.Exists(currentOutputDirectoryPath)
                                                 ? currentOutputDirectoryPath
                                                 : Path.GetDirectoryName(
                                                     mduFilePath); // backwards Compatibility (output next to mdu file)
            return existingOutputDirectory;
        }

        #region Output

        private void SetOutputDirProperty()
        {
            WaterFlowFMProperty outputDirProperty = ModelDefinition.GetModelProperty(KnownProperties.OutDir);

            string existingOutputDir = outputDirProperty.GetValueAsString();
            if (!existingOutputDir.StartsWith(DirectoryNameConstants.OutputDirectoryName))
            {
                outputDirProperty.SetValueAsString(DirectoryNameConstants.OutputDirectoryName);
                Log.InfoFormat("Running this model requires the OutputDirectory to be overwritten to: {0}",
                               DirectoryNameConstants.OutputDirectoryName);
            }
        }
        #endregion
        internal void SyncModelTimesWithBase()
        {
            base.StartTime = StartTime;
            base.StopTime = StopTime;
            base.TimeStep = TimeStep;
        }

        private void InitializeAreaDataColumns()
        {
            MduFile.SetBridgePillarAttributes(Area.BridgePillars, BridgePillarsDataModel);
        }

        private string GetMduPathFromDeltaShellPath(string path, string subFoldersFromModelFolder = DirectoryNameConstants.InputDirectoryName)
        {
            var directoryName = path != null
                ? Path.GetDirectoryName(path) ?? ""
                : "";

            // dsproj_data/<model name>/<model name>.mdu
            return Path.Combine(directoryName, Name, subFoldersFromModelFolder, Name + ".mdu");
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

        private void MarkDirty()
        {
            unchecked { dirtyCounter++; } //unchecked is default, but its here to declare intent
        }
        private int dirtyCounter; //tells NHibernate we need to be saved
        private FMMapFileFunctionStore outputMapFileStore;
        private IEventedList<string> tracerDefinitions;
        private bool isLoading;
        private IEventedList<ILink1D2D> links;
        private FM1DFileFunctionStore output1DFileStore;
        private HeatFluxModelType heatFluxModelType;
        private IHydroRegion fmRegion;
        private ValidationReport report;

        private const int TotalImportSteps = 10;

        #region Output
        private string outputSnappedFeaturesPath;
        public string OutputSnappedFeaturesPath
        {
            get => outputSnappedFeaturesPath;
            set
            {
                if (outputSnappedFeaturesPath == value)
                {
                    return;
                }

                outputSnappedFeaturesPath = value;

                OnOutputSnappedFeaturesPathPropertyChanged(nameof(OutputSnappedFeaturesPath));
            }
        }

        public event PropertyChangedEventHandler OutputSnappedFeaturesPathPropertyChanged;

        protected void OnOutputSnappedFeaturesPathPropertyChanged(string name)
        {
            OutputSnappedFeaturesPathPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public TimeSpan OutputTimeStep
        {
            get { return (TimeSpan)ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value; }
            set { ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = value; }
        }

        public UnstructuredGridCellCoverage OutputWaterLevel
        {
            get
            {
                if (OutputMapFileStore != null)
                {
                    return OutputMapFileStore.Functions.OfType<UnstructuredGridCellCoverage>()
                        .FirstOrDefault(f => f.Components[0].Name.EndsWith("s1"));
                }
                return null;
            }
        }

        public virtual FMMapFileFunctionStore OutputMapFileStore
        {
            get { return outputMapFileStore; }
            protected set
            {
                outputMapFileStore = value;
            }
        }

        public virtual FM1DFileFunctionStore Output1DFileStore
        {
            get { return output1DFileStore; }
            protected set
            {
                output1DFileStore = value;
            }
        }

        public virtual FMHisFileFunctionStore OutputHisFileStore { get; protected set; }
        
        public virtual FMClassMapFileFunctionStore OutputClassMapFileStore { get; protected set; }

        public virtual FouFileFunctionStore OutputFouFileStore { get; protected set; }

        public const string DiaFileDataItemTag = "DiaFile";

        private string currentOutputDirectoryPath;
        public string WorkingOutputDirectoryPath =>
            Path.Combine(WorkingDirectory, DirectoryName, DirectoryNameConstants.OutputDirectoryName);

        private bool HasOpenFunctionStores =>
            OutputMapFileStore != null || OutputHisFileStore != null || OutputClassMapFileStore != null;

        /// <summary>
        /// Saves the output by either moving or copying the source output to the target output directory.
        /// </summary>
        /// <remarks> When a file is locked, we report an error and return. </remarks>
        private void SaveOutput()
        {
            if (string.IsNullOrEmpty(currentOutputDirectoryPath))
            {
                return;
            }

            var sourceOutputDirectory = new DirectoryInfo(currentOutputDirectoryPath);
            if (!sourceOutputDirectory.Exists)
            {
                currentOutputDirectoryPath = PersistentOutputDirectoryPath;
                return;
            }

            var targetOutputDirectory = new DirectoryInfo(PersistentOutputDirectoryPath);
            string sourceOutputDirectoryPath = sourceOutputDirectory.FullName;
            string targetOutputDirectoryPath = targetOutputDirectory.FullName;

            bool sourceIsWorkingDir = sourceOutputDirectoryPath == WorkingOutputDirectoryPath;

            if (OutputIsEmpty && !HasOpenFunctionStores)
            {
                CleanDirectory(PersistentOutputDirectoryPath);

                if (sourceIsWorkingDir)
                {
                    CleanDirectory(WorkingDirectory);
                }

                currentOutputDirectoryPath = PersistentOutputDirectoryPath;

                return;
            }

            if (sourceOutputDirectory.EqualsDirectory(targetOutputDirectory))
            {
                return;
            }

            //copy all files and subdirectories from source directory "output" to persistent directory "output"
            if (!FileUtils.IsDirectoryEmpty(sourceOutputDirectoryPath))
            {
                FileUtils.CreateDirectoryIfNotExists(targetOutputDirectoryPath);

                if (sourceIsWorkingDir)
                {
                    List<string> lockedFiles = GetLockedFiles(WorkingDirectory).ToList();

                    if (lockedFiles.Any())
                    {
                        ReportLockedFiles(lockedFiles);
                        return;
                    }

                    CleanDirectory(targetOutputDirectoryPath);
                    MoveAllContentDirectory(sourceOutputDirectory, targetOutputDirectoryPath);
                }
                else
                {
                    CleanDirectory(targetOutputDirectoryPath);
                    FileUtils.CopyAll(sourceOutputDirectory, targetOutputDirectory, string.Empty);
                }
            }

            currentOutputDirectoryPath = targetOutputDirectoryPath;
            ReconnectOutputFiles(currentOutputDirectoryPath, true);

            if (sourceIsWorkingDir)
            {
                CleanDirectory(WorkingDirectory);
            }
        }

        private void MoveAllContentDirectory(DirectoryInfo sourceDirectory, string targetDirectoryPath)
        {
            foreach (FileInfo file in sourceDirectory.EnumerateFiles())
            {
                string targetPath = Path.Combine(targetDirectoryPath, file.Name);
                file.MoveTo(targetPath);
            }

            bool onSameVolume = Directory.GetDirectoryRoot(sourceDirectory.FullName)
                                         .Equals(Directory.GetDirectoryRoot(targetDirectoryPath));

            foreach (DirectoryInfo directory in sourceDirectory.EnumerateDirectories())
            {
                MoveDirectory(directory, targetDirectoryPath, onSameVolume);
            }
        }

        private static void ReportLockedFiles(IEnumerable<string> filePaths)
        {
            string separator = Environment.NewLine + "- ";
            string lockedFilesMessage = separator + string.Join(separator, filePaths);
            Log.Error("There are one or more files locked, please close the following file(s) and save again:" +
                      lockedFilesMessage);
        }

        private IEnumerable<string> GetLockedFiles(string sourceDirectoryPath)
        {
            var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);

            foreach (FileInfo file in sourceDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                string path = file.FullName;
                string parentDirectoryName = Path.GetFileName(Path.GetDirectoryName(path));

                // Snapped feature files are locked when the map in the GUI is open, so we ignore and copy snapped files instead.
                if (parentDirectoryName != SnappedFeaturesDirectoryName && FileUtils.IsFileLocked(path))
                {
                    yield return path;
                }
            }
        }

        private void MoveDirectory(DirectoryInfo sourceDirectoryInfo, string targetParentDirectoryPath,
                                   bool onSameVolume)
        {
            var targetDirectoryInfo = new DirectoryInfo(Path.Combine(targetParentDirectoryPath, sourceDirectoryInfo.Name));

            if (onSameVolume && sourceDirectoryInfo.Name != SnappedFeaturesDirectoryName)
            {
                sourceDirectoryInfo.MoveTo(targetDirectoryInfo.FullName);
            }
            else
            {
                FileUtils.CopyAll(sourceDirectoryInfo, targetDirectoryInfo, string.Empty);
            }
        }

        /// <summary>
        /// Removes all files and directories from the directory.
        /// </summary>
        /// <param name="directoryPath"> The directory path of the directory that needs to be cleaned. </param>
        private static void CleanDirectory(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
            {
                return;
            }

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo directory in directoryInfo.EnumerateDirectories())
            {
                try
                {
                    directory.Delete(true);
                }
                // Do NOT remove: when File Explorer is opened in the directory, an IO exeption is thrown.
                // There is no way of checking for this case, so we have to catch it. The second time it is called, it works fine.
                // https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true
                catch (IOException)
                {
                    directory.Delete(true);
                }
            }
        }
        
        public string DelwaqHydFolderName
        {
            get { return "DFM_DELWAQ_" + Name; }
        }
        /// <summary>
        /// Representation of the output directory for a D-Flow FM model.
        /// </summary>
        private class FmOutputDirectory
        {
            private readonly DirectoryInfo outputDirectoryInfo;

            /// <summary>
            /// Creates a new instance of <see cref="FmOutputDirectory"/>.
            /// </summary>
            /// <param name="directoryPath"></param>
            public FmOutputDirectory(string directoryPath)
            {
                outputDirectoryInfo = new DirectoryInfo(directoryPath);
            }

            /// <summary>
            /// Determines whether the output directory exists.
            /// </summary>
            public bool Exists => outputDirectoryInfo.Exists;

            /// <summary>
            /// Determines whether the output directory contains output.
            /// </summary>
            public bool ContainsOutput => File.Exists(MapFilePath)
                                          || File.Exists(HisFilePath)
                                          || File.Exists(ClassMapFilePath)
                                          || File.Exists(WaqOutputDirectoryPath)
                                          || File.Exists(SnappedOutputDirectoryPath)
                                          || RestartFilePaths.Any();

            /// <summary>
            /// The file path to the map file.
            /// </summary>
            /// <remarks> Returns null in case the file was not found. </remarks>
            public string MapFilePath => FindFileThatEndsWith(FileConstants.MapFileExtension);

            /// <summary>
            /// The file path to the his file.
            /// </summary>
            /// <remarks> Returns null in case the file was not found. </remarks>
            public string HisFilePath => FindFileThatEndsWith(FileConstants.HisFileExtension);

            /// <summary>
            /// The file path to the class map file.
            /// </summary>
            /// <remarks> Returns null in case the file was not found. </remarks>
            public string ClassMapFilePath => FindFileThatEndsWith(FileConstants.ClassMapFileExtension);

            /// <summary>
            /// The file path to the fou file.
            /// </summary>
            /// <remarks> Returns null in case the file was not found. </remarks>
            public string FouFilePath => FindFileThatEndsWith(FileConstants.FouFileExtension); 

            /// <summary>
            /// The path to the waq output directory.
            /// </summary>
            /// <remarks> Returns null in case the directory was not found. </remarks>
            public string WaqOutputDirectoryPath => GetDirectoryPathStartingWith(FileConstants.PrefixDelwaqDirectoryName);

            /// <summary>
            /// The path to the snapped output directory.
            /// </summary>
            /// <remarks> Returns null in case the directory was not found. </remarks>
            public string SnappedOutputDirectoryPath => GetDirectoryPathStartingWith(FileConstants.SnappedFeaturesDirectoryName);

            /// <summary>
            /// The paths of the restart files.
            /// </summary>
            public IEnumerable<string> RestartFilePaths => FindFilesThatEndWith(FileConstants.RestartFileExtension);

            private string GetDirectoryPathStartingWith(string directoryNameStart)
            {
                return outputDirectoryInfo.EnumerateDirectories()
                                          .FirstOrDefault(d => d.Name.StartsWith(directoryNameStart, StringComparison.Ordinal))?
                                          .FullName;
            }

            private string FindFileThatEndsWith(string extension)
            {
                return outputDirectoryInfo.EnumerateFiles()
                                          .FirstOrDefault(f => f.Name.EndsWith(extension, StringComparison.Ordinal))?
                                          .FullName;
            }

            private IEnumerable<string> FindFilesThatEndWith(string extension)
            {
                return outputDirectoryInfo.EnumerateFiles()
                                          .Where(f => f.Name.EndsWith(extension, StringComparison.Ordinal))?
                                          .Select(f => f.FullName);
            }
        }

        protected virtual void ReconnectOutputFiles(string outputDirectoryPath, bool switchTo = false)
        {
            if (string.IsNullOrEmpty(outputDirectoryPath))
            {
                return;
            }

            var outputDirectory = new FmOutputDirectory(outputDirectoryPath);
            if (!outputDirectory.Exists || !outputDirectory.ContainsOutput)
            {
                return;
            }

            FireImportProgressChanged(this, "Reading output files - Reading Map file", 1, 2);
            BeginEdit(new DefaultEditAction("Reconnect output files"));

            ReconnectMapFile(outputDirectory.MapFilePath, switchTo);
            ReconnectHistoryFile(outputDirectory.HisFilePath, switchTo);
            ReconnectClassMapFile(outputDirectory.ClassMapFilePath, switchTo);
            ReconnectFouFile(outputDirectory.FouFilePath, switchTo);
            ReconnectWaterQualityOutputDirectory(outputDirectory.WaqOutputDirectoryPath);
            ReconnectSnappedOutputDirectory(outputDirectory.SnappedOutputDirectoryPath);
            ReconnectRestartFiles(outputDirectory.RestartFilePaths);
            ReportProgressText();

            OutputIsEmpty = false;

            EndEdit();
        }

        private void ReconnectMapFile(string mapFilePath, bool switchTo)
        {
            // deal with issue that kernel doesn't understand any coordinate systems other than RD & WGS84 :
            if (mapFilePath != null)
            {
                ReportProgressText("Reading map file");
                var cs = UGridFileHelper.ReadCoordinateSystem(mapFilePath);

                // update map file coordinate system:
                if (!Grid.IsEmpty)
                {
                    if (CoordinateSystem != null && cs != CoordinateSystem)
                        NetFile.WriteCoordinateSystem(mapFilePath, CoordinateSystem);
                    if (switchTo && OutputMapFileStore != null)
                    {
                        OutputMapFileStore.Path = mapFilePath;
                    }
                    else
                    {
                        OutputMapFileStore = new FMMapFileFunctionStore();

                        // don't change this to a property setter, because the timing is of great importance.
                        // elsewise, there will be no subscription to the read and Path triggers the Read().
                        try
                        {
                            OutputMapFileStore.Path = mapFilePath;
                        }
                        catch (Exception e)
                        {
                            Log.Error($"Error reading map file {e.Message}");
                            OutputMapFileStore = null;
                        }
                    }
                }

                if (Network != null && !Network.IsEdgesEmpty && !Network.IsVerticesEmpty)
                {

                    if (switchTo && Output1DFileStore != null)
                    {
                        Output1DFileStore.Path = mapFilePath;
                    }
                    else
                    {
                        Output1DFileStore = new FM1DFileFunctionStore(Network);
                        // don't change this to a property setter, because the timing is of great importance.
                        // elsewise, there will be no subscription to the read and Path triggers the Read().
                        Output1DFileStore.Path = mapFilePath;
                    }
                }
            }
        }

        private void ReconnectHistoryFile(string hisFilePath, bool switchTo)
        {
            if (OutputMapFileStore != null && OutputMapFileStore.Grid == null)
            {
                Log.Warn("Associated output files are unsupported, these will not be loaded");
                OutputMapFileStore = null;
                return;
            }

            if (hisFilePath != null)
            {
                ReportProgressText("Reading his file");
                FireImportProgressChanged(this,"Reading output files - Reading His file", 1, 2);
                if (switchTo && OutputHisFileStore != null)
                {
                    OutputHisFileStore.Path = hisFilePath;
                }
                else
                {
                    OutputHisFileStore = new FMHisFileFunctionStore(Network, Area);
                    OutputHisFileStore.Path = hisFilePath;
                    OutputHisFileStore.CoordinateSystem = CoordinateSystem;
                }
            }
        }

        private void ReconnectClassMapFile(string classMapFilePath, bool switchTo)
        {
            if (classMapFilePath == null)
            {
                return;
            }

            ReportProgressText("Reading class map file");
            FireImportProgressChanged(this,"Reading output files - Reading Class Map file", 1, 2);
            if (switchTo && OutputClassMapFileStore != null)
            {
                OutputClassMapFileStore.Path = classMapFilePath;
            }
            else
            {
                OutputClassMapFileStore = new FMClassMapFileFunctionStore(classMapFilePath);
            }
        }

        private void ReconnectFouFile(string fouFilePath, bool switchTo)
        {
            if (fouFilePath == null)
            {
                return;
            }

            ReportProgressText("Reading fou file");
            FireImportProgressChanged(this, "Reading output files - Reading Fou file", 1, 2);
            if (switchTo && OutputFouFileStore != null)
            {
                OutputFouFileStore.Path = fouFilePath;
            }
            else
            {
                OutputFouFileStore = new FouFileFunctionStore {Path = fouFilePath};
            }
        }

        /// <summary>
        /// Gets the cache file.
        /// </summary>
        /// <value>
        /// The cache file.
        /// </value>
        public CacheFile CacheFile =>
            cacheFile ?? (cacheFile = new CacheFile(this, new OverwriteCopyHandler()));
        
        public string DelwaqOutputDirectoryName => FileConstants.PrefixDelwaqDirectoryName + Name;
        
        public string DelwaqOutputDirectoryPath { get; set; }

        private void ReconnectWaterQualityOutputDirectory(string waqOutputDirectoryPath)
        {
            if (waqOutputDirectoryPath != null)
            {
                DelwaqOutputDirectoryPath = waqOutputDirectoryPath;
            }
        }
        
        private void ClearWaqOutputDirProperty()
        {
            ModelDefinition.GetModelProperty(KnownProperties.WaqOutputDir).SetValueAsString(string.Empty);
        }
        
        private void SetWaqOutputDirProperty()
        {
            if (!SpecifyWaqOutputInterval)
            {
                return;
            }

            string relativeDWaqOutputDirectory = Path.Combine(DirectoryNameConstants.OutputDirectoryName, DelwaqOutputDirectoryName);
            WaterFlowFMProperty waqOutputDirProperty = ModelDefinition.GetModelProperty(KnownProperties.WaqOutputDir);
            waqOutputDirProperty.SetValueAsString(relativeDWaqOutputDirectory);
        }
        
        private void ReconnectSnappedOutputDirectory(string snappedOutputDirectoryPath)
        {
            if (snappedOutputDirectoryPath != null)
            {
                OutputSnappedFeaturesPath = snappedOutputDirectoryPath;
            }
        }

        public IEnumerable<RestartFile> RestartOutput { get; private set; } = Enumerable.Empty<RestartFile>();

        private void ReconnectRestartFiles(IEnumerable<string> restartFilePaths)
        {
            RestartOutput = restartFilePaths.Select(p => new RestartFile(p)).ToList();
        }
        #endregion

        #region Coupling

        private IEnumerable<object> InputFeatureCollections
        {
            get
            {
                yield return Area.Pumps;
                yield return Area.Weirs;
                yield return Area.Gates;
                yield return Area.LeveeBreaches.OfType<LeveeBreach>().ToList();
                yield return SourcesAndSinks.Select(ss => ss.Feature).ToList();
            }
        }

        private IEnumerable<object> OutputFeatureCollections
        {
            get
            {
                yield return Area.ObservationPoints;
                yield return Area.ObservationCrossSections;
                
            }
        }

        private bool OutputFeatureCollectionsContains(object item)
        {
            if (item is GroupableFeature2DPoint)
            {
                return Area.ObservationPoints.Contains(item);
            }

            if (item is ObservationCrossSection2D)
            {
                return Area.ObservationCrossSections.Contains(item);
            }
            return false;
        }

        private bool InputFeatureCollectionsContains(object item)
        {
            if (item is Pump2D)
            {
                return Area.Pumps.Contains(item);
            }
            
            if (item is Weir2D)
            {
                return Area.Weirs.Contains(item);
            }

            if (item is Gate2D)
            {
                return Area.Gates.Contains(item);
            }

            if (item is LeveeBreach)
            {
                return Area.LeveeBreaches.Contains(item);
            }

            if (item is Feature2D sourceAndSinkFeature)
            {
                return SourcesAndSinks?.Any(ss => ss.Feature.Equals(sourceAndSinkFeature)) ?? false;
            }

            return false;
        }

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
        
        private void RemoveAreaFeature(IFeature feature)
        {
            if (areaDataItems.TryGetValue(feature, out var dataItemsToBeRemoved))
            {
                foreach (var dataItem in dataItemsToBeRemoved)
                {
                    UnSubscribeFromDataItem(dataItem, true);
                    OnDataItemRemoved(dataItem);
                }
            }
            areaDataItems.Remove(feature);
        }

        private void AddAreaItem(IFeature feature, bool isInputSender)
        {
            var listToAdd = GetDataItemListForFeature(feature, isInputSender);
            areaDataItems.Add(feature, listToAdd);
        }

        private void UpdateAreaDataItems(IFeature feature, bool isInputSender)
        {
            if(areaDataItems.ContainsKey(feature))
            {
                var listToReplace = GetDataItemListForFeature(feature, isInputSender);               
                areaDataItems[feature] = listToReplace;
            }
        }

        private List<IDataItem> GetDataItemListForFeature(IFeature feature, bool isInputSender)
        {
            return GetQuantitiesForLocation(feature).Select(quantity => new DataItem(feature)
            {
                Name = feature.ToString(),
                Tag = quantity,
                Role = isInputSender ? DataItemRole.Input : DataItemRole.Output,
                ValueType = typeof(double),
                ValueConverter = new WaterFlowFMFeatureValueConverter(this, feature, quantity, String.Empty) // TODO: insert unit
            }).OfType<IDataItem>().ToList();
        }

        private IEnumerable<string> GetQuantitiesForLocation(IFeature location)
        {
            var pump = location as IPump;
            if (pump != null)
            {
                yield return KnownStructureProperties.Capacity;
            }

            var gate = location as IGate;
            if (gate != null)
            {
                yield return "GateHeight";
                yield return "GateLowerEdgeLevel";
                yield return "GateOpeningWidth";
                yield return "GateOpeningHorizontalDirection";
            }
            var orifice = location as IOrifice;
            if (orifice != null)
            {
                yield return "gateLowerEdgeLevel";
            }

            var weir = location as IWeir;
            if (weir != null)
            {
                yield return "CrestLevel";
                var generalStructureWeirFormula = weir.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureWeirFormula != null)
                {
                    yield return "GateHeight";
                    yield return "GateLowerEdgeLevel";
                    yield return "GateOpeningWidth";
                    yield return "GateOpeningHorizontalDirection";
                }
                var gatedWeirFormula = weir.WeirFormula as GatedWeirFormula;
                if (gatedWeirFormula != null)
                {
                    yield return "GateHeight";
                    yield return "GateLowerEdgeLevel";
                    yield return "GateOpeningWidth";
                    yield return "GateOpeningHorizontalDirection";
                }

            }

            if (Area.ObservationPoints.Contains(location))
            {
                //TODO: add temperature and tracers
                yield return "water_level";
                if (UseSalinity)
                {
                    yield return "salinity";
                }

                if (UseTemperature)
                {
                    yield return "temperature";
                }

                yield return "water_depth";
                foreach (var tracerDefinition in TracerDefinitions)
                {
                    yield return tracerDefinition;
                }
            }

            if (Area.ObservationCrossSections.Contains(location))
            {
                yield return "discharge";
                yield return "velocity";
                yield return "water_level";
                yield return "water_depth";
            }
            if (Area.LeveeBreaches.Contains(location))
            {
                yield return "dambreak_s1up";
                yield return "dambreak_s1dn";
                yield return "dambreak_breach_depth";
                yield return "dambreak_breach_width";
                yield return "dambreak_instantaneous_discharge";
                yield return "dambreak_cumulative_discharge";
            }

            if (SourcesAndSinks?.Any(ss => ss.Feature.Equals(location)) ?? false)
            {
                yield return "discharge";
                yield return "change_in_salinity";
                yield return "change_in_temperature";
            }
        }

        public double GetValueFromModelApi(IFeature feature, string parameterName)
        {
            string featureCategory = GetFeatureCategory(feature);
            if (featureCategory == null)
            {
                return Double.NaN;
            }

            // temporary fix for DELFT3DFM-1302 (this should be done in Dimr)
            if (featureCategory == "weirs" && parameterName == "crest_level")
            {
                var weir = (Weir)feature;
                if (!weir.UseCrestLevelTimeSeries)
                {
                    return weir.CrestLevel;
                }

                if (weir.CrestLevelTimeSeries.GetValues<double>().Any())
                {
                    return weir.CrestLevelTimeSeries.GetValues<double>().FirstOrDefault();
                }
            }

            if (runner.Api == null)
            {
                return Double.NaN;
            }

            var nameable = feature as INameable;
            if (nameable == null)
                return Double.NaN;

            return ((double[])GetVar(featureCategory, nameable.Name, parameterName))[0];
        }

        public void SetToModelApi(IFeature feature, string parameterName, double value)
        {
            string featureCategory = GetFeatureCategory(feature);
            if (featureCategory == null)
            {
                return;
            }

            var nameable = feature as INameable;
            if (nameable == null)
                return;

            SetVar(new [] { value }, featureCategory, nameable.Name, parameterName);
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

        public bool IsActivityOfEnumType(ModelType type)
        {
            return type == ModelType.DFlowFM;
        }

        public void OnFinishIntegratedModelRun(string hydroModelWorkingDirectoryPath)
        {
            if ((bool)ModelDefinition.GetModelProperty(KnownProperties.UseCaching).Value)
            {
                // Actions, which should be done in the IDimrModel after a successful integrated model
                // run.

                // We know the cache file will either exist at the runMduPath because it 
                // was copied here, or it will be generated by the kernel during the run.
                string runMduPath = Path.Combine(hydroModelWorkingDirectoryPath, DirectoryName,
                                                 $"{Name}{FileConstants.MduFileExtension}");

                CacheFile.UpdatePathToMduLocation(runMduPath);
            }
        }

        public ISet<string> IgnoredFilePathsWhenCleaningWorkingDirectory =>
            CacheFile.UseCaching && CacheFile.Path.StartsWith(WorkingDirectory)
                ? new HashSet<string> { CacheFile.Path }
                : new HashSet<string>();
        
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

        #region IDimrModel

        public virtual string LibraryName
        {
            get { return "dflowfm"; }
        }

        public virtual string InputFile
        {
            get { return Path.GetFileName(MduSavePath); }
        }

        public virtual string DirectoryName
        {
            get { return "dflowfm"; }
        }

        public virtual string SnappedFeaturesDirectoryName
        {
            get { return "snapped"; }
        }

        public virtual bool IsMasterTimeStep { get { return true; } }

        public virtual string ShortName
        {
            get { return "flow"; }
        }

        public virtual string DimrModelRelativeOutputDirectory => Path.Combine(DirectoryName, DirectoryNameConstants.OutputDirectoryName);

        public virtual string GetItemString(IDataItem dataItem)
        {
            var category = GetFeatureCategory(dataItem.GetFeature());

            var dataItemName = dataItem.ValueConverter.OriginalValue is INetworkFeature networkFeature ? networkFeature.Name : dataItem.Name;

            var parameterName = GetConvertedParameterName(dataItem.GetParameterName(), category);
            string nameWithoutHashTags = dataItemName.Replace("##", "~~");

            var concatNames = new List<string>(new[] { category, nameWithoutHashTags, parameterName });

            concatNames.RemoveAll(s => s == null);

            return string.Join("/", concatNames);
        }

        private static string GetConvertedParameterName(string parameterName, string category, bool lookForValue = false)
        {
            var namesLookup = WaterFlowFMModelDataSet.GetDictionaryForCategory(category);
            if (namesLookup == null)
            {
                return parameterName;
            }

            if (!lookForValue)
            {
                string dhydroParameterName;
                return namesLookup.TryGetValue(parameterName, out dhydroParameterName)
                    ? dhydroParameterName
                    : parameterName;
            }

            return namesLookup.ContainsValue(parameterName)
                ? namesLookup.First(kvp => kvp.Value == parameterName).Key
                : parameterName;
        }

        public virtual Type ExporterType
        {
            get { return typeof(WaterFlowFMFileExporter); }
        }

        public virtual string GetExporterPath(string directoryName)
        {
            return Path.Combine(directoryName, InputFile == null ? Name + ".mdu" : Path.GetFileName(InputFile));
        }

        public virtual bool CanRunParallel
        {
            get { return true; }
        }

        public virtual string MpiCommunicatorString
        {
            get { return "DFM_COMM_DFMWORLD"; }
        }

        public virtual string KernelDirectoryLocation
        {
            get { return DimrApiDataSet.DFlowFmDllPath; }
        }

        public virtual void DisconnectOutput()
        {
            var storeNames = new[]
            {
                nameof(OutputMapFileStore),
                nameof(Output1DFileStore),
                nameof(OutputHisFileStore),
                nameof(OutputClassMapFileStore),
                nameof(OutputFouFileStore)
            };

            var properties = storeNames.Select(n => GetType().GetProperty(n)).ToArray();

            if (properties.Any(p => p.GetValue(this) != null))
            {
                using (this.InEditMode("Disconnecting from output files"))
                {
                    properties.ForEach(ClearFunctionStore);
                }
            }

            OutputSnappedFeaturesPath = null;
            OutputIsEmpty = true;
        }

        public virtual void ConnectOutput(string outputPath)
        {
            currentOutputDirectoryPath = outputPath;
            ReadDiaFile(outputPath); 
            ReconnectOutputFiles(outputPath);
            ClearWaqOutputDirProperty();
        }

        private void ReadDiaFile(string outputDirectory)
        {
            ReportProgressText("Reading dia file");
            var diaFileName = $"{Name}.dia";
            string diaFilePath = Path.Combine(outputDirectory, diaFileName);
            if (File.Exists(diaFilePath))
            {
                try
                {
                    var logDataItem = DataItems.FirstOrDefault(di => di.Tag == WaterFlowFMModelDataSet.DiaFileDataItemTag);
                    if (logDataItem == null)
                    {
                        // add logfile dataitem if not exists
                        var textDocument = new TextDocument(true) { Name = diaFileName };
                        logDataItem = new DataItem(textDocument, DataItemRole.Output, WaterFlowFMModelDataSet.DiaFileDataItemTag);
                        DataItems.Add(logDataItem);
                    }

                    var log = DiaFileReader.Read(diaFilePath);
                    ((TextDocument)logDataItem.Value).Content = log;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat(Resources.WaterFlowFMModel_ReadDiaFile_Error_reading_log_file___0____1_, diaFileName, ex.Message);
                }
            }
            else
            {
                Log.WarnFormat(Resources.WaterFlowFMModel_ReadDiaFile_Could_not_find_log_file___0__at_expected_path___1_, diaFileName, diaFilePath);
            }
        }

        public virtual ValidationReport Validate()
        {
            if (Status == ActivityStatus.Initializing && !ValidateBeforeRun)
            {
                return null;
            }

            return WaterFlowFmModelValidationExtensions.Validate(this);
        }

        public virtual ValidationReport ValidationReport
        {
            get { return report == null ? (report = Validate()) : report.Equals(Validate()) ? report : (report = Validate()); }
        }
        public new virtual ActivityStatus Status
        {
            get { return base.Status; }
            set { base.Status = value; }
        }

        [EditAction]
        public virtual bool RunsInIntegratedModel { get; set; }

        public virtual string DimrExportDirectoryPath => WorkingDirectory;

        [NoNotifyPropertyChange]
        public new virtual DateTime CurrentTime
        {
            get { return base.CurrentTime; }
            set
            {
                base.CurrentTime = value; 
                OnProgressChanged();
            }
        }
        public virtual Array GetVar(string category, string itemName = null, string parameter = null)
        {
            if (category == CellsToFeaturesName)
            {
                if (OutputMapFileStore != null && OutputMapFileStore.BoundaryCellValues != null)
                    return OutputMapFileStore.BoundaryCellValues.ToArray();
                return null;
            }

            if (category == GridPropertyName)
            {
                return new[] { grid };
            }

            if (runner.CanCommunicateWithDimrApi)
            {
                var itemText = string.IsNullOrEmpty(itemName) ? "" : $"/{itemName}";
                var parameterText = string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(parameter) ? "" : $"/{parameter}";
                
                return runner.GetVar($"{Name}/{category}{itemText}{parameterText}");
            }

            IFeature feature = null;
            switch (category)
            {
                case Model1DParametersCategories.Weirs:
                    feature = Network.GetBranchFeatureByName<IWeir>(itemName);
                    break;
                case Model1DParametersCategories.Culverts:
                    feature = Network.GetBranchFeatureByName<ICulvert>(itemName);
                    break;
                case Model1DParametersCategories.Pumps:
                    feature = Network.GetBranchFeatureByName<IPump>(itemName);
                    break;
                case Model1DParametersCategories.Laterals:
                    feature = Network.GetBranchFeatureByName<ILateralSource>(itemName);
                    break;
            }

            return new[] { EngineParameters.GetInitialValue(feature, parameter) };
        }
        public virtual void SetVar(Array values, string category, string itemName = null, string parameter = null)
        {
            if (category == DisableFlowNodeRenumberingPropertyName)
            {
                var boolArray = values as bool[];
                if (boolArray != null && boolArray.Length > 0)
                    DisableFlowNodeRenumbering = boolArray[0];
                return;
            }
            if (!string.IsNullOrEmpty(itemName))
            {
                if (!string.IsNullOrEmpty(parameter))
                {
                    runner.SetVar(string.Format("{0}/{1}/{2}/{3}", Name, category, itemName, parameter), values);
                    return;
                }
                runner.SetVar(string.Format("{0}/{1}/{2}", Name, category, itemName), values);
                return;
            }
            runner.SetVar(string.Format("{0}/{1}", Name, category), values);
        }
        public bool DisableFlowNodeRenumbering { get; set; }
        
        #endregion

        #region TimeDependentModelBase

        protected override void OnInitialize()
        {
            previousProgress = 0;
            DataItems.RemoveAllWhere(di => di.Tag == WaterFlowFMModelDataSet.DiaFileDataItemTag);
            boundaryConditionDataList = BoundaryConditions1D.ToList();

            
            ReportProgressText("Initializing");
            SetOutputDirProperty();
            SetWaqOutputDirProperty();
            if (Directory.Exists(WorkingOutputDirectoryPath))
            {
                DisconnectOutput();
                FileUtils.DeleteIfExists(WorkingOutputDirectoryPath);
                FileUtils.CreateDirectoryIfNotExists(WorkingOutputDirectoryPath);
            }

            runner.OnInitialize();
            
            if (Status != ActivityStatus.Failed)
            {
                InitializeRunTimeGridOperationApi();
            }

            ReportProgressText();
        }
        
        protected override void OnCleanup()
        {
            if (boundaryConditionDataList != null)
                foreach (var bc in boundaryConditionDataList)
                {
                    var data = bc.Data;
                    if (data != null)
                    {
                        data.SkipArgumentValidationInEvaluate = false;
                    }
                }

            boundaryConditionDataList = null;

            if (runTimeGridOperationApi != null)
            {
                runTimeGridOperationApi.Dispose();
                runTimeGridOperationApi = null;
            }
            snapApiInErrorMode = false;
            base.OnCleanup();
            runner.OnCleanup();
            
            ReportProgressText();
            previousProgress = 0;
        }


        protected override void OnExecute()
        {
            runner.OnExecute();
        }

        protected override void OnFinish()
        {
            runner.OnFinish();
            currentOutputDirectoryPath = WorkingOutputDirectoryPath;

            // We know the cache file will either exist at the runMduPath because it 
            // was copied here, or it will be generated by the kernel during the run.
            string runMduPath = Path.Combine(WorkingDirectory,
                                             "dflowfm",
                                             $"{Name}{FileConstants.MduFileExtension}");

            CacheFile.UpdatePathToMduLocation(runMduPath);
        }
        #endregion

        protected override void OnProgressChanged()
        {
            // Only update gui for every 1 percent progress (performance)
            if (ProgressPercentage - previousProgress < 0.01) return;

            previousProgress = ProgressPercentage;
            runner.OnProgressChanged();
            base.OnProgressChanged();
        }

        public override string ProgressText
        {
            get { return string.IsNullOrEmpty(progressText) ? base.ProgressText : progressText; }
        }

        private void ReportProgressText(string text = null)
        {
            progressText = text;
            base.OnProgressChanged();
        }

        private void CopyRestartFile(string targetDir)
        {
            string sourceDirectory = ModelDefinition.ModelDirectory;
            if (String.IsNullOrWhiteSpace(sourceDirectory))
                return;

            var restartFileName = ModelDefinition.GetModelProperty(KnownProperties.RestartFile).GetValueAsString();
            if (String.IsNullOrWhiteSpace(restartFileName))
                return;
            string sourcePath = Path.Combine(sourceDirectory, restartFileName);
            if (File.Exists(sourcePath))
            {
                string targetPath = Path.Combine(targetDir, restartFileName);
                FileUtils.CopyFile(sourcePath, targetPath);
            }
        }

        internal void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public IEnumerable<IDataItem> GetDataItemsUsedForCouplingModel(DataItemRole role)
        {
            return GetChildDataItemLocations(role).SelectMany(GetChildDataItems);
        }

        public string GetUpToDateDataItemName(string oldDataItemName)
        {
            string[] partsTargetName = oldDataItemName.Split('.');

            if (partsTargetName.Length <= 1 || 
                !backwardsCompatibilityMapping.TryGetValue(partsTargetName.Last(), out string newName))
            {
                return oldDataItemName;
            }

            partsTargetName[partsTargetName.Length - 1] = newName;
            return string.Join(".", partsTargetName);
        }

        private static readonly Dictionary<string, string> backwardsCompatibilityMapping = new Dictionary<string, string>
        {
            {"levelcenter", KnownStructureProperties.CrestLevel},
            {"sill_level", KnownStructureProperties.CrestLevel},
            {"crest_level", KnownStructureProperties.CrestLevel},
            {"gateheight", KnownStructureProperties.GateLowerEdgeLevel},
            {"lower_edge_level", KnownStructureProperties.GateLowerEdgeLevel},
            {"door_opening_width", KnownStructureProperties.GateOpeningWidth},
            {"opening_width", KnownStructureProperties.GateOpeningWidth}
        };

        /// <summary>
        /// Gets the data item by item string.
        /// </summary>
        /// <param name="itemString"> The item string. </param>
        /// <returns> The matching data item. </returns>
        /// <remarks>
        /// <paramref name="itemString"/> cannot be null.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when
        /// - <paramref name="itemString"/> does not contain 3 elements
        /// - category in <paramref name="itemString"/> is unknown
        /// - feature in <paramref name="itemString"/> is unknown
        /// - parameter name in <paramref name="itemString"/> is unknown.
        /// </exception>
        public virtual IEnumerable<IDataItem> GetDataItemsByItemString(string itemString, string itemString2)
        {
            string[] stringParts = itemString.Split('/');

            if (stringParts.Length != 3)
            {
                throw new ArgumentException(string.Format("{0} should contain a category, feature name and a parameter name.",
                                                          itemString));
            }

            string category = stringParts[0];
            string featureName = stringParts[1];
            string parameterName = stringParts[2];

            IFeature feature = GetAreaFeature(category, featureName);

            if (feature == null)
            {
                throw new ArgumentException(string.Format("feature {0} in {1} cannot be found in the FM model.",
                                                          featureName, itemString));
            }
            
            string parameterName2 = itemString2.Split('/').LastOrDefault() ?? string.Empty;
            IDataItem dataItem = GetChildDataItems(feature).FirstOrDefault(di =>
            {
                var parameterValueConverter = di.ValueConverter as ParameterValueConverter;
                return parameterValueConverter?.ParameterName == parameterName || 
                       parameterValueConverter?.ParameterName == parameterName2;
            });
            
            if (dataItem == null)
            {
                return null;
            }

            return new[]
            {
                dataItem
            };
        }

        /// <summary>
        /// The dimr coupling for this <see cref="RainfallRunoffModel"/>.
        /// </summary>
        /// <remarks>
        /// Always returns an up-to-date <see cref="IDimrCoupling"/>.
        /// Does not return <c>null</c>.
        /// </remarks>
        public IDimrCoupling DimrCoupling
        {
            get
            {
                if (dimrCoupling == null || dimrCoupling.HasEnded)
                {
                    dimrCoupling = new WaterFlowFmDimrCoupling(Network);
                    return dimrCoupling;
                }

                return dimrCoupling;
            }
        }

        private IFeature GetAreaFeature(string featureCategory, string featureName)
        {

            IEnumerable<INameable> featuresFromCategory = Area.GetFeaturesFromCategory(featureCategory)
                                                              .Concat(Network.GetFeaturesFromCategory(featureCategory))
                                                              .Concat(featureCategory == Model1DParametersCategories.SourceSinks ? SourcesAndSinks.Select(sas => sas.Feature) : Enumerable.Empty<IFeature>())
                                                              .Concat(featureCategory == Model1DParametersCategories.BoundaryConditions ? BoundaryConditions1D : Enumerable.Empty<IFeature>())
                                                              .OfType<INameable>();

            return (IFeature)featuresFromCategory.FirstOrDefault(f => f.Name.Equals(featureName));
        }

        protected virtual void Dispose(bool disposing)
        {
            this.disposing = disposing;

            if (disposing)
            {
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
                runner?.Dispose();
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