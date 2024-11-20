using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes
{
    public class RectangleShapeFeature : ShapeFeatureBase
    {
        public double Left { get; set; }
        public double Right { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Width { get { return Right - Left; } }
        public double Height { get { return Top - Bottom; } }

        public RectangleShapeFeature(IChart chart, double left, double top, double right, double bottom) : base(chart)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public override Rectangle GetBounds()
        {
            var rectangle = new Rectangle
                                {
                                    X = ChartCoordinateService.ToDeviceX(Chart, Left),
                                    Y = ChartCoordinateService.ToDeviceY(Chart, Top),
                                    Width = ChartCoordinateService.ToDeviceWidth(Chart, Right - Left),
                                    Height = ChartCoordinateService.ToDeviceHeight(Chart, Top - Bottom)
                                };
            return rectangle;
        }

        protected override void Paint(IChartDrawingContext chartDrawingContext)
        {
            var g = (ChartGraphics) chartDrawingContext.Graphics;
            var bounds = GetBounds();
            g.Rectangle(bounds);
        }

        public override object Clone()
        {
            return new RectangleShapeFeature(Chart, Left, Top, Right, Bottom);
        }

        public override IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode)
        {
            return new RectangleShapeEditor(this, new ChartCoordinateService(Chart), shapeEditMode);
        }

        public override IGeometry Geometry
        {
            get
            {
                var vertices = new List<Coordinate>
                                   {
                                       new Coordinate(Left, Bottom),
                                       new Coordinate(Right, Bottom),
                                       new Coordinate(Right, Top),
                                       new Coordinate(Left, Top)
                                   };
                vertices.Add((Coordinate)vertices[0].Clone());
                ILinearRing newLinearRing = new LinearRing(vertices.ToArray());
                return new Polygon(newLinearRing, (ILinearRing[])null);
            }
            set
            {
                base.Geometry = value;
            }
        }
    }
}