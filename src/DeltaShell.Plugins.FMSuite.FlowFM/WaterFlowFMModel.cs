using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BasicModelInterface;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
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
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Features.Generic;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using SharpMap;
using SharpMap.Api;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;
using EngineParameter = DeltaShell.NGHS.IO.DataObjects.Model1D.EngineParameter;
using FixedWeir = DelftTools.Hydro.Structures.FixedWeir;
using ObservationCrossSection2D = DelftTools.Hydro.ObservationCrossSection2D;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    [Entity]
    public partial class WaterFlowFMModel : TimeDependentModelBase, IDimrStateAwareModel, IFileBased, IHasCoordinateSystem, IGridOperationApi, IDisposable, IHydroModel, IHydFileModel, IDimrModel, IWaterFlowFMModel, ISedimentModelData
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (WaterFlowFMModel));
        private readonly DimrRunner runner;

        public const string CellsToFeaturesName = "CellsToFeatures";

        public const string IsPartOf1D2DModelPropertyName = "IsPartOf1D2DModel";
        public const string DisableFlowNodeRenumberingPropertyName = "DisableFlowNodeRenumbering";
        public const string GridPropertyName = "Grid";
        private DepthLayerDefinition depthLayerDefinition;
        private WaterFlowFMModelDefinition modelDefinition;
        private bool disposing;
        private bool updatingGroupName;

        private IList<ModelFeatureCoordinateData<FixedWeir>> allFixedWeirsAndCorrespondingProperties;
        private IEventedList<SourceAndSink> sourcesAndSinks;
        private IEventedList<ISedimentFraction> sedimentFractions;
        private IEventedList<BoundaryConditionSet> boundaryConditionSets;
        private List<Model1DBoundaryNodeData> boundaryConditionDataList;
        private IDataItem areaDataItem;
        private IDataItem networkDataItem;

        private readonly Dictionary<IFeature, List<IDataItem>> areaDataItems = new Dictionary<IFeature, List<IDataItem>>();
        private readonly Dictionary<IFeature, List<IDataItem>> networkDataItems = new Dictionary<IFeature, List<IDataItem>>();
        private double previousProgress;
        private string progressText;

        public WaterFlowFMModel() : this(null)
        {
        }
        
        public WaterFlowFMModel(string mduFilePath, ImportProgressChangedDelegate progressChanged = null) : base("FlowFM")
        {
            runner = new DimrRunner(this);
            ImportProgressChanged = progressChanged;

            InitializeModelProperties();
            tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            AddNetworkToModel();
            AddAreaToModel();

            var hydroAreaParent = Area.Parent;
            var hydroNetworkParent = Network.Parent;
            
            fmRegion = new HydroRegion{Name = Name, SubRegions = new EventedList<IRegion>{ Area, Network}};

            Area.Parent = hydroAreaParent;
            Network.Parent = hydroNetworkParent;

            if (!string.IsNullOrEmpty(mduFilePath))
            {
                LoadStateFromMdu(mduFilePath);
                
                FeatureFile1D2DReader.Read1D2DFeatures(mduFilePath, ModelDefinition, Network, RoughnessSections);

                LoadLinks();

                FireImportProgressChanged(this, "Reading spatial operations", 9, TotalImportSteps);
                var modelDataItems = AddSpatialDataItems();
                ImportSpatialOperationsAfterCreating(modelDataItems);
            }
            else
            {
                ModelDefinition = new WaterFlowFMModelDefinition();
                ModelDefinition.SetModelProperty(KnownProperties.NetFile, Name + NetFile.FullExtension);
                SynchronizeModelDefinitions();

                Grid = new UnstructuredGrid();
                InitializeUnstructuredGridCoverages();

                AddSpatialDataItems();
                RenameSubFilesIfApplicable();
            }

            UpdateRoughnessSections();
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
            SubscribeToNetwork();
            NetworkDiscretization = new Discretization {Network = network, Name = DiscretizationObjectName, SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered };
            
            // q's supplied by externals
            AddInflowsDataItem();

            boundaryNodeDataItemSet = new DataItemSet(new EventedList<Model1DBoundaryNodeData>(), WaterFlowFMModelDataSet.BoundaryConditionsTag, DataItemRole.Input, true, WaterFlowFMModelDataSet.BoundaryConditionsTag, typeof(Model1DBoundaryNodeData))
            {
                //ValueType = typeof(FeatureData<IFunction, INode>),
                Owner = this
            };
            
            lateralSourceDataItemSet = new DataItemSet(new EventedList<Model1DLateralSourceData>(), WaterFlowFMModelDataSet.LateralSourcesDataTag, DataItemRole.Input, true, WaterFlowFMModelDataSet.LateralSourcesDataTag, typeof(Model1DLateralSourceData))
            {
                Owner = this
            };
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
            ((INotifyPropertyChange) this).PropertyChanged += OnFMModelPropertyChanged;
        }

        private void OnFMModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MarkDirty();
            if (e.PropertyName == nameof(Name))
            {
                fmRegion.Name = Name;
            }
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
        
        private void BoundaryConditions1DOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model1DBoundaryNodeData.DataType) && sender is Model1DBoundaryNodeData)
            {
                var bc = sender as Model1DBoundaryNodeData;
                var bcDataItem = boundaryNodeDataItemSet.DataItems.First(di => ReferenceEquals(di.Value, bc));
                bcDataItem.Hidden = bc.DataType == Model1DBoundaryNodeDataType.None;
            }
        }
        [EditAction]
        private void BoundaryConditions1DOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var modelDefinitionBoundaryConditions1D = sender as IEventedList<Model1DBoundaryNodeData>;
            if (modelDefinitionBoundaryConditions1D != null)
            {
                var model1DBoundaryNode = e.GetRemovedOrAddedItem() as Model1DBoundaryNodeData;
                if (model1DBoundaryNode == null) return;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (!BoundaryConditions1DDataItemSet.DataItems.Select(di =>di.Value).OfType<Model1DBoundaryNodeData>().Contains(model1DBoundaryNode))
                        {
                            BoundaryConditions1DDataItemSet.DataItems.Add(new DataItem(model1DBoundaryNode){Hidden = model1DBoundaryNode?.DataType == Model1DBoundaryNodeDataType.None});
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        var existingDataItem = BoundaryConditions1DDataItemSet.DataItems
                            .Where(di => di.Value is Model1DBoundaryNodeData).FirstOrDefault(di =>
                                di.Value as Model1DBoundaryNodeData == model1DBoundaryNode);
                        if (existingDataItem != null)
                        {
                            BoundaryConditions1DDataItemSet.DataItems.Remove(existingDataItem);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        BoundaryConditions1DDataItemSet.DataItems.Clear();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        private void LateralSourceDatasOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var model1DLateralSourceDatas = sender as IEventedList<Model1DLateralSourceData>;
            if (model1DLateralSourceDatas != null)
            {
                var model1DLateralSourceData = e.GetRemovedOrAddedItem() as Model1DLateralSourceData;
                if (model1DLateralSourceData == null) return;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (LateralSourcesDataItemSet.DataItems.Where(di => di.Value is Model1DLateralSourceData).All(di => di.Value as Model1DLateralSourceData != model1DLateralSourceData))
                        {
                            LateralSourcesDataItemSet.DataItems.Add(new DataItem(model1DLateralSourceData));
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        var existingDataItem = LateralSourcesDataItemSet.DataItems
                            .Where(di => di.Value is Model1DLateralSourceData).FirstOrDefault(di =>
                                di.Value as Model1DLateralSourceData == model1DLateralSourceData);
                        if (existingDataItem != null)
                        {
                            LateralSourcesDataItemSet.DataItems.Remove(existingDataItem);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        LateralSourcesDataItemSet.DataItems.Clear();
                        break;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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
        }

        private void LoadLinks()
        {
            if (!File.Exists(NetFilePath)) return;
            var loadedLinks = UGrid1D2DLinksAdapter.Load1D2DLinks(NetFilePath).ToList();
            if (NetworkDiscretization == null || Grid == null) return;
            Links1D2DHelper.SetGeometry1D2DLinks(loadedLinks, NetworkDiscretization.Locations, Grid.Cells);
            Links = new EventedList<ILink1D2D>(loadedLinks);
            RefreshMappings();
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
            set { }
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
            private set
            { 
                // just sending an event
            }
        }

        public bool UseSalinity
        {
            get { return (bool)ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool UseSecondaryFlow
        {
            get { return (bool)ModelDefinition.GetModelProperty(KnownProperties.SecondaryFlow).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool UseTemperature
        {
            get { return (HeatFluxModelType)ModelDefinition.GetModelProperty(KnownProperties.Temperature).Value != HeatFluxModelType.None; }
        }
            
        private void ToggleTemperature(bool useTemperature)
        {
            if (UseTemperature == useTemperature) return;
            BoundaryConditions1D.ForEach(bc => bc.UseTemperature = useTemperature);
            LateralSourcesData.ForEach(lat => lat.UseTemperature = useTemperature);
        }
        public bool UseMorSed
        {
            get { return ModelDefinition.UseMorphologySediment; }
            private set
            {
                // empty, but just used for event bubbling                
            }
        }

        public bool WriteSnappedFeatures
        {
            get { return ModelDefinition.WriteSnappedFeatures; }
            private set
            {
                // empty, but just used for event bubbling                
            }
        }

        [PropertyGrid]
        [DisplayName("Validate before run")]
        [Category("Run mode")]
        public bool ValidateBeforeRun { get; set; }

        [PropertyGrid]
        [DisplayName("Show model run console")]
        [Category("Run mode")]
        public bool ShowModelRunConsole { get; set; }

        // DELFT3DFM-371: Disable Model Inspection
        /*
        [PropertyGrid]
        [DisplayName("Model inspection")]
        [Description("Run with model inspection")]
        [Category("Run mode")]
        public bool ModelInspection { get; set; }
        */

        [PropertyGrid]
        [DisplayName("Use RPC")]
        [Description("For development only--remove at release")]
        [Category("Run mode")]
        public bool UseRPC
        {
            get { return !UseLocalApi; }
            set { UseLocalApi = !value; }
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
        }

        protected override void OnDataItemLinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            // subscribe to newly linked hydro area:
            var areaDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.HydroAreaTag);
            if (Equals(e.Target, areaDataItem) && !e.Relinking)
            {
                ((INotifyCollectionChange)areaDataItem.Value).CollectionChanged += HydroAreaCollectionChanged;
                ((INotifyPropertyChanged)areaDataItem.Value).PropertyChanged += HydroAreaPropertyChanged;
            }

            var networkDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag);
            if (Equals(e.Target, networkDataItem) && !e.Relinking)
            {
                var hydroNetwork = (IHydroNetwork) networkDataItem.Value;
                SubscribeToNetwork();
                Network = hydroNetwork;
            }

            base.OnDataItemLinked(sender, e);
        }

        protected override void OnDataItemUnlinking(object sender, LinkingUnlinkingEventArgs<IDataItem> e)
        {
            // unsubscribe from area before unlink
            areaDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.HydroAreaTag);
            if (Equals(e.Target, areaDataItem))
            {
                ((INotifyCollectionChange)areaDataItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
                ((INotifyPropertyChanged)areaDataItem.Value).PropertyChanged -= HydroAreaPropertyChanged;
            }

            var networkDataItem = GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag);
            if (Equals(e.Target, networkDataItem))
            {
                var hydroNetwork = (IHydroNetwork)networkDataItem.Value;

                UnSubscribeFromNetwork();
                Network = hydroNetwork;
            }

            base.OnDataItemUnlinking(sender, e);
        }

        private void LoadStateFromMdu(string mduFilePath)
        {
            // in case we're reloading into an existing flow model instance..cleanup first
            syncers.ForEach(s => s.Dispose());
            syncers.Clear();

            TracerDefinitions.Clear();
            
            LoadModelFromMdu(mduFilePath);

            SynchronizeModelDefinitions();

            // import SedimentFractions (these are not part of the model definition, however they are needed for SourcesAndSinks and TracerDefinitions)
            var mduFileDir = Path.GetDirectoryName(mduFilePath);
            var sedimentFileProperty = ModelDefinition.Properties.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.SedFile));
            if (mduFileDir != null && sedimentFileProperty != null && UseMorSed && File.Exists(Path.Combine(mduFileDir, sedimentFileProperty.Value.ToString())))
            {
                SedimentFile.LoadSediments(SedFilePath, this);
            }

            FireImportProgressChanged(this, "Reading grid", 4, TotalImportSteps);
            Grid = UnstructuredGridFileHelper.LoadFromFile(NetFilePath) ?? new UnstructuredGrid();

            UnstructuredGridFileHelper.DoIfUgrid(NetFilePath, uGridAdaptor =>
                {
                    bathymetryNoDataValue = uGridAdaptor.uGrid.ZCoordinateFillValue;
                });

            FireImportProgressChanged(this, "Renaming sub files", 5, TotalImportSteps);
            RenameSubFilesIfApplicable();

            FireImportProgressChanged(this, "Initialize input spatial data", 6, TotalImportSteps);
            InitializeUnstructuredGridCoverages();

            CoordinateSystem = UnstructuredGridFileHelper.GetCoordinateSystem(NetFilePath);

            // read depth layer definition
            DepthLayerDefinition = ModelDefinition.Kmx == 0
                ? new DepthLayerDefinition(DepthLayerType.Single)
                : new DepthLayerDefinition(ModelDefinition.Kmx);

            // find all names for tracer definitions
            AssembleTracerDefinitions();

            AssembleSpatiallyVaryingSedimentProperties();

            // now that tracers and sediment fractions are imported we can complete the source and sink function
            SourcesAndSinks.ForEach(sourceAndSink =>
            {
                var componentSettings = new Dictionary<string, bool>()
                {
                    {SourceAndSink.SalinityVariableName, UseSalinity},
                    {SourceAndSink.TemperatureVariableName, UseTemperature},
                    {SourceAndSink.SecondaryFlowVariableName, UseSecondaryFlow }
                };

                sourceAndSink.SedimentFractionNames.ForEach(sfn => componentSettings.Add(sfn, UseMorSed));
                sourceAndSink.TracerNames.ForEach(tn => componentSettings.Add(tn, true));
                sourceAndSink.PopulateFunctionValuesFromAttributes(componentSettings);
            });

            FireImportProgressChanged(this, "Reading model output", 8, TotalImportSteps);

            LoadRestartFile(mduFilePath);
            ReconnectOutputFiles(Path.GetDirectoryName(mduFilePath));
            RefreshBoundaryConditions1DDataItemSet();
        }

        private void SynchronizeModelDefinitions()
        {
            //Network = ModelDefinition.Network;
            //NetworkDiscretization = ModelDefinition.NetworkDiscretization;
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

        private void SourcesAndSinksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var sourceAndSink = e.GetRemovedOrAddedItem() as SourceAndSink;

            if (sourceAndSink == null)
                return;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                SyncFractionsAndTracers(sourceAndSink);
            }
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

        private void BoundaryConditionSetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var tracerBoundaryConditions = Enumerable.Empty<FlowBoundaryCondition>(); ;

            var boundaryConditionSet = e.GetRemovedOrAddedItem() as BoundaryConditionSet;
            if (boundaryConditionSet == null)
            {
                var flowBoundaryCondition = e.GetRemovedOrAddedItem() as FlowBoundaryCondition;
                if (flowBoundaryCondition != null && flowBoundaryCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                {
                    tracerBoundaryConditions = new List<FlowBoundaryCondition>() { flowBoundaryCondition };
                }
            }
            else
            {
                tracerBoundaryConditions = boundaryConditionSet.BoundaryConditions
                    .OfType<FlowBoundaryCondition>()
                    .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Tracer);
            }

            foreach (var tracerBoundaryCondition in tracerBoundaryConditions)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        AddTracerToSourcesAndSink(tracerBoundaryCondition.TracerName);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        RemoveTracerFromSourcesAndSink(tracerBoundaryCondition.TracerName);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        throw new NotImplementedException("Renaming of Tracers is not yet supported");
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        SourcesAndSinks.ForEach(ss => ss.TracerNames.Clear());
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
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

        private void TracerDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var name = (string) e.GetRemovedOrAddedItem();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // sync the initial tracers
                    InitialTracers.Add(CreateUnstructuredGridCellCoverage(name, Grid));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // sync the initial tracers
                    InitialTracers.RemoveAllWhere(tr => tr.Name == name);

                    // remove all boundary conditions with that tracer name
                    foreach (var set in BoundaryConditionSets)
                    {
                        set.BoundaryConditions.RemoveAllWhere(bc =>
                        {
                            var flowCondition = bc as FlowBoundaryCondition;

                            if (flowCondition != null && 
                                flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer &&
                                Equals(flowCondition.TracerName, e.GetRemovedOrAddedItem()))
                            {
                                return true;
                            }
                            return false;
                        });
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // can't rename yet
                    throw new NotImplementedException("Renaming of tracer definitions is not yet supported");
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // sync the initial tracers
                    InitialTracers.Clear();

                    // remove all tracer boundary conditions
                    foreach (var set in BoundaryConditionSets)
                    {
                        set.BoundaryConditions.RemoveAllWhere(bc =>
                        {
                            var flowCondition = bc as FlowBoundaryCondition;
                            return flowCondition != null && flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer;
                        });
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AssembleTracerDefinitions()
        {
            foreach (var boundaryCondition in BoundaryConditions)
            {
                var flowCondition = boundaryCondition as FlowBoundaryCondition;
                if (flowCondition != null)
                {
                    if (flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                    {
                        if(!TracerDefinitions.Contains(flowCondition.TracerName))
                        {
                            TracerDefinitions.Add(flowCondition.TracerName);
                        }
                        AddTracerToSourcesAndSink(flowCondition.TracerName);
                    }
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

        private void SedimentFractionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                var sedimentFraction = sender as ISedimentFraction;

                if (sedimentFraction != null)
                {
                    sedimentFraction.UpdateSpatiallyVaryingNames();
                }
            }

            if (e.PropertyName == "CurrentFormulaType"
                || e.PropertyName == "CurrentSedimentType")
            {
                var sedimentFraction = sender as ISedimentFraction;
                if (sedimentFraction != null)
                {
                    sedimentFraction.UpdateSpatiallyVaryingNames();
                    var activeSpatiallyVarying = sedimentFraction.GetAllActiveSpatiallyVaryingPropertyNames();
                    var spatiallyVarying = sedimentFraction.GetAllSpatiallyVaryingPropertyNames();
                    InitialFractions.RemoveAllWhere( 
                        fr => spatiallyVarying.Contains(fr.Name) && !activeSpatiallyVarying.Contains(fr.Name));

                    foreach (var layerName in activeSpatiallyVarying)
                    {
                        AddToIntialFractions(layerName);
                    }
                    
                    sedimentFraction.CompileAndSetVisibilityAndIfEnabled();

                    if (e.PropertyName == "CurrentFormulaType")
                    {
                        sedimentFraction.SetTransportFormulaInCurrentSedimentType();
                    }
                }
                return;
            }

            var prop = sender as ISpatiallyVaryingSedimentProperty;
            if (prop == null) return;

            if (e.PropertyName == "IsSpatiallyVarying")
            {
                if (prop.IsSpatiallyVarying)
                {
                    AddToIntialFractions(prop.SpatiallyVaryingName);
                }
                else
                {
                    InitialFractions.RemoveAllWhere(tr => tr.Name.Equals(prop.SpatiallyVaryingName));
                }
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

        private void SedimentFractionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var sedimentFraction = e.GetRemovedOrAddedItem() as ISedimentFraction;
            if( sedimentFraction == null )
                return;
            var name = sedimentFraction.Name;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    sedimentFraction.UpdateSpatiallyVaryingNames();
                    sedimentFraction.CompileAndSetVisibilityAndIfEnabled();
                    sedimentFraction.SetTransportFormulaInCurrentSedimentType();
                    SourcesAndSinks.ForEach(ss => ss.SedimentFractionNames.Add(sedimentFraction.Name));

                    if (InitialFractions == null || BoundaryConditionSets == null) break;
                    
                    // sync the initial fractions
                    SyncInitialFractions(sedimentFraction);                
                    AddSedimentFractionToFlowBoundaryConditionFunction(name);            
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // sync the initial fractions
                    var layersToRemove = sedimentFraction.GetAllActiveSpatiallyVaryingPropertyNames();
                    InitialFractions.RemoveAllWhere( ifs => layersToRemove.Contains(ifs.Name) );

                    // Remove dataItems for coverages related to Removed Fraction
                    DataItems.RemoveAllWhere(di => di.Value is UnstructuredGridCoverage && layersToRemove.Contains(di.Name));                               
                    RemoveSedimentFractionFromBoundaryConditionSets(name);

                    SourcesAndSinks.ForEach(ss => ss.SedimentFractionNames.Remove(sedimentFraction.Name));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException("Renaming of sediment fraction is not yet supported");
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // sync the initial fractions
                    InitialFractions.Clear();

                    RemoveAllSedimentFractionsFromBoundaryConditionSets();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

        public void ImportSpatialOperationsAfterLoading()
        {
            foreach (var spatialOperation in ModelDefinition.SpatialOperations)
            {
                var dataItemName = spatialOperation.Key;
                var spatialOperationList = spatialOperation.Value;
                var dataItem = DataItems.FirstOrDefault(di => di.Name == dataItemName);

                // when only one operation is found and it has the same name as when you would generate it from saving,
                // it will not override the operations found in the database. Assuming that we are loading a dsproj file.
                // Goes wrong when you change the file name of the quantity and you only have one quantity.
                if (spatialOperationList.Count != 1 || !(spatialOperationList[0] is ImportSamplesOperation) ||
                    dataItem == null || dataItem.ValueConverter != null || !(dataItem.Value is UnstructuredGridCoverage))
                    continue;
                IEnumerable<double> valuesToSet;
                var coverage = (UnstructuredGridCoverage)dataItem.Value;
                if (spatialOperationList[0] is ImportRasterSamplesSpatialOperationExtension)
                {
                    var samplesOperation = (ImportRasterSamplesSpatialOperationExtension)spatialOperationList[0];
                    
                    var rasterFile = RasterFile.ReadPointValues(samplesOperation.FilePath).ToList();

                    var componentValueCount = coverage.Arguments.Aggregate(0,
                        (totaal, arguments) => totaal == 0 ? arguments.Values.Count : totaal * arguments.Values.Count);

                    valuesToSet = rasterFile.Count != componentValueCount
                        ? new InterpolateOperation().InterpolateToGrid(rasterFile, coverage, coverage.Grid)
                        : rasterFile.Select(p => p.Value);
                }
                else
                {
                    var samplesOperation = (ImportSamplesOperation) spatialOperationList[0];
                    var xyzFile = XyzFile.Read(samplesOperation.FilePath).ToList();

                    var componentValueCount = coverage.Arguments.Aggregate(0,
                        (totaal, arguments) => totaal == 0 ? arguments.Values.Count : totaal * arguments.Values.Count);

                    valuesToSet = xyzFile.Count != componentValueCount
                        ? new InterpolateOperation().InterpolateToGrid(xyzFile, coverage, coverage.Grid)
                        : xyzFile.Select(p => p.Value);
                }

                if(valuesToSet.Any())
                    coverage.SetValues(valuesToSet);
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
                    var importSamplesSpatialOperationExtension = operation as ImportSamplesSpatialOperationExtension;
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
            }
        }
        
        private IEventedList<IDataItem> AddSpatialDataItems()
        {
            AddOrRenameDataItem(Bathymetry, WaterFlowFMModelDefinition.BathymetryDataItemName);

            // Backwards compatibility
            // BedLevel dataitem value used to be exclusively UnstructuredGridVertexCoverages, now it needs to be more generic
            var bedLevelDataItem = DataItems.FirstOrDefault(di => di.Name == WaterFlowFMModelDefinition.BathymetryDataItemName);
            if (bedLevelDataItem != null) bedLevelDataItem.ValueType = typeof(UnstructuredGridCoverage);

            AddOrRenameDataItem(InitialWaterLevel, WaterFlowFMModelDefinition.InitialWaterLevelDataItemName);
            AddOrRenameDataItem(Roughness, WaterFlowFMModelDefinition.RoughnessDataItemName);
            AddOrRenameDataItem(Viscosity, WaterFlowFMModelDefinition.ViscosityDataItemName);
            AddOrRenameDataItem(Diffusivity, WaterFlowFMModelDefinition.DiffusivityDataItemName);
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
                var boundaryCondition1DDataItems = BoundaryConditions1D.Select(bc => bc.SeriesDataItem);
                var lateralDataItems = LateralSourcesData.Select(d => d.SeriesDataItem);

                return base.AllDataItems.Concat(areaDataItems.Values.SelectMany(v => v)).Concat(boundaryCondition1DDataItems).Concat(lateralDataItems);
            }
        }

        private void OnModelDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var prop = sender as WaterFlowFMProperty;
            if (prop != null && e.PropertyName == TypeUtils.GetMemberName(() => prop.Value))
            {
                if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.FixedWeirScheme,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    allFixedWeirsAndCorrespondingProperties?.ForEach(p => p.UpdateDataColumns(prop.GetValueAsString()));                    
                }
                if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.BedlevType,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    var bedLevelType = (UnstructuredGridFileHelper.BedLevelLocation)prop.Value;
                    BeginEdit(new DefaultEditAction("Updating Bathymetry coverage"));
                    UpdateBathymetryCoverage(bedLevelType);
                    EndEdit();
                }

                if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.UseSalinity,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching salinity process"));
                    UseSalinity = UseSalinity;
                    BoundaryConditions1D?.ForEach(bc => bc.UseSalt = UseSalinity);
                    LateralSourcesData?.ForEach(lat => lat.UseSalt = UseSalinity);
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.UseMorSed,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching morphology process"));
                    UseMorSed = UseMorSed;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteSnappedFeatures,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching write snapped features options"));
                    WriteSnappedFeatures = WriteSnappedFeatures;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ISlope,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching Bed slope formulation"));
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.IHidExp,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching Hiding and exposure formulation"));
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.Kmx,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching 3D dynamics"));
                    UseDepthLayers = UseDepthLayers;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.ICdtyp,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching wind formulation type"));
                    CdType = CdType;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.Temperature,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching heat flux model"));
                    HeatFluxModelType = ModelDefinition.HeatFluxModel.Type;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.SecondaryFlow,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching secondary flow process"));
                    UseSecondaryFlow = UseSecondaryFlow;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteHisFile,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching WriteHisFile"));
                    WriteHisFile = WriteHisFile;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyHisStart,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching SpecifyHisStart"));
                    SpecifyHisStart = SpecifyHisStart;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyHisStop,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching SpecifyHisStop"));
                    SpecifyHisStop = SpecifyHisStop;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteMapFile,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching WriteMapFile"));
                    WriteMapFile = WriteMapFile;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyMapStart,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching SpecifyMapStart"));
                    SpecifyMapStart = SpecifyMapStart;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyMapStop,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching SpecifyMapStop"));
                    SpecifyMapStop = SpecifyMapStop;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteRstFile,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching WriteRstFile"));
                    WriteRstFile = WriteRstFile;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyRstStart,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching SpecifyRstStart"));
                    SpecifyRstStart = SpecifyRstStart;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyRstStop,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching SpecifyRstStop"));
                    SpecifyRstStop = SpecifyRstStop;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.WaveModelNr,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching Waves Model Nr"));
                    WaveModel = WaveModel;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.Irov,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching Wall behavior type"));
                    WaveModel = WaveModel;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyWaqOutputInterval,
                StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching Waq output interval time"));
                    SpecifyWaqOutputInterval = SpecifyWaqOutputInterval;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyWaqOutputStartTime,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching Waq output start time"));
                    SpecifyWaqOutputStartTime = SpecifyWaqOutputStartTime;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.SpecifyWaqOutputStopTime,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching Waq output end time"));
                    SpecifyWaqOutputStopTime = SpecifyWaqOutputStopTime;
                    EndEdit();
                }
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

        public void Dispose()
        {
            disposing = true;
            // also disposes grid snap api, so if you remove this, at least make sure you dispose that one (holds remote instance in the air):
            Grid = null;
            DisposeSnapApi();
            syncers.ForEach(s => s.Dispose());
            syncers.Clear();

            allFixedWeirsAndCorrespondingProperties.ForEach(d => d.Dispose());
            BridgePillarsDataModel.ForEach( d => d.Dispose());
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

            //uncomment when required:
            //yield return Grid;

            yield return Links;

            foreach (var link in Links)
            {
                yield return link;
            }

            yield return InitialSalinity;
            yield return Viscosity;
            yield return Diffusivity;
            yield return Roughness;
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
            if ((role & DataItemRole.Input) == DataItemRole.Input || (role & DataItemRole.Output) == DataItemRole.Output)
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

                INetworkLocation[] segmentsCentroidLocations = NetworkDiscretization.Segments.Values
                                        .Where(s => s.Geometry.Centroid != null)
                                        .Select(s => new NetworkLocation(s.Branch, (s.EndChainage + s.Chainage) / 2))
                                        .OfType<INetworkLocation>()
                                        .ToArray();

                yield return new Feature // all locations
                {
                    Geometry = NetworkDiscretization.Geometry,
                    Attributes = new DictionaryFeatureAttributeCollection
                            {
                                { "locations", NetworkDiscretization.Locations.Values },
                                { "StandardFeatureName", EngineParameters.GetStandardFeatureName(ElementSet.GridpointsOnBranches)},
                                { "ElementType", "GridpointsOnBranches" }
                            }
                };

                yield return new Feature // all staggered locations
                {
                    Geometry = new GeometryCollection(segmentsCentroidLocations.Select(nl => nl.Geometry).ToArray()),
                    Attributes = new DictionaryFeatureAttributeCollection
                            {
                                { "locations", segmentsCentroidLocations },
                                { "StandardFeatureName", EngineParameters.GetStandardFeatureName(ElementSet.ReachSegElmSet)},
                                { "ElementType", "ReachSegElmSet" }
                            }
                };
            }
        }
        public override IEnumerable<IDataItem> GetChildDataItems(IFeature location)
        {
            if (location == null) yield break;

            List<IDataItem> items;
            areaDataItems.TryGetValue(location, out items);

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
                    var existingDataItem =
                        networkDataItem.Children.FirstOrDefault(
                            delegate (IDataItem di)
                            {
                                var valueConverter = di.ValueConverter as Model1DBranchFeatureValueConverter;
                                return di.ValueType == typeof(double)
                                       && (
                                           valueConverter != null &&
                                           valueConverter.ParameterName == engineParameter.Name &&
                                           valueConverter.Role == engineParameter.Role
                                           && valueConverter.ElementSet == engineParameter.ElementSet &&
                                           valueConverter.QuantityType == engineParameter.QuantityType
                                           && Equals(valueConverter.Location, location
                                           ));
                            });

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
                            new Unit("Cubic meter", "m³"));
                        break;
                    case Model1DBoundaryNodeDataType.FlowConstant:
                    case Model1DBoundaryNodeDataType.FlowTimeSeries:
                        yield return new EngineParameter(QuantityType.Discharge, ElementSet.QBoundaries,
                            DataItemRole.Input, FunctionAttributes.StandardNames.WaterDischarge,
                            new Unit("Cubic meter", "m³"));
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

        private IList<ExplicitValueConverterLookupItem> explicitValueConverterLookupItems;

        public bool UseLocalApi { get; set; }
        
        // Do not remove...used in HydroModelBuilder.py
        public void SetWaveForcing()
        {
            ModelDefinition.GetModelProperty(KnownProperties.WaveModelNr).SetValueAsString("3");
        }
        
        protected override void OnInputCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        // [TOOLS-22813] Override OnInputPropertyChanged to stop base class (ModelBase) from clearing the output
        protected override void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model1DBoundaryNodeData.DataType) && sender is Model1DBoundaryNodeData)
            {
                var bc = sender as Model1DBoundaryNodeData;
                
                var bcDataItem = boundaryNodeDataItemSet.DataItems.First(di => ReferenceEquals(di.Value, bc));
                bcDataItem.Hidden = bc.DataType == Model1DBoundaryNodeDataType.None;
            }
        }

        protected override void OnClearOutput()
        {
            if (OutputMapFileStore != null)
            {
                OutputMapFileStore.Functions.Clear();
                OutputMapFileStore.Close();
                OutputMapFileStore = null;
            }
            if (Output1DFileStore != null)
            {            
                Output1DFileStore.Functions.Clear();
                Output1DFileStore.Close();
                Output1DFileStore = null;
            }
            if (OutputHisFileStore != null)
            {
                OutputHisFileStore.Functions.Clear();
                OutputHisFileStore.Close();
                OutputHisFileStore = null;
            }
        }
       
        public override IProjectItem DeepClone()
        {
            var tempDir = FileUtils.CreateTempDirectory();
            var mduFileName = MduFilePath != null ? Path.GetFileName(MduFilePath) : "some_temp.mdu";
            var tempFilePath = Path.Combine(tempDir, mduFileName);
            ExportTo(tempFilePath, false);

            return new WaterFlowFMModel(tempFilePath);
        }

        public event EventHandler AfterExecute;

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
                    OutputMapFileStore.CoordinateSystem = value;
                }

                if (Output1DFileStore != null)
                {
                    //Output1DFileStore.CoordinateSystem = value;
                }
                
                if (Network != null)
                {
                    if (Network.CoordinateSystem != value) Network.CoordinateSystem = value;
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

        private readonly string tempWorkingDirectory;
        public virtual string WorkingDirectory
        {
            get { return ExplicitWorkingDirectory ?? tempWorkingDirectory; }
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

        private void SpatialDataLayersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Equals(sender, InitialSalinity.Coverages))
            {
                AddOrRenameDataItems(InitialSalinity, WaterFlowFMModelDefinition.InitialSalinityDataItemName);
            }
            else
            {
                throw new ArgumentException("Unexpected layered spatial data: " + e.GetRemovedOrAddedItem());
            }
        }

        private void SpatialDataTracersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Equals(sender, InitialTracers))
            {
                AddOrRenameTracerDataItems();
            }
            else
            {
                throw new ArgumentException("Unexpected layered spatial data: " + e.GetRemovedOrAddedItem());
            }
        }

        private void SpatialDataFractionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Equals(sender, InitialFractions))
            {
                AddOrRenameFractionDataItems();

                // Invoke property changed, so Gui can update
                InitialCoverageSetChanged = true;
            }
            else
            {
                throw new ArgumentException("Unexpected layered spatial data: " + e.GetRemovedOrAddedItem());
            }
        }

        public bool InitialCoverageSetChanged { get; set; }

        #endregion

        #region Mdu file

        private readonly MduFile mduFile = new MduFile();

        public string MduSavePath
        {
            get { return GetMduPathFromDeltaShellPath(Path.GetDirectoryName(MduFilePath)); }
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

        //Do not remove, is used by python code
        public string ComFilePath
        {
            get
            {
                return !String.IsNullOrEmpty(MduFilePath)
                    ? Path.Combine(WorkingDirectory, ModelDefinition.RelativeComFilePath)
                    : null;
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
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyHisStart
        {
            get { return (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyHisStart).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyHisStop
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyHisStop).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool WriteMapFile
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyMapStart
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyMapStart).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyMapStop
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyMapStop).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool WriteRstFile
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyRstStart
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyRstStart).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyRstStop
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyRstStop).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyWaqOutputInterval
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyWaqOutputStartTime
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStartTime).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }
        public bool SpecifyWaqOutputStopTime
        {
            get { return (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStopTime).Value; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public object WaveModel
        {
            // cannot actually return anything, because it's a dynamic enum
            get { return null; }
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public ImportProgressChangedDelegate ImportProgressChanged { get; set; }
        
        private void LoadModelFromMdu(string mduFilePath)
        {
            MduFilePath = mduFilePath;
            var mduFileDir = Path.GetDirectoryName(mduFilePath);
            Name = Path.GetFileNameWithoutExtension(mduFilePath);
            ModelDefinition = new WaterFlowFMModelDefinition(mduFileDir, Name);
            
            // initialize model definition from mdu file if it exists
            if (File.Exists(mduFilePath))
            {
                isLoading = true;
                mduFile.Read(mduFilePath, ModelDefinition, Area, Network, NetworkDiscretization, BoundaryConditions1D, LateralSourcesData, allFixedWeirsAndCorrespondingProperties, (name, current, total) => FireImportProgressChanged(this, "Reading mdu - " + name, current, total), BridgePillarsDataModel);

                isLoading = false;
                SyncModelTimesWithBase();
            }

            var netFileProperty = ModelDefinition.GetModelProperty(KnownProperties.NetFile);
            if (String.IsNullOrEmpty(netFileProperty.GetValueAsString()))
            {
                netFileProperty.Value = Name + NetFile.FullExtension;
            }

            FireImportProgressChanged(this, "Loading restart", 2, TotalImportSteps);
            LoadRestartInfo(mduFilePath);

            // sync the heat flux model, because events are off during reading
            HeatFluxModelType = ModelDefinition.HeatFluxModel.Type;
        }
        
        internal void SyncModelTimesWithBase()
        {
            base.StartTime = StartTime;
            base.StopTime = StopTime;
            base.TimeStep = TimeStep;
        }

        private static void FireImportProgressChanged(WaterFlowFMModel model, string currentStepName, int currentStep, int totalSteps)
        {
            if (model.ImportProgressChanged == null) return;
            model.ImportProgressChanged(currentStepName, currentStep, totalSteps);
        }

        public virtual bool ExportTo(string mduPath, bool switchTo = true, bool writeExtForcings = true, bool writeFeatures=true)
        {
            var dirName = Path.GetDirectoryName(mduPath);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            // make sure on save / export, restart file + mdu are up to date and could be ran standalone with correct info
            if (switchTo)
            {
                SaveRestartInfo(mduPath);
            }
            InitializeRestart(dirName);

            if (switchTo)
            {
                RenameSubFilesIfApplicable();
            }
            
            if (writeExtForcings)
            {
                var spatVarSedPropNames =
                    SedimentFractions.Where(sf => sf.CurrentSedimentType != null).SelectMany(
                        sf =>
                            sf.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>()
                                .Where(p => p.IsSpatiallyVarying)).Select(p => p.SpatiallyVaryingName).ToList();
                spatVarSedPropNames.AddRange(SedimentFractions.Where(sf => sf.CurrentFormulaType != null).SelectMany(
                        sf =>
                            sf.CurrentFormulaType.Properties.OfType<ISpatiallyVaryingSedimentProperty>()
                                .Where(p => p.IsSpatiallyVarying)).Select(p => p.SpatiallyVaryingName).ToList());
                ModelDefinition.SelectSpatialOperations(DataItems, TracerDefinitions, spatVarSedPropNames) ;
                ModelDefinition.Bathymetry = Bathymetry;
            }

            if (!IsEditing)
                InitializeAreaDataColumns();
            ReloadGrid(); 

            
            mduFile.Write(mduPath, ModelDefinition, Area, Network, RoughnessSections, BoundaryConditions1D, LateralSourcesData, allFixedWeirsAndCorrespondingProperties, switchTo: switchTo, writeExtForcings: writeExtForcings, writeFeatures: writeFeatures, disableFlowNodeRenumbering: DisableFlowNodeRenumbering, sedimentModelData: UseMorSed ? this : null);
            
            if (!IsEditing)
                RestoreAreaDataColumns();

            if (switchTo)
            {
                MduFilePath = mduPath;
                SaveOutput();
            }
            return true;
        }

        private void InitializeAreaDataColumns()
        {
            MduFile.SetBridgePillarAttributes(Area.BridgePillars, BridgePillarsDataModel);
        }

        private void RestoreAreaDataColumns()
        {
            MduFile.CleanBridgePillarAttributes(Area.BridgePillars);
        }

        private void OnSwitchTo(string mduPath)
        {
            if (MduFilePath == null) // switch from nothing: load
            {
                OnLoad(mduPath);
            }
            else // else: switch from existing: only change path
            {
                MduFilePath = mduPath;
                
                if (MduFile == null) return;
                mduFile.Path = mduPath;
                SwitchFileBasedItems();
            }
        }

        private void SwitchFileBasedItems()
        {
            foreach (var windField in WindFields.OfType<IFileBased>())
            {
                var newPath = Path.Combine(Path.GetDirectoryName(ExtFilePath), Path.GetFileName(windField.Path));
                windField.SwitchTo(newPath);
            }

            foreach (var notUsedExtForceFileItem in UnsupportedFileBasedExtForceFileItems)
            {
                var newPath = Path.Combine(Path.GetDirectoryName(ExtFilePath), Path.GetFileName(notUsedExtForceFileItem.Path));
                notUsedExtForceFileItem.SwitchTo(newPath);
            }
        }

        private void OnLoad(string mduPath)
        {
            CreateDataItemsNotAvailableInPreviousVersion();
            LoadStateFromMdu(mduPath);
            
            FeatureFile1D2DReader.Read1D2DFeatures(mduPath, ModelDefinition, Network, RoughnessSections);

            LoadLinks();

            ImportSpatialOperationsAfterLoading();
        }

        private void OnAddedToProject(string mduPath)
        {
            MduFilePath = mduPath;
            ExportTo(MduFilePath);
        }

        private void OnSave()
        {
            string modelDir = null;
            string outputDir = null;
            if (MduFilePath != MduSavePath)
            {
                modelDir = Path.GetDirectoryName(MduFilePath);
                outputDir = Path.GetDirectoryName(MapFilePath);
            }
            if( ExportTo(MduSavePath))
            {
                /*Make sure the ModelDirectory gets updated when saving*/
                ModelDefinition.ModelDirectory = Path.GetDirectoryName(MduSavePath);
            }
            if (modelDir != null && Directory.Exists(modelDir))
            {
                Directory.Delete(modelDir, true);
            }
            if (outputDir != null && Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
        }

        private string GetMduPathFromDeltaShellPath(string path)
        {
            var directoryName = path != null
                ? Path.GetDirectoryName(path) ?? ""
                : "";

            // dsproj_data/<model name>/<model name>.mdu
            return Path.Combine(directoryName, Name, Name + ".mdu");
        }

        private void RenameSubFilesIfApplicable()
        {
            foreach (var subFile in SubFiles) 
            {
                var waterFlowFMProperty = subFile.Key;
                
                if (waterFlowFMProperty.GetValueAsString().Equals(subFile.Value)) continue;
                
                if (waterFlowFMProperty.Equals(ModelDefinition.GetModelProperty(KnownProperties.NetFile)))
                {
                    var oldPath = NetFilePath;
                    waterFlowFMProperty.SetValueAsString(subFile.Value);
                    var newPath = NetFilePath;

                    if (!File.Exists(oldPath) ||
                        String.Equals(Path.GetFullPath(oldPath), Path.GetFullPath(newPath),
                            StringComparison.CurrentCultureIgnoreCase)) continue;
                    
                    File.Copy(oldPath, newPath, true);
                    File.Delete(oldPath);
                }
                else
                {
                    waterFlowFMProperty.SetValueAsString(subFile.Value);
                }
            }

            ModelDefinition.ModelName = Name;
        }

        #endregion

        #region IFileBased

        // todo: transactional?
        private string filePath;
        private bool isOpen;

        string IFileBased.Path
        {
            get { return filePath; }
            set
            {
                if (filePath == value)
                    return;

                filePath = value;

                if (filePath == null)
                    return;

                if (filePath.StartsWith("$"))
                {
                    if (MduFilePath != null)
                        OnSave();
                }
            }
        }

        IEnumerable<string> IFileBased.Paths
        {
            get { yield return ((IFileBased) this).Path; }
        }

        public bool IsFileCritical { get { return true; } }

        bool IFileBased.IsOpen
        {
            get { return isOpen; }
        }

        void IFileBased.CreateNew(string path)
        {
            OnAddedToProject(GetMduPathFromDeltaShellPath(path));
            filePath = path;
            isOpen = true;
        }

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

        void IFileBased.Close()
        {
            isOpen = false;
        }

        void IFileBased.Open(string path)
        {
            isOpen = true;
        }

        void IFileBased.CopyTo(string destinationPath)
        {
            var mduPath = GetMduPathFromDeltaShellPath(destinationPath);

            var dirName = Path.GetDirectoryName(mduPath);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            RenameSubFilesIfApplicable();
            ExportTo(mduPath, false);
        }

        /// <summary>
        /// Relocate to reconnects the item to the given path. Does NOT perform copyTo.
        /// </summary>
        void IFileBased.SwitchTo(string newPath)
        {
            filePath = newPath;
            OnSwitchTo(GetMduPathFromDeltaShellPath(newPath));
        }

        void IFileBased.Delete()
        {
            // todo: delete mdu & stuff
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

        private const int TotalImportSteps = 10;

        #endregion

        #region Output

        public string OutputSnappedFeaturesPath
        {
            get
            {
                var outputDirectory = ExplicitWorkingDirectory;
                if (outputDirectory == null)
                {
                    //We might still be working in the temp folder.
                    outputDirectory = WorkingDirectory;
                }

                return Path.Combine(outputDirectory, DirectoryName, ModelDefinition.OutputDirectory, SnappedFeaturesDirectoryName);
            }
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
        private string WaqHydFilePath { get; set; }

        private void SaveOutput()
        {
            var oldMapFilePath = OutputMapFileStore == null ? null : OutputMapFileStore.Path;
            var oldHisFilePath = OutputHisFileStore == null ? null : OutputHisFileStore.Path;

            if (oldMapFilePath != null && Path.GetFullPath(oldMapFilePath).ToLower() != Path.GetFullPath(MapFilePath).ToLower())
            {
                var directory = Path.GetDirectoryName(MapFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.Copy(oldMapFilePath, MapFilePath, true);
            }
            else if (oldMapFilePath == null && File.Exists(MapFilePath))
            {
                File.Delete(MapFilePath);
            }

            if (oldHisFilePath != null && Path.GetFullPath(oldHisFilePath).ToLower() != Path.GetFullPath(HisFilePath).ToLower())
            {
                var directory = Path.GetDirectoryName(HisFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.Copy(oldHisFilePath, HisFilePath, true);
            }
            else if (oldHisFilePath == null && File.Exists(HisFilePath))
            {
                File.Delete(HisFilePath);
            }

            // copy the complete delwaq output folder
            string waqOutputDir = Path.Combine(Path.GetDirectoryName(MduFilePath), DelwaqHydFolderName);
            if(WaqHydFilePath != null && WaqHydFilePath != waqOutputDir)
            {
                // delete the old delwaq files, they have been recreated
                FileUtils.DeleteIfExists(waqOutputDir);
                FileUtils.CopyDirectory(WaqHydFilePath, waqOutputDir);
            }

            ReconnectOutputFiles(MapFilePath, HisFilePath, waqOutputDir, switchTo: true);
        }

        public string DelwaqHydFolderName
        {
            get { return "DFM_DELWAQ_" + Name; }
        }

        protected virtual void ReconnectOutputFiles(string outputDirectory)
        {
            ReconnectOutputFiles(Path.Combine(outputDirectory, ModelDefinition.RelativeMapFilePath),
                Path.Combine(outputDirectory, ModelDefinition.RelativeHisFilePath), Path.Combine(outputDirectory, DelwaqHydFolderName));
        }

        private void ReconnectOutputFiles(string mapFilePath, string hisFilePath, string waqFolderPath, bool switchTo = false)
        {
            var existsMapFile = File.Exists(mapFilePath);
            var existsHisFile = File.Exists(hisFilePath);
            var existsWaqFolder = Directory.Exists(waqFolderPath);

            if (!existsMapFile && !existsHisFile && !existsWaqFolder) return;

            FireImportProgressChanged(this, "Reading output files - Reading Map file", 1, 2);
            BeginEdit(new DefaultEditAction("Reconnect output files"));

            // deal with issue that kernel doesn't understand any coordinate systems other than RD & WGS84 :
            if (existsMapFile)
            {
                ReportProgressText("Reading map file");
                var cs = UnstructuredGridFileHelper.GetCoordinateSystem(mapFilePath);

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
                        OutputMapFileStore.Path = mapFilePath;
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
                        Output1DFileStore = new FM1DFileFunctionStore();
                        // don't change this to a property setter, because the timing is of great importance.
                        // elsewise, there will be no subscription to the read and Path triggers the Read().
                        Output1DFileStore.Path = mapFilePath;
                    }
                }
            }

            if (existsHisFile)
            {
                ReportProgressText("Reading his file");
                FireImportProgressChanged(this, "Reading output files - Reading His file", 1, 2);
                if (switchTo && OutputHisFileStore != null)
                {

                    OutputHisFileStore.Path = hisFilePath;
                }
                else
                {
                    OutputHisFileStore = new FMHisFileFunctionStore(hisFilePath, CoordinateSystem,
                        Area.ObservationPoints,
                        Area.ObservationCrossSections,
                        Area.Weirs.Where( w => w.WeirFormula is GeneralStructureWeirFormula),
                        Area.LeveeBreaches);
                }
            }

            if (existsWaqFolder)
            {
                WaqHydFilePath = waqFolderPath;
            }
            
            OutputIsEmpty = false;

            EndEdit();
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

            return false;
        }

        private void HydroAreaCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!isLoading)
            {
                var fixedWeir = e.GetRemovedOrAddedItem() as FixedWeir;
                if (fixedWeir != null)
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (allFixedWeirsAndCorrespondingProperties.FirstOrDefault(d => d.Feature == fixedWeir) == null)
                            {
                                allFixedWeirsAndCorrespondingProperties.Add(
                                    CreateModelFeatureCoordinateDataFor(fixedWeir));
                            }

                            break;
                        case NotifyCollectionChangedAction.Remove:
                            var dataToRemove =
                                allFixedWeirsAndCorrespondingProperties.FirstOrDefault(d => d.Feature == fixedWeir);
                            if (dataToRemove == null) break;

                            allFixedWeirsAndCorrespondingProperties.Remove(dataToRemove);
                            dataToRemove.Dispose();
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            var dataToUpdate =
                                allFixedWeirsAndCorrespondingProperties.FirstOrDefault(d => d.Feature == fixedWeir);
                            if (dataToUpdate == null)
                            {
                                allFixedWeirsAndCorrespondingProperties.Add(
                                    CreateModelFeatureCoordinateDataFor(fixedWeir));
                                break;
                            }

                            dataToUpdate.Feature = fixedWeir;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                var bridgePillar = e.GetRemovedOrAddedItem() as BridgePillar;
                if (bridgePillar != null)
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            BridgePillarsDataModel.Add(
                                CreateModelFeatureCoordinateDataFor(bridgePillar));
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            var dataToRemove =
                                BridgePillarsDataModel.FirstOrDefault(
                                    d => d.Feature == bridgePillar);
                            if (dataToRemove == null) break;

                            BridgePillarsDataModel.Remove(dataToRemove);
                            dataToRemove.Dispose();
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            var dataToUpdate =
                                BridgePillarsDataModel.FirstOrDefault(
                                    d => d.Feature == bridgePillar);
                            if (dataToUpdate == null)
                            {
                                BridgePillarsDataModel.Add(
                                    CreateModelFeatureCoordinateDataFor(bridgePillar));
                                break;
                            }

                            dataToUpdate.Feature = bridgePillar;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            var groupableFeature = e.GetRemovedOrAddedItem() as IGroupableFeature;
            if (groupableFeature != null && e.Action != NotifyCollectionChangedAction.Remove && !Area.IsEditing)
            {
                groupableFeature.UpdateGroupName(this);
            }
            
            var inputSender = InputFeatureCollectionsContains(e.GetRemovedOrAddedItem());
            var outputSender = OutputFeatureCollectionsContains(e.GetRemovedOrAddedItem());
            
            if (inputSender || outputSender)
            {
                var feature = (IFeature) e.GetRemovedOrAddedItem();
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        AddAreaItem(feature, inputSender);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        RemoveAreaFeature(feature);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        foreach (var areaDataItem in areaDataItems)
                        {
                            RemoveAreaFeature(areaDataItem.Key);
                        }
                        areaDataItems.Clear();
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        var oldFeature = e.OldItems?.OfType<IFeature>().FirstOrDefault();
                        RemoveAreaFeature(oldFeature);
                        AddAreaItem(feature, inputSender);
                        break;
                    default:
                        throw new NotImplementedException(
                            String.Format("Action {0} on feature collection not supported", e.Action));
                }
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

        private void HydroAreaPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var weir = sender as IWeir;
            if (weir != null)
            {
                if (e.PropertyName == TypeUtils.GetMemberName<Weir>(w => w.WeirFormula))
                {
                    var isInputSender = Area.Weirs.Any(w => w.Name == weir.Name);
                    UpdateAreaDataItems(weir, isInputSender);
                }
            }

            var groupableFeature = sender as IGroupableFeature;
            if (updatingGroupName || Area.IsEditing || groupableFeature == null ||
                e.PropertyName != TypeUtils.GetMemberName<IGroupableFeature>(g => g.GroupName)) return;

            updatingGroupName = true;// prevent recursive calls

            groupableFeature.UpdateGroupName(this);

            if (groupableFeature.IsDefaultGroup)
            {
                groupableFeature.IsDefaultGroup = false;
            }

            updatingGroupName = false;
        }

        private void RemoveAreaFeature(IFeature feature)
        {
            List<IDataItem> dataItemsToBeRemoved;
            if (areaDataItems.TryGetValue(feature, out dataItemsToBeRemoved))
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
                yield return KnownStructureProperties.GateLowerEdgeLevel;
                yield return KnownStructureProperties.GateOpeningWidth;
                yield return KnownStructureProperties.GateSillLevel;
            }

            var weir = location as IWeir;
            if (weir != null)
            {
                yield return KnownStructureProperties.CrestLevel;

                var weirFormula = weir.WeirFormula as GeneralStructureWeirFormula;
                if (weirFormula != null)
                {
                    yield return KnownGeneralStructureProperties.GateLowerEdgeLevel.GetDescription();
                    yield return KnownStructureProperties.GateLowerEdgeLevel;
                    yield return KnownGeneralStructureProperties.CrestWidth.GetDescription();
                    yield return KnownGeneralStructureProperties.CrestLevel.GetDescription();
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
                yield return "water_depth";
            }

            if (Area.ObservationCrossSections.Contains(location))
            {
                yield return "discharge";
                yield return "velocity";
                yield return "water_level";
                yield return "water_depth";
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
            if (feature is IPump)
            {
                return "pumps";
            }
            if (feature is IGate)
            {
                return "gates";
            }
            if (feature is IWeir)
            {
                return "weirs";
            }
            if (Area.ObservationPoints.Contains(feature))
            {
                return "observations";
            }
            if (Area.ObservationCrossSections.Contains(feature))
            {
                return "crosssections";
            }

            return null;
        }

        #endregion

        #region IHydroModel

        public IHydroRegion Region
        {
            get { return fmRegion; }
        }

        public Type SupportedRegionType
        {
            get { return typeof (HydroArea); }
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

                    //spatialOperations.AddRange(spatialOperation);
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

        #region Control computational timestep loop
        /*
        public override bool InitializeComputationalTimeStep(ref TimeSpan dt)
        {
            var targetTime = ((CurrentTime + TimeStep) - ReferenceTime).TotalSeconds;
            
            var timeStep = FlexibleMeshModelApi.InitializeComputationalTimeStep(targetTime, dt.TotalSeconds);
            dt = TimeSpan.FromTicks((long)(timeStep*TimeSpan.TicksPerSecond));

            return true;
        }

        public override bool RunComputationalTimeStep(ref TimeSpan dt)
        {
            var dtOld = dt;
            var timeStep = dt.TotalSeconds;
            timeStep = FlexibleMeshModelApi.RunComputationalTimeStep(timeStep);

            FlexibleMeshModelApi.Compute1d2dCoefficients();
            dt = TimeSpan.FromTicks((long)(timeStep * TimeSpan.TicksPerSecond));
            return (timeStep == dtOld.TotalSeconds);
        }
        */


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

        public virtual string GetItemString(IDataItem dataItem)
        {
            var feature = GetFeatureCategory(dataItem.GetFeature());

            var dataItemName = dataItem.Name;

            var parameterName = dataItem.GetParameterName();

            var concatNames = new List<string>(new[] { feature, dataItemName, parameterName });

            concatNames.RemoveAll(s => s == null);

            return string.Join("/", concatNames);
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
            var hasMapFileStore = OutputMapFileStore != null;
            var hasHisFileStore = OutputHisFileStore != null;
            if (hasMapFileStore || hasHisFileStore)
            {
                BeginEdit(new DefaultEditAction("Disconnecting from output files"));

                if (hasMapFileStore)
                {
                    OutputMapFileStore.Close();
                    OutputMapFileStore = null;
                    Output1DFileStore?.Close();
                    Output1DFileStore = null;
                }
                if (hasHisFileStore)
                {
                    OutputHisFileStore.Close();
                    OutputHisFileStore = null;
                }
                EndEdit();
            }
        }

        public virtual void ConnectOutput(string outputPath)
        {
            ReconnectOutputFiles(outputPath);
        }

        private void ReadDiaFile()
        {
            var validPath = ExplicitWorkingDirectory ?? Path.GetDirectoryName(OutputSnappedFeaturesPath);
            if (!Directory.Exists(validPath)) return;

            var outputDirectory = Path.Combine(validPath, DirectoryName);
            if (!Directory.Exists(outputDirectory)) return;

            ReportProgressText("Reading dia file");
            var diaFileName = string.Format("{0}.dia", Name);
            var diaFilePath = Path.Combine(outputDirectory, Path.GetDirectoryName(ModelDefinition.RelativeMapFilePath)??string.Empty, diaFileName);
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
            return ValidateBeforeRun ? WaterFlowFmModelValidationExtensions.Validate(this) : null;
        }
        public new virtual ActivityStatus Status
        {
            get { return base.Status; }
            set { base.Status = value; }
        }

        [EditAction]
        public virtual bool RunsInIntegratedModel { get; set; }

        [NoNotifyPropertyChange]
        public new virtual DateTime CurrentTime
        {
            get { return base.CurrentTime; }
            set { base.CurrentTime = value; }
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
                return new[] {grid};
            }

            return !string.IsNullOrEmpty(itemName) ? !string.IsNullOrEmpty(parameter) ? runner.GetVar(string.Format("{0}/{1}/{2}/{3}", Name, category, itemName, parameter)) : runner.GetVar(string.Format("{0}/{1}/{2}", Name, category, itemName)) : runner.GetVar(string.Format("{0}/{1}", Name, category));
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

            var mduPath = Path.Combine(WorkingDirectory, Path.GetFileName(MduFilePath));

            ReportProgressText("Exporting to mdu file");
            ExportTo(mduPath, false);
            
            ReportProgressText("Initializing");
            runner.OnInitialize();
            InitializeRunTimeGridOperationApi();

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
            ReadDiaFile();

            ReportProgressText();
        }


        protected override void OnExecute()
        {
            runner.OnExecute();
        }

        protected override void OnFinish()
        {
            runner.OnFinish();
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

        public void SetModelStateHandlerModelWorkingDirectory(string modelExplicitWorkingDirectory)
        {
            ModelStateHandler.ModelWorkingDirectory = modelExplicitWorkingDirectory;
        }

        private void OnWaterFlowFm1D2DLinkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Link1D2D & e.PropertyName.Equals("Geometry"))
            { 
                //update indexes
                var link = (Link1D2D) sender;
                var firstCoordinate = link.Geometry?.Coordinates.First();
                var lastCoordinate = link.Geometry?.Coordinates.Last();
                link.DiscretisationPointIndex = Links1D2DHelper.FindCalculationPointIndex(firstCoordinate, NetworkDiscretization, link.SnapToleranceUsed);
                link.FaceIndex = Links1D2DHelper.FindCellIndex(lastCoordinate, Grid);
            }
        }
    }
}