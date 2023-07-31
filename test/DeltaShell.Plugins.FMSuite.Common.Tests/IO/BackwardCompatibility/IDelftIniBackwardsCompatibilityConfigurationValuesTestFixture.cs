using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility
{
    [TestFixture]
    public abstract class IDelftIniBackwardsCompatibilityConfigurationValuesTestFixture
    {
        protected abstract IEnumerable<string> ObsoleteProperties { get; }
        protected abstract IReadOnlyDictionary<string, string> ConditionalObsoleteProperties { get; }
        protected abstract IReadOnlyDictionary<string, NewPropertyData> LegacyPropertyMapping { get; }
        protected abstract IEnumerable<KeyValuePair<string, string>> LegacyCategoryMapping { get; }

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            IDelftIniBackwardsCompatibilityConfigurationValues configurationValues = GetConfigurationValues();

            // Assert
            Assert.That(configurationValues.ObsoleteProperties, Is.EquivalentTo(ObsoleteProperties));
            Assert.That(configurationValues.ConditionalObsoleteProperties, Is.EquivalentTo(ConditionalObsoleteProperties));
            AssertThatLegacyPropertyMappingIsEquivalent(configurationValues.LegacyPropertyMapping, LegacyPropertyMapping);
            Assert.That(configurationValues.LegacyCategoryMapping, Is.EquivalentTo(LegacyCategoryMapping));
        }

        private static void AssertThatLegacyPropertyMappingIsEquivalent(
            IReadOnlyDictionary<string, NewPropertyData> actualMapping,
            IReadOnlyDictionary<string, NewPropertyData> expectedMapping
        )
        {
            Assert.That(actualMapping.Count, Is.EqualTo(expectedMapping.Count));

            foreach (KeyValuePair<string, NewPropertyData> keyValuePair in expectedMapping)
            {
                string legacyName = keyValuePair.Key;
                NewPropertyData expectedNewData = keyValuePair.Value;

                if (!actualMapping.TryGetValue(legacyName, out NewPropertyData actualNewData))
                {
                    Assert.Fail($"Expected legacy property `{legacyName}` was not found in the actual mapping.");
                }

                Assert.That(actualNewData.Name, Is.EqualTo(expectedNewData.Name));
                Assert.That(actualNewData.Updater.GetType(), Is.EqualTo(expectedNewData.Updater.GetType()));
            }
        }

        protected abstract IDelftIniBackwardsCompatibilityConfigurationValues GetConfigurationValues();
    }
}