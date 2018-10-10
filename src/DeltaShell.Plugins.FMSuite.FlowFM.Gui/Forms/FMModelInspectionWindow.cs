using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BasicModelInterface;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Tools;
using SharpMap.UI.Tools.Decorations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public partial class FMModelInspectionWindow : Form
    {
        private readonly IEnumerable<IMapLayerProvider> mapLayerProviders;
        private Dictionary<string, QuantityInfo> supportedQuantities;
        private UnstructuredGrid outputGrid;
        private IBasicModelInterface api;
        private const string VelocityVectors = "vectorvelocity";
        private const string WindVelovityVectors = "windspeed";
        private bool firstTime = true;
        private DateTime lastDateTime;
        private int stepsToPerform;
        private bool autoPlay;
        private bool mapRendered;
        private bool gotMapFile;
        private Map map;
        private readonly IGui gui;
        

        public FMModelInspectionWindow(ITimeDependentModel model, IEnumerable<IMapLayerProvider> mapLayerProviders, IGui gui)
        {
            this.mapLayerProviders = mapLayerProviders;
            this.gui = gui;
            InitializeComponent();
            api = model.BMIEngine;
        }

        public void AfterExecute(WaterFlowFMModel model)
        {
            var currentTime = model.CurrentTime;

            if (stepsToPerform > 0)
            {
                if (lastDateTime != currentTime)
                    stepsToPerform--;
                lastDateTime = currentTime;
                if (stepsToPerform > 0)
                    return;
            }

            Text = string.Format("Model Inspection, model time: {0}", currentTime);

            mapRendered = false;

            if (firstTime)
            {
                supportedQuantities = LocationInfoApiHelper.ReadQuantities(model, api.VariableNames.Distinct().ToArray());
                GetOutputGrid(model);
                InitializeMap(model);
                firstTime = false;
            }
            else
            {
                var visibleCoverageLayers = map.GetAllVisibleLayers(false).OfType<UnstructuredGridCoverageBaseLayer>();
                foreach (var coverageLayer in visibleCoverageLayers)
                {
                    UpdateQuantityValues(api, coverageLayer.Coverage);
                }               
            }
        }

        private void GetOutputGrid(WaterFlowFMModel model)
        {
            var mapFilePath = Path.Combine(model.WorkingDirectory, model.ModelDefinition.RelativeMapFilePath);

            gotMapFile = File.Exists(mapFilePath);
            lblNoMapFile.Visible = !gotMapFile;
            outputGrid = gotMapFile
                ? NetFileImporter.ImportModelGrid(mapFilePath)
                : model.Grid;
        }

        private void InitializeMap(WaterFlowFMModel model)
        {
            map = new Map();
            map.MapRendered += map_MapRendered;

            // enable gradient tool
            mapControl.Tools.Add(new GradientThemeRangeTool());
            mapControl.MouseUp += mapControl_MouseUp;

            // query tool (always on)
            var queryTool = mapControl.Tools.OfType<QueryTool>().First();
            mapControl.ActivateTool(queryTool);
            mapControl.Map = map;

            var layerProviders = mapLayerProviders.ToArray();
            
            // add area layer
            var areaLayer = MapLayerProviderHelper.CreateLayersRecursive(model.Area, model, layerProviders);
            if (areaLayer != null){ map.Layers.Add(areaLayer); }

            // add Bathymetry
            var bathemetryLayer = MapLayerProviderHelper.CreateLayersRecursive(model.Bathymetry, model, layerProviders);
            if (bathemetryLayer != null) { map.Layers.Add(bathemetryLayer); }
            
            var gridLayer = MapLayerProviderHelper.CreateLayersRecursive(model.Grid, null, layerProviders);
            if (gridLayer != null)
            {
                map.Layers.Add(gridLayer);

                var vertexCoverages = GetGroupCoverageLayer(model, supportedQuantities.Where(q => q.Value.ElementType == ElementType.Vertex), "Verticies");
                var flowLinkCoverages = GetGroupCoverageLayer(model, supportedQuantities.Where(q => q.Value.ElementType == ElementType.FlowLink), "Flow Links");
                var cellCoverages = GetGroupCoverageLayer(model, supportedQuantities.Where(q => q.Value.ElementType == ElementType.Cell), "Cell Centers");

                if(vertexCoverages.Layers.Count > 0) map.Layers.Add(vertexCoverages);
                if (flowLinkCoverages.Layers.Count > 0) map.Layers.Add(flowLinkCoverages);
                if (cellCoverages.Layers.Count > 0) map.Layers.Add(cellCoverages);
            }

            // show s1 in map (Water level)
            var defaultLayer = map.GetAllLayers(false).First(l => l.Name == "s1"); // TODO: "Water level" (DELFT3DFM-431)
            if(defaultLayer != null) defaultLayer.Visible = true;

            map.PropertyChanged += MapPropertyChanged;
            mapLegendView.Data = map;
            map.ZoomToExtents();
        }

        private GroupLayer GetGroupCoverageLayer(WaterFlowFMModel model, IEnumerable<KeyValuePair<string, QuantityInfo>> quantities, string groupLayerName)
        {
            var groupedCoverages = new GroupLayer(groupLayerName);
            foreach (var quantity in quantities)
            {
                var coverage = CreateUnstructuredGridCoverage(quantity.Value, quantity.Key);
                var mapLayerProvider = mapLayerProviders.FirstOrDefault(lp => lp.CanCreateLayerFor(coverage, model));
                if (mapLayerProvider == null) continue;

                var layer = mapLayerProvider.CreateLayer(coverage, model);
                layer.Visible = false;
                layer.ShowInTreeView = true;
                groupedCoverages.Layers.Add(layer);

                var coverageLayer = layer as UnstructuredGridCoverageBaseLayer;
                if (coverageLayer != null) UpdateQuantityValues(api, coverageLayer.Coverage);
            }
            return groupedCoverages;
        }

        void mapControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (map.Layers.Any(l=>l.RenderRequired))
                mapControl.Refresh();
        }

        void map_MapRendered(System.Drawing.Graphics g)
        {
            mapRendered = true;
        }

        private void btnSingleStepClick(object sender, System.EventArgs e)
        {
            DoSteps(1);
        }

        private void btnCloseClick(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            DoSteps(100);
        }

        private void DoSteps(int numStepsToPerform)
        {
            stepsToPerform = numStepsToPerform;
            if (!autoPlay)
                btnPlayPause.Enabled = false;
            panelButtons.Enabled = false;
            DialogResult = DialogResult.OK;
        }

        public DialogResult WaitForUserInput()
        {
            // steps left to skip, don't wait for user input
            if (stepsToPerform > 0)
                return DialogResult.OK;

            Application.DoEvents(); // give redraw a chance (but not guaranteed)

            // if sync is on, we wait till the map is redrawn
            var syncWithRendering = chkSync.Checked;
            while (syncWithRendering && !mapRendered)
                Application.DoEvents();
            
            // still in auto play: don't wait for user input
            if (autoPlay)
                return DialogResult.OK;

            // not skipping / playing: wait for user, reenable buttons etc
            panelButtons.Enabled = true;
            btnPlayPause.Enabled = true;

            // create our own message loop here:
            DialogResult = DialogResult.None;
            while (DialogResult == DialogResult.None)
                Application.DoEvents();
            
            return DialogResult;
        }

        private void FMModelInspectionWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && !autoPlay)
            {
                e.SuppressKeyPress = true;
                autoPlay = true;
                DoSteps(1);
            }
        }

        private void FMModelInspectionWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                e.SuppressKeyPress = true;
                SetAutoPlay(false);
            }
        }

        private void btnPlayPauseClick(object sender, EventArgs e)
        {
            SetAutoPlay(!autoPlay); // toggle
        }

        private void SetAutoPlay(bool autoPlayOn)
        {
            if (autoPlayOn)
            {
                btnPlayPause.Text = "Pause";
                autoPlay = true;
                DoSteps(1);
            }
            else
            {
                autoPlay = false;
                btnPlayPause.Text = "Play";
            }
        }

        private void OnDispose()
        {
            if (mapControl != null)
            {
                var layers = mapControl.Map.Layers.ToList();
                mapControl.Map.Layers.Clear();
                foreach(var layer in layers)
                    layer.DisposeLayersRecursive();

                mapControl.Dispose();
                mapControl.Map = null;
                mapControl = null;
            }
        }

        private void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Visible") return;

            var coverageLayer = sender as UnstructuredGridCoverageBaseLayer;
            if (coverageLayer != null && coverageLayer.Visible)
            {
                UpdateQuantityValues(api, coverageLayer.Coverage);
            }
        }
 
        private UnstructuredGridCoverage CreateUnstructuredGridCoverage(QuantityInfo selectedQuantity, string quantityName)
        {
            UnstructuredGridCoverage coverage;
            switch (selectedQuantity.ElementType)
            {
                case ElementType.Vertex:
                    coverage = new UnstructuredGridVertexCoverage(outputGrid, false);
                    coverage.Components[0].Name = quantityName;
                    break;
                case ElementType.FlowLink:
                    if (selectedQuantity.BmiName == WindVelovityVectors)
                    {
                        coverage = new UnstructuredGridFlowLinkCoverage(outputGrid, false);
                        coverage.Components.Add(new Variable<double>());
                        coverage.Components[0].Name = "wx";
                        coverage.Components[1].Name = "wy";
                    }
                    else
                    {
                        coverage = new UnstructuredGridFlowLinkCoverage(outputGrid, false);
                        coverage.Components[0].Name = quantityName;
                    }
                    break;
                case ElementType.Cell:
                    if (selectedQuantity.BmiName == VelocityVectors)
                    {
                        coverage = new UnstructuredGridCellCoverage(outputGrid, false);
                        coverage.Components.Add(new Variable<double>());
                        coverage.Components[0].Name = "ucx";
                        coverage.Components[1].Name = "ucy";
                    }
                    else
                    {
                        coverage = new UnstructuredGridCellCoverage(outputGrid, false);
                        coverage.Components[0].Name = quantityName;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            coverage.Name = quantityName;
            foreach (var component in coverage.Components)
            {
                component.NoDataValue = -999.0;
            }
            return coverage;
        }

        private void UpdateQuantityValues(IBasicModelInterface api, ICoverage coverage)
        {
            var currentQuantity = supportedQuantities.FirstOrDefault(q => q.Key == coverage.Name).Value;
            if (currentQuantity == null) return;

            if (currentQuantity.BmiName == VelocityVectors)
            {
                var vx = GetCurrentQuantityValues(api, "ucx", ElementType.Cell,
                    (double) coverage.Components[0].NoDataValue);
                var vy = GetCurrentQuantityValues(api, "ucy", ElementType.Cell,
                    (double) coverage.Components[1].NoDataValue);
                
                coverage.Components[0].SetValues(vx);
                coverage.Components[1].SetValues(vy);
            }
            else if (currentQuantity.BmiName == WindVelovityVectors)
            {
                var vx = GetCurrentQuantityValues(api, "wx", ElementType.FlowLink,
                    (double) coverage.Components[0].NoDataValue);
                var vy = GetCurrentQuantityValues(api, "wy", ElementType.FlowLink,
                    (double) coverage.Components[1].NoDataValue);

                coverage.Components[0].SetValues(vx);
                coverage.Components[1].SetValues(vy);
            }
            else
            {
                var values = GetCurrentQuantityValues(api, currentQuantity.BmiName, currentQuantity.ElementType,
                    (double) coverage.Components[0].NoDataValue);

                if (values != null) coverage.SetValues(values);
            }
        }

        private IEnumerable<double> GetCurrentQuantityValues(IBasicModelInterface api, string bmiName,
            ElementType elementType, double noDataValue)
        {
            
            int maxExpected;
            switch (elementType)
            {
                case ElementType.Vertex:
                    maxExpected = outputGrid.Vertices.Count;
                    break;
                case ElementType.FlowLink:
                    maxExpected = outputGrid.FlowLinks.Count;
                    break;
                case ElementType.Cell:
                    maxExpected = outputGrid.Cells.Count;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            try
            {
                var values = api.GetValues(bmiName) as double[];
                if (values != null && values.Length > 0)
                    return values.Take(maxExpected);
            }
            catch (Exception)
            {
                return Enumerable.Repeat(noDataValue, maxExpected);
            }
            return null;
        }

        private void FMModelInspectionWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
