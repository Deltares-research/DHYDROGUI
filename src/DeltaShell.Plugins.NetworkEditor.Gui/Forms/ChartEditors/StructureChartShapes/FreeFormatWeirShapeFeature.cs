using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapeEditors;
using GeoAPI.Geometries;
using SharpMap.Converters.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    // todo use ShapeFeatureBase
    public class FreeFormatWeirShapeFeature : CompositeShapeFeature
    {
        public PolygonShapeFeature PolygonShapeFeature { get; set; }
        public RectangleShapeFeature WaterShape { get; set; }

        public IWeir Weir { get; set; }
        public IList<Coordinate> CrestShape { get; set; }
        private double MinYValue { get; set; }

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
                                                      weir.CrestLevel + minZValue);
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
            var offsetY = Weir.OffsetY;
            var crestLevel = Weir.CrestLevel;

            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(offsetY + CrestShape[CrestShape.Count - 1].X, MinYValue + crestLevel), // right bottom
                                   new Coordinate(offsetY + CrestShape[0].X, MinYValue + crestLevel)                     // left bottom
                               };
            for (int i = 0; i < CrestShape.Count; i++)
            {
                vertices.Add(new Coordinate(offsetY + CrestShape[i].X, crestLevel + CrestShape[i].Y)); // top line
            }
            vertices.Add(new Coordinate(offsetY + CrestShape[CrestShape.Count - 1].X, MinYValue + crestLevel)); // close polygon
            ILinearRing newLinearRing = GeometryFactory.CreateLinearRing(vertices.ToArray());
            IPolygon polygon = GeometryFactory.CreatePolygon(newLinearRing, null);
            if (null != PolygonShapeFeature)
            {
                ShapeFeatures.Remove(PolygonShapeFeature);
            }
            PolygonShapeFeature = new PolygonShapeFeature(Chart, polygon);
            ShapeFeatures.Add(PolygonShapeFeature);
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
            get { return base.Selected; }
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