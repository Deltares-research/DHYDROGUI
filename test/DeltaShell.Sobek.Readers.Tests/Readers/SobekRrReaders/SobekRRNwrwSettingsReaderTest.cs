using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;
using NUnit.Framework.Constraints;

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
    }
}