using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Editing;
using DelftTools.Utils.IO;
using DeltaShell.Dimr;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
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

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    [Entity]
    public partial class WaterFlowFMModel : TimeDependentModelBase, IDimrStateAwareModel, IFileBased,
                                            IHasCoordinateSystem, IGridOperationApi, IDisposable, IHydroModel,
                                            IHydFileModel, IDimrModel, IWaterFlowFMModel, ISedimentModelData
    {
        private const string HydroAreaTag = "hydro_area_tag";
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowFMModel));
        private readonly DimrRunner runner;
        private WaterFlowFMModelDefinition modelDefinition;

        /// <summary>
        /// Creates a new instance of the <see cref="WaterFlowFMModel"/>.
        /// </summary>
        /// <param name="mduFilePath">The path to the mdu file (optional).</param>
        /// <param name="clearOutputDirs">Whether or not any existing output directory properties need to be cleared.</param>
        /// <param name="progressChanged">A handle for notifying progress changes (optional).</param>
        public WaterFlowFMModel(string mduFilePath = null, bool clearOutputDirs = false, ImportProgressChangedDelegate progressChanged = null) :
            base("FlowFM")
        {
            runner = new DimrRunner(this);
            importProgressChanged = progressChanged;

            // Create sediment model data item
            SedimentModelDataItem = new SedimentModelDataItem();

            // Set default settings
            SnapVersion = 0;
            ValidateBeforeRun = true;
            DisableFlowNodeRenumbering = false;
            TracerDefinitions = new EventedList<string>();
            SedimentFractions = new EventedList<ISedimentFraction>();

            BridgePillarsDataModel = new List<ModelFeatureCoordinateData<BridgePillar>>();

            SedimentOverallProperties = SedimentFractionHelper.GetSedimentationOverAllProperties();

            var area = new HydroArea();
            AddDataItem(area, DataItemRole.Input, HydroAreaTag);
            areaDataItem = GetDataItemByTag(HydroAreaTag);

            ((INotifyCollectionChanged) area).CollectionChanged += HydroAreaCollectionChanged;
            ((INotifyPropertyChanged) area).PropertyChanged += HydroAreaPropertyChanged;
            ((INotifyPropertyChange) this).PropertyChanged += (s, e) => { MarkDirty(); };
            ((INotifyCollectionChanged) this).CollectionChanged += (s, e) => { MarkDirty(); };

            // Initialize mdu model settings
            if (string.IsNullOrEmpty(mduFilePath))
            {
                ModelDefinition = new WaterFlowFMModelDefinition();
                ModelDefinition.GetModelProperty(KnownProperties.NetFile).Value = Name + NetFile.FullExtension;

                SynchronizeModelDefinitions();

                Grid = new UnstructuredGrid();

                InitializeUnstructuredGridCoverages();

                AddSpatialDataItems();

                RenameSubFilesIfApplicable();
            }
            else
            {
                LoadStateFromMdu(mduFilePath);

                AddSpatialDataItems();

                FireImportProgressChanged("Reading spatial operations", 9, TotalImportSteps);
                ImportSpatialOperationsAfterCreating();

                if (clearOutputDirs)
                {
                    ClearOutputDirAndWaqDirProperty();
                }
            }

            InitializeSyncers();

            importProgressChanged = null;
        }

        public Type SupportedRegionType => typeof(HydroArea);

        /// <summary>
        /// Make a copy of the file if it is located in the DeltaShell working directory
        /// </summary>
        public bool CopyFromWorkingDirectory { get; }

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

        #region Implementation of IWaterFlowFMModel

        public bool DisableFlowNodeRenumbering { get; set; }

        #endregion

        // Do not remove...used in HydroModelBuilder.py
        public void SetWaveForcing()
        {
            ModelDefinition.GetModelProperty(KnownProperties.WaveModelNr).SetValueAsString("3");
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
                return KnownFeatureCategories.ObservationPoints;
            }

            if (Area.ObservationCrossSections.Contains(feature))
            {
                return KnownFeatureCategories.ObservationCrossSections;
            }

            return null;
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

        private void AddToInitialCoverages(IList<UnstructuredGridCellCoverage> initialCoverages, string spatiallyVaryingName)
        {
            if (initialCoverages == null)
            {
                return;
            }

            IDataItem t = DataItems.FirstOrDefault(di => di.Name == spatiallyVaryingName);
            if (t == null)
            {
                UnstructuredGridCellCoverage unstructuredGridCellCoverage =
                    CreateUnstructuredGridCellCoverage(spatiallyVaryingName, Grid);
                initialCoverages.Add(unstructuredGridCellCoverage);
            }
            else
            {
                var unstrGridCellCoverage = t.Value as UnstructuredGridCellCoverage;
                if (unstrGridCellCoverage == null)
                {
                    t.Value = CreateUnstructuredGridCellCoverage(spatiallyVaryingName, Grid);
                    initialCoverages.Add((UnstructuredGridCellCoverage) t.Value);
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
                    if (!initialCoverages.Contains(unstrGridCellCoverage))
                    {
                        initialCoverages.Add(unstrGridCellCoverage);
                    }
                }
            }
        }

        private void AddToInitialFractions(string spatiallyVaryingName)
        {
            AddToInitialCoverages(InitialFractions, spatiallyVaryingName);
        }

        private void AddToInitialTracers(string spatiallyVaryingName)
        {
            AddToInitialCoverages(InitialTracers, spatiallyVaryingName);
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

        #region Model Data

        private IEventedList<ISedimentFraction> sedimentFractions;
        private IEventedList<BoundaryConditionSet> boundaryConditionSets;
        private IEventedList<string> tracerDefinitions;
        private IEventedList<SourceAndSink> sourcesAndSinks;
        private IDataItem areaDataItem;
        private DepthLayerDefinition depthLayerDefinition;

        private readonly Dictionary<IFeature, List<IDataItem>> areaDataItems =
            new Dictionary<IFeature, List<IDataItem>>();

        private readonly Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>> fixedWeirProperties =
            new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

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

        public IEventedList<IWindField> WindFields { get; private set; }

        public IList<IUnsupportedFileBasedExtForceFileItem> UnsupportedFileBasedExtForceFileItems { get; private set; }

        public HeatFluxModelType HeatFluxModelType { get; private set; }

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

        private int CdType
        {
            get => Convert.ToInt32(ModelDefinition.GetModelProperty(KnownProperties.ICdtyp).Value);
            set {}
        }

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
                    OutputMapFileStore.SetCoordinateSystem(value);
                }

                // Note: coverages are handled via the feature collections.

                InvalidateSnapping();
            }
        }

        #endregion

        #region IHydroModel

        public IHydroRegion Region => Area;

        #endregion

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
                        fw => fixedWeirProperties.Add(fw, CreateModelFeatureCoordinateDataFor((FixedWeir) fw)));
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

        /// <summary>
        /// Gets the bridge pillars data model.
        /// </summary>
        /// <value>
        /// The bridge pillars data model.
        /// </value>
        public IList<ModelFeatureCoordinateData<BridgePillar>> BridgePillarsDataModel { get; private set; }

        public IEnumerable<ModelFeatureCoordinateData<FixedWeir>> FixedWeirsProperties => fixedWeirProperties.Values;

        #endregion Model Data

        #region IHasCoordinateSystem

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

                    var newOperation = new AddSamplesOperation(false) {Name = spatialOperationValueConverter.SpatialOperationSet.Name};
                    newOperation.SetInputData(AddSamplesOperation.SamplesInputName,
                                              new PointCloudFeatureProvider
                                              {
                                                  PointCloud = coverage.ToPointCloud(0, true)
                                              });

                    spatialOperationsLookupTable.Add(dataItem.Name, new[]
                    {
                        newOperation
                    });
                }
            }

            return spatialOperationsLookupTable;
        }

        public IEventedList<ISedimentProperty> SedimentOverallProperties { get; set; }

        #endregion

        #region IDisposable

        private bool disposing;

        public void Dispose()
        {
            disposing = true;
            // also disposes grid snap api, so if you remove this, at least make sure you dispose that one (holds remote instance in the air):
            Grid = null;
            DisposeSnapApi();
            ClearSyncers();

            fixedWeirProperties.Values.ForEach(d => d.Dispose());
            fixedWeirProperties.Clear();
            BridgePillarsDataModel.ForEach(d => d.Dispose());
        }

        #endregion
    }
}