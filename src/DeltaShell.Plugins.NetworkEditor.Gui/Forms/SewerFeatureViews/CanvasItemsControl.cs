using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class CanvasItemsControl : ItemsControl
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var leftBinding = new Binding { Path = new PropertyPath(TypeUtils.GetMemberName<IDrawingShape>(s => s.LeftOffset)), Mode = BindingMode.TwoWay };
            var topBinding = new Binding { Path = new PropertyPath(TypeUtils.GetMemberName<IDrawingShape>(s => s.TopOffset)), Mode = BindingMode.TwoWay };

            var contentControl = element as FrameworkElement;
            contentControl?.SetBinding(Canvas.LeftProperty, leftBinding);
            contentControl?.SetBinding(Canvas.TopProperty, topBinding);

            base.PrepareContainerForItemOverride(element, item);
        }
    }
}