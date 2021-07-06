using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class StructureRegionTest
    {
        [Test]
        public void Capacity_IsCorrectConfigurationSetting()
        {
            // Call
            ConfigurationSetting capacitySettings = StructureRegion.Capacity;

            // Assert
            Assert.That(capacitySettings.Key, Is.EqualTo("capacity"));
            Assert.That(capacitySettings.DefaultValue, Is.Null);
            Assert.That(capacitySettings.Description, Is.EqualTo("Pump capacity (m3/s)"));
            Assert.That(capacitySettings.Format, Is.EqualTo("F4"));
        }
    }
}