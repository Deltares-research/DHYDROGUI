using System;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.Tests
{
    public class TestModelProperty : ModelProperty
    {
        public TestModelProperty(ModelPropertyDefinition propertyDefinition, string valueAsString)
            : base(propertyDefinition, valueAsString)
        {
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}