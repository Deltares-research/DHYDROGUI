using System.Collections.Generic;
using DeltaShell.NGHS.IO.Ini;
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
        public void UpdateProperty_ArgumentNull_ThrowsArgumentNullException(string oldPropertyKey,
                                                                            string newPropertyKey,
                                                                            IniSection section,
                                                                            ILogHandler logHandler)
        {
            // Setup
            var updater = new DefaultPropertyUpdater();

            // Call
            void Call() => updater.UpdateProperty(oldPropertyKey, newPropertyKey, section, logHandler);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void UpdatePropertyUpdatesLegacyPropertyKeyAndLogsMessage()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            const string oldPropertyKey = "oldKey";
            var legacyProperty = new IniProperty(oldPropertyKey);
            const string newPropertyKey = "newKey";
            var section = new IniSection("TestSection");
            section.AddProperty(legacyProperty);

            var updater = new DefaultPropertyUpdater();

            // Call
            updater.UpdateProperty(oldPropertyKey, newPropertyKey, section, logHandler);

            // Assert
            IniProperty updatedProperty = section.GetProperty(newPropertyKey);
            Assert.NotNull(updatedProperty);

            const string expectedLogMessage = "Backwards Compatibility: '{0}' has been updated to '{1}'";
            logHandler.Received(1).ReportWarningFormat(expectedLogMessage, oldPropertyKey, newPropertyKey);
        }

        private static IEnumerable<TestCaseData> GetArgumentNullCases()
        {
            var oldPropertyKey = "OldProperty";
            var newPropertyKey = "NewProperty";
            var section = new IniSection("TestSection");
            var logHandler = Substitute.For<ILogHandler>();

            yield return new TestCaseData(null, newPropertyKey, section, logHandler);
            yield return new TestCaseData(oldPropertyKey, null, section, logHandler);
            yield return new TestCaseData(oldPropertyKey, newPropertyKey, section, null);
            yield return new TestCaseData(oldPropertyKey, newPropertyKey, null, logHandler);
        }
    }
}