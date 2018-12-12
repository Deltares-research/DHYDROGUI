using System;
using System.IO;
using System.Linq;
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
        public void GivenAValidRtcDirectoryPath_WhenReadingAllTheFiles_TheExpectedRtcModelIsReturned_SimpleModel()
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "RealTimeControlModelXmlReader", "SimpleModel" ));
            Assert.That(Directory.Exists(directoryPath));

            // When
            var rtcModel = RealTimeControlModelXmlReader.Read(directoryPath);

            // Then
            Assert.NotNull(rtcModel);

            Assert.AreEqual(true, rtcModel.LimitMemory);

            Assert.AreEqual(new DateTime(2018, 12, 12, 0, 0, 0), rtcModel.StartTime);
            Assert.AreEqual(new DateTime(2018, 12, 13, 0, 0, 0), rtcModel.StopTime);
            Assert.AreEqual(new TimeSpan(0, 30, 0), rtcModel.TimeStep);

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
        public void GivenAValidRtcDirectoryPath_WhenReadingAllTheFiles_TheExpectedRtcModelIsReturned_RMM()
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "RealTimeControlModelXmlReader", "RMM"));
            Assert.That(Directory.Exists(directoryPath));

            // When
            var rtcModel = RealTimeControlModelXmlReader.Read(directoryPath);

            // Then
            Assert.NotNull(rtcModel);

            Assert.AreEqual(false, rtcModel.LimitMemory);

            Assert.AreEqual(new DateTime(1991, 1, 5, 3, 20, 0), rtcModel.StartTime);
            Assert.AreEqual(new DateTime(1991, 1, 9, 0, 0, 0), rtcModel.StopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0), rtcModel.TimeStep);

            Assert.AreEqual(23, rtcModel.ControlGroups.Count);

            var controlGroups = rtcModel.ControlGroups;

            var allInputs = controlGroups.SelectMany(c => c.Inputs);
            Assert.AreEqual(23, allInputs.Count());

            var allOutputs = controlGroups.SelectMany(c => c.Outputs);
            Assert.AreEqual(31, allOutputs.Count());

            var allConditions = controlGroups.SelectMany(c => c.Conditions);
            Assert.AreEqual(43, allConditions.Count());

            var allRules = controlGroups.SelectMany(c => c.Rules);
            Assert.AreEqual(53, allRules.Count());

            // Sample
            var controlGroupHi = controlGroups.FirstOrDefault(cg => cg.Name == "HollandscheIJsselkering");
            Assert.NotNull(controlGroupHi);

            var inputs = controlGroupHi.Inputs;
            Assert.AreEqual(3, inputs.Count);

            var outputs = controlGroupHi.Outputs;
            Assert.AreEqual(1, outputs.Count);

            var conditions = controlGroupHi.Conditions;
            Assert.AreEqual(5, conditions.Count);

            var timeCondition = conditions.OfType<TimeCondition>().ToList();
            Assert.NotNull(timeCondition);
            Assert.AreEqual(2, timeCondition.Count);

            var standardConditions = conditions.OfType<StandardCondition>().Where(c => c.GetType() != typeof(TimeCondition)).ToList();
            Assert.NotNull(standardConditions);
            Assert.AreEqual(3, standardConditions.Count);

            var rules = controlGroupHi.Rules;
            Assert.AreEqual(4, rules.Count);

            var timeRules = rules.OfType<TimeRule>().ToList();
            Assert.NotNull(timeRules);
            Assert.AreEqual(1, timeRules.Count);

            var relativeTimeRules = rules.OfType<RelativeTimeRule>().ToList();
            Assert.NotNull(relativeTimeRules);
            Assert.AreEqual(3, relativeTimeRules.Count);
        }

        [TestCase("SimpleModel")]
        [TestCase("RMM")]
        public void GivenAValidRtcDirectoryPath_WhenReadingAllTheFiles_ThenNoExceptionIsThrown(string directoryName)
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "RealTimeControlModelXmlReader", directoryName));
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
            var path = "InvalidPath";

			// Then
            TestHelper.AssertLogMessageIsGenerated(() =>
                {
					// When
                    var model = RealTimeControlModelXmlReader.Read(path);
                    Assert.IsNull(model);
                }, 
                string.Format(Resources.RealTimeControlModelXmlReader_Read_Directory___0___does_not_exist_, path));
        }


        [Test]
        public void GivenAnInvalidRtcDirectoryPath_WhenReading_ThenNoExceptionIsThrownAndNullIsReturned()
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "RealTimeControlModelXmlReader", "Invalid"));
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
