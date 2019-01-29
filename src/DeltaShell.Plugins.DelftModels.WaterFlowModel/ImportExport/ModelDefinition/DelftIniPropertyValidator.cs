using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using Resources = DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties.Resources;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition {
    public static class DelftIniPropertyValidator
    {
        private static List<DelftIniProperty> properties;
        private static List<Tuple<string, bool, string>> defaultPropertyValues;
        private static IList<string> errors;
        public static string categoryHeader;

        public static void ValidateProperties(DelftIniCategory category, IList<string> errorMessages)
        {
            errors = errorMessages;
            categoryHeader = category.Name;
            properties = category.Properties.ToList();

            RestrictPropertyToCategory(category);
            GetDefaultValues();
            CheckIfRequired();
        }

        private static void GetDefaultValues()
        {
            var values = DelftIniPropertyValidation.LookupTable.
                         Where(t => t.Key.Equals(categoryHeader)).
                         Select(t => t.Value).ToList(); 

            defaultPropertyValues = new List<Tuple<string, bool, string>>();
            values.ForEach(v => v.ForEach(pv => defaultPropertyValues.Add(pv)));
        }

        private static void CheckIfRequired()
        {
            foreach (var property in properties)
            {
                var propertyIsRequired = defaultPropertyValues.Any(pv => pv.Item2 && pv.Item1 == property.Name);

                if (propertyIsRequired)
                {
                    CheckPropertyAvailability(property);
                }
            }
        }

        private static void RestrictPropertyToCategory(DelftIniCategory category)
        {
            properties.ForEach(property => property.Id = category.Name);
        }

        private static void CheckPropertyAvailability(DelftIniProperty property)
        {
            var propertyValueMatchesDefaultValue = defaultPropertyValues.Any(dpv => dpv.Item3.Equals(property.Value)); 
            var propertyNameMatchesDefaultName =defaultPropertyValues.Where(pv => pv.Item1.Equals(property.Name)).ToList();
            var defaultValues = propertyNameMatchesDefaultName.FirstOrDefault(pv => pv.Item2 && property.Id == categoryHeader);

            if (string.IsNullOrEmpty(property.Value))
            {
                var errorMessage = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability__0__Property___1___on_line_number_is_missing____2___will_be_set_as_default, property.LineNumber, property.Name, defaultValues?.Item3);
                errors.Add(errorMessage);
            }

            if (!propertyValueMatchesDefaultValue && !string.IsNullOrEmpty(property.Value))
            {
                var errorMessage = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability__0__Property__1__on_line_number_is_invalid____2___will_be_set_as_default, property.LineNumber, property.Name, defaultValues?.Item3);
                errors.Add(errorMessage);
            }
        }
    }
}