using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes
{
    public class CircleShapeFeature : ShapeFeatureBase
    {
        public CircleShapeFeature(IChart chart, Coordinate center, double xRadius, double yRadius)
            : base(chart)
        {
            Center = center;
            XRadius = xRadius;
            YRadius = yRadius;
        }

        public Coordinate Center { get; set; }
        public double XRadius { get; set; }
        public double YRadius { get; set; }

        public override Rectangle GetBounds()
        {
            var rectangle = new Rectangle
            {
                X = ChartCoordinateService.ToDeviceX(Chart, Center.X - (XRadius / 2)),
                Y = ChartCoordinateService.ToDeviceY(Chart, Center.Y + (YRadius / 2)),
                Width = ChartCoordinateService.ToDeviceWidth(Chart, XRadius),
                Height = ChartCoordinateService.ToDeviceHeight(Chart, YRadius)
            };
            return rectangle;
        }

        public override void Paint(IChartDrawingContext chartDrawingContext)
        {
            var g = (ChartGraphics) chartDrawingContext.Graphics;
            g.Ellipse(GetBounds());
        }

        public override object Clone()
        {
            return new CircleShapeFeature(Chart, Center, XRadius, YRadius);
        }

        public override IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode)
        {
            return new CircleShapeEditor(this, new ChartCoordinateService(Chart), shapeEditMode);
        }
    }
}