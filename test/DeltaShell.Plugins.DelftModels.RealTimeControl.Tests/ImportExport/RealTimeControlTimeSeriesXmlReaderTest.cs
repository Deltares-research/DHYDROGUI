using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlTimeSeriesXmlReaderTest
    {
        [Test]
        public void GivenANonExistingFile_WhenReading_ThenExpectedMessageIsGiven()
        {
            // Given
            var filePath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "Invalid"));
            Assert.That(!File.Exists(filePath));

            var controlGroups = new List<IControlGroup>();

            // Then
            TestHelper.AssertLogMessageIsGenerated(() =>
            {
                // When
                RealTimeControlTimeSeriesXmlReader.Read(filePath, controlGroups);
            },
                string.Format(Resources.RealTimeControlTimeSeriesXmlReader_Read_File___0___does_not_exist_, filePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndNullIsGivenAsParameterForControlgroups_WhenReading_ThenMethodDoesNotThrowAnException()
        {
            // Given
            const string fileName = "timeseries_import.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "TimeSeriesFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(File.Exists(filePath));

            Assert.DoesNotThrow(() =>
            {
                // When
                RealTimeControlTimeSeriesXmlReader.Read(filePath, null);
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileWithValidData_WhenReading_ThenCorrectTimeSeriesAreSet()
        {
            // Given
            const string fileName = "timeseries_import.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "TimeSeriesFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(File.Exists(filePath));

            var controlGroup = new ControlGroup { Name = "control_group" };
            var timeRule = new TimeRule("time_rule");
            var timeCondition = new TimeCondition { Name = "time_condition" };

            controlGroup.Conditions.Add(timeCondition);
            controlGroup.Rules.Add(timeRule);

            var controlGroups = new List<IControlGroup> { controlGroup };

            // When
            RealTimeControlTimeSeriesXmlReader.Read(filePath, controlGroups);

            // Then
            var timeRuleTimeSeries = timeRule.TimeSeries;
            Assert.AreEqual(2, timeRuleTimeSeries.GetValues().Count);

            var timeRuleTimeSeriesDates = timeRuleTimeSeries.Arguments[0].Values;
            Assert.AreEqual(new DateTime(2018, 12, 12), timeRuleTimeSeriesDates[0]);
            Assert.AreEqual(new DateTime(2018, 12, 13), timeRuleTimeSeriesDates[1]);

            var timeRuleTimeSeriesValues = timeRuleTimeSeries.Components[0].Values;
            Assert.AreEqual(0.5, timeRuleTimeSeriesValues[0]);
            Assert.AreEqual(100.001, timeRuleTimeSeriesValues[1]);

            var timeConditionTimeSeries = timeCondition.TimeSeries;
            Assert.AreEqual(3, timeConditionTimeSeries.GetValues().Count);

            var timeConditionTimeSeriesDates = timeConditionTimeSeries.Arguments[0].Values;
            Assert.AreEqual(new DateTime(2018, 12, 12), timeConditionTimeSeriesDates[0]);
            Assert.AreEqual(new DateTime(2018, 12, 12, 12, 0, 0), timeConditionTimeSeriesDates[1]);
            Assert.AreEqual(new DateTime(2018, 12, 13), timeConditionTimeSeriesDates[2]);

            var timeConditionTimeSeriesValues = timeConditionTimeSeries.Components[0].Values;
            Assert.AreEqual(false, timeConditionTimeSeriesValues[0]);
            Assert.AreEqual(true, timeConditionTimeSeriesValues[1]);
            Assert.AreEqual(false, timeConditionTimeSeriesValues[2]);
        }
    }
}
