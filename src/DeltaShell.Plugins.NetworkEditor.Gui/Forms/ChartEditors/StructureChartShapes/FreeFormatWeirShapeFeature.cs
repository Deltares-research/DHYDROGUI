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
                                                   maxZValue,
                                                   weir.OffsetY + geometry.Coordinates[geometry.Coordinates.Length - 1].X,
                                                   minZValue);
            ShapeFeatures.Add(WaterShape);
        }

        //public Chart Chart { get; set; }
//        private Chart chart;
//        public override Chart Chart
//        {
//            get { return chart; }
//            set
//            {
//                if (value == null)
//                {
//                    // when chart is disposed by teechart al references to chart in tools are set to null
////                    CrestShape.ValuesChanged -= CrestShape_ValuesChanged;
//                }
//                else
//                {
//                    if (chart == null)
//                    {
////                        CrestShape.ValuesChanged += CrestShape_ValuesChanged;
//                    }
//                }
//                chart = value;
//            }
//        }

        public override bool Selected
        {
            get
            {
                return base.Selected;
            }
            set
            {
                PolygonShapeFeature.Selected = value;
                base.Selected = value;
            }
        }

        public PolygonShapeFeature PolygonShapeFeature { get; set; }
        public RectangleShapeFeature WaterShape { get; set; }

        public IWeir Weir { get; set; }
        public IList<Coordinate> CrestShape { get; set; }

        public VectorStyle WaterStyle
        {
            set
            {
                var transparentStyle = (VectorStyle) value.Clone();
                transparentStyle.Fill = Brushes.Transparent;
                WaterShape.NormalStyle = transparentStyle;
                WaterShape.DisabledStyle = transparentStyle;
                WaterShape.SelectedStyle = value;
            }
        }

        //public void ChangeValue(double oldValue, double newValue, double value)
        //{
        //    CrestShape.ValuesChanged -= CrestShape_ValuesChanged;
        //    if (oldValue != newValue)
        //    {
        //        // temporary hack check for existence of argument value
        //        if (!CrestShape.Arguments[0].Values.Contains(oldValue))
        //        {
        //            log.Warn(string.Format("Error: y value {0} not found in crestshape.", oldValue));
        //        }
        //        else
        //        {
        //            CrestShape.RemoveValues(new VariableValueFilter<double>(CrestShape.Arguments[0], new[] { oldValue }));
        //        }
        //    }
        //    CrestShape[newValue] = value;
        //    CrestShape.ValuesChanged += CrestShape_ValuesChanged;
        //}

        public void ChangeValue(int index, double xValue, double yValue)
        {
            CrestShape[index].X = xValue;
            CrestShape[index].Y = yValue;
        }

        //void CrestShape_ValuesChanged(object sender, FunctionValuesChangedEventArgs e)
        //{
        //    UpdateGeometry();
        //    Invalidate();
        //}

        public void UpdateGeometry()
        {
            if (CrestShape.Count == 0)
            {
                return;
            }

            double offsetY = Weir.OffsetY;

            var vertices = new List<Coordinate>
            {
                new Coordinate(offsetY + CrestShape[CrestShape.Count - 1].X, MinYValue), // right bottom
                new Coordinate(offsetY + CrestShape[0].X, MinYValue)                     // left bottom
            };
            for (var i = 0; i < CrestShape.Count; i++)
            {
                vertices.Add(new Coordinate(offsetY + CrestShape[i].X, CrestShape[i].Y)); // top line
            }

            vertices.Add(new Coordinate(offsetY + CrestShape[CrestShape.Count - 1].X, MinYValue)); // close polygon
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
            var coordinates = new List<Coordinate>();

            int count = PolygonShapeFeature.Geometry.Coordinates.Length - 3;
            for (var i = 0; i < count; i++)
            {
                coordinates.Add(new Coordinate(PolygonShapeFeature.Geometry.Coordinates[i + 2].X,
                                               PolygonShapeFeature.Geometry.Coordinates[i + 2].Y));
            }

            return coordinates;
        }

        public override IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode)
        {
            return new FreeFormatWeirEditor(this, new ChartCoordinateService(PolygonShapeFeature.Chart), shapeEditMode);
        }

        private double MinYValue { get; set; }
    }
}