using System;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using GeoAPI.Geometries;
using SharpMap.Converters.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes
{
    public class PolygonShapeFeature : ShapeFeatureBase
    {
        public PolygonShapeFeature(IChart chart, IPolygon polygon)
            : base(chart)
        {
            Geometry = (IGeometry) polygon.Clone();
        }

        public override bool Contains(double x, double y)
        {
            // todo: fix invalid geometry
            try
            {
                return Geometry.Contains(GeometryFactory.CreatePoint(x, y));
            }
            catch (Exception)
            {

                return false;
            }
        }

        public override bool Contains(int x, int y)
        {
            double worldX = ChartCoordinateService.ToWorldX(Chart, x);
            double worldY = ChartCoordinateService.ToWorldY(Chart, y);
            return Contains(worldX, worldY);
        }

        protected override void Paint(IChartDrawingContext chartDrawingContext)
        {
            var g = (ChartGraphics)chartDrawingContext.Graphics;
            Point[] devicePoint = new Point[Geometry.Coordinates.Length];
            for (int i = 0; i < devicePoint.Length; i++)
            {
                devicePoint[i].X = ChartCoordinateService.ToDeviceX(Chart, Geometry.Coordinates[i].X);
                devicePoint[i].Y = ChartCoordinateService.ToDeviceY(Chart, Geometry.Coordinates[i].Y);
            }
            g.Polygon(devicePoint);
        }

        public override object Clone()
        {
            return new PolygonShapeFeature(Chart, (IPolygon)Geometry);
        }

        public override IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode)
        {
            return new PolygonEditor(this, new ChartCoordinateService(Chart), shapeEditMode);
        }
    }
}