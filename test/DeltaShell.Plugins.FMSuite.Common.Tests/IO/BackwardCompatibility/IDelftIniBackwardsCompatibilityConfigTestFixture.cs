using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility
{
    [TestFixture]
    public abstract class IDelftIniBackwardsCompatibilityConfigTestFixture
    {
        protected abstract IDelftIniBackwardsCompatibilityConfig GetConfig();
        protected abstract IEnumerable<string> ObsoleteProperties { get; }
        protected abstract IEnumerable<KeyValuePair<string, string>> LegacyPropertyMapping { get; }
        protected abstract IEnumerable<KeyValuePair<string, string>> LegacyCategoryMapping { get; }

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            IDelftIniBackwardsCompatibilityConfig config = GetConfig();

            // Assert
            Assert.That(config.ObsoleteProperties, Is.EquivalentTo(ObsoleteProperties));
            Assert.That(config.LegacyPropertyMapping, Is.EquivalentTo(LegacyPropertyMapping));
            Assert.That(config.LegacyCategoryMapping, Is.EquivalentTo(LegacyCategoryMapping));
        }
    }
}