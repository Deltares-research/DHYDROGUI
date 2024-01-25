using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.EnumOperations;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.Helpers.EnumOperations
{
    public class EnumFormatterTest
    {
        [Test]
        public void GetFormattedDescriptions()
        {
            // Call
            string result = EnumFormatter.GetFormattedDescriptions<TestEnum>();

            // Assert
            Assert.That(result, Is.EqualTo("one, two, four, TheFifthValue"));
        }

        private enum TestEnum
        {
            [System.ComponentModel.Description("one")]
            TheFirstValue,

            [System.ComponentModel.Description("two")]
            TheSecondValue,

            [System.ComponentModel.Description("")]
            TheThirdValue,

            [System.ComponentModel.Description("four")]
            TheFourthValue,

            TheFifthValue
        }
    }
}