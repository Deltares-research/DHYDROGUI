using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public static class DrawingShapesDimensionsHelper
    {
        public static void GetDimensions(this IList<IDrawingShape> shapes, out double minX, out double maxX, out double minY, out double maxY)
        {
            HeightDimensionsFromShapes(shapes, out minY, out maxY);
            WidthDimensionsFromShapes(shapes, out minX, out maxX);
        }

        private static void HeightDimensionsFromShapes(IEnumerable<IDrawingShape> shapes, out double minY, out double maxY)
        {
            var drawingShapes = shapes.ToList();
            var max = drawingShapes.Max(s => s.TopLevel);
            var min = drawingShapes.Min(s => s.BottomLevel);
            
            minY = min;
            maxY = max;
        }

        private static void WidthDimensionsFromShapes(IEnumerable<IDrawingShape> shapes, out double minX, out double maxX)
        {
            var drawingShapes = shapes.ToList();
            var pipeShapes = drawingShapes.OfType<PipeShape>();
            var other = drawingShapes.Except(pipeShapes);

            var width = other.Sum(s => s.Width);

            minX = 0;
            maxX = width;
        }
    }
}