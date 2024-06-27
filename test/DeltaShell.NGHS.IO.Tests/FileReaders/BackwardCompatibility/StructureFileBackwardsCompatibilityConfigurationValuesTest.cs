using System;
using Deltares.Infrastructure.IO.Ini.BackwardCompatibility;
using DeltaShell.NGHS.IO.FileReaders.BackwardCompatibility;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.BackwardCompatibility
{
    [TestFixture]
    public class StructureFileBackwardsCompatibilityConfigurationValuesTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var configurationValues = new StructureFileBackwardsCompatibilityConfigurationValues();

            // Assert
            Assert.That(configurationValues.ObsoleteProperties, Is.Empty);
            Assert.That(configurationValues.LegacyPropertyMapping, Is.Empty);
            Assert.That(configurationValues.LegacySectionMapping, Is.Empty);
            Assert.That(configurationValues.UnsupportedPropertyValues, Has.Count.EqualTo(1));
            Assert.That(configurationValues.UnsupportedPropertyValues, Has.Exactly(1)
                                                                          .Matches(IsPropertyInfo("structure", "type", "extraresistance")));
        }

        private static Predicate<IniPropertyInfo> IsPropertyInfo(string iniSection, string property, string value)
        {
            return info => info.Section.Equals(iniSection) &&
                           info.Property.Equals(property) &&
                           info.Value.Equals(value);
        }
    }
}