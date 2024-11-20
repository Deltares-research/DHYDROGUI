using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public class WaterFlowFMProperty : ModelProperty
    {
        public WaterFlowFMProperty(ModelPropertyDefinition propertyDefinition, string valueAsString) :
            base(propertyDefinition, valueAsString)
        {
        }

        public static WaterFlowFMPropertyDefinition CreatePropertyDefinitionForUnknownProperty(string mduGroupName, string mduPropertyName, string comment)
        {
            return new WaterFlowFMPropertyDefinition
            {
                Caption = mduPropertyName,
                MduPropertyName = mduPropertyName,
                FileSectionName = mduGroupName,
                FilePropertyKey = mduPropertyName,
                Category = ModelSchemaCsvFile.DefaultGUIGroupCaption,
                SubCategory = null,
                DataType = typeof (string),
                DefaultValueAsString = "",
                EnabledDependencies = "",
                VisibleDependencies = "",
                Description = comment,
                IsDefinedInSchema = false,
                IsFile = false
            };
        }

        public new WaterFlowFMPropertyDefinition PropertyDefinition
        {
            get { return (WaterFlowFMPropertyDefinition) base.PropertyDefinition; }
        }

        public override object Clone()
        {
            return new WaterFlowFMProperty(PropertyDefinition, GetValueAsString());
        }
    }
}