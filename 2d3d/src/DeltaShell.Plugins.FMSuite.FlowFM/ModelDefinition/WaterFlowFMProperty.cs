using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public class WaterFlowFMProperty : ModelProperty
    {
        public WaterFlowFMProperty(ModelPropertyDefinition propertyDefinition, string valueAsString) :
            base(propertyDefinition, valueAsString) {}

        public new WaterFlowFMPropertyDefinition PropertyDefinition =>
            (WaterFlowFMPropertyDefinition) base.PropertyDefinition;

        public override object Clone()
        {
            return new WaterFlowFMProperty(PropertyDefinition, GetValueAsString()) { LineNumber = LineNumber };
        }
    }

    public enum PropertySource
    {
        None,
        MduFile,
        MorphologyFile,
        SedimentFile
    }
}