using System;
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
    }
}