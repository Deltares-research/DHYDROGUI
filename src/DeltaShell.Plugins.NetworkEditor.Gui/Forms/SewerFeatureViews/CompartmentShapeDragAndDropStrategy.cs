using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class CompartmentShapeDragAndDropStrategy : DrawingShapeDragAndDropStrategy, IDragAndDropStrategy 
    {
        public CompartmentShapeDragAndDropStrategy(ObservableCollection<IDrawingShape> shapes) : base( shapes )
        {
        }

        public void DragStart(Canvas canvas, ContentPresenter contentPresenter)
        {
            SetDragContent(canvas,contentPresenter);
        }

        public bool FindNewPosition(double horizontalOffset)
        {
            var movedShapeMiddle = horizontalOffset + InitialShapeCenter;

            var cps = GetContentPresentersByContentType<CompartmentShape>().Concat(GetContentPresentersByContentType<InternalConnectionShape>());
            var tuple = cps.Select(cp => new KeyValuePair<ContentPresenter, double>(cp, GetElementMiddle(cp))).ToList();
            var closestLeft = tuple.Where(t => t.Value <= movedShapeMiddle).OrderBy(t => Math.Abs(t.Value - movedShapeMiddle)).FirstOrDefault().Key;
            // Find new index when adding after closest left
            if (closestLeft?.Content is IDrawingShape closestLeftShape)
            {
                if (closestLeftShape == DraggedShape) return false;
                var indexOfClosestLeft = Shapes.IndexOf(closestLeftShape);
                bool movingRight = OldIndex < indexOfClosestLeft; 
                NewIndex =  movingRight ? indexOfClosestLeft : indexOfClosestLeft + 1;

                return true;
            }

            // Find new index when adding just before closest right
            var closestRight = tuple.Where(t => t.Value > movedShapeMiddle).OrderBy(t => Math.Abs(t.Value - movedShapeMiddle)).FirstOrDefault().Key;
            if (closestRight?.Content is IDrawingShape closestRightShape)
            {
                NewIndex = Shapes.IndexOf(closestRightShape);
                return true;
            }
            return false;
        }

        public bool Validate()
        {
            return Shapes.IndexInRange(OldIndex) && Shapes.IndexInRange(NewIndex);
        }

        public void Reposition()
        {
            MoveDrawingShapeAndClearDragContent();
        }
    }
}