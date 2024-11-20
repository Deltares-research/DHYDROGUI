using System.Windows;
using System.Windows.Controls;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class SedimentPropertyDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (element == null || item == null)
            {
                return null;
            }

            var sedimentProperty = item as ISedimentProperty;
            if (sedimentProperty == null)
            {
                return null;
            }

            return element.FindResource(sedimentProperty.DataTemplateName) as DataTemplate;
        }
    }
}