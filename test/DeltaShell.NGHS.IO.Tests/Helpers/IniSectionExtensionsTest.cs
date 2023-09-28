using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Helpers
{
    [TestFixture]
    public class IniSectionExtensionsTest
    {
        [Test]
        public void ReadProperty_IsOptionalFalse_PropertyNotFound_LogsError()
        {
            // Setup
            var iniSection = new IniSection("some_name") {LineNumber = 7};

            // Call
            void Call() => iniSection.ReadProperty<double>("some_property", isOptional: false);

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo("Property 'some_property' is not found in the file for section 'some_name' on line 7"));
        }

        [Test]
        public void ReadPropertiesToListOfType_IsOptionalFalse_PropertyNotFound_LogsError()
        {
            // Setup
            var iniSection = new IniSection("some_name") {LineNumber = 7};

            // Call
            void Call() => iniSection.ReadPropertiesToListOfType<double>("some_property", isOptional: false);

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo("Property 'some_property' is not found in the file for section 'some_name' on line 7"));
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
            var properties = new List<IniProperty> 
            { 
                new IniProperty("property_name1", "Value1", ""),
                new IniProperty("property_name2", "value2", ""),
            };

            var iniSection = new IniSection("section_name") { LineNumber = 7 };
            iniSection.AddMultipleProperties(properties);

            // Call
            var enumValue1 = iniSection.ReadProperty<TestEnum>("property_name1");
            var enumValue2 = iniSection.ReadProperty<TestEnum>("property_name2");

            // Assert
            Assert.AreEqual(TestEnum.Value1, enumValue1, "Reading enum with exact case match should work");
            Assert.AreEqual(TestEnum.Value2, enumValue2, "Reading enum with different case match should work");
        }
    }
}