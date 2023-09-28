using System;
using System.Collections.Generic;
using DHYDRO.Common.IO.Ini.BackwardCompatibility;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.Ini.BackwardCompatibility
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

            var config = new TestConfigurationValues() { LegacyPropertyMapping = new Dictionary<string, string>() };

            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);

            // Call
            string result = backwardsCompatibilityHelper.GetUpdatedPropertyKey(propertyKey, logHandler);

            // Assert
            Assert.That(result, Is.Null);
            logHandler.DidNotReceiveWithAnyArgs().ReportWarningFormat("message");
        }

        [Test]
        [TestCaseSource(nameof(GetUpdatedPropertyKeyata))]
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
            var config = new TestConfigurationValues() { LegacyPropertyMapping = new Dictionary<string, string>() };

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
        [TestCaseSource(nameof(GetUpdatedSectionNameData))]
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
        [TestCaseSource(nameof(IsUnsupportedPropertyValue_ArgNullCases))]
        public void IsUnsupportedPropertyValue_ArgNull_ThrowArgumentNullException(string section, string property, string value, string expParamName)
        {
            // Setup
            var config = new TestConfigurationValues();
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.IsUnsupportedPropertyValue(section, property, value);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        [TestCaseSource(nameof(IsUnsupportedPropertyValueCases))]
        public void IsUnsupportedPropertyValue_ArgNull_ThrowArgumentNullException(string section, string property, string value, bool expResult)
        {
            // Setup
            var config = new TestConfigurationValues { UnsupportedPropertyValues = new[] { new IniPropertyInfo("some_section", "some_property", "some_value") } };
            var backwardsCompatibilityHelper = new IniBackwardsCompatibilityHelper(config);

            // Call
            bool result = backwardsCompatibilityHelper.IsUnsupportedPropertyValue(section, property, value);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        private sealed class TestConfigurationValues : IIniBackwardsCompatibilityConfigurationValues
        {
            public ISet<string> ObsoleteProperties { get; set; }
            public IReadOnlyDictionary<string, string> LegacyPropertyMapping { get; set; }
            public IReadOnlyDictionary<string, string> LegacySectionMapping { get; set; }
            public IEnumerable<IniPropertyInfo> UnsupportedPropertyValues { get; set; }
        }

        private static IEnumerable<TestCaseData> GetUpdatedPropertyKeyata()
        {
            const string expectedProperty = "expected_property";
            var testConfig = new TestConfigurationValues { LegacyPropertyMapping = new Dictionary<string, string> { { "legacy_property", expectedProperty } } };

            yield return new TestCaseData(testConfig, "legacy_property", expectedProperty);
            yield return new TestCaseData(testConfig, "LEGACY_PROPERTY", expectedProperty);
            yield return new TestCaseData(testConfig, "legacy_PROPERTY", expectedProperty);
            yield return new TestCaseData(testConfig, "LeGaCy_PrOpErTy", expectedProperty);
            yield return new TestCaseData(testConfig, "legacy_Property", expectedProperty);
        }

        private static IEnumerable<TestCaseData> GetUpdatedSectionNameData()
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

        private static IEnumerable<TestCaseData> IsUnsupportedPropertyValue_ArgNullCases()
        {
            yield return ToData(null, "some_property", "some_value", "sectionName");
            yield return ToData("some_section", null, "some_value", "propertyKey");
            yield return ToData("some_section", "some_property", null, "value");

            TestCaseData ToData(string section, string property, string value, string expParamName)
                => new TestCaseData(section, property, value, expParamName).SetName(expParamName);
        }

        private static IEnumerable<TestCaseData> IsUnsupportedPropertyValueCases()
        {
            yield return new TestCaseData("some_section", "some_property", "some_value", true);
            yield return new TestCaseData("Some_Section", "Some_Property", "Some_Value", true);
            yield return new TestCaseData("some_other_section", "some_property", "some_value", false);
            yield return new TestCaseData("some_section", "some_other_property", "some_value", false);
            yield return new TestCaseData("some_section", "some_property", "some_other_value", false);
        }
    }
}