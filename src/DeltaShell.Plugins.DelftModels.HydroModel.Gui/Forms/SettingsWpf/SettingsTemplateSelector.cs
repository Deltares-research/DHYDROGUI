using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    public class SettingsTemplateSelector : DataTemplateSelector
    {
        private const string tabContentTemplateKey = "TabContentTemplate";
        private const string tabCustomContentTemplateKey = "TabCustomContentTemplate";
        private const string subCategoryTemplateKey = "SubCategoryTemplate";
        private const string subCategoryCustomTemplateKey = "SubCategoryCustomTemplate";
        private const string textBoxTemplateKey = "TextBoxTemplate";
        private const string dateTemplateKey = "DateTemplate";
        private const string dateTimeTemplateKey = "DateTimeTemplate";
        private const string checkboxTemplateKey = "CheckboxTemplate";
        private const string timeSpanTemplateKey = "TimeSpanTemplate";
        private const string listTemplateKey = "ListTemplate";
        private const string comboBoxTemplateKey = "ComboBoxTemplate";

        private readonly IDictionary<Type, string> defaultTemplates = new Dictionary<Type, string>()
        {
            {typeof(string), textBoxTemplateKey},
            {typeof(double), textBoxTemplateKey},
            {typeof(int), textBoxTemplateKey},
            {typeof(DateTime), dateTimeTemplateKey},
            {typeof(bool), checkboxTemplateKey},
            {typeof(TimeSpan), timeSpanTemplateKey},
            {typeof(IList<double>), listTemplateKey},
            {typeof(Enum), comboBoxTemplateKey},
        };

        /// <summary>
        /// When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate" /> based on custom logic.
        /// </summary>
        /// <param name="item"> The data object for which to select the template. </param>
        /// <param name="container"> The data-bound object. </param>
        /// <returns>
        /// Returns a <see cref="T:System.Windows.DataTemplate" /> or null. The default value is null.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var fe = (FrameworkElement) container;
            /*CHECK FIRST FOR CUSTOM CONTROLS*/
            if (item is WpfGuiCategory category)
            {
                return GetTemplateForCategory(category, fe);
            }

            if (item is WpfGuiSubCategory subCategory)
            {
                return GetTemplateForSubCategory(subCategory, fe);
            }

            if (item is WpfGuiProperty property)
            {
                return GetTemplateForProperty(item, container, property, fe);
            }

            /* There were not any custom controls, so go ahead with the regular templates*/
            /*Todo: make a switch or create a dictionary for this. */
            return base.SelectTemplate(item, container);
        }

        private DataTemplate GetTemplateForProperty(object item, DependencyObject container, WpfGuiProperty property,
                                                    FrameworkElement fe)
        {
            Type type = property.ValueType;
            if (type == typeof(DateTime))
            {
                string format = property.UnitSymbol.Trim('[', ']').ToLower();
                if (format == DateTimeFormats.Date)
                {
                    return fe.FindResource(dateTemplateKey) as DataTemplate;
                }
            }

            if (type?.BaseType == typeof(Enum))
            {
                return fe.FindResource(comboBoxTemplateKey) as DataTemplate;
            }

            return defaultTemplates.TryGetValue(type, out string templateKey)
                       ? fe.FindResource(templateKey) as DataTemplate
                       : base.SelectTemplate(item, container);
        }

        private static DataTemplate GetTemplateForSubCategory(WpfGuiSubCategory subCategory, FrameworkElement fe)
        {
            if (!subCategory.HasCustomControl)
            {
                return fe.FindResource(subCategoryTemplateKey) as DataTemplate;
            }

            return fe.FindResource(subCategoryCustomTemplateKey) as DataTemplate;
        }

        private static DataTemplate GetTemplateForCategory(WpfGuiCategory category, FrameworkElement fe)
        {
            if (!category.HasCustomControl)
            {
                return fe.FindResource(tabContentTemplateKey) as DataTemplate;
            }

            return fe.FindResource(tabCustomContentTemplateKey) as DataTemplate;
        }
    }
}