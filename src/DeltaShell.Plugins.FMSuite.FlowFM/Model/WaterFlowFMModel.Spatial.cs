using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid;
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

        public Envelope GridExtent { get; private set; }

        /// <summary>
        /// Gets the collection of spatial data items.
        /// This collection is equal to <seealso cref="SpatialData.DataItems"/>
        /// and should not be changed or accessed from outside.
        /// Moreover, changing this collection will not lead to changes in the domain.
        /// Instead, use the public methods of <see cref="SpatialData"/>.
        /// This construct is needed to propagate events.
        /// </summary>
        public IEventedList<IDataItem> SpatialDataItems { get; private set; }

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
                    grid.GenerateFlowLinks();
                }

                if (!gridsAreEqual)
                {
                    InvalidateSnapping();
                    if (!verticesEqual)
                    {
                        RefreshGridExtents();
                    }

                    UpdateSpatialDataAfterGridSet(grid, !verticesEqual, !cellsEqual, !linksEqual);

                    MarkOutputOutOfSync();
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

        private static double GetBathymetryNoDataValue(UnstructuredGridCoverage bathymetry) =>
            double.TryParse(bathymetry.Components[0].NoDataValue.ToString(), out double noDataValue) 
                ? noDataValue 
                : -999D;

        public void ReloadGrid(bool writeNetFile = true, bool loadBathymetry = false)
        {
            try
            {
                BeginEdit("Replacing unstructured grid");
                if (writeNetFile)
                {
                    WriteNetFile(NetFilePath, Grid);
                }

                var fileOperations = new UnstructuredGridFileOperations(NetFilePath); //may throw...
                UnstructuredGrid newGrid = fileOperations.GetGrid(callCreateCells: true); 
                
                if (newGrid == null)
                {
                    Grid = new UnstructuredGrid();
                }
                else
                {
                    if (loadBathymetry)
                    {
                        UnstructuredGridCoverage originalBathymetry = GetOriginalCoverage(SpatialData.Bathymetry);
                        originalBathymetry.Arguments[0].Clear();
                        //HACK: signals the interpolation method to use the grid node z-values...
                        originalBathymetry.Components[0].Clear(); 

                        bathymetryNoDataValue = GetBathymetryNoDataValue(originalBathymetry);
                    }

                    fileOperations.DoIfUgrid(uGridAdapter =>
                    {
                        if (1 > uGridAdapter.uGrid.GetNumberOf2DMeshes())
                        {
                            bathymetryNoDataValue = -999.0d;
                            return;
                        }

                        uGridAdapter.uGrid.GetAllNodeCoordinatesForMeshId(1);

                        bathymetryNoDataValue = uGridAdapter.uGrid.ZCoordinateFillValue;
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
            BeginEdit("Removing grid...");
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
            if (SpatialData.Bathymetry == null)
            {
                return;
            }

            double[] zValues = null;
            switch (bedLevelType)
            {
                case UnstructuredGridFileHelper.BedLevelLocation.Faces:
                case UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes:
                    if (SpatialData.Bathymetry is UnstructuredGridCellCoverage)
                    {
                        return;
                    }

                    zValues = GetZValuesFromNetFile(bedLevelType);
                    SpatialData.Bathymetry = UnstructuredGridCoverageFactory.CreateCellCoverage(
                        WaterFlowFMModelDefinition.BathymetryDataItemName, Grid, zValues);
                    break;
                case UnstructuredGridFileHelper.BedLevelLocation.CellEdges:
                    Log.WarnFormat(
                        Resources
                            .WaterFlowFMModel_UpdateBathymetryCoverage_Unstructured_grid_edge_coverages_are_not_currently_supported);
                    // Not supported yet, so create a VertexCoverage for now
                    if (SpatialData.Bathymetry is UnstructuredGridVertexCoverage)
                    {
                        return;
                    }

                    zValues = GetZValuesFromNetFile(bedLevelType);
                    SpatialData.Bathymetry = UnstructuredGridCoverageFactory.CreateVertexCoverage(
                        WaterFlowFMModelDefinition.BathymetryDataItemName, Grid, zValues);
                    break;
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev:
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev:
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev:
                    if (SpatialData.Bathymetry is UnstructuredGridVertexCoverage)
                    {
                        return;
                    }

                    zValues = GetZValuesFromNetFile(bedLevelType);
                    SpatialData.Bathymetry = UnstructuredGridCoverageFactory.CreateVertexCoverage(
                        WaterFlowFMModelDefinition.BathymetryDataItemName, Grid, zValues);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bedLevelType), bedLevelType, null);
            }

            Log.InfoFormat(
                Resources
                    .WaterFlowFMModel_UpdateBathymetryCoverage_The_BedLevel_location_specified_does_not_match_the_existing_BedLevel_data__a_new_BedLevel_Data_will_be_generated_);

            // re-apply spatial operations
            IDataItem bathymetryDataItem = SpatialData.DataItems.First(d => d.Value == SpatialData.Bathymetry);
            var spatialOperationsValueConverter = bathymetryDataItem.ValueConverter as SpatialOperationSetValueConverter;
            if (spatialOperationsValueConverter?.SpatialOperationSet != null &&
                spatialOperationsValueConverter.SpatialOperationSet.Operations.Any())
            {
                spatialOperationsValueConverter.SpatialOperationSet.Operations.ForEach(o => o.Execute());
                Log.InfoFormat(
                    Resources
                        .WaterFlowFMModel_UpdateBathymetryCoverage_Reapplying_existing_spatial_operations_to_new_BedLevel_Data);
            }
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

        private void SetSpatialCoverages()
        {
            LoadBathymetry();
            SpatialData.InitialWaterLevel = UnstructuredGridCoverageFactory.CreateCellCoverage(
                WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, Grid);
            SpatialData.InitialSalinity = UnstructuredGridCoverageFactory.CreateCellCoverage(
                WaterFlowFMModelDefinition.InitialSalinityDataItemName, Grid);
            SpatialData.InitialTemperature = UnstructuredGridCoverageFactory.CreateCellCoverage(
                WaterFlowFMModelDefinition.InitialTemperatureDataItemName, Grid);
            SpatialData.Viscosity = UnstructuredGridCoverageFactory.CreateFlowLinkCoverage(
                WaterFlowFMModelDefinition.ViscosityDataItemName, Grid);
            SpatialData.Diffusivity = UnstructuredGridCoverageFactory.CreateFlowLinkCoverage(
                WaterFlowFMModelDefinition.DiffusivityDataItemName, Grid);
            SpatialData.Roughness = UnstructuredGridCoverageFactory.CreateFlowLinkCoverage(
                WaterFlowFMModelDefinition.RoughnessDataItemName, Grid);
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
                    SpatialData.Bathymetry = UnstructuredGridCoverageFactory.CreateCellCoverage(
                        WaterFlowFMModelDefinition.BathymetryDataItemName, Grid, zValues);
                }
                else
                {
                    if (zValues == null)
                    {
                        zValues = grid.Vertices.Count > 0 ? grid.Vertices.Select(v => v.Z).ToArray() : null;
                    }

                    SpatialData.Bathymetry = UnstructuredGridCoverageFactory.CreateVertexCoverage(
                        WaterFlowFMModelDefinition.BathymetryDataItemName, Grid, zValues);
                }
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

        // Can be further optimized by letting InsertGrid accept lists of coverages
        private void UpdateSpatialDataAfterGridSet(UnstructuredGrid newGrid, bool nodesChanged, bool cellsChanged,
                                                   bool linksChanged)
        {
            foreach (IDataItem di in SpatialData.DataItems)
            {
                UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, di);
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
                                      .OfType<UnstructuredGridCoverage>().Except(originalCoverage))
                    {
                        UpdateCoverageAfterGridSet(cov, newGrid, nodesChanged, cellsChanged, linksChanged, false);
                    }

                    UpdateCoverageAfterGridSet(valueConverter.ConvertedValue as UnstructuredGridCoverage, newGrid,
                                               nodesChanged, cellsChanged, linksChanged, false);
                }
                finally
                {
                    dataItem.ValueConverter = valueConverter;
                    valueConverter.SpatialOperationSet.Execute();
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

            coverage.ReplaceMissingValuesWithDefaultValues();
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
                coverage.BeginEdit("Clearing grid from coverage");

                ClearVariable(coverage.Components[0]);
                ClearVariable(coverage.Arguments[0]);

                coverage.Grid = null;
                coverage.EndEdit();
                return;
            }

            if (IsBathymetryCoverage(coverage) && !coverage.GetValues<double>().Any())
            {
                switch (coverage)
                {
                    case UnstructuredGridVertexCoverage vertexCoverage: 
                        vertexCoverage.LoadBathymetry(newGrid, bathymetryNoDataValue);
                        return;
                    case UnstructuredGridCellCoverage cellCoverage:
                        cellCoverage.LoadBathymetry(newGrid, 
                                                    NetFilePath,
                                                    bathymetryNoDataValue);
                        return;
                }
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

        private static bool IsBathymetryCoverage(UnstructuredGridCoverage coverage) =>
            coverage.Name == WaterFlowFMModelDefinition.BathymetryDataItemName;


        private void InitializeSpatialDataItems()
        {
            SpatialDataItems = new EventedList<IDataItem>(SpatialData.DataItems);
            SpatialData.DataItems.CollectionChanged +=
                SyncHelper.GetSyncNotifyCollectionChangedEventHandler(SpatialDataItems);
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

        public int SnapVersion { get; private set; }

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