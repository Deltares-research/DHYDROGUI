using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Styles;
using SharpMap.UI.Helpers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools
{
    public class GenerateLinksMapTool: MapTool
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(GenerateLinksMapTool));

        private const string SelectPolygonLayerName = "temp_layer";
        private NewLineTool selectPolygonTool;
        private Cursor selectPolygonCursor;
        private VectorLayer cachedPolygonLayer;
        private NetworkCoverageLocationLayer discretizationLayer;
        private UnstructuredGridLayer gridLayer;

        // This tool has 2 stages: first user draws a bounding polygon, during which InDrawingStage == true.
        // After drawing was finished by user (double-click) the function ExecuteWizard is called.
        private bool InDrawingStage { get; set; }

        public GenerateLinksMapTool()
        {
            Name = FlowFMMapViewDecorator.GenerateLinksToolName;
            Func<ILayer, bool> boundingPolygonLayerFilter = l => l.Equals(SelectPolygonLayer);
            selectPolygonCursor = MapCursors.CreateArrowOverlayCuror(Resources.guide);
            selectPolygonTool = new NewLineTool(boundingPolygonLayerFilter, "select_polygon_tool")
            {
                Cursor = selectPolygonCursor
            };
            InDrawingStage = false;
        }

        public override bool Enabled
        {
            get
            {
                // Check discretization exists and has data. 
                discretizationLayer = Map.GetAllLayers(false).OfType<NetworkCoverageLocationLayer>()
                    .FirstOrDefault(l => l.Coverage is IDiscretization);
                var discretization = discretizationLayer?.Coverage as IDiscretization;
                if (discretization == null)
                {
                    return false;
                }

                // Check grid exists and has data. 
                gridLayer = Map.GetAllLayers(false).OfType<UnstructuredGridLayer>()
                    .FirstOrDefault();
                var grid = gridLayer?.Grid;
                if (grid == null)
                {
                    return false;
                }

                return (discretization.Locations.GetValues().Any() && grid.Cells.Any());
            }
        }

        public override Cursor Cursor
        {
            get { return selectPolygonCursor; }
        }

        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            if (!InDrawingStage)
            {
                InDrawingStage = true;
                EnterBoundingBoxDrawingMode();
            }
            selectPolygonTool.OnMouseDown(worldPosition, e);
        }

        public override void OnMouseMove(Coordinate worldPosition, MouseEventArgs e)
        {
            if (InDrawingStage)
            {
                selectPolygonTool.OnMouseMove(worldPosition, e);
            }
        }

        public override void OnMouseUp(Coordinate worldPosition, MouseEventArgs e)
        {
            if (InDrawingStage)
            {
                selectPolygonTool.OnMouseUp(worldPosition, e);
            }
        }

        public override void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (InDrawingStage)
            {
                InDrawingStage = false;
                selectPolygonTool.OnMouseDoubleClick(sender, e);
                ExecuteGenerateLinks();
            }
        }

        public override void Render(Graphics graphics, Map mapBox)
        {
            if (InDrawingStage)
            {
                selectPolygonTool.Render(graphics, mapBox);
            }
        }

        private void Cleanup()
        {
            // remove temporary layer
            Map.Layers.Remove(SelectPolygonLayer);
            cachedPolygonLayer = null;
            Map.ZoomToExtents();
        }

        private void ExecuteGenerateLinks()
        {
            var fmLayer = Map.GetAllLayers(true).OfType<ModelGroupLayer>().FirstOrDefault(l => l.Model is WaterFlowFMModel);
            var selectedArea = GetSelectedAreaAsPolygon();
            if (fmLayer == null)
            {
                log.Error("Can not find the FM layer to create the grid on.");
            }
            if (fmLayer == null || selectedArea == null)
            {
                Cleanup();
                return;
            }

            var fmModel = (WaterFlowFMModel)fmLayer.Model;

            try
            {
                GenerateLinks(fmModel, selectedArea);
            }
            catch (Exception e)
            {
                log.Warn("Error or warning while generating links: " + e.Message);
            }
            finally
            {
                Cleanup();
            }
        }

        private void GenerateLinks(WaterFlowFMModel fmModel, IPolygon selectedArea)
        {
            //check conditions
            if (string.IsNullOrWhiteSpace(fmModel.NetFilePath)) return;
            if (fmModel.Grid == null || !fmModel.Grid.Cells.Any()) return;
            if (fmModel.NetworkDiscretization == null || !fmModel.NetworkDiscretization.Locations.AllValues.Any()) return;

            // Talk to the api!
            var gGeomApi = new GridGeomApi();
            var linksFrom = new List<int>();
            var linksTo = new List<int>();
            var startIndex = 1;
            int linksCount = 0;

            try
            {
                var ierr = gGeomApi.Get1d2dLinksFromGridAndNetwork(fmModel.NetFilePath, fmModel.NetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount, selectedArea);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    log.ErrorFormat("1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.", Name);
                    return;
                }

                fmModel.Links.Clear();
                fmModel.Links.AddRange(Creates1d2dLinks(linksCount, linksFrom, linksTo, fmModel.Grid, fmModel.NetworkDiscretization));
            }
            catch (Exception e)
            {
                log.DebugFormat("Unexpected exception thrown while generating 1D2D links: {0}", e.Message);
                log.ErrorFormat("1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.", Name);
            }
        }

        private IList<WaterFlowFM1D2DLink> Creates1d2dLinks(int linksCount, List<int> linksToIndex, List<int> linksFromIndex, UnstructuredGrid grid, IDiscretization networkDiscretization)
        {
            var lstNewLinks = new List<WaterFlowFM1D2DLink>();
            for (int i = 0; i < linksCount; i++)
            {
                var toCell = linksToIndex[i] - 1;
                var fromPoint = linksFromIndex[i] - 1;

                var fromCell = grid.Cells[toCell];
                var toNode = networkDiscretization.Locations.Values[fromPoint];
                var link = new WaterFlowFM1D2DLink(fromPoint, toCell)
                {
                    Geometry = new LineString(new[] { fromCell.Center, toNode.Geometry.Coordinate })
                };
                lstNewLinks.Add(link);
            }
            return lstNewLinks;
        }

        private void EnterBoundingBoxDrawingMode()
        {
            Map.Layers.Add(SelectPolygonLayer);
            MapControl.Tools.Add(selectPolygonTool);
            selectPolygonTool.MapControl = MapControl;
        }

        // TODO: Method seems to be unused, remove?
        [Obsolete]
        private void ExitBoundingBoxDrawingMode()
        {
            MapControl.Tools.Remove(selectPolygonTool);
        }

        private IPolygon GetSelectedAreaAsPolygon()
        {
            var feature = SelectPolygonLayer.DataSource.Features[0] as Feature;
            var lineString = feature != null ? feature.Geometry as ILineString : null;

            if (lineString == null)
            {
                log.Error("Polygon for grid creation must be an ILineString.");
                return null;
            }

            // Construct the userPolygon
            var userPolygon = ConvertToPolygon(lineString);
            if (userPolygon == null)
            {
                return null;
            }

            if (discretizationLayer != null && discretizationLayer.CoordinateTransformation != null)
            {
                userPolygon = (Polygon) GeometryTransform.TransformPolygon(userPolygon, discretizationLayer.CoordinateTransformation.MathTransform.Inverse());
            }

            return userPolygon;
        }


        private Polygon ConvertToPolygon(ILineString lineString)
        {
            var coordinates = lineString.Coordinates.Distinct().ToArray();
            if (coordinates.Length < 3)
            {
                log.Error("Polygon should have at least three vertices.");
                return null;
            }

            var closedUserCoordinates = new CoordinateList(coordinates);
            closedUserCoordinates.CloseRing();

            // todo: add check if polygon intersects itself
            return new Polygon(new LinearRing(closedUserCoordinates.ToCoordinateArray()));
        }

        private static T DoWithoutNodingValidator<T>(Func<T> function)
        {
            // This noding validator has a dramatic effect on the performance. Disable it here temporarily. 
            var oldNodingValidatorDisabled = OverlayOp.NodingValidatorDisabled;
            OverlayOp.NodingValidatorDisabled = true;

            var result = function();

            // Set the noding validator to the value it had before. 
            OverlayOp.NodingValidatorDisabled = oldNodingValidatorDisabled;

            return result;
        }

        private VectorLayer SelectPolygonLayer
        {
            get
            {
                if (cachedPolygonLayer != null) return cachedPolygonLayer;
                var pen = new Pen(Color.Tomato, 2);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                cachedPolygonLayer = new VectorLayer(SelectPolygonLayerName)
                {
                    DataSource = new FeatureCollection(new List<IFeature>(), typeof(Feature)),
                    Visible = true,
                    Style = new VectorStyle
                    {
                        Fill = new SolidBrush(Color.Tomato),
                        Symbol = null,
                        Line = pen,
                        Outline = pen
                    },
                    ShowInTreeView = false,
                    NameIsReadOnly = true,
                    CanBeRemovedByUser = false
                };
                return cachedPolygonLayer;
            }
        }

    }
}

