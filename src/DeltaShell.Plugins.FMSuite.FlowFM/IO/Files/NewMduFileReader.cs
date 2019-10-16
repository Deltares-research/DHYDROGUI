using System.Collections.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public class NewMduFileReader
    {
        public void Read(string filePath, WaterFlowFMModelDefinition definition)
        {
            IList<IDelftIniCategory> categories = new DelftIniReader().ReadDelftIniFile(filePath);

            UpdateLegacyNames(categories);


            foreach (IDelftIniCategory category in categories)
            {
                foreach (IDelftIniProperty property in category.Properties)
                {
                    SetPropertyValue(definition, property, category.Name);
                }
            }

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

        private static void SetPropertyValue(WaterFlowFMModelDefinition definition, IDelftIniProperty property, string categoryName)
        {
            WaterFlowFMProperty modelProperty = definition.GetModelProperty(property.Name);
            if (modelProperty == null)
            {
                WaterFlowFMPropertyDefinition newPropertyDefinition = WaterFlowFMPropertyDefinitionCreator.CreateForUnknownProperty(categoryName, property.Name, property.Comment);
                var newProperty = new WaterFlowFMProperty(newPropertyDefinition, property.Value);

                definition.AddProperty(newProperty);
                return;
            }

            modelProperty.Value = property.Value;
        }
    }
}