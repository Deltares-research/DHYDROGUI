using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net.Core;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Model
{
    [TestFixture]
    public class WaqHistoryFileParserTest
    {
        [Test]
        public void Parse_WithNonExistentFile_ThenCorrectErrorIsGiven()
        {
            const string fileName = "no_exist.his";
            WaterQualityObservationVariableOutput observationVariableOutput = CreateObservationVariableOutput();

            // Call
            void Call()
            {
                WaqHistoryFileParser.Parse(fileName,
                                           new List<WaterQualityObservationVariableOutput> {observationVariableOutput},
                                           MonitoringOutputLevel.Points);
            }

            // Assert
            IEnumerable<string> renderedErrorMessages = TestHelper.GetAllRenderedMessages(() => Call(), Level.Error);

            string expectedMessage = string.Format(Resources.WaqProcessorHelper_ParseHisFileData_An_error_occurred_while_reading_file,
                                                   fileName);
            Assert.That(renderedErrorMessages.Contains(expectedMessage));
        }

        [Test]
        public void Parse_WithInvalidFileFormat_ThenCorrectErrorIsGiven()
        {
            const string fileName = "file.invalid";
            WaterQualityObservationVariableOutput observationVariableOutput = CreateObservationVariableOutput();

            // Call
            void Call()
            {
                WaqHistoryFileParser.Parse(fileName,
                                           new List<WaterQualityObservationVariableOutput> {observationVariableOutput},
                                           MonitoringOutputLevel.Points);
            }

            // Assert
            IEnumerable<string> renderedErrorMessages = TestHelper.GetAllRenderedMessages(() => Call(), Level.Error);

            string expectedMessage = string.Format(Resources.WaqProcessorHelper_ParseHisFileData_Invalid_file_format,
                                                   fileName);
            Assert.That(renderedErrorMessages.Contains(expectedMessage));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Parse_WithValidNetCdfHisFile_ThenTimeSeriesDataIsSetOnObservationVariableOutputs()
        {
            // Setup
            string historyFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "deltashell_his.nc");
            WaterQualityObservationVariableOutput observationVariableOutput = CreateObservationVariableOutput();

            // Pre-condition
            Assert.That(observationVariableOutput.TimeSeriesList.All(ts => ts.Time.Values.Count == 0));

            // Call
            WaqHistoryFileParser.Parse(historyFilePath,
                                       new List<WaterQualityObservationVariableOutput> {observationVariableOutput},
                                       MonitoringOutputLevel.Points);

            // Assert
            Assert.That(observationVariableOutput.TimeSeriesList.All(ts => ts.Time.Values.Count == 7),
                        "Time series for all output variables should have data.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ParseHisFileDataWithoutSkippingSpecificOutput()
        {
            var mocks = new MockRepository();
            WaterQualityModel waterQualityModel1D = CreateWaterQualityModel1DStub(mocks);

            mocks.ReplayAll();

            string historyFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "deltashell.his");

            WaqHistoryFileParser.Parse(historyFilePath, waterQualityModel1D.ObservationVariableOutputs, waterQualityModel1D.ModelSettings.MonitoringOutputLevel);

            // Output data should be added to "O2" for all output variables
            AssertObservationVariableOutput(waterQualityModel1D, 0, 865, 865, 865, 865, 865);

            // Output data should be added to "ALL SEGMENTS" for all output variables
            AssertObservationVariableOutput(waterQualityModel1D, 1, 865, 865, 865, 865, 865);

            mocks.VerifyAll();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ParseHisFileDataWithIrrelevantObservationPointOutputConfiguration()
        {
            var mocks = new MockRepository();
            WaterQualityModel waterQualityModel1D = CreateWaterQualityModel1DStub(mocks);

            mocks.ReplayAll();

            string historyFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "deltashell.his");

            // Monitoring output level "None" => no data should be parsed from the his file
            WaqHistoryFileParser.Parse(historyFilePath, waterQualityModel1D.ObservationVariableOutputs, MonitoringOutputLevel.None);

            // No output data should be added to "O2"
            AssertObservationVariableOutput(waterQualityModel1D, 0, 0, 0, 0, 0, 0);

            // No output data should be added to "ALL SEGMENTS"
            AssertObservationVariableOutput(waterQualityModel1D, 1, 0, 0, 0, 0, 0);

            // Monitoring output level "Points" + no observation points => no data should be parsed from the his file
            WaqHistoryFileParser.Parse(historyFilePath, waterQualityModel1D.ObservationVariableOutputs.Where(v => v.ObservationVariable != null).ToList(), MonitoringOutputLevel.Points);

            // No output data should be added to "O2"
            AssertObservationVariableOutput(waterQualityModel1D, 0, 0, 0, 0, 0, 0);

            // No output data should be added to "ALL SEGMENTS"
            AssertObservationVariableOutput(waterQualityModel1D, 1, 0, 0, 0, 0, 0);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void When_ParseHisFileData_WithSinglePoint_Then_TimeSeriesValues_Are_Added()
        {
            // 1. Set up test data
            string hisFile = TestHelper.GetTestFilePath(@"BloomCase\bloom.his");
            const int expectedTimeSeriesSize = 365;
            const string obsPointName = "ObsPoint 1";
            const string timeSeriesName = "Continuity";
            WaterQualityModel waqModel = CreateBloomMockWaqModel(timeSeriesName, obsPointName);
            WaterQualityObservationVariableOutput variableOutput = waqModel.ObservationVariableOutputs.SingleOrDefault(ovo => ovo.Name.Equals(obsPointName));
            TimeSeries targetTimeSeries =
                variableOutput?.TimeSeriesList.SingleOrDefault(tsl => tsl.Name.Equals(timeSeriesName));

            // 2. Set up initial expectations
            Assert.That(File.Exists(hisFile), Is.True, "Test file was not found.");
            Assert.That(variableOutput, Is.Not.Null, "Variable output was not found.");
            Assert.That(targetTimeSeries, Is.Not.Null, "Target time series was not found.");
            Assert.That(targetTimeSeries.GetValues().Count > 0, Is.False);

            // 3. Run test
            using (var tempDir = new TemporaryDirectory())
            {
                string tempHisFile = tempDir.CopyTestDataFileToTempDirectory(hisFile);
                Assert.That(File.Exists(tempHisFile), Is.True, "Test file was not found in temporary folder.");
                WaqHistoryFileParser.Parse(tempHisFile, waqModel.ObservationVariableOutputs, MonitoringOutputLevel.Points);
            }

            // 4. Verify final expectations.
            Assert.That(targetTimeSeries.GetValues().Count, Is.EqualTo(expectedTimeSeriesSize), "Number of retrieved values does not match expectations.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void When_ParseHisFileData_WithSinglePoint_And_NoOutputVariableTuples_Then_TimeSeriesValues_IsEmpty()
        {
            // 1. Set up test data
            string hisFile = TestHelper.GetTestFilePath(@"BloomCase\bloom.his");
            WaterQualityModel waqModel = CreateBloomMockWaqModel();

            // 2. Set up initial expectations
            Assert.That(File.Exists(hisFile), Is.True, "Test file was not found.");
            Assert.That(waqModel.ObservationVariableOutputs, Is.Empty, "Not expected output variables at this point");

            // 3. Run test
            using (var tempDir = new TemporaryDirectory())
            {
                string tempHisFile = tempDir.CopyTestDataFileToTempDirectory(hisFile);
                Assert.That(File.Exists(tempHisFile), Is.True, "Test file was not found in temporary folder.");
                WaqHistoryFileParser.Parse(tempHisFile, waqModel.ObservationVariableOutputs, MonitoringOutputLevel.Points);
            }

            // 4. Verify final expectations.
            Assert.That(waqModel.ObservationVariableOutputs, Is.Empty, "Not expected output variables at this point");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void When_ParseHisFileData_WithSinglePoint_And_NoMatchingHisFileVariableData_Then_TimeSeriesValues_IsEmpty()
        {
            // 1. Set up test data
            string hisFile = TestHelper.GetTestFilePath(@"BloomCase\bloom.his");
            const int expectedTimeSeriesSize = 0;
            const string obsPointName = "FakePoint";
            const string timeSeriesName = "Continuity";
            WaterQualityModel waqModel = CreateBloomMockWaqModel(timeSeriesName, obsPointName);
            WaterQualityObservationVariableOutput variableOutput = waqModel.ObservationVariableOutputs.SingleOrDefault(ovo => ovo.Name.Equals(obsPointName));
            TimeSeries targetTimeSeries =
                variableOutput?.TimeSeriesList.SingleOrDefault(tsl => tsl.Name.Equals(timeSeriesName));

            // 2. Set up initial expectations
            Assert.That(File.Exists(hisFile), Is.True, "Test file was not found.");
            Assert.That(variableOutput, Is.Not.Null, "Variable output was not found.");
            Assert.That(targetTimeSeries, Is.Not.Null, "Target time series was not found.");
            Assert.That(targetTimeSeries.GetValues().Count > 0, Is.False);

            // 3. Run test
            using (var tempDir = new TemporaryDirectory())
            {
                string tempHisFile = tempDir.CopyTestDataFileToTempDirectory(hisFile);
                Assert.That(File.Exists(tempHisFile), Is.True, "Test file was not found in temporary folder.");
                WaqHistoryFileParser.Parse(tempHisFile, waqModel.ObservationVariableOutputs, MonitoringOutputLevel.Points);
            }

            // 4. Verify final expectations.
            Assert.That(targetTimeSeries.GetValues().Count, Is.EqualTo(expectedTimeSeriesSize), "Number of retrieved values does not match expectations.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void When_ParseHisFileData_WithSinglePoint_Variable_Not_In_TimeSeries_Then_LogMessageIsGiven()
        {
            // 1. Set up test data
            string hisFile = TestHelper.GetTestFilePath(@"BloomCase\bloom.his");
            const string outputVariableName = "ObsPoint 1";
            const string timeSeriesName = "Fake time series";
            var expectedLogMsg =
                $"Time steps are inconsistent for the data related to variable {timeSeriesName}.";
            WaterQualityModel waqModel = CreateBloomMockWaqModel(timeSeriesName, outputVariableName);
            WaterQualityObservationVariableOutput variableOutput = waqModel.ObservationVariableOutputs.SingleOrDefault(ovo => ovo.Name.Equals(outputVariableName));
            TimeSeries targetTimeSeries =
                variableOutput?.TimeSeriesList.SingleOrDefault(tsl => tsl.Name.Equals(timeSeriesName));

            // 2. Set up initial expectations
            Assert.That(File.Exists(hisFile), Is.True, "Test file was not found.");
            Assert.That(variableOutput, Is.Not.Null, "Variable output was not found.");
            Assert.That(targetTimeSeries, Is.Not.Null, "Target time series was not found.");
            Assert.That(targetTimeSeries.GetValues().Count > 0, Is.False);

            // 3. Run test
            using (var tempDir = new TemporaryDirectory())
            {
                string tempHisFile = tempDir.CopyTestDataFileToTempDirectory(hisFile);
                Assert.That(File.Exists(tempHisFile), Is.True, "Test file was not found in temporary folder.");
                void Call() => WaqHistoryFileParser.Parse(tempHisFile, waqModel.ObservationVariableOutputs, MonitoringOutputLevel.Points);

                // 4. Verify final expectations.
                TestHelper.AssertAtLeastOneLogMessagesContains(Call, expectedLogMsg);
                Assert.That(targetTimeSeries.GetValues(), Is.Empty, "Number of retrieved values does not match expectations.");
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase("hisFile\\his_with_limit_chlo.nc")]
        [TestCase("hisFile\\old_binary.his")]
        public void Parse_ReadsTimeSeriesWithSpacesCorrectly(string testDataHisPath)
        {
            using (var tempDir = new TemporaryDirectory())
            {
                string sourceHisFile = TestHelper.GetTestFilePath(testDataHisPath);
                string hisFilePath = tempDir.CopyTestDataFileToTempDirectory(sourceHisFile);
                
                var observationVariableOutputs = new List<WaterQualityObservationVariableOutput>()
                {
                    new WaterQualityObservationVariableOutput(new []{ new DelftTools.Utils.Tuple<string, string>("Limit Chlo", "-")})
                    {
                        Name = "Observation Point01"
                    }
                };

                void Call() => WaqHistoryFileParser.Parse(hisFilePath, observationVariableOutputs, MonitoringOutputLevel.Points);

                TestHelper.AssertLogMessagesCount(Call, 0);
                Assert.That(observationVariableOutputs[0].TimeSeriesList.First().GetValues(), Is.Not.Empty);
            }
        }

        private static WaterQualityObservationVariableOutput CreateObservationVariableOutput()
        {
            var outputVariableTuples = new List<DelftTools.Utils.Tuple<string, string>>
            {
                new DelftTools.Utils.Tuple<string, string>("EColi", ""),
                new DelftTools.Utils.Tuple<string, string>("Salinity", "")
            };
            return new WaterQualityObservationVariableOutput(outputVariableTuples) {Name = "Observation Point01"};
        }

        [TestCase(null)]
        [TestCase("")]
        public void Parse_WithFilePathNullOrEmpty_ThenThrowsArgumentException(string filePathArgument)
        {
            var waterQualityModel1D = new WaterQualityModel();

            void Call() => WaqHistoryFileParser.Parse(filePathArgument,
                                                      waterQualityModel1D.ObservationVariableOutputs,
                                                      waterQualityModel1D.ModelSettings.MonitoringOutputLevel);

            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("filePath"));
            Assert.That(exception.Message.StartsWith("Argument 'filePath' cannot be null or empty."));
        }

        private static WaterQualityModel CreateWaterQualityModel1DStub(MockRepository mocks)
        {
            var waterQualityModel1D = mocks.Stub<WaterQualityModel>();
            var modelSettings = new WaterQualityModelSettings {MonitoringOutputLevel = MonitoringOutputLevel.PointsAndAreas};

            var outputVariableTuples = new List<DelftTools.Utils.Tuple<string, string>>
            {
                new DelftTools.Utils.Tuple<string, string>("cTR1", ""),
                new DelftTools.Utils.Tuple<string, string>("cTR2", ""),
                new DelftTools.Utils.Tuple<string, string>("cTR3", ""),
                new DelftTools.Utils.Tuple<string, string>("cTR4", ""),
                new DelftTools.Utils.Tuple<string, string>("Continuity", "")
            };

            var observationVariableOutputs = new List<WaterQualityObservationVariableOutput>
            {
                new WaterQualityObservationVariableOutput(outputVariableTuples) {Name = "O2"},
                new WaterQualityObservationVariableOutput(outputVariableTuples) {Name = "ALL SEGMENTS"}
            };

            waterQualityModel1D.Stub(m => m.ObservationVariableOutputs).Return(observationVariableOutputs);
            waterQualityModel1D.Stub(m => m.ModelSettings).Return(modelSettings);

            return waterQualityModel1D;
        }

        private static void AssertObservationVariableOutput(WaterQualityModel waterQualityModel1D, int observationVariableOutputIndex, int cTR1ValueCount, int cTR2ValueCount, int cTR3ValueCount, int cTR4ValueCount, int continuityValueCount)
        {
            Assert.AreEqual(cTR1ValueCount, waterQualityModel1D.ObservationVariableOutputs[observationVariableOutputIndex].TimeSeriesList.ElementAt(0).GetValues().Count);
            Assert.AreEqual(cTR2ValueCount, waterQualityModel1D.ObservationVariableOutputs[observationVariableOutputIndex].TimeSeriesList.ElementAt(1).GetValues().Count);
            Assert.AreEqual(cTR3ValueCount, waterQualityModel1D.ObservationVariableOutputs[observationVariableOutputIndex].TimeSeriesList.ElementAt(2).GetValues().Count);
            Assert.AreEqual(cTR4ValueCount, waterQualityModel1D.ObservationVariableOutputs[observationVariableOutputIndex].TimeSeriesList.ElementAt(3).GetValues().Count);
            Assert.AreEqual(continuityValueCount, waterQualityModel1D.ObservationVariableOutputs[observationVariableOutputIndex].TimeSeriesList.ElementAt(4).GetValues().Count);
        }

        private static WaterQualityModel CreateBloomMockWaqModel(string timeSeriesName = null, string variableName = null)
        {
            var mocks = new MockRepository();
            var waterQualityModel1D = mocks.Stub<WaterQualityModel>();
            var modelSettings = new WaterQualityModelSettings {MonitoringOutputLevel = MonitoringOutputLevel.PointsAndAreas};

            var observationVariableOutputs = new List<WaterQualityObservationVariableOutput>();
            var outputVariableTuples = new List<DelftTools.Utils.Tuple<string, string>>();
            if (timeSeriesName != null)
            {
                outputVariableTuples.Add(new DelftTools.Utils.Tuple<string, string>(timeSeriesName, ""));
            }

            if (variableName != null)
            {
                observationVariableOutputs.Add(
                    new WaterQualityObservationVariableOutput(outputVariableTuples) {Name = variableName});
            }

            waterQualityModel1D.Stub(m => m.ObservationVariableOutputs).Return(observationVariableOutputs);
            waterQualityModel1D.Stub(m => m.ModelSettings).Return(modelSettings);
            mocks.ReplayAll();

            return waterQualityModel1D;
        }
    }
}