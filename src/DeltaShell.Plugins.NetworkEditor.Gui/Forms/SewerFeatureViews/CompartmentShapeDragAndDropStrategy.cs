using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class CompartmentShapeDragAndDropStrategy : IDragAndDropStrategy
    {
        private readonly ObservableCollection<IDrawingShape> shapes;
        private int oldIndex;
        private int newIndex;

        public CompartmentShapeDragAndDropStrategy(ObservableCollection<IDrawingShape> shapes)
        {
            this.shapes = shapes;
        }

        public bool FindNewPosition(Canvas canvas, ContentPresenter contentPresenter, double leftOffset, double originalLeft)
        {
            var originalDrawingShape = contentPresenter.Content as IDrawingShape;
            
            var cps = GetContentPresentersByContentType<CompartmentShape>(canvas).Concat(GetContentPresentersByContentType<ConnectionShape>(canvas));
            var movedShapeMiddle = leftOffset + originalLeft + 0.5 * contentPresenter.ActualWidth;

            var tuple = cps.Select(cp => new KeyValuePair<ContentPresenter, double>(cp, GetElementMiddle(cp))).ToList();
            var closestLeft = tuple.Where(t => t.Value <= movedShapeMiddle).OrderBy(t => Math.Abs(t.Value - movedShapeMiddle)).FirstOrDefault().Key;
            // Find new index when adding after closest left
            if (closestLeft?.Content is IDrawingShape)
            {
                var leftItem = (IDrawingShape)closestLeft.Content;
                if (leftItem == originalDrawingShape) return false;
                var indexOfClosestLeft = shapes.IndexOf(leftItem);
                oldIndex = shapes.IndexOf(originalDrawingShape);
                newIndex = oldIndex < indexOfClosestLeft ? indexOfClosestLeft : indexOfClosestLeft + 1;

                return true;
            }

            // Find new index when adding just before closest right
            var closestRight = tuple.Where(t => t.Value > movedShapeMiddle).OrderBy(t => Math.Abs(t.Value - movedShapeMiddle)).FirstOrDefault().Key;
            if (!(closestRight?.Content is IDrawingShape)) return false;

            var closestRightShape = (IDrawingShape)closestRight.Content;
            var indexOfClosestRight = shapes.IndexOf(closestRightShape);

            oldIndex = shapes.IndexOf(originalDrawingShape);
            newIndex = indexOfClosestRight;
            return true;
        }

        public bool Validate()
        {
            return IndexInRange(shapes, oldIndex) && IndexInRange(shapes, newIndex);
        }

        public void Reposition()
        {
            shapes.Move(oldIndex, newIndex);

            oldIndex = 0;
            newIndex = 0;
        }

        protected virtual IEnumerable<ContentPresenter> GetContentPresentersByContentType<T>(Canvas canvas) where T : IDrawingShape
        {
            return canvas.Children.OfType<ContentPresenter>().Where(cp => cp.Content is T);
        }

        protected virtual double GetElementMiddle(FrameworkElement element)
        {
            var x = Canvas.GetLeft(element) + 0.5 * element.ActualWidth;
            return x;
        }

        private static bool IndexInRange(ICollection shapes, int index)
        {
            return index >= 0 && index < shapes.Count;
        }
    }
}