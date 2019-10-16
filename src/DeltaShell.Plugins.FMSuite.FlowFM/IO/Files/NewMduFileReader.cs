using System.Collections.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public static class NewMduFileReader
    {
        public static void Read(string filePath, WaterFlowFMModelDefinition definition)
        {
            IList<IDelftIniCategory> categories = new DelftIniReader().ReadDelftIniFile(filePath);
            UpdateLegacyNames(categories);
            SetPropertyValues(definition, categories);

            definition.SetGuiTimePropertiesFromMduProperties();
        }

        private static void UpdateLegacyNames(IList<IDelftIniCategory> categories)
        {
            categories.ForEach(category =>
            {
                category.Name = MduFileBackwardsCompatibilityHelper.GetUpdatedPropertyName(category.Name);
                category.Properties.ForEach(property =>
                {
                    property.Name =
                        MduFileBackwardsCompatibilityHelper.GetUpdatedPropertyName(property.Name);
                });
            });
        }

        private static void SetPropertyValues(WaterFlowFMModelDefinition definition, IList<IDelftIniCategory> categories)
        {
            foreach (IDelftIniCategory category in categories)
            {
                foreach (IDelftIniProperty property in category.Properties)
                {
                    SetPropertyValue(definition, property, category.Name);
                }
            }
        }

        private static void SetPropertyValue(WaterFlowFMModelDefinition definition, IDelftIniProperty property, string categoryName)
        {
            WaterFlowFMProperty modelProperty = definition.GetModelProperty(property.Name);
            if (modelProperty == null)
            {
                string propertyComment = property.Comment == string.Empty ? null : property.Comment; // This is a little odd, maybe string.Empty is not so bad?.
                WaterFlowFMPropertyDefinition newPropertyDefinition = WaterFlowFMPropertyDefinitionCreator.CreateForUnknownProperty(categoryName, property.Name, propertyComment);
                var newProperty = new WaterFlowFMProperty(newPropertyDefinition, property.Value);

                definition.AddProperty(newProperty);
                return;
            }

            modelProperty.Value = property.Value;
        }
    }
}