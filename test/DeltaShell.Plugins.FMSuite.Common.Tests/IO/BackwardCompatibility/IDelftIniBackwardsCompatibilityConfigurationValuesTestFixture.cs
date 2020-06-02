using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility
{
    [TestFixture]
    public abstract class IDelftIniBackwardsCompatibilityConfigurationValuesTestFixture
    {
        protected abstract IEnumerable<string> ObsoleteProperties { get; }
        protected abstract IEnumerable<KeyValuePair<string, string>> LegacyPropertyMapping { get; }
        protected abstract IEnumerable<KeyValuePair<string, string>> LegacyCategoryMapping { get; }

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            IDelftIniBackwardsCompatibilityConfigurationValues configurationValues = GetConfigurationValues();

            // Assert
            Assert.That(configurationValues.ObsoleteProperties, Is.EquivalentTo(ObsoleteProperties));
            Assert.That(configurationValues.LegacyPropertyMapping, Is.EquivalentTo(LegacyPropertyMapping));
            Assert.That(configurationValues.LegacyCategoryMapping, Is.EquivalentTo(LegacyCategoryMapping));
        }

        protected abstract IDelftIniBackwardsCompatibilityConfigurationValues GetConfigurationValues();
    }
}