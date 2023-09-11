using DelftTools.Hydro;
using DelftTools.TestUtils;
using DHYDRO.Common.Logging;
using NSubstitute;
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
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var actualSewerConnectionWaterType = WaterTypeConverter.ConvertStringToSewerConnectionWaterType(waterTypeString, logHandler);
            Assert.That(actualSewerConnectionWaterType, Is.EqualTo(expectedSewerConnectionWaterType));
        }

        [Test]
        public void GivenInvalidWaterTypeString_WhenCallingWaterTypeConverter_ThenAddsMessageToLogAndSetsWaterTypeToNone()
        {
            var invalidWaterTypeString = "InvalidWaterType";
            var expectedWaterType = SewerConnectionWaterType.None;
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            SewerConnectionWaterType actualWaterType = WaterTypeConverter.ConvertStringToSewerConnectionWaterType(invalidWaterTypeString, logHandler);

            Assert.That(actualWaterType, Is.EqualTo(expectedWaterType));
            logHandler.Received(1).ReportWarningFormat(Properties.Resources.Water_type__0__is_not_a_valid_water_type_Setting_water_type_to_none, invalidWaterTypeString);

        }
    }
}