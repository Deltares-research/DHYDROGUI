using DelftTools.Hydro;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class WaterTypeConverterTest
    {
        [Test]
        [TestCase("GmD", SewerConnectionWaterType.Combined)]
        [TestCase("Combined", SewerConnectionWaterType.Combined)]
        [TestCase("HWA", SewerConnectionWaterType.StormWater)]
        [TestCase("dwa", SewerConnectionWaterType.DryWater)]
        [TestCase("Dry Weather", SewerConnectionWaterType.DryWater)]
        [TestCase("none", SewerConnectionWaterType.None)]
        [TestCase("nVT", SewerConnectionWaterType.None)]
        public void GivenWaterTypeString_WhenCallingWaterTypeConverter_ThenReturnsCorrectSewerConnectionWaterType(string waterTypeString, SewerConnectionWaterType expectedSewerConnectionWaterType)
        {
            var actualSewerConnectionWaterType = WaterTypeConverter.ConvertStringToSewerConnectionWaterType(waterTypeString);
            Assert.That(actualSewerConnectionWaterType, Is.EqualTo(expectedSewerConnectionWaterType));
        }

        [Test]
        public void GivenInvalidWaterTypeString_WhenCallingWaterTypeConverter_ThenAddsMessageToLogAndSetsWaterTypeToNone()
        {
            var invalidWaterTypeString = "InvalidWaterType";
            var expectedWaterType = SewerConnectionWaterType.None;
            var expectedMessage = $"Water type {invalidWaterTypeString} is not a valid water type. Setting the water type to 'none'.";

            SewerConnectionWaterType actualWaterType = WaterTypeConverter.ConvertStringToSewerConnectionWaterType(invalidWaterTypeString);

            Assert.That(actualWaterType, Is.EqualTo(expectedWaterType));
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => actualWaterType = WaterTypeConverter.ConvertStringToSewerConnectionWaterType(invalidWaterTypeString), expectedMessage);

        }
    }
}