using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
            var fe = (FrameworkElement)container;
            /*CHECK FIRST FOR CUSTOM CONTROLS*/
            if (item is WpfGuiCategory)
            {
                var category = item as WpfGuiCategory;
                if(!category.HasCustomControl)
                    return fe.FindResource("TabContentTemplate") as DataTemplate;
                return fe.FindResource("TabCustomContentTemplate") as DataTemplate;
            }
            
            if (item is WpfGuiSubCategory)
            {
                var subCategory = item as WpfGuiSubCategory;
                if (!subCategory.HasCustomControl)
                    return fe.FindResource("SubCategoryTemplate") as DataTemplate;
                return fe.FindResource("SubCategoryCustomTemplate") as DataTemplate;
            }

            if (!(item is WpfGuiProperty)) return base.SelectTemplate(item, container);
            
            var property = item as WpfGuiProperty;

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