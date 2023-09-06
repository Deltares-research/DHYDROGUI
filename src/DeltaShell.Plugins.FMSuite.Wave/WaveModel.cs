using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BasicModelInterface;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.CommonTools.TextData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using DeltaShell.Plugins.FMSuite.Common.IO.Writers;
using DeltaShell.Plugins.FMSuite.Wave.Api;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Exporters;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using DHYDRO.Common.Logging;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    /// <summary>
    /// <see cref="WaveModel"/> implements the model interfaces for the Wave plugin.
    /// </summary>
    /// <seealso cref="TimeDependentModelBase" />
    /// <seealso cref="IDisposable" />
    /// <seealso cref="IGridOperationApi" />
    /// <seealso cref="IWaveModel" />
    /// <seealso cref="IHydroModel" />
    /// <seealso cref="IDimrModel" />
    [Entity]
    public class WaveModel : TimeDependentModelBase,
                             IDisposable,
                             IGridOperationApi,
                             IWaveModel,
                             IHydroModel,
                             IDimrModel
    {
        // Also add model specific data items to the exclude list in <see cref="BuildModel"/>
        private static readonly ILog log = LogManager.GetLogger(typeof(WaveModel));

        private static readonly string GridPropertyName = nameof(WaveDomainData.Grid);
        private readonly BoundaryContainerSyncService boundaryContainerSyncService;

        private readonly DimrRunner runner;
        private ICoordinateSystem coordinateSystem;
        private string progressText;

        private IWaveDomainData outerDomain;

        private IGridOperationApi gridOperationApi;
        private double previousProgress = 0;

        /// <summary>
        /// Creates a new empty <see cref="WaveModel"/>.
        /// </summary>
        public WaveModel() : this(BuildEmptyModel, false) { }

        /// <summary>
        /// Creates a new <see cref="WaveModel"/> from the provided <paramref name="mdwFilePath"/>.
        /// </summary>
        /// <param name="mdwFilePath">The path to the mdw file.</param>
        /// <param name="connectToOutput">Whether to attempt to connect the output or not.</param>
        public WaveModel(string mdwFilePath, bool connectToOutput = true) :
            this(model => BuildModelFromMdw(model, mdwFilePath),
                 connectToOutput)
        {
        }

        private WaveModel(Action<WaveModel> creationCode, bool connectToOutput) : base("Waves")
        {
            runner = new DimrRunner(this, new DimrApiFactory());
            TimeFrameData = new TimeFrameData();

            creationCode(this);

            SynchronizeOuterDomainWithModelDefinition();

            InitializeWaveOutputObjects();

            ShowModelRunConsole = false;
            ValidateBeforeRun = true;

            WaveDomainHelper.GetAllDomains(OuterDomain).ForEach(SyncWithModelDefaults);
            gridOperationApi = new WaveGridOperationApi(OuterDomain.Grid);

            ((INotifyPropertyChanged)this).PropertyChanged += (s, e) => MarkDirty();
            ((INotifyCollectionChanged)this).CollectionChanged += (s, e) => MarkDirty();

            InitializeCouplingTime();

            if (connectToOutput)
            {
                InitializeWaveOutputData();
            }

            boundaryContainerSyncService = new BoundaryContainerSyncService(this);
#pragma warning disable 618
            BoundariesFromBoundaryContainer = BoundaryContainer.Boundaries;
#pragma warning restore 618
        }

        private void InitializeWaveOutputObjects()
        {
            OutputDiagnosticFiles = new EventedList<ReadOnlyTextFileData>();
            OutputSpectraFiles = new EventedList<ReadOnlyTextFileData>();
            OutputSwanFiles = new EventedList<ReadOnlyTextFileData>();
            OutputWavmFileFunctionStores = new EventedList<IWavmFileFunctionStore>();
            OutputWavhFileFunctionStores = new EventedList<IWavhFileFunctionStore>();

            WaveOutputData = new WaveOutputData(new WaveOutputDataHarvester(FeatureContainer),
                                                new WaveOutputDataCopyHandler());

            WaveOutputData.DiagnosticFiles.CollectionChanged +=
                SyncHelper.GetSyncNotifyCollectionChangedEventHandler(OutputDiagnosticFiles);
            WaveOutputData.SpectraFiles.CollectionChanged +=
                SyncHelper.GetSyncNotifyCollectionChangedEventHandler(OutputSpectraFiles);
            WaveOutputData.SwanFiles.CollectionChanged +=
                SyncHelper.GetSyncNotifyCollectionChangedEventHandler(OutputSwanFiles);
            WaveOutputData.WavmFileFunctionStores.CollectionChanged +=
                SyncHelper.GetSyncNotifyCollectionChangedEventHandler(OutputWavmFileFunctionStores);
            WaveOutputData.WavhFileFunctionStores.CollectionChanged +=
                SyncHelper.GetSyncNotifyCollectionChangedEventHandler(OutputWavhFileFunctionStores);
        }

        private void SynchronizeOuterDomainWithModelDefinition()
        {
            if (!Equals(OuterDomain, ModelDefinition.OuterDomain))
            {
                OuterDomain = ModelDefinition.OuterDomain;
            }

            if (OuterDomain?.Grid != null)
            {
                UpdateCoordinateSystem(OuterDomain.Grid.CoordinateSystem);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this wave model is online coupled to a fm model.
        /// Always true for wave model inside an integrated model, since waves models can
        /// not run stand-alone in DIMR.
        /// </summary>
        public bool IsCoupledToFlow { get; set; }

        /// <summary>
        /// Gets the model definition.
        /// </summary>
        public WaveModelDefinition ModelDefinition
        {
            get => modelDefinition;
            private set
            {
                if (modelDefinition != null)
                {
                    ((INotifyPropertyChange)modelDefinition.Properties).PropertyChanged -=
                        OnModelDefinitionPropertyChanged;
                    UnsubscribeFromFeatureContainer();
                }

                modelDefinition = value;
                if (modelDefinition != null)
                {
                    ((INotifyPropertyChange)modelDefinition.Properties).PropertyChanged +=
                        OnModelDefinitionPropertyChanged;
                    SubscribeToFeatureContainer();
                }
            }
        }

        #region ModelDefinition Properties
        public int SimulationMode
        {
            get => (int)ModelDefinition
                         .GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.SimulationMode)
                         .Value;
            set
            {
                // stationary, quasi-stationary, non-stationary. Used for event bubbling.
                // doesn't do anything, used for events
            }
        }

        public int DirectionalSpaceType
        {
            get => (int)ModelDefinition
                         .GetModelProperty(KnownWaveSections.GeneralSection,
                                           KnownWaveProperties.DirectionalSpaceType).Value;
            set
            {
                // doesn't do anything, used for events
            }
        }

        public bool WriteCOM
        {
            get => (bool)ModelDefinition
                          .GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.WriteCOM).Value;
            set
            {
                // only used for event bubbling
            }
        }

        public bool WriteTable
        {
            get => (bool)ModelDefinition
                          .GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.WriteTable).Value;
            set
            {
                // only used for event bubbling
            }
        }

        public bool MapWriteNetCDF
        {
            get => (bool)ModelDefinition
                          .GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.MapWriteNetCDF)
                          .Value;
            set
            {
                // only used for event bubbling
            }
        }

        public bool Breaking
        {
            get => (bool)ModelDefinition
                          .GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.Breaking).Value;
            set
            {
                // only used for event bubbling
            }
        }

        public bool Triads
        {
            get => (bool)ModelDefinition
                          .GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.Triads).Value;
            set
            {
                // only used for event bubbling
            }
        }

        public bool Diffraction
        {
            get => (bool)ModelDefinition
                          .GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.Diffraction)
                          .Value;
            set
            {
                // only used for event bubbling
            }
        }

        public int BedFriction
        {
            get =>
                (int)
                ModelDefinition.GetModelProperty(KnownWaveSections.ProcessesSection,
                                                 KnownWaveProperties.BedFriction).Value;
            set
            {
                // only used for event bubbling
            }
        }

        public bool WaveSetup
        {
            get => ModelDefinition.WaveSetup;
            set
            {
                //only used for event bubbling
            }
        }

        public IBoundaryContainer BoundaryContainer => ModelDefinition.BoundaryContainer;

        public IWaveFeatureContainer FeatureContainer => ModelDefinition.FeatureContainer;

        private void SubscribeToFeatureContainer()
        {
            FeatureContainer.ObservationCrossSections.CollectionChanged +=
                OnFeatureContainerCollectionChanged;
            FeatureContainer.ObservationPoints.CollectionChanged +=
                OnFeatureContainerCollectionChanged;
            FeatureContainer.Obstacles.CollectionChanged +=
                OnFeatureContainerCollectionChanged;
        }

        private void UnsubscribeFromFeatureContainer()
        {
            FeatureContainer.ObservationCrossSections.CollectionChanged -=
                OnFeatureContainerCollectionChanged;
            FeatureContainer.ObservationPoints.CollectionChanged -=
                OnFeatureContainerCollectionChanged;
            FeatureContainer.Obstacles.CollectionChanged -=
                OnFeatureContainerCollectionChanged;
        }

        private void OnFeatureContainerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => MarkDirty();

        #endregion

        /// <summary>
        /// Only used for bubbling events for updating project tree. Don't remove the setter.
        /// It should be public.
        /// </summary>
        [Obsolete("Use BoundaryContainer.Boundaries")]
        public IEventedList<IWaveBoundary> BoundariesFromBoundaryContainer { get; set; }

        public IWaveDomainData OuterDomain
        {
            get => outerDomain;
            set
            {
                if (outerDomain != null)
                {
                    ((INotifyPropertyChanged)outerDomain).PropertyChanged -= OnOuterDomainPropertyChanged;
                    RemoveDataItemsForDomain(outerDomain);
                }

                outerDomain = value;
                ModelDefinition.OuterDomain = outerDomain;

                if (outerDomain != null)
                {
                    ((INotifyPropertyChanged)outerDomain).PropertyChanged += OnOuterDomainPropertyChanged;
                    AddDataItemsForDomain(outerDomain);

                    gridOperationApi = new WaveGridOperationApi(outerDomain.Grid);
                }
            }
        }

        private void InitializeWaveOutputData()
        {
            if (OutputDirPath == null ||
                !Directory.Exists(OutputDirPath) ||
                !Directory.EnumerateFiles(OutputDirPath).Any())
            {
                return;
            }

            var logHandler = new LogHandler(Resources.WaveModel_Connect_model_output, log);
            WaveOutputData.ConnectTo(OutputDirPath, false, logHandler);
            logHandler.LogReport();

            OutputIsEmpty = false;
        }

        // Note that the private set here and the assignment in the 
        // constructor are required for PostSharp to properly propagate the 
        // changes in the WaveOutputData.        
        /// <summary>
        /// Gets the <see cref="IWaveOutputData" /> of this <see cref="WaveModel" />
        /// </summary>
        public IWaveOutputData WaveOutputData { get; private set; }

        /// <summary>
        /// Gets the <see cref="ITimeFrameData"/> of this <see cref="WaveModel"/>.
        /// </summary>
        public ITimeFrameData TimeFrameData { get; private set; }

        #region OutputData Properties
        // Note: The following properties have been exposed to ensure event propagation
        // works correctly, and should not be used directly.

        /// <summary>
        /// Gets the output diagnostic files.
        /// </summary>
        /// <remarks>
        /// This <see cref="IEventedList{ReadOnlyTextFileData}"/> is synced
        /// with <see cref="WaveOutputData"/> diagnostic files. However any
        /// changes to this evented list will *not* be reflected in the
        /// output data. As such it is strongly recommended to use the
        /// <see cref="WaveOutputData"/> directly.
        /// </remarks>
        [Aggregation]
        public IEventedList<ReadOnlyTextFileData> OutputDiagnosticFiles { get; private set; }

        /// <summary>
        /// Gets the output spectra files.
        /// </summary>
        /// <remarks>
        /// This <see cref="IEventedList{ReadOnlyTextFileData}"/> is synced
        /// with <see cref="WaveOutputData"/> spectra files. However any
        /// changes to this evented list will *not* be reflected in the
        /// output data. As such it is strongly recommended to use the
        /// <see cref="WaveOutputData"/> directly.
        /// </remarks>
        [Aggregation]
        public IEventedList<ReadOnlyTextFileData> OutputSpectraFiles { get; private set; }
        
        /// <summary>
        /// Gets the output SWAN files.
        /// </summary>
        /// <remarks>
        /// This <see cref="IEventedList{ReadOnlyTextFileData}"/> is synced
        /// with <see cref="WaveOutputData"/> SWAN files. However any
        /// changes to this evented list will *not* be reflected in the
        /// output data. As such it is strongly recommended to use the
        /// <see cref="WaveOutputData"/> directly.
        /// </remarks>
        [Aggregation]
        public IEventedList<ReadOnlyTextFileData> OutputSwanFiles { get; private set; }

        /// <summary>
        /// Gets the output <see cref="WavmFileFunctionStore"/> objects.
        /// </summary>
        /// <remarks>
        /// This <see cref="IEventedList{WavmFileFunctionStore}"/> is synced
        /// with <see cref="WaveOutputData"/> <see cref="WavmFileFunctionStore"/>
        /// objects. However any changes to this evented list will *not*
        /// be reflected in the output data. As such it is strongly recommended
        /// to use the <see cref="WaveOutputData"/> directly.
        /// </remarks>
        [Aggregation]
        public IEventedList<IWavmFileFunctionStore> OutputWavmFileFunctionStores { get; private set; }

        /// <summary>
        /// Gets the output <see cref="WavhFileFunctionStore"/> objects.
        /// </summary>
        /// <remarks>
        /// This <see cref="IEventedList{WavhFileFunctionStore}"/> is synced
        /// with <see cref="WaveOutputData"/> <see cref="WavhFileFunctionStore"/>
        /// objects. However any changes to this evented list will *not*
        /// be reflected in the output data. As such it is strongly recommended
        /// to use the <see cref="WaveOutputData"/> directly.
        /// </remarks>
        [Aggregation]
        public IEventedList<IWavhFileFunctionStore> OutputWavhFileFunctionStores { get; private set; }

        #endregion

        public MdwFile MdwFile { get; } = new MdwFile();

        [PropertyGrid]
        [DisplayName("Validate before run")]
        [Category("Run mode")]
        public bool ValidateBeforeRun { get; set; }

        [PropertyGrid]
        [DisplayName("Show model run console")]
        [Category("Run mode")]
        public bool ShowModelRunConsole { get; set; }

        /// <summary>
        /// Gets or sets the function to retrieve the working directory path.
        /// </summary>
        public Func<string> WorkingDirectoryPathFunc { get; set; } = () => DefaultModelSettings.DefaultDeltaShellWorkingDirectory;

        public IHydroRegion Region => null;

        /// <summary>
        /// Showing the progress of a run.
        /// </summary>
        public override string ProgressText => string.IsNullOrEmpty(progressText) ? base.ProgressText : progressText;

        public ICoordinateSystem CoordinateSystem
        {
            get => coordinateSystem;
            set
            {
                coordinateSystem = value;
                AfterCoordinateSystemSet();
            }
        }

        public override DateTime StartTime
        {
            get => startTime;
            set
            {
                startTime = value;
                // This base model setting is made to make the base logic right
                base.StartTime = value;
            }
        }

        public override DateTime StopTime
        {
            get => stopTime;
            set
            {
                stopTime = value;
                // This base model setting is made to make the base logic right
                base.StopTime = value;
            }
        }

        public override TimeSpan TimeStep
        {
            get => timeStep;
            set
            {
                timeStep = value;
                // This base model setting is made to make the base logic right
                base.TimeStep = value;
            }
        }

        public override IBasicModelInterface BMIEngine => runner.Api;

        public void AddSubDomain(IWaveDomainData domain, IWaveDomainData subDomain)
        {
            domain.SubDomains.Add(subDomain);
            subDomain.SuperDomain = domain;
            AddDataItemsForDomain(subDomain);
            AfterCoordinateSystemSet();
        }

        public void DeleteSubDomain(IWaveDomainData domain, WaveDomainData subDomain)
        {
            domain.SubDomains.Remove(subDomain);
            RemoveDataItemsForDomain(subDomain);
        }

        public string ImportIntoModelDirectory(string filePath) =>
            WaveModelFileHelper.ImportIntoModelDirectory(InputDirPath, filePath);

        public void SyncWithModelDefaults(IWaveDomainData domain)
        {
            // only when set to default, we shouldn't overwrite domain-parameters
            SpectralDomainData spectral = domain.SpectralDomainData;
            if (spectral.UseDefaultDirectionalSpace)
            {
                spectral.DirectionalSpaceType = ModelDefinition.DefaultDirectionalSpaceType;
                spectral.NDir = ModelDefinition.DefaultNumberOfDirections;
                spectral.StartDir = ModelDefinition.DefaultStartDirection;
                spectral.EndDir = ModelDefinition.DefaultEndDirection;
            }

            if (spectral.UseDefaultFrequencySpace)
            {
                spectral.NFreq = ModelDefinition.DefaultNumberOfFrequencies;
                spectral.FreqMin = ModelDefinition.DefaultStartFrequency;
                spectral.FreqMax = ModelDefinition.DefaultEndFrequency;
            }

            HydroFromFlowSettings hydro = domain.HydroFromFlowData;
            if (hydro.UseDefaultHydroFromFlowSettings)
            {
                hydro.BedLevelUsage = ModelDefinition.DefaultBedLevelUsage;
                hydro.WaterLevelUsage = ModelDefinition.DefaultWaterLevelUsage;
                hydro.VelocityUsage = ModelDefinition.DefaultVelocityUsage;
                hydro.VelocityUsageType = ModelDefinition.DefaultVelocityUsageType;
                hydro.WindUsage = ModelDefinition.DefaultWindUsage;
            }
        }

        /// <summary>
        /// Load the grid specified in <paramref name="domain"/> from the <paramref name="workingDirectory"/>.
        /// </summary>
        /// <param name="workingDirectory"> The working directory. </param>
        /// <param name="domain"> The domain. </param>
        /// <remarks>
        /// If no file exists, a default grid will be created.
        /// </remarks>
        public static void LoadGrid(string workingDirectory, IWaveDomainData domain)
        {
            string grdFilePath = Path.Combine(workingDirectory, domain.GridFileName);

            CurvilinearGrid grid = File.Exists(grdFilePath)
                                       ? Delft3DGridFileReader.Read(grdFilePath)
                                       : CurvilinearGrid.CreateDefault();
            grid.Name = $"Grid ({Path.GetFileNameWithoutExtension(grdFilePath)})";
            grid.CoordinateSystem = grid.Attributes[CurvilinearGrid.CoordinateSystemKey] == "Spherical"
                                        ? new OgrCoordinateSystemFactory().CreateFromEPSG(4326)
                                        : null;
            domain.Grid = grid;
        }

        /// <summary>
        /// Load the bathymetry of the specified <paramref name="domain"/> and
        /// update the <paramref name="model"/>.
        /// </summary>
        /// <param name="model"> The model which will be updated. </param>
        /// <param name="directory"> The working directory in which the Bathymetry of the <paramref name="domain"/> resides. </param>
        /// <param name="domain"> The domain. </param>
        /// <remarks>
        /// If no file of domain.BedLevelFileName exists in the <paramref name="directory"/> then
        /// an error is logged, and the an empty bathymetry will be loaded on the <paramref name="domain"/>.
        /// </remarks>
        public static void LoadBathymetry(WaveModel model, string directory, IWaveDomainData domain)
        {
            CurvilinearGrid grid = domain.Grid;
            var bathymetry = new CurvilinearCoverage(grid.Size1, grid.Size2, grid.X.Values, grid.Y.Values) { Name = $"Bathymetry ({Path.GetFileNameWithoutExtension(domain.BedLevelFileName)})" };
            bathymetry.Components[0].NoDataValue = -999.0;
            bathymetry.Components[0].DefaultValue = bathymetry.Components[0].NoDataValue;

            string depthFilePath = Path.Combine(directory, domain.BedLevelFileName);
            if (File.Exists(depthFilePath) && !grid.IsEmpty)
            {
                List<double> bathymetryValues =
                    Delft3DDepthFileReader.Read(depthFilePath, grid.Size1, grid.Size2).ToList();

                if (bathymetryValues.Count != grid.Size2 * grid.Size1)
                {
                    log.ErrorFormat(
                        "Failed to load bathymetry; data in file does not match the size of the target grid: {0}x{1}",
                        grid.Size1, grid.Size2);
                    return;
                }

                bathymetry.SetValues(bathymetryValues);
            }

            IDataItem di = model.DataItems.FirstOrDefault(d => d.Name == domain.Bathymetry.Name);

            domain.Bathymetry = bathymetry;
            if (di != null)
            {
                di.Value = domain.Bathymetry;
                if (di.ValueConverter is SpatialOperationSetValueConverter converter)
                {
                    converter.OriginalValue = domain.Bathymetry.Clone();
                }
            }
        }

        public static bool IsValidCoordinateSystem(ICoordinateSystem system) =>
            !system.IsGeographic || system.Name == "WGS 84";

        // all saving should go through here, but beware, NHibernate will disable
        // event bubbling when saving...
        public void ModelSaveTo(string targetMdwFilePath, bool switchTo)
        {
            string targetDir = Path.GetDirectoryName(targetMdwFilePath);
            string modelDir = Path.GetDirectoryName(targetDir);

            if (modelDir == null)
            {
                throw new InvalidOperationException("Model cannot be directly saved under the root.");
            }

            if (switchTo)
            {
                string outputDir = Path.Combine(modelDir, "output");
                SaveModelOutputStateTo(outputDir);
            }

            ExportModelInputTo(targetMdwFilePath, switchTo);
        }

        /// <summary>
        /// Exports the model input to the specified <paramref name="mdwFilePath"/>.
        /// </summary>
        /// <param name="mdwFilePath">The target mdw file path.</param>
        /// <param name="switchTo">Whether or not the model and the data should be switched to the new location.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="mdwFilePath"/> is <c>null</c>.
        /// </exception>
        public void ExportModelInputTo(string mdwFilePath, bool switchTo = false)
        {
            Ensure.NotNullOrEmpty(mdwFilePath, nameof(mdwFilePath));

            string targetDir = Path.GetDirectoryName(mdwFilePath);
            FileUtils.CreateDirectoryIfNotExists(targetDir);

            MdwFile.SaveTo(mdwFilePath, new MdwFileDTO(ModelDefinition, TimeFrameData), switchTo);

            // write spatial data:
            SaveBathymetries(WaveDomainHelper.GetAllDomains(OuterDomain), targetDir);
        }

        private void SaveModelOutputStateTo(string outputTargetDirectory)
        {
            Ensure.NotNullOrEmpty(outputTargetDirectory, nameof(outputTargetDirectory));

            var logHandler = new LogHandler(Resources.WaveModel_Saving_of_the_model_output, log);

            var targetDirectoryInfo = new DirectoryInfo(outputTargetDirectory);
            FileUtils.CreateDirectoryIfNotExists(targetDirectoryInfo.FullName);

            if (!WaveOutputData.IsConnected)
            {
                ClearDirectory(targetDirectoryInfo);
            }
            else if (WaveOutputData.IsStoredInWorkingDirectory ||
                     !IsSavedToCurrentOutputDirectory(targetDirectoryInfo))
            {
                WaveOutputData.SwitchTo(targetDirectoryInfo.FullName, logHandler);
            }

            logHandler.LogReport();
        }

        private bool IsSavedToCurrentOutputDirectory(FileSystemInfo targetDirectoryInfo) =>
            GetPreviousOutputDirPath() == targetDirectoryInfo.FullName;

        private static void ClearDirectory(DirectoryInfo directoryInfo) =>
            FileUtils.CreateDirectoryIfNotExists(directoryInfo.FullName, true);

        /// <summary>
        /// Reloads all grids associated with each domain.
        /// </summary>
        public void ReloadAllGrids()
        {
            BeginEdit(new DefaultEditAction("Reload all grids"));
            try
            {
                WaveDomainHelper.GetAllDomains(OuterDomain).ForEach(LoadWaveDomain);
            }
            finally
            {
                EndEdit();
            }
        }

        public override IProjectItem DeepClone()
        {
            string tempDir = FileUtils.CreateTempDirectory();
            string fileName = Path.GetFileName(MdwFilePath);
            string tempFilePath = Path.Combine(tempDir, fileName);
            ModelSaveTo(tempFilePath, false);

            return new WaveModel(tempFilePath);
        }

        public virtual ValidationReport Validate() =>
            new WaveModelValidator().Validate(this);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RestoreEnvironment();
                runner?.Dispose();
            }
        }

        public IGeometry GetGridSnappedGeometry(string featureType, IGeometry geometry)
        {
            return gridOperationApi != null ? gridOperationApi.GetGridSnappedGeometry(featureType, geometry) : geometry;
        }

        public IEnumerable<IGeometry> GetGridSnappedGeometry(string featureType, ICollection<IGeometry> geometries)
        {
            throw new NotImplementedException();
        }

        public bool SnapsToGrid(IGeometry geometry)
        {
            throw new NotImplementedException();
        }

        public int[] GetLinkedCells()
        {
            throw new NotImplementedException();
        }

        public bool CanSetCoordinateSystem(ICoordinateSystem potentialCoordinateSystem)
        {
            return WaveModelCoordinateConversion.IsSaneCoordinateSystemForModel(this, potentialCoordinateSystem);
        }

        public void TransformCoordinates(ICoordinateTransformation transformation)
        {
            BeginEdit(new DefaultEditAction("Converting model coordinates"));

            WaveModelCoordinateConversion.Transform(this, transformation);
            CoordinateSystem = (ICoordinateSystem)transformation.TargetCS;

            EndEdit();

            // grid(s) transformed, sync data to disk:
            foreach (IWaveDomainData domain in WaveDomainHelper.GetAllDomains(OuterDomain))
            {
                string targetGridFileName = Path.Combine(InputDirPath, domain.GridFileName);
                Delft3DGridFileWriter.Write(domain.Grid, targetGridFileName);
            }
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            foreach (object item in base.GetDirectChildren())
            {
                yield return item;
            }

            yield return TimeFrameData;

            foreach (IWaveDomainData domain in WaveDomainHelper.GetAllDomains(ModelDefinition.OuterDomain))
            {
                yield return domain.Grid;
                yield return domain.Bathymetry;
            }

            foreach (IWaveBoundary boundary in BoundaryContainer.Boundaries)
            {
                yield return boundary;
            }

            yield return WaveOutputData;

            foreach (ReadOnlyTextFileData diagnosticFile in WaveOutputData.DiagnosticFiles)
            {
                yield return diagnosticFile;
            }

            foreach (ReadOnlyTextFileData spectraFile in WaveOutputData.SpectraFiles)
            {
                yield return spectraFile;
            }
            
            foreach (ReadOnlyTextFileData swanFile in WaveOutputData.SwanFiles)
            {
                yield return swanFile;
            }

            foreach (IWavmFileFunctionStore wavmFileFunctionStore in WaveOutputData.WavmFileFunctionStores)
            {
                yield return wavmFileFunctionStore;
            }

            foreach (IWavhFileFunctionStore wavhFileFunctionStore in WaveOutputData.WavhFileFunctionStores)
            {
                yield return wavhFileFunctionStore;

                foreach (IFunction function in wavhFileFunctionStore.Functions)
                {
                    yield return function;
                }
            }
        }

        protected override void OnReset()
        {
            base.OnReset();
            ReportProgressText(); // Reset the progress text
        }

        protected override void OnInitialize()
        {
            previousProgress = 0;

            ReportProgressText("Initializing");

            DisconnectOutput();

            if (DimrExportDirectoryPath != null)
            {
                FileUtils.CreateDirectoryIfNotExists(DimrExportDirectoryPath, true);
            }

            runner.OnInitialize();

            ReportProgressText();
        }

        protected override void OnExecute()
        {
            runner.OnExecute();
        }

        protected override void OnFinish()
        {
            base.OnFinish();
            runner.OnFinish();
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();
            runner.OnCleanup();

            ReportProgressText();
        }

        protected override void OnProgressChanged()
        {
            // Only update gui for every 1 percent progress (performance)
            if (ProgressPercentage - previousProgress < 0.01)
            {
                return;
            }

            previousProgress = ProgressPercentage;
            runner.OnProgressChanged();
            base.OnProgressChanged();
        }

        internal void SyncModelTimesWithBase()
        {
            base.StartTime = StartTime;
            base.StopTime = StopTime;
            base.TimeStep = TimeStep;
        }

        private string InputDirPath => Path.GetDirectoryName(MdwFilePath);

        private string OutputDirPath
        {
            get
            {
                if (string.IsNullOrEmpty(InputDirPath))
                {
                    return null;
                }

                string outputDir = Path.Combine(InputDirPath, "..", "output");
                return Path.GetFullPath(outputDir);
            }
        }

        private string GetPreviousOutputDirPath()
        {
            if (PreviousMdwPath == null)
            {
                return null;
            }

            string outputDir = Path.Combine(PreviousMdwPath, "..", "..", "output");
            return Path.GetFullPath(outputDir);
        }

        [EditAction]
        private void RemoveDataItemsForDomain(IWaveDomainData domain)
        {
            foreach (IWaveDomainData subDomain in WaveDomainHelper.GetAllDomains(domain))
            {
                DataItems.RemoveAllWhere(di => Equals(subDomain.Bathymetry, di.Value));
            }
        }

        [EditAction]
        private void AddDataItemsForDomain(IWaveDomainData domain)
        {
            foreach (IWaveDomainData subDomain in WaveDomainHelper.GetAllDomains(domain))
            {
                DataItems.Add(new DataItem(subDomain.Bathymetry, DataItemRole.Input));
            }
        }

        [EditAction]
        private void ReplaceDataItemsForDomain(IWaveDomainData newDomainData)
        {
            foreach (IWaveDomainData subDomain in WaveDomainHelper.GetAllDomains(newDomainData))
            {
                foreach (IDataItem dataItem in DataItems)
                {
                    if (dataItem.Name == subDomain.Bathymetry.Name)
                    {
                        dataItem.Value = subDomain.Bathymetry;
                    }
                }
            }
        }

        private void OnOuterDomainPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (!(sender is WaveDomainData domain) || eventArgs.PropertyName != GridPropertyName)
            {
                return;
            }

            if (Equals(OuterDomain, domain))
            {
                gridOperationApi = new WaveGridOperationApi(OuterDomain.Grid);
            }

            UpdateBathymetry(domain);
            UpdateBathymetryOperations(domain);

            if (domain.Grid != null)
            {
                UpdateCoordinateSystem(domain.Grid.CoordinateSystem);
            }

            if (domain.Grid != null)
            {
                CoordinateSystem = domain.Grid.CoordinateSystem;
            }
        }

        /// <summary>
        /// if current Coordinate System is not set always take <paramref name="potentialCoordinateSystem"/>.
        /// if this model has a CoordinateSystem but the new grid doesn't, set the grids coordinate system to the model coordinate
        /// system.
        /// if this model has a CoordinateSystem and the new grid has a coordinate system too check if they are the same. If so do
        /// nothing.
        /// if this model has a CoordinateSystem and the new grid has a coordinate system too but they are different check if you
        /// can transform the model coordinate system to the grid coordinate system.
        /// </summary>
        /// <param name="potentialCoordinateSystem"> </param>
        private void UpdateCoordinateSystem(ICoordinateSystem potentialCoordinateSystem)
        {
            if (CoordinateSystem == null)
            {
                CoordinateSystem = potentialCoordinateSystem;
                return;
            }

            if (potentialCoordinateSystem == null)
            {
                log.WarnFormat(
                    Resources
                        .WaveModel_OnOuterDomainPropertyChanged_Grid_is_set_in_project_but_doesn_t_contain_a_coordinate_system__The_model_has_co_ordinate_system__0___setting_grid_to_this_co_oordinate_system_type_,
                    CoordinateSystem);

                CoordinateSystem = null;
                AfterCoordinateSystemSet();
            }
            else
            {
                if (coordinateSystem.EqualsTo(potentialCoordinateSystem))
                {
                    return;
                }

                // else (check for) transform
                if (!CanSetCoordinateSystem(potentialCoordinateSystem))
                {
                    throw new Exception(string.Format(
                                            Resources
                                                .WaveModel_OnOuterDomainPropertyChanged_The_model_coordinates_do_not_appear_to_be_in___0____as_they_fall_outside_the_expected_range_of_values_for_this_system__Please_verify_the_selected_coordinate_system_is_the_system_the_coordinates_were_measured_in__Continuing_could_lead_to_the_map_visualization_failing_and_unexpected_behaviour_of_spatial_operations__1__1_Grid_coordinates_are_incompatible_with_current_model_coordinate_system,
                                            potentialCoordinateSystem, Environment.NewLine));
                }

                //transform model
                log.WarnFormat(
                    Resources
                        .WaveModel_OnOuterDomainPropertyChanged_Grid_is_set_in_project_but_isn_t_the_same_coordinate_system_as_our_model__The_model_has_co_ordinate_system__0___the_grid_has__1___Setting_the_model_to_the_grid_co_ordinate_system_type__1__,
                    CoordinateSystem, potentialCoordinateSystem);
                TransformCoordinates(
                    new OgrCoordinateSystemFactory().CreateTransformation(CoordinateSystem,
                                                                          potentialCoordinateSystem));
            }
        }

        private void InitializeCouplingTime()
        {
            StartTime = ModelDefinition.ModelReferenceDateTime;
            StopTime = ModelDefinition.ModelReferenceDateTime.AddDays(1);
        }

        private static void BuildModelFromMdw(WaveModel model, string mdwFilePath)
        {
            Ensure.NotNull(mdwFilePath, nameof(mdwFilePath));

            model.MdwFile.MdwFilePath = mdwFilePath;
            model.Name = Path.GetFileNameWithoutExtension(mdwFilePath);

            MdwFileDTO dto = model.MdwFile.Load(mdwFilePath);
            if (model.ModelDefinition != null)
            {
                WaveModelDefinitionLoadHelper.TransferLoadedProperties(model.ModelDefinition, dto.WaveModelDefinition);
            }
            else
            {
                model.ModelDefinition = dto.WaveModelDefinition;
            }

            model.TimeFrameData.SynchronizeDataWith(dto.TimeFrameData);

            model.SyncModelTimesWithBase();

            IList<IWaveDomainData> allDomains = WaveDomainHelper.GetAllDomains(model.ModelDefinition.OuterDomain);

            model.BuildWaveDomains(allDomains, model.InputDirPath, model);
        }

        private static void BuildEmptyModel(WaveModel model)
        {
            model.ModelDefinition = new WaveModelDefinition { OuterDomain = new WaveDomainData("Outer") };
        }

        /// <summary>
        /// Method describing how to react on changes in the model definition properties.
        /// For BedFrictionCoef and MaxIter, this method is needed for setting the correct default values
        /// after selecting another BedFriction or Sim Mode option.
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="e"> </param>
        private void OnModelDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var prop = (WaveModelProperty)sender;
            if (e.PropertyName != nameof(prop.Value))
            {
                return;
            }

            if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.BedFriction,
                                                                StringComparison.InvariantCultureIgnoreCase))
            {
                BeginEdit(new DefaultEditAction("Switching bed friction coefficient"));

                WaveModelProperty bedFrictionProperty = ModelDefinition.GetModelProperty(
                    KnownWaveSections.ProcessesSection,
                    KnownWaveProperties.BedFriction);
                WaveModelProperty bedFrictionCoefficientProperty = ModelDefinition.GetModelProperty(
                    KnownWaveSections.ProcessesSection,
                    KnownWaveProperties.BedFrictionCoef);

                bedFrictionCoefficientProperty.SetValueAsString(
                    bedFrictionCoefficientProperty.PropertyDefinition.MultipleDefaultValues[
                        (int)bedFrictionProperty.Value]);

                TriggerPropertyChanged(KnownWaveSections.ProcessesSection, KnownWaveProperties.BedFriction, o => BedFriction = (int)o);
                EndEdit();
            }

            else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.SimulationMode,
                                                                     StringComparison.InvariantCultureIgnoreCase))
            {
                BeginEdit(new DefaultEditAction("Switching simulation mode"));
                WaveModelProperty simulationModeProperty = modelDefinition.GetModelProperty(
                    KnownWaveSections.GeneralSection,
                    KnownWaveProperties.SimulationMode);

                WaveModelProperty maxNrIterationsProperty = modelDefinition.GetModelProperty(
                    KnownWaveSections.NumericsSection,
                    KnownWaveProperties.MaxIter);

                maxNrIterationsProperty.SetValueAsString(
                    maxNrIterationsProperty.PropertyDefinition.MultipleDefaultValues[
                        (int)simulationModeProperty.Value]);

                TriggerPropertyChanged(KnownWaveSections.GeneralSection, KnownWaveProperties.SimulationMode, o => SimulationMode = (int)o);
                EndEdit();
            }
            else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.DirectionalSpaceType,
                                                                     StringComparison.InvariantCultureIgnoreCase))
            {
                BeginEdit(new DefaultEditAction("Switching directional space type"));
                TriggerPropertyChanged(KnownWaveSections.GeneralSection, KnownWaveProperties.DirectionalSpaceType, o => DirectionalSpaceType = (int)o);
                EndEdit();
            }
            else if (prop.PropertyDefinition.FilePropertyName.Equals(
                KnownWaveProperties.WriteCOM, StringComparison.InvariantCultureIgnoreCase))
            {
                BeginEdit(new DefaultEditAction("Switching write COM"));
                TriggerPropertyChanged(KnownWaveSections.OutputSection, KnownWaveProperties.WriteCOM, o => WriteCOM = (bool)o);
                EndEdit();
            }
            else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.WriteTable,
                                                                     StringComparison.InvariantCultureIgnoreCase))
            {
                BeginEdit(new DefaultEditAction("Switching write table"));
                TriggerPropertyChanged(KnownWaveSections.OutputSection, KnownWaveProperties.WriteTable, o => WriteTable = (bool)o);
                EndEdit();
            }
            else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.MapWriteNetCDF,
                                                                     StringComparison.InvariantCultureIgnoreCase))
            {
                BeginEdit(new DefaultEditAction("Switching MapWriteNetCDF"));
                TriggerPropertyChanged(KnownWaveSections.OutputSection, KnownWaveProperties.MapWriteNetCDF, o => MapWriteNetCDF = (bool)o);
                EndEdit();
            }
            else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.Breaking,
                                                                     StringComparison.InvariantCultureIgnoreCase))
            {
                BeginEdit(new DefaultEditAction("Switching Breaking"));
                TriggerPropertyChanged(KnownWaveSections.ProcessesSection, KnownWaveProperties.Breaking, o => Breaking = (bool)o);
                EndEdit();
            }
            else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.Triads,
                                                                     StringComparison.InvariantCultureIgnoreCase))
            {
                BeginEdit(new DefaultEditAction("Switching Triads"));
                TriggerPropertyChanged(KnownWaveSections.ProcessesSection, KnownWaveProperties.Triads, o => Triads = (bool)o);
                EndEdit();
            }
            else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.Diffraction,
                                                                     StringComparison.InvariantCultureIgnoreCase))
            {
                BeginEdit(new DefaultEditAction("Switching Diffraction"));
                TriggerPropertyChanged(KnownWaveSections.ProcessesSection, KnownWaveProperties.Diffraction, o => Diffraction = (bool)o);
                EndEdit();
            }
            else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.WaveSetup,
                                                                     StringComparison.InvariantCultureIgnoreCase))
            {
                BeginEdit(new DefaultEditAction("Switching WaveSetup"));
                TriggerPropertyChanged(KnownWaveSections.ProcessesSection, KnownWaveProperties.WaveSetup, o => WaveSetup = (bool)o);

                if ((bool)prop.Value)
                {
                    log.WarnFormat(Resources.WaveModel_WaveSetup_With_WaveSetup_set_to_True_parallel_runs_will_fail__normal_runs_with_lakes_will_produce_unreliable_values_);
                }

                EndEdit();
            }
        }

        private void TriggerPropertyChanged(string propertyCategory, string propertyName,
                                            Action<object> setPropertyAction)
        {
            // To trigger a property changed on the WaveModel, this self assignment is necessary.
            object propertyValue = ModelDefinition.GetModelProperty(propertyCategory, propertyName).Value;
            setPropertyAction(propertyValue);
        }

        private void BuildWaveDomains(IEnumerable<IWaveDomainData> allDomains, string workingDirectory, WaveModel model)
        {
            foreach (IWaveDomainData domain in allDomains)
            {
                LoadBathymetry(model, workingDirectory, domain);
            }
        }

        [EditAction]
        private void AfterCoordinateSystemSet()
        {
            IList<IWaveDomainData> domains = WaveDomainHelper.GetAllDomains(OuterDomain);

            foreach (IWaveDomainData waveDomainData in domains)
            {
                waveDomainData.Grid.CoordinateSystem = coordinateSystem;
                waveDomainData.Bathymetry.CoordinateSystem = coordinateSystem;

                if (waveDomainData.Grid.CoordinateSystem != null &&
                    waveDomainData.Grid.Attributes.ContainsKey(CurvilinearGrid.CoordinateSystemKey))
                {
                    waveDomainData.Grid.Attributes[CurvilinearGrid.CoordinateSystemKey] =
                        waveDomainData.Grid.CoordinateSystem.IsGeographic
                            ? CoordinateSystemType.Spherical
                            : CoordinateSystemType.Cartesian;
                }
            }
        }

        private void SaveBathymetries(IEnumerable<IWaveDomainData> allDomains, string projectPath)
        {
            foreach (IWaveDomainData domain in allDomains)
            {
                if (!domain.Bathymetry.X.Values.Any())
                {
                    continue;
                }

                string targetFile = Path.Combine(projectPath, domain.BedLevelFileName);
                int sizeM = domain.Bathymetry.Size2;
                int sizeN = domain.Bathymetry.Size1;
                Delft3DDepthFileWriter.Write(domain.Bathymetry.GetValues<double>().ToArray(), sizeN, sizeM,
                                             targetFile);
            }
        }

        private static void RestoreEnvironment()
        {
            string oldArch = Environment.GetEnvironmentVariable(WaveEnvironmentConstants.OldArchKey);
            if (!string.IsNullOrEmpty(oldArch))
            {
                Environment.SetEnvironmentVariable(WaveEnvironmentConstants.ArchKey, oldArch);
                Environment.SetEnvironmentVariable(WaveEnvironmentConstants.OldArchKey, null);
            }

            WaveEnvironmentHelper.DimrRun = false;
        }

        private void ReportProgressText(string text = null)
        {
            progressText = text;
            base.OnProgressChanged();
        }

        private void LoadWaveDomain(IWaveDomainData domain)
        {
            LoadGrid(InputDirPath, domain);

            UpdateBathymetry(domain);
            UpdateBathymetryOperations(domain);
        }

        private static void UpdateBathymetry(IWaveDomainData domain)
        {
            domain.Bathymetry.Resize(domain.Grid.Size1, domain.Grid.Size2, domain.Grid.X.Values, domain.Grid.Y.Values);
        }

        private void UpdateBathymetryOperations(IWaveDomainData domain)
        {
            IDataItem dataItem = GetDataItemByValue(domain.Bathymetry);

            if (!(dataItem?.ValueConverter is SpatialOperationSetValueConverter bathyValueConverter))
            {
                return;
            }

            var curvilinearCoverage = (CurvilinearCoverage)bathyValueConverter.OriginalValue;
            curvilinearCoverage.BeginEdit(new DefaultEditAction("Reloading coverage grid"));
            curvilinearCoverage.Resize(
                domain.Grid.Size1, domain.Grid.Size2,
                domain.Grid.X.Values, domain.Grid.Y.Values);
            curvilinearCoverage.EndEdit();
            bathyValueConverter.SpatialOperationSet.SetDirty();
        }

        private void OnAddedToProject(string mdwFilePath)
        {
            // implicit switch
            ModelSaveTo(mdwFilePath, true);
        }

        private string GetModelDirectoryPathFromMdwFile()
        {
            if (string.IsNullOrEmpty(MdwFilePath))
            {
                return Name;
            }

            string modelDirectoryName = Path.GetFileNameWithoutExtension(MdwFilePath);
            var modelDirInfo = new DirectoryInfo(MdwFilePath);

            while (modelDirInfo != null && modelDirInfo.Name != modelDirectoryName)
            {
                modelDirInfo = modelDirInfo.Parent;
            }

            return modelDirInfo?.Parent != null
                       ? modelDirInfo.FullName
                       : Path.GetDirectoryName(Path.GetDirectoryName(MdwFilePath)); // default behaviour if file-based repository is corrupted.

        }

        private void OnSave()
        {
            string modelDirectoryPath = GetModelDirectoryPathFromMdwFile();
            string mdwSavePath = GetMdwPathFromDeltaShellPath(modelDirectoryPath);
            bool isRenamed = mdwSavePath != MdwFilePath;

            ModelSaveTo(mdwSavePath, true);

            if (!isRenamed)
            {
                return;
            }

            FileUtils.DeleteIfExists(modelDirectoryPath);
        }

        private void OnCopyTo(string targetMdwFilePath)
        {
            ModelSaveTo(targetMdwFilePath, false);
        }

        private string PreviousMdwPath { get; set; }

        private void OnSwitchTo(string newMdwFilePath)
        {
            if (MdwFile.MdwFilePath == null)
            {
                BuildModelFromMdw(this, newMdwFilePath);
                ReplaceOuterDomain();

                InitializeWaveOutputData();
            }
            else
            {
                PreviousMdwPath = MdwFile.MdwFilePath;
                MdwFile.MdwFilePath = newMdwFilePath;
            }
        }

        private void ReplaceOuterDomain()
        {
            if (Equals(OuterDomain, ModelDefinition.OuterDomain))
            {
                return;
            }

            if (OuterDomain != null)
            {
                ((INotifyPropertyChanged)OuterDomain).PropertyChanged -= OnOuterDomainPropertyChanged;
            }

            outerDomain = ModelDefinition.OuterDomain;
            ReplaceDataItemsForDomain(OuterDomain);

            if (OuterDomain != null)
            {
                ((INotifyPropertyChanged)OuterDomain).PropertyChanged += OnOuterDomainPropertyChanged;
                gridOperationApi = new WaveGridOperationApi(OuterDomain.Grid);
            }
        }

        public static class CoordinateSystemType
        {
            public const string Spherical = "Spherical";
            public const string Cartesian = "Cartesian";
        }

        #region IFileBased and NHibernate

        //tells NHibernate we need to be saved
        private void MarkDirty()
        {
            unchecked
            {
                dirtyCounter++;
            }
        }

        private int dirtyCounter;

        private string path;
        private WaveModelDefinition modelDefinition;
        private DateTime startTime;

        private DateTime stopTime;

        private TimeSpan timeStep;

        string IFileBased.Path
        {
            get => path;
            set
            {
                if (path == value)
                {
                    return;
                }

                path = value;

                if (path == null)
                {
                    return;
                }

                if (path.StartsWith("$") && IsOpen)
                {
                    OnSave();
                }
            }
        }

        public IEnumerable<string> Paths
        {
            get
            {
                yield return ((IFileBased)this).Path;
            }
        }

        public bool IsFileCritical => true;

        public bool IsOpen { get; private set; }

        /// <summary>
        /// Make a copy of the file if it is located in the DeltaShell working directory
        /// </summary>
        public bool CopyFromWorkingDirectory { get; }

        public virtual string MdwFilePath => MdwFile?.MdwFilePath;

        void IFileBased.CreateNew(string mdwPath)
        {
            OnAddedToProject(GetMdwPathFromDeltaShellPath(mdwPath));
            path = mdwPath;
            IsOpen = true;
        }

        void IFileBased.Close()
        {
            IsOpen = false;
        }

        void IFileBased.Open(string mdwPath)
        {
            IsOpen = true;
        }

        void IFileBased.CopyTo(string destinationPath)
        {
            OnCopyTo(GetMdwPathFromDeltaShellPath(destinationPath));
        }

        void IFileBased.SwitchTo(string newPath)
        {
            path = newPath;
            OnSwitchTo(GetMdwPathFromDeltaShellPath(newPath));
            IsOpen = true;
        }

        void IFileBased.Delete()
        {
            // Nothing to be done, enforced through IFileBased
        }

        private string GetMdwPathFromDeltaShellPath(string dsPath)
        {
            // dsproj_data/<model name>/<model name>.mdw
            return Path.Combine(Path.GetDirectoryName(dsPath),
                                Name,
                                DirectoryNameConstants.InputDirectoryName,
                                Name + FileConstants.MdwFileExtension);
        }

        #endregion

        #region IDimrModel

        public virtual string LibraryName => "wave";

        public virtual string InputFile => Name + FileConstants.MdwFileExtension;

        public virtual string DirectoryName => "wave";

        public virtual bool IsMasterTimeStep => !IsCoupledToFlow;

        public virtual string ShortName => "wave";

        public virtual string GetItemString(IDataItem dataItem)
        {
            return null;
        }

        public virtual IEnumerable<IDataItem> GetDataItemsByItemString(string itemString)
        {
            throw new NotImplementedException();
        }

        public virtual Type ExporterType => typeof(WaveModelFileExporter);

        public virtual string GetExporterPath(string directoryName) =>
            Path.Combine(directoryName, InputFile);

        public virtual bool CanRunParallel => true;

        public virtual string MpiCommunicatorString => null;

        public virtual string KernelDirectoryLocation
        {
            get
            {
                using (new WaveEnvironmentHelper(string.Empty))
                {
                    WaveEnvironmentHelper.DimrRun = true;

                    return string.Join(";",
                                       DimrApiDataSet.WaveExePath,
                                       DimrApiDataSet.SwanExePath,
                                       DimrApiDataSet.SwanScriptPath,
                                       DimrApiDataSet.EsmfExePath,
                                       DimrApiDataSet.EsmfScriptPath);
                }
            }
        }

        /// <summary>
        /// Disconnects the output.
        /// </summary>
        /// <remarks>
        /// Note that this does not clear the output, it merely severs
        /// the connection.
        /// </remarks>
        public virtual void DisconnectOutput()
        {
            if (WaveOutputData.IsConnected)
            {
                WaveOutputData.Disconnect();
            }
        }

        public virtual void ConnectOutput(string outputPath)
        {
            var logHandler = new LogHandler(Resources.WaveModel_Connect_model_output, log);

            bool isInWorkingDir = outputPath.StartsWith(WorkingDirectoryPathFunc());
            WaveOutputData.ConnectTo(outputPath, isInWorkingDir, logHandler);

            logHandler.LogReport();
        }

        protected override void OnClearOutput()
        {
            if (WaveOutputData.IsConnected)
            {
                WaveOutputData.Disconnect();
            }
        }

        public new virtual ActivityStatus Status
        {
            get => base.Status;
            set => base.Status = value;
        }

        [EditAction]
        public virtual bool RunsInIntegratedModel { get; set; }

        /// <summary>
        /// Gets the dimr export directory path.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Thrown when this property is set, because the model should use the application's working directory.
        /// </exception>
        public virtual string DimrExportDirectoryPath => Path.Combine(WorkingDirectoryPathFunc(), Name);

        public virtual string DimrModelRelativeWorkingDirectory => DirectoryName;

        public virtual string DimrModelRelativeOutputDirectory => DirectoryName;

        [NoNotifyPropertyChange]
        public new virtual DateTime CurrentTime
        {
            get => base.CurrentTime;
            set => base.CurrentTime = value;
        }

        public virtual Array GetVar(string category, string itemName = null, string parameter = null)
        {
            //wave doesn't run standalone via dimr but via kernels
            return new[]
            {
                default(double)
            };
        }

        public virtual void SetVar(Array values, string category, string itemName = null, string parameter = null)
        {
            //wave doesn't run standalone via dimr but via kernels
        }

        public virtual void OnFinishIntegratedModelRun(string hydroModelWorkingDirectoryPath)
        {
            // Actions, which should be done in the IDimrModel after a successful integrated model
            // run.
        }

        public ISet<string> IgnoredFilePathsWhenCleaningWorkingDirectory => new HashSet<string>();

        #endregion
    }
}