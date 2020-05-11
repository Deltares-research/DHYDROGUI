using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BasicModelInterface;
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
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using DeltaShell.Plugins.FMSuite.Common.IO.Writers;
using DeltaShell.Plugins.FMSuite.Wave.Api;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.IO.Exporters;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    [Entity]
    public class WaveModel : TimeDependentModelBase, IDisposable, IGridOperationApi, IWaveModel, IFileBased,
                             IHydroModel, IDimrModel
    {
        // Also add model specific dataitems to the exclude list in <see cref="BuildModel"/>
        public const string WavmStoreDataItemTag = "WavmStoreDataItemTag";
        public const string SwanLogDataItemTag = "SwanLogDataItemTag";
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaveModel));

        private static readonly string GridPropertyName = nameof(WaveDomainData.Grid);

        private readonly MdwFile mdwFile = new MdwFile();

        private readonly BoundaryContainerSyncService boundaryContainerSyncService;

        private readonly string tempWorkingDirectory;
        private readonly DimrRunner runner;
        private ICoordinateSystem coordinateSystem;
        private string progressText;
        private IWaveModelApi waveApi;

        private IWaveDomainData outerDomain;
        private string previousGridName;

        private IGridOperationApi gridOperationApi;
        private double previousProgress = 0;

        public WaveModel() : this(BuildEmptyModel) {}

        public WaveModel(string mdwPath) : this(model => BuildModelFromMdw(model, mdwPath)) {}

        private WaveModel(Action<WaveModel> creationCode) : base("Waves")
        {
            runner = new DimrRunner(this);
            BuildModel(creationCode, false);

            ShowModelRunConsole = false;
            ValidateBeforeRun = true;

            WaveDomainHelper.GetAllDomains(outerDomain).ForEach(SyncWithModelDefaults);
            gridOperationApi = new WaveGridOperationApi(outerDomain.Grid);

            ((INotifyPropertyChanged) this).PropertyChanged += (s, e) => MarkDirty();
            ((INotifyCollectionChanged) this).CollectionChanged += (s, e) => MarkDirty();

            dataItems.Add(new DataItem(new TextDocument(true) {Name = "Swan run log"}, DataItemRole.Output,
                                       SwanLogDataItemTag));

            tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            InitializeCouplingTime();

            boundaryContainerSyncService = new BoundaryContainerSyncService(this);
#pragma warning disable 618
            BoundariesFromBoundaryContainer = BoundaryContainer.Boundaries;
#pragma warning restore 618
        }

        /// <summary>
        /// Gets a value indicating whether this wave model is online coupled to a fm model.
        /// Always true for wave model inside an integrated model, since waves models can
        /// not run stand-alone in DIMR.
        /// </summary>
        public bool IsCoupledToFlow { get; set; }

        public int SimulationMode
        {
            get => (int) ModelDefinition
                         .GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.SimulationMode)
                         .Value;
            set
            {
                // stationary, quasi-stationary, non-stationary. Used for event bubbling.
                // don't don anything, used for events
            }
        }

        public int DirectionalSpaceType
        {
            get => (int) ModelDefinition
                         .GetModelProperty(KnownWaveCategories.GeneralCategory,
                                           KnownWaveProperties.DirectionalSpaceType).Value;
            set
            {
                // don't don anything, used for events
            }
        }

        public bool WriteCOM
        {
            get => (bool) ModelDefinition
                          .GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.WriteCOM).Value;
            set
            {
                // only used for evt bubbling
            }
        }

        public bool WriteTable
        {
            get => (bool) ModelDefinition
                          .GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.WriteTable).Value;
            set
            {
                // only used for event bubbling
            }
        }

        public bool MapWriteNetCDF
        {
            get => (bool) ModelDefinition
                          .GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.MapWriteNetCDF)
                          .Value;
            set
            {
                // only used for event bubbling
            }
        }

        public bool Breaking
        {
            get => (bool) ModelDefinition
                          .GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.Breaking).Value;
            set
            {
                // only used for event bubbling
            }
        }

        public bool Triads
        {
            get => (bool) ModelDefinition
                          .GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.Triads).Value;
            set
            {
                // only used for event bubbling
            }
        }

        public bool Diffraction
        {
            get => (bool) ModelDefinition
                          .GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.Diffraction)
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
                ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory,
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

        public WaveInputFieldData TimePointData
        {
            get => ModelDefinition.TimePointData;
        }

        /// <summary>
        /// Only used for bubbling events for updating project tree. Don't remove the setter.
        /// It should be public.
        /// </summary>
        [Obsolete("Use BoundaryContainer.Boundaries")]
        public IEventedList<IWaveBoundary> BoundariesFromBoundaryContainer { get; set; }

        public WaveModelDefinition ModelDefinition
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
                if (modelDefinition != null)
                {
                    ((INotifyPropertyChange) modelDefinition.Properties).PropertyChanged +=
                        OnModelDefinitionPropertyChanged;
                }
            }
        }

        public IWaveDomainData OuterDomain
        {
            get => outerDomain;
            set
            {
                if (outerDomain != null)
                {
                    ((INotifyPropertyChanging) outerDomain).PropertyChanging -= OnOuterDomainPropertyChanging;
                    ((INotifyPropertyChanged) outerDomain).PropertyChanged -= OnOuterDomainPropertyChanged;
                    RemoveDataItemsForDomain(outerDomain);
                }

                outerDomain = value;
                ModelDefinition.OuterDomain = outerDomain;

                if (outerDomain != null)
                {
                    ((INotifyPropertyChanging) outerDomain).PropertyChanging += OnOuterDomainPropertyChanging;
                    ((INotifyPropertyChanged) outerDomain).PropertyChanged += OnOuterDomainPropertyChanged;
                    AddDataItemsForDomain(outerDomain);

                    gridOperationApi = new WaveGridOperationApi(outerDomain.Grid);
                }
            }
        }

        public IEnumerable<WavmFileFunctionStore> WavmFunctionStores
        {
            get
            {
                return
                    WaveDomainHelper.GetAllDomains(outerDomain).Select(
                                        domain => GetDataItemByTag(WavmStoreDataItemTag + domain.Name))
                                    .Where(di => di != null)
                                    .Select(di => di.Value as WavmFileFunctionStore);
            }
        }

        public MdwFile MdwFile => mdwFile;

        public string WorkingDirectory => ExplicitWorkingDirectory ?? tempWorkingDirectory;

        [PropertyGrid]
        [DisplayName("Validate before run")]
        [Category("Run mode")]
        public bool ValidateBeforeRun { get; set; }

        [PropertyGrid]
        [DisplayName("Show model run console")]
        [Category("Run mode")]
        public bool ShowModelRunConsole { get; set; }

        public IHydroRegion Region => null;

        /// <summary>
        /// Showing the progress of a run.
        /// </summary>
        public override string ProgressText => string.IsNullOrEmpty(progressText) ? base.ProgressText : progressText;

        public IBoundaryContainer BoundaryContainer
        {
            get => ModelDefinition.BoundaryContainer;
        }

        public IEventedList<Feature2DPoint> ObservationPoints
        {
            get => ModelDefinition.ObservationPoints;
        }

        public IEventedList<Feature2D> ObservationCrossSections
        {
            get => ModelDefinition.ObservationCrossSections;
        }

        public IEventedList<WaveObstacle> Obstacles
        {
            get => ModelDefinition.Obstacles;
        }

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

        public string ImportIntoModelDirectory(string filePath)
        {
            return WaveModelFileHelper.ImportIntoModelDirectory(Path.GetDirectoryName(MdwFilePath), filePath);
        }

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
            var bathymetry = new CurvilinearCoverage(grid.Size1, grid.Size2, grid.X.Values, grid.Y.Values) {Name = $"Bathymetry ({Path.GetFileNameWithoutExtension(domain.BedLevelFileName)})"};
            bathymetry.Components[0].NoDataValue = -999.0;
            bathymetry.Components[0].DefaultValue = bathymetry.Components[0].NoDataValue;

            string depthFilePath = Path.Combine(directory, domain.BedLevelFileName);
            if (File.Exists(depthFilePath) && !grid.IsEmpty)
            {
                List<double> bathymetryValues =
                    Delft3DDepthFileReader.Read(depthFilePath, grid.Size1, grid.Size2).ToList();

                if (bathymetryValues.Count != grid.Size2 * grid.Size1)
                {
                    Log.ErrorFormat(
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
                if (di.ValueConverter is SpatialOperationSetValueConverter)
                {
                    ((SpatialOperationSetValueConverter) di.ValueConverter).OriginalValue = domain.Bathymetry.Clone();
                }
            }
        }

        public static bool IsValidCoordinateSystem(ICoordinateSystem system)
        {
            return !system.IsGeographic || system.Name == "WGS 84";
        }

        // all saving should go through here, but beware, NHibernate will disable
        // event bubbling when saving...
        public void ModelSaveTo(string targetMdwFilePath, bool switchTo)
        {
            string targetDir = Path.GetDirectoryName(targetMdwFilePath);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            mdwFile.SaveTo(targetMdwFilePath, ModelDefinition, switchTo);

            // write spatial data:
            SaveBathymetries(WaveDomainHelper.GetAllDomains(OuterDomain), targetDir);

            SaveOutput(targetDir, switchTo);
        }

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

        public virtual ValidationReport Validate()
        {
            return new WaveModelValidator().Validate(this);
        }

        public void Dispose()
        {
            if (waveApi != null)
            {
                waveApi.Dispose();
            }

            RestoreEnvironment();
        }

        public IGeometry GetGridSnappedGeometry(string featureType, IGeometry geometry)
        {
            return gridOperationApi != null ? gridOperationApi.GetGridSnappedGeometry(featureType, geometry) : geometry;
        }

        public bool SnapsToGrid(IGeometry geometry)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IGeometry> GetGridSnappedGeometry(string featureType, ICollection<IGeometry> geometries)
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
            CoordinateSystem = (ICoordinateSystem) transformation.TargetCS;

            EndEdit();

            // grid(s) transformed, sync data to disk:
            string modelDir = Path.GetDirectoryName(mdwFile.MdwFilePath);
            foreach (IWaveDomainData domain in WaveDomainHelper.GetAllDomains(OuterDomain))
            {
                string targetGridFileName = Path.Combine(modelDir, domain.GridFileName);
                Delft3DGridFileWriter.Write(domain.Grid, targetGridFileName);
            }
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            foreach (object item in base.GetDirectChildren())
            {
                yield return item;
            }

            yield return TimePointData;

            foreach (IWaveDomainData domain in WaveDomainHelper.GetAllDomains(ModelDefinition.OuterDomain))
            {
                yield return domain.Grid;
                yield return domain.Bathymetry;
            }

            foreach (WavmFileFunctionStore wavmFileFunctionStore in WavmFunctionStores)
            {
                if (wavmFileFunctionStore != null && !string.IsNullOrEmpty(wavmFileFunctionStore.Path))
                {
                    yield return wavmFileFunctionStore;
                }
            }

            foreach (IWaveBoundary boundary in BoundaryContainer.Boundaries)
            {
                yield return boundary;
            }
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

        protected override void OnCancel()
        {
            if (waveApi == null)
            {
                return; // we never got past validation..
            }

            waveApi.Dispose();
            waveApi = null;

            base.OnCancel();
        }

        protected virtual void ReconnectWavmFile(string outputPath)
        {
            ReportProgressText("Reading output (WAVM) file");
            List<IWaveDomainData> domains = WaveDomainHelper.GetAllDomains(OuterDomain).ToList();
            if (domains.Count > 1)
            {
                for (var i = 0; i < domains.Count; ++i)
                {
                    string wavmFile = Path.Combine(outputPath, "wavm-" + Name + "-" + domains[i].Name + ".nc");
                    ConnectWavmFile(wavmFile, i);
                }
            }
            else
            {
                string wavmFile = Path.Combine(outputPath, "wavm-" + Name + ".nc");
                ConnectWavmFile(wavmFile, 0);
            }
        }

        protected override void OnClearOutput()
        {
            BeginEdit(new DefaultEditAction("Clearing all wave output"));
            WavmFunctionStores.ForEach(fs => fs.Close());
            GetDataItemValueByTag<TextDocument>(SwanLogDataItemTag).Content = string.Empty;
            EndEdit();
        }

        internal void SyncModelTimesWithBase()
        {
            base.StartTime = StartTime;
            base.StopTime = StopTime;
            base.TimeStep = TimeStep;
        }

        [EditAction]
        private void RemoveDataItemsForDomain(IWaveDomainData domain)
        {
            foreach (IWaveDomainData subdomain in WaveDomainHelper.GetAllDomains(domain))
            {
                DataItems.RemoveAllWhere(di => Equals(subdomain.Bathymetry, di.Value));
                DataItems.RemoveAllWhere(di => di.Tag.Equals(WavmStoreDataItemTag + subdomain.Name));
            }
        }

        [EditAction]
        private void AddDataItemsForDomain(IWaveDomainData domain)
        {
            foreach (IWaveDomainData subdomain in WaveDomainHelper.GetAllDomains(domain))
            {
                DataItems.Add(new DataItem(subdomain.Bathymetry, DataItemRole.Input));
                AddDataItem(new WavmFileFunctionStore(""), DataItemRole.Input,
                            WavmStoreDataItemTag + subdomain.Name);
            }
        }

        [EditAction]
        private void ReplaceDataItemsForDomain(IWaveDomainData newDomainData)
        {
            foreach (IWaveDomainData subdomain in WaveDomainHelper.GetAllDomains(newDomainData))
            {
                foreach (IDataItem dataItem in DataItems)
                {
                    if (dataItem.Name == subdomain.Bathymetry.Name)
                    {
                        dataItem.Value = subdomain.Bathymetry;
                    }
                }
            }
        }

        private void OnOuterDomainPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            var domain = sender as WaveDomainData;
            if (domain == null || e.PropertyName != nameof(domain.GridFileName))
            {
                return;
            }

            previousGridName = domain.Name;
        }

        private void OnOuterDomainPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            var domain = sender as WaveDomainData;
            if (domain != null)
            {
                if (eventArgs.PropertyName == nameof(domain.GridFileName) && !string.IsNullOrEmpty(previousGridName))
                {
                    var dataItem = GetDataItemByTag(WavmStoreDataItemTag + previousGridName) as DataItem;
                    if (dataItem == null)
                    {
                        return;
                    }

                    dataItem.Tag = WavmStoreDataItemTag + domain.Name;
                    dataItemByTagDictionaryIsDirty = true;
                }

                if (eventArgs.PropertyName == GridPropertyName)
                {
                    if (Equals(OuterDomain, domain))
                    {
                        gridOperationApi = new WaveGridOperationApi(outerDomain.Grid);
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
                Log.WarnFormat(
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
                Log.WarnFormat(
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

        /// <summary>
        /// Watch out, this method can/will be called multiple times for the same instance!!
        /// </summary>
        /// <param name="creationCode"> </param>
        private void BuildModel(Action<WaveModel> creationCode, bool loading)
        {
            creationCode(this);
            if (loading && !Equals(OuterDomain, ModelDefinition.OuterDomain))
            {
                if (outerDomain != null)
                {
                    ((INotifyPropertyChanging) outerDomain).PropertyChanging -= OnOuterDomainPropertyChanging;
                    ((INotifyPropertyChanged) outerDomain).PropertyChanged -= OnOuterDomainPropertyChanged;
                }

                outerDomain = ModelDefinition.OuterDomain;
                ReplaceDataItemsForDomain(outerDomain);
                if (outerDomain != null)
                {
                    ((INotifyPropertyChanging) outerDomain).PropertyChanging += OnOuterDomainPropertyChanging;
                    ((INotifyPropertyChanged) outerDomain).PropertyChanged += OnOuterDomainPropertyChanged;
                    gridOperationApi = new WaveGridOperationApi(outerDomain.Grid);
                }
            }
            else if (!Equals(OuterDomain, ModelDefinition.OuterDomain))
            {
                OuterDomain = ModelDefinition.OuterDomain;
            }

            if (outerDomain != null && outerDomain.Grid != null && !loading)
            {
                UpdateCoordinateSystem(outerDomain.Grid.CoordinateSystem);
            }
        }

        private static void BuildModelFromMdw(WaveModel model, string mdwFilePath)
        {
            model.MdwFile.MdwFilePath = mdwFilePath;
            model.Name = Path.GetFileNameWithoutExtension(mdwFilePath);
            model.ModelDefinition = model.mdwFile.Load(mdwFilePath);

            model.SyncModelTimesWithBase();

            string mdwDir = Path.GetDirectoryName(mdwFilePath);
            IList<IWaveDomainData> allDomains = WaveDomainHelper.GetAllDomains(model.ModelDefinition.OuterDomain);

            model.BuildWaveDomains(allDomains, mdwDir, model);
        }

        private static void BuildEmptyModel(WaveModel model)
        {
            model.ModelDefinition = new WaveModelDefinition {OuterDomain = new WaveDomainData("Outer")};
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
            var prop = (WaveModelProperty) sender;
            if (e.PropertyName == nameof(prop.Value))
            {
                if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.BedFriction,
                                                                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching bed friction coefficient"));

                    WaveModelProperty bedFrictionProperty = ModelDefinition.GetModelProperty(
                        KnownWaveCategories.ProcessesCategory,
                        KnownWaveProperties.BedFriction);
                    WaveModelProperty bedFrictionCoefficientProperty = ModelDefinition.GetModelProperty(
                        KnownWaveCategories.ProcessesCategory,
                        KnownWaveProperties.BedFrictionCoef);

                    bedFrictionCoefficientProperty.SetValueAsString(
                        bedFrictionCoefficientProperty.PropertyDefinition.MultipleDefaultValues[
                            (int) bedFrictionProperty.Value]);

                    BedFriction = BedFriction;
                    EndEdit();
                }

                if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.SimulationMode,
                                                                    StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching simulation mode"));
                    WaveModelProperty simulationModeProperty = modelDefinition.GetModelProperty(
                        KnownWaveCategories.GeneralCategory,
                        KnownWaveProperties.SimulationMode);

                    WaveModelProperty maxNrIterationsProperty = modelDefinition.GetModelProperty(
                        KnownWaveCategories.NumericsCategory,
                        KnownWaveProperties.MaxIter);

                    maxNrIterationsProperty.SetValueAsString(
                        maxNrIterationsProperty.PropertyDefinition.MultipleDefaultValues[
                            (int) simulationModeProperty.Value]);

                    SimulationMode = SimulationMode;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.DirectionalSpaceType,
                                                                         StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching directional space type"));
                    DirectionalSpaceType = DirectionalSpaceType;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.FilePropertyName.Equals(
                    KnownWaveProperties.WriteCOM, StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching write COM"));
                    WriteCOM = WriteCOM;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.WriteTable,
                                                                         StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching write table"));
                    WriteTable = WriteTable;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.MapWriteNetCDF,
                                                                         StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switcing MapWriteNetCDF"));
                    MapWriteNetCDF = MapWriteNetCDF;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.Breaking,
                                                                         StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching Breaking"));
                    Breaking = Breaking;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.Triads,
                                                                         StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching Triads"));
                    Triads = Triads;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.Diffraction,
                                                                         StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching Diffraction"));
                    Diffraction = Diffraction;
                    EndEdit();
                }
                else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.WaveSetup,
                                                                         StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching WaveSetup"));
                    WaveSetup = WaveSetup;
                    if ((bool) prop.Value)
                    {
                        Log.WarnFormat(
                            Resources
                                .WaveModel_WaveSetup_With_WaveSetup_set_to_True_parallel_runs_will_fail__normal_runs_with_lakes_will_produce_unreliable_values_);
                    }

                    EndEdit();
                }
            }
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
            domains.ForEach(d =>
            {
                d.Grid.CoordinateSystem = coordinateSystem;
                d.Bathymetry.CoordinateSystem = coordinateSystem;

                if (d.Grid.CoordinateSystem != null &&
                    d.Grid.Attributes.ContainsKey(CurvilinearGrid.CoordinateSystemKey))
                {
                    d.Grid.Attributes[CurvilinearGrid.CoordinateSystemKey] =
                        d.Grid.CoordinateSystem.IsGeographic
                            ? CoordinateSystemType.Spherical
                            : CoordinateSystemType.Cartesian;
                }
            });
        }

        private void SaveOutput(string targetDirectory, bool switchTo)
        {
            foreach (WavmFileFunctionStore wavmFileFunctionStore in WavmFunctionStores)
            {
                string oldOutputFilePath = wavmFileFunctionStore.Path;
                string wavmOutputFileName = Path.GetFileName(oldOutputFilePath);
                if (wavmOutputFileName == null)
                {
                    continue;
                }

                string newOutputFilePath = Path.Combine(targetDirectory, wavmOutputFileName);
                if (wavmFileFunctionStore.Functions.Count == 0)
                {
                    if (File.Exists(newOutputFilePath) && !FileUtils.IsDirectory(newOutputFilePath))
                    {
                        FileUtils.DeleteIfExists(newOutputFilePath);
                        wavmFileFunctionStore.Path = string.Empty;
                    }

                    continue;
                }

                bool savingToTheSameOutputFile = string.Equals(Path.GetFullPath(oldOutputFilePath), Path.GetFullPath(newOutputFilePath), StringComparison.CurrentCultureIgnoreCase);
                if (string.IsNullOrEmpty(oldOutputFilePath) || savingToTheSameOutputFile || !File.Exists(oldOutputFilePath))
                {
                    continue;
                }

                File.Copy(oldOutputFilePath, newOutputFilePath, true);
                if (switchTo)
                {
                    wavmFileFunctionStore.Path = newOutputFilePath;
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

        private void ConnectWavmFile(string wavmFile, int i)
        {
            if (File.Exists(wavmFile))
            {
                BeginEdit(new DefaultEditAction("Reconnect output (WAVM) file"));

                WavmFunctionStores.ElementAt(i).Path = wavmFile;
                OutputIsEmpty = false;

                EndEdit();
            }
            else
            {
                Log.WarnFormat(
                    Resources.WaveModel_ReconnectWavmFile_Could_not_find_output_file__0__,
                    wavmFile);
            }
        }

        private void ReportProgressText(string text = null)
        {
            progressText = text;
            base.OnProgressChanged();
        }

        private void LoadWaveDomain(IWaveDomainData domain)
        {
            LoadGrid(Path.GetDirectoryName(MdwFilePath), domain);

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
            if (dataItem != null)
            {
                var bathyValueConverter = dataItem.ValueConverter as SpatialOperationSetValueConverter;
                if (bathyValueConverter != null)
                {
                    var curvilinearCoverage = (CurvilinearCoverage) bathyValueConverter.OriginalValue;
                    curvilinearCoverage.BeginEdit(new DefaultEditAction("Reloading coverage grid"));
                    curvilinearCoverage.Resize(
                        domain.Grid.Size1, domain.Grid.Size2,
                        domain.Grid.X.Values, domain.Grid.Y.Values);
                    curvilinearCoverage.EndEdit();
                    bathyValueConverter.SpatialOperationSet.SetDirty();
                }
            }
        }

        private void ReconnectSwanDiagFile(string outputPath)
        {
            ReportProgressText("Reading Swan dia file");
            string swanDiagFile = Path.Combine(outputPath,
                                               "swn-diag." + Name);
            var swanLog = GetDataItemValueByTag<TextDocument>(SwanLogDataItemTag);

            if (File.Exists(swanDiagFile))
            {
                try
                {
                    string log = File.ReadAllText(swanDiagFile);
                    swanLog.Content = log;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat(Resources.WaveModel_ReadSwanDiagFile_Error_reading_log_file__0__1_,
                                    swanDiagFile, ex.Message);
                }
            }
            else
            {
                Log.WarnFormat(
                    Resources.WaveModel_ReadSwanDiagFile_Could_not_find_log_file__0__,
                    swanDiagFile);
            }
        }

        private void OnAddedToProject(string mdwFilePath)
        {
            // implicit switch
            ModelSaveTo(mdwFilePath, true);
        }

        private void OnSave()
        {
            ModelSaveTo(mdwFile.MdwFilePath, true);
        }

        private void OnCopyTo(string targetMdwFilePath)
        {
            ModelSaveTo(targetMdwFilePath, false);
        }

        private void OnSwitchTo(string newMdwFilePath)
        {
            if (mdwFile.MdwFilePath == null)
            {
                BuildModel(model => BuildModelFromMdw(model, newMdwFilePath), true);
            }
            else
            {
                mdwFile.MdwFilePath = newMdwFilePath;
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
        //private IHydroRegion region;

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
                yield return ((IFileBased) this).Path;
            }
        }

        public bool IsFileCritical => true;

        public bool IsOpen { get; private set; }

        /// <summary>
        /// Make a copy of the file if it is located in the DeltaShell working directory
        /// </summary>
        public bool CopyFromWorkingDirectory { get; }

        public virtual string MdwFilePath => mdwFile?.MdwFilePath;

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

        void IFileBased.Delete() {}

        private string GetMdwPathFromDeltaShellPath(string dsPath)
        {
            // dsproj_data/<model name>/<model name>.mdw
            return Path.Combine(Path.GetDirectoryName(dsPath), Path.Combine(Name, Name + ".mdw"));
        }

        #endregion

        #region IDimrModel

        public virtual string LibraryName => "wave";

        public virtual string InputFile => Path.GetFileName(MdwFilePath);

        public virtual string DirectoryName => "wave";

        public virtual bool IsMasterTimeStep => !IsCoupledToFlow;

        public virtual string ShortName => "wave";

        public virtual string GetItemString(IDataItem value)
        {
            return null;
        }

        public virtual IDataItem GetDataItemByItemString(string itemString)
        {
            throw new NotImplementedException();
        }

        public virtual Type ExporterType => typeof(WaveModelFileExporter);

        public virtual string GetExporterPath(string directoryName)
        {
            return Path.Combine(directoryName, MdwFilePath == null ? Name + ".mdw" : Path.GetFileName(MdwFilePath));
        }

        public virtual bool CanRunParallel => true;

        public virtual string MpiCommunicatorString => null;

        public virtual string KernelDirectoryLocation
        {
            get
            {
                using (var waveDllHelper = new WaveEnvironmentHelper(string.Empty))
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

        public virtual void DisconnectOutput()
        {
            if (!OutputIsEmpty)
            {
                OnClearOutput();
            }
        }

        public virtual void ConnectOutput(string outputPath)
        {
            ReconnectWavmFile(outputPath);
            ReconnectSwanDiagFile(outputPath);
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
            get => ExplicitWorkingDirectory;
            set => ExplicitWorkingDirectory = value;
        }

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
            //wave doesnt run standalone via dimr but via kernels
            return new[]
            {
                default(double)
            };
        }

        public virtual void SetVar(Array values, string category, string itemName = null, string parameter = null)
        {
            //wave doesnt run standalone via dimr but via kernels
        }

        public virtual void PrepareForIntegratedModelRun()
        {
            // Initialization logic which should be executed as part of an
            // integrated model HydroModel initialization.
        }

        #endregion
    }
}