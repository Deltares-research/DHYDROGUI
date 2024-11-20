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
        [Test]
        public void ReadFromXml_RtcModelIsNull_ThrowsArgumentNullException()
        {
            // Given
            string directoryPath = GetTestDataDirectory();

            RealTimeControlModelXmlReader reader = CreateReader();

            // When
            // Then
            Assert.That(() => reader.ReadFromXml(null, directoryPath), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void ReadFromXml_DirectoryIsNullOrEmpty_ThrowsArgumentException(string directoryPath)
        {
            // Given
            RealTimeControlModel rtcModel = CreateModel();
            RealTimeControlModelXmlReader reader = CreateReader();

            // When
            // Then
            Assert.That(() => reader.ReadFromXml(rtcModel, directoryPath), Throws.ArgumentException);
        }

        [Test]
        public void ReadFromXml_DirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            // Given
            const string invalidPath = "InvalidPath";
            Assert.That(!Directory.Exists(invalidPath));

            RealTimeControlModel rtcModel = CreateModel();
            RealTimeControlModelXmlReader reader = CreateReader();

            // When
            // Then
            Assert.That(() => reader.ReadFromXml(rtcModel, invalidPath), Throws.InstanceOf<DirectoryNotFoundException>());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidRtcDirectoryPath_WhenReadingAllTheFiles_TheExpectedRtcModelIsRead_SimpleModel()
        {
            // Given
            string directoryPath = GetTestDataDirectory();

            RealTimeControlModel rtcModel = CreateModel();
            RealTimeControlModelXmlReader reader = CreateReader();

            // When
            reader.ReadFromXml(rtcModel, directoryPath);

            // Then
            Assert.AreEqual(true, rtcModel.LimitMemory, "Option 'limit memory' was expected to be true.");

            CheckSimpleModelTimeSettings(rtcModel);
            CheckSimpleModelControlGroupValidity(rtcModel);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidRtcDirectoryPathWithUseRestartSetToFalse_WhenReadingAllTheFiles_TheExpectedWarningMessageIsGiven()
        {
            // Given
            string directoryPath = GetTestDataDirectory();

            RealTimeControlModel rtcModel = CreateModel();
            RealTimeControlModelXmlReader reader = CreateReader();

            // When
            // Then
            string expectedPartOfMessage = string.Format(Resources.RealTimeControlModelXmlReader_Please_note_that_Use_Restart_option_in_D_RTC_is_set_to_False, directoryPath);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => reader.ReadFromXml(rtcModel, directoryPath), expectedPartOfMessage);
        }

        private static RealTimeControlModelXmlReader CreateReader()
        {
            return new RealTimeControlModelXmlReader();
        }

        private static RealTimeControlModel CreateModel()
        {
            return new RealTimeControlModel();
        }

        private static string GetTestDataDirectory()
        {
            return TestHelper.GetTestFilePath(Path.Combine("ImportExport", "SimpleModel"));
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