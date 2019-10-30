using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Collections.Generic;
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
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Styles;
using SharpMap.UI.Helpers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools
{
    public class GenerateLinksMapTool: Base1D2DLinksMapTool
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(GenerateLinksMapTool));

        private const string SelectPolygonLayerName = "temp_layer";
        private NewLineTool selectPolygonTool;
        private Cursor selectPolygonCursor;
        private VectorLayer cachedPolygonLayer;

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

        public override void Render(Graphics graphics)
        {
            if (InDrawingStage)
            {
                selectPolygonTool.Render(graphics);
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
            var linksFrom = new List<int>();
            var linksTo = new List<int>();
            var startIndex = 0;
            int linksCount = 0;

            try
            {
                if (!MapTool1D2DLinksHelper.Generate1D2DLinks(fmModel, selectedArea, startIndex, ref linksFrom, ref linksTo, ref linksCount, LinkType)) return;

                var created1D2DLinks = Creates1d2dLinks(linksCount, linksFrom, linksTo, fmModel.Grid, fmModel.NetworkDiscretization, LinkType, startIndex);
                created1D2DLinks = GetNew1D2DLinks(created1D2DLinks, fmModel.Links);
                fmModel.Links.AddRange(created1D2DLinks);
            }
            catch (Exception e)
            {
                log.DebugFormat("Unexpected exception thrown while generating 1D2D links: {0}", e.Message);
                log.ErrorFormat("1D2D Links were not generated between the grid and the network of WaterFlowFMModel {0}. Please make sure the grid has been saved and the network is correct.", Name);
            }
        }

        private IList<Link1D2D> GetNew1D2DLinks(IList<Link1D2D> created1D2DLinks, IEventedList<ILink1D2D> existingLinks)
        {
            var result = new List<Link1D2D>();
            foreach (var newLink in created1D2DLinks)
            {
                if (!existingLinks.Contains(newLink))
                {
                    result.Add(newLink);
                }
            }
            return result;
        }
        
        private IList<Link1D2D> Creates1d2dLinks(int linksCount, List<int> linksFromIndex, List<int> linksToIndex, UnstructuredGrid grid, IDiscretization networkDiscretization, LinkType linkType, int startIndex)
        {
            var lstNewLinks = new List<Link1D2D>();
            for (int i = 0; i < linksCount; i++)
            {
                //seems lists are swapt  
                var pointIndex = linksToIndex[i] - startIndex;
                var cellIndex = linksFromIndex[i] - startIndex;

                var fromCell = grid.Cells[cellIndex];
                var toNode = networkDiscretization.Locations.Values[pointIndex];
                var link = new Link1D2D(pointIndex, cellIndex)
                {
                    Geometry = new LineString(new[] { fromCell.Center, toNode.Geometry.Coordinate }),
                    TypeOfLink = linkType
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

