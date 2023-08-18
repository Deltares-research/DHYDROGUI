using System;
using System.Linq;
using DeltaShell.NGHS.IO.DelftIniObjects;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DelftIniObjects
{
    [TestFixture]
    public class DelftIniPropertyTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            const string propertyName = "myName";
            const string propertyValue = "myValue";
            const string propertyComment = "myComment";

            // Call
            var property = new DelftIniProperty(propertyName, propertyValue, propertyComment);

            Assert.That(property.Id, Is.EqualTo(propertyName));
            Assert.That(property.Name, Is.EqualTo(propertyName));
            Assert.That(property.Value, Is.EqualTo(propertyValue));
            Assert.That(property.Comment, Is.EqualTo(propertyComment));
        }

        [Test]
        public void ConstructorWithLineNumber_ExpectedValues()
        {
            // Setup
            const string propertyName = "myName";
            const string propertyValue = "myValue";
            const string propertyComment = "myComment";
            int propertyLineNumber = new Random().Next(0, 100);

            // Call
            var property = new DelftIniProperty(propertyName, propertyValue, propertyComment, propertyLineNumber);

            Assert.That(property.Name, Is.EqualTo(propertyName));
            Assert.That(property.Value, Is.EqualTo(propertyValue));
            Assert.That(property.Comment, Is.EqualTo(propertyComment));
            Assert.That(property.LineNumber, Is.EqualTo(propertyLineNumber));
        }

        [Test]
        public void Constructor_PropertyIsNull_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                var _ = new DelftIniProperty(null);
            });
        }

        [Test]
        public void ConstructorWithExistingProperty_CreatesCopy()
        {
            // Setup
            const string propertyName = "myName";
            const string propertyValue = "myValue";
            const string propertyComment = "myComment";
            int propertyLineNumber = new Random().Next(0, 100);
            var property = new DelftIniProperty(propertyName, propertyValue, propertyComment, propertyLineNumber);

            // Call
            var copy = new DelftIniProperty(property);

            // Assert
            Assert.That(copy.Id, Is.EqualTo(propertyName));
            Assert.That(copy.Name, Is.EqualTo(propertyName));
            Assert.That(copy.Value, Is.EqualTo(propertyValue));
            Assert.That(copy.Comment, Is.EqualTo(propertyComment));
            Assert.That(copy.LineNumber, Is.EqualTo(propertyLineNumber));
        }
        
        [Test]
        public void IdEqualsTo_IdIsNull_ThrowsArgumentNullException()
        {
            // Setup
            var property = new DelftIniProperty("myName", "myValue", "myComment");
            
            // Assert
            Assert.Throws<ArgumentNullException>(() => property.IdEqualsTo(null));
        }
        
        [Test]
        [TestCase("myName", ExpectedResult = true)]
        [TestCase("MYNAME", ExpectedResult = true)]
        [TestCase("MyName", ExpectedResult = true)]
        [TestCase("name", ExpectedResult = false)]
        public bool IdEqualsTo_WithValidIdentifier_ReturnsExpectedValue(string id)
        {
            // Setup
            var property = new DelftIniProperty("myName", "myValue", "myComment");
            
            // Call
            return property.IdEqualsTo(id);
        }
        
        [Test]
        public void UpdateIdentifiers_PropertiesIsNull_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => DelftIniProperty.UpdateIdentifiers(null));
        }
        
        [Test]
        public void UpdateIdentifiers_MultiplePropertiesWithSameName_AssignsUniqueIdentifiers()
        {
            // Setup
            DelftIniProperty[] properties =
            {
                new DelftIniProperty("a", "", ""), 
                new DelftIniProperty("b", "", ""), 
                new DelftIniProperty("c", "", ""), 
                new DelftIniProperty("c", "", "")
            };

            // Call
            DelftIniProperty.UpdateIdentifiers(properties);

            // Assert
            var expected = new[] { "a0", "b0", "c0", "c1" };
            Assert.That(properties.Select(c => c.Id), Is.EqualTo(expected));
        }
        
        [Test]
        public void UpdateIdentifiers_MultiplePropertiesWithSameNameDifferentCasing_AssignsUniqueIdentifiers()
        {
            // Setup
            DelftIniProperty[] categories =
            {
                new DelftIniProperty("Property", "", ""), 
                new DelftIniProperty("property", "", ""), 
                new DelftIniProperty("PROPERTY", "", "")
            };

            // Call
            DelftIniProperty.UpdateIdentifiers(categories);

            // Assert
            var expected = new[] { "Property0", "property1", "PROPERTY2" };
            Assert.That(categories.Select(c => c.Id), Is.EqualTo(expected));
        }
    }
}