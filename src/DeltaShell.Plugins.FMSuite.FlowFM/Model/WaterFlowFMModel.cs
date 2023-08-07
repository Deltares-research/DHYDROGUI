using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
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
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Restart;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
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
    public partial class WaterFlowFMModel : TimeDependentModelBase,
                                            IFileBased,
                                            IRestartModel<WaterFlowFMRestartFile>,
                                            IGridOperationApi,
                                            IDisposable,
                                            IHydroModel,
                                            IHydFileModel,
                                            IDimrModel,
                                            IWaterFlowFMModel,
                                            ISedimentModelData,
                                            ICoupledModel
    {
        private const string HydroAreaTag = "hydro_area_tag";
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowFMModel));
        private readonly DimrRunner runner;
        private WaterFlowFMModelDefinition modelDefinition;

        /// <summary>
        /// Creates a new instance of the <see cref="WaterFlowFMModel"/>.
        /// </summary>
        public WaterFlowFMModel() : base("FlowFM")
        {
            runner = new DimrRunner(this, new DimrApiFactory());

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

            ((INotifyCollectionChanged)area).CollectionChanged += HydroAreaCollectionChanged;
            ((INotifyPropertyChanged)area).PropertyChanged += HydroAreaPropertyChanged;
            ((INotifyPropertyChange)this).PropertyChanged += (s, e) => { MarkDirty(); };
            ((INotifyCollectionChanged)this).CollectionChanged += (s, e) => { MarkDirty(); };

            ModelDefinition = new WaterFlowFMModelDefinition();
            ModelDefinition.GetModelProperty(KnownProperties.NetFile).Value = Name + NetFile.FullExtension;

            SynchronizeModelDefinitions();

            SpatialData = new SpatialData(this);
            InitializeSpatialDataItems();

            Grid = new UnstructuredGrid();

            SetSpatialCoverages();
            RenameSubFilesIfApplicable();

            InitializeSyncers();

            SuspendClearOutputOnInputChange = true;
        }

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
                    ((INotifyPropertyChange)modelDefinition.Properties).PropertyChanged -=
                        OnModelDefinitionPropertyChanged;
                }

                modelDefinition = value;

                OnModelDefinitionChanged();

                if (modelDefinition != null)
                {
                    ((INotifyPropertyChange)modelDefinition.Properties).PropertyChanged +=
                        OnModelDefinitionPropertyChanged;
                }
            }
        }

        #region Implementation of IWaterFlowFMModel

        public ISpatialData SpatialData { get; }

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

            if (feature is IStructure weir)
            {
                IStructureFormula structureFormula = weir.Formula;
                switch (structureFormula)
                {
                    case GeneralStructureFormula _:
                        return KnownFeatureCategories.GeneralStructures;
                    case SimpleGateFormula _:
                        return KnownFeatureCategories.Gates;
                    default:
                        return KnownFeatureCategories.Weirs;
                }
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
                    ss.Function.AddTracer(name);
                }
            });
        }

        private void AddToInitialFractions(string spatiallyVaryingName)
        {
            SpatialData.AddFraction(UnstructuredGridCoverageFactory.CreateCellCoverage(spatiallyVaryingName, Grid));
        }

        private ModelFeatureCoordinateData<FixedWeir> CreateModelFeatureCoordinateDataFor(FixedWeir fixedWeir)
        {
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<FixedWeir> { Feature = fixedWeir };
            string scheme = ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).GetValueAsString();

            modelFeatureCoordinateData.UpdateDataColumns(scheme);
            return modelFeatureCoordinateData;
        }

        private ModelFeatureCoordinateData<BridgePillar> CreateModelFeatureCoordinateDataFor(BridgePillar bridgePillar)
        {
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar> { Feature = bridgePillar };
            modelFeatureCoordinateData.UpdateDataColumns();

            return modelFeatureCoordinateData;
        }

        #region Model Data

        private IEventedList<ISedimentFraction> sedimentFractions;
        private IEventedList<string> tracerDefinitions;
        private IEventedList<SourceAndSink> sourcesAndSinks;
        private IEventedList<Feature2D> pipes;
        private IEventedList<Feature2D> boundaries;
        private IDataItem areaDataItem;
        private DepthLayerDefinition depthLayerDefinition;

        private readonly Dictionary<IFeature, List<IDataItem>> areaDataItems =
            new Dictionary<IFeature, List<IDataItem>>();

        private readonly Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>> fixedWeirProperties =
            new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

        private bool disposed;

        public IEventedList<ISedimentFraction> SedimentFractions
        {
            get => sedimentFractions;
            set
            {
                if (sedimentFractions != null)
                {
                    ((INotifyPropertyChanged)SedimentFractions).PropertyChanged -= SedimentFractionPropertyChanged;
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
            get => ModelDefinition.GetReferenceDateAsDateTime();
            set => ModelDefinition.SetReferenceDateAsDateTime(value);
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
            get => (bool)ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool UseSecondaryFlow
        {
            get => (bool)ModelDefinition.GetModelProperty(KnownProperties.SecondaryFlow).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool UseTemperature =>
            (HeatFluxModelType)ModelDefinition.GetModelProperty(KnownProperties.Temperature).Value !=
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
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.WriteHisFile).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyHisStart
        {
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyHisStart).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyHisStop
        {
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyHisStop).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool WriteMapFile
        {
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyMapStart
        {
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyMapStart).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyMapStop
        {
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyMapStop).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool WriteClassMapFile
        {
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.WriteClassMapFile).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool WriteRstFile
        {
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyRstStart
        {
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyRstStart).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyRstStop
        {
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyRstStop).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyWaqOutputInterval
        {
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyWaqOutputStartTime
        {
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStartTime).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        public bool SpecifyWaqOutputStopTime
        {
            get => (bool)ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStopTime).Value;
            private set
            {
                // empty, but just used for event bubbling
            }
        }

        private int CdType
        {
            get => Convert.ToInt32(ModelDefinition.GetModelProperty(KnownProperties.ICdtyp).Value);
            set
            {
                // empty, but just used for event bubbling
            }
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

                return (HydroArea)GetDataItemValueByTag(HydroAreaTag);
            }
            set
            {
                IDataItem areaItem = GetDataItemByTag(HydroAreaTag);

                if (areaItem.Value != null)
                {
                    ((INotifyCollectionChanged)areaItem.Value).CollectionChanged -= HydroAreaCollectionChanged;
                    ((INotifyPropertyChanged)value).PropertyChanged -= HydroAreaPropertyChanged;
                }

                fixedWeirProperties.Clear();

                BridgePillarsDataModel.Clear();

                areaItem.Value = value;

                if (value != null)
                {
                    value.FixedWeirs.ForEach(
                        fw => fixedWeirProperties.Add(fw, CreateModelFeatureCoordinateDataFor((FixedWeir)fw)));
                    value.BridgePillars.ForEach(
                        bp => BridgePillarsDataModel.Add(CreateModelFeatureCoordinateDataFor(bp)));

                    ((INotifyCollectionChanged)value).CollectionChanged += HydroAreaCollectionChanged;
                    ((INotifyPropertyChanged)value).PropertyChanged += HydroAreaPropertyChanged;
                }
            }
        }

        public IEventedList<Feature2D> Boundaries
        {
            get => boundaries;
            private set
            {
                if (boundaries != null)
                {
                    boundaries.CollectionChanged -= FMRegionCollectionChanged;
                }

                boundaries = value;

                if (boundaries != null)
                {
                    boundaries.CollectionChanged += FMRegionCollectionChanged;
                }
            }
        }

        public IEventedList<BoundaryConditionSet> BoundaryConditionSets { get; private set; }

        public IEventedList<Feature2D> Pipes
        {
            get => pipes;
            private set
            {
                if (pipes != null)
                {
                    Pipes.CollectionChanged -= FMRegionCollectionChanged;
                }

                pipes = value;

                if (pipes != null)
                {
                    Pipes.CollectionChanged += FMRegionCollectionChanged;
                }
            }
        }



        public IEventedList<SourceAndSink> SourcesAndSinks
        {
            get => sourcesAndSinks;
            set
            {
                if (sourcesAndSinks != null)
                {
                    SourcesAndSinks.CollectionChanged -= SourcesAndSinksCollectionChanged;
                    SourcesAndSinks.CollectionChanged -= FMRegionCollectionChanged;
                }

                sourcesAndSinks = value;

                if (sourcesAndSinks != null)
                {
                    SourcesAndSinks.CollectionChanged += SourcesAndSinksCollectionChanged;
                    SourcesAndSinks.CollectionChanged += FMRegionCollectionChanged;
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
                                             spaceVarName => AllDataItems.Where(di => di.Name.Equals(spaceVarName)))
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
                var spatialOperationValueConverter = (SpatialOperationSetValueConverter)dataItem.ValueConverter;
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

                    var newOperation = new AddSamplesOperation(false) { Name = spatialOperationValueConverter.SpatialOperationSet.Name };
                    newOperation.SetInputData(AddSamplesOperation.SamplesInputName,
                                              new PointCloudFeatureProvider { PointCloud = coverage.ToPointCloud(0, true) });

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                disposed = true;

                // also disposes grid snap api, so if you remove this, at least make sure you dispose that one (holds remote instance in the air):
                Grid = null;
                DisposeSnapApi();
                runner?.Dispose();
                ClearSyncers();

                fixedWeirProperties.Values.ForEach(d => d.Dispose());
                fixedWeirProperties.Clear();
                BridgePillarsDataModel.ForEach(d => d.Dispose());
            }
        }

        #endregion

        public IEnumerable<IDataItem> GetDataItemsUsedForCouplingModel(DataItemRole role)
        {
            return GetChildDataItemLocations(role).SelectMany(GetChildDataItems);
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
    }
}