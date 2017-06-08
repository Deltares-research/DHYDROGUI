using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.Restart;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;

using log4net;

using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel
{
    [Entity]
    public class WaterQualityModel : TimeDependentModelBase, IStateAwareModelEngine, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterQualityModel));

        #region Tags

        public const string DispersionTag = "DispersionTag";
        public const string ProcessCoefficientsTag = "ProcessCoefficientsTag";
        public const string BloomAlgaeTag = "BloomAlgaeTag";
        public const string InputFileHybridTag = "InputFileHybridTag";
        public const string BathymetryTag = "BathymetryTag";
        public const string BoundaryDataTag = "BoundaryDataTag";
        public const string LoadsDataTag = "LoadsDataTag";
        public const string GridTag = "GridTag";
        public const string SubstanceProcessLibraryTag = "SubstanceProcessLibraryTag";
        public const string InputFileCommandLineTag = "InputFileCommandLineTag";
        public const string LoadsTag = "LoadsTag";
        public const string InitialConditionsTag = "InitialConditionsTag";
        public const string ObservationPointsTag = "ObservationPointsTag";
        public const string ObservationAreasTag = "ObservationAreasTag";
        public const string MonitoringOutputTag = "MonitoringOutputTag";
        public const string OutputSubstancesTag = "OutputSubstancesTag";
        public const string OutputParametersTag = "OutputParametersTag";

        #endregion

        #region Fields

        private static readonly int[] SupportedMetaDataVersions = { 1 };

        private double progressPercentage;
        private bool enableMarkOutputOutOfSync;

        private readonly ModelFileBasedStateHandler modelStateHandler;
        private IWaqPreProcessor waqPreProcessor;
        private IWaqProcessor waqProcessor;
        private WaqInitializationSettings waqInitializationSettings;
        private bool importingHydroData;
        private string importProgress;
        private string salinityRelativeFilePath;
        private string temperatureRelativeFilePath;
        private string shearStressesRelativeFilePath;
        private AttributesFileData attributeData;
        private PointToGridCellMapper pointToGridCellMapper;
        private bool hasHydroDataImported;
        private LayerType layerType;

        private readonly string tempWorkDirectory;
        private string modelDataDirectory;
        private LazyMapFileFunctionStore mapFileFunctionStore;
        private ICoordinateSystem overriddenCoordinateSystem;
        private IHydroData hydroData;
        private WaterQualityModelSettings modelSettings;
        private string verticalDiffusionRelativeFilePath;
        private IEventedList<WaterQualityObservationPoint> observationPoints;
        private IEventedList<WaterQualityLoad> loads;

        #endregion

        public WaterQualityModel() : base("Water Quality")
        {
            tempWorkDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            modelSettings = new WaterQualityModelSettings
            {
                MonitoringOutputLevel = MonitoringOutputLevel.PointsAndAreas
            };

            modelStateHandler = new ModelFileBasedStateHandler(Name,
                new List<DelftTools.Utils.Tuple<string, string>>
                    {
                        new DelftTools.Utils.Tuple<string, string>("deltashell_res.map", "deltashell_res_in.map")
                    });

            InitializeInputDataItems();

            HydrodynamicLayerThicknesses = null;
            NumberOfHydrodynamicLayersPerWaqLayer = null;

            HorizontalDispersion = 1.0;
            VerticalDispersion = 1e-7;
            UseAdditionalHydrodynamicVerticalDiffusion = false;
            Boundaries = new EventedList<WaterQualityBoundary>();
            BoundaryNodeIds = new Dictionary<WaterQualityBoundary, int[]>();
            Loads = new EventedList<WaterQualityLoad>();
            ObservationPoints = new EventedList<WaterQualityObservationPoint>();

            this.SetupModelDataFolderStructure(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

            AddDataItemSet(new EventedList<UnstructuredGridCellCoverage>(), "Substances", DataItemRole.Output, OutputSubstancesTag);
            AddDataItemSet(new EventedList<UnstructuredGridCellCoverage>(), "Output parameters", DataItemRole.Output, OutputParametersTag);

            if (modelSettings.MonitoringOutputLevel != MonitoringOutputLevel.None)
            {
                AddDataItemSet(new EventedList<WaterQualityObservationVariableOutput>(), "Monitoring locations", DataItemRole.Output, MonitoringOutputTag);
            }

            SubscribeToInternalEvents();
            enableMarkOutputOutOfSync = true;
        }

        # region Public properties

        public override string KernelVersions
        {
            get
            {
                var asmPath1 = DelwaqFileStructureHelper.GetDelwaq1ExePath();
                var asmPath2 = DelwaqFileStructureHelper.GetDelwaq2ExePath();

                var kernelVersions = "";

                if (File.Exists(asmPath1))
                {
                    kernelVersions += string.Format("Kernel: {0}  {1}",
                        DelwaqFileStructureHelper.DELWAQ1_EXE,
                        FileVersionInfo.GetVersionInfo(asmPath1).FileVersion) + Environment.NewLine;
                }
                if (File.Exists(asmPath2))
                {
                    kernelVersions += string.Format("Kernel: {0}  {1}",
                        DelwaqFileStructureHelper.DELWAQ2_EXE,
                        FileVersionInfo.GetVersionInfo(asmPath2).FileVersion) + Environment.NewLine;
                }

                return kernelVersions;
            }
        }

        /// <summary>
        /// The settings of the water quality model
        /// </summary>
        public virtual WaterQualityModelSettings ModelSettings
        {
            get { return modelSettings; }
            protected set { modelSettings = value; }
        }

        /// <summary>
        /// The input file of the water quality model
        /// </summary>
        public virtual TextDocument InputFile
        {
            get { return InputFileCommandLine; }
        }

        /// <summary>
        /// Mapper for resolving coordinates (x,y,(z)) to cell indices.
        /// </summary>
        public virtual PointToGridCellMapper PointToGridCellMapper
        {
            get { return pointToGridCellMapper; }
        }

        /// <summary>
        /// The input file of the water quality model in case of command line calculations
        /// </summary>
        public virtual TextDocument InputFileCommandLine
        {
            get
            {
                var inputFileDataItem = GetDataItemByTag(InputFileCommandLineTag);

                return inputFileDataItem != null
                           ? inputFileDataItem.Value as TextDocument
                           : null;
            }
        }

        /// <summary>
        /// The input file of the water quality model in case of hybrid calculations
        /// </summary>
        public virtual TextDocument InputFileHybrid
        {
            get
            {
                var inputFileDataItem = GetDataItemByTag(InputFileHybridTag);

                return inputFileDataItem != null
                           ? inputFileDataItem.Value as TextDocument
                           : null;
            }
        }

        /// <summary>
        /// The substance process library of the water quality model 
        /// </summary>
        public virtual SubstanceProcessLibrary SubstanceProcessLibrary
        {
            get { return (SubstanceProcessLibrary)GetDataItemByTag(SubstanceProcessLibraryTag).Value; }
        }

        /// <summary>
        /// The (dry waste) loads of the water quality model.
        /// </summary>
        public virtual IEventedList<WaterQualityLoad> Loads
        {
            get { return loads; }
            protected set
            {
                if (loads != null)
                {
                    loads.CollectionChanged -= OnInputCollectionChanged;
                }
                loads = value;
                if (loads != null)
                {
                    loads.CollectionChanged += OnInputCollectionChanged;
                }
            }
            
        }

        /// <summary>
        /// The Observation points of the water quality model.
        /// </summary>
        public virtual IEventedList<WaterQualityObservationPoint> ObservationPoints
        {
            get { return observationPoints; }
            protected set
            {
                if (observationPoints != null)
                {
                    observationPoints.CollectionChanged -= OnInputCollectionChanged;
                }
                observationPoints = value;
                if (observationPoints != null)
                {
                    observationPoints.CollectionChanged += OnInputCollectionChanged;
                }
            }
        }

        /// <summary>
        /// The observation areas of the water quality model.
        /// </summary>
        public virtual WaterQualityObservationAreaCoverage ObservationAreas
        {
            get { return GetDataItemValueByTag<WaterQualityObservationAreaCoverage>(ObservationAreasTag); }
        }

        /// <summary>
        /// The initial conditions of the water quality model
        /// </summary>
        public virtual IEventedList<IFunction> InitialConditions
        {
            get { return GetDataItemSetByTag(InitialConditionsTag).AsEventedList<IFunction>(); }
        }

        /// <summary>
        /// The process coefficients of the water quality model
        /// </summary>
        public virtual IEventedList<IFunction> ProcessCoefficients
        {
            get { return GetDataItemSetByTag(ProcessCoefficientsTag).AsEventedList<IFunction>(); }
        }

        /// <summary>
        /// The dispersion definitions of the water quality model
        /// </summary>
        public virtual IEventedList<IFunction> Dispersion
        {
            get { return GetDataItemSetByTag(DispersionTag).AsEventedList<IFunction>(); }
        }

        /// <summary>
        /// Gets or sets dispersion in the horizontal axis in m^2/s.
        /// </summary>
        public virtual double HorizontalDispersion
        {
            get { return (double)Dispersion[0].Components[0].DefaultValue; }
            set { SetHorizontalDispersion(value); }
        }

        /// <summary>
        /// Gets or sets dispersion in the vertical axis in m^2/s.
        /// </summary>
        public virtual double VerticalDispersion { get; set; }

        /// <summary>
        /// Determines if additional vertical diffusion data available in the hydro dynamic
        /// data should be used (true) or ignored (false).
        /// </summary>
        public virtual bool UseAdditionalHydrodynamicVerticalDiffusion { get; set; }

        /// <summary>
        /// The monitoring output data item set of the water quality model
        /// </summary>
        public virtual IDataItemSet MonitoringOutputDataItemSet
        {
            get { return GetDataItemSetByTag(MonitoringOutputTag); }
        }

        /// <summary>
        /// The monitoring output of the water quality model
        /// </summary>
        public virtual IList<WaterQualityObservationVariableOutput> ObservationVariableOutputs
        {
            get
            {
                var monitoringOutputDataItemSet = MonitoringOutputDataItemSet;

                return monitoringOutputDataItemSet != null
                           ? monitoringOutputDataItemSet.AsEventedList<WaterQualityObservationVariableOutput>()
                           : new EventedList<WaterQualityObservationVariableOutput>();
            }
        }
        
        /// <summary>
        /// The output substances data item set of the water quality model
        /// </summary>
        public virtual IDataItemSet OutputSubstancesDataItemSet
        {
            get { return GetDataItemSetByTag(OutputSubstancesTag); }
        }
        
        /// <summary>
        /// The output parameters data item set of the water quality model
        /// </summary>
        public virtual IDataItemSet OutputParametersDataItemSet
        {
            get { return GetDataItemSetByTag(OutputParametersTag); }
        }

        /// <summary>
        /// The calculation grid of the water quality model 
        /// </summary>
        public virtual UnstructuredGrid Grid
        {
            get { return (UnstructuredGrid)GetDataItemByTag(GridTag).Value; }
        }

        /// <summary>
        /// The bathymetry for the water quality model
        /// </summary>
        public virtual UnstructuredGridVertexCoverage Bathymetry
        {
            get { return (UnstructuredGridVertexCoverage)GetDataItemByTag(BathymetryTag).Value; }
            protected set { GetDataItemByTag(BathymetryTag).Value = value; }
        }

        /// <summary>
        /// The reference time from the hydro data
        /// </summary>
        public virtual DateTime ReferenceTime { get; set; }

        /// <summary>
        /// This interface makes it possible to retrieve the hydrodynamic data
        /// that is required to run the waq model.
        /// Could be a *.hyd-file, but could also be an actual SOBEK model or FM model
        /// that is running in the background. E.g. via integrated model.
        /// </summary>
        public virtual IHydroData HydroData
        {
            get { return hydroData; }
            protected set
            {
                if (hydroData != null)
                {
                    hydroData.DataChanged -= HydroDataOnDataChanged;
                    hydroData.Dispose();
                }

                hydroData = value;

                if (hydroData != null)
                {
                    hydroData.DataChanged += HydroDataOnDataChanged;
                }
            }
        }

        /// <summary>
        /// Indicates if hydro dynamic data from <see cref="HydroData"/> has been 
        /// successfully imported or not. 
        /// </summary>
        public virtual bool HasHydroDataImported
        {
            get { return hasHydroDataImported; }
            protected set
            {
                hasHydroDataImported = value;

                if (value)
                {
                    HasEverImportedHydroData = true;
                }
            }
        }

        /// <summary>
        /// Indicates if there every was hydro data imported on this model.
        /// Should never be set to false.
        /// </summary>
        public virtual bool HasEverImportedHydroData { get; protected set; }

        /// <summary>
        /// Describes the type of grid used by the hydro dynamics data.
        /// </summary>
        public virtual HydroDynamicModelType ModelType { get; protected set; }

        /// <summary>
        /// Describes the applied layer modeling in the hydro dynamics data.
        /// </summary>
        public virtual LayerType LayerType
        {
            get { return layerType; }
            protected set
            {
                if (layerType != value)
                {
                    SetWaqPointHeights();
                    layerType = value;
                }
            }
        }

        /// <summary>
        /// The allowed top-level for Z coordinates in the model.
        /// </summary>
        public virtual double ZTop { get; protected set; }

        /// <summary>
        /// The allowed bottom-level for Z coordinates in the model.
        /// </summary>
        public virtual double ZBot { get; protected set; }

        /// <summary>
        /// The areas file can be found in the *.hyd-file and
        /// is passed in the input file.
        /// *.are
        /// <see cref="IHydroData.VolumesRelativePath"/>
        /// </summary>
        public virtual string AreasRelativeFilePath { get; protected set; }

        /// <summary>
        /// The volumes file can be found in the *.hyd-file and
        /// is passed in the input file.
        /// *.vol
        /// <see cref="IHydroData.VolumesRelativePath"/>
        /// </summary>
        public virtual string VolumesRelativeFilePath { get; protected set; }

        /// <summary>
        /// The flows file can be found in the *.hyd-file and
        /// is passed in the input file.
        /// *.flo
        /// <see cref="IHydroData.FlowsRelativePath"/>
        /// </summary>
        public virtual string FlowsRelativeFilePath { get; protected set; }

        /// <summary>
        /// The pointers file can be found in the *.hyd-file and
        /// is passed in the input file.
        /// *.poi
        /// <see cref="IHydroData.GetPointersRelativeFilePath"/>
        /// </summary>
        public virtual string PointersRelativeFilePath { get; protected set; }

        /// <summary>
        /// The lenghts file can be found in the *.hyd-file and
        /// is passed in the input file.
        /// *.len
        /// <see cref="IHydroData.GetLengthsRelativeFilePath"/>
        /// </summary>
        public virtual string LengthsRelativeFilePath { get; protected set; }

        /// <summary>
        /// The vertical diffusion file can be found in the *.hyd-file and
        /// is passed in the input file.
        /// *.vdf
        /// <see cref="IHydroData.VerticalDiffusionRelativePath"/>
        /// </summary>
        public virtual string VerticalDiffusionRelativeFilePath
        {
            get { return verticalDiffusionRelativeFilePath; }
            protected set
            {
                verticalDiffusionRelativeFilePath = value;

                if (!HasEverImportedHydroData)
                {
                    UseAdditionalHydrodynamicVerticalDiffusion = !string.IsNullOrWhiteSpace(value);
                }
            }
        }

        /// <summary>
        /// The surfaces file can be found in the *.hyd-file and
        /// is passed in the input file.
        /// *.srf
        /// <see cref="IHydroData.GetSurfacesRelativeFilePath"/>
        /// </summary>
        public virtual string SurfacesRelativeFilePath { get; protected set; }

        /// <summary>
        /// The salinity file can be found in the *.hyd-file and
        /// is passed in the input file.
        /// *.sal
        /// <see cref="IHydroData.GetSalinityRelativeFilePath"/>
        /// </summary>
        public virtual string SalinityRelativeFilePath
        {
            get { return salinityRelativeFilePath; }
            protected set
            {
                salinityRelativeFilePath = value;

                HandleNewHydroDynamicsFunctionDataSet(GetDataItemSetByTag(ProcessCoefficientsTag), "Salinity");
            }
        }

        /// <summary>
        /// The temperature file can be found in the *.hyd-file and
        /// is passed in the input file.
        /// *.tmp?
        /// <see cref="IHydroData.GetTemperatureRelativeFilePath"/>
        /// </summary>
        public virtual string TemperatureRelativeFilePath
        {
            get { return temperatureRelativeFilePath; }
            protected set
            {
                temperatureRelativeFilePath = value;

                HandleNewHydroDynamicsFunctionDataSet(GetDataItemSetByTag(ProcessCoefficientsTag), "Temp");
            }
        }

        /// <summary>
        /// The shear stress file can be found in the *.hyd-file and
        /// is passed in the input file.
        /// *.tau
        /// <see cref="IHydroData.GetShearStressesRelativeFilePath"/>
        /// </summary>
        public virtual string ShearStressesRelativeFilePath
        {
            get { return shearStressesRelativeFilePath; }
            protected set
            {
                shearStressesRelativeFilePath = value;

                HandleNewHydroDynamicsFunctionDataSet(GetDataItemSetByTag(ProcessCoefficientsTag), "Tau");
            }
        }

        /// <summary>
        /// The attributes file that will be included as INCLUDE in the input file.
        /// <see cref="IHydroData.GetAttributesRelativeFilePath"/>
        /// </summary>
        public virtual string AttributesRelativeFilePath { get; protected set; }

        public virtual int NumberOfHorizontalExchanges { get; protected set; }

        public virtual int NumberOfVerticalExchanges { get; protected set; }

        public virtual int NumberOfHydrodynamicLayers { get; protected set; }

        public virtual int NumberOfDelwaqSegmentsPerHydrodynamicLayer { get; protected set; }

        public virtual int NumberOfWaqSegmentLayers { get; protected set; }

        public virtual IEventedList<WaterQualityBoundary> Boundaries { get; protected set; }

        public virtual IDictionary<WaterQualityBoundary, int[]> BoundaryNodeIds { get; protected set; }

        public virtual double[] HydrodynamicLayerThicknesses { get; protected set; }

        public virtual int[] NumberOfHydrodynamicLayersPerWaqLayer { get; protected set; }

        public override string ProgressText
        {
            get
            {
                if (importingHydroData)
                {
                    return importProgress;
                }

                if (Status == ActivityStatus.Initializing)
                {
                    return "Initializing";
                }

                if (Status == ActivityStatus.Finishing)
                {
                    return "Parsing output";
                }

                return GetProgressTextCore(progressPercentage/100.0);
            }
        }

        public virtual bool UseSaveStateTimeRange { get; set; }

        public virtual DateTime SaveStateStartTime { get; set; }

        public virtual DateTime SaveStateStopTime { get; set; }

        public virtual TimeSpan SaveStateTimeStep { get; set; }

        /// <summary>
        /// The explicit output directory is used to send
        /// delwaq1 a directory where it can directly put its output files.
        /// This property is set when saving the model or opening
        /// the model.
        /// 
        /// There is no need to save this in NHibernate.
        /// It is set with ProjectSaved and ProjectOpened.
        /// </summary>
        public virtual string ExplicitOutputDirectory
        {
            get { return ModelSettings.OutputDirectory; }
            set { SetOutputDirectory(value); }
        }

        public override string ExplicitWorkingDirectory
        {
            get { return base.ExplicitWorkingDirectory; }
            set
            {
                base.ExplicitWorkingDirectory = value;
                SettingExpliticWorkingDirectory(value);
            }
        }

        public virtual string ModelDataDirectory
        {
            get { return modelDataDirectory; }
            set { modelDataDirectory = value; }
        }

        public virtual DataTableManager BoundaryDataManager { get { return (DataTableManager)GetDataItemByTag(BoundaryDataTag).Value; } }

        public virtual DataTableManager LoadsDataManager { get { return (DataTableManager)GetDataItemByTag(LoadsDataTag).Value; } }

        /// <summary>
        /// The coordinate system can be found in the grid, 
        /// but may be overridden by the user.
        /// </summary>
        public virtual ICoordinateSystem CoordinateSystem
        {
            get { return Grid != null ? Grid.CoordinateSystem : null; }
            set
            {
                overriddenCoordinateSystem = value;

                if (Grid == null) return;

                var existingGridCoordinateSystemString = Grid.CoordinateSystem == null
                    ? string.Empty
                    : Grid.CoordinateSystem.PROJ4;

                var newCoordinateSystemString = value == null
                    ? string.Empty
                    : value.PROJ4;

                if (existingGridCoordinateSystemString == newCoordinateSystemString) return;

                Grid.CoordinateSystem = value;
                OnInputPropertyChanged(this, new PropertyChangedEventArgs("CoordinateSystem"));
            }
        }

        public virtual LazyMapFileFunctionStore MapFileFunctionStore
        {
            get
            {
                if (mapFileFunctionStore != null)
                {
                    return mapFileFunctionStore;
                }

                var storeFromCoverages = this.GetOutputCoverages().Select(c => c.Store).OfType<LazyMapFileFunctionStore>().FirstOrDefault();
                mapFileFunctionStore = storeFromCoverages ?? new LazyMapFileFunctionStore();

                return mapFileFunctionStore;
            }
        }

        # endregion

        public override IProjectItem DeepClone()
        {
            throw new NotSupportedException("WaterQualityModel does not support cloning.");
        }
       
        public virtual void ImportHydroData(IHydroData data, bool importTimers = false, bool importCoordinateSystem = false, bool markOutputOutOfSync = true)
        {
            if (data == null)
            {   
                HasHydroDataImported = false;
                throw new ArgumentNullException("data", "No hydrodynamics data was specified.");
            }

            if (data.Equals(HydroData))
                return;

            HasHydroDataImported = false;

            enableMarkOutputOutOfSync = markOutputOutOfSync;

            var schematizationRemainsUnchanged = data.HasSameSchematization(HydroData);
            HydroData = data;

            BeginEdit(new DefaultEditAction("Importing hydrodynamics data"));

            importingHydroData = true;

            try
            {
                SetImportProgress("Importing grid");
                ModelType = HydroData.HydroDynamicModelType;
                LayerType = HydroData.LayerType;
                ZTop = HydroData.ZTop;
                ZBot = HydroData.ZBot;
                SetNewGrid(HydroData.Grid, schematizationRemainsUnchanged);

                if (!schematizationRemainsUnchanged)
                {
                    ClearOutput();
                }

                // import settings that should not be overridden by re-importing the hyd file
                if (!HasEverImportedHydroData || importTimers)
                {
                    SetImportProgress("Importing timers");
                    StartTime = HydroData.ConversionStartTime;
                    StopTime = HydroData.ConversionStopTime;
                    TimeStep = HydroData.ConversionTimeStep;
                    ReferenceTime = HydroData.ConversionReferenceTime;

                    // import the times from the hyd file when importing the first time.
                    ModelSettings.HisStartTime = HydroData.ConversionStartTime;
                    ModelSettings.HisStopTime = HydroData.ConversionStopTime;
                    ModelSettings.HisTimeStep = HydroData.ConversionTimeStep;
                    ModelSettings.MapStartTime = HydroData.ConversionStartTime;
                    ModelSettings.MapStopTime = HydroData.ConversionStopTime;
                    ModelSettings.MapTimeStep = HydroData.ConversionTimeStep;
                    ModelSettings.BalanceStartTime = HydroData.ConversionStartTime;
                    ModelSettings.BalanceStopTime = HydroData.ConversionStopTime;
                    ModelSettings.BalanceTimeStep = HydroData.ConversionTimeStep;
                }

                if (!HasEverImportedHydroData || importCoordinateSystem)
                {
                    CoordinateSystem = HydroData.Grid == null ? null : HydroData.Grid.CoordinateSystem;
                }

                SetImportProgress("Importing file paths");
                AreasRelativeFilePath = HydroData.AreasRelativePath;
                VolumesRelativeFilePath = HydroData.VolumesRelativePath;
                FlowsRelativeFilePath = HydroData.FlowsRelativePath;
                PointersRelativeFilePath = HydroData.PointersRelativePath;
                LengthsRelativeFilePath = HydroData.LengthsRelativePath;
                SalinityRelativeFilePath = HydroData.SalinityRelativePath;
                TemperatureRelativeFilePath = HydroData.TemperatureRelativePath;
                VerticalDiffusionRelativeFilePath = HydroData.VerticalDiffusionRelativePath;
                SurfacesRelativeFilePath = HydroData.SurfacesRelativePath;
                ShearStressesRelativeFilePath = HydroData.ShearStressesRelativePath;
                AttributesRelativeFilePath = HydroData.AttributesRelativePath;

                SetImportProgress("Importing exchanges and layer information");
                NumberOfHorizontalExchanges = HydroData.NumberOfHorizontalExchanges;
                NumberOfVerticalExchanges = HydroData.NumberOfVerticalExchanges;
                NumberOfHydrodynamicLayers = HydroData.NumberOfHydrodynamicLayers;
                NumberOfDelwaqSegmentsPerHydrodynamicLayer = HydroData.NumberOfDelwaqSegmentsPerHydrodynamicLayer;
                NumberOfWaqSegmentLayers = HydroData.NumberOfWaqSegmentLayers;
                HydrodynamicLayerThicknesses = HydroData.HydrodynamicLayerThicknesses;
                NumberOfHydrodynamicLayersPerWaqLayer = HydroData.NumberOfHydrodynamicLayersPerWaqSegmentLayer;

                SetImportProgress("Importing boundaries");
                ResolveBoundaryImport(HydroData.GetBoundaries());
                BoundaryNodeIds = HydroData.GetBoundaryNodeIds();

                SetImportProgress("Importing attributes");
                var fileInfo = new FileInfo(Path.Combine(Path.GetDirectoryName(HydroData.FilePath), AttributesRelativeFilePath));

                attributeData = AttributesFileReader.ReadAll(NumberOfDelwaqSegmentsPerHydrodynamicLayer, NumberOfWaqSegmentLayers, fileInfo);
                pointToGridCellMapper = SetUpPointToGridCellMapper();

                HasHydroDataImported = true;
            }
            finally
            {
                importingHydroData = false;
                EndEdit();
                enableMarkOutputOutOfSync = true;
            }
        }

        private void ResolveBoundaryImport(IEnumerable<WaterQualityBoundary> importedBoundaries)
        {
            List<WaterQualityBoundary> newBoundaries = new List<WaterQualityBoundary>();
            foreach (var waterQualityBoundary in importedBoundaries)
            {
                // find an already loaded boundary
                var existingBoundary = Boundaries.FirstOrDefault(b => b.Name == waterQualityBoundary.Name);

                if (existingBoundary != null)
                {
                    // copy the location aliases
                    // TODO: extend this list if there is more to be mapped
                    waterQualityBoundary.LocationAliases = existingBoundary.LocationAliases;
                }

                newBoundaries.Add(waterQualityBoundary);
            }

            Boundaries.Clear();
            Boundaries.AddRange(newBoundaries);
        }

        private PointToGridCellMapper SetUpPointToGridCellMapper()
        {
            var mapper = new PointToGridCellMapper { Grid = Grid };
            var waqRelativeThicknesses = new double[NumberOfWaqSegmentLayers];
            var hydroIndex = 0;
            for (int i = 0; i < NumberOfWaqSegmentLayers; i++)
            {
                var waqRelativeThickness = 0.0;
                var addUptoHydroLayer = hydroIndex + NumberOfHydrodynamicLayersPerWaqLayer[i];
                while (hydroIndex < addUptoHydroLayer)
                {
                    waqRelativeThickness += HydrodynamicLayerThicknesses[hydroIndex++];
                }

                waqRelativeThicknesses[i] = waqRelativeThickness;
            }
            if (LayerType == LayerType.Sigma)
            {
                mapper.SetSigmaLayers(waqRelativeThicknesses);
            }
            else if (LayerType == LayerType.ZLayer)
            {
                mapper.SetZLayers(waqRelativeThicknesses, ZTop, ZBot);
            }
            return mapper;
        }

        /// <summary>
        /// Determines whether the model has data available in its hydro dynamics for a 
        /// specific function, process or substance. 
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns>True if there is data defined in the hydro dynamics, false otherwise.</returns>
        public virtual bool HasDataInHydroDynamics(IFunction function)
        {
            return function != null && HasDataInHydroDynamics(function.Name);
        }

        /// <summary>
        /// Determines whether the model has data available in its hydro dynamics for a 
        /// specific function, process or substance. 
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <returns>True if there is data defined in the hydro dynamics, false otherwise.</returns>
        public virtual bool HasDataInHydroDynamics(string functionName)
        {
            if (HydroData != null)
            {
                return HydroData.HasDataFor(functionName);
            }
            return false;
        }

        /// <summary>
        /// Gets the file path for a given function, process or substance when available 
        /// in the hydro dynamics.
        /// </summary>
        /// <param name="function">The funcion.</param>
        /// <returns>The filepath for the given function if <see cref="HasDataInHydroDynamics(IFunction)"/> 
        /// returns true for <paramref name="function"/>.</returns>
        /// <exception cref="InvalidOperationException">When <see cref="HasDataInHydroDynamics(IFunction)"/> 
        /// returns false for <paramref name="function"/>.</exception>
        public virtual string GetFilePathFromHydroDynamics(IFunction function)
        {
            if (HydroData == null || !HydroData.HasDataFor(function.Name))
            {
                throw new InvalidOperationException(string.Format("Function '{0}' is not available in the hydro data.", function.Name));
            }
            return HydroData.GetFilePathFor(function.Name);
        }

        /// <summary>
        /// Determines whether the given coordinate falls within an active cell or not.
        /// </summary>
        /// <returns>True if the cell is active; false when it's inactive.</returns>
        /// <exception cref="System.InvalidOperationException">When no hydro data has been importer.</exception>
        public virtual bool IsInsideActiveCell(Coordinate coordinate)
        {
            if (!HasHydroDataImported)
            {
                throw new InvalidOperationException("Cannot determine if location is inside active cell as no hydro dynamic data was imported.");
            }

            var index = GetSegmentIndexForLocation(coordinate);
            return attributeData.IsSegmentActive(index);
        }

        /// <summary>
        /// Determines whether the given coordinate falls within an active cell or not.
        /// </summary>
        /// <returns>True if the cell is active; false when it's inactive.</returns>
        /// <exception cref="System.InvalidOperationException">When no hydro data has been importer.</exception>
        public virtual bool IsInsideActiveCell2D(Coordinate coordinate)
        {
            if (!HasHydroDataImported)
            {
                throw new InvalidOperationException("Cannot determine if location is inside active cell as no hydro dynamic data was imported.");
            }

            var index = GetSegmentIndexForLocation2D(coordinate);
            return attributeData.IsSegmentActive(index);
        }

        /// <summary>
        /// Returns the cell index for a given location.
        /// </summary>
        public virtual int GetSegmentIndexForLocation(Coordinate coordinate)
        {
            if (!HasHydroDataImported)
            {
                throw new InvalidOperationException("Cannot determine grid cell index for location as no hydro dynamic data was imported.");
            }

            return pointToGridCellMapper.GetWaqSegmentIndex(coordinate.X, coordinate.Y, coordinate.Z);
        }

        /// <summary>
        /// Returns the cell index for a given location in 2D plane.
        /// So only the top layer.
        /// </summary>
        public virtual int GetSegmentIndexForLocation2D(Coordinate coordinate)
        {
            if (!HasHydroDataImported)
            {
                throw new InvalidOperationException("Cannot determine grid cell index for location as no hydro dynamic data was imported.");
            }

            return pointToGridCellMapper.GetWaqSegmentIndex2D(coordinate.X, coordinate.Y);
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            foreach (var directChild in base.GetDirectChildren())
            {
                yield return directChild;
            }

            yield return InitialConditions;
            yield return ProcessCoefficients;
            yield return Dispersion;
            yield return ObservationPoints;
            yield return Loads;
            yield return BoundaryDataManager;
            yield return LoadsDataManager;
        }

        public virtual double GetDefaultZ()
        {
            switch (LayerType)
            {
                case LayerType.Undefined:
                    return double.NaN;
                case LayerType.Sigma:
                    return 0;
                case LayerType.ZLayer:
                    return ZTop;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Restart file

        public virtual void ValidateInputState(out IEnumerable<string> errors, out IEnumerable<string> warnings)
        {
            try
            {
                var modelState = (ModelStateFilesImpl)modelStateHandler.CreateStateFromFile("validate", RestartInput.Path);

                ModelStateValidator.ValidateInputState(modelState, SupportedMetaDataVersions, GetMetaDataRequirements, GetOptionalMetaDataRequirements, "WaterQualityModel", out errors, out warnings);
            }
            catch (ArgumentException e)
            {
                errors = new[] { e.Message };
                warnings = Enumerable.Empty<string>();
            }
        }

        public virtual IModelState GetCopyOfCurrentState()
        {
            return modelStateHandler.GetState();
        }

        public virtual void SetState(IModelState modelState)
        {
            modelStateHandler.FeedStateToModel(modelState);
        }

        public virtual void ReleaseState(IModelState modelState)
        {
            modelStateHandler.ReleaseState(modelState);
        }

        public virtual IModelState CreateStateFromFile(string persistentStateFilePath)
        {
            return modelStateHandler.CreateStateFromFile(Name, persistentStateFilePath);
        }

        public virtual IEnumerable<DateTime> GetRestartWriteTimes()
        {
            if (UseSaveStateTimeRange)
            {
                var time = SaveStateStartTime;
                while (time <= SaveStateStopTime)
                {
                    yield return time;

                    time += SaveStateTimeStep;
                }
            }
        }

        public virtual void SaveStateToFile(IModelState modelState, string persistentStateFilePath)
        {
            modelState.MetaData = new ModelStateMetaData
            {
                ModelTypeId = "WaterQualityModel",
                Version = SupportedMetaDataVersions.Last(),
                Attributes = GetMetaDataRequirements(SupportedMetaDataVersions.Last())
            };
            modelStateHandler.SaveStateToFile(modelState, persistentStateFilePath);
        }

        #endregion

        # region Model

        protected override void OnInitialize()
        {
            Log.Info(KernelVersions);
            InvokeAndRestoreDirectory(OnInitializeCore);
        }

        private void OnInitializeCore()
        {
            ClearOutput();

            var validationReport = new WaterQualityModelValidator().Validate(this);
            if (validationReport.ErrorCount > 0)
            {
                throw new FormatException("Water quality model could not initialize. Please check the validation report.");
            }

            // a workaround to set the work directory on the model settings first
            SetValidWorkDirectory();

            FileUtils.CreateDirectoryIfNotExists(ModelSettings.WorkDirectory);
            FileUtils.CreateDirectoryIfNotExists(ModelSettings.OutputDirectory);

            waqInitializationSettings = WaqInitializationSettingsBuilder.BuildWaqInitializationSettings(this);

            // use the work directory to unzip the restart state to if use restart is true
            modelStateHandler.ModelWorkingDirectory = ModelSettings.WorkDirectory;

            if (UseRestart)
            {
                if (RestartInput.IsEmpty)
                {
                    throw new InvalidOperationException("Cannot use restart; restart empty!");
                }

                modelStateHandler.FeedStateToModel(modelStateHandler.CreateStateFromFile(Name, RestartInput.Path));
            }

            // use the output directory to find the files to zip if writerestart is true.
            modelStateHandler.ModelWorkingDirectory = ModelSettings.OutputDirectory;

            waqPreProcessor = new WaqFileBasedPreProcessor();
            waqPreProcessor.InitializeWaq(waqInitializationSettings, (displayName, filePath) => this.AddTextDocument(displayName, filePath));

            //initialize and fill initial values in output coverages (needs to be available after initialize for rtc to pick up, for example)
            waqProcessor = new WaqFileBasedProcessor();
            waqProcessor.Initialize(waqInitializationSettings);
        }

        protected override void OnExecute()
        {
            InvokeAndRestoreDirectory(OnExecuteCore);
        }

        private void OnExecuteCore()
        {
            waqProcessor.Process(waqInitializationSettings, SetProgress);
            CurrentTime = StopTime;

            Status = ActivityStatus.Done;

            OutputIsEmpty = false;
        }

        protected override void OnFinish()
        {
            if (ModelSettings.OutputDirectory == null)
            {
                Log.Error("Could not add output because work directory is empty.");
                return;
            }

            MapFileFunctionStore.Path = Path.Combine(ModelSettings.OutputDirectory, "deltashell.map");

            waqProcessor.AddOutput(ModelSettings.OutputDirectory, ObservationVariableOutputs, (displayName, filePath) => this.AddTextDocument(displayName, filePath), ModelSettings.MonitoringOutputLevel);
        }

        protected override void OnCleanup()
        {
            ClearPreProcessorAndProcessor();

            waqInitializationSettings = null;
            progressPercentage = 0.0;
        }

        protected override void OnClearOutput()
        {
            MapFileFunctionStore.Path = null;

            var outputDataItems = dataItems.Where(di => di.Role.HasFlag(DataItemRole.Output)).Select(di => di.Value).ToList();
            var outputCoverages = outputDataItems.OfType<UnstructuredGridCellCoverage>().ToList();
            var featureCoverages = outputDataItems.OfType<IFeatureCoverage>();
            var textDocuments = outputDataItems.OfType<TextDocument>();

            // Clear all output coverages
            foreach (var unstructuredGridCellCoverage in outputCoverages)
            {
                unstructuredGridCellCoverage.ClearCoverage();
            }

            // Clear all dynamic feature coverages
            foreach (var featureCoverage in featureCoverages)
            {
                featureCoverage.Filters.Clear();
                featureCoverage.Clear();
            }

            // Remove all text documents
            foreach (var textDocument in textDocuments)
            {
                DataItems.Remove(GetDataItemByValue(textDocument));
            }

            // Todo : Enable when monitoring output is added
            /*            // If relevant, clear the monitoring output data item time series
                        foreach (var observationVariableOutputTimeSeries in ObservationVariableOutputs.SelectMany(ovo => ovo.TimeSeriesList))
                        {
                            observationVariableOutputTimeSeries.Clear();
                        }*/
        }

        # endregion

        [EditAction]
        protected override void OnInputCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (!enableMarkOutputOutOfSync) return;

            this.InputCollectionChanged(sender, e);

            MarkOutputOutOfSync();
        }

        [EditAction]
        protected override void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!enableMarkOutputOutOfSync) return;

            this.InputPropertyChanged(sender, e);

            MarkOutputOutOfSync();
        }

        /// <summary>
        /// Occures when the hydro data has changed (file has been edited) (async event)
        /// </summary>
        public virtual event EventHandler<EventArgs> HydroDataChanged;

        private void SetNewGrid(UnstructuredGrid value, bool schematizationRemainsUnchanged)
        {
            // never set grid to null (this creates invalid UnstructuredGridCellCoverages)
            var gridToSet = value ?? new UnstructuredGrid();

            if (overriddenCoordinateSystem != null)
            {
                gridToSet.CoordinateSystem = overriddenCoordinateSystem;
            }

            GetDataItemByTag(GridTag).Value = gridToSet;
            Bathymetry = CreateAndFillBathymetryCoverage(gridToSet);

            ReplaceGridOnUnstructuredGridCoveragesWithSpatialOperations(GetDataItemSetByTag(InitialConditionsTag), schematizationRemainsUnchanged);
            ReplaceGridOnUnstructuredGridCoveragesWithSpatialOperations(GetDataItemSetByTag(ProcessCoefficientsTag), schematizationRemainsUnchanged);
            ReplaceGridOnUnstructuredGridCoveragesWithSpatialOperations(GetDataItemSetByTag(DispersionTag), schematizationRemainsUnchanged);
            ReplaceGridOnUnstructuredGridCoverageWithSpatialOperations(GetDataItemByTag(ObservationAreasTag), schematizationRemainsUnchanged);
            ReplaceGridOnUnstructuredGridCoverages(this.GetOutputCoverages(), schematizationRemainsUnchanged);
        }

        [EditAction]
        private void SetOutputDirectory(string value)
        {
            ModelSettings.OutputDirectory = value;
        }

        [EditAction]
        private void SettingExpliticWorkingDirectory(string directory)
        {
            ModelSettings.WorkDirectory = directory;
        }

        private void SubscribeToInternalEvents()
        {
            // subscribe to evented lists that are not in the DataItems collection
            if (Loads != null)
            {
                Loads.CollectionChanged += OnInputCollectionChanged;
            }
            if (ObservationPoints != null)
            {
                ObservationPoints.CollectionChanged += OnInputCollectionChanged;
            }

            if (ModelSettings != null)
            {
                ((INotifyPropertyChanged) ModelSettings).PropertyChanged += OnInputPropertyChanged;
            }
        }
      
        private string GetWaqDataFolderName()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return Path.GetFileName(tempWorkDirectory);
            }
            return Name.Replace(" ", "_");
        }

        [EditAction]
        private void SetWaqPointHeights()
        {
            var defaultZ = GetDefaultZ();

            foreach (var load in Loads)
            {
                load.Z = defaultZ;
            }
            foreach (var observationPoint in ObservationPoints)
            {
                observationPoint.Z = defaultZ;
            }
        }

        [EditAction]
        private void SetHorizontalDispersion(double value)
        {
            WaterQualityFunctionFactory.SetDefaultValue(Dispersion[0], value);
        }

        private void ClearPreProcessorAndProcessor()
        {
            waqProcessor = null;

            // clean the pre processor before setting it to null.
            if (waqPreProcessor != null)
            {
                waqPreProcessor.Dispose();
            }
            waqPreProcessor = null;
        }

        private void InitializeInputDataItems()
        {
            AddDataItem(new TextDocument { Name = "Input File", Content = Resources.TemplateInpFileNew }, DataItemRole.Input, InputFileCommandLineTag);
            AddDataItem(new TextDocument { Name = "Input File", Content = Resources.TemplateInpFileHybrid }, DataItemRole.Input, InputFileHybridTag);

            AddDataItem(CreateSubstanceProcessLibrary(), "Process Library", DataItemRole.Input, SubstanceProcessLibraryTag);

            var initialGrid = new UnstructuredGrid();
            AddDataItem(initialGrid, "Grid", DataItemRole.Input, GridTag);
            AddDataItem(CreateAndFillBathymetryCoverage(initialGrid), "Bed Level", DataItemRole.Input, BathymetryTag);
            AddDataItem(CreateObservationAreasCoverage(initialGrid), "Observation Areas", DataItemRole.Input, ObservationAreasTag);
            AddDataItem(new DataTableManager(), "Boundary Data", DataItemRole.Input, BoundaryDataTag);
            AddDataItem(new DataTableManager(), "Loads Data", DataItemRole.Input, LoadsDataTag);

            AddDataItemSet(new EventedList<IFunction>(), "Initial Conditions", DataItemRole.Input, InitialConditionsTag, true);
            AddDataItemSet(new EventedList<IFunction>(), "Process Coefficients", DataItemRole.Input, ProcessCoefficientsTag, true);
            AddDataItemSet(new EventedList<IFunction>(CreateDispersionFunctions()), "Horizontal Dispersion", DataItemRole.Input, DispersionTag, true);
        }

        private void SetImportProgress(string progress)
        {
            importProgress = progress;
            OnProgressChanged();
        }

        private void SetProgress(double progress)
        {
            progressPercentage = progress;
            OnProgressChanged();
        }

        private static SubstanceProcessLibrary CreateSubstanceProcessLibrary()
        {
            return new SubstanceProcessLibrary
            {
                Name = "Process Library",
                OutputParameters =
                               {
                                   new WaterQualityOutputParameter
                                       {
                                           Name = Resources.SubstanceProcessLibrary_OutputParameters_Volume,
                                           Description = Resources.SubstanceProcessLibrary_OutputParameters_Volume_description
                                       },
                                   new WaterQualityOutputParameter
                                       {
                                           Name = Resources.SubstanceProcessLibrary_OutputParameters_Surf,
                                           Description = Resources.SubstanceProcessLibrary_OutputParameters_Surf_description
                                       },
                                   new WaterQualityOutputParameter
                                       {
                                           Name = Resources.SubstanceProcessLibrary_OutputParameters_Temp,
                                           Description = Resources.SubstanceProcessLibrary_OutputParameters_Temp_description
                                       },
                                   new WaterQualityOutputParameter
                                       {
                                           Name = Resources.SubstanceProcessLibrary_OutputParameters_Rad,
                                           Description = Resources.SubstanceProcessLibrary_OutputParameters_Rad_description
                                       }
                               }
            };
        }

        private static IEnumerable<IFunction> CreateDispersionFunctions()
        {
            return new EventedList<IFunction> { WaterQualityFunctionFactory.CreateConst("Dispersion", 0, "Dispersion", "m2/s", "Horizontal Dispersion") };
        }

        private static UnstructuredGridVertexCoverage CreateAndFillBathymetryCoverage(UnstructuredGrid grid)
        {
            // create new bathymetry
            var bathymetry = new UnstructuredGridVertexCoverage(grid, false)
            {
                Name = "Bed Level",
                IsEditable = false,
            };
            bathymetry.Components[0].NoDataValue = -999.0;
            bathymetry.Components[0].DefaultValue = bathymetry.Components[0].NoDataValue;

            if (grid.Vertices.Count > 0)
                bathymetry.SetValues(grid.Vertices.Select(v => v.Z));

            return bathymetry;
        }

        private static WaterQualityObservationAreaCoverage CreateObservationAreasCoverage(UnstructuredGrid grid)
        {
            return new WaterQualityObservationAreaCoverage(grid);
        }

        private void SetValidWorkDirectory()
        {
            if (ModelSettings == null)
            {
                return;
            }
            // check if it is explicit, or if there is a project data directory
            if (ExplicitWorkingDirectory != null)
            {
                ModelSettings.WorkDirectory = ExplicitWorkingDirectory;
            }
            else if (ModelDataDirectory != null)
            {
                ModelSettings.WorkDirectory = Path.Combine(Path.GetDirectoryName(ModelDataDirectory), GetWaqDataFolderName() + "_output");
            }
            else
            {
                // use a folder that was created
                ModelSettings.WorkDirectory = Path.Combine(tempWorkDirectory, GetWaqDataFolderName() + "_output");
            }
        }

        /// <summary>
        /// Replace the grid on a list of coverages.
        /// This method does not look at spatial operations.
        /// 
        /// Use this method for output coverages for example.
        /// </summary>
        /// <param name="functions"></param>
        /// <param name="onlyUpdateGrid"></param>
        /// <seealso cref="ReplaceGridOnUnstructuredGridCoveragesWithSpatialOperations"/>
        private void ReplaceGridOnUnstructuredGridCoverages(IEnumerable<IFunction> functions, bool onlyUpdateGrid)
        {
            foreach (var function in functions)
            {
                var coverage = function as UnstructuredGridCellCoverage;
                if (coverage != null)
                {
                    coverage.AssignNewGridToCoverage(Grid, !onlyUpdateGrid);
                }
            }
        }

        private void ReplaceGridOnUnstructuredGridCoverageWithSpatialOperations(IDataItem dataItem, bool onlyUpdateGrid)
        {
            ReplaceGridOnUnstructuredGridCoverages(new[] { (IFunction)dataItem.Value }, onlyUpdateGrid);
            SetGridAndExecuteSpatialOperation(dataItem);
        }

        /// <summary>
        /// Replace grid on unstructured coverages, because the grid was replaced via the <see cref="Grid"/> property.
        /// This method checks spatial operations.
        /// 
        /// Use this method for functions that require spatial operation input.
        /// </summary>
        /// <seealso cref="ReplaceGridOnUnstructuredGridCoverages"/>.
        private void ReplaceGridOnUnstructuredGridCoveragesWithSpatialOperations(IDataItemSet dataItemSet, bool onlyUpdateGrid)
        {
            List<IFunction> functionListToReplace = new List<IFunction>();
            foreach (var dataItem in dataItemSet.DataItems)
            {
                if (dataItem.Value != null)
                {
                    functionListToReplace.Add((IFunction)dataItem.Value);
                }
                else if (dataItem.ComposedValue != null)
                {
                    functionListToReplace.Add((IFunction)dataItem.ComposedValue);
                }
            }

            ReplaceGridOnUnstructuredGridCoverages(functionListToReplace, onlyUpdateGrid);

            foreach (var dataItem in dataItemSet.DataItems)
            {
                SetGridAndExecuteSpatialOperation(dataItem);
            }
        }

        private void SetGridAndExecuteSpatialOperation(IDataItem dataItem)
        {
            // Existing input grid cell coverages often have a 'initial value' Spatial Operation, created due to WaterQualityModelSyncExtensions.
            // We need to update the original coverage in order to have the Spatial Operations working with the new grid.
            var valueConverter = dataItem.ValueConverter as SpatialOperationSetValueConverter;
            if (valueConverter == null)
            {
                return;
            }

            var original = (UnstructuredGridCellCoverage)valueConverter.OriginalValue;
            original.AssignNewGridToCoverage(Grid);

            // set the grid extents as mask for the first operation, because it depends on the grid.
            var operation = valueConverter.SpatialOperationSet.Operations.FirstOrDefault(
                o => o.Name == WaterQualityModelSyncExtensions.InitialValueOperationName);

            if (operation != null)
            {
                WaterQualityModelSyncExtensions.SetGridExtentsAsInputMask(operation, original);
            }

            // execute the spatial operation set
            // this call is not required during loading of the project,
            // but it is required when the hyd file is re-imported in any other case.
            // See TOOLS-22124 for more info
            valueConverter.SpatialOperationSet.Execute();
        }

        private Dictionary<string, string> GetMetaDataRequirements(int version)
        {
            if (version != 1)
                throw new NotImplementedException(String.Format("Meta data version {0} for model type {1} is not supported", version, "WaterQualityModel"));

            return new Dictionary<string, string>
                {
                    {"CorrectForEvap", ModelSettings.CorrectForEvaporation.ToString()}
                };
        }

        private Dictionary<string, string> GetOptionalMetaDataRequirements(int version)
        {
            if (version != 1)
                throw new NotImplementedException(String.Format("Meta data version {0} for model type {1} is not supported", version, "WaterQualityModel"));

            return new Dictionary<string, string>
                    {
                        {"NrOfActiveSubstances", SubstanceProcessLibrary.ActiveSubstances.Count().ToString(CultureInfo.InvariantCulture)}
                    };
        }

        private void HandleNewHydroDynamicsFunctionDataSet(IDataItemSet functionCollection, string functionName)
        {
            var dataItem = functionCollection.DataItems.FirstOrDefault(p => p.Name == functionName);
            if (dataItem == null || SubstanceProcessLibrary == null) return;

            var function = GetFunctionForDataItem(dataItem);

            var hasDataInHydroDynamics = HasDataInHydroDynamics(functionName);
            var isFromHydroDynamics = function.IsFromHydroDynamics();

            IFunctionTypeCreator creator;
            if (hasDataInHydroDynamics && !HasEverImportedHydroData) // if there is data and it is the first time, automatically set it to from hydrodynamics. 
            {
                creator = FunctionTypeCreatorFactory.CreateFunctionFromHydroDynamicsCreator(HasDataInHydroDynamics, GetFilePathFromHydroDynamics);
            }
            else if (!hasDataInHydroDynamics && isFromHydroDynamics) // if there is no data in the hydrodynamics, but it is set as such, set it back to constant
            {
                creator = FunctionTypeCreatorFactory.CreateConstantCreator();
            }
            else
            {
                return;
            }

            FunctionTypeCreator.ReplaceFunctionUsingCreator(functionCollection.AsEventedList<IFunction>(), function, creator, this);
        }

        private IFunction GetFunctionForDataItem(IDataItem dataItem)
        {
            var function = dataItem.Value as IFunction ?? dataItem.ValueConverter.OriginalValue as IFunction;
            Debug.Assert(function != null,
                "Assumption: If DataItem.Value should return null here, we are dealing with " +
                "an UnstructuredGridCellCoverage which uses ValueConverters and hasn't executed " +
                "yet such as during save/load cycle.");
            return function;
        }

        private void HydroDataOnDataChanged(object sender, EventArgs<string> eventArgs)
        {
            if (HydroDataChanged == null) return;
            HydroDataChanged(this, eventArgs);
        }

        public void Dispose()
        {
            HydroData = null;
        }

        public virtual void SetEnableMarkOutputOutOfSync(bool enableMarkOutputOutOfSyncValue)
        {
            enableMarkOutputOutOfSync = enableMarkOutputOutOfSyncValue;
        }
    }
}
