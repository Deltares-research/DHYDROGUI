using System.Collections.Generic;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility
{
    [TestFixture]
    public class DefaultPropertyUpdaterTest
    {
        [Test]
        [TestCaseSource(nameof(GetArgumentNullCases))]
        public void UpdateProperty_ArgumentNull_ThrowsArgumentNullException(DelftIniProperty legacyProperty,
                                                                            string newPropertyName,
                                                                            DelftIniCategory legacyPropertyCategory,
                                                                            ILogHandler logHandler)
        {
            // Setup
            var updater = new DefaultPropertyUpdater();

            // Call
            void Call() => updater.UpdateProperty(legacyProperty, newPropertyName, legacyPropertyCategory, logHandler);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void UpdatePropertyUpdatesLegacyPropertyNameAndLogsMessage()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            const string oldName = "oldName";
            var legacyProperty = new DelftIniProperty(oldName, string.Empty, string.Empty);
            const string newPropertyName = "newName";
            var legacyPropertyCategory = new DelftIniCategory(string.Empty);

            var updater = new DefaultPropertyUpdater();

            // Call
            updater.UpdateProperty(legacyProperty, newPropertyName, legacyPropertyCategory, logHandler);

            // Assert
            Assert.That(legacyProperty.Name, Is.EqualTo(newPropertyName));

            const string expectedLogMessage = "Backwards Compatibility: '{0}' has been updated to '{1}'";
            logHandler.Received(1).ReportWarningFormat(expectedLogMessage, oldName, newPropertyName);
        }

        private static IEnumerable<TestCaseData> GetArgumentNullCases()
        {
            var legacyProperty = new DelftIniProperty(string.Empty, string.Empty, string.Empty);
            var newPropertyName = string.Empty;
            var legacyPropertyCategory = new DelftIniCategory(string.Empty);
            var logHandler = Substitute.For<ILogHandler>();

            yield return new TestCaseData(null, newPropertyName, legacyPropertyCategory, logHandler);
            yield return new TestCaseData(legacyProperty, null, legacyPropertyCategory, logHandler);
            yield return new TestCaseData(legacyProperty, newPropertyName, legacyPropertyCategory, null);
        }
    }
}