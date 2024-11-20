using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Logging;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlDataAndToolsConfigXmlReaderTest
    {
        private const string AssertMessage_CollectedLogMessagesDidNotContainExpectedMessage = "The collected log messages did not contain the expected message.";
        private static readonly string DirectoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "DataAndToolsConfigFiles"));
        private RealTimeControlDataAndToolsConfigXmlReader dataAndToolsConfigReader;
        private ILogHandler logHandler;
        private readonly TimeSpan timeSpan = new TimeSpan();
        private readonly string validDataConfigFilePath = Path.Combine(DirectoryPath, "rtcDataConfig.xml");
        private readonly string validToolsConfigFilePath = Path.Combine(DirectoryPath, "rtcToolsConfig.xml");

        [SetUp]
        public void SetUp()
        {
            logHandler = new LogHandler("");
            dataAndToolsConfigReader = new RealTimeControlDataAndToolsConfigXmlReader(logHandler);
            Assert.That(File.Exists(validDataConfigFilePath));
            Assert.That(File.Exists(validToolsConfigFilePath));
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
            dataAndToolsConfigReader = null;
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenExistingDataAndToolsConfigFilesWithValidData_WhenReading_ThenListOfControlGroupsIsReturnedWithCorrectAmountOfRtcComponents()
        {
            // When
            IList<IControlGroup> controlGroups = dataAndToolsConfigReader.Read(validDataConfigFilePath, validToolsConfigFilePath, timeSpan);

            // Then
            Assert.NotNull(controlGroups, "List of controlgroups is not expected to be NULL after reading from file.");
            Assert.AreEqual(7, controlGroups.Count, "Exactly 7 control groups were expected to be created from file.");
            Assert.AreEqual(7, controlGroups.SelectMany(g => g.Inputs).Count(), "Exactly 7 inputs were expected to be created from file.");
            Assert.AreEqual(7, controlGroups.SelectMany(g => g.Outputs).Count(), "Exactly 7 outputs were expected to be created from file.");
            Assert.AreEqual(6, controlGroups.SelectMany(g => g.Conditions).Count(), "Exactly 6 conditions were expected to be created from file.");
            Assert.AreEqual(7, controlGroups.SelectMany(g => g.Rules).Count(), "Exactly 7 rules were expected to be created from file.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenNonExistingDataConfigFile_WhenReading_ThenEmptyListOfControlGroupIsReturnedAndExpectedErrorsAreLogged()
        {
            // Given
            const string invalidDataConfigFilePath = "invalid";
            Assert.That(!File.Exists(invalidDataConfigFilePath), $"Path '{invalidDataConfigFilePath}' was expected to not exist.");

            // When
            IList<IControlGroup> controlGroups = dataAndToolsConfigReader.Read(invalidDataConfigFilePath, validToolsConfigFilePath, timeSpan);

            // Then
            Assert.IsTrue(logHandler.LogMessages.AllMessages.Any(m => m.Contains(invalidDataConfigFilePath)),
                          AssertMessage_CollectedLogMessagesDidNotContainExpectedMessage);
            AssertNotNullAndEmpty(controlGroups);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenNonExistingToolsConfigFile_WhenReading_ThenEmptyListOfControlGroupIsReturnedAndExpectedErrorsAreLogged()
        {
            // Given
            const string invalidToolsConfigFilePath = "invalid";
            Assert.That(!File.Exists(invalidToolsConfigFilePath), $"Path '{invalidToolsConfigFilePath}' was expected to not exist.");

            // When
            IList<IControlGroup> controlGroups = dataAndToolsConfigReader.Read(validDataConfigFilePath, invalidToolsConfigFilePath, timeSpan);

            // Then
            Assert.IsTrue(logHandler.LogMessages.AllMessages.Any(m => m.Contains(invalidToolsConfigFilePath)),
                          AssertMessage_CollectedLogMessagesDidNotContainExpectedMessage);
            AssertNotNullAndEmpty(controlGroups);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenExistingDataAndToolsConfigFilesWithValidDataWithoutControlGroups_WhenReading_ThenEmptyListOfControlGroupIsReturnedAndExpectedErrorsAreLogged()
        {
            // Given
            string toolsConfigFilePath = Path.Combine(DirectoryPath, "rtcToolsConfig_NoControlGroups.xml");
            Assert.That(File.Exists(toolsConfigFilePath), $"Path '{toolsConfigFilePath}' was expected to exist.");

            string expectedMessage = string.Format(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_control_groups_from_file___0___, toolsConfigFilePath);

            // When
            IList<IControlGroup> controlGroups = dataAndToolsConfigReader.Read(validDataConfigFilePath, toolsConfigFilePath, timeSpan);

            // Then
            Assert.IsTrue(logHandler.LogMessages.AllMessages.Contains(expectedMessage),
                          AssertMessage_CollectedLogMessagesDidNotContainExpectedMessage);
            AssertNotNullAndEmpty(controlGroups);
        }

        private static void AssertNotNullAndEmpty(IList<IControlGroup> controlGroups)
        {
            Assert.NotNull(controlGroups, "List of control groups was not expected to be NULL.");
            Assert.AreEqual(0, controlGroups.Count, "List of control groups was expected to be empty.");
        }
    }
}