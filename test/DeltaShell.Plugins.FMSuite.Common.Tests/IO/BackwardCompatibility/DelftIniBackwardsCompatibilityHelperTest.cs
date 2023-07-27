using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility
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

            var config = new TestConfigurationValues() {LegacyPropertyMapping = new Dictionary<string, string>()};

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
            var config = new TestConfigurationValues() {LegacyPropertyMapping = new Dictionary<string, string>()};

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

            var config = new TestConfigurationValues() {LegacyCategoryMapping = new Dictionary<string, string>()};

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
            var config = new TestConfigurationValues() {LegacyCategoryMapping = new Dictionary<string, string>()};

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
            var config = new TestConfigurationValues() {ObsoleteProperties = new HashSet<string>()};
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.IsObsoletePropertyName(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("propertyName"));
        }

        [Test]
        public void RemoveObsoletePropertiesWithWarning_CategoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var config = new TestConfigurationValues { ObsoleteProperties = new HashSet<string>() };
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            void Call() => backwardsCompatibilityHelper.RemoveObsoletePropertiesWithWarning(null, logHandler);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("category"));
        }

        [Test]
        public void RemoveObsoletePropertiesWithWarning_LogHandlerNull_ThrowsArgumentNullException()
        {
            // Setup
            var config = new TestConfigurationValues { ObsoleteProperties = new HashSet<string>() };
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);
            var category = new DelftIniCategory("category_name");

            // Call
            void Call() => backwardsCompatibilityHelper.RemoveObsoletePropertiesWithWarning(category, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("logHandler"));
        }

        [Test]
        public void RemoveObsoletePropertiesWithWarning_RemovesObsoletePropertiesFromTheCategoryAndLogsWarning()
        {
            // Setup
            var obsoletePropertyName = "obsolete_property";
            var config = new TestConfigurationValues { ObsoleteProperties = new HashSet<string> { obsoletePropertyName } };
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);

            var obsoleteProperty = new DelftIniProperty(obsoletePropertyName, "property_value", "property_comment");
            var nonObsoleteProperty = new DelftIniProperty("property_name", "property_value", "property_comment");

            var category = new DelftIniCategory("category_name");
            category.AddProperty(obsoleteProperty);
            category.AddProperty(nonObsoleteProperty);

            var logHandler = Substitute.For<ILogHandler>();

            // Call
            backwardsCompatibilityHelper.RemoveObsoletePropertiesWithWarning(category, logHandler);

            // Assert
            Assert.That(category.Properties, Does.Not.Contain(obsoleteProperty));
            logHandler.Received(1).ReportWarning($"Key {obsoletePropertyName} is deprecated and automatically removed from model.");
        }

        private sealed class TestConfigurationValues : IDelftIniBackwardsCompatibilityConfigurationValues
        {
            public ISet<string> ObsoleteProperties { get; set; }
            public IReadOnlyDictionary<string, string> LegacyPropertyMapping { get; set; }
            public IReadOnlyDictionary<string, string> LegacyCategoryMapping { get; set; }
        }

        private static IEnumerable<TestCaseData> GetUpdatedPropertyNameData()
        {
            const string expectedProperty = "expected_property";
            var testConfig = new TestConfigurationValues {LegacyPropertyMapping = new Dictionary<string, string> {{"legacy_property", expectedProperty}}};

            yield return new TestCaseData(testConfig, "legacy_property", expectedProperty);
            yield return new TestCaseData(testConfig, "LEGACY_PROPERTY", expectedProperty);
            yield return new TestCaseData(testConfig, "legacy_PROPERTY", expectedProperty);
            yield return new TestCaseData(testConfig, "LeGaCy_PrOpErTy", expectedProperty);
            yield return new TestCaseData(testConfig, "legacy_Property", expectedProperty);
        }

        private static IEnumerable<TestCaseData> GetUpdatedCategoryNameData()
        {
            const string expectedCategory = "expected_category";
            var testConfig = new TestConfigurationValues {LegacyCategoryMapping = new Dictionary<string, string> {{"legacy_category", expectedCategory}}};

            yield return new TestCaseData(testConfig, "legacy_category", expectedCategory);
            yield return new TestCaseData(testConfig, "LEGACY_CATEGORY", expectedCategory);
            yield return new TestCaseData(testConfig, "legacy_CATEGORY", expectedCategory);
            yield return new TestCaseData(testConfig, "LeGaCy_CaTeGoRy", expectedCategory);
            yield return new TestCaseData(testConfig, "legacy_Category", expectedCategory);
        }

        private static IEnumerable<TestCaseData> GetIsObsoletePropertyNameData()
        {
            const string propertyName = "propertyName";

            var emptyConfig = new TestConfigurationValues {ObsoleteProperties = new HashSet<string>()};
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
                new TestConfigurationValues {ObsoleteProperties = new HashSet<string> {propertyName.ToLowerInvariant()}};
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
    }
}