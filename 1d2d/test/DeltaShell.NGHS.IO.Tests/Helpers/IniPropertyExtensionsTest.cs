using System;
using System.Linq;
using DelftTools.TestUtils;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Helpers;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Helpers
{
    [TestFixture]
    public class IniPropertyExtensionsTest
    {
        [Test]
        public void ReadBooleanValue_PropertyNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((IniProperty) null).ReadBooleanValue();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("property"));
        }

        [Test]
        public void ReadBooleanValue_CannotParseValue_LogsError()
        {
            // Setup
            var property = new IniProperty("some_property", "two", "some_comment") {LineNumber = 3};

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
            var property = new IniProperty("some_property", propertyValue, "some_comment");

            // Call
            bool result = property.ReadBooleanValue();

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }
    }
}