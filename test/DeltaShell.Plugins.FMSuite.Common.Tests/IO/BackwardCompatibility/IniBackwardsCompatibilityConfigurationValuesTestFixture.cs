using System.Collections.Generic;
using Deltares.Infrastructure.IO.Ini.BackwardCompatibility;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility
{
    [TestFixture]
    public abstract class IniBackwardsCompatibilityConfigurationValuesTestFixture
    {
        protected abstract IEnumerable<string> ObsoleteProperties { get; }
        protected abstract IReadOnlyDictionary<string, string> ConditionalObsoleteProperties { get; }
        protected abstract IReadOnlyDictionary<string, NewPropertyData> LegacyPropertyMapping { get; }
        protected abstract IEnumerable<KeyValuePair<string, string>> LegacyCategoryMapping { get; }

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            IIniBackwardsCompatibilityConfigurationValues configurationValues = GetConfigurationValues();

            // Assert
            Assert.That(configurationValues.ObsoleteProperties, Is.EquivalentTo(ObsoleteProperties));
            Assert.That(configurationValues.ConditionalObsoleteProperties, Is.EquivalentTo(ConditionalObsoleteProperties));
            AssertThatLegacyPropertyMappingIsEquivalent(configurationValues.LegacyPropertyMapping, LegacyPropertyMapping);
            Assert.That(configurationValues.LegacySectionMapping, Is.EquivalentTo(LegacyCategoryMapping));
        }

        private static void AssertThatLegacyPropertyMappingIsEquivalent(
            IReadOnlyDictionary<string, NewPropertyData> actualMapping,
            IReadOnlyDictionary<string, NewPropertyData> expectedMapping
        )
        {
            Assert.That(actualMapping.Count, Is.EqualTo(expectedMapping.Count));

            foreach (KeyValuePair<string, NewPropertyData> keyValuePair in expectedMapping)
            {
                string legacyKey = keyValuePair.Key;
                NewPropertyData expectedNewData = keyValuePair.Value;

                if (!actualMapping.TryGetValue(legacyKey, out NewPropertyData actualNewData))
                {
                    Assert.Fail($"Expected legacy property `{legacyKey}` was not found in the actual mapping.");
                }

                Assert.That(actualNewData.Key, Is.EqualTo(expectedNewData.Key));
                Assert.That(actualNewData.Updater.GetType(), Is.EqualTo(expectedNewData.Updater.GetType()));
            }
        }

        protected abstract IIniBackwardsCompatibilityConfigurationValues GetConfigurationValues();
    }
}