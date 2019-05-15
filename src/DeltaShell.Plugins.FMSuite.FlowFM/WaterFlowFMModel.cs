using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
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
using DeltaShell.Plugins.FMSuite.Common.IO;
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
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using SharpMap;
using SharpMap.Api;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    [Entity]
    public partial class WaterFlowFMModel : TimeDependentModelBase, IDimrStateAwareModel, IFileBased,
                                            IHasCoordinateSystem, IGridOperationApi, IDisposable, IHydroModel,
                                            IHydFileModel, IDimrModel, IWaterFlowFMModel, ISedimentModelData
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowFMModel));
        private readonly DimrRunner runner;

        public const string CellsToFeaturesName = "CellsToFeatures";
        public const string DiaFileDataItemTag = "DiaFile";

        public const string IsPartOf1D2DModelPropertyName = "IsPartOf1D2DModel";
        public const string DisableFlowNodeRenumberingPropertyName = "DisableFlowNodeRenumbering";
        public const string GridPropertyName = "Grid";
        private DepthLayerDefinition depthLayerDefinition;
        private WaterFlowFMModelDefinition modelDefinition;
        private bool disposing;
        private bool updatingGroupName;

        private Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>> fixedWeirProperties =
            new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

        /// <summary>
        /// Gets the bridge pillars data model.
        /// </summary>
        /// <value>
        /// The bridge pillars data model.
        /// </value>
        public IList<ModelFeatureCoordinateData<BridgePillar>> BridgePillarsDataModel { get; private set; }

        private IEventedList<SourceAndSink> sourcesAndSinks;
        private IEventedList<ISedimentFraction> sedimentFractions;
        private IEventedList<BoundaryConditionSet> boundaryConditionSets;
        private IDataItem areaDataItem;

        private readonly Dictionary<IFeature, List<IDataItem>> areaDataItems =
            new Dictionary<IFeature, List<IDataItem>>();

        private double previousProgress = 0;
        private string progressText;

        public WaterFlowFMModel() : this(null)
        {
            // Create model definition
            ModelDefinition = new WaterFlowFMModelDefinition();
            ModelDefinition.GetModelProperty(KnownProperties.NetFile).Value = Name + NetFile.FullExtension;
            ModelDefinition.GetModelProperty(GuiProperties.PartOf1D2DModel).Value = false;

            SynchronizeModelDefinitions();

            Grid = new UnstructuredGrid();
            InitializeUnstructuredGridCoverages();

            AddSpatialDataItems();
            RenameSubFilesIfApplicable();
        }

        /// <summary>
        /// Constructor for existing mdu file
        /// </summary>
        public WaterFlowFMModel(string mduFilePath, ImportProgressChangedDelegate progressChanged = null) :
            base("FlowFM")
        {
            runner = new DimrRunner(this);
            ImportProgressChanged = progressChanged;

            //Create Sediment mode data item
            SedimentModelDataItem = new SedimentModelDataItem();

            // set default settings
            SnapVersion = 0;
            ValidateBeforeRun = true;
            DisableFlowNodeRenumbering = false;
            TracerDefinitions = new EventedList<string>();
            SedimentFractions = new EventedList<ISedimentFraction>();

            BridgePillarsDataModel = new List<ModelFeatureCoordinateData<BridgePillar>>();

            SedimentOverallProperties = SedimentFractionHelper.GetSedimentationOverAllProperties();

            // DELFT3DFM-371: Disable Model Inspection
            // ModelInspection = true;

            var area = new HydroArea();
            AddDataItem(area, DataItemRole.Input, HydroAreaTag);
            areaDataItem = GetDataItemByTag(HydroAreaTag);

            ((INotifyCollectionChanged) area).CollectionChanged += HydroAreaCollectionChanged;
            ((INotifyPropertyChanged) area).PropertyChanged += HydroAreaPropertyChanged;
            ((INotifyPropertyChange) this).PropertyChanged += (s, e) => { MarkDirty(); };
            ((INotifyCollectionChanged) this).CollectionChanged += (s, e) => { MarkDirty(); };

            // Load mdu model settings
            if (string.IsNullOrEmpty(mduFilePath))
            {
                return;
            }

            LoadStateFromMdu(mduFilePath);

            FireImportProgressChanged(this, "Reading spatial operations", 9, TotalImportSteps);
            AddSpatialDataItems();
            ImportSpatialOperationsAfterCreating();
        }

        /// <summary> Import the WaterFlowFMModel described by the specified mdu file path. </summary>
        /// <param name="mduFilePath"> The mdu file path. </param>
        /// <param name="progressChanged"> The progressChanged delegate provided by the importer. </param>
        /// <returns>
        /// A WaterFlowFMModel describing the mdu defined at the <paramref name="mduFilePath" />
        /// </returns>
        public static WaterFlowFMModel Import(string mduFilePath, ImportProgressChangedDelegate progressChanged)
        {
            var model = new WaterFlowFMModel(mduFilePath, progressChanged)
            {
                ImportProgressChanged = null,
            };
            model.ClearOutputDirAndWaqDirProperty();

            return model;
        }

        public WaterFlowFMModelDefinition ModelDefinition
        {
            get => modelDefinition;
            private set
            {
                if (modelDefinition != null)
                {
                    ((INotifyPropertyChange) modelDefinition.Properties).PropertyChanged -=
                        OnModelDefinitionPropertyChanged;
                }

                modelDefinition = value;

                OnModelDefinitionChanged();

                if (modelDefinition != null)
                {
                    ((INotifyPropertyChange) modelDefinition.Properties).PropertyChanged +=
                        OnModelDefinitionPropertyChanged;
                }
            }
        }

        public DepthLayerDefinition DepthLayerDefinition
        {
            get => depthLayerDefinition;
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
            get => (DateTime) ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value;
            set => ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value = value;
        }

        private int CdType
        {
            get => Convert.ToInt32(ModelDefinition.GetModelProperty(KnownProperties.ICdtyp).Value);
            set {}
        }

        public IEventedList<IWindField> WindFields { get; private set; }
        public IList<IUnsupportedFileBasedExtForceFileItem> UnsupportedFileBasedExtForceFileItems { get; private set; }

        public HeatFluxModelType HeatFluxModelType { get; private set; }

        public IEnumerable<ModelFeatureCoordinateData<FixedWeir>> FixedWeirsProperties => fixedWeirProperties.Values;

        public bool UseDepthLayers
        {
            get => ModelDefinition.Kmx != 0;
            private set
            {
                // just sending an event
            }
        }

        public bool UseSalinity
        {
            get => (bool) ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool UseSecondaryFlow
        {
            get => (bool) ModelDefinition.GetModelProperty(KnownProperties.SecondaryFlow).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool UseTemperature =>
            (HeatFluxModelType) ModelDefinition.GetModelProperty(KnownProperties.Temperature).Value !=
            HeatFluxModelType.None;

        public bool UseMorSed
        {
            get => ModelDefinition.UseMorphologySediment;
            private set
            {
                // empty, but just used for event bubbling                
            }
        }

        public bool WriteSnappedFeatures
        {
            get => ModelDefinition.WriteSnappedFeatures;
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

        private void LoadStateFromMdu(string mduFilePath)
        {
            // in case we're reloading into an existing flow model instance..cleanup first
            syncers.ForEach(s => s.Dispose());
            syncers.Clear();

            TracerDefinitions.Clear();

            LoadModelFromMdu(mduFilePath);

            SynchronizeModelDefinitions();

            // import SedimentFractions (these are not part of the model definition, however they are needed for SourcesAndSinks and TracerDefinitions)
            string mduFileDir = Path.GetDirectoryName(mduFilePath);
            WaterFlowFMProperty sedimentFileProperty =
                ModelDefinition.Properties.FirstOrDefault(
                    p => p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.SedFile));
            if (mduFileDir != null && sedimentFileProperty != null && UseMorSed &&
                File.Exists(Path.Combine(mduFileDir, sedimentFileProperty.Value.ToString())))
            {
                SedimentFile.LoadSediments(SedFilePath, this);
            }

            FireImportProgressChanged(this, "Reading grid", 4, TotalImportSteps);
            var is1D2DModel = (bool) ModelDefinition.GetModelProperty(GuiProperties.PartOf1D2DModel).Value;
            Grid = ReadGridFromNetFile(NetFilePath, is1D2DModel) ?? new UnstructuredGrid();

            UnstructuredGridFileHelper.DoIfUgrid(NetFilePath,
                                                 uGridAdaptor =>
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
                    {SourceAndSink.SecondaryFlowVariableName, UseSecondaryFlow}
                };

                sourceAndSink.SedimentFractionNames.ForEach(sfn => componentSettings.Add(sfn, UseMorSed));
                sourceAndSink.TracerNames.ForEach(tn => componentSettings.Add(tn, true));
                sourceAndSink.PopulateFunctionValuesFromAttributes(componentSettings);
            });

            FireImportProgressChanged(this, "Reading model output", 8, TotalImportSteps);

            LoadRestartFile(mduFilePath);

            currentOutputDirectoryPath = PersistentOutputDirectoryPath;

            if (ModelDefinition.ContainsProperty(KnownProperties.OutputDir))
            {
                string mduOutputDir =
                    ModelDefinition.GetModelProperty(KnownProperties.OutputDir).GetValueAsString()?.Trim();

                if (!string.IsNullOrEmpty(mduOutputDir))
                {
                    // We currently assume all OutputDirectoryNames are relative.
                    string mduOutputDirPath = Path.Combine(mduFileDir, mduOutputDir);
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

            ReconnectOutputFiles(existingOutputDirectory);
        }

        private void SynchronizeModelDefinitions()
        {
            HeatFluxModelType = ModelDefinition.HeatFluxModel.Type; // sync the heat flux model
            Boundaries = ModelDefinition.Boundaries;
            BoundaryConditionSets = ModelDefinition.BoundaryConditionSets;
            WindFields = ModelDefinition.WindFields;
            UnsupportedFileBasedExtForceFileItems = ModelDefinition.UnsupportedFileBasedExtForceFileItems;
            Pipes = ModelDefinition.Pipes;
            SourcesAndSinks = ModelDefinition.SourcesAndSinks;

            // read depth layer definition
            DepthLayerDefinition = ModelDefinition.Kmx == 0
                                       ? new DepthLayerDefinition(DepthLayerType.Single)
                                       : new DepthLayerDefinition(ModelDefinition.Kmx);

            syncers.Add(
                new FeatureDataSyncer<Feature2D, BoundaryConditionSet>(Boundaries, BoundaryConditionSets,
                                                                       CreateBoundaryCondition));
            syncers.Add(new FeatureDataSyncer<Feature2D, SourceAndSink>(Pipes, SourcesAndSinks, CreateSourceAndSink));
        }

        private void SourcesAndSinksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var sourceAndSink = e.GetRemovedOrAddedItem() as SourceAndSink;

            if (sourceAndSink == null)
            {
                return;
            }

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
                        string tracerName = flowCondition.TracerName;
                        if (!sourceAndSink.TracerNames.Contains(tracerName))
                        {
                            sourceAndSink.TracerNames.Add(tracerName);
                        }
                    }
                });
            });
        }

        private void BoundaryConditionSetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IEnumerable<FlowBoundaryCondition> tracerBoundaryConditions = Enumerable.Empty<FlowBoundaryCondition>();
            ;

            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            var boundaryConditionSet = removedOrAddedItem as BoundaryConditionSet;
            if (boundaryConditionSet == null)
            {
                var flowBoundaryCondition = removedOrAddedItem as FlowBoundaryCondition;
                if (flowBoundaryCondition != null &&
                    flowBoundaryCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                {
                    tracerBoundaryConditions = new List<FlowBoundaryCondition>() {flowBoundaryCondition};
                }
            }
            else
            {
                tracerBoundaryConditions = boundaryConditionSet.BoundaryConditions
                                                               .OfType<FlowBoundaryCondition>()
                                                               .Where(fbc => fbc.FlowQuantity ==
                                                                             FlowBoundaryQuantityType.Tracer);
            }

            foreach (FlowBoundaryCondition tracerBoundaryCondition in tracerBoundaryConditions)
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
            if (BoundaryConditions.OfType<FlowBoundaryCondition>().All(bc => bc.TracerName != name))
            {
                SourcesAndSinks.ForEach(ss => ss.TracerNames.Remove(name));
            }
        }

        private void AddTracerToSourcesAndSink(string name)
        {
            SourcesAndSinks.ForEach(ss =>
            {
                if (!ss.TracerNames.Contains(name))
                {
                    ss.TracerNames.Add(name);
                }
            });
        }

        private void TracerDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            var name = (string) removedOrAddedItem;
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
                    foreach (BoundaryConditionSet set in BoundaryConditionSets)
                    {
                        set.BoundaryConditions.RemoveAllWhere(bc =>
                        {
                            var flowCondition = bc as FlowBoundaryCondition;

                            if (flowCondition != null &&
                                flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer &&
                                Equals(flowCondition.TracerName, removedOrAddedItem))
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
                    foreach (BoundaryConditionSet set in BoundaryConditionSets)
                    {
                        set.BoundaryConditions.RemoveAllWhere(bc =>
                        {
                            var flowCondition = bc as FlowBoundaryCondition;
                            return flowCondition != null &&
                                   flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer;
                        });
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AssembleTracerDefinitions()
        {
            foreach (IBoundaryCondition boundaryCondition in BoundaryConditions)
            {
                var flowCondition = boundaryCondition as FlowBoundaryCondition;
                if (flowCondition != null)
                {
                    if (flowCondition.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                    {
                        if (!TracerDefinitions.Contains(flowCondition.TracerName))
                        {
                            TracerDefinitions.Add(flowCondition.TracerName);
                        }

                        AddTracerToSourcesAndSink(flowCondition.TracerName);
                    }
                }
            }

            IEnumerable<string> sp = SedimentFractions
                                     .SelectMany(sf => sf.GetAllActiveSpatiallyVaryingPropertyNames()).Distinct();
            foreach (string quantity in ModelDefinition
                                        .SpatialOperations.Keys.Except(WaterFlowFMModelDefinition.SpatialDataItemNames)
                                        .Except(sp))
            {
                if (!TracerDefinitions.Contains(quantity))
                {
                    TracerDefinitions.Add(quantity);
                }
            }
        }

        private void AssembleSpatiallyVaryingSedimentProperties()
        {
            List<ISpatiallyVaryingSedimentProperty> spatiallyVaryingSedimentProperties = SedimentFractions
                                                                                         .SelectMany(
                                                                                             f => f.CurrentSedimentType
                                                                                                   .Properties
                                                                                                   .OfType<
                                                                                                       ISpatiallyVaryingSedimentProperty
                                                                                                   >().Where(
                                                                                                       sp => sp
                                                                                                           .IsSpatiallyVarying))
                                                                                         .ToList();
            spatiallyVaryingSedimentProperties.AddRange(SedimentFractions
                                                        .Where(f => f.CurrentFormulaType != null)
                                                        .SelectMany(
                                                            f => f.CurrentFormulaType.Properties
                                                                  .OfType<ISpatiallyVaryingSedimentProperty>()
                                                                  .Where(sp => sp.IsSpatiallyVarying)));
            foreach (ISpatiallyVaryingSedimentProperty spatiallyVaryingSedimentProperty in
                spatiallyVaryingSedimentProperties)
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
                    List<string> activeSpatiallyVarying = sedimentFraction.GetAllActiveSpatiallyVaryingPropertyNames();
                    List<string> spatiallyVarying = sedimentFraction.GetAllSpatiallyVaryingPropertyNames();
                    InitialFractions.RemoveAllWhere(
                        fr => spatiallyVarying.Contains(fr.Name) && !activeSpatiallyVarying.Contains(fr.Name));

                    foreach (string layerName in activeSpatiallyVarying)
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
            if (prop == null)
            {
                return;
            }

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
            if (InitialFractions == null)
            {
                return;
            }

            IDataItem t = DataItems.FirstOrDefault(di => di.Name == spatiallyVaryingName);
            if (t == null)
            {
                UnstructuredGridCellCoverage unstructuredGridCellCoverage =
                    CreateUnstructuredGridCellCoverage(spatiallyVaryingName, Grid);
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
                    if (spOperationSet != null)
                    {
                        spOperationSet.SpatialOperationSet.Execute();
                    }
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
            if (sedimentFraction == null)
            {
                return;
            }

            string name = sedimentFraction.Name;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    sedimentFraction.UpdateSpatiallyVaryingNames();
                    sedimentFraction.CompileAndSetVisibilityAndIfEnabled();
                    sedimentFraction.SetTransportFormulaInCurrentSedimentType();
                    SourcesAndSinks.ForEach(ss => ss.SedimentFractionNames.Add(sedimentFraction.Name));

                    if (InitialFractions == null || BoundaryConditionSets == null)
                    {
                        break;
                    }

                    // sync the initial fractions
                    SyncInitialFractions(sedimentFraction);
                    AddSedimentFractionToFlowBoundaryConditionFunction(name);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // sync the initial fractions
                    List<string> layersToRemove = sedimentFraction.GetAllActiveSpatiallyVaryingPropertyNames();
                    InitialFractions.RemoveAllWhere(ifs => layersToRemove.Contains(ifs.Name));

                    // Remove dataItems for coverages related to Removed Fraction
                    DataItems.RemoveAllWhere(di => di.Value is UnstructuredGridCoverage &&
                                                   layersToRemove.Contains(di.Name));
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
            foreach (BoundaryConditionSet set in BoundaryConditionSets)
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
            foreach (string layerName in sedimentFraction.GetAllActiveSpatiallyVaryingPropertyNames())
            {
                if (InitialFractions.FirstOrDefault(fr => fr.Name.Equals(layerName)) == null)
                {
                    AddToIntialFractions(layerName);
                }
            }
        }

        private void AddSedimentFractionToFlowBoundaryConditionFunction(string name)
        {
            foreach (BoundaryConditionSet set in BoundaryConditionSets)
            {
                foreach (IBoundaryCondition bc in set.BoundaryConditions)
                {
                    var flowCondition = bc as FlowBoundaryCondition;
                    if (flowCondition != null
                        && flowCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport)
                    {
                        foreach (IFunction point in bc.PointData)
                        {
                            flowCondition.AddSedimentFractionToFunction(point, name);
                        }
                    }
                }
            }
        }

        private void RemoveSedimentFractionFromBoundaryConditionSets(string name)
        {
            foreach (BoundaryConditionSet set in BoundaryConditionSets)
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

                foreach (IBoundaryCondition bc in set.BoundaryConditions)
                {
                    var flowCondition = bc as FlowBoundaryCondition;
                    if (flowCondition != null
                        && flowCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport)
                    {
                        foreach (IFunction point in bc.PointData)
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
                        && (flowCondition.SedimentFractionNames == null ||
                            flowCondition.SedimentFractionNames.Count == 0))
                    {
                        return true;
                    }

                    return false;
                });
            }
        }

        public void ImportSpatialOperationsAfterLoading()
        {
            foreach (KeyValuePair<string, IList<ISpatialOperation>> spatialOperation in ModelDefinition
                .SpatialOperations)
            {
                string dataItemName = spatialOperation.Key;
                IList<ISpatialOperation> spatialOperationList = spatialOperation.Value;
                IDataItem dataItem = DataItems.FirstOrDefault(di => di.Name == dataItemName);

                // when only one operation is found and it has the same name as when you would generate it from saving,
                // it will not override the operations found in the database. Assuming that we are loading a dsproj file.
                // Goes wrong when you change the file name of the quantity and you only have one quantity.
                if (spatialOperationList.Count != 1 || !(spatialOperationList[0] is ImportSamplesOperation) ||
                    dataItem == null || dataItem.ValueConverter != null ||
                    !(dataItem.Value is UnstructuredGridCoverage))
                {
                    continue;
                }

                var samplesOperation = (ImportSamplesOperation) spatialOperationList[0];
                var coverage = (UnstructuredGridCoverage) dataItem.Value;
                List<IPointValue> xyzFile = new XyzFile().Read(samplesOperation.FilePath).ToList();

                int componentValueCount =
                    coverage.Arguments.Aggregate(
                        0,
                        (totaal, arguments) =>
                            totaal == 0 ? arguments.Values.Count : totaal * arguments.Values.Count);

                IEnumerable<double> valuesToSet = xyzFile.Count != componentValueCount
                                                      ? new InterpolateOperation().InterpolateToGrid(
                                                          xyzFile, coverage, coverage.Grid)
                                                      : xyzFile.Select(p => p.Value);

                if (valuesToSet.Any())
                {
                    coverage.SetValues(valuesToSet);
                }
            }
        }

        private void ImportSpatialOperationsAfterCreating()
        {
            foreach (KeyValuePair<string, IList<ISpatialOperation>> spatialOperation in ModelDefinition
                .SpatialOperations)
            {
                string dataItemName = spatialOperation.Key;
                IList<ISpatialOperation> spatialOperationList = spatialOperation.Value;
                IDataItem dataItem = DataItems.FirstOrDefault(di => di.Name == dataItemName);

                if (!spatialOperationList.Any())
                {
                    continue;
                }

                if (dataItem == null)
                {
                    Log.Error("No data item found with name " + dataItemName);
                    continue;
                }

                if (dataItem.ValueConverter == null)
                {
                    dataItem.ValueConverter =
                        SpatialOperationValueConverterFactory.Create(dataItem.Value, dataItem.ValueType);
                }

                var valueConverter = dataItem.ValueConverter as SpatialOperationSetValueConverter;
                if (valueConverter == null)
                {
                    continue;
                }

                valueConverter.SpatialOperationSet.Operations.Clear();

                foreach (ISpatialOperation operation in spatialOperationList)
                {
                    // samples should directly be applied to the coverage with an interpolate operation
                    var importSamplesSpatialOperationExtension = operation as ImportSamplesSpatialOperationExtension;
                    if (importSamplesSpatialOperationExtension != null)
                    {
                        DelftTools.Utils.Tuple<ImportSamplesOperation, InterpolateOperation> operations =
                            importSamplesSpatialOperationExtension.CreateOperations();
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

            // Backwards compatibility
            // BedLevel dataitem value used to be exclusively UnstructuredGridVertexCoverages, now it needs to be more generic
            IDataItem bedLevelDataItem =
                DataItems.FirstOrDefault(di => di.Name == WaterFlowFMModelDefinition.BathymetryDataItemName);
            if (bedLevelDataItem != null)
            {
                bedLevelDataItem.ValueType = typeof(UnstructuredGridCoverage);
            }

            AddOrRenameDataItem(InitialWaterLevel, WaterFlowFMModelDefinition.InitialWaterLevelDataItemName);
            AddOrRenameDataItem(Roughness, WaterFlowFMModelDefinition.RoughnessDataItemName);
            AddOrRenameDataItem(Viscosity, WaterFlowFMModelDefinition.ViscosityDataItemName);
            AddOrRenameDataItem(Diffusivity, WaterFlowFMModelDefinition.DiffusivityDataItemName);
            AddOrRenameDataItem(InitialTemperature, WaterFlowFMModelDefinition.InitialTemperatureDataItemName);
            AddOrRenameDataItems(InitialSalinity, WaterFlowFMModelDefinition.InitialSalinityDataItemName);
            AddOrRenameTracerDataItems();
            AddOrRenameFractionDataItems();
        }

        private void AddOrRenameTracerDataItems()
        {
            foreach (UnstructuredGridCellCoverage initialTracer in InitialTracers)
            {
                AddOrRenameDataItem(initialTracer, initialTracer.Name);
            }
        }

        private void AddOrRenameFractionDataItems()
        {
            foreach (UnstructuredGridCellCoverage initialFraction in InitialFractions)
            {
                AddOrRenameDataItem(initialFraction, initialFraction.Name);
            }
        }

        private void AddOrRenameDataItem(ICoverage coverage, string name)
        {
            IDataItem existingDataItem = GetDataItemByValue(coverage);
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

        private void OnModelDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var prop = (WaterFlowFMProperty) sender;
            if (e.PropertyName == TypeUtils.GetMemberName(() => prop.Value))
            {
                if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.FixedWeirScheme,
                                                                   StringComparison.InvariantCultureIgnoreCase))
                {
                    fixedWeirProperties.Values.ForEach(p => p.UpdateDataColumns(prop.GetValueAsString()));
                }

                if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.BedlevType,
                                                                   StringComparison.InvariantCultureIgnoreCase))
                {
                    var bedLevelType = (UnstructuredGridFileHelper.BedLevelLocation) prop.Value;
                    BeginEdit(new DefaultEditAction("Updating Bathymetry coverage"));
                    UpdateBathymetryCoverage(bedLevelType);
                    EndEdit();
                }

                if (prop.PropertyDefinition.MduPropertyName.Equals(KnownProperties.UseSalinity,
                                                                   StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching salinity process"));
                    UseSalinity = UseSalinity;
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
                else if (prop.PropertyDefinition.MduPropertyName.Equals(GuiProperties.WriteClassMapFile,
                                                                        StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching WriteClassMapFile"));
                    WriteClassMapFile = WriteClassMapFile;
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

            fixedWeirProperties.Values.ForEach(d => d.Dispose());
            fixedWeirProperties.Clear();
            BridgePillarsDataModel.ForEach(d => d.Dispose());
        }

        #region TimedependentModelBase

        private IList<ExplicitValueConverterLookupItem> explicitValueConverterLookupItems;

        // Do not remove...used in HydroModelBuilder.py
        public void SetWaveForcing()
        {
            ModelDefinition.GetModelProperty(KnownProperties.WaveModelNr).SetValueAsString("3");
        }

        private void ClearFunctionStore(ReadOnlyNetCdfFunctionStoreBase functionStore)
        {
            functionStore.Functions.Clear();
            functionStore.Close();
        }

        private bool HasOpenFunctionStores =>
            OutputMapFileStore != null || OutputHisFileStore != null || OutputClassMapFileStore != null;

        public event EventHandler AfterExecute;

        #endregion

        #region IHasCoordinateSystem

        public ICoordinateSystem CoordinateSystem
        {
            get => ModelDefinition.CoordinateSystem;
            set
            {
                if (Equals(ModelDefinition.CoordinateSystem, value))
                {
                    return;
                }

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
            get
            {
                if (areaDataItem == null)
                {
                    areaDataItem = GetDataItemByTag(HydroAreaTag);
                }

                return (HydroArea) GetDataItemValueByTag(HydroAreaTag);
            }
            set
            {
                IDataItem areaItem = GetDataItemByTag(HydroAreaTag);

                if (areaItem.Value != null)
                {
                    ((INotifyCollectionChanged) areaItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
                    ((INotifyPropertyChanged) value).PropertyChanged -= HydroAreaPropertyChanged;
                }

                fixedWeirProperties.Clear();

                BridgePillarsDataModel.Clear();

                areaItem.Value = value;

                if (value != null)
                {
                    value.FixedWeirs.ForEach(
                        fw => fixedWeirProperties.Add(fw, CreateModelFeatureCoordinateDataFor(fw)));
                    value.BridgePillars.ForEach(
                        bp => BridgePillarsDataModel.Add(CreateModelFeatureCoordinateDataFor(bp)));

                    ((INotifyCollectionChanged) value).CollectionChanged += HydroAreaCollectionChanged;
                    ((INotifyPropertyChanged) value).PropertyChanged += HydroAreaPropertyChanged;
                }
            }
        }

        public IEventedList<Feature2D> Boundaries { get; private set; }

        public IEventedList<BoundaryConditionSet> BoundaryConditionSets
        {
            get => boundaryConditionSets;
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
            get => sourcesAndSinks;
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

        public IEnumerable<IBoundaryCondition> BoundaryConditions => ModelDefinition.BoundaryConditions;

        private static BoundaryConditionSet CreateBoundaryCondition(Feature2D feature)
        {
            return new BoundaryConditionSet {Feature = feature};
        }

        private static SourceAndSink CreateSourceAndSink(Feature2D feature)
        {
            return new SourceAndSink {Feature = feature};
        }

        #endregion

        public bool HydFileOutput { get; set; } // always on ??

        #region Spatial data

        private void AddOrRenameDataItems(CoverageDepthLayersList coverageDepthLayersList, string name)
        {
            var i = 1;
            bool uniform = coverageDepthLayersList.VerticalProfile.Type == VerticalProfileType.Uniform;

            foreach (ICoverage coverage in coverageDepthLayersList.Coverages)
            {
                string numberedName = uniform ? name : name + "_" + i++;
                AddOrRenameDataItem(coverage, numberedName);
            }
        }

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

        public virtual string MduFilePath { get; protected set; }

        public MduFile MduFile => mduFile;

        public bool WriteHisFile
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.WriteHisFile).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyHisStart
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyHisStart).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyHisStop
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyHisStop).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool WriteMapFile
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyMapStart
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyMapStart).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyMapStop
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyMapStop).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool WriteClassMapFile
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.WriteClassMapFile).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool WriteRstFile
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyRstStart
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyRstStart).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyRstStop
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyRstStop).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyWaqOutputInterval
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyWaqOutputStartTime
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStartTime).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyWaqOutputStopTime
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStopTime).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public object WaveModel
        {
            // cannot actually return anything, because it's a dynamic enum
            get => null;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public ImportProgressChangedDelegate ImportProgressChanged { get; set; }

        private void LoadModelFromMdu(string mduFilePath)
        {
            MduFilePath = mduFilePath;
            string mduFileDir = Path.GetDirectoryName(mduFilePath);
            Name = Path.GetFileNameWithoutExtension(mduFilePath);
            ModelDefinition = new WaterFlowFMModelDefinition(mduFileDir, Name);

            // intialize model definition from mdu file if it exists
            if (File.Exists(mduFilePath))
            {
                isLoading = true;
                mduFile.Read(mduFilePath, ModelDefinition, Area, fixedWeirProperties,
                             (name, current, total) =>
                                 FireImportProgressChanged(this, "Reading mdu - " + name, current, total),
                             BridgePillarsDataModel);
                isLoading = false;
                SyncModelTimesWithBase();
            }

            WaterFlowFMProperty netFileProperty = ModelDefinition.GetModelProperty(KnownProperties.NetFile);
            if (string.IsNullOrEmpty(netFileProperty.GetValueAsString()))
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

        private static void FireImportProgressChanged(WaterFlowFMModel model, string currentStepName, int currentStep,
                                                      int totalSteps)
        {
            if (model.ImportProgressChanged == null)
            {
                return;
            }

            model.ImportProgressChanged(currentStepName, currentStep, totalSteps);
        }

        public virtual bool ExportTo(string mduPath, bool switchTo = true, bool writeExtForcings = true,
                                     bool writeFeatures = true)
        {
            string dirName = Path.GetDirectoryName(mduPath);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            modelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent).SetValueAsString("1");

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
                List<string> spatVarSedPropNames =
                    SedimentFractions.Where(sf => sf.CurrentSedimentType != null).SelectMany(
                                         sf =>
                                             sf.CurrentSedimentType.Properties
                                               .OfType<ISpatiallyVaryingSedimentProperty>()
                                               .Where(p => p.IsSpatiallyVarying)).Select(p => p.SpatiallyVaryingName)
                                     .ToList();
                spatVarSedPropNames.AddRange(SedimentFractions.Where(sf => sf.CurrentFormulaType != null).SelectMany(
                                                                  sf =>
                                                                      sf.CurrentFormulaType.Properties
                                                                        .OfType<ISpatiallyVaryingSedimentProperty>()
                                                                        .Where(p => p.IsSpatiallyVarying))
                                                              .Select(p => p.SpatiallyVaryingName).ToList());
                ModelDefinition.SelectSpatialOperations(DataItems, TracerDefinitions, spatVarSedPropNames);
                ModelDefinition.Bathymetry = Bathymetry;
            }

            InitializeAreaDataColumns();

            if (RunsInIntegratedModel)
            {
                SetOutputDirAndWaqDirProperty();
            }

            var mduFileWriteConfig = new MduFileWriteConfig()
            {
                WriteExtForcings = writeExtForcings,
                WriteFeatures = writeFeatures,
                DisableFlowNodeRenumbering = DisableFlowNodeRenumbering
            };
            mduFile.Write(mduPath,
                          ModelDefinition,
                          Area,
                          fixedWeirProperties.Values,
                          mduFileWriteConfig,
                          switchTo,
                          UseMorSed ? this : null);

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
            const string postfixExplicitWorkingDirectory = "_output";

            string previousModelDir = null;
            string previousExplicitWorkingDirectory = null;
            if (MduFilePath != MduSavePath)
            {
                previousModelDir = RecursivelyGetModelDirectoryPathFromMduFile();
                previousExplicitWorkingDirectory = previousModelDir + postfixExplicitWorkingDirectory;
            }

            if (ExportTo(MduSavePath))
            {
                /*Make sure the ModelDirectory gets updated when saving*/
                ModelDefinition.ModelDirectory = RecursivelyGetModelDirectoryPathFromMduFile();
            }

            if (previousModelDir == null)
            {
                return;
            }

            FileUtils.DeleteIfExists(previousModelDir);
            FileUtils.DeleteIfExists(previousExplicitWorkingDirectory);
        }

        private void RenameSubFilesIfApplicable()
        {
            foreach (KeyValuePair<WaterFlowFMProperty, string> subFile in SubFiles)
            {
                WaterFlowFMProperty waterFlowFMProperty = subFile.Key;

                if (waterFlowFMProperty.GetValueAsString().Equals(subFile.Value))
                {
                    continue;
                }

                if (waterFlowFMProperty.Equals(ModelDefinition.GetModelProperty(KnownProperties.NetFile)))
                {
                    string oldPath = NetFilePath;
                    waterFlowFMProperty.SetValueAsString(subFile.Value);
                    string newPath = NetFilePath;

                    if (!File.Exists(oldPath) ||
                        string.Equals(Path.GetFullPath(oldPath), Path.GetFullPath(newPath),
                                      StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }

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

        private void MarkDirty()
        {
            unchecked
            {
                dirtyCounter++;
            } //unchecked is default, but its here to declare intent
        }

        private int dirtyCounter; //tells NHibernate we need to be saved
        private const string HydroAreaTag = "hydro_area_tag";
        private FMMapFileFunctionStore outputMapFileStore;
        private IEventedList<string> tracerDefinitions;
        private bool isLoading;

        private const int TotalImportSteps = 10;

        #region Output

        public TimeSpan OutputTimeStep
        {
            get => (TimeSpan) ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value;
            set => ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = value;
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
            get => outputMapFileStore;
            protected set => outputMapFileStore = value;
        }

        public virtual FMClassMapFileFunctionStore OutputClassMapFileStore { get; protected set; }

        public virtual FMHisFileFunctionStore OutputHisFileStore { get; protected set; }

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
                    CleanDirectory(WorkingDirectoryPath);
                }

                currentOutputDirectoryPath = PersistentOutputDirectoryPath;

                return;
            }

            if (sourceOutputDirectoryPath == targetOutputDirectoryPath)
            {
                return;
            }

            //copy all files and subdirectories from source directory "output" to persistent directory "output"
            if (!FileUtils.IsDirectoryEmpty(sourceOutputDirectoryPath))
            {
                FileUtils.CreateDirectoryIfNotExists(targetOutputDirectoryPath);

                if (sourceIsWorkingDir)
                {
                    List<string> lockedFiles = GetLockedFiles(WorkingDirectoryPath).ToList();

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

            string waqOutputDir = Path.Combine(PersistentOutputDirectoryPath, DelwaqOutputDirectoryName);
            string snappedOutputDir = Path.Combine(PersistentOutputDirectoryPath, SnappedFeaturesDirectoryName);
            ReconnectOutputFiles(MapFilePath, HisFilePath, ClassMapFilePath, waqOutputDir, snappedOutputDir, true);

            if (sourceIsWorkingDir)
            {
                CleanDirectory(WorkingDirectoryPath);
            }

            currentOutputDirectoryPath = PersistentOutputDirectoryPath;
        }

        /// <summary>
        /// Moves all content in the source directory into the target directory.
        /// </summary>
        /// <param name="sourceDirectory"> The source directory. </param>
        /// <param name="targetDirectoryPath"> The target directory path. </param>
        /// <remarks> <paramref name="sourceDirectory" /> should exist. </remarks>
        private void MoveAllContentDirectory(DirectoryInfo sourceDirectory, string targetDirectoryPath)
        {
            foreach (FileInfo file in sourceDirectory.EnumerateFiles())
            {
                MoveFile(file, targetDirectoryPath);
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

        private static void MoveFile(FileInfo file, string targetDirectoryPath)
        {
            string targetPath = Path.Combine(targetDirectoryPath, file.Name);
            file.MoveTo(targetPath);
        }

        private void MoveDirectory(DirectoryInfo sourceDirectoryInfo, string targetParentDirectoryPath,
                                   bool onSameVolume)
        {
            var targetDirectoryInfo =
                new DirectoryInfo(Path.Combine(targetParentDirectoryPath, sourceDirectoryInfo.Name));

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

        protected virtual void ReconnectOutputFiles(string outputDirectory)
        {
            string mapFilePath = Path.Combine(outputDirectory, ModelDefinition.MapFileName);
            string hisFilePath = Path.Combine(outputDirectory, ModelDefinition.HisFileName);
            string classMapFilePath = Path.Combine(outputDirectory, ModelDefinition.ClassMapFileName);
            string waqFilePath = Path.Combine(outputDirectory, DelwaqOutputDirectoryName);
            string snappedFolderPath = Path.Combine(outputDirectory, SnappedFeaturesDirectoryName);

            ReconnectOutputFiles(mapFilePath, hisFilePath, classMapFilePath, waqFilePath, snappedFolderPath);
        }

        private void ReconnectOutputFiles(string mapFilePath, string hisFilePath, string classMapFilePath,
                                          string waqFolderPath, string snappedFolderPath, bool switchTo = false)
        {
            bool existsMapFile = File.Exists(mapFilePath);
            bool existsHisFile = File.Exists(hisFilePath);
            bool existsClassMapFile = File.Exists(classMapFilePath);
            bool existsWaqFolder = Directory.Exists(waqFolderPath);
            bool existsSnappedFolder = Directory.Exists(snappedFolderPath);

            if (!existsMapFile && !existsHisFile && !existsClassMapFile && !existsWaqFolder && !existsSnappedFolder)
            {
                return;
            }

            FireImportProgressChanged(this, "Reading output files - Reading Map file", 1, 2);
            BeginEdit(new DefaultEditAction("Reconnect output files"));

            // deal with issue that kernel doesn't understand any coordinate systems other than RD & WGS84 :
            if (existsMapFile)
            {
                ReportProgressText("Reading map file");
                ICoordinateSystem cs = UnstructuredGridFileHelper.GetCoordinateSystem(mapFilePath);

                // update map file coordinate system:
                if (CoordinateSystem != null && cs != CoordinateSystem)
                {
                    NetFile.WriteCoordinateSystem(mapFilePath, CoordinateSystem);
                }

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
                                                                    Area.Weirs.Where(
                                                                        w =>
                                                                            w.WeirFormula is
                                                                                GeneralStructureWeirFormula));
                }
            }

            if (existsClassMapFile)
            {
                ReportProgressText("Reading class map file");
                FireImportProgressChanged(this, "Reading output files - Reading Class Map file", 1, 2);
                if (switchTo && OutputClassMapFileStore != null)
                {
                    OutputClassMapFileStore.Path = classMapFilePath;
                }
                else
                {
                    OutputClassMapFileStore = new FMClassMapFileFunctionStore(classMapFilePath);
                }
            }

            if (existsWaqFolder)
            {
                DelwaqOutputDirectoryPath = waqFolderPath;
            }

            if (existsSnappedFolder)
            {
                OutputSnappedFeaturesPath = snappedFolderPath;
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

        private void HydroAreaCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            if (!isLoading)
            {
                var fixedWeir = removedOrAddedItem as FixedWeir;
                if (fixedWeir != null)
                {
                    ModelFeatureCoordinateData<FixedWeir> weirProperties = fixedWeirProperties.ContainsKey(fixedWeir)
                                                                               ? fixedWeirProperties[fixedWeir]
                                                                               : null;

                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (weirProperties == null)
                            {
                                fixedWeirProperties.Add(fixedWeir, CreateModelFeatureCoordinateDataFor(fixedWeir));
                            }

                            break;
                        case NotifyCollectionChangedAction.Remove:
                            if (weirProperties == null)
                            {
                                break;
                            }

                            fixedWeirProperties.Remove(weirProperties.Feature);
                            weirProperties.Dispose();

                            break;
                        case NotifyCollectionChangedAction.Replace:
                            if (weirProperties == null)
                            {
                                fixedWeirProperties.Add(fixedWeir, CreateModelFeatureCoordinateDataFor(fixedWeir));
                                break;
                            }

                            weirProperties.Feature = fixedWeir;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                var bridgePillar = removedOrAddedItem as BridgePillar;
                if (bridgePillar != null)
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            BridgePillarsDataModel.Add(
                                CreateModelFeatureCoordinateDataFor(bridgePillar));
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            ModelFeatureCoordinateData<BridgePillar> dataToRemove =
                                BridgePillarsDataModel.FirstOrDefault(
                                    d => d.Feature == bridgePillar);
                            if (dataToRemove == null)
                            {
                                break;
                            }

                            BridgePillarsDataModel.Remove(dataToRemove);
                            dataToRemove.Dispose();
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            ModelFeatureCoordinateData<BridgePillar> dataToUpdate =
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

            var groupableFeature = removedOrAddedItem as IGroupableFeature;
            if (groupableFeature != null && e.Action != NotifyCollectionChangedAction.Remove && !Area.IsEditing)
            {
                groupableFeature.UpdateGroupName(this);
            }

            bool inputSender = removedOrAddedItem is Pump2D || removedOrAddedItem is Weir2D;
            bool outputSender = removedOrAddedItem is ObservationCrossSection2D ||
                                removedOrAddedItem is GroupableFeature2DPoint;

            if (inputSender || outputSender)
            {
                var feature = (IFeature) removedOrAddedItem;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        AddAreaItem(feature, inputSender);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        RemoveAreaFeature(feature);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        foreach (KeyValuePair<IFeature, List<IDataItem>> areaDataItem in areaDataItems)
                        {
                            RemoveAreaFeature(areaDataItem.Key);
                        }

                        areaDataItems.Clear();
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        var oldFeature = (IFeature) e.OldItems[0];
                        RemoveAreaFeature(oldFeature);
                        AddAreaItem(feature, inputSender);
                        break;
                    default:
                        throw new NotImplementedException(
                            string.Format("Action {0} on feature collection not supported", e.Action));
                }
            }
        }

        private ModelFeatureCoordinateData<FixedWeir> CreateModelFeatureCoordinateDataFor(FixedWeir fixedWeir)
        {
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<FixedWeir> {Feature = fixedWeir};
            string scheme = ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).GetValueAsString();

            modelFeatureCoordinateData.UpdateDataColumns(scheme);
            return modelFeatureCoordinateData;
        }

        private ModelFeatureCoordinateData<BridgePillar> CreateModelFeatureCoordinateDataFor(BridgePillar bridgePillar)
        {
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar> {Feature = bridgePillar};
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
                    bool isInputSender = Area.Weirs.Any(w => w.Name == weir.Name);
                    UpdateAreaDataItems(weir, isInputSender);
                }
            }

            var groupableFeature = sender as IGroupableFeature;
            if (updatingGroupName || Area.IsEditing || groupableFeature == null ||
                e.PropertyName != TypeUtils.GetMemberName<IGroupableFeature>(g => g.GroupName))
            {
                return;
            }

            updatingGroupName = true; // prevent recursive calls

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
                foreach (IDataItem dataItem in dataItemsToBeRemoved)
                {
                    UnSubscribeFromDataItem(dataItem, true);
                    OnDataItemRemoved(dataItem);
                }
            }

            areaDataItems.Remove(feature);
        }

        private void AddAreaItem(IFeature feature, bool isInputSender)
        {
            List<IDataItem> listToAdd = GetDataItemListForFeature(feature, isInputSender);
            areaDataItems.Add(feature, listToAdd);
        }

        private void UpdateAreaDataItems(IFeature feature, bool isInputSender)
        {
            if (areaDataItems.TryGetValue(feature, out List<IDataItem> dataItemsDependentOnThisFeature))
            {
                List<IDataItem> listToReplace = GetDataItemListForFeature(feature, isInputSender);

                List<IDataItem> dataItemsLinkedToRTC =
                    dataItemsDependentOnThisFeature.Where(di => di.LinkedTo != null).ToList();

                foreach (IDataItem dataItem in dataItemsLinkedToRTC)
                {
                    Log.WarnFormat(
                        Resources
                            .WaterFlowFMModel_ChangingWeirFormulaWhenAlsoUsedInRTC_Structure_component__0__has_been_removed_from_RTC_Control_Group__1__due_to_type_change,
                        dataItem.Name + "_" + dataItem.Tag, dataItem.LinkedTo.Parent.Name);

                    OnDataItemRemoved(dataItem);
                }

                areaDataItems[feature] = listToReplace;
            }
        }

        private List<IDataItem> GetDataItemListForFeature(IFeature feature, bool isInputSender)
        {
            IEnumerable<string> quantities = QuantityGenerator.GetQuantitiesForFeature(feature, UseSalinity);
            return quantities.Select(quantity => new DataItem(feature)
            {
                Name = feature.ToString(),
                Tag = quantity,
                Role = isInputSender ? DataItemRole.Input : DataItemRole.Output,
                ValueType = typeof(double),
                ValueConverter =
                    new WaterFlowFMFeatureValueConverter(this, feature, quantity, string.Empty) // TODO: insert unit
            }).OfType<IDataItem>().ToList();
        }

        public double GetValueFromModelApi(IFeature feature, string parameterName)
        {
            string featureCategory = GetFeatureCategory(feature);
            if (featureCategory == null)
            {
                return double.NaN;
            }

            // temporary fix for DELFT3DFM-1302 (this should be done in Dimr)
            if (featureCategory == "weirs" && parameterName == "crest_level")
            {
                var weir = (Weir) feature;
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
                return double.NaN;
            }

            var nameable = feature as INameable;
            if (nameable == null)
            {
                return double.NaN;
            }

            return ((double[]) GetVar(featureCategory, nameable.Name, parameterName))[0];
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
            {
                return;
            }

            SetVar(new[]
            {
                value
            }, featureCategory, nameable.Name, parameterName);
        }

        public virtual string GetFeatureCategory(IFeature feature)
        {
            if (feature is IPump)
            {
                return KnownFeatureCategories.Pumps;
            }

            if (feature is IWeir weir)
            {
                IWeirFormula weirFormula = weir.WeirFormula;
                if (weirFormula is GeneralStructureWeirFormula)
                {
                    return KnownFeatureCategories.GeneralStructures;
                }

                if (weirFormula is GatedWeirFormula)
                {
                    return KnownFeatureCategories.Gates;
                }

                return KnownFeatureCategories.Weirs;
            }

            if (Area.ObservationPoints.Contains(feature))
            {
                return KnownFeatureCategories.Observations;
            }

            if (Area.ObservationCrossSections.Contains(feature))
            {
                return KnownFeatureCategories.CrossSections;
            }

            return null;
        }

        #endregion

        #region IHydroModel

        public IHydroRegion Region => Area;

        public Type SupportedRegionType => typeof(HydroArea);

        public IEventedList<string> TracerDefinitions
        {
            get => tracerDefinitions;
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
            get => sedimentFractions;
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
                    ((INotifyPropertyChanged) SedimentFractions).PropertyChanged += SedimentFractionPropertyChanged;
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

            IDataItem[] dataItemsFound = SedimentModelDataItem
                                         .SpacialVariableNames
                                         .SelectMany(
                                             spaceVarName => DataItems.Where(di => di.Name.Equals(spaceVarName)))
                                         .ToArray();
            List<IDataItem> dataItemsWithConverter =
                dataItemsFound.Where(d => d.ValueConverter is SpatialOperationSetValueConverter).ToList();
            List<IDataItem> dataItemsWithOutConverter = dataItemsFound.Except(dataItemsWithConverter).ToList();

            SedimentModelDataItem.SpatialOperation = GetSpatialOperationsLookupTable(dataItemsWithConverter);
            SedimentModelDataItem.Coverages = dataItemsWithOutConverter.Select(di => di.Value)
                                                                       .OfType<UnstructuredGridCoverage>()
                                                                       .GroupBy(c => c.GetType())
                                                                       .ToList();
            SedimentModelDataItem.DataItemNameLookup =
                dataItemsWithOutConverter.ToDictionary(di => di.Value, di => di.Name);

            return SedimentModelDataItem;
        }

        public Dictionary<string, IList<ISpatialOperation>> GetSpatialOperationsLookupTable(
            List<IDataItem> dataItemsWithConverter)
        {
            var spatialOperationsLookupTable = new Dictionary<string, IList<ISpatialOperation>>();
            foreach (IDataItem dataItem in dataItemsWithConverter)
            {
                var spatialOperationValueConverter = (SpatialOperationSetValueConverter) dataItem.ValueConverter;
                if (
                    spatialOperationValueConverter.SpatialOperationSet.Operations.All(
                        WaterFlowFMModelDefinition.SupportedByExtForceFile))
                {
                    // put in everything except spatial operation sets,
                    // because we only use interpolate commands that will grab the importsamplesoperation via the input parameters.
                    List<ISpatialOperation> spatialOperation = spatialOperationValueConverter
                                                               .SpatialOperationSet.GetOperationsRecursive()
                                                               .Where(s => !(s is ISpatialOperationSet))
                                                               .Select(WaterFlowFMModelDefinition
                                                                           .ConvertSpatialOperation)
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
                    if (coverage == null || coverage.Components[0].NoDataValues != null &&
                        coverage.GetValues<double>()
                                .All(v => coverage.Components[0].NoDataValues.Contains(v)))
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

                    spatialOperationsLookupTable.Add(dataItem.Name, new[]
                    {
                        newOperation
                    });
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

        public virtual string LibraryName => "dflowfm";

        public virtual string InputFile => Path.GetFileName(MduSavePath);

        public virtual string DirectoryName => "dflowfm";

        public virtual bool IsMasterTimeStep => true;

        public virtual string ShortName => "flow";

        public virtual string GetItemString(IDataItem dataItem)
        {
            string feature = GetFeatureCategory(dataItem.GetFeature());

            string dataItemName = dataItem.Name;

            string parameterName = dataItem.GetParameterName();

            var concatNames = new List<string>(new[]
            {
                feature,
                dataItemName,
                parameterName
            });

            concatNames.RemoveAll(s => s == null);

            return string.Join("/", concatNames);
        }

        public virtual IDataItem GetDataItemByItemString(string itemString)
        {
            throw new NotImplementedException();
        }

        public virtual Type ExporterType => typeof(WaterFlowFMFileExporter);

        public virtual string GetExporterPath(string directoryName)
        {
            return Path.Combine(directoryName, InputFile == null ? Name + mduExtension : Path.GetFileName(InputFile));
        }

        public virtual bool CanRunParallel => true;

        public virtual string MpiCommunicatorString => "DFM_COMM_DFMWORLD";

        public virtual string KernelDirectoryLocation => DimrApiDataSet.DFlowFmDllPath;

        /// <summary>
        /// Disconnects the output.
        /// </summary>
        public virtual void DisconnectOutput()
        {
            if (HasOpenFunctionStores)
            {
                BeginEdit(new DefaultEditAction("Disconnecting from output files"));

                if (OutputMapFileStore != null)
                {
                    OutputMapFileStore.Close();
                    OutputMapFileStore = null;
                }

                if (OutputHisFileStore != null)
                {
                    OutputHisFileStore.Close();
                    OutputHisFileStore = null;
                }

                if (OutputClassMapFileStore != null)
                {
                    OutputClassMapFileStore.Close();
                    OutputClassMapFileStore = null;
                }

                EndEdit();
            }

            OutputSnappedFeaturesPath = null;
        }

        public virtual void ConnectOutput(string outputPath)
        {
            currentOutputDirectoryPath = outputPath;
            ReconnectOutputFiles(outputPath);
            ReadDiaFile(outputPath);
            ClearOutputDirAndWaqDirProperty();
        }

        private void ReadDiaFile(string outputDirectory)
        {
            ReportProgressText("Reading dia file");
            string diaFileName = string.Format("{0}.dia", Name);

            string diaFilePath = Path.Combine(outputDirectory, diaFileName);
            if (File.Exists(diaFilePath))
            {
                try
                {
                    IDataItem logDataItem = DataItems.FirstOrDefault(di => di.Tag == DiaFileDataItemTag);
                    if (logDataItem == null)
                    {
                        // add logfile dataitem if not exists
                        var textDocument = new TextDocument(true) {Name = diaFileName};
                        logDataItem = new DataItem(textDocument, DataItemRole.Output, DiaFileDataItemTag);
                        DataItems.Add(logDataItem);
                    }

                    string log = DiaFileReader.Read(diaFilePath);
                    ((TextDocument) logDataItem.Value).Content = log;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat(Resources.WaterFlowFMModel_ReadDiaFile_Error_reading_log_file___0____1_,
                                    diaFileName, ex.Message);
                }
            }
            else
            {
                Log.WarnFormat(
                    Resources.WaterFlowFMModel_ReadDiaFile_Could_not_find_log_file___0__at_expected_path___1_,
                    diaFileName, diaFilePath);
            }
        }

        public virtual ValidationReport Validate()
        {
            return ValidateBeforeRun || Status != ActivityStatus.Initializing
                       ? WaterFlowFmModelValidationExtensions.Validate(this)
                       : new ValidationReport("", new List<ValidationIssue>());
        }

        public new virtual ActivityStatus Status
        {
            get => base.Status;
            set => base.Status = value;
        }

        [EditAction]
        public virtual bool RunsInIntegratedModel { get; set; }

        public virtual string DimrExportDirectoryPath
        {
            get => WorkingDirectoryPath;
            set => WorkingDirectoryPath = value;
        }

        public virtual string DimrModelRelativeWorkingDirectory => Path.Combine(DirectoryName, InputDirectoryName);

        public virtual string DimrModelRelativeOutputDirectory => Path.Combine(DirectoryName, OutputDirectoryName);

        [NoNotifyPropertyChange]
        public new virtual DateTime CurrentTime
        {
            get => base.CurrentTime;
            set => base.CurrentTime = value;
        }

        public virtual Array GetVar(string category, string itemName = null, string parameter = null)
        {
            if (category == CellsToFeaturesName)
            {
                if (OutputMapFileStore != null && OutputMapFileStore.BoundaryCellValues != null)
                {
                    return OutputMapFileStore.BoundaryCellValues.ToArray();
                }

                return null;
            }

            if (category == GridPropertyName)
            {
                return new[]
                {
                    grid
                };
            }

            return !string.IsNullOrEmpty(itemName)
                       ? !string.IsNullOrEmpty(parameter)
                             ? runner.GetVar(string.Format("{0}/{1}/{2}/{3}", Name, category, itemName, parameter))
                             : runner.GetVar(string.Format("{0}/{1}/{2}", Name, category, itemName))
                       : runner.GetVar(string.Format("{0}/{1}", Name, category));
        }

        public virtual void SetVar(Array values, string category, string itemName = null, string parameter = null)
        {
            if (category == IsPartOf1D2DModelPropertyName)
            {
                var boolArray = values as bool[];
                if (boolArray != null && boolArray.Length > 0)
                {
                    bool isPartOf1D2DModel = boolArray[0];
                    // This property is made because 1D2D integrated models do not support UGrid format.
                    // Remove when this dependency has vanished (DELFT3DFM-989)
                    WaterFlowFMProperty isPartOf1D2DModelGuiProperty =
                        ModelDefinition.GetModelProperty(GuiProperties.PartOf1D2DModel);
                    if ((bool) isPartOf1D2DModelGuiProperty.Value != isPartOf1D2DModel)
                    {
                        isPartOf1D2DModelGuiProperty.Value = isPartOf1D2DModel;
                        if (isPartOf1D2DModel && UseMorSed)
                        {
                            ModelDefinition.UseMorphologySediment = false;
                            Log.InfoFormat(
                                Resources
                                    .WaterFlowFMModel_SetVar_FM_Model__0__is_part_of_a_1D2D_model_and_can_t_have_morphology_properties_and___or_sediments__Removing_these_properties_from_the_model,
                                Name);
                        }

                        ModelDefinition.SetMapFormatPropertyValue();
                    }
                }

                return;
            }

            if (category == DisableFlowNodeRenumberingPropertyName)
            {
                var boolArray = values as bool[];
                if (boolArray != null && boolArray.Length > 0)
                {
                    DisableFlowNodeRenumbering = boolArray[0];
                }

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

        private void ReportProgressText(string text = null)
        {
            progressText = text;
            base.OnProgressChanged();
        }

        public void SetModelStateHandlerModelWorkingDirectory(string modelExplicitWorkingDirectory)
        {
            ModelStateHandler.ModelWorkingDirectory = modelExplicitWorkingDirectory;
        }

        private void SetOutputDirAndWaqDirProperty()
        {
            WaterFlowFMProperty outputDirProperty = ModelDefinition.GetModelProperty(KnownProperties.OutputDir);

            string existingOutputDir = outputDirProperty.GetValueAsString();
            if (!existingOutputDir.StartsWith(OutputDirectoryName))
            {
                outputDirProperty.SetValueAsString(OutputDirectoryName);
                Log.InfoFormat("Running this model requires the OutputDirectory to be overwritten to: {0}",
                               OutputDirectoryName);
            }

            if (!SpecifyWaqOutputInterval)
            {
                return;
            }

            string relativeDWaqOutputDirectory = Path.Combine(OutputDirectoryName, DelwaqOutputDirectoryName);
            WaterFlowFMProperty waqOutputDirProperty = ModelDefinition.GetModelProperty(KnownProperties.WaqOutputDir);
            waqOutputDirProperty.SetValueAsString(relativeDWaqOutputDirectory);
        }

        private void ClearOutputDirAndWaqDirProperty()
        {
            ModelDefinition.GetModelProperty(KnownProperties.OutputDir).SetValueAsString(string.Empty);
            ModelDefinition.GetModelProperty(KnownProperties.WaqOutputDir).SetValueAsString(string.Empty);
        }
    }
}