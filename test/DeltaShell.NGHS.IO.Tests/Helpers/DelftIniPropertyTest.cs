using System;
using DeltaShell.NGHS.IO.DelftIniObjects;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Helpers
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
    }
}