using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Logging;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlTimeSeriesXmlReaderTest
    {
        private const string AssertMessage_NumberOfLoggedMessagesWasExpectedToBeZero = "Number of logged messages was expected to be zero.";
        private RealTimeControlTimeSeriesXmlReader timeSeriesReader;
        private ILogHandler logHandler;
        private readonly string directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "TimeSeriesFiles"));

        [SetUp]
        public void SetUp()
        {
            logHandler = new LogHandler("");
            timeSeriesReader = new RealTimeControlTimeSeriesXmlReader(logHandler);
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
            timeSeriesReader = null;
        }

        [Test]
        public void GivenANonExistingFile_WhenReading_ThenMethodDoesNotThrowAnException()
        {
            // Given
            const string filePath = "invalid";
            Assert.That(!File.Exists(filePath),
                        $"File path '{filePath}' was expected to not exist.");

            Assert.DoesNotThrow(() => timeSeriesReader.Read(filePath, new IControlGroup[0]),
                                "Method throws an unexpected exception when the specified file path does not exist");

            Assert.AreEqual(0, logHandler.LogMessages.AllMessages.Count(),
                            AssertMessage_NumberOfLoggedMessagesWasExpectedToBeZero);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndNullIsGivenAsParameterForControlGroups_WhenReading_ThenMethodDoesNotThrowAnException()
        {
            // Given
            string filePath = Path.Combine(directoryPath, "timeseries_import.xml");
            Assert.That(File.Exists(filePath),
                        $"File path '{filePath}' was expected to exist.");

            Assert.DoesNotThrow(() => timeSeriesReader.Read(filePath, null),
                                "Method throws an unexpected exception when the parameter for control groups is null");

            Assert.AreEqual(0, logHandler.LogMessages.AllMessages.Count(),
                            AssertMessage_NumberOfLoggedMessagesWasExpectedToBeZero);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileWithValidData_WhenReading_ThenTimeSeriesIsSetOnRule()
        {
            // Given
            string filePath = Path.Combine(directoryPath, "timeseries_import.xml");
            Assert.That(File.Exists(filePath),
                        $"File path '{filePath}' was expected to exist.");

            ControlGroup controlGroup = CreateControlGroupWithATimeConditionAndTimeRule();

            // When
            timeSeriesReader.Read(filePath, new IControlGroup[]
            {
                controlGroup
            });

            // Then
            AssertTimeSeriesIsSet(controlGroup);
        }

        private static ControlGroup CreateControlGroupWithATimeConditionAndTimeRule()
        {
            var timeRule = new TimeRule("time_rule");
            var timeCondition = new TimeCondition {Name = "time_condition"};
            var controlGroup = new ControlGroup {Name = "control_group"};
            controlGroup.Conditions.Add(timeCondition);
            controlGroup.Rules.Add(timeRule);

            Assert.AreEqual(0, timeRule.TimeSeries.GetValues().Count,
                            "Expected was that the number of time series record would be zero before setting the time series on the time rule.");
            Assert.AreEqual(0, timeCondition.TimeSeries.GetValues().Count,
                            "Expected was that the number of time series record would be zero before setting the time series on the time condition.");
            return controlGroup;
        }

        private static void AssertTimeSeriesIsSet(ControlGroup controlGroup)
        {
            TimeRule timeRule = controlGroup.Rules.OfType<TimeRule>().Single();
            Assert.AreEqual(2, timeRule.TimeSeries.GetValues().Count,
                            "Expected was that the count of time series records of the time rule was 2.");

            TimeCondition timeCondition = controlGroup.Conditions.OfType<TimeCondition>().Single();
            Assert.AreEqual(3, timeCondition.TimeSeries.GetValues().Count,
                            "Expected was that the count of time series records of the time condition was 3.");
        }
    }
}