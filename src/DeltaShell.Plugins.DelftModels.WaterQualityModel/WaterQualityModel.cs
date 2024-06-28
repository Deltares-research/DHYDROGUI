using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataItemMetaData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api.SpatialOperations;

[assembly: InternalsVisibleTo(" DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests")]

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel
{
    [Entity]
    public class WaterQualityModel : TimeDependentModelBase, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterQualityModel));

        /// <summary>
        /// Occurs when the hydro data has changed (file has been edited) (async event)
        /// </summary>
        public virtual event EventHandler<EventArgs> HydroDataChanged;

        public WaterQualityModel() : base("Water Quality")
        {
            modelSettings = new WaterQualityModelSettings {MonitoringOutputLevel = MonitoringOutputLevel.PointsAndAreas};

            InitializeInputDataItems();
            InitializeWaqProcessesRules();

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

            AddDataItemSet(new EventedList<UnstructuredGridCellCoverage>(), OutputSubstancesDataItemMetaData.Name,
                           DataItemRole.Output, OutputSubstancesDataItemMetaData.Tag);
            AddDataItemSet(new EventedList<UnstructuredGridCellCoverage>(), OutputParametersDataItemMetaData.Name,
                           DataItemRole.Output, OutputParametersDataItemMetaData.Tag);

            if (modelSettings.MonitoringOutputLevel != MonitoringOutputLevel.None)
            {
                AddDataItemSet(new EventedList<WaterQualityObservationVariableOutput>(),
                               MonitoringOutputDataItemMetaData.Name, DataItemRole.Output,
                               MonitoringOutputDataItemMetaData.Tag);
            }

            SubscribeToInternalEvents();
            enableMarkOutputOutOfSync = true;
        }

        /// <summary>
        /// Overriden to synchronize the StartTime and the output timers.
        /// </summary>
        public override DateTime StartTime
        {
            get => base.StartTime;
            set
            {
                if (StartTime == value)
                {
                    return;
                }

                base.StartTime = value;

                ModelSettings.BalanceStartTime = StartTime;
                ModelSettings.MapStartTime = StartTime;
                ModelSettings.HisStartTime = StartTime;
                LogSynchronizedTimer("Start Time", StartTime);
            }
        }

        /// <summary>
        /// Overriden to synchronize the StopTime and the output timers.
        /// </summary>
        public override DateTime StopTime
        {
            get => base.StopTime;
            set
            {
                if (StopTime == value)
                {
                    return;
                }

                base.StopTime = value;

                ModelSettings.BalanceStopTime = StopTime;
                ModelSettings.MapStopTime = StopTime;
                ModelSettings.HisStopTime = StopTime;
                LogSynchronizedTimer("Stop Time", StopTime);
            }
        }

        public virtual bool UseRestart { get; set; }

        public virtual bool WriteRestart { get; set; }

        /// <summary>
        /// Imports the contents of a HydFile into the WAQ model.
        /// </summary>
        /// <param name="data"> Contents from a HydFile (or generated HydroData). </param>
        /// <param name="skipImportTimers"> Optional parameter (default False). </param>
        /// <param name="markOutputOutOfSync"> Optional parameter (default True). </param>
        public virtual void ImportHydroData(IHydroData data,
                                            bool skipImportTimers = false, bool markOutputOutOfSync = true)
        {
            if (data == null)
            {
                HasHydroDataImported = false;
                throw new ArgumentNullException(nameof(data), "No hydrodynamics data was specified.");
            }

            //As per issue D3DFMIQ-318, we should override the coordinate system with the imported one. 
            bool coordinateSystemChanges = CoordinateSystem != data.Grid?.CoordinateSystem;
            CoordinateSystem = data.Grid?.CoordinateSystem;
            if (coordinateSystemChanges)
            {
                Log.Info(
                    string.Format(
                        Resources
                            .WaterQualityModel_ImportHydroData_The_coordinate_system_of_the_model___0__has_been_set_to__1_,
                        Name,
                        data.Grid?.CoordinateSystem == null
                            ? "<empty>"
                            : data.Grid.CoordinateSystem.ToString()));
            }

            if (data.Equals(HydroData))
            {
                OverWriteModelTimersWithImportTimers(skipImportTimers, data);
                OverWriteSegmentFunctions(data);
                return;
            }

            HasHydroDataImported = false;

            enableMarkOutputOutOfSync = markOutputOutOfSync;

            bool schematizationRemainsUnchanged = data.HasSameSchematization(HydroData);

            try
            {
                BeginEdit("Importing hydrodynamics data");
                HydroData = data;

                importingHydroData = true;

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

                //As of issue D3DFMIQ-329, the timers should be overriden when importing the hyd file again.
                OverWriteModelTimersWithImportTimers(skipImportTimers, HydroData);

                SetImportProgress("Importing file paths");
                AreasRelativeFilePath = HydroData.AreasRelativePath;
                VolumesRelativeFilePath = HydroData.VolumesRelativePath;
                FlowsRelativeFilePath = HydroData.FlowsRelativePath;
                PointersRelativeFilePath = HydroData.PointersRelativePath;
                LengthsRelativeFilePath = HydroData.LengthsRelativePath;
                VerticalDiffusionRelativeFilePath = HydroData.VerticalDiffusionRelativePath;
                GridRelativeFilePath = HydroData.GridRelativePath;
                AttributesRelativeFilePath = HydroData.AttributesRelativePath;
                OverWriteSegmentFunctions(HydroData);

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
                var fileInfo =
                    new FileInfo(Path.Combine(Path.GetDirectoryName(HydroData.FilePath), AttributesRelativeFilePath));

                attributeData =
                    AttributesFileReader.ReadAll(NumberOfDelwaqSegmentsPerHydrodynamicLayer, NumberOfWaqSegmentLayers,
                                                 fileInfo);
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

        /// <summary>
        /// Determines whether the model has data available in its hydro dynamics for a
        /// specific function, process or substance.
        /// </summary>
        /// <param name="function"> The function. </param>
        /// <returns> True if there is data defined in the hydro dynamics, false otherwise. </returns>
        public virtual bool HasDataInHydroDynamics(IFunction function)
        {
            return function != null && HasDataInHydroDynamics(function.Name);
        }

        /// <summary>
        /// Determines whether the model has data available in its hydro dynamics for a
        /// specific function, process or substance.
        /// </summary>
        /// <param name="functionName"> The name of the function. </param>
        /// <returns> True if there is data defined in the hydro dynamics, false otherwise. </returns>
        public virtual bool HasDataInHydroDynamics(string functionName)
        {
            return HydroData != null && HydroData.HasDataFor(functionName);
        }

        /// <summary>
        /// Gets the file path for a given function, process or substance when available
        /// in the hydro dynamics.
        /// </summary>
        /// <param name="function"> The funcion. </param>
        /// <returns>
        /// The filepath for the given function if <see cref="HasDataInHydroDynamics(IFunction)"/>
        /// returns true for <paramref name="function"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// When <see cref="HasDataInHydroDynamics(IFunction)"/>
        /// returns false for <paramref name="function"/>.
        /// </exception>
        public virtual string GetFilePathFromHydroDynamics(IFunction function)
        {
            if (HydroData == null || !HydroData.HasDataFor(function.Name))
            {
                throw new InvalidOperationException(
                    string.Format("Function '{0}' is not available in the hydro data.", function.Name));
            }

            return HydroData.GetFilePathFor(function.Name);
        }

        /// <summary>
        /// Determines whether the given coordinate falls within an active cell or not.
        /// </summary>
        /// <returns> True if the cell is active; false when it's inactive. </returns>
        /// <exception cref="System.InvalidOperationException"> When no hydro data has been importer. </exception>
        public virtual bool IsInsideActiveCell(Coordinate coordinate)
        {
            if (!HasHydroDataImported)
            {
                throw new InvalidOperationException(
                    "Cannot determine if location is inside active cell as no hydro dynamic data was imported.");
            }

            int index = GetSegmentIndexForLocation(coordinate);
            return attributeData.IsSegmentActive(index);
        }

        /// <summary>
        /// Determines whether the given coordinate falls within an active cell or not.
        /// </summary>
        /// <returns> True if the cell is active; false when it's inactive. </returns>
        /// <exception cref="System.InvalidOperationException"> When no hydro data has been importer. </exception>
        public virtual bool IsInsideActiveCell2D(Coordinate coordinate)
        {
            if (!HasHydroDataImported)
            {
                throw new InvalidOperationException(
                    "Cannot determine if location is inside active cell as no hydro dynamic data was imported.");
            }

            int index = GetSegmentIndexForLocation2D(coordinate);
            return attributeData.IsSegmentActive(index);
        }

        /// <summary>
        /// Returns the cell index for a given location.
        /// </summary>
        public virtual int GetSegmentIndexForLocation(Coordinate coordinate)
        {
            if (!HasHydroDataImported)
            {
                throw new InvalidOperationException(
                    "Cannot determine grid cell index for location as no hydro dynamic data was imported.");
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
                throw new InvalidOperationException(
                    "Cannot determine grid cell index for location as no hydro dynamic data was imported.");
            }

            return pointToGridCellMapper.GetWaqSegmentIndex2D(coordinate.X, coordinate.Y);
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
                    throw new InvalidOperationException($"{nameof(LayerType)} is not a valid {typeof(LayerType)}");
            }
        }

        public virtual void SetEnableMarkOutputOutOfSync(bool enableMarkOutputOutOfSyncValue)
        {
            enableMarkOutputOutOfSync = enableMarkOutputOutOfSyncValue;
        }

        public override IProjectItem DeepClone()
        {
            throw new NotSupportedException("WaterQualityModel does not support cloning.");
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            foreach (object directChild in base.GetDirectChildren())
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
            yield return OutputFolder;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                HydroData = null;
            }
        }

        /// <summary>
        /// Method to connect the DeltaShell framework working directory to the ModelSettings.WorkingDirectory.
        /// The model also adds a folder with the model name to the path.
        /// </summary>
        /// <param name="workingDirectoryWithoutModelName">Function to get the working directory without the model name.</param>
        protected internal virtual void SetWorkingDirectoryInModelSettings(Func<string> workingDirectoryWithoutModelName)
        {
            modelSettings.WorkingDirectoryPathFuncWithModelName = () => Path.Combine(workingDirectoryWithoutModelName(), GetWaqDataFolderName());
        }

        protected override void OnInputCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!enableMarkOutputOutOfSync)
            {
                return;
            }

            this.InputCollectionChanged(e);

            MarkOutputOutOfSync();
        }

        protected override void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!enableMarkOutputOutOfSync)
            {
                return;
            }

            this.InputPropertyChanged(sender, e);

            MarkOutputOutOfSync();
        }

        private void OverWriteSegmentFunctions(IHydroData data)
        {
            SetImportProgress("Sync of segment functions");
            SurfacesRelativeFilePath = data.SurfacesRelativePath;
            VelocitiesFilePath = data.VelocitiesRelativePath;
            WidthsFilePath = data.WidthsRelativePath;
            ChezyCoefficientsFilePath = data.ChezyCoefficientsRelativePath;
            SalinityRelativeFilePath = HydroData.SalinityRelativePath;
            TemperatureRelativeFilePath = HydroData.TemperatureRelativePath;
            ShearStressesRelativeFilePath = HydroData.ShearStressesRelativePath;
        }

        private void OverWriteModelTimersWithImportTimers(bool skipImportTimers, IHydroData dataToOverwrite)
        {
            if (skipImportTimers)
            {
                return;
            }

            SetImportProgress("Importing timers");
            StartTime = dataToOverwrite.ConversionStartTime;
            StopTime = dataToOverwrite.ConversionStopTime;
            TimeStep = dataToOverwrite.ConversionTimeStep;
            ReferenceTime = dataToOverwrite.ConversionReferenceTime;

            //Sync of time step needs to be explicit.
            ModelSettings.HisTimeStep = dataToOverwrite.ConversionTimeStep;
            ModelSettings.MapTimeStep = dataToOverwrite.ConversionTimeStep;
            ModelSettings.BalanceTimeStep = dataToOverwrite.ConversionTimeStep;
            LogSynchronizedTimer("Time Step", TimeStep);
        }

        private void ResolveBoundaryImport(IEnumerable<WaterQualityBoundary> importedBoundaries)
        {
            var newBoundaries = new List<WaterQualityBoundary>();
            if (importedBoundaries != null)
            {
                foreach (WaterQualityBoundary waterQualityBoundary in importedBoundaries)
                {
                    // find an already loaded boundary
                    WaterQualityBoundary existingBoundary =
                        Boundaries.FirstOrDefault(b => b.Name == waterQualityBoundary.Name);

                    if (existingBoundary != null)
                    {
                        // copy the location aliases
                        waterQualityBoundary.LocationAliases = existingBoundary.LocationAliases;
                    }

                    newBoundaries.Add(waterQualityBoundary);
                }
            }

            Boundaries.Clear();
            Boundaries.AddRange(newBoundaries);
        }

        private PointToGridCellMapper SetUpPointToGridCellMapper()
        {
            var mapper = new PointToGridCellMapper {Grid = Grid};
            var waqRelativeThicknesses = new double[NumberOfWaqSegmentLayers];
            var hydroIndex = 0;
            for (var i = 0; i < NumberOfWaqSegmentLayers; i++)
            {
                var waqRelativeThickness = 0.0;
                int addUptoHydroLayer = hydroIndex + NumberOfHydrodynamicLayersPerWaqLayer[i];
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
        /// Gets the PROJ4 string representation of the coordinate system.
        /// </summary>
        /// <param name="coordinateSystem">
        /// The <see cref="ICoordinateSystem"/> to retrieve the string
        /// representation for.
        /// </param>
        /// <returns>
        /// A PROJ4 string representation, or an empty string when:
        /// <list type="bullet">
        ///     <item><paramref name="coordinateSystem"/> is <c>null</c>.</item>
        ///     <item>No PROJ4 transformation is available.</item>
        /// </list>
        /// </returns>
        private static string GetProj4CoordinateSystemString(ICoordinateSystem coordinateSystem)
        {
            if (coordinateSystem == null)
            {
                return string.Empty;
            }

            try
            {
                return coordinateSystem.PROJ4;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private void LogSynchronizedTimer(string timer, object value)
        {
            //For some info the test will only pass if the message is contained in the string.Format, but not in the Log.InfoFormat.
            string message =
                string.Format(
                    Resources
                        .WaterQualityModel_LogSynchronizedTimer_Output_timers___0___have_been_synchronized_to_match_the_Simulation__0____1___,
                    timer, value);
            Log.Info(message);
        }

        private void SetNewGrid(UnstructuredGrid value, bool schematizationRemainsUnchanged)
        {
            // never set grid to null (this creates invalid UnstructuredGridCellCoverages)
            UnstructuredGrid gridToSet = value ?? new UnstructuredGrid();

            if (overriddenCoordinateSystem != null)
            {
                gridToSet.CoordinateSystem = overriddenCoordinateSystem;
            }

            GetDataItemByTag(GridDataItemMetaData.Tag).Value = gridToSet;
            Bathymetry = CreateAndFillBathymetryCoverage(gridToSet);

            ReplaceGridOnUnstructuredGridCoveragesWithSpatialOperations(
                GetDataItemSetByTag(InitialConditionsDataItemMetaData.Tag), schematizationRemainsUnchanged);
            ReplaceGridOnUnstructuredGridCoveragesWithSpatialOperations(
                GetDataItemSetByTag(ProcessCoefficientsDataItemMetaData.Tag), schematizationRemainsUnchanged);
            ReplaceGridOnUnstructuredGridCoveragesWithSpatialOperations(
                GetDataItemSetByTag(DispersionDataItemMetaData.Tag), schematizationRemainsUnchanged);
            ReplaceGridOnUnstructuredGridCoverageWithSpatialOperations(
                GetDataItemByTag(ObservationAreasDataItemMetaData.Tag), schematizationRemainsUnchanged);
            ReplaceGridOnUnstructuredGridCoverages(this.GetOutputCoverages(), schematizationRemainsUnchanged);
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
                return "Water_Quality";
            }

            return Name.Replace(" ", "_");
        }

        private void SetWaqPointHeights()
        {
            double defaultZ = GetDefaultZ();

            foreach (WaterQualityLoad load in Loads)
            {
                load.Z = defaultZ;
            }

            foreach (WaterQualityObservationPoint observationPoint in ObservationPoints)
            {
                observationPoint.Z = defaultZ;
            }
        }

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
            AddDataItem(new TextDocument
            {
                Name = InputFileCommandLineDataItemMetaData.Name,
                Content = Resources.TemplateInpFileNew
            }, DataItemRole.Input, InputFileCommandLineDataItemMetaData.Tag);
            AddDataItem(new TextDocument
            {
                Name = InputFileHybridDataItemMetaData.Name,
                Content = Resources.TemplateInpFileHybrid
            }, DataItemRole.Input, InputFileHybridDataItemMetaData.Tag);

            AddDataItem(CreateSubstanceProcessLibrary(), SubstanceProcessLibraryDataItemMetaData.Name,
                        DataItemRole.Input, SubstanceProcessLibraryDataItemMetaData.Tag);

            var initialGrid = new UnstructuredGrid();
            AddDataItem(initialGrid, GridDataItemMetaData.Name, DataItemRole.Input, GridDataItemMetaData.Tag);
            AddDataItem(CreateAndFillBathymetryCoverage(initialGrid), BathymetryDataItemMetaData.Name,
                        DataItemRole.Input, BathymetryDataItemMetaData.Tag);
            AddDataItem(CreateObservationAreasCoverage(initialGrid), ObservationAreasDataItemMetaData.Name,
                        DataItemRole.Input, ObservationAreasDataItemMetaData.Tag);
            AddDataItem(new DataTableManager(), BoundaryDataDataItemMetaData.Name, DataItemRole.Input,
                        BoundaryDataDataItemMetaData.Tag);
            AddDataItem(new DataTableManager(), LoadsDataDataItemMetaData.Name, DataItemRole.Input,
                        LoadsDataDataItemMetaData.Tag);

            AddDataItemSet(new EventedList<IFunction>(), InitialConditionsDataItemMetaData.Name, DataItemRole.Input,
                           InitialConditionsDataItemMetaData.Tag, true);
            AddDataItemSet(new EventedList<IFunction>(), ProcessCoefficientsDataItemMetaData.Name, DataItemRole.Input,
                           ProcessCoefficientsDataItemMetaData.Tag, true);
            AddDataItemSet(new EventedList<IFunction>(CreateDispersionFunctions()), DispersionDataItemMetaData.Name,
                           DataItemRole.Input, DispersionDataItemMetaData.Tag, true);
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
            return new EventedList<IFunction>
            {
                WaterQualityFunctionFactory.CreateConst("Dispersion", 0, "Dispersion", "m2/s",
                                                        "Horizontal Dispersion")
            };
        }

        private static UnstructuredGridVertexCoverage CreateAndFillBathymetryCoverage(UnstructuredGrid grid)
        {
            // create new bathymetry
            var bathymetry = new UnstructuredGridVertexCoverage(grid, false)
            {
                Name = "Bed Level",
                IsEditable = false
            };
            bathymetry.Components[0].NoDataValue = -999.0;
            bathymetry.Components[0].DefaultValue = bathymetry.Components[0].NoDataValue;

            if (grid.Vertices.Count > 0)
            {
                bathymetry.SetValues(grid.Vertices.Select(v => v.Z));
            }

            return bathymetry;
        }

        private static WaterQualityObservationAreaCoverage CreateObservationAreasCoverage(UnstructuredGrid grid)
        {
            return new WaterQualityObservationAreaCoverage(grid);
        }

        /// <summary>
        /// Replace the grid on a list of coverages.
        /// This method does not look at spatial operations.
        /// Use this method for output coverages for example.
        /// </summary>
        /// <param name="functions"> </param>
        /// <param name="onlyUpdateGrid"> </param>
        /// <seealso cref="ReplaceGridOnUnstructuredGridCoveragesWithSpatialOperations"/>
        private void ReplaceGridOnUnstructuredGridCoverages(IEnumerable<IFunction> functions, bool onlyUpdateGrid)
        {
            foreach (IFunction function in functions)
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
            ReplaceGridOnUnstructuredGridCoverages(new[]
            {
                (IFunction) dataItem.Value
            }, onlyUpdateGrid);
            SetGridAndExecuteSpatialOperation(dataItem);
        }

        /// <summary>
        /// Replace grid on unstructured coverages, because the grid was replaced via the <see cref="Grid"/> property.
        /// This method checks spatial operations.
        /// Use this method for functions that require spatial operation input.
        /// </summary>
        /// <seealso cref="ReplaceGridOnUnstructuredGridCoverages"/>
        /// .
        private void ReplaceGridOnUnstructuredGridCoveragesWithSpatialOperations(
            IDataItemSet dataItemSet, bool onlyUpdateGrid)
        {
            var functionListToReplace = new List<IFunction>();
            foreach (IDataItem dataItem in dataItemSet.DataItems)
            {
                if (dataItem.Value != null)
                {
                    functionListToReplace.Add((IFunction) dataItem.Value);
                }
                else if (dataItem.ComposedValue != null)
                {
                    functionListToReplace.Add((IFunction) dataItem.ComposedValue);
                }
            }

            ReplaceGridOnUnstructuredGridCoverages(functionListToReplace, onlyUpdateGrid);

            foreach (IDataItem dataItem in dataItemSet.DataItems)
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

            var original = (UnstructuredGridCellCoverage) valueConverter.OriginalValue;
            original.AssignNewGridToCoverage(Grid);

            // set the grid extents as mask for the first operation, because it depends on the grid.
            ISpatialOperation operation = valueConverter.SpatialOperationSet.Operations.FirstOrDefault(
                o => o.Name == WaterQualityModelSyncExtensions
                         .InitialValueOperationName);

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

        private void HandleNewHydroDynamicsFunctionDataSet(IDataItemSet functionCollection, string functionName)
        {
            IDataItem dataItem =
                functionCollection.DataItems.FirstOrDefault(
                    p => p.Name.ToLowerInvariant() == functionName.ToLowerInvariant());
            if (dataItem == null || SubstanceProcessLibrary == null)
            {
                return;
            }

            IFunction function = GetFunctionForDataItem(dataItem);

            bool hasDataInHydroDynamics = HasDataInHydroDynamics(functionName);
            bool isFromHydroDynamics = function.IsFromHydroDynamics();

            IFunctionTypeCreator creator;
            if (hasDataInHydroDynamics
            ) // if there is data and it is the first time, automatically set it to from hydrodynamics. 
            {
                creator = FunctionTypeCreatorFactory.CreateFunctionFromHydroDynamicsCreator(
                    HasDataInHydroDynamics, GetFilePathFromHydroDynamics);
            }
            else if (isFromHydroDynamics
            ) // if there is no data in the hydrodynamics, but it is set as such, set it back to constant
            {
                creator = FunctionTypeCreatorFactory.CreateConstantCreator();
            }
            else
            {
                return;
            }

            FunctionTypeCreator.ReplaceFunctionUsingCreator(functionCollection.AsEventedList<IFunction>(), function,
                                                            creator, this);
            Log.InfoFormat(
                Resources
                    .WaterQualityModel_HandleNewHydroDynamicsFunctionDataSet_The_process_coefficient__0__has_been_updated_with_the_latest_Hydrodynamic_data_file_,
                function.Name);
        }

        private IFunction GetFunctionForDataItem(IDataItem dataItem)
        {
            IFunction function = dataItem.Value as IFunction ?? dataItem.ValueConverter.OriginalValue as IFunction;
            Debug.Assert(function != null,
                         "Assumption: If DataItem.Value should return null here, we are dealing with " +
                         "an UnstructuredGridCellCoverage which uses ValueConverters and hasn't executed " +
                         "yet such as during save/load cycle.");
            return function;
        }

        private void HydroDataOnDataChanged(object sender, EventArgs<string> eventArgs)
        {
            if (HydroDataChanged == null)
            {
                return;
            }

            HydroDataChanged(this, eventArgs);
        }

        #region Tags

        public static readonly DispersionDataItemMetaData DispersionDataItemMetaData = new DispersionDataItemMetaData();

        public static readonly ProcessCoefficientsDataItemMetaData ProcessCoefficientsDataItemMetaData =
            new ProcessCoefficientsDataItemMetaData();

        public static readonly BloomAlgaeDataItemMetaData BloomAlgaeDataItemMetaData = new BloomAlgaeDataItemMetaData();

        public static readonly InputFileHybridDataItemMetaData InputFileHybridDataItemMetaData =
            new InputFileHybridDataItemMetaData();

        public static readonly BathymetryDataItemMetaData BathymetryDataItemMetaData = new BathymetryDataItemMetaData();

        public static readonly BoundaryDataDataItemMetaData BoundaryDataDataItemMetaData =
            new BoundaryDataDataItemMetaData();

        public static readonly BoundariesDataItemMetaData BoundariesDataItemMetaData = new BoundariesDataItemMetaData();
        public static readonly LoadsDataDataItemMetaData LoadsDataDataItemMetaData = new LoadsDataDataItemMetaData();
        public static readonly GridDataItemMetaData GridDataItemMetaData = new GridDataItemMetaData();

        public static readonly SubstanceProcessLibraryDataItemMetaData SubstanceProcessLibraryDataItemMetaData =
            new SubstanceProcessLibraryDataItemMetaData();

        public static readonly InputFileCommandLineDataItemMetaData InputFileCommandLineDataItemMetaData =
            new InputFileCommandLineDataItemMetaData();

        public static readonly LoadsDataItemMetaData LoadsDataItemMetaData = new LoadsDataItemMetaData();

        public static readonly InitialConditionsDataItemMetaData InitialConditionsDataItemMetaData =
            new InitialConditionsDataItemMetaData();

        public static readonly ObservationPointsDataItemMetaData ObservationPointsDataItemMetaData =
            new ObservationPointsDataItemMetaData();

        public static readonly ObservationAreasDataItemMetaData ObservationAreasDataItemMetaData =
            new ObservationAreasDataItemMetaData();

        public static readonly MonitoringOutputDataItemMetaData MonitoringOutputDataItemMetaData =
            new MonitoringOutputDataItemMetaData();

        public static readonly OutputSubstancesDataItemMetaData OutputSubstancesDataItemMetaData =
            new OutputSubstancesDataItemMetaData();

        public static readonly OutputParametersDataItemMetaData OutputParametersDataItemMetaData =
            new OutputParametersDataItemMetaData();

        public static readonly BalanceOutputDataItemMetaData BalanceOutputDataItemMetaData =
            new BalanceOutputDataItemMetaData();

        public static readonly MonitoringFileDataItemMetaData MonitoringFileDataItemMetaData =
            new MonitoringFileDataItemMetaData();

        public static readonly ListFileDataItemMetaData ListFileDataItemMetaData = new ListFileDataItemMetaData();

        public static readonly ProcessFileDataItemMetaData ProcessFileDataItemMetaData =
            new ProcessFileDataItemMetaData();

        #endregion

        #region Fields

        private double progressPercentage;
        private bool enableMarkOutputOutOfSync;

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

        private string modelDataDirectory;
        private LazyMapFileFunctionStore mapFileFunctionStore;
        private ICoordinateSystem overriddenCoordinateSystem;
        private IHydroData hydroData;
        private WaterQualityModelSettings modelSettings;
        private string verticalDiffusionRelativeFilePath;
        private IEventedList<WaterQualityObservationPoint> observationPoints;
        private IEventedList<WaterQualityLoad> loads;
        private string surfacesRelativeFilePath;
        private string chezyCoefficientsFilePath;
        private string widthsFilePath;
        private string velocitiesFilePath;

        #endregion

        #region Public properties

        private IList<WaqProcessValidationRule> _waqProcessesRules;

        public virtual IList<WaqProcessValidationRule> WaqProcessesRules => _waqProcessesRules;

        private void InitializeWaqProcessesRules()
        {
            //Get the file location
            Assembly assembly = typeof(WaterQualityModel).Assembly;
            string assemblyLocation = assembly.Location;
            DirectoryInfo directoryInfo = new FileInfo(assemblyLocation).Directory;

            if (directoryInfo == null)
            {
                return;
            }

            //Initialize it.
            _waqProcessesRules = new WaqProcessesRules().ReadValidationCsv(directoryInfo.FullName);
        }

        public override string KernelVersions
        {
            get
            {
                string delWaqExePath = WaterQualityApiDataSet.DelWaqExePath;

                if (File.Exists(delWaqExePath))
                {
                    return $"Kernel: {Path.GetFileName(delWaqExePath)}  {FileVersionInfo.GetVersionInfo(delWaqExePath).FileVersion}";
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// The settings of the water quality model
        /// </summary>
        public virtual WaterQualityModelSettings ModelSettings
        {
            get => modelSettings;
            protected set => modelSettings = value;
        }

        /// <summary>
        /// The input file of the water quality model
        /// </summary>
        public virtual TextDocument InputFile => InputFileCommandLine;

        /// <summary>
        /// Mapper for resolving coordinates (x,y,(z)) to cell indices.
        /// </summary>
        public virtual PointToGridCellMapper PointToGridCellMapper => pointToGridCellMapper;

        /// <summary>
        /// The input file of the water quality model in case of command line calculations
        /// </summary>
        public virtual TextDocument InputFileCommandLine
        {
            get
            {
                IDataItem inputFileDataItem = GetDataItemByTag(InputFileCommandLineDataItemMetaData.Tag);

                return inputFileDataItem?.Value as TextDocument;
            }
        }

        /// <summary>
        /// The input file of the water quality model in case of hybrid calculations
        /// </summary>
        public virtual TextDocument InputFileHybrid
        {
            get
            {
                IDataItem inputFileDataItem = GetDataItemByTag(InputFileHybridDataItemMetaData.Tag);

                return inputFileDataItem?.Value as TextDocument;
            }
        }

        /// <summary>
        /// The substance process library of the water quality model
        /// </summary>
        public virtual SubstanceProcessLibrary SubstanceProcessLibrary =>
            (SubstanceProcessLibrary) GetDataItemByTag(SubstanceProcessLibraryDataItemMetaData.Tag).Value;

        /// <summary>
        /// The (dry waste) loads of the water quality model.
        /// </summary>
        public virtual IEventedList<WaterQualityLoad> Loads
        {
            get => loads;
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
            get => observationPoints;
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
        public virtual WaterQualityObservationAreaCoverage ObservationAreas =>
            GetDataItemValueByTag<WaterQualityObservationAreaCoverage>(ObservationAreasDataItemMetaData.Tag);

        /// <summary>
        /// The initial conditions of the water quality model
        /// </summary>
        public virtual IEventedList<IFunction> InitialConditions =>
            GetDataItemSetByTag(InitialConditionsDataItemMetaData.Tag).AsEventedList<IFunction>();

        /// <summary>
        /// The process coefficients of the water quality model
        /// </summary>
        public virtual IEventedList<IFunction> ProcessCoefficients =>
            GetDataItemSetByTag(ProcessCoefficientsDataItemMetaData.Tag).AsEventedList<IFunction>();

        /// <summary>
        /// The dispersion definitions of the water quality model
        /// </summary>
        public virtual IEventedList<IFunction> Dispersion =>
            GetDataItemSetByTag(DispersionDataItemMetaData.Tag).AsEventedList<IFunction>();

        /// <summary>
        /// Gets or sets dispersion in the horizontal axis in m^2/s.
        /// </summary>
        public virtual double HorizontalDispersion
        {
            get => (double) Dispersion[0].Components[0].DefaultValue;
            set => SetHorizontalDispersion(value);
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
        public virtual IDataItemSet MonitoringOutputDataItemSet =>
            GetDataItemSetByTag(MonitoringOutputDataItemMetaData.Tag);

        /// <summary>
        /// The monitoring output of the water quality model
        /// </summary>
        public virtual IList<WaterQualityObservationVariableOutput> ObservationVariableOutputs
        {
            get
            {
                IDataItemSet monitoringOutputDataItemSet = MonitoringOutputDataItemSet;

                return monitoringOutputDataItemSet != null
                           ? monitoringOutputDataItemSet.AsEventedList<WaterQualityObservationVariableOutput>()
                           : new EventedList<WaterQualityObservationVariableOutput>();
            }
        }

        /// <summary>
        /// The output substances data item set of the water quality model
        /// </summary>
        public virtual IDataItemSet OutputSubstancesDataItemSet =>
            GetDataItemSetByTag(OutputSubstancesDataItemMetaData.Tag);

        /// <summary>
        /// The output parameters data item set of the water quality model
        /// </summary>
        public virtual IDataItemSet OutputParametersDataItemSet =>
            GetDataItemSetByTag(OutputParametersDataItemMetaData.Tag);

        /// <summary>
        /// The calculation grid of the water quality model
        /// </summary>
        public virtual UnstructuredGrid Grid => (UnstructuredGrid) GetDataItemByTag(GridDataItemMetaData.Tag).Value;

        /// <summary>
        /// The bathymetry for the water quality model
        /// </summary>
        public virtual UnstructuredGridVertexCoverage Bathymetry
        {
            get => (UnstructuredGridVertexCoverage) GetDataItemByTag(BathymetryDataItemMetaData.Tag).Value;
            protected set => GetDataItemByTag(BathymetryDataItemMetaData.Tag).Value = value;
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
            get => hydroData;
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
            get => hasHydroDataImported;
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
            get => layerType;
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
        /// Gets or sets the velocities file path.
        /// <see cref="IHydroData.GetVelocitiesFilePath"/>
        /// </summary>
        /// <value>
        /// The velocities file path.
        /// </value>
        public virtual string VelocitiesFilePath
        {
            get => velocitiesFilePath;
            protected set
            {
                velocitiesFilePath = value;
                HandleNewHydroDynamicsFunctionDataSet(GetDataItemSetByTag(ProcessCoefficientsDataItemMetaData.Tag),
                                                      "Velocity");
            }
        }

        /// <summary>
        /// Gets or sets the widths file path.
        /// <see cref="IHydroData.GetWidthsFilePath"/>
        /// </summary>
        /// <value>
        /// The widths file path.
        /// </value>
        public virtual string WidthsFilePath
        {
            get => widthsFilePath;
            protected set
            {
                widthsFilePath = value;
                HandleNewHydroDynamicsFunctionDataSet(GetDataItemSetByTag(ProcessCoefficientsDataItemMetaData.Tag),
                                                      "Width");
            }
        }

        /// <summary>
        /// Gets or sets the chezy coefficients file path.
        /// <see cref="IHydroData.GetChezyCoefficientsFilePath"/>
        /// </summary>
        /// <value>
        /// The chezy coefficients file path.
        /// </value>
        public virtual string ChezyCoefficientsFilePath
        {
            get => chezyCoefficientsFilePath;
            protected set
            {
                chezyCoefficientsFilePath = value;
                HandleNewHydroDynamicsFunctionDataSet(GetDataItemSetByTag(ProcessCoefficientsDataItemMetaData.Tag),
                                                      "Chezy");
            }
        }

        /// <summary>
        /// The vertical diffusion file can be found in the *.hyd-file and
        /// is passed in the input file.
        /// *.vdf
        /// <see cref="IHydroData.VerticalDiffusionRelativePath"/>
        /// </summary>
        public virtual string VerticalDiffusionRelativeFilePath
        {
            get => verticalDiffusionRelativeFilePath;
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
        public virtual string SurfacesRelativeFilePath
        {
            get => surfacesRelativeFilePath;
            protected set
            {
                surfacesRelativeFilePath = value;
                HandleNewHydroDynamicsFunctionDataSet(GetDataItemSetByTag(ProcessCoefficientsDataItemMetaData.Tag),
                                                      "Surf");
            }
        }

        /// <summary>
        /// The salinity file can be found in the *.hyd-file and
        /// is passed in the input file.
        /// *.sal
        /// <see cref="IHydroData.GetSalinityRelativeFilePath"/>
        /// </summary>
        public virtual string SalinityRelativeFilePath
        {
            get => salinityRelativeFilePath;
            protected set
            {
                salinityRelativeFilePath = value;

                HandleNewHydroDynamicsFunctionDataSet(GetDataItemSetByTag(ProcessCoefficientsDataItemMetaData.Tag),
                                                      "Salinity");
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
            get => temperatureRelativeFilePath;
            protected set
            {
                temperatureRelativeFilePath = value;

                HandleNewHydroDynamicsFunctionDataSet(GetDataItemSetByTag(ProcessCoefficientsDataItemMetaData.Tag),
                                                      "Temp");
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
            get => shearStressesRelativeFilePath;
            protected set
            {
                shearStressesRelativeFilePath = value;

                HandleNewHydroDynamicsFunctionDataSet(GetDataItemSetByTag(ProcessCoefficientsDataItemMetaData.Tag),
                                                      "Tau");
            }
        }

        /// <summary>
        /// Gets or sets the relative path from the .hyd file to the grid file path.
        /// </summary>
        /// <value>
        /// The relative grid file path.
        /// </value>
        public virtual string GridRelativeFilePath { get; protected set; }

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

                return GetProgressTextCore(progressPercentage / 100.0);
            }
        }

        public virtual bool UseSaveStateTimeRange { get; set; }

        public virtual DateTime SaveStateStartTime { get; set; }

        public virtual DateTime SaveStateStopTime { get; set; }

        public virtual TimeSpan SaveStateTimeStep { get; set; }

        /// <summary>
        /// Persistent model data folder within Project folder
        /// </summary>
        public virtual string ModelDataDirectory
        {
            get => modelDataDirectory;
            set => modelDataDirectory = value;
        }

        public virtual DataTableManager BoundaryDataManager =>
            (DataTableManager) GetDataItemByTag(BoundaryDataDataItemMetaData.Tag).Value;

        public virtual DataTableManager LoadsDataManager =>
            (DataTableManager) GetDataItemByTag(LoadsDataDataItemMetaData.Tag).Value;

        private IFileBasedFolder outputFolder;

        /// <summary>
        /// Gets or sets the output folder.
        /// </summary>
        /// <value>
        /// The output folder.
        /// </value>
        public virtual IFileBasedFolder OutputFolder
        {
            get => outputFolder;
            set
            {
                if (outputFolder == value)
                {
                    return;
                }

                if (value != null && outputFolder != null && value.Path == outputFolder.Path)
                {
                    OnOutputFolderChanged();
                    return;
                }

                if (outputFolder != null)
                {
                    outputFolder.PropertyChanged -= OnOutputFolderPropertyChanged;
                }

                outputFolder = value;

                if (outputFolder != null)
                {
                    outputFolder.PropertyChanged += OnOutputFolderPropertyChanged;
                }

                OnOutputFolderChanged();
            }
        }

        private void OnOutputFolderChanged()
        {
            WaterQualityOutputDisconnector.Disconnect(this);
            WaterQualityOutputConnector.Connect(this);
        }

        private void OnOutputFolderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(OutputFolder.Path)))
            {
                WaterQualityOutputConnector.Connect(this);
            }
        }

        /// <summary>
        /// The coordinate system can be found in the grid,
        /// but may be overridden by the user.
        /// </summary>
        public virtual ICoordinateSystem CoordinateSystem
        {
            get => Grid != null ? Grid.CoordinateSystem : null;
            set
            {
                overriddenCoordinateSystem = value;

                if (Grid == null)
                {
                    return;
                }

                string existingGridCoordinateSystemString = GetProj4CoordinateSystemString(Grid.CoordinateSystem);
                string newCoordinateSystemString = GetProj4CoordinateSystemString(value);
                if (existingGridCoordinateSystemString == newCoordinateSystemString)
                {
                    return;
                }

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

                LazyMapFileFunctionStore storeFromCoverages =
                    this.GetOutputCoverages().Select(c => c.Store).OfType<LazyMapFileFunctionStore>().FirstOrDefault();
                mapFileFunctionStore = storeFromCoverages ?? new LazyMapFileFunctionStore();

                return mapFileFunctionStore;
            }
        }

        # endregion

        # region Model

        protected override void OnInitialize()
        {
            Log.Info(KernelVersions);
            InvokeAndRestoreDirectory(OnInitializeCore);
        }

        private void OnInitializeCore()
        {
            ValidationReport validationReport = new WaterQualityModelValidator().Validate(this);
            if (validationReport.ErrorCount > 0)
            {
                throw new FormatException(
                    "Water quality model could not initialize. Please check the validation report.");
            }

            FileUtils.CreateDirectoryIfNotExists(ModelSettings.WorkDirectory);

            waqInitializationSettings = WaqInitializationSettingsBuilder.BuildWaqInitializationSettings(this);

            WaterQualityOutputDisconnector.Disconnect(this);

            waqProcessor = new WaqFileBasedProcessor();
            waqPreProcessor = new WaqFileBasedPreProcessor();
            waqPreProcessor.InitializeWaq(waqInitializationSettings);
        }

        protected override void OnExecute()
        {
            InvokeAndRestoreDirectory(OnExecuteCore);
        }

        protected override void OnCancel()
        {
            if (waqProcessor != null)
            {
                waqProcessor.TryToCancel = true;
            }
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
            string outputDirectory = ModelSettings.WorkingOutputDirectory;
            if (outputDirectory == null)
            {
                Log.Error("Could not add output because work directory is empty.");
                return;
            }

            ConnectOutput(outputDirectory);
        }

        private void ConnectOutput(string outputDirectory)
        {
            if (OutputFolder == null)
            {
                OutputFolder = new FileBasedFolder(outputDirectory);
                return;
            }

            var outputDirectoryInfo = new DirectoryInfo(outputDirectory);
            if (OutputFolder.FullPath == outputDirectoryInfo.FullName)
            {
                WaterQualityOutputConnector.Connect(this);
                return;
            }

            OutputFolder.Path = outputDirectory;
        }

        protected override void OnCleanup()
        {
            ClearPreProcessorAndProcessor();
            waqInitializationSettings = null;
            progressPercentage = 0.0;
        }

        protected override void OnClearOutput()
        {
            if (OutputFolder == null || OutputIsEmpty)
            {
                return;
            }

            WaterQualityOutputDisconnector.Disconnect(this);
            OutputFolder.Path = null;
        }

        # endregion
    }
}