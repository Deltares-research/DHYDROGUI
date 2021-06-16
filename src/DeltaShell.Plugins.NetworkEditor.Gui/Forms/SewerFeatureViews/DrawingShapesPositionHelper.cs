using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public static class DrawingShapesPositionHelper
    {
        public static void PositionShapes(this ICollection<IDrawingShape> shapes, double maxY)
        {
            if (shapes == null || !shapes.Any()) return;

            // Position side by side shapes
            var sideBySideShapes = shapes.Where(s => s is CompartmentShape || s is InternalConnectionShape).ToList();

            PositionShapesSideBySide(sideBySideShapes, maxY);

            // Pipe shapes
            PositionPipeShapes(shapes.OfType<PipeShape>(), maxY);
        }

        private static void PositionShapesSideBySide(IList<IDrawingShape> shapes, double maxY)
        {
            DistributeShapesHorizontal(shapes);

            // Set top levels of shapes
            SetTopOfShapes(shapes, maxY);
        }

        private static void SetTopOfShape(IDrawingShape shape, double maxY)
        {
            if (shape == null) return;
            shape.TopOffset = maxY - shape.TopLevel;
        }

        private static void SetTopOfShapes(IEnumerable<IDrawingShape> shapes, double maxY)
        {
            foreach (var shape in shapes)
            {
                SetTopOfShape(shape, maxY);
            }
        }
        
        private static void DistributeShapesHorizontal(IEnumerable<IDrawingShape> shapes)
        {
            var left = 0.0;
            foreach (var shape in shapes)
            {
                shape.LeftOffset = left;
                left += shape.Width;
            }
        }

        private static void PositionPipeShapes(IEnumerable<PipeShape> shapes, double maxY)
        {
            foreach (var shape in shapes)
            {
                SetTopOfShape(shape, maxY);
                SetShapeLeftOffsetRelativeTo(shape, shape.ConnectedCompartmentShape, 0.5);
            }
        }

        /// <summary>
        /// Sets a shape horizontal position relative to another shape. 
        /// </summary>
        /// <param name="shape">Shape to correct</param>
        /// <param name="referenceShape">Factor to determine the offset, 0) shapes collide at the left edge, 1) at right edge, 0.5) in the middle</param>
        /// <param name="offsetFactor">Offset factor</param>
        private static void SetShapeLeftOffsetRelativeTo(IDrawingShape shape, IDrawingShape referenceShape, double offsetFactor)
        {
            if (referenceShape == null) return;

            var leftOffset = referenceShape.LeftOffset + referenceShape.Width * offsetFactor - shape.Width * 0.5;
            shape.LeftOffset = leftOffset;
        }
    }
}