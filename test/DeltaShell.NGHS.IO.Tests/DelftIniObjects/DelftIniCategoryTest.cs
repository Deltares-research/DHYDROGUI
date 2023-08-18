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
        public void Constructor_CategoryIsNull_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                var _ = new DelftIniCategory(null);
            });
        }
        
        [Test]
        public void Constructor_WithExistingCategory_CreatesCopy()
        {
            // Setup
            DelftIniProperty[] properties =
            {
                GetProperty("Property1"),
                GetProperty("Property2")
            };

            var category = new DelftIniCategory("category", 10);
            category.AddProperties(properties);
            
            // Call
            var copy = new DelftIniCategory(category);
            
            // Assert
            Assert.That(copy.Id, Is.EqualTo(category.Id));
            Assert.That(copy.Name, Is.EqualTo(category.Name));
            Assert.That(copy.LineNumber, Is.EqualTo(category.LineNumber));
            Assert.That(copy.Properties.Select(x => x.Name), Is.EqualTo(category.Properties.Select(x => x.Name)));
            Assert.That(copy.Properties.Select(x => x.Value), Is.EqualTo(category.Properties.Select(x => x.Value)));
        }
        
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
        
        [Test]
        public void ContainsPropertyWithId_IdIsNull_ThrowsArgumentNullException()
        {
            // Setup
            var category = new DelftIniCategory("category_name");
            
            // Assert
            Assert.Throws<ArgumentNullException>(() => category.ContainsPropertyWithId(null));
        }
                
        [Test]
        [TestCase("property_name", ExpectedResult = true)]
        [TestCase("PROPERTY_NAME", ExpectedResult = true)]
        [TestCase("Property_Name", ExpectedResult = true)]
        [TestCase("category_name", ExpectedResult = false)]
        [TestCase("propertyname", ExpectedResult = false)]
        public bool ContainsPropertyWithId_WithValidIdentifier_ReturnsExpectedValue(string id)
        {
            // Setup
            var category = new DelftIniCategory("category_name");
            var property = new DelftIniProperty("property_name", "property_value", "property_comment");
            category.AddProperty(property);
            
            // Call
            return category.ContainsPropertyWithId(id);
        }
        
        [Test]
        public void IdEqualsTo_IdIsNull_ThrowsArgumentNullException()
        {
            // Setup
            var category = new DelftIniCategory("category_name");
            
            // Assert
            Assert.Throws<ArgumentNullException>(() => category.IdEqualsTo(null));
        }
        
        [Test]
        [TestCase("category_name", ExpectedResult = true)]
        [TestCase("CATEGORY_NAME", ExpectedResult = true)]
        [TestCase("Category_Name", ExpectedResult = true)]
        [TestCase("categoryname", ExpectedResult = false)]
        public bool IdEqualsTo_WithValidIdentifier_ReturnsExpectedValue(string id)
        {
            // Setup
            var category = new DelftIniCategory("category_name");
            
            // Call
            return category.IdEqualsTo(id);
        }

        private static DelftIniProperty GetProperty(string name) => new DelftIniProperty(name, $"{name}-value", string.Empty);
    }
}