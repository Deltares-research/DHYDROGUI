using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.SedMor.ModelDefinition
{
    public class SedMorProperty : ModelProperty
    {
        public SedMorProperty(ModelPropertyDefinition propertyDefinition, string valueAsString)
            : base(propertyDefinition, valueAsString)
        {
        }

        public override object Clone()
        {
            return new SedMorProperty(PropertyDefinition, GetValueAsString());
        }
    }
}