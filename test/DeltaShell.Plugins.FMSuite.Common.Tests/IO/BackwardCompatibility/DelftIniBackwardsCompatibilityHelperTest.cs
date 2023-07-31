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

            var config = new TestConfigurationValues() { LegacyPropertyMapping = new Dictionary<string, NewPropertyData>() };

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
            var config = new TestConfigurationValues() { LegacyPropertyMapping = new Dictionary<string, NewPropertyData>() };

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
        public void IsConditionalObsoletePropertyName_PropertyNameNull_ThrowsArgumentNullException()
        {
            // Setup
            var config = new TestConfigurationValues() { ConditionalObsoleteProperties = new Dictionary<string, string>() };
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.IsConditionalObsoletePropertyName(null, new DelftIniCategory("randomName"));

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void IsConditionalObsoletePropertyName_CategoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var config = new TestConfigurationValues() { ConditionalObsoleteProperties = new Dictionary<string, string>() };
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => backwardsCompatibilityHelper.IsConditionalObsoletePropertyName("randomName", null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsConditionalObsoletePropertyName_MappingDoesNotContainProperty_ReturnsFalse()
        {
            // Setup
            const string propertyName = "property_to_check";
            var category = new DelftIniCategory("randomCategory");
            category.AddProperty(new DelftIniProperty(propertyName, string.Empty, string.Empty));

            var config = new TestConfigurationValues
            {
                ConditionalObsoleteProperties = new Dictionary<string, string>()
                {
                    { "legacy_property", "conditionalProperty" }
                }
            };
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);
            
            // Call
            bool isObsolete = backwardsCompatibilityHelper.IsConditionalObsoletePropertyName(propertyName, category);

            // Assert
            Assert.That(isObsolete, Is.False);
        }
        
        [Test]
        public void IsConditionalObsoletePropertyName_MappingContainsPropertyButCategoryDoesNotContainRequiredProperty_ReturnsFalse()
        {
            // Setup
            const string propertyName = "property_to_check";
            var category = new DelftIniCategory("randomCategory");
            category.AddProperty(new DelftIniProperty("randomProperty", string.Empty, string.Empty));

            var config = new TestConfigurationValues
            {
                ConditionalObsoleteProperties = new Dictionary<string, string>()
                {
                    { propertyName, "conditionalProperty" }
                }
            };
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);
            
            // Call
            bool isObsolete = backwardsCompatibilityHelper.IsConditionalObsoletePropertyName(propertyName, category);

            // Assert
            Assert.That(isObsolete, Is.False);
        }
        
        [Test]
        public void IsConditionalObsoletePropertyName_MappingContainsPropertyAndCategoryContainsRequiredProperty_ReturnsTrue()
        {
            // Setup
            const string propertyName = "property_to_check";
            const string conditionalPropertyName = "conditionalProperty";
            
            var category = new DelftIniCategory("randomCategory");
            category.AddProperty(new DelftIniProperty(conditionalPropertyName, string.Empty, string.Empty));

            var config = new TestConfigurationValues
            {
                ConditionalObsoleteProperties = new Dictionary<string, string>()
                {
                    { propertyName, conditionalPropertyName }
                }
            };
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);
            
            // Call
            bool isObsolete = backwardsCompatibilityHelper.IsConditionalObsoletePropertyName(propertyName, category);

            // Assert
            Assert.That(isObsolete, Is.True);
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
            const string obsoletePropertyName = "obsolete_property";
            var config = new TestConfigurationValues
            {
                ObsoleteProperties = new HashSet<string>
                {
                    obsoletePropertyName
                },
                ConditionalObsoleteProperties = new Dictionary<string, string>()
            };
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
        
        [Test]
        public void RemoveObsoletePropertiesWithWarning_RemovesConditionalObsoletePropertiesFromTheCategoryAndLogsWarning()
        {
            // Setup
            var category = new DelftIniCategory("randomCategory");
            
            const string obsoletePropertyName = "obsolete_property";
            var obsoleteProperty = new DelftIniProperty(obsoletePropertyName, string.Empty, string.Empty);
            category.AddProperty(obsoleteProperty);
            
            const string conditionalPropertyName = "conditionalObsoleteProperty";
            var conditionalObsoleteProperty = new DelftIniProperty(conditionalPropertyName, string.Empty, string.Empty);
            category.AddProperty(conditionalObsoleteProperty);

            var config = new TestConfigurationValues
            {
                ConditionalObsoleteProperties = new Dictionary<string, string>()
                {
                    { obsoletePropertyName, conditionalPropertyName }
                },
                ObsoleteProperties = new HashSet<string>()
            };
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(config);
            
            var logHandler = Substitute.For<ILogHandler>();
            
            // Call
            backwardsCompatibilityHelper.RemoveObsoletePropertiesWithWarning(category, logHandler);
            
            // Assert
            Assert.That(category.Properties, Does.Not.Contain(obsoleteProperty));
            logHandler.Received(1).ReportWarning($"Key {obsoletePropertyName} is deprecated and automatically removed from model.");

        }

        [Test]
        [TestCaseSource(nameof(GetArgumentNullCases))]
        public void UpdateProperty_ArgumentNull_ThrowsArgumentNullException(DelftIniProperty property,
                                                                            DelftIniCategory propertyCategory,
                                                                            ILogHandler logHandler)
        {
            // Setup
            var config = new TestConfigurationValues();
            var helper = new DelftIniBackwardsCompatibilityHelper(config);

            // Call
            void Call() => helper.UpdateProperty(property, propertyCategory, logHandler);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void UpdateProperty_WhenPropertyNameNotInLegacyPropertyMapping_DoesNotUpdateProperty()
        {
            // Setup
            var propertyUpdater = Substitute.For<IPropertyUpdater>();
            var legacyPropertyMapping = new Dictionary<string, NewPropertyData> { { "oldproperty", new NewPropertyData("newproperty", propertyUpdater) } };

            var config = new TestConfigurationValues() { LegacyPropertyMapping = legacyPropertyMapping };
            var helper = new DelftIniBackwardsCompatibilityHelper(config);

            const string randomNameNotInMapping = "randomName";
            var property = new DelftIniProperty(randomNameNotInMapping, string.Empty, string.Empty);
            var category = new DelftIniCategory(string.Empty);
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            helper.UpdateProperty(property, category, logHandler);

            // Assert
            propertyUpdater.DidNotReceiveWithAnyArgs().UpdateProperty(default, default, default, default);
        }

        [Test]
        public void UpdateProperty_UpdatesProperty()
        {
            // Setup
            const string randomNameInMapping = "oldproperty";
            const string randomNewName = "newname";

            var propertyUpdater = Substitute.For<IPropertyUpdater>();
            var legacyPropertyMapping = new Dictionary<string, NewPropertyData> { { randomNameInMapping, new NewPropertyData(randomNewName, propertyUpdater) } };

            var config = new TestConfigurationValues() { LegacyPropertyMapping = legacyPropertyMapping };
            var helper = new DelftIniBackwardsCompatibilityHelper(config);

            var property = new DelftIniProperty(randomNameInMapping, string.Empty, string.Empty);
            var category = new DelftIniCategory(string.Empty);
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            helper.UpdateProperty(property, category, logHandler);

            // Assert
            propertyUpdater.Received().UpdateProperty(property, randomNewName, category, logHandler);
        }

        private static IEnumerable<TestCaseData> GetArgumentNullCases()
        {
            var property = new DelftIniProperty(string.Empty, string.Empty, string.Empty);
            var category = new DelftIniCategory(string.Empty);
            var logHandler = Substitute.For<ILogHandler>();

            yield return new TestCaseData(null, category, logHandler);
            yield return new TestCaseData(property, null, logHandler);
            yield return new TestCaseData(property, category, null);
        }

        private sealed class TestConfigurationValues : IDelftIniBackwardsCompatibilityConfigurationValues
        {
            public ISet<string> ObsoleteProperties { get; set; }
            public IReadOnlyDictionary<string, string> ConditionalObsoleteProperties { get; set; }
            public IReadOnlyDictionary<string, NewPropertyData> LegacyPropertyMapping { get; set; }
            public IReadOnlyDictionary<string, string> LegacyCategoryMapping { get; set; }
        }

        private static IEnumerable<TestCaseData> GetUpdatedPropertyNameData()
        {
            const string expectedProperty = "expected_property";
            var testConfig = new TestConfigurationValues { LegacyPropertyMapping = new Dictionary<string, NewPropertyData> { { "legacy_property", new NewPropertyData(expectedProperty, new DefaultPropertyUpdater()) } } };

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
    }
}