using System;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.ModelSchema
{
    [TestFixture]
    public class DateOnlyModelPropertyTest
    {
        static readonly string validDateAsString = "20231231";
        static readonly string invalidDateAsString = "20231131"; // November has only 30 days 
        static readonly string notSupportedDateTimeAsString = "2023-12-31T23:59:59"; 

        [Test]
        public void ModelPropertyCreatedSuccessfullyWithValidDateAsString()
        {
            var definition = new TestModelPropertyDefinition { DataType = typeof(DateOnly) };
            TestModelProperty property = null;
            Assert.DoesNotThrow(() => property = new TestModelProperty(definition, validDateAsString));
            Assert.AreEqual(definition, property.PropertyDefinition);
        }

        [Test]
        public void ModelPropertyThrowsWithInvalidDateAsString()
        {
            var definition = new TestModelPropertyDefinition { DataType = typeof(DateOnly) };
            TestModelProperty property = null;
            Assert.Throws<FormatException>(() => property = new TestModelProperty(definition, invalidDateAsString));
            Assert.IsNull(property);
        }

        [Test]
        public void ModelPropertyThrowsWhenAssignedDoubleValueAsString()
        {
            var property = new TestModelProperty(new TestModelPropertyDefinition { DataType = typeof(DateOnly) },
                                                 validDateAsString);
            Assert.Throws<FormatException>(() => property.SetValueFromString("1.2"));
        }

        [Test]
        public void ModelPropertyThrowsWhenAssignedDoubleValue()
        {
            var property = new TestModelProperty(new TestModelPropertyDefinition { DataType = typeof(DateOnly) },
                                                 validDateAsString);
            Assert.Throws<ArgumentException>(() => property.Value = 1.2);
        }

        [Test]
        public void ModelPropertyThrowsWhenAssignedDateTimeValue()
        {
            var property = new TestModelProperty(new TestModelPropertyDefinition { DataType = typeof(DateOnly) },
                                                 validDateAsString);
            Assert.Throws<FormatException>(() => property.SetValueFromString(notSupportedDateTimeAsString));
        }
    }
}