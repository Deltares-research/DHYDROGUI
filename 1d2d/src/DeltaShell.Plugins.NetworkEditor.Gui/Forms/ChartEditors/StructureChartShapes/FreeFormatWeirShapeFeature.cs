using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapeEditors;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class FreeFormatWeirShapeFeature : CompositeShapeFeature
    {
        public PolygonShapeFeature PolygonShapeFeature { get; private set; }
        public RectangleShapeFeature WaterShape { get; }

        public IWeir Weir { get; }
        public IList<Coordinate> CrestShape { get; set; }
        private double MinYValue { get; }

        public VectorStyle WaterStyle
        {
            set
            {
                VectorStyle transparentStyle = (VectorStyle)value.Clone();
                transparentStyle.Fill = Brushes.Transparent;
                WaterShape.NormalStyle = transparentStyle;
                WaterShape.DisabledStyle = transparentStyle;
                WaterShape.SelectedStyle = value;
            }
        }

        public FreeFormatWeirShapeFeature(IChart chart, IWeir weir, IGeometry geometry, double minZValue, double maxZValue)
            : base(chart)
        {
            CrestShape = geometry.Coordinates;
            Chart = chart;
            Weir = weir;
            MinYValue = minZValue;
            UpdateGeometry();
            WaterShape = new RectangleShapeFeature(Chart,
                                                      weir.OffsetY + geometry.Coordinates[0].X,
                                                      weir.CrestLevel + maxZValue,
                                                      weir.OffsetY + geometry.Coordinates[geometry.Coordinates.Length - 1].X,
                                                      minZValue);
            ShapeFeatures.Add(WaterShape);
        }

        public void ChangeValue(int index, double xValue, double yValue)
        {
            CrestShape[index].X = xValue;
            CrestShape[index].Y = yValue;
        }

        public void UpdateGeometry()
        {
            if (CrestShape.Count == 0)
            {
                return;
            }
            
            IEnumerable<Coordinate> coordinates = GetChartCoordinates();
            IPolygon polygon = CreatePolygon(coordinates);

            if (null != PolygonShapeFeature)
            {
                ShapeFeatures.Remove(PolygonShapeFeature);
            }
            PolygonShapeFeature = new PolygonShapeFeature(Chart, polygon);
            ShapeFeatures.Add(PolygonShapeFeature);
        }
        
        private IEnumerable<Coordinate> GetChartCoordinates()
        {
            double offsetY = Weir.OffsetY;
            double crestLevel = Weir.CrestLevel;

            // bottom right
            var bottomRight = new Coordinate(offsetY + CrestShape[CrestShape.Count - 1].X, MinYValue);
            yield return bottomRight;
            
            // bottom left
            yield return new Coordinate(offsetY + CrestShape[0].X, MinYValue);

            // top line
            foreach (Coordinate c in CrestShape)
            {
                yield return new Coordinate(offsetY + c.X, crestLevel + c.Y);
            }

            // bottom right
            yield return bottomRight;
        }
        
        private static IPolygon CreatePolygon(IEnumerable<Coordinate> coordinates)
        {
            ILinearRing newLinearRing = new LinearRing(coordinates.ToArray());
            return new Polygon(newLinearRing, (ILinearRing[])null);
        }

        public IList<Coordinate> GetCoordinates()
        {
            List<Coordinate> coordinates = new List<Coordinate>();

            int count = PolygonShapeFeature.Geometry.Coordinates.Length - 3;
            for (int i = 0; i < count; i++)
            {
                coordinates.Add(new Coordinate(PolygonShapeFeature.Geometry.Coordinates[i + 2].X,
                                                                 PolygonShapeFeature.Geometry.Coordinates[i + 2].Y));
            }
            return coordinates;
        }

        public override bool Selected
        {
            get => base.Selected;
            set
            {
                PolygonShapeFeature.Selected = value;
                base.Selected = value;
            }
        }

        public override IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode)
        {
            return new FreeFormatWeirEditor(this, new ChartCoordinateService(PolygonShapeFeature.Chart), shapeEditMode);
        }

    }
}