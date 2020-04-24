using DelftTools.Hydro;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class WaterTypeConverterTest
    {
        [Test]
        [TestCase("GmD", SewerConnectionWaterType.Combined)]
        [TestCase("HWA", SewerConnectionWaterType.StormWater)]
        [TestCase("dwa", SewerConnectionWaterType.DryWater)]
        [TestCase("nVT", SewerConnectionWaterType.None)]
        public void GivenWaterTypeString_WhenCallingWaterTypeConverter_ThenReturnsCorrectSewerConnectionWaterType(string waterTypeString, SewerConnectionWaterType expectedSewerConnectionWaterType)
        {
            var actualSewerConnectionWaterType = WaterTypeConverter.ConvertStringToSewerConnectionWaterType(waterTypeString);
            Assert.That(actualSewerConnectionWaterType, Is.EqualTo(expectedSewerConnectionWaterType));
        }
    }
}