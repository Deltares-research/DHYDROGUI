using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DelftTools.Controls.Wpf.Extensions;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    public class SettingsTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate" /> based on custom logic.
        /// </summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>
        /// Returns a <see cref="T:System.Windows.DataTemplate" /> or null. The default value is null.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(container is WpfSettingsView wpfSettingsView))
            {
                wpfSettingsView = container.TryFindParent<WpfSettingsView>();
            }

            var fe = wpfSettingsView?.MainGrid ?? container as FrameworkElement;
            if (fe == null) 
                return base.SelectTemplate(item, container);

            /*CHECK FIRST FOR CUSTOM CONTROLS*/
            if (item is WpfGuiCategory category)
            {
                if(!category.HasCustomControl)
                    return fe.FindResource("tabContentTemplate") as DataTemplate;
                return fe.FindResource("tabCustomContentTemplate") as DataTemplate;
            }
            
            if (item is WpfGuiSubCategory subCategory)
            {
                if (!subCategory.HasCustomControl)
                    return fe.FindResource("subCategoryTemplate") as DataTemplate;
                return fe.FindResource("subCategoryCustomTemplate") as DataTemplate;
            }

            if (!(item is WpfGuiProperty property)) 
                return base.SelectTemplate(item, container);
            
            if (property.HasCustomControl)
                return fe.FindResource("propertyCustomTemplate") as DataTemplate;

            /* There were not any custom controls, so go ahead with the regular templates*/
            /*Todo: make a switch or create a dictionary for this. */
            var type = property.ValueType;
            if (type == typeof(string)
                || type == typeof(double)
                || type == typeof(int))
                return fe.FindResource("TextBoxTemplate") as DataTemplate;

            if (type == typeof(DateTime))
                return fe.FindResource("DateTimeTemplate") as DataTemplate;

            if (type == typeof(bool))
                return fe.FindResource("CheckboxTemplate") as DataTemplate;

            if (type == typeof(TimeSpan))
            {
                return fe.FindResource("TimeSpanTemplate") as DataTemplate;
            }

            if (type == typeof(IList<double>))
            {
                return fe.FindResource("ListTemplate") as DataTemplate;
            }

            if (type == typeof(Enum)
                || type?.BaseType == typeof(Enum))
            {
                return fe.FindResource("ComboBoxTemplate") as DataTemplate;
            }
            
            return base.SelectTemplate(item, container);
        }
    }
}