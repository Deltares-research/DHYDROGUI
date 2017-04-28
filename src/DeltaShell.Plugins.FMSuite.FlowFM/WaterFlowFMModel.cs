using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BasicModelInterface;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
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
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using SharpMap;
using SharpMap.Api;
using SharpMap.SpatialOperations;
using INotifyCollectionChanged = DelftTools.Utils.Collections.INotifyCollectionChanged;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    [Entity]
    public partial class WaterFlowFMModel : TimeDependentModelBase, IDimrStateAwareModel, IFileBased, IHasCoordinateSystem, IGridOperationApi, IDisposable, IHydroModel, IHydFileModel, IDimrModel, IWaterFlowFMModel
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (WaterFlowFMModel));
        private readonly DimrRunner runner;

        public const string CellsToFeaturesName = "CellsToFeatures";
        public const string UseNetCDFMapFormatPropertyName = "UseNetCDFMapFormat";
        public const string DisableFlowNodeRenumberingPropertyName = "DisableFlowNodeRenumbering";
        public const string GridPropertyName = "Grid";
        private DepthLayerDefinition depthLayerDefinition;
        private WaterFlowFMModelDefinition modelDefinition;
        private bool disposing;

        private readonly Dictionary<IFeature, List<IDataItem>> areaDataItems = new Dictionary<IFeature, List<IDataItem>>();

        public WaterFlowFMModel() : this(null)
        {
            // network
            Network = new HydroNetwork { Name = "Network" };

            // Create empty model definition
            ModelDefinition = new WaterFlowFMModelDefinition();
            ModelDefinition.GetModelProperty(KnownProperties.NetFile).Value = Name + NetFile.FullExtension;

            SynchronizeModelDefinitions();

            Grid = new UnstructuredGrid();
            InitializeUnstructuredGridCoverages();

            AddSpatialDataItems();
            RenameSubFilesIfApplicable();
        }

        /// <summary>
        /// Constructor for existing mdu file
        /// </summary>
        public WaterFlowFMModel(string mduFilePath, ImportProgressChangedDelegate progressChanged = null) : base("FlowFM")
        {
            runner = new DimrRunner(this);
            ImportProgressChanged = progressChanged;

            // set default settings
            SnapVersion = 0;
            ValidateBeforeRun = true;
            UseNetCDFMapFormat = false;
            DisableFlowNodeRenumbering = false;
            TracerDefinitions = new EventedList<string>();
            tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            
            // DELFT3DFM-371: Disable Model Inspection
            // ModelInspection = true;

            var area = new HydroArea();
            AddDataItem(area, DataItemRole.Input, HydroAreaTag);

            ((INotifyCollectionChanged) area).CollectionChanged += HydroAreaCollectionChanged;
            ((INotifyPropertyChange) this).PropertyChanged += (s, e) => { MarkDirty(); };
            ((INotifyCollectionChanged) this).CollectionChanged += (s, e) => { MarkDirty(); };

            // Load mdu model settings
            if (string.IsNullOrEmpty(mduFilePath)) return;
            LoadStateFromMdu(mduFilePath);

            FireImportProgressChanged(this, "Reading spatial operations", 9, TotalImportSteps);
            AddSpatialDataItems();
            ImportSpatialOperationsAfterCreating();
            
        }

        public WaterFlowFMModelDefinition ModelDefinition
        {
            get { return modelDefinition; }
            private set
            {
                if (modelDefinition != null)
                {
                    ((INotifyPropertyChange) (modelDefinition.Properties)).PropertyChanged -= OnModelDefinitionPropertyChanged;
                }

                modelDefinition = value;

                OnModelDefinitionChanged();

                if (modelDefinition != null)
                {
                    ((INotifyPropertyChange) (modelDefinition.Properties)).PropertyChanged += OnModelDefinitionPropertyChanged;
                }
            }
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
                BeginEdit(new DefaultEditAction("Changing depth layer definition"));
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

        public HeatFluxModelType HeatFluxModelType { get; private set; }

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
            get { return (bool) ModelDefinition.GetModelProperty(GuiProperties.UseTemperature).Value; }
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

            var areaDataItem = GetDataItemByTag(HydroAreaTag);
            if (areaDataItem != null)
            {
                ((INotifyCollectionChange) areaDataItem.Value).CollectionChanged += HydroAreaCollectionChanged;
            }
        }

        protected override void OnBeforeDataItemsSet()
        {
            base.OnBeforeDataItemsSet();

            var areaDataItem = GetDataItemByTag(HydroAreaTag);
            if (areaDataItem != null)
            {
                ((INotifyCollectionChange)areaDataItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
            }
        }

        protected override void OnDataItemLinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            // subscribe to newly linked hydro area:
            var areaDataItem = GetDataItemByTag(HydroAreaTag);
            if (Equals(e.Target, areaDataItem) && !e.Relinking)
            {
                ((INotifyCollectionChange)areaDataItem.Value).CollectionChanged += HydroAreaCollectionChanged;
            }

            base.OnDataItemLinked(sender, e);
        }

        protected override void OnDataItemUnlinking(object sender, LinkingUnlinkingEventArgs<IDataItem> e)
        {
            // unsubscribe from area before unlink
            var areaDataItem = GetDataItemByTag(HydroAreaTag);
            if (Equals(e.Target, areaDataItem))
            {
                ((INotifyCollectionChange)areaDataItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
            }

            base.OnDataItemUnlinking(sender, e);
        }

        private void LoadStateFromMdu(string mduFilePath)
        {
            // in case we're reloading into an existing flow model instance..cleanup first
            syncers.ForEach(s => s.Dispose());
            syncers.Clear();

            TracerDefinitions.Clear();
            
            LoadModelFromMdu(this, mduFilePath);

            SynchronizeModelDefinitions();

            FireImportProgressChanged(this, "Reading grid", 4, TotalImportSteps);
            Grid = ReadGridFromNetFile(NetFilePath, UseNetCDFMapFormat) ?? new UnstructuredGrid();

            UnstructuredGridFileHelper.DoIfUgrid(NetFilePath, uGridAdaptor =>
                {
                    bathymetryNoDataValue = uGridAdaptor.uGrid.zCoordinateFillValue;
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
            
            FireImportProgressChanged(this, "Reading model output", 8, TotalImportSteps);

            LoadRestartFile(mduFilePath);
            ReconnectOutputFiles(Path.GetDirectoryName(mduFilePath));
        }

        private void SynchronizeModelDefinitions()
        {
            HeatFluxModelType = ModelDefinition.HeatFluxModel.Type; // sync the heat flux model
            Boundaries = ModelDefinition.Boundaries;
            BoundaryConditionSets = ModelDefinition.BoundaryConditionSets;
            WindFields = ModelDefinition.WindFields;
            Pipes = ModelDefinition.Pipes;
            SourcesAndSinks = ModelDefinition.SourcesAndSinks;

            // read depth layer definition
            DepthLayerDefinition = ModelDefinition.Kmx == 0
                ? new DepthLayerDefinition(DepthLayerType.Single)
                : new DepthLayerDefinition(ModelDefinition.Kmx);

            syncers.Add(new FeatureDataSyncer<Feature2D, BoundaryConditionSet>(Boundaries, BoundaryConditionSets, CreateBoundaryCondition));
            syncers.Add(new FeatureDataSyncer<Feature2D, SourceAndSink>(Pipes, SourcesAndSinks, CreateSourceAndSink));
        }

        private void TracerDefinitionsCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            var name = (string) e.Item;
            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    // sync the initial tracers
                    InitialTracers.Add(CreateUnstructuredGridCellCoverage(name, Grid));
                    break;
                case NotifyCollectionChangeAction.Remove:
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
                                Equals(flowCondition.TracerName, e.Item))
                            {
                                return true;
                            }
                            return false;
                        });
                    }
                    break;
                case NotifyCollectionChangeAction.Replace:
                    // can't rename yet
                    throw new NotImplementedException("Renaming of tracer definitions is not yet supported");
                    break;
                case NotifyCollectionChangeAction.Reset:
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
                    if (flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer &&
                        !TracerDefinitions.Contains(flowCondition.TracerName))
                    {
                        TracerDefinitions.Add(flowCondition.TracerName);
                    }
                }
            }
            foreach (var quantity in ModelDefinition.SpatialOperations.Keys.Except(WaterFlowFMModelDefinition.SpatialDataItemNames))
            {
                if (!TracerDefinitions.Contains(quantity))
                {
                    TracerDefinitions.Add(quantity);
                }
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

                var samplesOperation = (ImportSamplesOperation) spatialOperationList[0];
                var coverage = (UnstructuredGridCoverage) dataItem.Value;
                var xyzFile = new XyzFile().Read(samplesOperation.FilePath).ToList();

                var componentValueCount = coverage.Arguments.Aggregate(0,(totaal, arguments) => totaal == 0 ? arguments.Values.Count : totaal * arguments.Values.Count);

                var valuesToSet = xyzFile.Count != componentValueCount
                    ? new InterpolateOperation().InterpolateToGrid(xyzFile, coverage, coverage.Grid)
                    : xyzFile.Select(p => p.Value);

                if(valuesToSet.Any())
                    coverage.SetValues(valuesToSet);
            }
        }

        private void ImportSpatialOperationsAfterCreating()
        {
            foreach (var spatialOperation in ModelDefinition.SpatialOperations)
            {
                var dataItemName = spatialOperation.Key;
                var spatialOperationList = spatialOperation.Value;
                var dataItem = DataItems.FirstOrDefault(di => di.Name == dataItemName);

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
        
        private void AddSpatialDataItems()
        {
            AddOrRenameDataItem(Bathymetry, WaterFlowFMModelDefinition.BathymetryDataItemName);
            AddOrRenameDataItem(InitialWaterLevel, WaterFlowFMModelDefinition.InitialWaterLevelDataItemName);
            AddOrRenameDataItem(Roughness, WaterFlowFMModelDefinition.RoughnessDataItemName);
            AddOrRenameDataItem(Viscosity, WaterFlowFMModelDefinition.ViscosityDataItemName);
            AddOrRenameDataItem(Diffusivity, WaterFlowFMModelDefinition.DiffusivityDataItemName);
            AddOrRenameDataItem(InitialTemperature, WaterFlowFMModelDefinition.InitialTemperatureDataItemName);
            AddOrRenameDataItems(InitialSalinity, WaterFlowFMModelDefinition.InitialSalinityDataItemName);
            AddOrRenameTracerDataItems();
        }

        private void AddOrRenameTracerDataItems()
        {
            foreach (var initialTracer in InitialTracers)
            {
                AddOrRenameDataItem(initialTracer, initialTracer.Name);
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
            get { return base.AllDataItems.Concat(areaDataItems.Values.SelectMany(v => v)); }
        }

        private void OnModelDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var prop = (WaterFlowFMProperty) sender;
            if (e.PropertyName == TypeUtils.GetMemberName(() => prop.Value))
            {
                if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.UseSalinity,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching salinity process"));
                    UseSalinity = UseSalinity;
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
                    StringComparison.InvariantCultureIgnoreCase) ||
                         prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.UseTemperature,
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
        }

        public void Dispose()
        {
            disposing = true;
            // also disposes grid snap api, so if you remove this, at least make sure you dispose that one (holds remote instance in the air):
            Grid = null;
            DisposeSnapApi();
            syncers.ForEach(s => s.Dispose());
            syncers.Clear();
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
                       
            yield return InitialSalinity;
            yield return Viscosity;
            yield return Diffusivity;
            yield return Roughness;
            yield return InitialWaterLevel;
            yield return InitialTemperature;
            yield return InitialTracers;

            // for QueryTimeSeries tool:
            if (OutputHisFileStore != null)
                foreach (var featureCoverage in OutputHisFileStore.Functions)
                    yield return featureCoverage;

            if (OutputMapFileStore != null)
                foreach (var function in OutputMapFileStore.Functions)
                    yield return function;
        }

        public override IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
        {
            if ((role & DataItemRole.Input) == DataItemRole.Input)
            {
                return InputFeatureCollections.OfType<IList>().SelectMany(l => l.OfType<IFeature>());
            }

            if ((role & DataItemRole.Output) == DataItemRole.Output)
            {
                return OutputFeatureCollections.OfType<IList>().SelectMany(l => l.OfType<IFeature>());
            }

            return Enumerable.Empty<IFeature>();
        }

        public override IEnumerable<IDataItem> GetChildDataItems(IFeature location)
        {
            if (location == null) yield break;

            List<IDataItem> items;
            areaDataItems.TryGetValue(location, out items);

            if (items == null) yield break;

            foreach (var di in items)
            {
                yield return di;
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
            }
        }

        [NoNotifyPropertyChange]
        public override DateTime StopTime
        {
            get { return (DateTime)ModelDefinition.GetModelProperty(GuiProperties.StopTime).Value; }
            set
            {
                ModelDefinition.GetModelProperty(GuiProperties.StopTime).Value = value;
            }
        }

        public override TimeSpan TimeStep
        {
            get { return (TimeSpan) ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value; }
            set { ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value = value; }
        }
        private IList<ExplicitValueConverterLookupItem> explicitValueConverterLookupItems;

        public bool UseLocalApi { get; set; }
        
        // Do not remove...used in HydroModelBuilder.py
        public void SetWaveForcing()
        {
            ModelDefinition.GetModelProperty(KnownProperties.WaveModelNr).SetValueAsString("3");
        }
        
        protected override void OnInputCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
        }

        // [TOOLS-22813] Override OnInputPropertyChanged to stop base class (ModelBase) from clearing the output
        protected override void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        protected override void OnClearOutput()
        {
            if (OutputMapFileStore != null)
            {
                OutputMapFileStore.Functions.Clear();
                OutputMapFileStore.Close();
                OutputMapFileStore = null;
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
            get {return (HydroArea) GetDataItemValueByTag(HydroAreaTag); }
            set
            {
                var areaItem = GetDataItemByTag(HydroAreaTag);
                
                if (areaItem.Value != null) 
                {
                    ((INotifyCollectionChanged)areaItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
                }

                areaItem.Value = value;

                if (value != null)
                {
                    ((INotifyCollectionChanged)value).CollectionChanged += HydroAreaCollectionChanged;
                }
            }
        }

        public IEventedList<Feature2D> Boundaries { get; private set; }
        public IEventedList<BoundaryConditionSet> BoundaryConditionSets { get; private set; }
        public IEventedList<Feature2D> Pipes { get; private set; }
        public IEventedList<SourceAndSink> SourcesAndSinks { get; private set; }

        public IEnumerable<IBoundaryCondition> BoundaryConditions
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

        private void SpatialDataLayersChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (Equals(sender, InitialSalinity.Coverages))
            {
                AddOrRenameDataItems(InitialSalinity, WaterFlowFMModelDefinition.InitialSalinityDataItemName);
            }
            else
            {
                throw new ArgumentException("Unexpected layered spatial data: " + e.Item);
            }
        }

        private void SpatialDataTracersChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (Equals(sender, InitialTracers))
            {
                AddOrRenameTracerDataItems();
            }
            else
            {
                throw new ArgumentException("Unexpected layered spatial data: " + e.Item);
            }
        }

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
                        String.IsNullOrEmpty(propertyValue)) //skip default (empty) paths
                    {
                        continue;
                    }
                    var currentFileName = Path.GetFileName(propertyValue);
                    if (modelDefinitionName == null ||
                        (modelDefinitionName + pair.Value).Equals(currentFileName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        propertyValue = Name + pair.Value;
                    }
                    yield return new KeyValuePair<WaterFlowFMProperty, string>(property, propertyValue);
                }
            }
        }

        public virtual string MduFilePath { get; protected set; }

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

        private string MapFilePath
        {
            get
            {
                return !String.IsNullOrEmpty(MduFilePath)
                    ? Path.Combine(Path.GetDirectoryName(MduFilePath), ModelDefinition.RelativeMapFilePath)
                    : null;
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
        
        private static void LoadModelFromMdu(WaterFlowFMModel model, string mduFilePath)
        {
            model.MduFilePath = mduFilePath;
            var mduFileDir = Path.GetDirectoryName(mduFilePath);
            model.Name = Path.GetFileNameWithoutExtension(mduFilePath);
            model.ModelDefinition = new WaterFlowFMModelDefinition(mduFileDir, model.Name);
            
            // intialize model definition from mdu file if it exists
            if (File.Exists(mduFilePath))
            {
                model.mduFile.Read(mduFilePath, model.ModelDefinition, model.Area, (name, current, total) => FireImportProgressChanged(model, "Reading mdu - " + name, current, total));
            }

            var netFileProperty = model.ModelDefinition.GetModelProperty(KnownProperties.NetFile);
            if (String.IsNullOrEmpty(netFileProperty.GetValueAsString()))
            {
                netFileProperty.Value = model.Name + NetFile.FullExtension;
            }

            FireImportProgressChanged(model, "Loading restart", 2, TotalImportSteps);
            model.LoadRestartInfo(mduFilePath);

            // sync the heat flux model, because events are off during reading
            model.HeatFluxModelType = model.ModelDefinition.HeatFluxModel.Type;
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
                ModelDefinition.SelectSpatialOperations(DataItems, TracerDefinitions);
                ModelDefinition.Bathymetry = Bathymetry;
            }

            mduFile.Write(mduPath, ModelDefinition, Area, switchTo, writeExtForcings, writeFeatures, UseNetCDFMapFormat, DisableFlowNodeRenumbering);

            if (switchTo)
            {
                MduFilePath = mduPath;
                SaveOutput();
            }
            return true;
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
        }

        private void OnLoad(string mduPath)
        {
            LoadStateFromMdu(mduPath);
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
            // dsproj_data/<model name>/<model name>.mdu
            return Path.Combine(Path.GetDirectoryName(path), Path.Combine(Name, Name + ".mdu"));
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
        private const string HydroAreaTag = "hydro_area_tag";
        private FMMapFileFunctionStore outputMapFileStore;
        private IEventedList<string> tracerDefinitions;

        private const int TotalImportSteps = 10;

        #endregion

        #region Output

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
                var cs = UnstructuredGridFileHelper.GetCoordinateSystem(mapFilePath);

                // update map file coordinate system:
                if (CoordinateSystem != null && cs != CoordinateSystem)
                    NetFile.WriteCoordinateSystem(mapFilePath, CoordinateSystem);
                if (switchTo && OutputMapFileStore != null)
                {
                    OutputMapFileStore.Path = mapFilePath;
                }
                else
                {
                    OutputMapFileStore = new FMMapFileFunctionStore(this);
                    // don't change this to a property setter, because the timing is of great importance.
                    // elsewise, there will be no subscription to the read and Path triggers the Read().
                    OutputMapFileStore.Path = mapFilePath;
                }
            }

            if (existsHisFile)
            {
                FireImportProgressChanged(this, "Reading output files - Reading His file", 1, 2);
                if (switchTo && OutputHisFileStore != null)
                {

                    OutputHisFileStore.Path = hisFilePath;
                }
                else
                {
                    OutputHisFileStore = new FMHisFileFunctionStore(hisFilePath, CoordinateSystem,
                        Area.ObservationPoints,
                        Area.ObservationCrossSections);
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

        private void HydroAreaCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            var inputSender = InputFeatureCollections.Contains(sender);
            var outputSender = OutputFeatureCollections.Contains(sender);

            if (inputSender || outputSender)
            {
                var feature = (IFeature) e.Item;
                var oldFeature = (IFeature) e.OldItem;
                switch (e.Action)
                {
                    case NotifyCollectionChangeAction.Add:
                        AddAreaItem(feature, inputSender);
                        break;
                    case NotifyCollectionChangeAction.Remove:
                        RemoveAreaFeature(feature);
                        break;
                    case NotifyCollectionChangeAction.Reset:
                        foreach (var areaDataItem in areaDataItems)
                        {
                            RemoveAreaFeature(areaDataItem.Key);
                        }
                        areaDataItems.Clear();
                        break;
                        case NotifyCollectionChangeAction.Replace:
                        RemoveAreaFeature(oldFeature);
                        AddAreaItem(feature, inputSender);
                        break;
                    default:
                        throw new NotImplementedException(
                            String.Format("Action {0} on feature collection not supported", e.Action));
                }
            }
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
            var listToAdd = new List<IDataItem>();
            areaDataItems.Add(feature, listToAdd);

            listToAdd.AddRange(
                GetQuantitiesForLocation(feature).Select(quantity => new DataItem(feature)
                {
                    Name = feature.ToString(),
                    Tag = quantity,
                    Role = isInputSender ? DataItemRole.Input : DataItemRole.Output,
                    ValueType = typeof (double),
                    ValueConverter =
                        new WaterFlowFMFeatureValueConverter(this, feature, quantity, String.Empty) // TODO: insert unit
                }));
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
            if (runner.Api == null)
            {
                return Double.NaN;
            }

            string featureCategory = GetFeatureCategory(feature);
            if (featureCategory == null)
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
            get { return Area; }
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
            get { return Path.GetFileName(MduFilePath); }
        }

        public virtual string DirectoryName
        {
            get { return "dflowfm"; }
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
            get
            {
                return Path.GetDirectoryName(FlexibleMeshModelDll.DllPath);
            }
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
            ReconnectOutputFiles(Path.Combine(outputPath, DirectoryName));
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
        public virtual bool IsRunByDimr { get; set; }

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
            if (category == UseNetCDFMapFormatPropertyName)
            {
                var boolArray = values as bool[];
                if (boolArray != null && boolArray.Length > 0)
                    UseNetCDFMapFormat = boolArray[0];
            }
            if (category == DisableFlowNodeRenumberingPropertyName)
            {
                var boolArray = values as bool[];
                if (boolArray != null && boolArray.Length > 0)
                    DisableFlowNodeRenumbering = boolArray[0];
            }
            runner.SetVar(string.Format("{0}/{1}/{2}/{3}", Name, category, itemName, parameter), values);
        }
        #endregion

        #region TimeDependentModelBase

        protected override void OnInitialize()
        {
            var mduPath = Path.Combine(WorkingDirectory, Path.GetFileName(MduFilePath));
            ExportTo(mduPath, false);
            InitializeRunTimeGridOperationApi();
            runner.OnInitialize();
        }
        
        protected override void OnCleanup()
        {
            if (runTimeGridOperationApi != null)
            {
                runTimeGridOperationApi.Dispose();
                runTimeGridOperationApi = null;
            }
            snapApiInErrorMode = false;
            base.OnCleanup();
            runner.OnCleanup();
        }
        protected override void OnProgressChanged()
        {
            runner.OnProgressChanged();
            base.OnProgressChanged();
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

        public void SetModelStateHandlerModelWorkingDirectory(string modelExplicitWorkingDirectory)
        {
            ModelStateHandler.ModelWorkingDirectory = modelExplicitWorkingDirectory;
        }
    }
}