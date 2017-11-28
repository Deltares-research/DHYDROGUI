using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.Api.TempImpl;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
        private UnstructuredGrid grid;
        private double bathymetryNoDataValue;

        public UnstructuredGridCoverage Bathymetry { get; private set; }
        public UnstructuredGridCellCoverage InitialWaterLevel { get; private set; }
        public CoverageDepthLayersList InitialSalinity { get; private set; }
        public UnstructuredGridCellCoverage InitialTemperature { get; private set; }
        public UnstructuredGridFlowLinkCoverage Roughness { get; private set; }
        public UnstructuredGridFlowLinkCoverage Viscosity { get; private set; }
        public UnstructuredGridFlowLinkCoverage Diffusivity { get; private set; }
        public IEventedList<UnstructuredGridCellCoverage> InitialTracers { get; private set; }
        public IEventedList<UnstructuredGridCellCoverage> InitialFractions { get; private set; }

        protected virtual IGridOperationApi gridOperationApi { get; set; }
        protected virtual UnstrucGridOperationApi runTimeGridOperationApi { get; set; } //lives on the worker thread...
        protected virtual bool snapApiInErrorMode { get; set; }

        public UnstructuredGrid Grid
        {
            get
            {
                return grid;
            }
            set
            {
                if (grid == value) return;

                bool verticesEqual;
                bool cellsEqual;
                bool linksEqual;

                var gridsAreEqual = UnstructuredGridHelper.CompareGrids(grid, value, out verticesEqual, out cellsEqual, out linksEqual);
                ((INotifyPropertyChanged)this).PropertyChanged -= OnGridChanged;

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
                ((INotifyPropertyChanged)this).PropertyChanged += OnGridChanged;
            }
        }

        private void OnGridChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName != GridPropertyName) return;
            RefreshMappings();
        }

        public void SaveGrid()
        {
            try
            {
                var metaData = new UGridGlobalMetaData(Name, FlowFMApplicationPlugin.PluginName, FlowFMApplicationPlugin.PluginVersion);
                using (var uGrid2D = new UGrid(NetFilePath, metaData))
                {
                    // Calls for writing grid data (cloning)
                }
            }
            catch (Exception ex)
            {
                throw ex; // TODO: Rethrow the exception?
            }
        }

        private static IList<FlowLink> GenerateFlowLinksForEdges(UnstructuredGrid grid)
        {
            // optimized for performance
            var flowLinks = new List<FlowLink>();

            for (int index = 0; index < grid.Edges.Count; index++)
            {
                var gridEdge = grid.Edges[index];

                IList<int> gridVertexToCellIndex;
                grid.VertexToCellIndices.TryGetValue(gridEdge.VertexFromIndex, out gridVertexToCellIndex);
                if (gridVertexToCellIndex == null)
                    continue;

                IList<int> vertexToCellIndex;
                grid.VertexToCellIndices.TryGetValue(gridEdge.VertexToIndex, out vertexToCellIndex);
                if (vertexToCellIndex == null)
                    continue;

                var cellOne = -1;
                var cellTwo = -1;
                var moreThanTwo = false;
                for (int i = 0; i < gridVertexToCellIndex.Count; i++)
                {
                    var value1 = gridVertexToCellIndex[i];

                    for (int j = 0; j < vertexToCellIndex.Count; j++)
                    {
                        var value2 = vertexToCellIndex[j];

                        if (value1 != value2)
                            continue;

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

        public Envelope GridExtent { get; private set; }

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
                var isPartOf1D2DModel = (bool)ModelDefinition.GetModelProperty(GuiProperties.PartOf1D2DModel).Value;

                var newGrid = ReadGridFromNetFile(NetFilePath, isPartOf1D2DModel); //may throw...
                if (newGrid == null)
                {
                    Grid = new UnstructuredGrid();
                }
                else
                {
                    if (loadBathymetry)
                    {
                        var originalBathymetry = GetOriginalCoverage(Bathymetry);
                        originalBathymetry.Arguments[0].Clear();
                        originalBathymetry.Components[0].Clear(); //HACK: signals the interpolation method to use the grid node z-values...
                        double ndv ;
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
                            return ;
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

        private UnstructuredGridCoverage GetOriginalCoverage(UnstructuredGridCoverage coverage)
        {
            var dataItem = GetDataItemByValue(coverage);
            if (dataItem == null) return coverage;
            var valueConverter = dataItem.ValueConverter as CoverageSpatialOperationValueConverter;
            if (valueConverter == null) return coverage;
            return valueConverter.OriginalValue as UnstructuredGridCoverage;
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

        private static void WriteNetFile(string path, UnstructuredGrid grid)
        {
            if (path == null) return;
            UnstructuredGridFileHelper.WriteGridToFile(path, grid);
        }

        private IEnumerable<UnstructuredGridCoverage> SpatialData
        {
            get
            {
                yield return Bathymetry;
                yield return InitialWaterLevel;
                if (InitialSalinity != null)
                {
                    foreach (var initialSalinity in InitialSalinity.Coverages.OfType<UnstructuredGridCoverage>())
                    {
                        yield return initialSalinity;
                    }
                }
                if (InitialTracers != null)
                {
                    foreach (var tracer in InitialTracers)
                    {
                        yield return tracer;
                    }
                }
                if (InitialFractions != null)
                {
                    foreach (var fraction in InitialFractions)
                    {
                        yield return fraction;
                    }
                }
                yield return InitialTemperature;
                yield return Roughness;
                yield return Viscosity;
                yield return Diffusivity;
            }
        }

        private void InitializeUnstructuredGridCoverages()
        {
            var bathymetryValues = grid.Vertices.Count > 0 ? grid.Vertices.Select(v => v.Z) : null;
            Bathymetry = CreateUnstructuredGridVertexCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName, Grid, bathymetryValues);

            InitialWaterLevel = CreateUnstructuredGridCellCoverage(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, Grid);
            InitialSalinity = new CoverageDepthLayersList(s => CreateUnstructuredGridCellCoverage(s, Grid))
            {
                Name = WaterFlowFMModelDefinition.InitialSalinityDataItemName,
                VerticalProfile = new VerticalProfileDefinition(VerticalProfileType.Uniform),
            };
            InitialSalinity.Coverages.CollectionChanged += SpatialDataLayersChanged;
            InitialTracers = new EventedList<UnstructuredGridCellCoverage>();
            InitialTracers.CollectionChanged += SpatialDataTracersChanged;
            InitialTemperature = CreateUnstructuredGridCellCoverage(WaterFlowFMModelDefinition.InitialTemperatureDataItemName, Grid);
            Viscosity = CreateUnstructuredGridFlowLinkCoverage(WaterFlowFMModelDefinition.ViscosityDataItemName, Grid);
            Diffusivity = CreateUnstructuredGridFlowLinkCoverage(WaterFlowFMModelDefinition.DiffusivityDataItemName,Grid);
            Roughness = CreateUnstructuredGridFlowLinkCoverage(WaterFlowFMModelDefinition.RoughnessDataItemName, Grid);
            InitialFractions = new EventedList<UnstructuredGridCellCoverage>();
            InitialFractions.CollectionChanged += SpatialDataFractionsChanged;
        }

        internal void UpdateBathymetryCoverage(UnstructuredGridFileHelper.BedLevelLocation bedLevelType)
        {
            if (Bathymetry == null) return;

            switch (bedLevelType)
            {
                case UnstructuredGridFileHelper.BedLevelLocation.Faces:
                case UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes:
                    if (Bathymetry is UnstructuredGridCellCoverage) return;
                    // For now we just create a new Bathemetry and log a warning
                    // Later will will re-apply values converting from one coverage type to another
                    Bathymetry = CreateUnstructuredGridCellCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName, Grid);
                    break;
                case UnstructuredGridFileHelper.BedLevelLocation.CellEdges:
                    Log.WarnFormat("Unstructured grid edge coverages are not currently supported");
                    // Not supported yet, so create a VertexCoverage for now
                    if (Bathymetry is UnstructuredGridVertexCoverage) return;
                    // For now we just create a new Bathemetry and log a warning
                    // Later will will re-apply values converting from one coverage type to another
                    Bathymetry = CreateUnstructuredGridVertexCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName, Grid);
                    break;
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev:
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev:
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev:
                    if (Bathymetry is UnstructuredGridVertexCoverage) return;
                    // For now we just create a new Bathemetry and log a warning
                    // Later will will re-apply values converting from one coverage type to another
                    Bathymetry = CreateUnstructuredGridVertexCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName, Grid);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("bedLevelType", bedLevelType, null);
            }

            // assuming you didn't already return (above)
            Log.WarnFormat("The BedLevel location specified does not match the existing BedLevel data, a new BedLevel Data will be generated");

            var bedLevelDataItem = DataItems.FirstOrDefault(di => di.Name == WaterFlowFMModelDefinition.BathymetryDataItemName);
            if (bedLevelDataItem == null) return;

            bedLevelDataItem.Value = Bathymetry;

            // For now we just remove the spatial operations
            // Later we will re-apply them - DELFT3DFM-1031
            if (bedLevelDataItem.ValueConverter != null)
            {
                var spatialOperationsValueConverter = bedLevelDataItem.ValueConverter as SpatialOperationSetValueConverter;

                if (spatialOperationsValueConverter != null &&
                    spatialOperationsValueConverter.SpatialOperationSet != null)
                {
                    spatialOperationsValueConverter.SpatialOperationSet.Operations.Clear();
                }
                    
            }
        }

        private static UnstructuredGridVertexCoverage CreateUnstructuredGridVertexCoverage(string name, UnstructuredGrid grid, IEnumerable<double> componentValues = null)
        {
            return CreateUnstructuredGridCoverage(name, grid,
                () => new UnstructuredGridVertexCoverage(new UnstructuredGrid(), false),
                () => Enumerable.Range(0, grid.Vertices.Count),
                componentValues);
        }

        private static UnstructuredGridCellCoverage CreateUnstructuredGridCellCoverage(string name, UnstructuredGrid grid, IEnumerable<double> componentValues = null)
        {
            return CreateUnstructuredGridCoverage(name, grid,
                () => new UnstructuredGridCellCoverage(new UnstructuredGrid(), false),
                () => Enumerable.Range(0, grid.Cells.Count),
                componentValues);
        }

        private static UnstructuredGridFlowLinkCoverage CreateUnstructuredGridFlowLinkCoverage(string name, UnstructuredGrid grid, IEnumerable<double> componentValues = null)
        {
            return CreateUnstructuredGridCoverage(name, grid,
                () => new UnstructuredGridFlowLinkCoverage(new UnstructuredGrid(), false),
                () => Enumerable.Range(0, grid.FlowLinks.Count),
                componentValues);
        }

        private static T CreateUnstructuredGridCoverage<T>(string name, UnstructuredGrid grid, Func<T> createCoverage, Func<IEnumerable<int>> argumentValues, IEnumerable<double> componentValues = null) where T : UnstructuredGridCoverage
        {
            var result = createCoverage();

            result.Name = name;
            result.Grid = grid;

            result.Components[0].NoDataValue = -999d;
            result.Components[0].DefaultValue = -999d;

            FunctionHelper.SetValuesRaw(result.Arguments[0], argumentValues());

            var values = componentValues ?? Enumerable.Repeat(-999d, result.Arguments[0].Values.Count);
            FunctionHelper.SetValuesRaw(result.Components[0], values);

            return result;
        }

        private static UnstructuredGrid ReadGridFromNetFile(string netFilePath, bool is1D2DModel)
        {
            if (is1D2DModel)
            {
                try
                {
                    // Try to import the grid after an init step from FM kernel, in order to get the renumbered grid.
                    return GridHelper.CreateUnstructuredGridFromNetCdfFor1D2DLinks(netFilePath);
                }
                catch (Exception e)
                {
                    // Log exception but continue.
                    Log.WarnFormat("Error when reading grid after 1d2d initialisation step in the D-FLow FM kernel: {0}", e.Message);
                }
            }
            
            return UnstructuredGridFileHelper.LoadFromFile(netFilePath);
        }

        // Can be further optimized by letting InsertGrid accept lists of coverages
        private void UpdateSpatialDataAfterGridSet(UnstructuredGrid newGrid, bool nodesChanged, bool cellsChanged, bool linksChanged)
        {
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, Bathymetry, g => Bathymetry = g);
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, InitialWaterLevel, g => InitialWaterLevel = g);
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, Roughness, g => Roughness = g);
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, Viscosity, g => Viscosity = g);
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, Diffusivity, g => Diffusivity = g);
            UpdateCoverageGrid(newGrid, nodesChanged, cellsChanged, linksChanged, InitialTemperature, g => InitialTemperature = g);
            
            if (InitialSalinity != null)
            {
                var initialSalinity = InitialSalinity;
                InitialSalinity = null; // prevent events
                try
                {
                    foreach (var salinityLayer in initialSalinity.Coverages)
                    {
                        UpdateQuantityAfterGridSet(salinityLayer as UnstructuredGridCoverage, newGrid, nodesChanged, cellsChanged, linksChanged);
                    }
                }
                finally
                {
                    InitialSalinity = initialSalinity;
                }
            }

            if (InitialTracers != null)
            {
                var initialTracers = InitialTracers;
                InitialTracers = null; // prevents events
                try
                {
                    foreach (var tracer in initialTracers)
                    {
                        UpdateQuantityAfterGridSet(tracer, newGrid, nodesChanged, cellsChanged, linksChanged);
                    }
                }
                finally
                {
                    InitialTracers = initialTracers;                    
                }
            }

            if (InitialFractions != null)
            {
                var initialFracts = InitialFractions;
                try
                {
                    foreach (var fraction in initialFracts)
                    {
                        UpdateQuantityAfterGridSet(fraction, newGrid, nodesChanged, cellsChanged, linksChanged);
                    }
                }
                finally
                {
                    InitialFractions = initialFracts;
                }
            }
        }

        private void UpdateCoverageGrid<T>(UnstructuredGrid newGrid, bool nodesChanged, bool cellsChanged, bool linksChanged, T coverage, Action<T> setCoverage) where T : UnstructuredGridCoverage
        {
            if (coverage == null) return;

            var coverageref = coverage;// prevents events
            setCoverage(null);
            try
            {
                UpdateQuantityAfterGridSet(coverageref, newGrid, nodesChanged, cellsChanged, linksChanged);
            }
            finally
            {
                setCoverage(coverage);
            }
        }

        private void UpdateQuantityAfterGridSet(UnstructuredGridCoverage coverage, UnstructuredGrid newGrid,
            bool nodesChanged, bool cellsChanged, bool linksChanged)
        {
            if (coverage == null) return;

            var dataItem = DataItems.FirstOrDefault(di => Equals(di.Value, coverage));
            SpatialOperationSetValueConverter valueConverter = null;

            if (dataItem != null)
            {
                valueConverter = dataItem.ValueConverter as SpatialOperationSetValueConverter;
            }

            if (valueConverter != null && valueConverter.SpatialOperationSet.Operations.Any())
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
                    foreach (var cov in
                        valueConverter.SpatialOperationSet.GetAllFeatureProviders()
                            .OfType<CoverageFeatureProvider>()
                            .Select(fp => fp.Coverage)
                            .OfType<UnstructuredGridCoverage>().Except(new[] {originalCoverage}))
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
                if (dataItem != null)
                {
                    dataItem.Value = null;
                }
                try
                {
                    UpdateCoverageAfterGridSet(coverage, newGrid, nodesChanged, cellsChanged, linksChanged, true);

                }
                finally
                {
                    if (dataItem != null)
                    {
                        dataItem.Value = coverage;
                    }
                }
            }
        }

        private static void ClearVariable(IVariable variable)
        {
            var targetMda = variable.Values;

            var wasFiring = targetMda.FireEvents;
            var wasAutoSorted = targetMda.IsAutoSorted;
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
            bool nodesChanged, bool cellsChanged, bool linksChanged, bool reInterpolate)
        {
            if (disposing) return;

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
            if (vertexCoverage!=null && vertexCoverage.Name == WaterFlowFMModelDefinition.BathymetryDataItemName)
            {
                if (!vertexCoverage.GetValues<double>().Any())
                {
                    vertexCoverage.LoadBathymetry(newGrid, bathymetryNoDataValue);
                    return;
                }
            }

            if ((coverage is UnstructuredGridVertexCoverage && nodesChanged) ||
                (coverage is UnstructuredGridCellCoverage && cellsChanged) ||
                (coverage is UnstructuredGridFlowLinkCoverage && linksChanged))
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
            if (GridExtent == null) return true;
            var extentsPlusMargin = new Envelope(0.9 * GridExtent.MinX, 1.1 * GridExtent.MaxX, 0.9 * GridExtent.MinY,
                1.1 * GridExtent.MaxY);
            return extentsPlusMargin.Intersects(geometry.EnvelopeInternal);
        }

        // Creation should only occur on the UI thread!!!
        private IGridOperationApi GetGridSnapApi()
        {
            if (snapApiInErrorMode) // very crude..
            {
                Log.WarnFormat("Last attempt to perform snapping operation failed, please verify the model first.");
            }
            try
            {
                if (runTimeGridOperationApi != null)
                {
                    return runTimeGridOperationApi;
                }
                return gridOperationApi ?? (gridOperationApi = new UnstrucGridOperationApi(this));
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Kernel failed to perform operation: {0}", e.Message);
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
                    Log.ErrorFormat("Error disposing grid operation api: {0}", ex.Message);
                }
            }
            gridOperationApi = null;
            snapApiInErrorMode = false;
        }

        public IGeometry GetGridSnappedGeometry(string featureType, IGeometry geometry)
        {
            return GetGridSnapApi().GetGridSnappedGeometry(featureType, geometry);
        }

        public IEnumerable<IGeometry> GetGridSnappedGeometry(string featureType, ICollection<IGeometry> geometries)
        {
            return GetGridSnapApi().GetGridSnappedGeometry(featureType, geometries);
        }

        public int[] GetLinkedCells()
        {
            return GetGridSnapApi().GetLinkedCells();
        }

        #endregion
    }
}
