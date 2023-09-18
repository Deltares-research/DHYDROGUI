using System;
using DeltaShell.NGHS.IO.FileReaders.BackwardCompatibility;
using DHYDRO.Common.IO.BackwardCompatibility;
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
            Assert.That(configurationValues.LegacyCategoryMapping, Is.Empty);
            Assert.That(configurationValues.UnsupportedPropertyValues, Has.Count.EqualTo(1));
            Assert.That(configurationValues.UnsupportedPropertyValues, Has.Exactly(1)
                                                                          .Matches(IsPropertyInfo("structure", "type", "extraresistance")));
        }

        private static Predicate<DelftIniPropertyInfo> IsPropertyInfo(string iniSection, string property, string value)
        {
            return info => info.Category.Equals(iniSection) &&
                           info.Property.Equals(property) &&
                           info.Value.Equals(value);
        }
    }
}