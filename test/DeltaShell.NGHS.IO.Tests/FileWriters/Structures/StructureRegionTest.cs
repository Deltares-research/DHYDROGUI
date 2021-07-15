using System.Collections.Generic;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class StructureRegionTest
    {
        [Test]
        [TestCaseSource(nameof(ConfigurationSettingsCases))]
        public void ConfigurationSetting_IsCorrect(ConfigurationSetting setting, string expKey, string expDefault, string expDescription, string expFormat)
        {
            // Assert
            Assert.That(setting.Key, Is.EqualTo(expKey));
            Assert.That(setting.DefaultValue, Is.EqualTo(expDefault));
            Assert.That(setting.Description, Is.EqualTo(expDescription));
            Assert.That(setting.Format, Is.EqualTo(expFormat));
        }

        private static IEnumerable<TestCaseData> ConfigurationSettingsCases()
        {
            yield return new TestCaseData(StructureRegion.Capacity, "capacity", null, "Pump capacity (m3/s)", "F4");
        }
    }
}