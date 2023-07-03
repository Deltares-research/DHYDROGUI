using System;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Wave.ModelDefinition
{
    [Entity]
    public class WaveModelProperty : ModelProperty
    {
        public WaveModelProperty(ModelPropertyDefinition propertyDefinition, string valueAsString) : base(
            propertyDefinition, valueAsString) {}

        public override object Clone()
        {
            return new WaveModelProperty(PropertyDefinition, GetValueAsString());
        }

        public override string GetValueAsString()
        {
            if (PropertyDefinition.DataType == typeof(bool))
            {
                return (bool) Value ? "true" : "false";
            }
            
            if (PropertyDefinition.DataType == typeof(DateOnly))
            {
                return ((DateOnly) Value).ToString("yyyy-MM-dd");
            }

            return base.GetValueAsString();
        }
    }
}