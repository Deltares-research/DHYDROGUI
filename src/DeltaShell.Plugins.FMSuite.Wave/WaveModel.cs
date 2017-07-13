using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
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
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Wave.Api;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.IO.Exporters;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    [Entity]
    public class WaveModel : TimeDependentModelBase, IDisposable, IGridOperationApi, IHasCoordinateSystem, IFileBased, IHydroModel, IDimrModel
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaveModel));
        private IGeometry previousFeatureGeometry;
        private bool snappingGeometry;
        private ICoordinateSystem coordinateSystem;
        private IList<IDisposable> disposableItems = new List<IDisposable>();

        /// <summary>
        /// Mwaaah...
        /// </summary>
        public bool IsCoupledToFlow
        {
            get { return isCoupledToFlow; }
            set
            {
                isCoupledToFlow = value;
                if (!isCoupledToFlow)
                {
                    DeCoupleFromFlow();
                }
            }
        }

        [EditAction]
        private void DeCoupleFromFlow()
        {
            ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.WriteCOM)
                .SetValueAsString("false");
            foreach (var waveDomainData in WaveDomainHelper.GetAllDomains(OuterDomain))
            {
                waveDomainData.HydroFromFlowData.BedLevelUsage = UsageFromFlowType.DoNotUse;
                waveDomainData.HydroFromFlowData.WaterLevelUsage = UsageFromFlowType.DoNotUse;
                waveDomainData.HydroFromFlowData.VelocityUsage = UsageFromFlowType.DoNotUse;
                waveDomainData.HydroFromFlowData.WindUsage = UsageFromFlowType.DoNotUse;
            }
            ModelDefinition.DefaultBedLevelUsage = UsageFromFlowType.DoNotUse;
            ModelDefinition.DefaultWaterLevelUsage = UsageFromFlowType.DoNotUse;
            ModelDefinition.DefaultVelocityUsage = UsageFromFlowType.DoNotUse;
            ModelDefinition.DefaultWindUsage = UsageFromFlowType.DoNotUse;
        }

        public int SimulationMode
        {
            get
            {
                return (int)ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.SimulationMode).Value;
            }
            set
            {
                // stationary, quasi-stationary, non-stationary. Used for event bubbling.
                // don't don anything, used for events
            }
        }

        public bool WriteCOM
        {
            get
            {
                return (bool)ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.WriteCOM).Value;
            }
            set
            {
                // only used for evt bubbling
            }
        }

        public bool WriteTable
        {
            get
            {
                return (bool)ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.WriteTable).Value;
            }
            set
            {
                // only used for event bubbling
            }
        }

        public bool MapWriteNetCDF
        {
            get
            {
                return (bool)ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.MapWriteNetCDF).Value;
            }
            set
            {
                // only used for event bubbling
            }
        }

        public bool Breaking
        {
            get
            {
                return (bool)ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.Breaking).Value;
            }
            set
            {
                // only used for event bubbling
            }
        }

        public bool Triads
        {
            get
            {
                return (bool)ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.Triads).Value;
            }
            set
            {
                // only used for event bubbling
            }
        }

        public bool Diffraction
        {
            get
            {
                return (bool)ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.Diffraction).Value;
            }
            set
            {
                // only used for event bubbling
            }
        }

        public int BedFriction
        {
            get
            {
                return
                    (int)
                        ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory,
                            KnownWaveProperties.BedFriction).Value;
            }
            set
            {
                // only used for event bubbling
            }
        }

        public WaveModelDefinition ModelDefinition
        {
            get { return modelDefinition; }
            private set
            {
                if (modelDefinition != null)
                {
                    ((INotifyPropertyChange) (modelDefinition.Properties)).PropertyChanged -=
                        OnModelDefinitionPropertyChanged;
                }
                modelDefinition = value;
                if (modelDefinition != null)
                {
                    ((INotifyPropertyChange) (modelDefinition.Properties)).PropertyChanged +=
                        OnModelDefinitionPropertyChanged;
                }
            }
        }

        private readonly MdwFile mdwFile = new MdwFile();
        private IWaveModelApi waveApi;

        public readonly IEventedList<Feature2D> Boundaries = new EventedList<Feature2D>();
        public readonly IEventedList<Feature2D> Sp2Boundaries = new EventedList<Feature2D>();
        
        private WaveDomainData outerDomain;
        public WaveDomainData OuterDomain
        {
            get { return outerDomain; }
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

        [EditAction]
        private void RemoveDataItemsForDomain(WaveDomainData domain)
        {
            foreach (var subdomain in WaveDomainHelper.GetAllDomains(domain))
            {
                DataItems.RemoveAllWhere(di => Equals(subdomain.Bathymetry, di.Value));
                DataItems.RemoveAllWhere(di => di.Tag.Equals(WavmStoreDataItemTag + subdomain.Name));
            }
        }

        [EditAction]
        private void AddDataItemsForDomain(WaveDomainData domain)
        {
            foreach (var subdomain in WaveDomainHelper.GetAllDomains(domain))
            {
                DataItems.Add(new DataItem(subdomain.Bathymetry, DataItemRole.Input));
                AddDataItem(new WavmFileFunctionStore(""), DataItemRole.Input,
                    WavmStoreDataItemTag + subdomain.Name);
            }            
        }

        [EditAction]
        private void ReplaceDataItemsForDomain(WaveDomainData newDomainData)
        {
            foreach (var subdomain in WaveDomainHelper.GetAllDomains(newDomainData))
            {
                foreach (var dataItem in DataItems)
                {
                    if (dataItem.Name == subdomain.Bathymetry.Name)
                    {
                        dataItem.Value = subdomain.Bathymetry;
                    }
                }
            }
        }

        private static readonly string GridPropertyName = TypeUtils.GetMemberName<WaveDomainData>(d => d.Grid);
        private void OnOuterDomainPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            var domain = sender as WaveDomainData;
            if (domain != null && eventArgs.PropertyName == GridPropertyName)
            {
                if (Equals(OuterDomain, domain))
                {
                    gridOperationApi = new WaveGridOperationApi(outerDomain.Grid);
                }

                UpdateBathymetry(domain);
                UpdateBathymetryOperations(domain);
            }
        }
        
        public IEventedList<Feature2DPoint> ObservationPoints { get; set; }
        public IEventedList<Feature2D> ObservationCrossSections; 
        public IEventedList<WaveObstacle> Obstacles { get; set; }
        public IEventedList<WaveBoundaryCondition> BoundaryConditions { get; set; }

        public WaveInputFieldData TimePointData { get; set; }


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

        private IGridOperationApi gridOperationApi;

        // Also add model specific dataitems to the exclude list in <see cref="BuildModel"/>
        public const string WavmStoreDataItemTag = "WavmStoreDataItemTag";
        public const string SwanLogDataItemTag = "SwanLogDataItemTag";

        private readonly string tempWorkingDirectory;

        public WaveModel() : this(BuildEmptyModel)
        { 
        }

        public WaveModel(string mdwPath) : this(model => BuildModelFromMdw(model, mdwPath))
        {
        }

        private WaveModel(Action<WaveModel> creationCode) : base("Waves")
        {
            BuildModel(creationCode, false);

            ShowModelRunConsole = false;
            ValidateBeforeRun = true;

            WaveDomainHelper.GetAllDomains(outerDomain).ForEach(SyncWithModelDefaults);
            gridOperationApi = new WaveGridOperationApi(outerDomain.Grid);

            ((INotifyPropertyChanged) this).PropertyChanged += (s, e) => MarkDirty();
            ((INotifyCollectionChanged) this).CollectionChanged += (s, e) => MarkDirty();

            // todo: implement snapping through SnapRules
            ((INotifyPropertyChange) Boundaries).PropertyChanging += BoundariesPropertyChanging;
            ((INotifyPropertyChanged) Boundaries).PropertyChanged += BoundariesPropertyChanged;

            dataItems.Add(new DataItem(new TextDocument(true) {Name = "Swan run log"}, DataItemRole.Output,
                SwanLogDataItemTag));

            tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }


        /// <summary>
        /// Watch out, this method can/will be called multiple times for the same instance!!
        /// </summary>
        /// <param name="creationCode"></param>
        private void BuildModel(Action<WaveModel> creationCode, bool loading)
        {
            disposableItems.ForEach(d => d.Dispose());
            disposableItems.Clear();

            StartTime = CurrentTime;
            StopTime = CurrentTime;

            creationCode(this);
            if (loading && !Equals(OuterDomain, ModelDefinition.OuterDomain))
            {
                if (outerDomain != null)
                {
                    ((INotifyPropertyChanged) outerDomain).PropertyChanged -= OnOuterDomainPropertyChanged;
                }

                outerDomain = ModelDefinition.OuterDomain;
                ReplaceDataItemsForDomain(outerDomain);
                if (outerDomain != null)
                {
                    ((INotifyPropertyChanged) outerDomain).PropertyChanged += OnOuterDomainPropertyChanged;
                    gridOperationApi = new WaveGridOperationApi(outerDomain.Grid);
                }
            }
            else if (!Equals(OuterDomain, ModelDefinition.OuterDomain))
            {
                OuterDomain = ModelDefinition.OuterDomain;
            }

            BoundaryConditions = ModelDefinition.BoundaryConditions;
            Obstacles = ModelDefinition.Obstacles;
            TimePointData = ModelDefinition.TimePointData;
            ObservationPoints = ModelDefinition.ObservationPoints;
            ObservationCrossSections = ModelDefinition.ObservationCrossSections;
           
            disposableItems.Add(new FeatureDataSyncer<Feature2D, WaveBoundaryCondition>(Boundaries, BoundaryConditions,
                                                                                    f => CreateWaveBoundaryCondition(f, this)));
        }

        private static void BuildModelFromMdw(WaveModel model, string mdwFilePath)
        {
            model.MdwFile.MdwFilePath = mdwFilePath;
            model.Name = Path.GetFileNameWithoutExtension(mdwFilePath);
            model.ModelDefinition = model.mdwFile.Load(mdwFilePath);

            model.SyncModelTimesWithBase();

            var mdwDir = Path.GetDirectoryName(mdwFilePath);
            var allDomains = WaveDomainHelper.GetAllDomains(model.ModelDefinition.OuterDomain);
           
            model.BuildWaveDomains(allDomains, mdwDir, model);
            
            var convertedBoundaries =
                WaveBoundaryImportHelper.ConvertToCoordinateBased(model.ModelDefinition.OrientedBoundaryConditions,
                                                                  model.ModelDefinition.OuterDomain.Grid).ToList();

            model.ModelDefinition.BoundaryConditions.AddRange(convertedBoundaries);
            model.ModelDefinition.OrientedBoundaryConditions.Clear();
            
            // snap boundaries to grid
            var tempSnapApi = new WaveGridOperationApi(model.ModelDefinition.OuterDomain.Grid);
            var snappedBoundaries = model.ModelDefinition.BoundaryConditions.Select(b =>
                {
                    if (model.gridOperationApi != null)
                        b.Feature.Geometry = tempSnapApi.GetGridSnappedGeometry("boundaries", b.Feature.Geometry);
                    return b.Feature;
                });
            model.Boundaries.AddRange(snappedBoundaries);
            model.LoadSp2Boundary();
        }

        public MdwFile MdwFile
        {
            get { return mdwFile; }
        }

        private static void BuildEmptyModel(WaveModel model)
        {
            model.ModelDefinition = new WaveModelDefinition
            {
                OuterDomain = new WaveDomainData("Outer")
            };
        }

        public void AddSubDomain(WaveDomainData domain, WaveDomainData subDomain)
        {
            domain.SubDomains.Add(subDomain);
            subDomain.SuperDomain = domain;
            AddDataItemsForDomain(subDomain);
        }

        public void DeleteSubDomain(WaveDomainData domain, WaveDomainData subDomain)
        {
            domain.SubDomains.Remove(subDomain);
            RemoveDataItemsForDomain(subDomain);
        }

        private void BoundariesPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            var feature2D = sender as Feature2D;
            previousFeatureGeometry = feature2D != null && Boundaries.Contains(feature2D)
                                        ? feature2D.Geometry
                                        : null;
        }

        private void OnModelDefinitionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var prop = (WaveModelProperty)sender;
            if (e.PropertyName == TypeUtils.GetMemberName(() => prop.Value))
            {
                if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.SimulationMode,
                                                                   StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching simulation mode"));
                    SimulationMode = SimulationMode;
                    EndEdit();
                }
                else if(prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.WriteCOM, StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching write COM"));
                    WriteCOM = WriteCOM;
                    if (!WriteCOM)
                    {
                        ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.COMFile).SetValueAsString("");
                    }
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
                else if (prop.PropertyDefinition.FilePropertyName.Equals(KnownWaveProperties.BedFriction,
                                                                         StringComparison.InvariantCultureIgnoreCase))
                {
                    BeginEdit(new DefaultEditAction("Switching BedFriction"));
                    BedFriction = BedFriction;
                    EndEdit();
                }
            }
        }

        private void BoundariesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var feature2D = sender as Feature2D;

            if (snappingGeometry || feature2D == null || e.PropertyName != TypeUtils.GetMemberName(() => feature2D.Geometry))
                return;

            snappingGeometry = true;
            
            try
            {
                feature2D.Geometry = GetGridSnappedBoundary(feature2D.Geometry) ?? previousFeatureGeometry;
            }
            finally
            {
                snappingGeometry = false;
            }
        }

        public string ImportIntoModelDirectory(string filePath)
        {
            return WaveModelFileHelper.ImportIntoModelDirectory(Path.GetDirectoryName(MdwFilePath), filePath);
        }

        public bool BoundaryIsDefinedBySpecFile
        {
            get { return ModelDefinition.BoundaryIsDefinedBySpecFile; }
            set
            {
                if (value == ModelDefinition.BoundaryIsDefinedBySpecFile) return;

                ModelDefinition.BoundaryIsDefinedBySpecFile = value;

                // load sp2 file:
                if (ModelDefinition.BoundaryIsDefinedBySpecFile)
                {
                    Boundaries.Clear();
                    LoadSp2Boundary();
                }
                else
                {
                    Sp2Boundaries.Clear();
                }
            }
        }

        public string OverallSpecFile
        {
            get { return ModelDefinition.OverallSpecFile; }
            set
            {
                if (value == ModelDefinition.OverallSpecFile) return;

                ModelDefinition.OverallSpecFile = value;
                LoadSp2Boundary();
            }
        }

        private static WaveBoundaryCondition CreateWaveBoundaryCondition(Feature2D f, WaveModel model)
        {
            // default condition: parametrized and uniform
            var waveBoundaryCondition = (WaveBoundaryCondition)
                     (new WaveBoundaryConditionFactory().CreateBoundaryCondition(f, WaveBoundaryCondition.WaveQuantityName,
                                                                                 BoundaryConditionDataType
                                                                                     .ParametrizedSpectrumConstant));
            waveBoundaryCondition.Name = NamingHelper.GetUniqueName("BoundaryCondition" + "{0:D2}",
                                                                    model.BoundaryConditions.OfType<INameable>());
            return waveBoundaryCondition;
        }
        
        private void BuildWaveDomains(IEnumerable<WaveDomainData> allDomains, string workingDirectory, WaveModel model)
        {
            foreach (var domain in allDomains)
            {
                LoadGrid(workingDirectory, domain);
                LoadBathymetry(model, workingDirectory, domain);
            }
        }

        public void SyncWithModelDefaults(WaveDomainData domain)
        {
            // only when set to default, we shouldn't overwrite domain-parameters
            var spectral = domain.SpectralDomainData;
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
            var hydro = domain.HydroFromFlowData;
            if (hydro.UseDefaultHydroFromFlowSettings)
            {
                hydro.BedLevelUsage = ModelDefinition.DefaultBedLevelUsage;
                hydro.WaterLevelUsage = ModelDefinition.DefaultWaterLevelUsage;
                hydro.VelocityUsage = ModelDefinition.DefaultVelocityUsage;
                hydro.VelocityUsageType = ModelDefinition.DefaultVelocityUsageType;
                hydro.WindUsage = ModelDefinition.DefaultWindUsage;
            }
        }

        public static void LoadGrid(string workingDirectory, WaveDomainData domain)
        {
            var grdFilePath = Path.Combine(workingDirectory, domain.GridFileName);

            var grid = File.Exists(grdFilePath)
                           ? Delft3DGridFileReader.Read(grdFilePath)
                           : CurvilinearGrid.CreateDefault();
            grid.Name = "Grid (" + Path.GetFileNameWithoutExtension(grdFilePath) + ")";
            domain.Grid = grid;
        }

        public static void LoadBathymetry(WaveModel model, string directory, WaveDomainData domain)
        {
            var grid = domain.Grid;
            var bathymetry = new CurvilinearCoverage(grid.Size1, grid.Size2, grid.X.Values, grid.Y.Values)
                {
                    Name = "Bathymetry (" + Path.GetFileNameWithoutExtension(domain.BedLevelFileName) + ")"
                };
            bathymetry.Components[0].NoDataValue = -999.0;
            bathymetry.Components[0].DefaultValue = bathymetry.Components[0].NoDataValue;

            var depthFilePath = Path.Combine(directory, domain.BedLevelFileName);
            if (File.Exists(depthFilePath) && !grid.IsEmpty)
            {
                var bathymetryValues = Delft3DDepthFileReader.Read(depthFilePath, grid.Size1, grid.Size2).ToList();

                if (bathymetryValues.Count != grid.Size2*grid.Size1)
                {
                    Log.ErrorFormat(
                        "Failed to load bathymetry; data in file does not match the size of the target grid: {0}x{1}",
                        grid.Size1, grid.Size2);
                    return;
                }

                bathymetry.SetValues(bathymetryValues);
            }

            var di = model.DataItems.FirstOrDefault(d => d.Name == domain.Bathymetry.Name);

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

        private void LoadSp2Boundary()
        {
            Sp2Boundaries.Clear();

            var mdwDirectory = Path.GetDirectoryName(MdwFilePath);

            if (!ModelDefinition.BoundaryIsDefinedBySpecFile) return;
            if (mdwDirectory == null || ModelDefinition.OverallSpecFile == null) return;

            var sp2FilePath = Path.Combine(mdwDirectory, ModelDefinition.OverallSpecFile);
            if (!File.Exists(sp2FilePath)) return;

            var coordinates = new Sp2File().Read(sp2FilePath).Keys.ToList();

            if (coordinates.Count < 2) return;

            coordinates.Add(coordinates[0]);

            Sp2Boundaries.Add(new Feature2D
            {
                Name = Path.GetFileNameWithoutExtension(ModelDefinition.OverallSpecFile),
                Geometry = new LineString(coordinates.ToArray())
            });
        }

        public ICoordinateSystem CoordinateSystem
        {
            get { return coordinateSystem; }
            set
            {
                coordinateSystem = value;
                AfterCoordinateSystemSet();
            }
        }

        [EditAction]
        private void AfterCoordinateSystemSet()
        {
            var domains = WaveDomainHelper.GetAllDomains(OuterDomain);
            domains.ForEach(d =>
                {
                    d.Grid.CoordinateSystem = coordinateSystem;
                    d.Bathymetry.CoordinateSystem = coordinateSystem;
                   
                    if (d.Grid.CoordinateSystem != null && d.Grid.Attributes.ContainsKey(CurvilinearGrid.CoordinateSystemKey))
                    {
                        d.Grid.Attributes[CurvilinearGrid.CoordinateSystemKey] = 
                            d.Grid.CoordinateSystem.IsGeographic ? "Spherical" : "Cartesian";
                    }
                });
        }

        public bool CanSetCoordinateSystem(ICoordinateSystem potentialCoordinateSystem)
        {
            return WaveModelCoordinateConversion.IsSaneCoordinateSystemForModel(this, potentialCoordinateSystem);
        }

        public static bool IsValidCoordinateSystem(ICoordinateSystem system)
        {
            return !system.IsGeographic || system.Name == "WGS 84";
        }
        
        public void TransformCoordinates(ICoordinateTransformation transformation)
        {
            BeginEdit(new DefaultEditAction("Converting model coordinates"));

            WaveModelCoordinateConversion.Transform(this, transformation);
            CoordinateSystem = (ICoordinateSystem) transformation.TargetCS;

            EndEdit();

            // grid(s) transformed, sync data to disk:
            var modelDir = Path.GetDirectoryName(mdwFile.MdwFilePath);
            foreach (var domain in WaveDomainHelper.GetAllDomains(OuterDomain))
            {
                var targetGridFileName = Path.Combine(modelDir, domain.GridFileName);
                Delft3DGridFileWriter.Write(domain.Grid, targetGridFileName);
            }
        }

        private void ModelSave()
        {
            ModelSaveTo(mdwFile.MdwFilePath, true);
        }


        // all saving should go through here, but beware, NHibernate will disable
        // event bubbling when saving...
        public void ModelSaveTo(string targetMdwFilePath, bool switchTo)
        {
            var targetDir = Path.GetDirectoryName(targetMdwFilePath);
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);
            
            mdwFile.SaveTo(targetMdwFilePath, ModelDefinition, switchTo);

            // write spatial data:
            SaveBathymetries(WaveDomainHelper.GetAllDomains(OuterDomain), targetDir);

            SaveOutput(targetDir, switchTo);
        }

        private void SaveOutput(string targetDirectory, bool switchTo)
        {
            foreach (var wavmFileFunctionStore in WavmFunctionStores)
            {
                var oldPath = wavmFileFunctionStore.Path;

                if (string.IsNullOrEmpty(oldPath)) continue;
                
                var newPath = Path.Combine(targetDirectory, Path.GetFileName(wavmFileFunctionStore.Path));
                    
                if (String.Equals(Path.GetFullPath(oldPath), Path.GetFullPath(newPath),
                    StringComparison.CurrentCultureIgnoreCase)) continue;

                if (File.Exists(oldPath))
                {
                    File.Copy(oldPath, newPath, true);

                    if (switchTo)
                    {
                        wavmFileFunctionStore.Path = newPath;
                    }
                }
            }
        }

        private void SaveBathymetries(IEnumerable<WaveDomainData> allDomains, string projectPath)
        {
            foreach (var domain in allDomains)
            {
                if (!domain.Bathymetry.X.Values.Any()) continue;

                var targetFile = Path.Combine(projectPath, domain.BedLevelFileName);
                var sizeM = domain.Bathymetry.Size2;
                var sizeN = domain.Bathymetry.Size1;
                Delft3DDepthFileWriter.Write(domain.Bathymetry.GetValues<double>().ToArray(), sizeN, sizeM,
                                             targetFile);
            }
        }

        public string WorkingDirectory
        {
            get { return ExplicitWorkingDirectory ?? tempWorkingDirectory; }
        }

        private string SafeMdwFileName
        {
            get
            {
                return Path.GetFileName(MdwFilePath).Replace(" ", "_");
            }
        }

        public override DateTime StartTime
        {
            get { return startTime; }
            set
            {
                startTime = value;
                // This base model setting is made to make the base logic right
                base.StartTime = value;
            }
        }

        public override DateTime StopTime
        {
            get { return stopTime; }
            set
            {
                stopTime = value;
                // This base model setting is made to make the base logic right
                base.StopTime = value;
            }
        }

        public override TimeSpan TimeStep
        {
            get { return timeStep; }
            set
            {
                timeStep = value;
                // This base model setting is made to make the base logic right
                base.TimeStep = value;
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

        protected override void OnInitialize()
        {
            if (IsRunByDimr) return;

            waveApi = new RemoteWaveModelApi(ShowModelRunConsole)
            {
                ReferenceDateTime = ModelDefinition.ModelReferenceDateTime
            };

            if (ValidateBeforeRun)
            {
                var report = Validate();
                if (report.Severity() == ValidationSeverity.Error)
                {
                    var errorIssues = report.GetAllIssuesRecursive().Where(i => i.Severity == ValidationSeverity.Error);
                    var errorMessage = string.Format("Validation errors: {0}",
                        string.Join("\n", errorIssues.Select(
                            i => string.Format("\t{0}: {1}", i.Subject,
                                i.Message)).ToArray()));
                    throw new InvalidOperationException(
                        "Model validation failed; please review the validation report.\n\r" + errorMessage);
                }
            }

            var filePath = Path.Combine(WorkingDirectory, SafeMdwFileName);

            if (Directory.Exists(WorkingDirectory))
                Directory.Delete(WorkingDirectory, true);
            Directory.CreateDirectory(WorkingDirectory);

            if (IsCoupledToFlow)
            {
                var flowComFilePath = GetFlowComFilePath();
                var comFileProperty = ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.COMFile);
                comFileProperty.Value = FileUtils.GetRelativePath(WorkingDirectory, flowComFilePath);
            }

            ModelSaveTo(filePath, false);

            waveApi.SetVar("mode", IsCoupledToFlow ? "online with DflowFM" : "stand-alone");
            if (!IsCoupledToFlow)
            {
                waveApi.Initialize(filePath);
            }
            else
            {
                lazyInitializationFlag = true;
            }
        }

        public virtual ValidationReport Validate()
        {
            return new WaveModelValidator().Validate(this);
        }
        public virtual Func<string> GetFlowComFilePath { get; set; }

        private bool lazyInitializationFlag;

        protected override void OnExecute()
        {
            if (IsRunByDimr) return;

            
            if (!IsCoupledToFlow)
            {
                // dt = total seconds of the last time moment to calculate minus the reference time should be sent as parameter. (in seconds) 
                // will run wave on all timepoints
                var timestep = StopTime - ModelDefinition.ModelReferenceDateTime;
                waveApi.Update(timestep.TotalSeconds);
                CurrentTime = StopTime;
            }
            else
            {
                if (lazyInitializationFlag)
                {
                    waveApi.Initialize(Path.Combine(WorkingDirectory, SafeMdwFileName));
                    lazyInitializationFlag = false;
                }
                // wave has its own timesteps, not necessarily equal to those in flow-fm
                waveApi.Update(CurrentTime == StartTime ? 0 : TimeStep.TotalSeconds);
                CurrentTime += TimeStep;
            }

            if (CurrentTime >= StopTime)
            {
                Status = ActivityStatus.Done;

                var swanDiagFile = Path.Combine(WorkingDirectory, "swn-diag." + Path.GetFileNameWithoutExtension(SafeMdwFileName));
                var swanLog = GetDataItemValueByTag<TextDocument>(SwanLogDataItemTag);
                swanLog.Content = "";
                if (File.Exists(swanDiagFile))
                {
                    swanLog.Content = File.ReadAllText(swanDiagFile);
                }
                else
                {
                    var swanPrintFile = Path.Combine(WorkingDirectory, "PRINT");
                    if (File.Exists(swanPrintFile))
                    {
                        swanLog.Content = string.Format("Errors running Swan, content of {0}:", swanPrintFile);
                        swanLog.Content += Environment.NewLine;
                        swanLog.Content += Environment.NewLine;
                        swanLog.Content += new StreamReader(swanPrintFile).ReadToEnd();
                    }
                }

                ReconnectWavmFile(WorkingDirectory);
            }
        }

        protected override void OnFinish()
        {
            if (IsRunByDimr) return;

            if (waveApi != null)
            {
                waveApi.Finish();
            }
        }

        protected override void OnCleanup()
        {
            if (waveApi == null)
                return; // we never got past validation..

            waveApi.Dispose();
            waveApi = null;
            lazyInitializationFlag = false;

            base.OnCleanup();
        }

        public void Dispose()
        {
            if (waveApi != null)
                waveApi.Dispose();
            RestoreEnvironment();
            if (disposableItems != null)
            {
                disposableItems.ForEach(d => d.Dispose());
                disposableItems.Clear();
                disposableItems = null;
            }
        }

        private static void RestoreEnvironment()
        {
            var oldD3DHome = Environment.GetEnvironmentVariable("OLD_D3D_HOME");
            if (!string.IsNullOrEmpty(oldD3DHome))
            {
                Environment.SetEnvironmentVariable("D3D_HOME", oldD3DHome);
                Environment.SetEnvironmentVariable("OLD_D3D_HOME", null);
            }

            var oldArch = Environment.GetEnvironmentVariable("OLD_ARCH");
            if (!string.IsNullOrEmpty(oldArch))
            {
                Environment.SetEnvironmentVariable("ARCH", oldArch);
                Environment.SetEnvironmentVariable("OLD_ARCH", null);
            }
            WaveModelApi.WaveDllHelper.DimrRun = false;
        }

        protected override void OnCancel()
        {
            if (waveApi == null)
                return; // we never got past validation..

            waveApi.Dispose();
            waveApi = null;

            base.OnCancel();
        }

        protected virtual void ReconnectWavmFile(string outputPath)
        {
            var domains = WaveDomainHelper.GetAllDomains(OuterDomain).ToList();
            if (domains.Count > 1)
            {
                for (var i = 0; i < domains.Count; ++i)
                {
                    var wavmFile = Path.Combine(outputPath, "wavm-" + Name + "-" + domains[i].Name + ".nc");
                    if (File.Exists(wavmFile))
                    {
                        BeginEdit(new DefaultEditAction("Reconnect output (WAVM) file"));

                        WavmFunctionStores.ElementAt(i).Path = wavmFile;

                        EndEdit();
                    }
                }
            }
            else
            {
                var wavmFile = Path.Combine(outputPath, "wavm-" + Name + ".nc");
                if (File.Exists(wavmFile))
                {
                    BeginEdit(new DefaultEditAction("Reconnect output (WAVM) file"));

                    WavmFunctionStores.First().Path = wavmFile;

                    EndEdit();
                }
            }
            OutputIsEmpty = false;
        }

        protected override void OnClearOutput()
        {
            BeginEdit(new DefaultEditAction("Clearing all wave output"));
            WavmFunctionStores.ForEach(
                fs => fs.Close());
            EndEdit();
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

        private void LoadWaveDomain(WaveDomainData domain)
        {
            LoadGrid(Path.GetDirectoryName(MdwFilePath), domain);

            UpdateBathymetry(domain);
            UpdateBathymetryOperations(domain);
        }

        private static void UpdateBathymetry(WaveDomainData domain)
        {
            domain.Bathymetry.Resize(domain.Grid.Size1, domain.Grid.Size2, domain.Grid.X.Values, domain.Grid.Y.Values);
        }

        private void UpdateBathymetryOperations(WaveDomainData domain)
        {
            var dataItem = GetDataItemByValue(domain.Bathymetry);
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

        public void ResnapBoundaries()
        {
            try
            {
                BeginEdit(new DefaultEditAction("Snap boundaries"));
                var unsnappable = new List<Feature2D>();
                foreach (var b in Boundaries)
                {
                    if (gridOperationApi != null)
                    {
                        var snappedGeometry = GetGridSnappedBoundary(b.Geometry);
                        if (snappedGeometry == null)
                        {
                            unsnappable.Add(b);
                            continue;
                        }
                        b.Geometry = snappedGeometry;
                    }
                }

                // delete
                unsnappable.ForEach(b => Boundaries.Remove(b));
            }
            finally
            {
                EndEdit();
            }
        }

        public IGeometry GetGridSnappedGeometry(string featureType, IGeometry geometry)
        {
            return gridOperationApi != null ? gridOperationApi.GetGridSnappedGeometry(featureType, geometry) : geometry;
        }

        public bool SnapsToGrid(IGeometry geometry)
        {
            throw new NotImplementedException();
        }

        public IGeometry GetGridSnappedBoundary(IGeometry geometry)
        {
            // couldn't think of a better way to do this, for now..
            if (BoundaryIsDefinedBySpecFile)
            {
                Log.WarnFormat("Cannot add boundaries when the model boundary is defined by Swan spectrum file (*.sp2)");
                return null;
            }
            return GetGridSnappedGeometry("boundaries", geometry);
        }

        public IEnumerable<IGeometry> GetGridSnappedGeometry(string featureType, ICollection<IGeometry> geometries)
        {
            throw new NotImplementedException();
        }

        public int[] GetLinkedCells()
        {
            throw new NotImplementedException();
        }

        internal void SyncModelTimesWithBase()
        {
            base.StartTime = StartTime;
            base.StopTime = StopTime;
            base.TimeStep = TimeStep;
        }

        private void OnAddedToProject(string mdwFilePath)
        {
            // implicit switch
            ModelSaveTo(mdwFilePath, true);
        }

        private void OnSave()
        {
            ModelSave();
        }

        private void OnCopyTo(string targetMdwFilePath)
        {
            ModelSaveTo(targetMdwFilePath, false);
        }

        private void OnSwitchTo(string newMdwFilePath)
        {
            if (mdwFile.MdwFilePath == null)
            {
                BuildModel(model => BuildModelFromMdw(model, newMdwFilePath),true);
            }
            else
            {
                mdwFile.MdwFilePath = newMdwFilePath;
            }
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            foreach (var item in base.GetDirectChildren())
                yield return item;

            yield return TimePointData;

            foreach (var domain in WaveDomainHelper.GetAllDomains(ModelDefinition.OuterDomain))
            {
                yield return domain.Grid;
                yield return domain.Bathymetry;
            }

            yield return BoundaryConditions;

            foreach (var bc in BoundaryConditions)
            {
                yield return bc;
            }

            foreach (var wavmFileFunctionStore in WavmFunctionStores)
            {
                if (wavmFileFunctionStore != null && !string.IsNullOrEmpty(wavmFileFunctionStore.Path))
                {
                    yield return wavmFileFunctionStore;
                }
            }
        }

        #region IFileBased and NHibernate

        //tells NHibernate we need to be saved
        private void MarkDirty()
        {
            unchecked { dirtyCounter++; }
        }
        private int dirtyCounter;

        private string path;
        private WaveModelDefinition modelDefinition;
        private bool isCoupledToFlow;

        private DateTime startTime;

        private DateTime stopTime;

        private TimeSpan timeStep;
        //private IHydroRegion region;

        string IFileBased.Path
        {
            get { return path; }
            set
            {
                if (path == value)
                    return;

                path = value;

                if (path == null)
                    return;

                if (path.StartsWith("$") && IsOpen)
                {
                    OnSave();
                }
            }
        }

        public IEnumerable<string> Paths
        {
            get { yield return ((IFileBased) this).Path; }
        }

        public bool IsFileCritical { get { return true; } }

        public bool IsOpen { get; private set; }

        public virtual string MdwFilePath
        {
            get { return mdwFile != null ? mdwFile.MdwFilePath : null; }
        }
        
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
        }

        private string GetMdwPathFromDeltaShellPath(string dsPath)
        {
            // dsproj_data/<model name>/<model name>.mdw
            return Path.Combine(Path.GetDirectoryName(dsPath), Path.Combine(Name, Name + ".mdw"));
        }


        #endregion

        public override IProjectItem DeepClone()
        {
            var tempDir = FileUtils.CreateTempDirectory();
            var fileName = Path.GetFileName(MdwFilePath);
            var tempFilePath = Path.Combine(tempDir, fileName);
            ModelSaveTo(tempFilePath, false);

            return new WaveModel(tempFilePath);
        }

        public IHydroRegion Region
        {
            //wave doesn't have a region but can be part of a integrated model
            get { return null; }
        }
        #region IDimrModel

        public virtual string LibraryName
        {
            get { return "wave"; }
        }

        public virtual string InputFile
        {
            get { return Path.GetFileName(MdwFilePath); }
        }

        public virtual string DirectoryName
        {
            get { return "wave"; }
        }

        public virtual bool IsMasterTimeStep
        {
            get { return false; }
        }

        public virtual string ShortName
        {
            get { return "wave"; }
        }

        public virtual string GetItemString(IDataItem value)
        {
            return null;
        }

        public virtual Type ExporterType
        {
            get { return typeof(WaveModelFileExporter); }
        }

        public virtual string GetExporterPath(string directoryName)
        {
            return Path.Combine(directoryName, MdwFilePath == null ? Name + ".mdw" : Path.GetFileName(MdwFilePath));
        }

        public virtual bool CanRunParallel
        {
            get { return true; }
        }

        public virtual string MpiCommunicatorString { get { return null; } }

        public virtual string KernelDirectoryLocation
        {
            get
            {
                using (var waveDllHelper = new WaveModelApi.WaveDllHelper(string.Empty))
                {
                    WaveModelApi.WaveDllHelper.DimrRun = true;
                    return waveDllHelper.WaveExeDir + ";" + waveDllHelper.SwanExeDir + ";" + waveDllHelper.SwanScriptDir + ";" + waveDllHelper.EsmfPath + ";" + waveDllHelper.EsmfScriptPath;
                }
            }
        }

        public virtual void DisconnectOutput()
        {
            OnClearOutput();
        }

        public virtual void ConnectOutput(string outputPath)
        {
            ReconnectWavmFile(Path.Combine(outputPath, DirectoryName));
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
            //wave doesnt run standalone via dimr but via kernels
            return new[] {default(double)};
        }
        public virtual void SetVar(Array values, string category, string itemName = null, string parameter = null)
        {
            //wave doesnt run standalone via dimr but via kernels
        }
        #endregion
    }
}