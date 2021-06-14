using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlModelXmlReaderTest
    {
        private readonly string directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "SimpleModel"));
        private RealTimeControlModel rtcModel;

        [SetUp]
        public void SetUp()
        {
            Assert.IsTrue(Directory.Exists(directoryPath));
        }

        [TearDown]
        public void TearDown()
        {
            rtcModel = null;
        }

        [Test]
        public void GivenAnInvalidRtcDirectoryPath_WhenReading_ThenExpectedErrorMessageIsGiven()
        {
            // Given
            const string invalidPath = "InvalidPath";
            Assert.That(!Directory.Exists(invalidPath));
            rtcModel = new RealTimeControlModel();

            // When
            TestHelper.AssertAtLeastOneLogMessagesContains(() => rtcModel = RealTimeControlModelXmlReader.Read(invalidPath),
                                                           string.Format(Resources.RealTimeControlModelXmlReader_Read_Directory___0___does_not_exist_,
                                                                         invalidPath));

            // Then
            Assert.IsNull(rtcModel);
        }

        [Test]
        public void GivenAnInvalidRtcDirectoryPath_WhenReading_ThenNoExceptionIsThrownAndNullIsReturned()
        {
            // Given
            const string invalidPath = "InvalidPath";
            Assert.That(!Directory.Exists(invalidPath));
            rtcModel = new RealTimeControlModel();

            // When
            Assert.DoesNotThrow(
                () => rtcModel = RealTimeControlModelXmlReader.Read(invalidPath),
                "While reading from a non existing path, an unexpected exception was thrown");

            // Then
            Assert.Null(rtcModel);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidRtcDirectoryPath_WhenReadingAllTheFiles_TheExpectedRtcModelIsReturned_SimpleModel()
        {
            // When
            rtcModel = RealTimeControlModelXmlReader.Read(directoryPath);

            // Then
            Assert.NotNull(rtcModel,
                           "Returned model was not expected to be null after reading from an existing path.");
            Assert.AreEqual(true, rtcModel.LimitMemory,
                            "Option 'limit memory' was expected to be true.");

            CheckSimpleModelTimeSettings(rtcModel);
            CheckSimpleModelControlGroupValidity(rtcModel);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidRtcDirectoryPath_WhenReadingAllTheFiles_ThenNoExceptionIsThrown()
        {
            // When
            Assert.DoesNotThrow(() => rtcModel = RealTimeControlModelXmlReader.Read(directoryPath),
                                "While reading from a existing path, an unexpected exception was thrown");

            // Then
            Assert.NotNull(rtcModel,
                           "Returned model was not expected to be null after reading from an existing path.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidRtcDirectoryPathWithUseRestartSetToFalse_WhenReadingAllTheFiles_TheExpectedWarningMessageIsGiven()
        {
            // When
            rtcModel = RealTimeControlModelXmlReader.Read(directoryPath);

            // Then
            TestHelper.AssertAtLeastOneLogMessagesContains(() => rtcModel = RealTimeControlModelXmlReader.Read(directoryPath),
                                                           string.Format(Resources.RealTimeControlModelXmlReader_Please_note_that_Use_Restart_option_in_D_RTC_is_set_to_False,
                                                                         directoryPath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadingRTCModel_WhenIntervalRuleWithFixedSetpointHasBeenDefined_ThenTimeSeriesFileShouldCorrectSetpointTypeAfterReadingToolsConfigFile()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                string directoryTemp = tempDirectory.CopyDirectoryToTempDirectory(TestHelper.GetTestFilePath(Path.Combine("ImportExport", "IntervalRuleFixedSetpoint")));
                RealTimeControlModel model = RealTimeControlModelXmlReader.Read(directoryTemp);

                Assert.AreEqual(1, model.ControlGroups.Count);
                ControlGroup controlGroup = model.ControlGroups[0];
                Assert.AreEqual(1, controlGroup.Rules.Count);
                var intervalRule = model.ControlGroups[0].Rules[0] as IntervalRule;
                Assert.NotNull(intervalRule);
                
                Assert.AreEqual(IntervalRule.IntervalRuleSetPointType.Fixed, intervalRule.SetPointType);
                Assert.AreEqual(5, intervalRule.ConstantValue);
                Assert.AreEqual(IntervalRule.IntervalRuleIntervalType.Fixed, intervalRule.IntervalType);
                Assert.AreEqual(3, intervalRule.FixedInterval);
                Assert.AreEqual(1, intervalRule.Setting.Below);
                Assert.AreEqual(2, intervalRule.Setting.Above);
                Assert.AreEqual(4, intervalRule.DeadbandAroundSetpoint);
            }
        }

        private static void CheckSimpleModelTimeSettings(ITimeDependentModel rtcModel)
        {
            Assert.AreEqual(new DateTime(2018, 12, 12, 0, 0, 0), rtcModel.StartTime, "Model start time is incorrectly set.");
            Assert.AreEqual(new DateTime(2018, 12, 13, 0, 0, 0), rtcModel.StopTime, "Model stop time is incorrectly set.");
            Assert.AreEqual(new TimeSpan(0, 30, 0), rtcModel.TimeStep, "Model time step is incorrectly set.");
        }

        private static void CheckSimpleModelControlGroupValidity(IRealTimeControlModel rtcModel)
        {
            Assert.AreEqual(1, rtcModel.ControlGroups.Count, "Number of control groups was expected to be 1.");

            ControlGroup controlGroup = rtcModel.ControlGroups[0];

            Assert.AreEqual("control_group", controlGroup.Name);

            IEventedList<Input> inputs = controlGroup.Inputs;
            Assert.AreEqual(1, inputs.Count, "Number of inputs was expected to be 1.");
            Assert.AreEqual("[Input]parameter/quantity", inputs.First().Name);

            IEventedList<Output> outputs = controlGroup.Outputs;
            Assert.AreEqual(1, outputs.Count, "Number of outputs was expected to be 1.");
            Assert.AreEqual("[Output]parameter/quantity", outputs.First().Name);

            IEventedList<ConditionBase> conditions = controlGroup.Conditions;
            Assert.AreEqual(2, conditions.Count, "Number of conditions was expected to be 2.");

            List<TimeCondition> timeCondition = conditions.OfType<TimeCondition>().ToList();
            Assert.NotNull(timeCondition);
            Assert.AreEqual(1, timeCondition.Count, "Number of time conditions was expected to be 1.");
            Assert.AreEqual("time_condition", timeCondition.First().Name);

            List<StandardCondition> standardConditions = conditions.OfType<StandardCondition>()
                                                                   .Where(c => c.GetType() != typeof(TimeCondition)).ToList();
            Assert.NotNull(standardConditions);
            Assert.AreEqual(1, standardConditions.Count, "Number of standard conditions was expected to be 1.");
            Assert.AreEqual("standard_condition", standardConditions.First().Name);

            IEventedList<RuleBase> rules = controlGroup.Rules;
            Assert.AreEqual(2, rules.Count, "Number of rules was expected to be 2.");

            List<TimeRule> timeRules = rules.OfType<TimeRule>().ToList();
            Assert.NotNull(timeRules);
            Assert.AreEqual(1, timeRules.Count, "Number of time rules was expected to be 1.");
            Assert.AreEqual("time_rule", timeRules.First().Name);

            List<RelativeTimeRule> relativeTimeRules = rules.OfType<RelativeTimeRule>().ToList();
            Assert.NotNull(relativeTimeRules);
            Assert.AreEqual(1, relativeTimeRules.Count, "Number of relative time rules was expected to be 1.");
            Assert.AreEqual("relative_time_rule", relativeTimeRules.First().Name);
        }
    }
}