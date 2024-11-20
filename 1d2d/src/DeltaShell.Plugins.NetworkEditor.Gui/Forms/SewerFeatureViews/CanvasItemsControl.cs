using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class CanvasItemsControl : ItemsControl
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var leftBinding = new Binding { Path = new PropertyPath(nameof(IDrawingShape.LeftOffsetPix)), Mode = BindingMode.TwoWay };
            var topBinding = new Binding { Path = new PropertyPath(nameof(IDrawingShape.TopOffsetPix)), Mode = BindingMode.TwoWay };

            var contentControl = element as FrameworkElement;
            contentControl?.SetBinding(Canvas.LeftProperty, leftBinding);
            contentControl?.SetBinding(Canvas.TopProperty, topBinding);

            base.PrepareContainerForItemOverride(element, item);
        }
    }
}