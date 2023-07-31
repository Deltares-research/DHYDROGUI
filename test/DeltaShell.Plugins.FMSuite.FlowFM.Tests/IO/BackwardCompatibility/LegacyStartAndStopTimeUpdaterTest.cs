using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.BackwardCompatibility
{
    [TestFixture]
    public class LegacyStartAndStopTimeUpdaterTest
    {
        [Test]
        [TestCaseSource(nameof(GetArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(DelftIniProperty legacyProperty,
                                                                         string newPropertyName,
                                                                         DelftIniCategory legacyPropertyCategory,
                                                                         ILogHandler logHandler)
        {
            // Setup
            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            void Call() => updater.UpdateProperty(legacyProperty, newPropertyName, legacyPropertyCategory, logHandler);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void UpdateProperty_WrongLegacyProperty_DoesNotUpdateProperty()
        {
            // Setup
            const string name = "ThisNameIsNotTStartOrTStop";
            const string value = "randomValue";

            var logHandler = Substitute.For<ILogHandler>();
            var unsupportedProperty = new DelftIniProperty(name, value, string.Empty);

            const string newName = "newName";
            var category = new DelftIniCategory(string.Empty);

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            updater.UpdateProperty(unsupportedProperty, newName, category, logHandler);

            // Assert
            Assert.That(unsupportedProperty.Name, Is.EqualTo(name));
            Assert.That(unsupportedProperty.Value, Is.EqualTo(value));
        }

        [Test]
        [TestCaseSource(nameof(GetValidPropertyNameCases))]
        public void UpdateProperty_UpdatesPropertyNameAndLogsWarning(string propertyName, string newPropertyName)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            var updater = new LegacyStartAndStopTimeUpdater();

            var property = new DelftIniProperty(propertyName, "1", string.Empty);
            var category = new DelftIniCategory(string.Empty);
            category.AddProperty(new DelftIniProperty(KnownProperties.RefDate, "19900718", string.Empty));
            category.AddProperty(new DelftIniProperty(KnownProperties.Tunit, "S", string.Empty));

            // Call
            updater.UpdateProperty(property, newPropertyName, category, logHandler);

            // Assert
            Assert.That(property.Name, Is.EqualTo(newPropertyName));

            const string expectedLogMessage = "Backwards Compatibility: '{0}' has been updated to '{1}'";
            logHandler.Received(1).ReportWarningFormat(expectedLogMessage, propertyName, newPropertyName);
        }

        [Test]
        [TestCaseSource(nameof(GetValidPropertyNameCases))]
        public void UpdateProperty_MissingTUnit_ThrowsInvalidOperationException(string propertyName, string newPropertyName)
        {
            // Setup
            const string originalValue = "10";
            var logHandler = Substitute.For<ILogHandler>();
            var property = new DelftIniProperty(propertyName, originalValue, string.Empty);

            DelftIniCategory category = CreateCategoryWithRefDate();

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            void Call() => updater.UpdateProperty(property, newPropertyName, category, logHandler);

            // Assert
            var expectedMessage = $"The keyword `{KnownProperties.Tunit}` is missing in the mdu file.";
            Assert.That(Call, Throws.InvalidOperationException.With.Message.EqualTo(expectedMessage));
        }

        [Test]
        [TestCaseSource(nameof(GetValidPropertyNameCases))]
        public void UpdateProperty_MissingTUnitValue_UsesSecondsAsDefaultValue(string propertyName, string newPropertyName)
        {
            // Setup
            const string originalValue = "10";
            var logHandler = Substitute.For<ILogHandler>();
            var property = new DelftIniProperty(propertyName, originalValue, string.Empty);

            DelftIniCategory category = CreateCategoryWithRefDateAndTUnit();
            DelftIniProperty tUnitProperty = category.Properties.First(p => p.Name.EqualsCaseInsensitive(KnownProperties.Tunit));
            tUnitProperty.Value = null;

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            updater.UpdateProperty(property, newPropertyName, category, logHandler);

            // Assert
            const string expectedTUnitValue = "S"; // Updater should set the value to S if it is missing!
            Assert.That(tUnitProperty.Value, Is.EqualTo(expectedTUnitValue));
            
            const string expectedValue = "19900718000010"; // refdate = 1990-07-18, TStart/TStop = 10 and TUnit = S
            Assert.That(property.Value, Is.EqualTo(expectedValue));

            const string expectedLogMessage = "Backwards Compatibility: Value for '{0}' has been updated from '{1}' to '{2}'";
            logHandler.Received(1).ReportWarningFormat(expectedLogMessage, property.Name, originalValue, expectedValue);
        }

        [Test]
        [TestCaseSource(nameof(GetValidPropertyNameCases))]
        public void UpdateProperty_MissingRefDate_ThrowsInvalidOperationException(string propertyName, string newPropertyName)
        {
            // Setup
            const string originalValue = "10";
            var logHandler = Substitute.For<ILogHandler>();
            var property = new DelftIniProperty(propertyName, originalValue, string.Empty);

            DelftIniCategory category = CreateCategoryWithTUnit();

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            void Call() => updater.UpdateProperty(property, newPropertyName, category, logHandler);

            // Assert
            var expectedMessage = $"The keyword `{KnownProperties.RefDate}` is missing in the mdu file.";
            Assert.That(Call, Throws.InvalidOperationException.With.Message.EqualTo(expectedMessage));
        }

        [Test]
        [TestCaseSource(nameof(GetValidPropertyNameCases))]
        public void UpdateProperty_MissingRefDateValue_ThrowsInvalidOperationException(string propertyName, string newPropertyName)
        {
            // Setup
            const string originalValue = "10";
            var logHandler = Substitute.For<ILogHandler>();
            var property = new DelftIniProperty(propertyName, originalValue, string.Empty);

            DelftIniCategory category = CreateCategoryWithRefDateAndTUnit();
            DelftIniProperty tUnitProperty = category.Properties.First(p => p.Name.EqualsCaseInsensitive(KnownProperties.RefDate));
            tUnitProperty.Value = null;

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            void Call() => updater.UpdateProperty(property, newPropertyName, category, logHandler);

            // Assert
            var expectedMessage = $"The value for the required keyword `{KnownProperties.RefDate}` is missing in the mdu file.";
            Assert.That(Call, Throws.InvalidOperationException.With.Message.EqualTo(expectedMessage));
        }

        [Test]
        [TestCaseSource(nameof(GetValidPropertyNameCases))]
        public void UpdateProperty_UpdatesPropertyValueAndLogsMessage(string propertyName, string newPropertyName)
        {
            // Setup
            const string originalValue = "10";
            var logHandler = Substitute.For<ILogHandler>();
            var property = new DelftIniProperty(propertyName, originalValue, string.Empty);

            DelftIniCategory category = CreateCategoryWithRefDateAndTUnit();
            category.AddProperty(property);

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            updater.UpdateProperty(property, newPropertyName, category, logHandler);

            // Assert
            Assert.That(property.Name, Is.EqualTo(newPropertyName));

            const string expectedValue = "19900718000010"; // refdate = 1990-07-18, TStart/TStop = 10 and TUnit = S
            Assert.That(property.Value, Is.EqualTo(expectedValue));

            const string expectedLogMessage = "Backwards Compatibility: Value for '{0}' has been updated from '{1}' to '{2}'";
            logHandler.Received(1).ReportWarningFormat(expectedLogMessage, property.Name, originalValue, expectedValue);
        }

        private static IEnumerable<TestCaseData> GetArgumentNullCases()
        {
            var property = new DelftIniProperty(string.Empty, string.Empty, string.Empty);
            var newPropertyName = string.Empty;
            var category = new DelftIniCategory(string.Empty);
            var logHandler = Substitute.For<ILogHandler>();

            yield return new TestCaseData(null, newPropertyName, category, logHandler);
            yield return new TestCaseData(property, null, category, logHandler);
            yield return new TestCaseData(property, newPropertyName, null, logHandler);
            yield return new TestCaseData(property, newPropertyName, category, null);
        }

        private static IEnumerable<TestCaseData> GetValidPropertyNameCases()
        {
            yield return new TestCaseData(KnownLegacyProperties.TStart, KnownProperties.StartDateTime);
            yield return new TestCaseData(KnownLegacyProperties.TStop, KnownProperties.StopDateTime);
        }

        private static DelftIniCategory CreateCategoryWithRefDate()
        {
            var category = new DelftIniCategory("randomCategoryName");

            category.AddProperty(CreateProperty("RefDate", "19900718"));

            return category;
        }

        private static DelftIniCategory CreateCategoryWithTUnit()
        {
            var category = new DelftIniCategory("randomCategoryName");

            category.AddProperty(CreateProperty("TUnit", "S"));

            return category;
        }

        private static DelftIniCategory CreateCategoryWithRefDateAndTUnit()
        {
            var category = new DelftIniCategory("randomCategoryName");

            category.AddProperty(CreateProperty("TUnit", "S"));
            category.AddProperty(CreateProperty("RefDate", "19900718"));

            return category;
        }

        private static DelftIniProperty CreateProperty(string name, string value)
        {
            return new DelftIniProperty(name, value, string.Empty);
        }
    }
}