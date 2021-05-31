using System.Windows;
using System.Windows.Media;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class MoveAdorner : SimpleAdorner
    {
        private SolidColorBrush renderBrush = new SolidColorBrush(Colors.Pink);
        private Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);
        private Rect adornedElementRect;

        public MoveAdorner(UIElement adornedElement) : base(adornedElement)
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // Get a rectangle that represents the desired size of the rendered element
            // after the rendering pass.  This will be used to draw at the corners of the 
            // adorned element.
            adornedElementRect = new Rect(this.AdornedElement.RenderSize);

            VisualBrush brush = new VisualBrush(AdornedElement) { Opacity = 0.5 };
            drawingContext.DrawRectangle(brush, null, adornedElementRect);

            // Some arbitrary drawing implements.
            renderBrush.Opacity = 0.2;
            
            // Just draw a circle at each corner.
            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);
        }
    }
}