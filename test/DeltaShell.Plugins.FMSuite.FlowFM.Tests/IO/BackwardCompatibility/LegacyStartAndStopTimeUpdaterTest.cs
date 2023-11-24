using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.Ini;
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
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(string oldPropertyKey,
                                                                         string newPropertyKey,
                                                                         IniSection section,
                                                                         ILogHandler logHandler)
        {
            // Setup
            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            void Call() => updater.UpdateProperty(oldPropertyKey, newPropertyKey, section, logHandler);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        [TestCaseSource(nameof(GetValidPropertyKeyCases))]
        public void UpdateProperty_MissingLegacyProperty_ThrowsInvalidOperationException(string oldPropertyKey, string newPropertyKey)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            IniSection section = CreateSectionWithRefDate();

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            void Call() => updater.UpdateProperty(oldPropertyKey, newPropertyKey, section, logHandler);

            // Assert
            var expectedMessage = $"The keyword `{oldPropertyKey}` is missing in the mdu file.";
            Assert.That(Call, Throws.InvalidOperationException.With.Message.EqualTo(expectedMessage));
        }

        [Test]
        public void UpdateProperty_WrongLegacyProperty_DoesNotUpdateProperty()
        {
            // Setup
            const string key = "ThisNameIsNotTStartOrTStop";
            const string value = "randomValue";

            var logHandler = Substitute.For<ILogHandler>();
            var unsupportedProperty = new IniProperty(key, value, string.Empty);

            const string newKey = "newKey";
            var section = new IniSection("TestSection");
            section.AddProperty(unsupportedProperty);

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            updater.UpdateProperty(key, newKey, section, logHandler);

            // Assert
            unsupportedProperty = section.Properties.First();
            Assert.That(unsupportedProperty.Key, Is.EqualTo(key));
            Assert.That(unsupportedProperty.Value, Is.EqualTo(value));
        }

        [Test]
        [TestCaseSource(nameof(GetValidPropertyKeyCases))]
        public void UpdateProperty_UpdatesPropertyKeyAndLogsWarning(string oldPropertyKey, string newPropertyKey)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            var updater = new LegacyStartAndStopTimeUpdater();

            var property = new IniProperty(oldPropertyKey, "1");
            var section = new IniSection("TestSection");
            section.AddProperty(property);
            section.AddProperty(new IniProperty(KnownProperties.RefDate, "19900718"));
            section.AddProperty(new IniProperty(KnownProperties.Tunit, "S"));

            // Call
            updater.UpdateProperty(oldPropertyKey, newPropertyKey, section, logHandler);

            // Assert
            IniProperty updatedProperty = section.FindProperty(newPropertyKey);
            Assert.NotNull(updatedProperty);

            const string expectedLogMessage = "Backwards Compatibility: '{0}' has been updated to '{1}'";
            logHandler.Received(1).ReportWarningFormat(expectedLogMessage, oldPropertyKey, newPropertyKey);
        }

        [Test]
        [TestCaseSource(nameof(GetValidPropertyKeyCases))]
        public void UpdateProperty_MissingTUnit_ThrowsInvalidOperationException(string oldPropertyKey, string newPropertyKey)
        {
            // Setup
            const string originalValue = "10";
            var logHandler = Substitute.For<ILogHandler>();
            var property = new IniProperty(oldPropertyKey, originalValue);

            IniSection section = CreateSectionWithRefDate();
            section.AddProperty(property);

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            void Call() => updater.UpdateProperty(oldPropertyKey, newPropertyKey, section, logHandler);

            // Assert
            var expectedMessage = $"The keyword `{KnownProperties.Tunit}` is missing in the mdu file.";
            Assert.That(Call, Throws.InvalidOperationException.With.Message.EqualTo(expectedMessage));
        }

        [Test]
        [TestCaseSource(nameof(GetValidPropertyKeyCases))]
        public void UpdateProperty_MissingTUnitValue_UsesSecondsAsDefaultValue(string oldPropertyKey, string newPropertyKey)
        {
            // Setup
            const string originalValue = "10";
            var logHandler = Substitute.For<ILogHandler>();
            var property = new IniProperty(oldPropertyKey, originalValue);

            IniSection section = CreateSectionWithRefDateAndTUnit();
            section.AddProperty(property);
            
            IniProperty tUnitProperty = section.FindProperty(KnownProperties.Tunit);
            tUnitProperty.Value = null;

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            updater.UpdateProperty(oldPropertyKey, newPropertyKey, section, logHandler);

            // Assert
            IniProperty updatedProperty = section.FindProperty(newPropertyKey);
            Assert.NotNull(updatedProperty);

            const string expectedTUnitValue = "S"; // Updater should set the value to S if it is missing!
            Assert.That(tUnitProperty.Value, Is.EqualTo(expectedTUnitValue));
            
            const string expectedValue = "19900718000010"; // refdate = 1990-07-18, TStart/TStop = 10 and TUnit = S
            Assert.That(updatedProperty.Value, Is.EqualTo(expectedValue));

            const string expectedLogMessage = "Backwards Compatibility: Value for '{0}' has been updated from '{1}' to '{2}'";
            logHandler.Received(1).ReportWarningFormat(expectedLogMessage, updatedProperty.Key, originalValue, expectedValue);
        }

        [Test]
        [TestCaseSource(nameof(GetValidPropertyKeyCases))]
        public void UpdateProperty_MissingRefDate_ThrowsInvalidOperationException(string oldPropertyKey, string newPropertyKey)
        {
            // Setup
            const string originalValue = "10";
            var logHandler = Substitute.For<ILogHandler>();
            var property = new IniProperty(oldPropertyKey, originalValue);

            IniSection section = CreateSectionWithTUnit();
            section.AddProperty(property);

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            void Call() => updater.UpdateProperty(oldPropertyKey, newPropertyKey, section, logHandler);

            // Assert
            var expectedMessage = $"The keyword `{KnownProperties.RefDate}` is missing in the mdu file.";
            Assert.That(Call, Throws.InvalidOperationException.With.Message.EqualTo(expectedMessage));
        }

        [Test]
        [TestCaseSource(nameof(GetValidPropertyKeyCases))]
        public void UpdateProperty_MissingRefDateValue_ThrowsInvalidOperationException(string oldPropertyKey, string newPropertyKey)
        {
            // Setup
            const string originalValue = "10";
            var logHandler = Substitute.For<ILogHandler>();
            var property = new IniProperty(oldPropertyKey, originalValue);

            IniSection section = CreateSectionWithRefDateAndTUnit();
            section.AddProperty(property);

            IniProperty tUnitProperty = section.FindProperty(KnownProperties.RefDate);
            tUnitProperty.Value = null;

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            void Call() => updater.UpdateProperty(oldPropertyKey, newPropertyKey, section, logHandler);

            // Assert
            var expectedMessage = $"The value for the required keyword `{KnownProperties.RefDate}` is missing in the mdu file.";
            Assert.That(Call, Throws.InvalidOperationException.With.Message.EqualTo(expectedMessage));
        }

        [Test]
        [TestCaseSource(nameof(GetValidPropertyKeyCases))]
        public void UpdateProperty_UpdatesPropertyValueAndLogsMessage(string oldPropertyKey, string newPropertyKey)
        {
            // Setup
            const string originalValue = "10";
            var logHandler = Substitute.For<ILogHandler>();
            var property = new IniProperty(oldPropertyKey, originalValue, string.Empty);

            IniSection section = CreateSectionWithRefDateAndTUnit();
            section.AddProperty(property);

            var updater = new LegacyStartAndStopTimeUpdater();

            // Call
            updater.UpdateProperty(oldPropertyKey, newPropertyKey, section, logHandler);

            // Assert
            IniProperty updatedProperty = section.FindProperty(newPropertyKey);
            Assert.NotNull(updatedProperty);

            const string expectedValue = "19900718000010"; // refdate = 1990-07-18, TStart/TStop = 10 and TUnit = S
            Assert.That(updatedProperty.Value, Is.EqualTo(expectedValue));

            const string expectedLogMessage = "Backwards Compatibility: Value for '{0}' has been updated from '{1}' to '{2}'";
            logHandler.Received(1).ReportWarningFormat(expectedLogMessage, updatedProperty.Key, originalValue, expectedValue);
        }

        private static IEnumerable<TestCaseData> GetArgumentNullCases()
        {
            var oldPropertyKey = "OldPropertyKey";
            var newPropertyKey = "NewPropertyKey";
            var section = new IniSection("TestSection");
            var logHandler = Substitute.For<ILogHandler>();

            yield return new TestCaseData(null, newPropertyKey, section, logHandler);
            yield return new TestCaseData(oldPropertyKey, null, section, logHandler);
            yield return new TestCaseData(oldPropertyKey, newPropertyKey, null, logHandler);
            yield return new TestCaseData(oldPropertyKey, newPropertyKey, section, null);
        }

        private static IEnumerable<TestCaseData> GetValidPropertyKeyCases()
        {
            yield return new TestCaseData(KnownLegacyProperties.TStart, KnownProperties.StartDateTime);
            yield return new TestCaseData(KnownLegacyProperties.TStop, KnownProperties.StopDateTime);
        }

        private static IniSection CreateSectionWithRefDate()
        {
            var section = new IniSection("randomSectionName");

            section.AddProperty(CreateProperty("RefDate", "19900718"));

            return section;
        }

        private static IniSection CreateSectionWithTUnit()
        {
            var section = new IniSection("randomSectionName");

            section.AddProperty(CreateProperty("TUnit", "S"));

            return section;
        }

        private static IniSection CreateSectionWithRefDateAndTUnit()
        {
            var section = new IniSection("randomSectionName");

            section.AddProperty(CreateProperty("TUnit", "S"));
            section.AddProperty(CreateProperty("RefDate", "19900718"));

            return section;
        }

        private static IniProperty CreateProperty(string key, string value)
        {
            return new IniProperty(key, value, string.Empty);
        }
    }
}