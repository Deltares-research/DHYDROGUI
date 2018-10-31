using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.Api.TempImpl;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Coverages;
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
                    WriteNetFile(NetFilePath, Grid, Network, NetworkDiscretization, Links, Name, FlowFMApplicationPlugin.PluginName, FlowFMApplicationPlugin.PluginVersion, BedLevelLocation, BedLevelZValues);
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
                    WriteNetFile(NetFilePath, Grid, Network, NetworkDiscretization, Links, Name, FlowFMApplicationPlugin.PluginName, FlowFMApplicationPlugin.PluginVersion, BedLevelLocation, BedLevelZValues);
                }
            }
            finally
            {
                EndEdit();
            }
        }

        private double[] BedLevelZValues
        {
            get { return ModelDefinition.Bathymetry.Components[0].GetValues<double>().ToArray(); }
        }
        private UnstructuredGridFileHelper.BedLevelLocation BedLevelLocation
        {
            get
            {
                var bedLevelTypeProperty = modelDefinition.Properties.FirstOrDefault(p =>
                    p.PropertyDefinition != null &&
                    p.PropertyDefinition.MduPropertyName.ToLower() == KnownProperties.BedlevType);

                if (bedLevelTypeProperty == null)
                {
                    Log.WarnFormat("Cannot determine Bed level location, z-values will be exported as {0}", default(UnstructuredGridFileHelper.BedLevelLocation));
                    return default(UnstructuredGridFileHelper.BedLevelLocation);
                }

                return (UnstructuredGridFileHelper.BedLevelLocation)bedLevelTypeProperty.Value;

            }
        }
        public void WriteNetFile(string path)
        {
            WriteNetFile(path, Grid, Network, NetworkDiscretization, Links, Name, FlowFMApplicationPlugin.PluginName, FlowFMApplicationPlugin.PluginVersion, BedLevelLocation, BedLevelZValues);
        }

        private static void WriteNetFile(string path, UnstructuredGrid grid, IHydroNetwork network, IDiscretization networkDiscretization
            , IEnumerable<ILink1D2D> links, string name, string pluginName, string pluginVersion, UnstructuredGridFileHelper.BedLevelLocation location, double[] zValues)
        {
            if (path == null) return;
            UnstructuredGridFileHelper.WriteGridToFile(path, grid, network, networkDiscretization, links, name, pluginName, pluginVersion, location, zValues);
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
            var bedLevelLocationsForUnstructuredGridCellCoverage = new List<UnstructuredGridFileHelper.BedLevelLocation>
            {
                UnstructuredGridFileHelper.BedLevelLocation.Faces,
                UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes
            };

            // Update bathymetry coverage based on specified value in .mdu file
            var bedLevelTypeProperty = ModelDefinition.Properties.FirstOrDefault(p =>
                p.PropertyDefinition.MduPropertyName.ToLower() == KnownProperties.BedlevType);
            if (bedLevelTypeProperty != null)
            {
                var bedLevelType = (UnstructuredGridFileHelper.BedLevelLocation) bedLevelTypeProperty.Value;
                var zValues = GetZValuesFromNetFile(bedLevelType);
                if (bedLevelLocationsForUnstructuredGridCellCoverage.Contains(bedLevelType))
                {
                    Bathymetry = CreateUnstructuredGridCellCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName,
                        Grid, zValues);
                }
                else
                {
                    if (zValues == null)
                        zValues = grid.Vertices.Count > 0 ? grid.Vertices.Select(v => v.Z).ToArray() : null;
                    Bathymetry = CreateUnstructuredGridVertexCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName,
                        Grid, zValues);
                }

                // sync ModelDefinition with new Bathymetry
                ModelDefinition.Bathymetry = Bathymetry;
            }

            InitialWaterLevel =
                CreateUnstructuredGridCellCoverage(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, Grid);
            InitialSalinity = new CoverageDepthLayersList(s => CreateUnstructuredGridCellCoverage(s, Grid))
            {
                Name = WaterFlowFMModelDefinition.InitialSalinityDataItemName,
                VerticalProfile = new VerticalProfileDefinition(VerticalProfileType.Uniform),
            };
            InitialSalinity.Coverages.CollectionChanged += SpatialDataLayersChanged;
            InitialTracers = new EventedList<UnstructuredGridCellCoverage>();
            InitialTracers.CollectionChanged += SpatialDataTracersChanged;
            InitialTemperature =
                CreateUnstructuredGridCellCoverage(WaterFlowFMModelDefinition.InitialTemperatureDataItemName, Grid);
            Viscosity = CreateUnstructuredGridFlowLinkCoverage(WaterFlowFMModelDefinition.ViscosityDataItemName, Grid);
            Diffusivity =
                CreateUnstructuredGridFlowLinkCoverage(WaterFlowFMModelDefinition.DiffusivityDataItemName, Grid);
            Roughness = CreateUnstructuredGridFlowLinkCoverage(WaterFlowFMModelDefinition.RoughnessDataItemName, Grid);
            InitialFractions = new EventedList<UnstructuredGridCellCoverage>();
            InitialFractions.CollectionChanged += SpatialDataFractionsChanged;
        }

        internal void UpdateBathymetryCoverage(UnstructuredGridFileHelper.BedLevelLocation bedLevelType)
        {
            if (Bathymetry == null) return;

            double[] zValues = null;
            switch (bedLevelType)
            {
                case UnstructuredGridFileHelper.BedLevelLocation.Faces:
                case UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes:
                    if (Bathymetry is UnstructuredGridCellCoverage) return;
                    zValues = GetZValuesFromNetFile(bedLevelType);
                    Bathymetry = CreateUnstructuredGridCellCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName, Grid, zValues);
                    break;
                case UnstructuredGridFileHelper.BedLevelLocation.CellEdges:
                    Log.WarnFormat(Resources.WaterFlowFMModel_UpdateBathymetryCoverage_Unstructured_grid_edge_coverages_are_not_currently_supported);
                    // Not supported yet, so create a VertexCoverage for now
                    if (Bathymetry is UnstructuredGridVertexCoverage) return;
                    zValues = GetZValuesFromNetFile(bedLevelType);
                    Bathymetry = CreateUnstructuredGridVertexCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName, Grid, zValues);
                    break;
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev:
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev:
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev:
                    if (Bathymetry is UnstructuredGridVertexCoverage) return;
                    zValues = GetZValuesFromNetFile(bedLevelType);
                    Bathymetry = CreateUnstructuredGridVertexCoverage(WaterFlowFMModelDefinition.BathymetryDataItemName, Grid, zValues);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("bedLevelType", bedLevelType, null);
            }

            // sync ModelDefinition with new Bathymetry
            ModelDefinition.Bathymetry = Bathymetry;

            // update BedLevel DataItem
            var bedLevelDataItem = DataItems.FirstOrDefault(di => di.Name == WaterFlowFMModelDefinition.BathymetryDataItemName);
            if (bedLevelDataItem == null) return;

            Log.InfoFormat(Resources.WaterFlowFMModel_UpdateBathymetryCoverage_The_BedLevel_location_specified_does_not_match_the_existing_BedLevel_data__a_new_BedLevel_Data_will_be_generated_);
            bedLevelDataItem.Value = Bathymetry;

            // re-apply spatial operations
            var spatialOperationsValueConverter = bedLevelDataItem.ValueConverter as SpatialOperationSetValueConverter;
            if (spatialOperationsValueConverter?.SpatialOperationSet != null && spatialOperationsValueConverter.SpatialOperationSet.Operations.Any())
            {
                spatialOperationsValueConverter.SpatialOperationSet.Operations.ForEach(o => o.Execute());
                Log.InfoFormat(Resources.WaterFlowFMModel_UpdateBathymetryCoverage_Reapplying_existing_spatial_operations_to_new_BedLevel_Data);
            }
        }

        private double[] GetZValuesFromNetFile(UnstructuredGridFileHelper.BedLevelLocation bedLevelType)
        {
            if(string.IsNullOrEmpty(NetFilePath) || !File.Exists(NetFilePath)) return null;
            var zValuesFromNetFile = UnstructuredGridFileHelper.ReadZValues(NetFilePath, bedLevelType);
            return zValuesFromNetFile.Length > 0 ? zValuesFromNetFile : null;
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
                    Log.WarnFormat(Resources.WaterFlowFMModel_ReadGridFromNetFile_Error_when_reading_grid_after_1d2d_initialisation_step_in_the_D_FLow_FM_kernel___0_, e.Message);
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
                // TODO: this method does not take bathymetry as an UnstructuredGridCellCoverage into account! (DELFT3DFM-1355)
                if (!vertexCoverage.GetValues<double>().Any())
                {
                    vertexCoverage.LoadBathymetry(newGrid, bathymetryNoDataValue);
                    return;
                }
            }
            
            if ((coverage is UnstructuredGridVertexCoverage && nodesChanged) ||
                (coverage is UnstructuredGridCellCoverage && cellsChanged ) ||
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
        private IGridOperationApi GetGridSnapApi(bool fullExport = true)
        {
            if (snapApiInErrorMode) // very crude..
            {
                Log.WarnFormat(Resources.WaterFlowFMModel_GetGridSnapApi_Last_attempt_to_perform_snapping_operation_failed__please_verify_the_model_first_);
            }
            try
            {
                if (runTimeGridOperationApi != null)
                {
                    return runTimeGridOperationApi;
                }
                var api =  gridOperationApi ?? (gridOperationApi = new UnstrucGridOperationApi(this, fullExport));
                snapApiInErrorMode = false;
                return api;
            }
            catch (Exception e)
            {
                Log.ErrorFormat(Resources.WaterFlowFMModel_GetGridSnapApi_Kernel_failed_to_perform_operation___0_, e.Message);
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
                    Log.ErrorFormat(Resources.WaterFlowFMModel_DisposeSnapApi_Error_disposing_grid_operation_api___0_, ex.Message);
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
