using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DHYDRO.Common.Logging;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlRuntimeConfigXmlReaderTest
    {
        private const string AssertMessage_CollectedLogMessagesDidNotContainExpectedMessage = "The collected log messages did not contain the expected message.";

        private ILogHandler logHandler;
        private RealTimeControlRuntimeConfigXmlReader runtimeConfigReader;
        private readonly string directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "RuntimeConfigFiles"));
        private RealTimeControlModel rtcModel;

        private DateTime defaultStartTime;
        private DateTime defaultStopTime;
        private TimeSpan defaultTimeStep;
        private bool defaultLimitMemory;

        [SetUp]
        public void SetUp()
        {
            rtcModel = new RealTimeControlModel();
            logHandler = new LogHandler("");
            runtimeConfigReader = new RealTimeControlRuntimeConfigXmlReader(logHandler);
            Assert.IsTrue(Directory.Exists(directoryPath),
                          $"Directory path '{directoryPath}' was expected to exist.");
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
            runtimeConfigReader = null;
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndAnRtcModel_WhenReading_ThenExpectedDataIsSetOnModel()
        {
            // Given
            string filePath = Path.Combine(directoryPath, "rtcRuntimeConfig.xml");
            Assert.That(File.Exists(filePath),
                        $"File path '{filePath}' was expected to exist.");

            // When
            runtimeConfigReader.Read(filePath, rtcModel);

            // Then
            Assert.AreEqual(new DateTime(2019, 1, 30), rtcModel.StartTime);
            Assert.AreEqual(new DateTime(2019, 1, 31), rtcModel.StopTime);
            Assert.AreEqual(TimeSpan.FromHours(1), rtcModel.TimeStep);
            Assert.AreEqual(true, rtcModel.LimitMemory);
        }

        [Test]
        public void GivenANonExistingFile_WhenReading_ThenExpectedMessageIsGivenAndModelHasDefaultValues()
        {
            // Given
            const string filePath = "invalid_path";
            Assert.That(!File.Exists(filePath),
                        $"File path '{filePath}' was expected to not exist.");

            RetrieveDefaultValues();

            // When
            runtimeConfigReader.Read(filePath, rtcModel);

            // Then
            AssertValuesAreDefault();
            Assert.IsTrue(logHandler.LogMessages.AllMessages.Any(m => m.Contains(filePath)),
                          AssertMessage_CollectedLogMessagesDidNotContainExpectedMessage);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndNullIsGivenAsParameterForModel_WhenReading_ThenMethodDoesNotThrowAnException()
        {
            // Given
            string filePath = Path.Combine(directoryPath, "rtcRuntimeConfig.xml");
            Assert.That(File.Exists(filePath),
                        $"File path '{filePath}' was expected to exist.");

            // When, Then
            Assert.DoesNotThrow(() => runtimeConfigReader.Read(filePath, null));
        }

        private void AssertValuesAreDefault()
        {
            Assert.AreEqual(defaultStartTime, rtcModel.StartTime,
                            "Start time of model is incorrectly set.");
            Assert.AreEqual(defaultStopTime, rtcModel.StopTime,
                            "Stop time of model is incorrectly set.");
            Assert.AreEqual(defaultTimeStep, rtcModel.TimeStep,
                            "Time step of model is incorrectly set.");
            Assert.AreEqual(defaultLimitMemory, rtcModel.LimitMemory,
                            $"Option 'limit memory' was expected to be {defaultLimitMemory.ToString()}.");
        }

        private void RetrieveDefaultValues()
        {
            defaultStartTime = rtcModel.StartTime;
            defaultStopTime = rtcModel.StopTime;
            defaultTimeStep = rtcModel.TimeStep;
            defaultLimitMemory = rtcModel.LimitMemory;
        }
    }
}