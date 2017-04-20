using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.ModelSchema
{
    [TestFixture]
    public class ModelPropertyTest
    {
        [Test]
        public void SettingValueShouldValidateType()
        {
            var definition = new TestModelPropertyDefinition
                {
                    DataType = typeof (DateTime)
                };
            Assert.Throws<FormatException>(() => new TestModelProperty(definition, "1.2"));

            var property = new TestModelProperty(definition, "20130102");
            Assert.Throws<FormatException>(() => property.SetValueAsString("1.2"));

            Assert.Throws<ArgumentException>(() => property.Value = 1.2);

            var doubleArrayDefinition = new TestModelPropertyDefinition
                {
                    DataType = typeof (IList<double>)
                };
            var doubleArrayProperty = new TestModelProperty(doubleArrayDefinition, "1 2 3");
            doubleArrayProperty.Value = new List<double> {5, 6, 7, 8}; // Should not throw
            Assert.AreEqual(new List<double>{5,6,7,8}, doubleArrayProperty.Value);
        }
    }
}