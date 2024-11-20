using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class PipeShapeDragAndDropStrategy : DrawingShapeDragAndDropStrategy, IDragAndDropStrategy
    {
        private PipeShape pipeShape;
        private CompartmentShape newCompartmentShape;

        public PipeShapeDragAndDropStrategy(ObservableCollection<IDrawingShape> shapes)
            : base(shapes)
        {
        }

        public void DragStart(Canvas canvas, ContentPresenter contentPresenter)
        {
            SetDragContent(canvas,contentPresenter);
            pipeShape = DraggedShape as PipeShape;
        }
        
        /// <summary>
        /// The dropped pump becomes the last shape causing it to be drawn over anything else.
        /// </summary>
        /// <param name="horizontalOffset">the horizontal dragging distance</param>
        /// <returns>true iff a compartment exists at the drag position</returns>
        /// <remarks>This crude approach at repositioning the dropped pump is acceptable because
        /// <see cref="DrawingShapesCreationHelper.OrderShapes"/> arranges all pumps at the back of the
        /// <see cref="ManholeVisualisationViewModel.Shapes"/> list</remarks>
        public bool FindNewPosition(double horizontalOffset)
        {
            newCompartmentShape = FindCompartmentShapeClosestToDragPosition(horizontalOffset);
            if (newCompartmentShape == null) return false;

            NewIndex = Shapes.Count - 1;
            return true;
        }

        public bool Validate()
        {
            return newCompartmentShape != null;
        }

        public void Reposition()
        {
            var newCompartment = newCompartmentShape?.Compartment;

            if (pipeShape?.Pipe == null || newCompartment == null)
            {
                SetDragContent(null,null);
                return;
            }

            pipeShape.ConnectedCompartmentShape = newCompartmentShape;
            var manholeIsPipeSource = (IManhole)pipeShape.Pipe.Source == newCompartment.ParentManhole;
            if (manholeIsPipeSource)
            {
                pipeShape.Pipe.SourceCompartment = newCompartment;
            }
            else
            {
                pipeShape.Pipe.TargetCompartment = newCompartment;
            }

            MoveDrawingShapeAndClearDragContent();

            pipeShape = null;
            newCompartmentShape = null;
        }

        private CompartmentShape FindCompartmentShapeClosestToDragPosition(double horizontalOffset)
        {
            var compartmentShapes = GetContentPresentersByContentType<CompartmentShape>();
            var newShapeCenter = horizontalOffset + InitialShapeCenter;
            var tuple = compartmentShapes.Select(cp => new KeyValuePair<ContentPresenter, double>(cp, GetElementMiddle(cp))).ToList();
            var closestCompartment = tuple.OrderBy(t => Math.Abs(t.Value - newShapeCenter)).FirstOrDefault().Key;

            return closestCompartment.Content as CompartmentShape;
        }
    }
}