using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Helpers;
using log4net.Core;
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
        public void ReadValue_CannotParseValue_LogsError()
        {
            // Setup
            var property = new DelftIniProperty("some_property", "three", "some_comment") {LineNumber = 3};

            // Call
            void Call() => property.ReadValue<double>();

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo("Cannot parse three to a Double for property some_property on line 3."));
        }

        [Test]
        public void ReadBooleanValue_PropertyNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((IDelftIniProperty) null).ReadBooleanValue();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("property"));
        }

        [Test]
        public void ReadBooleanValue_CannotParseValue_LogsError()
        {
            // Setup
            var property = new DelftIniProperty("some_property", "two", "some_comment") {LineNumber = 3};

            // Call
            void Call() => property.ReadBooleanValue();

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo("Cannot parse two to a Boolean for property some_property on line 3."));
        }

        [TestCase("True", true)]
        [TestCase("true", true)]
        [TestCase("1", true)]
        [TestCase("2", true)]
        [TestCase("False", false)]
        [TestCase("false", false)]
        [TestCase("0", false)]
        public void ReadBooleanValue_ReturnsCorrectValue(string propertyValue, bool expResult)
        {
            // Setup
            var property = new DelftIniProperty("some_property", propertyValue, "some_comment");

            // Call
            bool result = property.ReadBooleanValue();

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }
    }
}