using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
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

        private void MarkDirty()
        {
            unchecked
            {
                dirtyCounter++;
            } //unchecked is default, but its here to declare intent
        }

        private int dirtyCounter; //tells NHibernate we need to be saved
        private const string HydroAreaTag = "hydro_area_tag";

        private IEventedList<string> tracerDefinitions;

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

        public virtual bool CanRunParallel => true;

        public virtual string MpiCommunicatorString => "DFM_COMM_DFMWORLD";

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
    }
}