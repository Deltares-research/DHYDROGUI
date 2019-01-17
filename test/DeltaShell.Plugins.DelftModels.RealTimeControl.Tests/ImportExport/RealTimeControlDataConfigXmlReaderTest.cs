using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlDataConfigXmlReaderTest
    {
        [Test]
        public void GivenANonExistingFile_WhenReading_ThenExpectedMessageIsGiven()
        {
            // Given
            var filePath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "Invalid"));
            Assert.That(!File.Exists(filePath));

            var controlGroups = new List<ControlGroup>();

            IList<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
            Assert.DoesNotThrow(() =>
            {
                TestHelper.AssertLogMessageIsGenerated(() =>
                    {
                        // When
                        connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, controlGroups);
                    },
                    string.Format(Resources.RealTimeControlDataConfigXmlReader_Read_File___0___does_not_exist_, filePath));
            });

            // Then
            Assert.IsNull(connectionPoints);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndNullIsGivenAsParameterForControlgroups_WhenReading_ThenNullIsReturnedAndNoExceptionIsThrown()
        {
            // Given
            const string fileName = "rtcDataConfig.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "DataConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);
            
            Assert.That(File.Exists(filePath));

            // When/Then
            IList<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
            Assert.DoesNotThrow(() => connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, null));
            Assert.IsNull(connectionPoints);
        }

        [Test]
        [Category(TestCategory.Jira)] // SOBEK3-1651
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileWithValidData_WhenReading_ThenCorrectConnectionPointsAreReturnedAndObjectsAreSetInControlgroups()
        {
            // Given
            const string fileName = "rtcDataConfig.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "DataConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);
            
            Assert.That(File.Exists(filePath));

            var controlGroups = new List<ControlGroup>();

            // When
            var connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, controlGroups);

            // Then
            CheckControlGroupValidity(controlGroups);
            CheckConnectionPointsValidity(connectionPoints);
        }

        private static void CheckControlGroupValidity(IList<ControlGroup> controlGroups)
        {
            Assert.IsNotEmpty(controlGroups);
            Assert.AreEqual(1, controlGroups.Count);

            var controlGroup = controlGroups[0];
            CheckControlGroupConditionsValidity(controlGroup);
            CheckControlGroupRulesValidity(controlGroup);
        }

        private static void CheckControlGroupConditionsValidity(IControlGroup controlGroup)
        {
            var conditions = controlGroup.Conditions;
            Assert.AreEqual(2, conditions.Count);

            CheckTimeConditionValidity(conditions);
            CheckStandardConditionValidity(conditions);
        }

        private static void CheckTimeConditionValidity(IEventedList<ConditionBase> conditions)
        {
            var timeConditions = conditions.OfType<TimeCondition>().ToList();
            Assert.NotNull(timeConditions);
            Assert.AreEqual(1, timeConditions.Count);

            var timeCondition = timeConditions[0];
            Assert.AreEqual("time_condition", timeCondition.Name);
            Assert.AreEqual(InterpolationType.Constant, timeCondition.InterpolationOptionsTime);
            Assert.AreEqual(ExtrapolationType.Constant, timeCondition.Extrapolation);
        }

        private static void CheckStandardConditionValidity(IEventedList<ConditionBase> conditions)
        {
            var standardConditions = conditions.OfType<StandardCondition>()
                .Where(c => c.GetType() != typeof(TimeCondition)).ToList();
            Assert.NotNull(standardConditions);
            Assert.AreEqual(1, standardConditions.Count);
            Assert.AreEqual("standard_condition", standardConditions[0].Name);
        }

        private static void CheckControlGroupRulesValidity(IControlGroup controlGroup)
        {
            var rules = controlGroup.Rules;
            Assert.AreEqual(2, rules.Count);

            CheckTimeRuleValidity(rules);
            CheckRelativeTimeRuleValidity(rules);
        }

        private static void CheckTimeRuleValidity(IEnumerable<RuleBase> rules)
        {
            var timeRules = rules.OfType<TimeRule>().ToList();
            Assert.NotNull(timeRules);
            Assert.AreEqual(1, timeRules.Count);

            var timeRule = timeRules[0];
            Assert.AreEqual("time_rule", timeRule.Name);
            Assert.AreEqual(InterpolationType.Linear, timeRule.InterpolationOptionsTime);
        }

        private static void CheckRelativeTimeRuleValidity(IEnumerable<RuleBase> rules)
        {
            var relativeTimeRules = rules.OfType<RelativeTimeRule>().ToList();
            Assert.NotNull(relativeTimeRules);
            Assert.AreEqual(1, relativeTimeRules.Count);

            var relativeTimeRule = relativeTimeRules[0];
            Assert.AreEqual("relative_time_rule", relativeTimeRule.Name);
            Assert.AreEqual(InterpolationType.Linear, relativeTimeRule.Interpolation);
        }

        private static void CheckConnectionPointsValidity(IList<ConnectionPoint> connectionPoints)
        {
            Assert.NotNull(connectionPoints);
            Assert.AreEqual(2, connectionPoints.Count);

            CheckInputConnectionPoint(connectionPoints);
            CheckOutputConnectionPoint(connectionPoints);
        }

        private static void CheckInputConnectionPoint(IList<ConnectionPoint> connectionPoints)
        {
            var inputs = connectionPoints.OfType<Input>().ToList();
            Assert.NotNull(inputs);
            Assert.AreEqual(1, inputs.Count);

            var input = inputs[0];
            Assert.AreEqual("[Input]parameter/quantity", input.Name);
            Assert.AreEqual(null, input.ParameterName);
            Assert.AreEqual(string.Empty, input.LocationName);
        }

        private static void CheckOutputConnectionPoint(IList<ConnectionPoint> connectionPoints)
        {
            var outputs = connectionPoints.OfType<Output>().ToList();
            Assert.NotNull(outputs);
            Assert.AreEqual(1, outputs.Count);

            var output = outputs[0];
            Assert.AreEqual("[Output]parameter/quantity", output.Name);
            Assert.AreEqual(null, output.ParameterName);
            Assert.AreEqual(string.Empty, output.LocationName);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndWithoutTimeSeriesElements_WhenReading_ThenExpectedMessageIsGivenAndNullIsReturned()
        {
            // Given
            const string fileName = "rtcDataConfig_noelements.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "DataConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(File.Exists(filePath));

            TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    // When
                    var controlGroups = new List<ControlGroup>();
                    var connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, controlGroups);

                    // Then
                    Assert.IsNull(connectionPoints);
                },
                string.Format(Resources.RealTimeControlDataConfigXmlReader_Read_File___0___seems_to_be_empty_, filePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndWithoutReadableControlGroupNames_WhenReading_ThenExpectedMessageIsGivenAndNullIsReturned()
        {
            // Given
            const string fileName = "rtcDataConfig_nocontrolgroupnames.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "DataConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(File.Exists(filePath));

            TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    // When
                    var controlGroups = new List<ControlGroup>();
                    var connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, controlGroups);
                    
                    // Then
                    Assert.IsNull(connectionPoints);
                },
                string.Format(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_control_groups_from_file___0___, filePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndWithoutInputsOrOutputs_WhenReading_ThenExpectedMessageIsGivenAndNullIsReturned()
        {
            // Given
            const string fileName = "rtcDataConfig_noconnectionpoints.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "DataConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(File.Exists(filePath));

            TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    // When
                    var controlGroups = new List<ControlGroup>();
                    var connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, controlGroups);

                    // Then
                    Assert.IsNull(connectionPoints);
                },
                string.Format(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_connection_points_from_file___0___, filePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileForRmmModel_WhenReading_ThenNoExceptionIsThrown()
        {
            // Given
            const string fileName = "rtcDataConfig.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "RMM"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(File.Exists(filePath));

            // When
            var controlGroups = new List<ControlGroup>();
            IList<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
            Assert.DoesNotThrow(() =>
            {
               connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, controlGroups);
            });

            // Then
            Assert.AreEqual(23, controlGroups.Count);
            Assert.AreEqual(23, connectionPoints.OfType<Input>().Count());
            Assert.AreEqual(31, connectionPoints.OfType<Output>().Count());
            Assert.AreEqual(43, controlGroups.SelectMany(c => c.Conditions).Count());
            Assert.AreEqual(53, controlGroups.SelectMany(c => c.Rules).Count());

            CheckSampleControlGroupValidity(controlGroups);
        }

        private static void CheckSampleControlGroupValidity(List<ControlGroup> controlGroups)
        {
            // We take one sample of the control groups and test its validity
            var controlGroup = controlGroups.FirstOrDefault(cg => cg.Name == "HollandscheIJsselkering");
            Assert.NotNull(controlGroup);

            Assert.AreEqual(0, controlGroup.Inputs.Count);
            Assert.AreEqual(0, controlGroup.Outputs.Count);

            var conditions = controlGroup.Conditions;
            Assert.AreEqual(5, conditions.Count);
            Assert.AreEqual(2, conditions.OfType<TimeCondition>().Count());
            Assert.AreEqual(3, conditions.OfType<StandardCondition>().Count(c => c.GetType() != typeof(TimeCondition)));

            var rules = controlGroup.Rules;
            Assert.AreEqual(4, rules.Count);
            Assert.AreEqual(1, rules.OfType<TimeRule>().Count());
            Assert.AreEqual(3, rules.OfType<RelativeTimeRule>().Count());
        }
    }
}

