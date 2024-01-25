using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.EnumOperations;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.Helpers.EnumOperations
{
    [TestFixture]
    public class EnumParserTest
    {
        public enum TestEnum
        {
            [System.ComponentModel.Description("")]
            None,

            [System.ComponentModel.Description("one")]
            TheFirstValue,

            [System.ComponentModel.Description("two")]
            TheSecondValue,

            [System.ComponentModel.Description("three")]
            TheThirdValue
        }

        [Test]
        [TestCase("one", TestEnum.TheFirstValue, true)]
        [TestCase("ONE", TestEnum.TheFirstValue, true)]
        [TestCase("two", TestEnum.TheSecondValue, true)]
        [TestCase("TWO", TestEnum.TheSecondValue, true)]
        [TestCase("three", TestEnum.TheThirdValue, true)]
        [TestCase("THREE", TestEnum.TheThirdValue, true)]
        [TestCase("four", TestEnum.None, false)]
        [TestCase("", TestEnum.None, true)]
        [TestCase(" ", TestEnum.None, true)]
        [TestCase(null, TestEnum.None, true)]
        public void Parse_ReturnsCorrectEnumValue(string input, TestEnum expConvertedValue, bool expResult)
        {
            // Arrange
            var parser = new EnumParser();

            // Act
            bool result = parser.TryParseByDescription(input, TestEnum.None, out TestEnum convertedValue);

            // Assert
            Assert.That(convertedValue, Is.EqualTo(expConvertedValue));
            Assert.That(result, Is.EqualTo(expResult));
        }
    }
}