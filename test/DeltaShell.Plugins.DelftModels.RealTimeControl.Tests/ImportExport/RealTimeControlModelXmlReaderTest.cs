using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlModelXmlReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidRtcDirectoryPath_WhenReadingAllTheFiles_TheExpectedRtcModelIsReturned_SimpleModel()
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "SimpleModel" ));
            Assert.That(Directory.Exists(directoryPath));

            // When
            var rtcModel = RealTimeControlModelXmlReader.Read(directoryPath);

            // Then
            Assert.NotNull(rtcModel);
            Assert.AreEqual(true, rtcModel.LimitMemory);

            CheckSimpleModelTimeSettings(rtcModel);
            CheckSimpleModelControlGroupValidity(rtcModel);
        }

        private static void CheckSimpleModelTimeSettings(ITimeDependentModel rtcModel)
        {
            Assert.AreEqual(new DateTime(2018, 12, 12, 0, 0, 0), rtcModel.StartTime);
            Assert.AreEqual(new DateTime(2018, 12, 13, 0, 0, 0), rtcModel.StopTime);
            Assert.AreEqual(new TimeSpan(0, 30, 0), rtcModel.TimeStep);
        }

        private static void CheckSimpleModelControlGroupValidity(IRealTimeControlModel rtcModel)
        {
            Assert.AreEqual(1, rtcModel.ControlGroups.Count);

            var controlGroup = rtcModel.ControlGroups[0];

            Assert.AreEqual("control_group", controlGroup.Name);

            var inputs = controlGroup.Inputs;
            Assert.AreEqual(1, inputs.Count);
            Assert.AreEqual("[Input]parameter/quantity", inputs.First().Name);

            var outputs = controlGroup.Outputs;
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[Output]parameter/quantity", outputs.First().Name);

            var conditions = controlGroup.Conditions;
            Assert.AreEqual(2, conditions.Count);

            var timeCondition = conditions.OfType<TimeCondition>().ToList();
            Assert.NotNull(timeCondition);
            Assert.AreEqual(1, timeCondition.Count);
            Assert.AreEqual("time_condition", timeCondition.First().Name);

            var standardConditions = conditions.OfType<StandardCondition>()
                .Where(c => c.GetType() != typeof(TimeCondition)).ToList();
            Assert.NotNull(standardConditions);
            Assert.AreEqual(1, standardConditions.Count);
            Assert.AreEqual("standard_condition", standardConditions.First().Name);

            var rules = controlGroup.Rules;
            Assert.AreEqual(2, rules.Count);

            var timeRules = rules.OfType<TimeRule>().ToList();
            Assert.NotNull(timeRules);
            Assert.AreEqual(1, timeRules.Count);
            Assert.AreEqual("time_rule", timeRules.First().Name);

            var relativeTimeRules = rules.OfType<RelativeTimeRule>().ToList();
            Assert.NotNull(relativeTimeRules);
            Assert.AreEqual(1, relativeTimeRules.Count);
            Assert.AreEqual("relative_time_rule", relativeTimeRules.First().Name);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidRtcDirectoryPath_WhenReadingAllTheFiles_ThenNoExceptionIsThrown()
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "SimpleModel"));
            Assert.That(Directory.Exists(directoryPath));

            // Then
            Assert.DoesNotThrow(() =>
            {
                // When
                var rtcModel = RealTimeControlModelXmlReader.Read(directoryPath);
                Assert.NotNull(rtcModel);
            });
        }

        [Test]
        public void GivenAnInvalidRtcDirectoryPath_WhenReading_ThenExpectedErrorMessageIsGiven()
        {
            // Given
            const string invalidPath = "InvalidPath";

            // Then
            TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    // When
                    var model = RealTimeControlModelXmlReader.Read(invalidPath);
                    Assert.IsNull(model);
                }, 
                string.Format(Resources.RealTimeControlModelXmlReader_Read_Directory___0___does_not_exist_, invalidPath));
        }


        [Test]
        public void GivenAnInvalidRtcDirectoryPath_WhenReading_ThenNoExceptionIsThrownAndNullIsReturned()
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "Invalid"));
            Assert.That(!Directory.Exists(directoryPath));

            // Then
            Assert.DoesNotThrow(() =>
            {
                // When
                var rtcModel = RealTimeControlModelXmlReader.Read(directoryPath);
                Assert.Null(rtcModel);
            });
        }
    }
}
