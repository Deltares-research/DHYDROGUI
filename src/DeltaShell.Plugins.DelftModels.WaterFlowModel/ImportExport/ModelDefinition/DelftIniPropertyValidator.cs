using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using Resources = DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties.Resources;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public static class DelftIniPropertyValidator
    {
        private static List<DelftIniProperty> Properties;

        private static readonly IList<string> Errors = new List<string>();

        private static string CategoryHeader;

        /// <summary>
        /// List of default values extracted from <see cref="DelftIniPropertyValidationLookup"/> belonging to
        /// a specific <see cref="DelftIniCategory"/>>
        /// <remarks>Temporary check until other delft ini properties can be validated.</remarks>
        /// </summary>
        private static List<Tuple<string, bool, List<string>>> DefaultPropertyValues = new List<Tuple<string, bool, List<string>>>();

        public static IList<string> ValidateProperties(this DelftIniCategory category)
        {
            Errors.Clear();
            DefaultPropertyValues.Clear();

            CategoryHeader = category.Name;
            Properties = category.Properties.ToList();
            category.RestrictPropertyToCategory();

            CheckIfRequired();
            
            return Errors;
        }

        private static void CheckIfRequired()
        {
            GetDefaultValues();

            foreach (var property in Properties)
            {
                if (DefaultPropertyValues.Count == 0)
                    return;

                var propertyIsRequired = DefaultPropertyValues.Any(pv => pv.Item2 && pv.Item1 == property.Name);

                if (propertyIsRequired)
                {
                    property.CheckPropertyAvailability();
                }
            }
        }

        private static void GetDefaultValues()
        {
            var values = DelftIniPropertyValidationLookup.LookupTable.
                Where(t => t.Key.Equals(CategoryHeader)).
                Select(t => t.Value).ToList();

            if (values.Count == 0) return;

            values.ForEach(v => v.ForEach(pv => DefaultPropertyValues.Add(pv)));
        }

        private static void RestrictPropertyToCategory(this DelftIniCategory category)
        {
            Properties.ForEach(property => property.Id = category.Name);
        }

        private static void CheckPropertyAvailability(this DelftIniProperty property)
        {
            var propertyValueMatchesDefaultValue = DefaultPropertyValues.Any(dpv => dpv.Item3.Any(i => i.Equals(property.Value))); 
            var propertyNameMatchesDefaultName =DefaultPropertyValues.Where(pv => pv.Item1.Equals(property.Name)).ToList();
            var defaultValues = propertyNameMatchesDefaultName.FirstOrDefault(pv => pv.Item2 && property.Id == CategoryHeader);

            if (string.IsNullOrEmpty(property.Value))
            {
                var errorMessage = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability_Property_on_line_number_is_missing_will_be_set_as_default, property.LineNumber, property.Name, defaultValues?.Item3.First());
                Errors.Add(errorMessage);
            }

            if (!propertyValueMatchesDefaultValue && !string.IsNullOrEmpty(property.Value))
            {
                var errorMessage = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability_Property_on_line_number_is_invalid_will_be_set_as_default, property.LineNumber, property.Name, defaultValues?.Item3.First());
                Errors.Add(errorMessage);
            }
            property.Value = defaultValues?.Item3.FirstOrDefault();
        }
    }
}