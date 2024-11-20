using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public abstract class DrawingShapeDragAndDropStrategy
    {
        private readonly ObservableCollection<IDrawingShape> shapes;
        private int oldIndex = -1;
        protected int NewIndex = -1;

        private ContentPresenter contentPresenter;
        private double initialShapeCenter;

        private Canvas canvas;
        protected double InitialShapeCenter => initialShapeCenter;
        protected int OldIndex => oldIndex;
        
        protected IDrawingShape DraggedShape => contentPresenter.Content as IDrawingShape;
        
        protected ObservableCollection<IDrawingShape> Shapes => shapes;
        
        protected DrawingShapeDragAndDropStrategy(ObservableCollection<IDrawingShape> shapes)
        {
            this.shapes = shapes;
        }

        protected void SetDragContent(Canvas drawingCanvas, ContentPresenter content)
        {
            canvas = drawingCanvas;
            contentPresenter = content;
            if (contentPresenter != null)
            {
                initialShapeCenter = GetElementMiddle(contentPresenter);
                oldIndex = Shapes.IndexOf(DraggedShape);
            }
            else
            {
                initialShapeCenter = 0;
                oldIndex = -1;
            }
            NewIndex = -1;
        }


        protected void MoveDrawingShapeAndClearDragContent()
        {
            if (shapes.IndexInRange(OldIndex) && shapes.IndexInRange(NewIndex))
                shapes.Move(OldIndex, NewIndex);
            SetDragContent(null,null);
        }

        protected IEnumerable<ContentPresenter> GetContentPresentersByContentType<T>() where T : IDrawingShape
        {
            return canvas.Children.OfType<ContentPresenter>().Where(cp => cp.Content is T);
        }

        protected static double GetElementMiddle(FrameworkElement element)
        {
            return Canvas.GetLeft(element) + 0.5 * element.ActualWidth;
        }
    }
}