using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    class RealTimeControlDataConfigXmlReaderTest
    {
        [Test]
        public void GivenANonExistingFile_WhenReading_ThenExpectedMessageIsGiven()
        {
            // Given
            var filePath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "Invalid"));
            Assert.That(!File.Exists(filePath));

            var controlGroups = new List<ControlGroup>();

            Assert.DoesNotThrow(() =>
            {
                TestHelper.AssertLogMessageIsGenerated(() =>
                    {
                        // When
                        var connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, controlGroups);
                        Assert.IsNull(connectionPoints);
                    },
                    string.Format(Resources.RealTimeControlDataConfigXmlReader_Read_File___0___does_not_exist_, filePath));
            });

            // Then
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndNullIsGivenAsParameterForControlgroups_WhenReading_ThenMethodIsReturnedAndNothingHappens()
        {
            // Given
            var fileName = "rtcDataConfig.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "DataConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));
            Assert.That(File.Exists(filePath));

            Assert.DoesNotThrow(() =>
            {
                var connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, null);
                Assert.IsNull(connectionPoints);
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileWithValidData_WhenReading_ThenCorrectConnectionPointsAreReturnedAndObjectsAreSetInControlgroups()
        {
            // Given
            var fileName = "rtcDataConfig.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "DataConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));
            Assert.That(File.Exists(filePath));

            var controlGroups = new List<ControlGroup>();

            // When
            var connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, controlGroups);

            // Then
            Assert.NotNull(connectionPoints);
            Assert.AreEqual(2, connectionPoints.Count);

            var inputs = connectionPoints.OfType<Input>().ToList();
            Assert.NotNull(inputs);
            Assert.AreEqual(1, inputs.Count);

            var input = inputs[0];
            Assert.AreEqual("[Input]parameter/quantity", input.Name);
            Assert.AreEqual(null, input.ParameterName);
            Assert.AreEqual(string.Empty, input.LocationName);

            var outputs = connectionPoints.OfType<Output>().ToList();
            Assert.NotNull(outputs);
            Assert.AreEqual(1, outputs.Count);

            var output = outputs[0];
            Assert.AreEqual("[Output]parameter/quantity", output.Name);
            Assert.AreEqual(null, output.ParameterName);
            Assert.AreEqual(string.Empty, output.LocationName);

            Assert.IsNotEmpty(controlGroups);
            Assert.AreEqual(1, controlGroups.Count);

            var controlGroup = controlGroups[0];

            var conditions = controlGroup.Conditions;
            Assert.AreEqual(2, conditions.Count);

            var timeConditions = conditions.OfType<TimeCondition>().ToList();
            Assert.NotNull(timeConditions);
            Assert.AreEqual(1, timeConditions.Count);

            var timeCondition = timeConditions[0];
            Assert.AreEqual("time_condition", timeCondition.Name);
            Assert.AreEqual(InterpolationType.Constant, timeCondition.InterpolationOptionsTime);
            Assert.AreEqual(ExtrapolationType.Constant, timeCondition.Extrapolation);

            var standardConditions = conditions.OfType<StandardCondition>()
                .Where(c => c.GetType() != typeof(TimeCondition)).ToList();
            Assert.NotNull(standardConditions);
            Assert.AreEqual(1, standardConditions.Count);
            Assert.AreEqual("standard_condition", standardConditions[0].Name);
    
            var rules = controlGroup.Rules;
            Assert.AreEqual(2, rules.Count);

            var timeRules = rules.OfType<TimeRule>().ToList();
            Assert.NotNull(timeRules);
            Assert.AreEqual(1, timeRules.Count);

            var timeRule = timeRules[0];
            Assert.AreEqual("time_rule", timeRule.Name);
            Assert.AreEqual(InterpolationType.Linear, timeRule.InterpolationOptionsTime);

            var relativeTimeRules = rules.OfType<RelativeTimeRule>().ToList();
            Assert.NotNull(relativeTimeRules);
            Assert.AreEqual(1, relativeTimeRules.Count);

            var relativeTimeRule = relativeTimeRules[0];
            Assert.AreEqual("relative_time_rule", relativeTimeRule.Name);
            Assert.AreEqual(InterpolationType.Linear, relativeTimeRule.Interpolation);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndWithoutTimeSeriesElements_WhenReading_ThenExpectedMessageIsGivenAndNullIsReturned()
        {
            // Given
            var fileName = "rtcDataConfig_noelements.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "DataConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));
            Assert.That(File.Exists(filePath));

            var controlGroups = new List<ControlGroup>();

            TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    // When
                    var connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, controlGroups);
                    Assert.IsNull(connectionPoints);
                },
                string.Format(Resources.RealTimeControlDataConfigXmlReader_Read_File___0___seems_to_be_empty_, filePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndWithoutReadableControlGroupNames_WhenReading_ThenExpectedMessageIsGivenAndNullIsReturned()
        {
            // Given
            var fileName = "rtcDataConfig_nocontrolgroupnames.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "DataConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));
            Assert.That(File.Exists(filePath));

            var controlGroups = new List<ControlGroup>();

            TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    // When
                    var connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, controlGroups);
                    Assert.IsNull(connectionPoints);
                },
                string.Format(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_control_groups_from_file___0___, filePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndWithoutInputsOrOutputs_WhenReading_ThenExpectedMessageIsGivenAndNullIsReturned()
        {
            // Given
            var fileName = "rtcDataConfig_noconnectionpoints.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "DataConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));
            Assert.That(File.Exists(filePath));

            var controlGroups = new List<ControlGroup>();

            TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    // When
                    var connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, controlGroups);
                    Assert.IsNull(connectionPoints);
                },
                string.Format(Resources.RealTimeControlDataConfigXmlReader_Read_Could_not_read_connection_points_from_file___0___, filePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileForRmmModel_WhenReading_ThenNoExceptionIsThrown()
        {
            // Given
            var fileName = "rtcDataConfig.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "RMM"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));
            Assert.That(File.Exists(filePath));

            var controlGroups = new List<ControlGroup>();
            IList<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();

            Assert.DoesNotThrow(() =>
            {
               connectionPoints = RealTimeControlDataConfigXmlReader.Read(filePath, controlGroups);
            });

            Assert.AreEqual(23, controlGroups.Count);
            Assert.AreEqual(23, connectionPoints.OfType<Input>().Count());
            Assert.AreEqual(31, connectionPoints.OfType<Output>().Count());
            Assert.AreEqual(43, controlGroups.SelectMany(c => c.Conditions).Count());
            Assert.AreEqual(53, controlGroups.SelectMany(c => c.Rules).Count());

            // Sample
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

