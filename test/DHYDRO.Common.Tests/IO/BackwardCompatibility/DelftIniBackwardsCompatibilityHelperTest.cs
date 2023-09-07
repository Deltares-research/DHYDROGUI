using System;
using System.Collections.Generic;
using DHYDRO.Common.IO.BackwardCompatibility;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BackwardCompatibility
{
    [TestFixture]
    public class DelftIniBackwardsCompatibilityHelperTest
    {
        [Test]
        public void Constructor_ConfigNull_ThrowsArgumentNullException()
        {
            void Call() => new DelftIniBackwardsCompatibilityHelper(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("configurationValues"));
        }

        [Test]
        public void GetUpdatedPropertyName_NotInConfigMapping_ReturnsNull()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            const string propertyName = "legacyProperty";

            var config = new TestConfigurationValues() { LegacyPropertyMapping = new Dictionary<string, string>() };

            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);

            // Call
            string result = backwardsCompatibilityHelper.GetUpdatedPropertyName(propertyName, logHandler);

            // Assert
            Assert.That(result, Is.Null);
            logHandler.DidNotReceiveWithAnyArgs().ReportWarningFormat("message");
        }

        [Test]
        [TestCaseSource(nameof(GetUpdatedPropertyNameData))]
        public void GetUpdatedPropertyName_InConfigMapping_ReturnsMappedValueAndLogsMessage(IDelftIniBackwardsCompatibilityConfigurationValues configurationValues,
                                                                                            string legacyPropertyName,
                                                                                            string expectedPropertyName)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(configurationValues);

            // Call
            string result = backwardsCompatibilityHelper.GetUpdatedPropertyName(legacyPropertyName, logHandler);

            // Assert
            Assert.That(result, Is.EqualTo(expectedPropertyName));
            logHandler.ReceivedWithAnyArgs(1).ReportWarningFormat("message");
        }

        [Test]
        public void GetUpdatedPropertyName_PropertyNameNull_ThrowsArgumentNullException()
        {
            // Setup
            var config = new TestConfigurationValues() { LegacyPropertyMapping = new Dictionary<string, string>() };

            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.GetUpdatedPropertyName(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("propertyName"));
        }

        [Test]
        public void GetUpdatedCategoryName_NotInConfigMapping_ReturnsNull()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            const string categoryName = "legacyProperty";

            var config = new TestConfigurationValues() { LegacyCategoryMapping = new Dictionary<string, string>() };

            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);

            // Call
            string result = backwardsCompatibilityHelper.GetUpdatedCategoryName(categoryName, logHandler);

            // Assert
            Assert.That(result, Is.Null);
            logHandler.DidNotReceiveWithAnyArgs().ReportWarningFormat("message");
        }

        [Test]
        [TestCaseSource(nameof(GetUpdatedCategoryNameData))]
        public void GetUpdatedCategoryName_InConfigMapping_ReturnsMappedValue(IDelftIniBackwardsCompatibilityConfigurationValues configurationValues,
                                                                              string legacyCategoryName,
                                                                              string expectedCategoryName)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(configurationValues);

            // Call
            string result = backwardsCompatibilityHelper.GetUpdatedCategoryName(legacyCategoryName, logHandler);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCategoryName));
            logHandler.ReceivedWithAnyArgs(1).ReportWarningFormat("message");
        }

        [Test]
        public void GetUpdatedCategoryName_CategoryNameNull_ThrowsArgumentNullException()
        {
            // Setup
            var config = new TestConfigurationValues() { LegacyCategoryMapping = new Dictionary<string, string>() };

            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.GetUpdatedCategoryName(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("categoryName"));
        }

        [Test]
        [TestCaseSource(nameof(GetIsObsoletePropertyNameData))]
        public void IsObsoletePropertyName_ExpectedResults(IDelftIniBackwardsCompatibilityConfigurationValues configurationValues,
                                                           string propertyName,
                                                           bool expectedResult)
        {
            // Setup
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(configurationValues);

            // Call
            bool result = backwardsCompatibilityHelper.IsObsoletePropertyName(propertyName);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult),
                        "Expected a different result from IsObsoletePropertyName");
        }

        [Test]
        public void IsObsoletePropertyName_PropertyNameNull_ThrowsArgumentNullException()
        {
            // Setup`
            var config = new TestConfigurationValues() { ObsoleteProperties = new HashSet<string>() };
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.IsObsoletePropertyName(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("propertyName"));
        }

        [Test]
        [TestCaseSource(nameof(IsUnsupportedPropertyValue_ArgNullCases))]
        public void IsUnsupportedPropertyValue_ArgNull_ThrowArgumentNullException(string category, string property, string value, string expParamName)
        {
            // Setup
            var config = new TestConfigurationValues();
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.IsUnsupportedPropertyValue(category, property, value);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        [TestCaseSource(nameof(IsUnsupportedPropertyValueCases))]
        public void IsUnsupportedPropertyValue_ArgNull_ThrowArgumentNullException(string category, string property, string value, bool expResult)
        {
            // Setup
            var config = new TestConfigurationValues { UnsupportedPropertyValues = new[] { new DelftIniPropertyInfo("some_category", "some_property", "some_value") } };
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);

            // Call
            bool result = backwardsCompatibilityHelper.IsUnsupportedPropertyValue(category, property, value);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        private sealed class TestConfigurationValues : IDelftIniBackwardsCompatibilityConfigurationValues
        {
            public ISet<string> ObsoleteProperties { get; set; }
            public IReadOnlyDictionary<string, string> LegacyPropertyMapping { get; set; }
            public IReadOnlyDictionary<string, string> LegacyCategoryMapping { get; set; }
            public IEnumerable<DelftIniPropertyInfo> UnsupportedPropertyValues { get; set; }
        }

        private static IEnumerable<TestCaseData> GetUpdatedPropertyNameData()
        {
            const string expectedProperty = "expected_property";
            var testConfig = new TestConfigurationValues { LegacyPropertyMapping = new Dictionary<string, string> { { "legacy_property", expectedProperty } } };

            yield return new TestCaseData(testConfig, "legacy_property", expectedProperty);
            yield return new TestCaseData(testConfig, "LEGACY_PROPERTY", expectedProperty);
            yield return new TestCaseData(testConfig, "legacy_PROPERTY", expectedProperty);
            yield return new TestCaseData(testConfig, "LeGaCy_PrOpErTy", expectedProperty);
            yield return new TestCaseData(testConfig, "legacy_Property", expectedProperty);
        }

        private static IEnumerable<TestCaseData> GetUpdatedCategoryNameData()
        {
            const string expectedCategory = "expected_category";
            var testConfig = new TestConfigurationValues { LegacyCategoryMapping = new Dictionary<string, string> { { "legacy_category", expectedCategory } } };

            yield return new TestCaseData(testConfig, "legacy_category", expectedCategory);
            yield return new TestCaseData(testConfig, "LEGACY_CATEGORY", expectedCategory);
            yield return new TestCaseData(testConfig, "legacy_CATEGORY", expectedCategory);
            yield return new TestCaseData(testConfig, "LeGaCy_CaTeGoRy", expectedCategory);
            yield return new TestCaseData(testConfig, "legacy_Category", expectedCategory);
        }

        private static IEnumerable<TestCaseData> GetIsObsoletePropertyNameData()
        {
            const string propertyName = "propertyName";

            var emptyConfig = new TestConfigurationValues { ObsoleteProperties = new HashSet<string>() };
            yield return new TestCaseData(emptyConfig,
                                          propertyName,
                                          false);
            var configWithoutPropertyName = new TestConfigurationValues
            {
                ObsoleteProperties = new HashSet<string>()
                {
                    "othername",
                    "otterName"
                }
            };
            yield return new TestCaseData(configWithoutPropertyName,
                                          propertyName,
                                          false);

            var configWithOnlyPropertyName =
                new TestConfigurationValues { ObsoleteProperties = new HashSet<string> { propertyName.ToLowerInvariant() } };
            yield return new TestCaseData(configWithOnlyPropertyName,
                                          propertyName,
                                          true);

            var configWithPropertyName = new TestConfigurationValues
            {
                ObsoleteProperties = new HashSet<string>
                {
                    propertyName.ToLowerInvariant(),
                    "othername",
                    "ottername"
                }
            };

            yield return new TestCaseData(configWithPropertyName,
                                          propertyName,
                                          true);
            yield return new TestCaseData(configWithPropertyName,
                                          "PROPERTYNAME",
                                          true);
            yield return new TestCaseData(configWithPropertyName,
                                          "pRoPeRtYnAmE",
                                          true);
        }

        private static IEnumerable<TestCaseData> IsUnsupportedPropertyValue_ArgNullCases()
        {
            yield return ToData(null, "some_property", "some_value", "category");
            yield return ToData("some_category", null, "some_value", "property");
            yield return ToData("some_category", "some_property", null, "value");

            TestCaseData ToData(string category, string property, string value, string expParamName)
                => new TestCaseData(category, property, value, expParamName).SetName(expParamName);
        }

        private static IEnumerable<TestCaseData> IsUnsupportedPropertyValueCases()
        {
            yield return new TestCaseData("some_category", "some_property", "some_value", true);
            yield return new TestCaseData("Some_Category", "Some_Property", "Some_Value", true);
            yield return new TestCaseData("some_other_category", "some_property", "some_value", false);
            yield return new TestCaseData("some_category", "some_other_property", "some_value", false);
            yield return new TestCaseData("some_category", "some_property", "some_other_value", false);
        }
    }
}