using System;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlRuntimeConfigSetterTest
    {
        private const string AssertMessage_StartTimeOfModelIsIncorrectlySet = "Start time of model is incorrectly set.";
        private const string AssertMessage_TimeStepOfModelIsIncorrectlySet = "Time step of model is incorrectly set.";
        private const string AssertMessage_StopTimeOfModelIsIncorrectlySet = "Stop time of model is incorrectly set.";

        private RealTimeControlRuntimeConfigSetter runtimeConfigSetter;
        private RealTimeControlModel rtcModel;

        [SetUp]
        public void SetUp()
        {
            runtimeConfigSetter = new RealTimeControlRuntimeConfigSetter();
            rtcModel = new RealTimeControlModel();
        }

        [TearDown]
        public void TearDown()
        {
            runtimeConfigSetter = null;
            rtcModel = null;
        }

        [Test]
        [TestCase("00:00:01", timeStepUnitEnumStringType1.second)] // second
        [TestCase("00:01:00", timeStepUnitEnumStringType1.minute)] // minute
        [TestCase("01:00:00", timeStepUnitEnumStringType1.hour)]   // hour
        [TestCase("1.00:00:00", timeStepUnitEnumStringType1.day)]  // day
        [TestCase("7.00:00:00", timeStepUnitEnumStringType1.week)] // week
        public void GivenAUserDefinedRuntimeXmlElement_WhenSetRunTimeSettingsIsCalled_ThenCorrectDataIsSetOnModel(string expectedTimeSpanString, timeStepUnitEnumStringType1 timeStepUnit)
        {
            // Given
            DateTime startDate = DateTime.Today;
            UserDefinedRuntimeComplexType runTimeSettingsElement = CreateRunTimeSettings(timeStepUnit, startDate);

            // When
            runtimeConfigSetter.SetRunTimeSettings(rtcModel, runTimeSettingsElement);

            // Then
            Assert.AreEqual(startDate, rtcModel.StartTime,
                            AssertMessage_StartTimeOfModelIsIncorrectlySet);
            Assert.AreEqual(expectedTimeSpanString, rtcModel.TimeStep.ToString(),
                            AssertMessage_TimeStepOfModelIsIncorrectlySet);
            Assert.AreEqual(startDate.AddDays(1), rtcModel.StopTime,
                            AssertMessage_StopTimeOfModelIsIncorrectlySet);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GivenModeXmlElement_WhenSetSimulationModeSettingsIsCalled_ThenCorrectDataIsSetOnModel(bool limitedMemory)
        {
            // Given
            ModeComplexType simulationModeSettingsElement = CreateSimulationModeSettings(limitedMemory);

            // When
            runtimeConfigSetter.SetSimulationModeSettings(rtcModel, simulationModeSettingsElement);

            // Then
            Assert.AreEqual(limitedMemory, rtcModel.LimitMemory,
                            $"Option 'limit memory' was expected to be {limitedMemory.ToString()}.");
        }

        [TestCase(-1, false)]
        [TestCase(0, true)]
        [TestCase(7200, true)]
        public void GivenAUserDefinedStateExportXmlElementWithAValidTimeStep_WhenSetRestartSettingsIsCalled_ThenCorrectDataIsSetOnModel(int restartTimeStep, bool expectedWriteRestart)
        {
            DateTime restartStartDate = DateTime.Today.AddYears(1);
            DateTime restartEndDate = restartStartDate.AddHours(12);

            AssertNotDefaultModelValues(restartStartDate, restartEndDate, restartTimeStep);

            UserDefinedStateExportComplexType restartSettingsElement = CreateRestartSettings(restartTimeStep, restartStartDate, restartEndDate);

            // When
            runtimeConfigSetter.SetRestartSettings(rtcModel, restartSettingsElement);

            // Then
            Assert.AreEqual(expectedWriteRestart, rtcModel.WriteRestart,
                            $"Option 'write restart' was expected to be {expectedWriteRestart.ToString()}.");

            if (expectedWriteRestart)
            {
                AssertReadValuesAreSet(restartStartDate, restartEndDate, restartTimeStep);
            }
            else
            {
                AssertDefaultValuesAreSet();
            }
        }

        private UserDefinedRuntimeComplexType CreateRunTimeSettings(timeStepUnitEnumStringType1 timeStepUnit, DateTime startDateTime)
        {
            var settings = new UserDefinedRuntimeComplexType
            {
                startDate = new DateTimeComplexType1
                {
                    date = startDateTime,
                    time = startDateTime
                },
                endDate = new DateTimeComplexType1
                {
                    date = startDateTime.AddDays(1),
                    time = startDateTime.AddDays(1)
                },
                timeStep = new TimeStepComplexType1
                {
                    unit = timeStepUnit,
                    multiplier = "1",
                    divider = "1"
                }
            };

            return settings;
        }

        private ModeComplexType CreateSimulationModeSettings(bool limitedMemory)
        {
            var mode = new ModeComplexType {Item = new ModeSimulationComplexType {limitedMemory = limitedMemory}};

            return mode;
        }

        private UserDefinedStateExportComplexType CreateRestartSettings(double timeStep, DateTime startDateTime, DateTime endDateTime)
        {
            var settings = new UserDefinedStateExportComplexType
            {
                startDate = new DateTimeComplexType1
                {
                    date = startDateTime,
                    time = startDateTime
                },
                endDate = new DateTimeComplexType1
                {
                    date = endDateTime,
                    time = endDateTime
                },
                stateTimeStep = timeStep
            };

            return settings;
        }

        private void AssertNotDefaultModelValues(DateTime restartStartDate, DateTime restartEndDate, int restartTimeStep)
        {
            Assert.AreNotEqual(restartStartDate, rtcModel.StartTime,
                               AssertMessage_StartTimeOfModelIsIncorrectlySet);
            Assert.AreNotEqual(restartEndDate, rtcModel.StopTime,
                               AssertMessage_StopTimeOfModelIsIncorrectlySet);
            Assert.AreNotEqual(restartTimeStep, rtcModel.SaveStateTimeStep.TotalSeconds,
                               AssertMessage_TimeStepOfModelIsIncorrectlySet);
        }

        private void AssertDefaultValuesAreSet()
        {
            Assert.AreEqual(rtcModel.StopTime, rtcModel.SaveStateStartTime,
                            AssertMessage_StartTimeOfModelIsIncorrectlySet);
            Assert.AreEqual(rtcModel.StopTime, rtcModel.SaveStateStopTime,
                            AssertMessage_StopTimeOfModelIsIncorrectlySet);
            Assert.AreEqual(rtcModel.TimeStep, rtcModel.SaveStateTimeStep,
                            AssertMessage_TimeStepOfModelIsIncorrectlySet);
        }

        private void AssertReadValuesAreSet(DateTime restartStartDate, DateTime restartEndDate, int restartTimeStep)
        {
            Assert.AreEqual(restartStartDate, rtcModel.SaveStateStartTime,
                            AssertMessage_StartTimeOfModelIsIncorrectlySet);
            Assert.AreEqual(restartEndDate, rtcModel.SaveStateStopTime,
                            AssertMessage_StopTimeOfModelIsIncorrectlySet);
            Assert.AreEqual(restartTimeStep, rtcModel.SaveStateTimeStep.TotalSeconds,
                            AssertMessage_TimeStepOfModelIsIncorrectlySet);
        }
    }
}