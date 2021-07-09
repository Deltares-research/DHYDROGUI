using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Helpers;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Helpers
{
    [TestFixture]
    public class DelftIniCategoryExtensionMethodsTest
    {
        [Test]
        public void ReadProperty_IsOptionalFalse_PropertyNotFound_LogsError()
        {
            // Setup
            var category = new DelftIniCategory("some_name") {LineNumber = 7};

            // Call
            void Call() => category.ReadProperty<double>("some_property", isOptional: false);

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo("Property 'some_property' is not found in the file for category 'some_name' on line 7"));
        }

        [Test]
        public void ReadPropertiesToListOfType_IsOptionalFalse_PropertyNotFound_LogsError()
        {
            // Setup
            var category = new DelftIniCategory("some_name") {LineNumber = 7};

            // Call
            void Call() => category.ReadPropertiesToListOfType<double>("some_property", isOptional: false);

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo("Property 'some_property' is not found in the file for category 'some_name' on line 7"));
        }
    }
}