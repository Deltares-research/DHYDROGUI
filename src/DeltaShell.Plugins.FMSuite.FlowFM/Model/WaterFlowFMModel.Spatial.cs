using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        private UnstructuredGrid grid;
        private double bathymetryNoDataValue;

        public UnstructuredGridCoverage Bathymetry
        {
            get => (UnstructuredGridCoverage) BathymetryDataItem?.Value;
            private set
            {
                SetDataItem(WaterFlowFMModelDefinition.BathymetryDataItemName, value);
                BathymetryDataItem.ValueType = typeof(UnstructuredGridCoverage);
            }
        }

        public UnstructuredGridCellCoverage InitialWaterLevel
        {
            get => (UnstructuredGridCellCoverage) InitialWaterLevelDataItem?.Value;
            private set => SetDataItem(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, value);
        }

        public UnstructuredGridCellCoverage InitialSalinity
        {
            get => (UnstructuredGridCellCoverage) InitialSalinityDataItem?.Value;
            private set => SetDataItem(WaterFlowFMModelDefinition.InitialSalinityDataItemName, value);
        }

        public UnstructuredGridCellCoverage InitialTemperature
        {
            get => (UnstructuredGridCellCoverage) InitialTemperatureDataItem?.Value;
            private set => SetDataItem(WaterFlowFMModelDefinition.InitialTemperatureDataItemName, value);
        }

        public UnstructuredGridFlowLinkCoverage Roughness
        {
            get => (UnstructuredGridFlowLinkCoverage) RoughnessDataItem?.Value;
            private set => SetDataItem(WaterFlowFMModelDefinition.RoughnessDataItemName, value);
        }

        public UnstructuredGridFlowLinkCoverage Viscosity
        {
            get => (UnstructuredGridFlowLinkCoverage) ViscosityDataItem?.Value;
            private set => SetDataItem(WaterFlowFMModelDefinition.ViscosityDataItemName, value);
        }

        public UnstructuredGridFlowLinkCoverage Diffusivity
        {
            get => (UnstructuredGridFlowLinkCoverage) DiffusivityDataItem?.Value;
            private set => SetDataItem(WaterFlowFMModelDefinition.DiffusivityDataItemName, value);
        }

        public IEnumerable<UnstructuredGridCellCoverage> InitialTracers => TracerDataItems.Select(d => d.Value).Cast<UnstructuredGridCellCoverage>();

        public IEventedList<UnstructuredGridCellCoverage> InitialFractions { get; private set; }

        public Envelope GridExtent { get; private set; }

        public UnstructuredGrid Grid
        {
            get => grid;
            set
            {
                if (grid == value)
                {
                    return;
                }

                bool verticesEqual;
                bool cellsEqual;
                bool linksEqual;

                bool gridsAreEqual =
                    UnstructuredGridHelper.CompareGrids(grid, value, out verticesEqual, out cellsEqual, out linksEqual);

                grid = value;

                if (grid != null)
                {
                    grid.CoordinateSystem = CoordinateSystem;

                    // add flowlinks to input grid for adding data on input FlowLink coverages (Roughness, Viscosity etc.)
                    grid.FlowLinks.AddRange(GenerateFlowLinksForEdges(grid));
                }

                if (!gridsAreEqual)
                {
                    InvalidateSnapping();
                    if (!verticesEqual)
                    {
                        RefreshGridExtents();
                    }

                    UpdateSpatialDataAfterGridSet(grid, !verticesEqual, !cellsEqual, !linksEqual);
                }
                else
                {
                    UpdateSpatialDataAfterGridSet(grid, false, false, false);
                }
            }
        }

        public void RefreshGridExtents()
        {
            GridExtent = grid == null ? null : grid.GetExtents();
        }

        public void ReloadGrid(bool writeNetFile = true, bool loadBathymetry = false)
        {
            try
            {
                BeginEdit(new DefaultEditAction("Replacing unstructured grid"));
                if (writeNetFile)
                {
                    WriteNetFile(NetFilePath, Grid);
                }

                UnstructuredGrid newGrid = ReadGridFromNetFile(NetFilePath); //may throw...
                if (newGrid == null)
                {
                    Grid = new UnstructuredGrid();
                }
                else
                {
                    if (loadBathymetry)
                    {
                        UnstructuredGridCoverage originalBathymetry = GetOriginalCoverage(Bathymetry);
                        originalBathymetry.Arguments[0].Clear();
                        originalBathymetry.Components[0]
                                          .Clear(); //HACK: signals the interpolation method to use the grid node z-values...
                        double ndv;
                        if (!double.TryParse(originalBathymetry.Components[0].NoDataValue.ToString(), out ndv))
                        {
                            bathymetryNoDataValue = -999.0d;
                        }
                        else
                        {
                            bathymetryNoDataValue = ndv;
                        }
                    }

                    UnstructuredGridFileHelper.DoIfUgrid(NetFilePath, uGridAdaptor =>
                    {
                        if (1 > uGridAdaptor.uGrid.GetNumberOf2DMeshes())
                        {
                            bathymetryNoDataValue = -999.0d;
                            return;
                        }

                        uGridAdaptor.uGrid.GetAllNodeCoordinatesForMeshId(1);

                        bathymetryNoDataValue = uGridAdaptor.uGrid.ZCoordinateFillValue;
                    });
                    Grid = newGrid;
                }
            }
            finally
            {
                EndEdit();
            }
        }

        public void RemoveGrid()
        {
            BeginEdit(new DefaultEditAction("Removing grid..."));
            try
            {
                Grid = new UnstructuredGrid();
                if (NetFilePath != null)
                {
                    WriteNetFile(NetFilePath, Grid);
                }
            }
            finally
            {
                EndEdit();
            }
        }

        public void WriteNetFile(string path)
        {
            WriteNetFile(path, Grid);
        }

        protected virtual IGridOperationApi gridOperationApi { get; set; }
        protected virtual bool snapApiInErrorMode { get; set; }

        internal void UpdateBathymetryCoverage(UnstructuredGridFileHelper.BedLevelLocation bedLevelType)
        {
            if (Bathymetry == null)
            {
                return;
            }

            double[] zValues = null;
            switch (bedLevelType)
            {
                case UnstructuredGridFileHelper.BedLevelLocation.Faces:
                case UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes:
                    if (Bathymetry is UnstructuredGridCellCoverage)
                    {
                        return;
                    }

                    zValues = GetZValuesFromNetFile(bedLevelType);
                    Bathymetry =
                        CreateUnstructuredGridCellCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName, Grid,
                                                           zValues);
                    break;
                case UnstructuredGridFileHelper.BedLevelLocation.CellEdges:
                    Log.WarnFormat(
                        Resources
                            .WaterFlowFMModel_UpdateBathymetryCoverage_Unstructured_grid_edge_coverages_are_not_currently_supported);
                    // Not supported yet, so create a VertexCoverage for now
                    if (Bathymetry is UnstructuredGridVertexCoverage)
                    {
                        return;
                    }

                    zValues = GetZValuesFromNetFile(bedLevelType);
                    Bathymetry =
                        CreateUnstructuredGridVertexCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName, Grid,
                                                             zValues);
                    break;
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev:
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev:
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev:
                    if (Bathymetry is UnstructuredGridVertexCoverage)
                    {
                        return;
                    }

                    zValues = GetZValuesFromNetFile(bedLevelType);
                    Bathymetry =
                        CreateUnstructuredGridVertexCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName, Grid,
                                                             zValues);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bedLevelType), bedLevelType, null);
            }

            // sync ModelDefinition with new Bathymetry
            ModelDefinition.Bathymetry = Bathymetry;

            // update BedLevel DataItem
            if (BathymetryDataItem == null)
            {
                return;
            }

            Log.InfoFormat(
                Resources
                    .WaterFlowFMModel_UpdateBathymetryCoverage_The_BedLevel_location_specified_does_not_match_the_existing_BedLevel_data__a_new_BedLevel_Data_will_be_generated_);
            BathymetryDataItem.Value = Bathymetry;

            // re-apply spatial operations
            var spatialOperationsValueConverter = BathymetryDataItem.ValueConverter as SpatialOperationSetValueConverter;
            if (spatialOperationsValueConverter?.SpatialOperationSet != null &&
                spatialOperationsValueConverter.SpatialOperationSet.Operations.Any())
            {
                spatialOperationsValueConverter.SpatialOperationSet.Operations.ForEach(o => o.Execute());
                Log.InfoFormat(
                    Resources
                        .WaterFlowFMModel_UpdateBathymetryCoverage_Reapplying_existing_spatial_operations_to_new_BedLevel_Data);
            }
        }

        private IDataItem BathymetryDataItem => DataItems.GetByName(WaterFlowFMModelDefinition.BathymetryDataItemName);
        private IDataItem InitialWaterLevelDataItem => DataItems.GetByName(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName);
        private IDataItem InitialSalinityDataItem => DataItems.GetByName(WaterFlowFMModelDefinition.InitialSalinityDataItemName);
        private IDataItem InitialTemperatureDataItem => DataItems.GetByName(WaterFlowFMModelDefinition.InitialTemperatureDataItemName);
        private IDataItem RoughnessDataItem => DataItems.GetByName(WaterFlowFMModelDefinition.RoughnessDataItemName);
        private IDataItem ViscosityDataItem => DataItems.GetByName(WaterFlowFMModelDefinition.ViscosityDataItemName);
        private IDataItem DiffusivityDataItem => DataItems.GetByName(WaterFlowFMModelDefinition.DiffusivityDataItemName);
        private IEnumerable<IDataItem> TracerDataItems => TracerDefinitions.Select(t => DataItems.GetByName(t)).Where(d => d != null);

        private void SetDataItem(string name, object value)
        {
            IDataItem dataItem = DataItems.GetByName(name);
            if (dataItem == null)
            {
                AddInputDataItem(value, name);
            }
            else
            {
                dataItem.ValueConverter = null;
                dataItem.Value = value;
            }
        }

        private void AddInputDataItem(object value, string name)
        {
            DataItems.Add(new DataItem(value, name) {Role = DataItemRole.Input});
        }

        private static IList<FlowLink> GenerateFlowLinksForEdges(UnstructuredGrid grid)
        {
            // optimized for performance
            var flowLinks = new List<FlowLink>();

            for (var index = 0; index < grid.Edges.Count; index++)
            {
                Edge gridEdge = grid.Edges[index];

                IList<int> gridVertexToCellIndex;
                grid.VertexToCellIndices.TryGetValue(gridEdge.VertexFromIndex, out gridVertexToCellIndex);
                if (gridVertexToCellIndex == null)
                {
                    continue;
                }

                IList<int> vertexToCellIndex;
                grid.VertexToCellIndices.TryGetValue(gridEdge.VertexToIndex, out vertexToCellIndex);
                if (vertexToCellIndex == null)
                {
                    continue;
                }

                int cellOne = -1;
                int cellTwo = -1;
                var moreThanTwo = false;
                for (var i = 0; i < gridVertexToCellIndex.Count; i++)
                {
                    int value1 = gridVertexToCellIndex[i];

                    for (var j = 0; j < vertexToCellIndex.Count; j++)
                    {
                        int value2 = vertexToCellIndex[j];

                        if (value1 != value2)
                        {
                            continue;
                        }

                        if (cellOne == -1)
                        {
                            cellOne = value1;
                            continue;
                        }

                        if (cellTwo == -1)
                        {
                            cellTwo = value2;
                            continue;
                        }

                        moreThanTwo = true;
                    }
                }

                if (!moreThanTwo && cellOne != -1 && cellTwo != -1)
                {
                    flowLinks.Add(new FlowLink(cellOne, cellTwo, gridEdge));
                }
            }

            return flowLinks;
        }

        private UnstructuredGridCoverage GetOriginalCoverage(UnstructuredGridCoverage coverage)
        {
            IDataItem dataItem = GetDataItemByValue(coverage);
            if (dataItem == null)
            {
                return coverage;
            }

            var valueConverter = dataItem.ValueConverter as CoverageSpatialOperationValueConverter;
            if (valueConverter == null)
            {
                return coverage;
            }

            return valueConverter.OriginalValue as UnstructuredGridCoverage;
        }

        private static void WriteNetFile(string path, UnstructuredGrid grid)
        {
            if (path == null)
            {
                return;
            }

            UnstructuredGridFileHelper.WriteGridToFile(path, grid);
        }

        private void InitializeUnstructuredGridCoverages()
        {
            LoadBathymetry();

            InitialWaterLevel =
                CreateUnstructuredGridCellCoverage(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, Grid);
            InitialSalinity = CreateUnstructuredGridCellCoverage(WaterFlowFMModelDefinition.InitialSalinityDataItemName, Grid);
            InitialTemperature =
                CreateUnstructuredGridCellCoverage(WaterFlowFMModelDefinition.InitialTemperatureDataItemName, Grid);
            Viscosity = CreateUnstructuredGridFlowLinkCoverage(WaterFlowFMModelDefinition.ViscosityDataItemName, Grid);
            Diffusivity =
                CreateUnstructuredGridFlowLinkCoverage(WaterFlowFMModelDefinition.DiffusivityDataItemName, Grid);
            Roughness = CreateUnstructuredGridFlowLinkCoverage(WaterFlowFMModelDefinition.RoughnessDataItemName, Grid);
            InitialFractions = new EventedList<UnstructuredGridCellCoverage>();
            InitialFractions.CollectionChanged += SpatialDataFractionsChanged;
        }

        private void LoadBathymetry()
        {
            var bedLevelLocationsForUnstructuredGridCellCoverage = new List<UnstructuredGridFileHelper.BedLevelLocation>
            {
                UnstructuredGridFileHelper.BedLevelLocation.Faces,
                UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes
            };

            // Update bathymetry coverage based on specified value in .mdu file
            WaterFlowFMProperty bedLevelTypeProperty = ModelDefinition.Properties.FirstOrDefault(p =>
                                                                                                     p.PropertyDefinition
                                                                                                      .MduPropertyName
                                                                                                      .ToLower() ==
                                                                                                     KnownProperties
                                                                                                         .BedlevType);
            if (bedLevelTypeProperty != null)
            {
                var bedLevelType = (UnstructuredGridFileHelper.BedLevelLocation) bedLevelTypeProperty.Value;
                double[] zValues = GetZValuesFromNetFile(bedLevelType);
                if (bedLevelLocationsForUnstructuredGridCellCoverage.Contains(bedLevelType))
                {
                    Bathymetry = CreateUnstructuredGridCellCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName,
                                                                    Grid, zValues);
                }
                else
                {
                    if (zValues == null)
                    {
                        zValues = grid.Vertices.Count > 0 ? grid.Vertices.Select(v => v.Z).ToArray() : null;
                    }

                    Bathymetry = CreateUnstructuredGridVertexCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName,
                                                                      Grid, zValues);
                }

                // sync ModelDefinition with new Bathymetry
                ModelDefinition.Bathymetry = Bathymetry;
            }
        }

        private double[] GetZValuesFromNetFile(UnstructuredGridFileHelper.BedLevelLocation bedLevelType)
        {
            if (string.IsNullOrEmpty(NetFilePath) || !File.Exists(NetFilePath))
            {
                return null;
            }

            double[] zValuesFromNetFile = UnstructuredGridFileHelper.ReadZValues(NetFilePath, bedLevelType);
            return zValuesFromNetFile.Length > 0 ? zValuesFromNetFile : null;
        }

        private static UnstructuredGridVertexCoverage CreateUnstructuredGridVertexCoverage(
            string name, UnstructuredGrid grid, IEnumerable<double> componentValues = null)
        {
            return CreateUnstructuredGridCoverage(name, grid,
                                                  () => new UnstructuredGridVertexCoverage(
                                                      new UnstructuredGrid(), false),
                                                  () => Enumerable.Range(0, grid.Vertices.Count),
                                                  componentValues);
        }

        private static UnstructuredGridCellCoverage CreateUnstructuredGridCellCoverage(
            string name, UnstructuredGrid grid, IEnumerable<double> componentValues = null)
        {
            return CreateUnstructuredGridCoverage(name, grid,
                                                  () => new UnstructuredGridCellCoverage(new UnstructuredGrid(), false),
                                                  () => Enumerable.Range(0, grid.Cells.Count),
                                                  componentValues);
        }

        private static UnstructuredGridFlowLinkCoverage CreateUnstructuredGridFlowLinkCoverage(
            string name, UnstructuredGrid grid, IEnumerable<double> componentValues = null)
        {
            return CreateUnstructuredGridCoverage(name, grid,
                                                  () => new UnstructuredGridFlowLinkCoverage(
                                                      new UnstructuredGrid(), false),
                                                  () => Enumerable.Range(0, grid.FlowLinks.Count),
                                                  componentValues);
        }

        private static T CreateUnstructuredGridCoverage<T>(string name, UnstructuredGrid grid, Func<T> createCoverage,
                                                           Func<IEnumerable<int>> argumentValues,
                                                           IEnumerable<double> componentValues = null)
            where T : UnstructuredGridCoverage
        {
            T result = createCoverage();

            result.Name = name;
            result.Grid = grid;

            result.Components[0].NoDataValue = -999d;
            result.Components[0].DefaultValue = -999d;

            FunctionHelper.SetValuesRaw(result.Arguments[0], argumentValues());

            IEnumerable<double> values = componentValues ?? Enumerable.Repeat(-999d, result.Arguments[0].Values.Count);
            FunctionHelper.SetValuesRaw(result.Components[0], values);

            return result;
        }

        private static UnstructuredGrid ReadGridFromNetFile(string netFilePath)
        {
            return UnstructuredGridFileHelper.LoadFromFile(netFilePath, callCreateCells: true);
        }

        // Can be further optimized by letting InsertGrid accept lists of coverages
        private void UpdateSpatialDataAfterGridSet(UnstructuredGrid newGrid, bool nodesChanged, bool cellsChanged,
                                                   bool linksChanged)
        {
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, BathymetryDataItem);
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, InitialWaterLevelDataItem);
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, RoughnessDataItem);
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, ViscosityDataItem);
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, DiffusivityDataItem);
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, InitialTemperatureDataItem);
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, InitialSalinityDataItem);

            foreach (var tracerDataItem in TracerDataItems)
            {
                UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, tracerDataItem);
            }

            if (InitialFractions != null)
            {
                IEventedList<UnstructuredGridCellCoverage> initialFracts = InitialFractions;
                try
                {
                    foreach (IDataItem fraction in initialFracts.Select(f=>dataItems.GetByName(f.Name)))
                    {
                        UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, fraction);
                    }
                }
                finally
                {
                    InitialFractions = initialFracts;
                }
            }
        }

        private void UpdateCoverageGrid(UnstructuredGrid newGrid,
                                        bool nodesChanged, bool cellsChanged, bool linksChanged, IDataItem dataItem)
        {
            var coverage = (UnstructuredGridCoverage) dataItem?.Value;
            if (coverage == null)
            {
                return;
            }

            var valueConverter = dataItem.ValueConverter as SpatialOperationSetValueConverter;
            if (valueConverter?.SpatialOperationSet.Operations.Any() == true)
            {
                dataItem.ValueConverter = null;
                dataItem.Value = null;
                try
                {
                    var originalCoverage = valueConverter.OriginalValue as UnstructuredGridCoverage;
                    if (originalCoverage != null)
                    {
                        UpdateCoverageAfterGridSet(originalCoverage, newGrid, nodesChanged, cellsChanged,
                                                   linksChanged, true);
                    }

                    foreach (UnstructuredGridCoverage cov in
                        valueConverter.SpatialOperationSet.GetAllFeatureProviders()
                                      .OfType<CoverageFeatureProvider>()
                                      .Select(fp => fp.Coverage)
                                      .OfType<UnstructuredGridCoverage>().Except(new[]
                                      {
                                          originalCoverage
                                      }))
                    {
                        UpdateCoverageAfterGridSet(cov, newGrid, nodesChanged, cellsChanged, linksChanged, false);
                    }

                    UpdateCoverageAfterGridSet(valueConverter.ConvertedValue as UnstructuredGridCoverage, newGrid,
                                               nodesChanged, cellsChanged, linksChanged, false);
                }
                finally
                {
                    dataItem.ValueConverter = valueConverter;
                    valueConverter.SpatialOperationSet.SetDirty();
                }
            }
            else
            {
                dataItem.Value = null;

                try
                {
                    UpdateCoverageAfterGridSet(coverage, newGrid, nodesChanged, cellsChanged, linksChanged, true);
                }
                finally
                {
                    dataItem.Value = coverage;
                }
            }
        }

        private static void ClearVariable(IVariable variable)
        {
            IMultiDimensionalArray targetMda = variable.Values;

            bool wasFiring = targetMda.FireEvents;
            bool wasAutoSorted = targetMda.IsAutoSorted;
            try
            {
                targetMda.FireEvents = false;
                targetMda.IsAutoSorted = false;
                targetMda.Clear();
            }
            finally
            {
                targetMda.FireEvents = wasFiring;
                targetMda.IsAutoSorted = wasAutoSorted;
            }
        }

        private void UpdateCoverageAfterGridSet(UnstructuredGridCoverage coverage, UnstructuredGrid newGrid,
                                                bool nodesChanged, bool cellsChanged, bool linksChanged,
                                                bool reInterpolate)
        {
            if (disposed)
            {
                return;
            }

            if (newGrid == null)
            {
                coverage.BeginEdit(new DefaultEditAction("Clearing grid from coverage"));

                ClearVariable(coverage.Components[0]);
                ClearVariable(coverage.Arguments[0]);

                coverage.Grid = null;
                coverage.EndEdit();
                return;
            }

            var vertexCoverage = coverage as UnstructuredGridVertexCoverage;
            // TODO: this method does not take bathymetry as an UnstructuredGridCellCoverage into account! (DELFT3DFM-1355)
            if (vertexCoverage != null && vertexCoverage.Name == WaterFlowFMModelDefinition.BathymetryDataItemName && !vertexCoverage.GetValues<double>().Any())
            {
                vertexCoverage.LoadBathymetry(newGrid, bathymetryNoDataValue);
                return;
            }

            if (coverage is UnstructuredGridVertexCoverage && nodesChanged ||
                coverage is UnstructuredGridCellCoverage && cellsChanged ||
                coverage is UnstructuredGridFlowLinkCoverage && linksChanged)
            {
                coverage.LoadGrid(newGrid, reInterpolate);
            }
            else
            {
                coverage.Grid = newGrid;
            }
        }

        #region IGridSnapApi

        public bool SnapsToGrid(IGeometry geometry)
        {
            throw new NotImplementedException();
        }

        // Creation should only occur on the UI thread!!!
        private IGridOperationApi GetGridSnapApi(bool fullExport = true)
        {
            if (snapApiInErrorMode) // very crude..
            {
                Log.WarnFormat(
                    Resources
                        .WaterFlowFMModel_GetGridSnapApi_Last_attempt_to_perform_snapping_operation_failed__please_verify_the_model_first_);
            }

            try
            {
                IGridOperationApi api = gridOperationApi ??
                                        (gridOperationApi = new UnstrucGridOperationApi(this, fullExport));
                snapApiInErrorMode = false;
                return api;
            }
            catch (Exception e)
            {
                Log.ErrorFormat(Resources.WaterFlowFMModel_GetGridSnapApi_Kernel_failed_to_perform_operation___0_,
                                e.Message);
                snapApiInErrorMode = true;
                throw;
            }
        }

        internal int SnapVersion { get; private set; }

        public void InvalidateSnapping()
        {
            DisposeSnapApi();
            // increment the 'snap version' to invalidate previously snapped features
            SnapVersion++;
        }

        [InvokeRequired]
        private void DisposeSnapApi()
        {
            // reset the grid snap api
            var disposable = gridOperationApi as IDisposable;
            if (disposable != null)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat(Resources.WaterFlowFMModel_DisposeSnapApi_Error_disposing_grid_operation_api___0_,
                                    ex.Message);
                }
            }

            gridOperationApi = null;
            snapApiInErrorMode = false;
        }

        public IGeometry GetGridSnappedGeometry(string featureType, IGeometry geometry)
        {
            return GetGridSnapApi(false).GetGridSnappedGeometry(featureType, geometry);
        }

        public IEnumerable<IGeometry> GetGridSnappedGeometry(string featureType, ICollection<IGeometry> geometries)
        {
            return GetGridSnapApi(false).GetGridSnappedGeometry(featureType, geometries);
        }

        public int[] GetLinkedCells()
        {
            return GetGridSnapApi().GetLinkedCells();
        }

        #endregion
    }
}