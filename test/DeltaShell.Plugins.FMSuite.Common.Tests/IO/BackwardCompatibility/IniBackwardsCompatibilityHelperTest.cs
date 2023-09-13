using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility
{
    [TestFixture]
    public class IniBackwardsCompatibilityHelperTest
    {
        [Test]
        public void Constructor_ConfigNull_ThrowsArgumentNullException()
        {
            void Call() => new IniBackwardsCompatibilityHelper(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("configurationValues"));
        }

        [Test]
        public void GetUpdatedPropertyKey_NotInConfigMapping_ReturnsNull()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            const string propertyKey = "legacyProperty";

            var config = new TestConfigurationValues() { LegacyPropertyMapping = new Dictionary<string, NewPropertyData>() };

            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);

            // Call
            string result = backwardsCompatibilityHelper.GetUpdatedPropertyKey(propertyKey, logHandler);

            // Assert
            Assert.That(result, Is.Null);
            logHandler.DidNotReceiveWithAnyArgs().ReportWarningFormat("message");
        }

        [Test]
        [TestCaseSource(nameof(GetUpdatedPropertyKeyData))]
        public void GetUpdatedPropertyKey_InConfigMapping_ReturnsMappedValueAndLogsMessage(IIniBackwardsCompatibilityConfigurationValues configurationValues,
                                                                                           string legacyPropertyKey,
                                                                                           string expectedPropertyKey)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(configurationValues);

            // Call
            string result = backwardsCompatibilityHelper.GetUpdatedPropertyKey(legacyPropertyKey, logHandler);

            // Assert
            Assert.That(result, Is.EqualTo(expectedPropertyKey));
            logHandler.ReceivedWithAnyArgs(1).ReportWarningFormat("message");
        }

        [Test]
        public void GetUpdatedPropertyKey_PropertyKeyNull_ThrowsArgumentNullException()
        {
            // Setup
            var config = new TestConfigurationValues() { LegacyPropertyMapping = new Dictionary<string, NewPropertyData>() };

            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.GetUpdatedPropertyKey(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("propertyKey"));
        }

        [Test]
        public void GetUpdatedSectionName_NotInConfigMapping_ReturnsNull()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            const string sectionName = "legacyProperty";

            var config = new TestConfigurationValues() { LegacySectionMapping = new Dictionary<string, string>() };

            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);

            // Call
            string result = backwardsCompatibilityHelper.GetUpdatedSectionName(sectionName, logHandler);

            // Assert
            Assert.That(result, Is.Null);
            logHandler.DidNotReceiveWithAnyArgs().ReportWarningFormat("message");
        }

        [Test]
        [TestCaseSource(nameof(GetUpdatedSectionKeyData))]
        public void GetUpdatedSectionName_InConfigMapping_ReturnsMappedValue(IIniBackwardsCompatibilityConfigurationValues configurationValues,
                                                                             string legacySectionName,
                                                                             string expectedSectionName)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(configurationValues);

            // Call
            string result = backwardsCompatibilityHelper.GetUpdatedSectionName(legacySectionName, logHandler);

            // Assert
            Assert.That(result, Is.EqualTo(expectedSectionName));
            logHandler.ReceivedWithAnyArgs(1).ReportWarningFormat("message");
        }

        [Test]
        public void GetUpdatedSectionName_SectionNameNull_ThrowsArgumentNullException()
        {
            // Setup
            var config = new TestConfigurationValues() { LegacySectionMapping = new Dictionary<string, string>() };

            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.GetUpdatedSectionName(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("sectionName"));
        }

        [Test]
        [TestCaseSource(nameof(GetIsObsoletePropertyKeyData))]
        public void IsObsoletePropertyKey_ExpectedResults(IIniBackwardsCompatibilityConfigurationValues configurationValues,
                                                          string propertyKey,
                                                          bool expectedResult)
        {
            // Setup
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(configurationValues);

            // Call
            bool result = backwardsCompatibilityHelper.IsObsoletePropertyKey(propertyKey);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult),
                        "Expected a different result from IsObsoletePropertyKey");
        }

        [Test]
        public void IsObsoletePropertyKey_PropertyKeyNull_ThrowsArgumentNullException()
        {
            // Setup`
            var config = new TestConfigurationValues() { ObsoleteProperties = new HashSet<string>() };
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.IsObsoletePropertyKey(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("propertyKey"));
        }
        
        [Test]
        public void IsConditionalObsoletePropertyKey_PropertyKeyNull_ThrowsArgumentNullException()
        {
            // Setup
            var config = new TestConfigurationValues() { ConditionalObsoleteProperties = new Dictionary<string, string>() };
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.IsConditionalObsoletePropertyKey(null, new IniSection("randomName"));

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void IsConditionalObsoletePropertyKey_SectionNull_ThrowsArgumentNullException()
        {
            // Setup
            var config = new TestConfigurationValues() { ConditionalObsoleteProperties = new Dictionary<string, string>() };
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.IsConditionalObsoletePropertyKey("randomKey", null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsConditionalObsoletePropertyKey_MappingDoesNotContainProperty_ReturnsFalse()
        {
            // Setup
            const string propertyKey = "property_to_check";
            var section = new IniSection("randomSection");
            section.AddProperty(propertyKey, string.Empty);

            var config = new TestConfigurationValues
            {
                ConditionalObsoleteProperties = new Dictionary<string, string>()
                {
                    { "legacy_property", "conditionalProperty" }
                }
            };
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);
            
            // Call
            bool isObsolete = backwardsCompatibilityHelper.IsConditionalObsoletePropertyKey(propertyKey, section);

            // Assert
            Assert.That(isObsolete, Is.False);
        }
        
        [Test]
        public void IsConditionalObsoletePropertyKey_MappingContainsPropertyButSectionDoesNotContainRequiredProperty_ReturnsFalse()
        {
            // Setup
            const string propertyKey = "property_to_check";
            var section = new IniSection("randomSection");
            section.AddProperty("randomProperty", string.Empty);

            var config = new TestConfigurationValues
            {
                ConditionalObsoleteProperties = new Dictionary<string, string>()
                {
                    { propertyKey, "conditionalProperty" }
                }
            };
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);
            
            // Call
            bool isObsolete = backwardsCompatibilityHelper.IsConditionalObsoletePropertyKey(propertyKey, section);

            // Assert
            Assert.That(isObsolete, Is.False);
        }
        
        [Test]
        public void IsConditionalObsoletePropertyKey_MappingContainsPropertyAndSectionContainsRequiredProperty_ReturnsTrue()
        {
            // Setup
            const string propertyKey = "property_to_check";
            const string conditionalPropertyKey = "conditionalProperty";
            
            var section = new IniSection("randomSection");
            section.AddProperty(conditionalPropertyKey, string.Empty);

            var config = new TestConfigurationValues
            {
                ConditionalObsoleteProperties = new Dictionary<string, string>()
                {
                    { propertyKey, conditionalPropertyKey }
                }
            };
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);
            
            // Call
            bool isObsolete = backwardsCompatibilityHelper.IsConditionalObsoletePropertyKey(propertyKey, section);

            // Assert
            Assert.That(isObsolete, Is.True);
        }

        [Test]
        public void RemoveObsoletePropertiesWithWarning_SectionNull_ThrowsArgumentNullException()
        {
            // Setup
            var config = new TestConfigurationValues { ObsoleteProperties = new HashSet<string>() };
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            void Call() => backwardsCompatibilityHelper.RemoveObsoletePropertiesWithWarning(null, logHandler);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("section"));
        }

        [Test]
        public void RemoveObsoletePropertiesWithWarning_LogHandlerNull_ThrowsArgumentNullException()
        {
            // Setup
            var config = new TestConfigurationValues { ObsoleteProperties = new HashSet<string>() };
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);
            var section = new IniSection("section_name");

            // Call
            void Call() => backwardsCompatibilityHelper.RemoveObsoletePropertiesWithWarning(section, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("logHandler"));
        }

        [Test]
        public void RemoveObsoletePropertiesWithWarning_RemovesObsoletePropertiesFromTheSectionAndLogsWarning()
        {
            // Setup
            const string obsoletePropertyKey = "obsolete_property";
            var config = new TestConfigurationValues
            {
                ObsoleteProperties = new HashSet<string>
                {
                    obsoletePropertyKey
                },
                ConditionalObsoleteProperties = new Dictionary<string, string>()
            };
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);

            var obsoleteProperty = new IniProperty(obsoletePropertyKey, "property_value");
            var nonObsoleteProperty = new IniProperty("property_key", "property_value");

            var section = new IniSection("section_name");
            section.AddProperty(obsoleteProperty);
            section.AddProperty(nonObsoleteProperty);

            var logHandler = Substitute.For<ILogHandler>();

            // Call
            backwardsCompatibilityHelper.RemoveObsoletePropertiesWithWarning(section, logHandler);

            // Assert
            Assert.That(section.Properties, Does.Not.Contain(obsoleteProperty));
            logHandler.Received(1).ReportWarning($"Key {obsoletePropertyKey} is deprecated and automatically removed from model.");
        }
        
        [Test]
        public void RemoveObsoletePropertiesWithWarning_RemovesConditionalObsoletePropertiesFromTheSectionAndLogsWarning()
        {
            // Setup
            var section = new IniSection("randomSection");
            
            const string obsoletePropertyKey = "obsolete_property";
            var obsoleteProperty = new IniProperty(obsoletePropertyKey);
            section.AddProperty(obsoleteProperty);
            
            const string conditionalPropertyKey = "conditionalObsoleteProperty";
            var conditionalObsoleteProperty = new IniProperty(conditionalPropertyKey);
            section.AddProperty(conditionalObsoleteProperty);

            var config = new TestConfigurationValues
            {
                ConditionalObsoleteProperties = new Dictionary<string, string>()
                {
                    { obsoletePropertyKey, conditionalPropertyKey }
                },
                ObsoleteProperties = new HashSet<string>()
            };
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);
            
            var logHandler = Substitute.For<ILogHandler>();
            
            // Call
            backwardsCompatibilityHelper.RemoveObsoletePropertiesWithWarning(section, logHandler);
            
            // Assert
            Assert.That(section.Properties, Does.Not.Contain(obsoleteProperty));
            logHandler.Received(1).ReportWarning($"Key {obsoletePropertyKey} is deprecated and automatically removed from model.");

        }

        [Test]
        [TestCaseSource(nameof(GetArgumentNullCases))]
        public void UpdateProperty_ArgumentNull_ThrowsArgumentNullException(IniProperty property,
                                                                            IniSection section,
                                                                            ILogHandler logHandler)
        {
            // Setup
            var config = new TestConfigurationValues();
            var helper = new IniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => helper.UpdateProperty(property, section, logHandler);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void UpdateProperty_WhenPropertyKeyNotInLegacyPropertyMapping_DoesNotUpdateProperty()
        {
            // Setup
            var propertyUpdater = Substitute.For<IPropertyUpdater>();
            var legacyPropertyMapping = new Dictionary<string, NewPropertyData> { { "oldproperty", new NewPropertyData("newproperty", propertyUpdater) } };

            var config = new TestConfigurationValues() { LegacyPropertyMapping = legacyPropertyMapping };
            var helper = new IniBackwardsCompatibilityHelper(config);

            const string randomKeyNotInMapping = "randomKey";
            var property = new IniProperty(randomKeyNotInMapping);
            var section = new IniSection("TestSection");
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            helper.UpdateProperty(property, section, logHandler);

            // Assert
            propertyUpdater.DidNotReceiveWithAnyArgs().UpdateProperty(default, default, default, default);
        }

        [Test]
        public void UpdateProperty_UpdatesProperty()
        {
            // Setup
            const string randomKeyInMapping = "oldproperty";
            const string randomNewKey = "newkey";

            var propertyUpdater = Substitute.For<IPropertyUpdater>();
            var legacyPropertyMapping = new Dictionary<string, NewPropertyData> { { randomKeyInMapping, new NewPropertyData(randomNewKey, propertyUpdater) } };

            var config = new TestConfigurationValues() { LegacyPropertyMapping = legacyPropertyMapping };
            var helper = new IniBackwardsCompatibilityHelper(config);

            var property = new IniProperty(randomKeyInMapping);
            var section = new IniSection("TestSection");
            section.AddProperty(property);
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            helper.UpdateProperty(property, section, logHandler);

            // Assert
            propertyUpdater.Received().UpdateProperty(randomKeyInMapping, randomNewKey, section, logHandler);
        }

        private static IEnumerable<TestCaseData> GetArgumentNullCases()
        {
            var property = new IniProperty("TestProperty");
            var section = new IniSection("TestSection");
            var logHandler = Substitute.For<ILogHandler>();

            yield return new TestCaseData(null, section, logHandler);
            yield return new TestCaseData(property, null, logHandler);
            yield return new TestCaseData(property, section, null);
        }

        private sealed class TestConfigurationValues : IIniBackwardsCompatibilityConfigurationValues
        {
            public ISet<string> ObsoleteProperties { get; set; }
            public IReadOnlyDictionary<string, string> ConditionalObsoleteProperties { get; set; }
            public IReadOnlyDictionary<string, NewPropertyData> LegacyPropertyMapping { get; set; }
            public IReadOnlyDictionary<string, string> LegacySectionMapping { get; set; }
        }

        private static IEnumerable<TestCaseData> GetUpdatedPropertyKeyData()
        {
            const string expectedProperty = "expected_property";
            var testConfig = new TestConfigurationValues { LegacyPropertyMapping = new Dictionary<string, NewPropertyData> { { "legacy_property", new NewPropertyData(expectedProperty, new DefaultPropertyUpdater()) } } };

            yield return new TestCaseData(testConfig, "legacy_property", expectedProperty);
            yield return new TestCaseData(testConfig, "LEGACY_PROPERTY", expectedProperty);
            yield return new TestCaseData(testConfig, "legacy_PROPERTY", expectedProperty);
            yield return new TestCaseData(testConfig, "LeGaCy_PrOpErTy", expectedProperty);
            yield return new TestCaseData(testConfig, "legacy_Property", expectedProperty);
        }

        private static IEnumerable<TestCaseData> GetUpdatedSectionKeyData()
        {
            const string expectedSection = "expected_section";
            var testConfig = new TestConfigurationValues { LegacySectionMapping = new Dictionary<string, string> { { "legacy_section", expectedSection } } };

            yield return new TestCaseData(testConfig, "legacy_section", expectedSection);
            yield return new TestCaseData(testConfig, "LEGACY_SECTION", expectedSection);
            yield return new TestCaseData(testConfig, "legacy_SECTION", expectedSection);
            yield return new TestCaseData(testConfig, "LeGaCy_SeCtIoN", expectedSection);
            yield return new TestCaseData(testConfig, "legacy_Section", expectedSection);
        }

        private static IEnumerable<TestCaseData> GetIsObsoletePropertyKeyData()
        {
            const string propertyKey = "propertyKey";

            var emptyConfig = new TestConfigurationValues { ObsoleteProperties = new HashSet<string>() };
            yield return new TestCaseData(emptyConfig,
                                          propertyKey,
                                          false);
            var configWithoutPropertyKey = new TestConfigurationValues
            {
                ObsoleteProperties = new HashSet<string>()
                {
                    "otherkey",
                    "otterKey"
                }
            };
            yield return new TestCaseData(configWithoutPropertyKey,
                                          propertyKey,
                                          false);

            var configWithOnlyPropertyKey =
                new TestConfigurationValues { ObsoleteProperties = new HashSet<string> { propertyKey.ToLowerInvariant() } };
            yield return new TestCaseData(configWithOnlyPropertyKey,
                                          propertyKey,
                                          true);

            var configWithPropertyKey = new TestConfigurationValues
            {
                ObsoleteProperties = new HashSet<string>
                {
                    propertyKey.ToLowerInvariant(),
                    "otherkey",
                    "otterkey"
                }
            };

            yield return new TestCaseData(configWithPropertyKey,
                                          propertyKey,
                                          true);
            yield return new TestCaseData(configWithPropertyKey,
                                          "PROPERTYKEY",
                                          true);
            yield return new TestCaseData(configWithPropertyKey,
                                          "pRoPeRtYkEy",
                                          true);
        }
    }
}