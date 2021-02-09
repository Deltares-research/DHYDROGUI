using System;
using System.ComponentModel;
using DeltaShell.NGHS.IO.DelftIniObjects;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DelftIniObjects
{
    [TestFixture]
    public class DelftIniCategoryTest
    {
        [Test]
        public void GetPropertyValue_PropertyNotFound_ReturnsDefaultValue()
        {
            // Setup
            DelftIniProperty[] properties =
            {
                GetProperty("Property_Name"),
                GetProperty("Property_name"),
                GetProperty("property_name")
            };

            var category = new DelftIniCategory("category");
            category.AddProperties(properties);

            // Call
            string value = category.GetPropertyValue("PROPERTY_NAME", "some default value");

            // Assert
            Assert.That(value, Is.EqualTo("some default value"));
        }

        [Test]
        public void GetPropertyValue_ComparisonTypeNotDefined_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            DelftIniProperty[] properties =
            {
                GetProperty("Property_Name"),
                GetProperty("Property_name"),
                GetProperty("property_name")
            };

            var category = new DelftIniCategory("category");
            category.AddProperties(properties);

            // Call
            void Call() => category.GetPropertyValue("property_name", comparisonType: (StringComparison) 100);

            // Assert
            var e = Assert.Throws<InvalidEnumArgumentException>(Call);
            Assert.That(e.Message, Is.EqualTo("comparisonType"));
        }

        [TestCase(StringComparison.Ordinal, "property_name-value")]
        [TestCase(StringComparison.CurrentCulture, "property_name-value")]
        [TestCase(StringComparison.InvariantCulture, "property_name-value")]
        [TestCase(StringComparison.OrdinalIgnoreCase, "Property_Name-value")]
        [TestCase(StringComparison.CurrentCultureIgnoreCase, "Property_Name-value")]
        [TestCase(StringComparison.InvariantCultureIgnoreCase, "Property_Name-value")]
        public void GetPropertyValue_ReturnsCorrectValue(StringComparison comparisonType, string expValue)
        {
            // Setup
            DelftIniProperty[] properties =
            {
                GetProperty("Property_Name"),
                GetProperty("Property_name"),
                GetProperty("property_name")
            };

            var category = new DelftIniCategory("category");
            category.AddProperties(properties);

            // Call
            string value = category.GetPropertyValue("property_name", comparisonType: comparisonType);

            // Assert
            Assert.That(value, Is.EqualTo(expValue));
        }

        private static DelftIniProperty GetProperty(string name) => new DelftIniProperty(name, $"{name}-value", string.Empty);
    }
}