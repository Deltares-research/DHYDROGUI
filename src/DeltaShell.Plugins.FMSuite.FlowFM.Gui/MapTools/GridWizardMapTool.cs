using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
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
    public class GridWizardMapTool : MapTool
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GridWizardMapTool));

        private const string BoundingPolygonLayerName = "temp_layer";
        private NewLineTool boundingPolygonTool;
        private Cursor boundingPolygonCursor;
        private VectorLayer cachedPolygonLayer;
        private NetworkCoverageLocationLayer flowDiscretizationLayer;

        public GridWizardMapTool()
        {
            Name = FlowFMMapViewDecorator.GridWizardToolName;
            Func<ILayer, bool> boundingPolygonLayerFilter = l => l.Equals(BoundingPolygonLayer);
            boundingPolygonCursor = MapCursors.CreateArrowOverlayCuror(Resources.guide);
            boundingPolygonTool = new NewLineTool(boundingPolygonLayerFilter, "bounding_polygon_tool")
            {
                Cursor = boundingPolygonCursor
            };
            InDrawingStage = false;
        }

        // This tool has 2 stages: first user draws a bounding polygon, during which InDrawingStage == true.
        // After drawing was finished by user (double-click) the function ExecuteWizard is called.
        private bool InDrawingStage { get; set; }

        public override bool Enabled
        {
            get
            {
                // Check whether flow1d discretization can be found. 
                flowDiscretizationLayer = Map.GetAllLayers(false).OfType<NetworkCoverageLocationLayer>()
                    .FirstOrDefault(l => l.Coverage is IDiscretization);
                if (flowDiscretizationLayer == null)
                {
                    return false;
                }
                var grid = flowDiscretizationLayer.Coverage as IDiscretization;
                if (grid == null)
                {
                    return false;
                }

                return (GetEmbankments().Any() && grid.Locations.GetValues().Any());
            }
        }

        public override Cursor Cursor
        {
            get { return boundingPolygonCursor; }
        }

        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            if (!InDrawingStage)
            {
                InDrawingStage = true;
                EnterBoundingBoxDrawingMode();
        }
            boundingPolygonTool.OnMouseDown(worldPosition, e);
        }

        public override void OnMouseMove(Coordinate worldPosition, MouseEventArgs e)
        {
            if (InDrawingStage)
            {
                boundingPolygonTool.OnMouseMove(worldPosition, e);
            }
        }

        public override void OnMouseUp(Coordinate worldPosition, MouseEventArgs e)
            {
            if (InDrawingStage)
            {
                boundingPolygonTool.OnMouseUp(worldPosition, e);
            }
        }

        public override void OnMouseDoubleClick(object sender, MouseEventArgs e)
            {
            if (InDrawingStage)
            {
                InDrawingStage = false;
                boundingPolygonTool.OnMouseDoubleClick(sender, e);
                ExitBoundingBoxDrawingMode();
                ExecuteWizard();
            }
        }

        public override void Render(Graphics graphics)
            {
            if (InDrawingStage)
            {
                boundingPolygonTool.Render(graphics);
            }
        }

        private void Cleanup()
            {
            // remove temporary layer
            Map.Layers.Remove(BoundingPolygonLayer);
            cachedPolygonLayer = null;
            Map.ZoomToExtents();
            }

        private void ExecuteWizard()
        {
            var rgfgridPolygons = GetRgfGridPolygons();
            if (!rgfgridPolygons.Any())
            {
                Cleanup();
                return;
            }

            // Check whether FM group layer can be found. 
            var fmLayer = Map.GetAllLayers(true).OfType<ModelGroupLayer>().FirstOrDefault(l => l.Model is WaterFlowFMModel);
            if (fmLayer == null)
            {
                log.Error("Can not find the FM layer to create the grid on.");
                Cleanup();
                return;
            }

            var fmModel = (WaterFlowFMModel)fmLayer.Model;

            RgfGridEditor.OpenGrid(fmModel.NetFilePath, fmModel.Grid == null || fmModel.Grid.IsEmpty, rgfgridPolygons, "polygon.pol");
            try
            {
                fmModel.ReloadGrid(false);
            }
            catch (Exception e)
            {
                log.Warn("Error or warning while creating grid: " + e.Message);
            }
            finally
            {
                Cleanup();
            }
        }

        private void EnterBoundingBoxDrawingMode()
        {
            Map.Layers.Add(BoundingPolygonLayer);
            MapControl.Tools.Add(boundingPolygonTool);
            boundingPolygonTool.MapControl = MapControl;
        }

        private void ExitBoundingBoxDrawingMode()
        {
            MapControl.Tools.Remove(boundingPolygonTool);
        }

        private IEnumerable<IPolygon> GetRgfGridPolygons()
        {
            var discretization = flowDiscretizationLayer != null
                ? flowDiscretizationLayer.Coverage as IDiscretization
                : null;

            if (discretization == null)
            {
                log.Error("Missing discretization of 1D water flow model.");
                return Enumerable.Empty<IPolygon>();
            }

            // Check whether polygon is okay
            var feature = BoundingPolygonLayer.DataSource.Features[0] as Feature;
            var lineString = feature != null ? feature.Geometry as ILineString : null;

            if (lineString == null)
            {
                log.Error("Polygon for grid creation must be an ILineString.");
                return Enumerable.Empty<IPolygon>();
            }

            // Construct the userPolygon
            var userPolygon = ConvertToPolygon(lineString);
            if (userPolygon == null)
            {
                return Enumerable.Empty<IPolygon>();
            }

            var embankmentLayer = GetEmbankmentLayer();
            if (embankmentLayer != null && embankmentLayer.CoordinateTransformation != null)
            {
                userPolygon = (Polygon) GeometryTransform.TransformPolygon(userPolygon, embankmentLayer.CoordinateTransformation.MathTransform.Inverse());
            }

            var gridWizard = new GridWizard();
            if (gridWizard.ShowDialog() != DialogResult.OK)
            {
                return Enumerable.Empty<IPolygon>();
            }

            // The where clause is used for optimisation: only embankments whose envelope intersects with the polygon are considered. 
            var embankments =
                GetEmbankments()
                    .Where(b => b.Geometry.Intersects(userPolygon)).Select(f => (Feature2D) f.Clone())
                    .ToList();
            var polygons = DoWithoutNodingValidator(() => GridWizardMapToolHelper.ComputePolygons(discretization, embankments, userPolygon,
                                gridWizard.SupportPointDistance, gridWizard.MinimumSupportPointDistance));
            return polygons ?? Enumerable.Empty<IPolygon>();
        }

        private static Polygon ConvertToPolygon(ILineString lineString)
            {
            var coordinates = lineString.Coordinates.Distinct().ToArray();
            if (coordinates.Length < 3)
            {
                log.Error("Polygon should have at least three vertices.");
                return null;
            }

            var closedUserCoordinates = new CoordinateList(coordinates);
            closedUserCoordinates.CloseRing();
            
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

        private ILayer GetEmbankmentLayer()
        {
            return Layers.FirstOrDefault(l => l.DataSource != null && l.DataSource.FeatureType == typeof(Embankment) && l.Name == "Embankments");
        }

        private IEnumerable<Embankment> GetEmbankments()
        {
            IList<Embankment> embankments = null;
            var embankmentLayer = GetEmbankmentLayer();
            if (embankmentLayer != null)
            {
                embankments = embankmentLayer.DataSource.Features as IList<Embankment>;
            }
            return embankments ?? new List<Embankment>();
        }

        private VectorLayer BoundingPolygonLayer
        {
            get
            {
                if (cachedPolygonLayer != null) return cachedPolygonLayer;
                cachedPolygonLayer = new VectorLayer(BoundingPolygonLayerName)
                {
                    DataSource = new FeatureCollection(new List<IFeature>(), typeof (Feature)),
                    Visible = true,
                    Style = new VectorStyle
                    {
                        Fill = new SolidBrush(Color.Tomato),
                        Symbol = null,
                        Line = new Pen(Color.Tomato, 2),
                        Outline = new Pen(Color.FromArgb(50, Color.Tomato), 2)
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