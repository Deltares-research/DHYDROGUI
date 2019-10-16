using System.Collections.Generic;
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
            foreach (IDelftIniCategory category in categories)
            {
                foreach (IDelftIniProperty property in category.Properties)
                {
                    WaterFlowFMProperty modelProperty = definition.GetModelProperty(property.Name);
                    if (modelProperty == null)
                    {
                        WaterFlowFMPropertyDefinition newPropertyDefinition =
                            WaterFlowFMPropertyDefinitionCreator.CreateForUnknownProperty(
                                category.Name, property.Name, property.Comment);
                        var newProperty = new WaterFlowFMProperty(newPropertyDefinition, property.Value);

                        definition.AddProperty(newProperty);
                        continue;
                    }

                    modelProperty.Value = property.Value;

                }
            }

            definition.SetGuiTimePropertiesFromMduProperties();
        }
    }
}