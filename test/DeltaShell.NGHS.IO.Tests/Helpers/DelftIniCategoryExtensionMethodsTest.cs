using System.Collections.Generic;
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

        private enum TestEnum
        {
            Value1,
            Value2
        }

        [Test]
        public void ReadProperty_Enum_ShouldNotBeCaseSensitive()
        {
            // Setup
            var properties = new List<DelftIniProperty> 
            { 
                new DelftIniProperty("property_name1", "Value1", ""),
                new DelftIniProperty("property_name2", "value2", ""),
            };

            var category = new DelftIniCategory("category_name") { LineNumber = 7, Properties = properties };

            // Call
            var enumValue1 = category.ReadProperty<TestEnum>("property_name1");
            var enumValue2 = category.ReadProperty<TestEnum>("property_name2");

            // Assert
            Assert.AreEqual(TestEnum.Value1, enumValue1, "Reading enum with exact case match should work");
            Assert.AreEqual(TestEnum.Value2, enumValue2, "Reading enum with different case match should work");
        }
    }
}