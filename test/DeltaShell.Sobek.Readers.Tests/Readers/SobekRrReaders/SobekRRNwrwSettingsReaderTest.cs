using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRNwrwSettingsReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void CorrectlyParseNwrwSettingsLineWithRfTag()
        {
            var line = "PLVG id '-1' rf 0.5 0.2 0.1 0.5 0.2 0.1 0.5 0.2 0.1 0.5 0.2 0.1 ms 0 0.5 1 0 0.5 1 0 2 4 2 4 6 ix 0 2 0 5 im 0 0.5 0 1 ic 0 3 0 3 dc 0 0.1 0 0.1 od 1 or 0 plvg";

            var sobekRRNwrwSettings = new SobekRRNwrwSettingsReader().Parse(line);
            var sobekRRNwrwSetting = sobekRRNwrwSettings.FirstOrDefault();

            Assert.That(sobekRRNwrwSetting, Is.Not.Null);
            Assert.That(sobekRRNwrwSetting.Name, Is.EqualTo(null));
            Assert.That(sobekRRNwrwSetting.Id, Is.EqualTo("-1"));
            Assert.That(sobekRRNwrwSetting.RunoffDelayFactors, Is.EqualTo(new []{0.5, 0.2, 0.1, 0.5, 0.2, 0.1 , 0.5, 0.2, 0.1 , 0.5, 0.2, 0.1 }));
            Assert.That(sobekRRNwrwSetting.IsOldFormatData, Is.EqualTo(false));
            Assert.That(sobekRRNwrwSetting.MaximumStorages, Is.EqualTo(new [] { 0, 0.5, 1, 0, 0.5, 1, 0, 2, 4, 2, 4, 6 }));
            Assert.That(sobekRRNwrwSetting.MaximumInfiltrationCapcaties, Is.EqualTo(new [] { 0, 2, 0, 5 }));
            Assert.That(sobekRRNwrwSetting.MinimumInfiltrationCapcaties, Is.EqualTo(new [] { 0, 0.5, 0, 1 }));
            Assert.That(sobekRRNwrwSetting.InfiltrationCapacityDecreases, Is.EqualTo(new [] { 0, 3, 0, 3 }));
            Assert.That(sobekRRNwrwSetting.InfiltrationCapacityIncreases, Is.EqualTo(new []{ 0, 0.1, 0, 0.1 }));
            Assert.That(sobekRRNwrwSetting.InfiltrationFromDepressions, Is.EqualTo(true));
            Assert.That(sobekRRNwrwSetting.InfiltrationFromRunoff, Is.EqualTo(false));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CorrectlyParseNwrwSettingsLineWithOldRuTag()
        {
            var line = "PLVG id '-1' ru 0.5 0.2 0.1 ms 0 0.5 1 0 0.5 1 0 2 4 2 4 6 ix 0 2 0 5 im 0 0.5 0 1 ic 0 3 0 3 dc 0 0.1 0 0.1 od 0 or 1 plvg";

            var sobekRRNwrwSettings = new SobekRRNwrwSettingsReader().Parse(line);

            var sobekRRNwrwSetting = sobekRRNwrwSettings.FirstOrDefault();
            Assert.That(sobekRRNwrwSetting, Is.Not.Null);
            Assert.That(sobekRRNwrwSetting.Name, Is.EqualTo(null));
            Assert.That(sobekRRNwrwSetting.Id, Is.EqualTo("-1"));
            Assert.That(sobekRRNwrwSetting.RunoffDelayFactors, Is.EqualTo(new[] { 0.5, 0.2, 0.1 }));
            Assert.That(sobekRRNwrwSetting.IsOldFormatData, Is.EqualTo(true));
            Assert.That(sobekRRNwrwSetting.MaximumStorages, Is.EqualTo(new[] { 0, 0.5, 1, 0, 0.5, 1, 0, 2, 4, 2, 4, 6 }));
            Assert.That(sobekRRNwrwSetting.MaximumInfiltrationCapcaties, Is.EqualTo(new[] { 0, 2, 0, 5 }));
            Assert.That(sobekRRNwrwSetting.MinimumInfiltrationCapcaties, Is.EqualTo(new[] { 0, 0.5, 0, 1 }));
            Assert.That(sobekRRNwrwSetting.InfiltrationCapacityDecreases, Is.EqualTo(new[] { 0, 3, 0, 3 }));
            Assert.That(sobekRRNwrwSetting.InfiltrationCapacityIncreases, Is.EqualTo(new[] { 0, 0.1, 0, 0.1 }));
            Assert.That(sobekRRNwrwSetting.InfiltrationFromDepressions, Is.EqualTo(false));
            Assert.That(sobekRRNwrwSetting.InfiltrationFromRunoff, Is.EqualTo(true));
        }
        
        [Test]
        public void GivenPluvius3BDataLineFromFM1D2D_1547Model_WhenParseNwrw_ThenExpectedDataOnCorrectAreasIndex()
        {
            // arrange
            var line = "NWRW id 'l_0-1001--1002' sl -.475 ar  0  2966  0  0  0  0  0  0  0  0  11322  0 np  0 dw 'Default_DWA' ms '' nwrw";

            // act
            var sobekRRNwrwData = new SobekRRNwrwReader().Parse(line);
            var sobekRRNwrwSetting = sobekRRNwrwData.FirstOrDefault();

            // asserts
            Assert.That(sobekRRNwrwSetting, Is.Not.Null);
            Assert.That(sobekRRNwrwSetting.Id, Is.EqualTo("l_0-1001--1002"));
            Assert.That(sobekRRNwrwSetting.Areas.Length, Is.EqualTo(12));
            Assert.That(sobekRRNwrwSetting.Areas[1], Is.EqualTo(2966));//NwrwSurfaceType.ClosedPavedFlat
            Assert.That(sobekRRNwrwSetting.Areas[2], Is.EqualTo(0));//NwrwSurfaceType.ClosedPavedFlatStretch
        }
    }
}