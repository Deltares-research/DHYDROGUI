using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Selector for selecting a data template for the Settings view.
    /// </summary>
    /// <seealso cref="DataTemplateSelector"/>
    public class SettingsTemplateSelector : DataTemplateSelector
    {
        private const string tabContentTemplateKey = "TabContentTemplate";
        private const string tabCustomContentTemplateKey = "TabCustomContentTemplate";
        private const string subCategoryTemplateKey = "SubCategoryTemplate";
        private const string subCategoryCustomTemplateKey = "SubCategoryCustomTemplate";
        private const string textBoxTemplateKey = "TextBoxTemplate";
        private const string dateTemplateKey = "DateTemplate";
        private const string dateOnlyTemplateKey = "DateOnlyTemplate";
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
            {typeof(DateOnly),dateOnlyTemplateKey},
            {typeof(bool), checkboxTemplateKey},
            {typeof(TimeSpan), timeSpanTemplateKey},
            {typeof(IList<double>), listTemplateKey},
            {typeof(Enum), comboBoxTemplateKey}
        };

        /// <summary>
        /// When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate"/> based on custom logic.
        /// </summary>
        /// <param name="item"> The data object for which to select the template. </param>
        /// <param name="container"> The data-bound object. </param>
        /// <returns>
        /// Returns a <see cref="T:System.Windows.DataTemplate"/> or null. The default value is null.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var fe = (FrameworkElement) container;
            switch (item)
            {
                case WpfGuiCategory category:
                    return GetTemplateForCategory(category, fe);
                case WpfGuiSubCategory subCategory:
                    return GetTemplateForSubCategory(subCategory, fe);
                case WpfGuiProperty property:
                    return GetTemplateForProperty(item, container, property, fe);
                default:
                    return base.SelectTemplate(item, container);
            }
        }

        private DataTemplate GetTemplateForProperty(object item, DependencyObject container, WpfGuiProperty property,
                                                    FrameworkElement fe)
        {
            Type type = property.ValueType;
            if (type == null)
            {
                return base.SelectTemplate(item, container);
            }

            if (type == typeof(DateTime))
            {
                string format = property.UnitSymbol.Trim('[', ']').ToLower();
                if (format == DateTimeFormats.Date)
                {
                    return fe.FindResource(dateTemplateKey) as DataTemplate;
                }
            }

            if (type == typeof(DateOnly))
            {
                return fe.FindResource(dateOnlyTemplateKey) as DataTemplate;
            }

            if (type.BaseType == typeof(Enum))
            {
                return fe.FindResource(comboBoxTemplateKey) as DataTemplate;
            }

            return defaultTemplates.TryGetValue(type, out string templateKey)
                       ? fe.FindResource(templateKey) as DataTemplate
                       : base.SelectTemplate(item, container);
        }

        private static DataTemplate GetTemplateForSubCategory(WpfGuiSubCategory subCategory, FrameworkElement fe)
        {
            return !subCategory.HasCustomControl
                       ? fe.FindResource(subCategoryTemplateKey) as DataTemplate
                       : fe.FindResource(subCategoryCustomTemplateKey) as DataTemplate;
        }

        private static DataTemplate GetTemplateForCategory(WpfGuiCategory category, FrameworkElement fe)
        {
            return !category.HasCustomControl
                       ? fe.FindResource(tabContentTemplateKey) as DataTemplate
                       : fe.FindResource(tabCustomContentTemplateKey) as DataTemplate;
        }
    }
}