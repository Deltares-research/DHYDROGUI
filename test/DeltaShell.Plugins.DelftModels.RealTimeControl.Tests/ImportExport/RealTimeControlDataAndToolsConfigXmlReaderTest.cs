using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
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
            var controlGroups = dataAndToolsConfigReader.Read(validDataConfigFilePath, validToolsConfigFilePath, timeSpan);

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
            var controlGroups = dataAndToolsConfigReader.Read(invalidDataConfigFilePath, validToolsConfigFilePath, timeSpan);

            // Then
            Assert.IsTrue(logHandler.LogMessagesTable.AllMessages.Any(m => m.Contains(invalidDataConfigFilePath)),
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
            var controlGroups = dataAndToolsConfigReader.Read(validDataConfigFilePath, invalidToolsConfigFilePath, timeSpan);

            // Then
            Assert.IsTrue(logHandler.LogMessagesTable.AllMessages.Any(m => m.Contains(invalidToolsConfigFilePath)),
                AssertMessage_CollectedLogMessagesDidNotContainExpectedMessage);
            AssertNotNullAndEmpty(controlGroups);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenExistingDataAndToolsConfigFilesWithValidDataWithoutConnectionPoints_WhenReading_ThenEmptyListOfControlGroupIsReturnedAndExpectedErrorsAreLogged()
        {
            // Given
            var dataConfigFilePath = Path.Combine(DirectoryPath, "rtcDataConfig_NoConnectionPoints.xml");
            Assert.That(File.Exists(dataConfigFilePath), $"Path '{dataConfigFilePath}' was expected to exist.");

            var expectedMessage =
                string.Format(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_connection_points_from_file___0___,
                    dataConfigFilePath);

            // When
            var controlGroups = dataAndToolsConfigReader.Read(dataConfigFilePath, validToolsConfigFilePath, timeSpan);

            // Then
            Assert.IsTrue(logHandler.LogMessagesTable.AllMessages.Contains(expectedMessage),
                AssertMessage_CollectedLogMessagesDidNotContainExpectedMessage);
            AssertNotNullAndEmpty(controlGroups);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenExistingDataAndToolsConfigFilesWithValidDataWithoutControlGroups_WhenReading_ThenEmptyListOfControlGroupIsReturnedAndExpectedErrorsAreLogged()
        {
            // Given
            var toolsConfigFilePath = Path.Combine(DirectoryPath, "rtcToolsConfig_NoControlGroups.xml");
            Assert.That(File.Exists(toolsConfigFilePath), $"Path '{toolsConfigFilePath}' was expected to exist.");

            var expectedMessage = string.Format(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_control_groups_from_file___0___, toolsConfigFilePath);

            // When
            var controlGroups = dataAndToolsConfigReader.Read(validDataConfigFilePath, toolsConfigFilePath, timeSpan);

            // Then
            Assert.IsTrue(logHandler.LogMessagesTable.AllMessages.Contains(expectedMessage),
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
