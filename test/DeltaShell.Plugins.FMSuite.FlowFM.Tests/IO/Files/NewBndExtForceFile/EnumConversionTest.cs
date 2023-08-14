using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile
{
    [TestFixture]
    public class EnumConversionTest
    {
        [Test]
        [TestCase("three")]
        [TestCase("THREE")]
        public void TryGetFromDescription_WithMultipleUsagesSameDescription_ThrowsInvalidOperationException(string description)
        {
            // Call
            void Call() => EnumConversion.TryGetFromDescription(description, out TestEnum _);

            // Assert
            Assert.That(Call, Throws.InvalidOperationException);
        }

        public enum TestEnum
        {
            [System.ComponentModel.Description("one")]
            TheFirstValue,

            [System.ComponentModel.Description("two")]
            TheSecondValue,

            [System.ComponentModel.Description("three")]
            TheThirdValue,

            [System.ComponentModel.Description("three")]
            TheFourthValue
        }

        [TestCase("one", true, TestEnum.TheFirstValue)]
        [TestCase("ONE", true, TestEnum.TheFirstValue)]
        [TestCase("two", true, TestEnum.TheSecondValue)]
        [TestCase("TWO", true, TestEnum.TheSecondValue)]
        [TestCase("", false, TestEnum.TheFirstValue)]
        [TestCase(null, false, TestEnum.TheFirstValue)]
        [TestCase("four", false, TestEnum.TheFirstValue)]
        public void TryGetFromDescription_ReturnsCorrectResultAndEnumValue(string description, bool expResult, TestEnum expEnumValue)
        {
            // Call
            bool result = EnumConversion.TryGetFromDescription(description, out TestEnum enumValue);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
            Assert.That(enumValue, Is.EqualTo(expEnumValue));
        }
    }
}