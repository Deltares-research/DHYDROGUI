using System;
using System.ComponentModel;
using System.Linq;
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

        [Test]
        public void RemoveProperty_PropertyNull_ThrowsArgumentNullException()
        {
            // Setup
            var category = new DelftIniCategory("category_name");

            // Call
            void Call() => category.RemoveProperty(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("property"));
        }

        [Test]
        public void RemoveProperty_DoesContainProperty_PropertyIsRemoved()
        {
            // Setup
            var category = new DelftIniCategory("category_name");
            var property = new DelftIniProperty("property_name", "property_value", "property_comment");
            category.AddProperty(property);

            Assert.That(category.Properties, Does.Contain(property));

            // Call
            category.RemoveProperty(property);

            // Assert
            Assert.That(category.Properties, Does.Not.Contain(property));
        }

        [Test]
        public void RemoveProperty_DoesNotContainProperty_RemovesNothing()
        {
            // Setup
            var category = new DelftIniCategory("category_name");
            var property1 = new DelftIniProperty("property_name_1", "property_value_1", "property_comment_1");
            var property2 = new DelftIniProperty("property_name_2", "property_value_2", "property_comment_2");
            category.AddProperty(property1);

            DelftIniProperty[] originalProperties = category.Properties.ToArray();

            // Call
            category.RemoveProperty(property2);

            // Assert
            Assert.That(category.Properties, Is.EqualTo(originalProperties));
        }

        private static DelftIniProperty GetProperty(string name) => new DelftIniProperty(name, $"{name}-value", string.Empty);
    }
}