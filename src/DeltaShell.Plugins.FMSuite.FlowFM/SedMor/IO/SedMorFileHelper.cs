using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.SedMor.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.SedMor.IO
{
    public static class SedMorFileHelper
    {
        public static void LoadCategoryIntoProperties(DelftIniCategory fileCategory,
                                                  IDictionary<string, SedMorProperty> properties,
                                                  IDictionary<string, SedMorPropertyDefinition> propertyDefinitions,
                                                  string guiCategory)
        {
            foreach (var morProperty in fileCategory.Properties)
            {
                var propName = morProperty.Name;
                var propertyValue = morProperty.Value;

                var definition = propertyDefinitions.ContainsKey(propName.ToLower())
                                     ? propertyDefinitions[propName.ToLower()]
                                     : new SedMorPropertyDefinition
                                         {
                                             DataType = typeof (string),
                                             FileCategoryName = fileCategory.Name,
                                             FilePropertyName = morProperty.Name,
                                             Category = guiCategory
                                         };

                properties[propName.ToLower()] = new SedMorProperty(definition, propertyValue);
            }
        }

        public static List<DelftIniCategory> PutPropertiesIntoCategories(Dictionary<string, SedMorProperty> sedMorProperties)
        {
            var categories = new List<DelftIniCategory>();
            foreach (var property in sedMorProperties.Values)
            {
                var propDef = property.PropertyDefinition;
                var propertyCategoryName = propDef.FileCategoryName;
                var delftIniCategory = categories.FirstOrDefault(c => c.Name == propertyCategoryName);
                if (delftIniCategory == null)
                {
                    delftIniCategory = new DelftIniCategory(propertyCategoryName);
                    categories.Add(delftIniCategory);
                }

                SedMorFileHelper.AddProperty(delftIniCategory, propDef.FilePropertyName, property);
            }
            return categories;
        }

        public static void AddProperty(DelftIniCategory delftIniCategory, string propertyName,
                                       SedMorProperty property)
        {
            var strValue = property.GetValueAsString();
            if (property.PropertyDefinition.DataType == typeof (string) &&
                !property.PropertyDefinition.FileCategoryName.EndsWith("FileInformation"))
            {
                if (strValue.Contains(" "))
                    strValue = "#" + strValue + "#";
            }
            delftIniCategory.AddProperty(propertyName, strValue);
        }
    }
}