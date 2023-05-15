using System;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.ModelSchema
{
    [TestFixture]
    public class DateTimeModelPropertyTest
    {
        static readonly string validDateAsString = "20231231";
        static readonly string validDateTimeAsString = "20231231235959";
        static readonly string invalidDateTimeAsString = "20231130000060"; // Seconds must be <60 

        [Test]
        public void ModelPropertyValueShouldValidateType()
        {
            var dateTimeDefinition = new TestModelPropertyDefinition
            {
                Caption = "DateTimePropertyDefinition",
                DataType = typeof(DateTime)
            };
            string dateTimeValueAsString = "20130102112359";
            Assert.Throws<FormatException>(() => new TestModelProperty(dateTimeDefinition, "1.2"));

            var property = new TestModelProperty(dateTimeDefinition, dateTimeValueAsString);
            Assert.Throws<FormatException>(() => property.SetValueAsString("1.2"));

            Assert.Throws<ArgumentException>(() => property.Value = 1.2);
        }

        [Test]
        public void ModelPropertyCreatedSuccessfullyWithValidDateTimeAsString()
        {
            var definition = new TestModelPropertyDefinition { DataType = typeof(DateTime) };
            TestModelProperty property = null;
            Assert.DoesNotThrow(() => property = new TestModelProperty(definition, validDateTimeAsString));
            Assert.AreEqual(definition, property.PropertyDefinition);
        }

        [Test]
        public void ModelPropertyThrowsWithInvalidDateAsString()
        {
            var definition = new TestModelPropertyDefinition { DataType = typeof(DateTime) };
            TestModelProperty property = null;
            Assert.Throws<FormatException>(() => property = new TestModelProperty(definition, invalidDateTimeAsString));
            Assert.IsNull(property);
        }

        [Test]
        public void ModelPropertyThrowsWhenAssignedDoubleValueAsString()
        {
            var property = new TestModelProperty(new TestModelPropertyDefinition { DataType = typeof(DateTime) },
                                                 validDateTimeAsString);
            Assert.Throws<FormatException>(() => property.SetValueAsString("1.2"));
        }

        [Test]
        public void ModelPropertyThrowsWhenAssignedDoubleValue()
        {
            var property = new TestModelProperty(new TestModelPropertyDefinition { DataType = typeof(DateTime) },
                                                 validDateTimeAsString);
            Assert.Throws<ArgumentException>(() => property.Value = 1.2);
        }

        [Test]
        public void ModelPropertyThrowsWhenAssignedDateValue()
        {
            var property = new TestModelProperty(new TestModelPropertyDefinition { DataType = typeof(DateTime) },
                                                 validDateTimeAsString);
            Assert.Throws<FormatException>(() => property.SetValueAsString(validDateAsString));
        }
    }
}