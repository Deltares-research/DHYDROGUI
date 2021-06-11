using System;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Helpers
{
    [TestFixture]
    public class DelftIniPropertyExtensionMethodsTest
    {
        [Test]
        public void ReadValue_PropertyNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((IDelftIniProperty) null).ReadValue<int>();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("property"));
        }

        [Test]
        public void ReadValue_AsInt_ReturnsCorrectValue()
        {
            // Setup
            var property = new DelftIniProperty("some_property", "1", "some_comment");

            // Call
            var result = property.ReadValue<int>();

            // Assert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void ReadValue_AsDouble_ReturnsCorrectValue()
        {
            // Setup
            var property = new DelftIniProperty("some_property", "1.5", "some_comment");

            // Call
            var result = property.ReadValue<double>();

            // Assert
            Assert.That(result, Is.EqualTo(1.5));
        }

        [Test]
        public void ReadValue_CannotConvertValue_ThrowsFormatException()
        {
            // Setup
            var property = new DelftIniProperty("some_property", "three", "some_comment");

            // Call
            void Call() => property.ReadValue<double>();

            // Assert
            var e = Assert.Throws<Exception>(Call);
            Assert.That(e.Message, Is.EqualTo("three is not a valid value for Double."));
        }
    }
}