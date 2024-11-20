using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Model
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class WaterQualityOutputConnectorTest
    {
        [Test]
        public void Connect_WhenArgumentIsNull_ThenNoExceptionIsThrown()
        {
            // Call
            void Call() => WaterQualityOutputConnector.Connect(null);

            // Assert
            Assert.DoesNotThrow(Call, "No exception should be thrown when argument is null.");
        }

        [Test]
        public void Connect_WhenOutputFolderOfModelIsNull_ThenNoExceptionIsThrown()
        {
            // Setup
            using (var model = new WaterQualityModel())
            {
                model.OutputFolder = null;

                // Call
                void Call() => WaterQualityOutputConnector.Connect(model);

                // Assert
                Assert.DoesNotThrow(Call, $"No exception should be thrown when {nameof(model.OutputFolder)} is null.");
            }
        }

        [Test]
        public void Connect_WhenBothMapFilesExist_ThenModelIsConnectedToTheNetCdfFile()
        {
            // Set-up
            using (var tempDir = new TemporaryDirectory())
            {
                string outputDirectory = tempDir.Path;
                string mapNetCdfFilePath = tempDir.CopyAllTestDataToTempDirectory(
                        Path.Combine("IO", "deltashell_map.nc"),
                        Path.Combine("IO", "deltashell.map"))
                    [0];

                using (var model = new WaterQualityModel {ModelSettings = {WorkingOutputDirectory = outputDirectory}})
                {
                    model.OutputFolder = new FileBasedFolder(outputDirectory);

                    // Call
                    WaterQualityOutputConnector.Connect(model);

                    // Assert
                    Assert.That(model.MapFileFunctionStore.Path, Is.EqualTo(mapNetCdfFilePath),
                                "When connecting to the output and both map files (.nc, .map) exist, the model should be connected to the .nc file.");
                }
            }
        }

        [Test]
        public void Connect_WithNetCdfFileWithUnsupportedConvention_ThenFileIsNotConnectedAndWarningIsGiven()
        {
            // Set-up
            using (var tempDir = new TemporaryDirectory())
            {
                string outputDirectory = tempDir.Path;
                string filePath = tempDir.CopyTestDataFileToTempDirectory(Path.Combine("IO",
                                                                                       "NetCDFConventions",
                                                                                       "CF1.5_UGRID0.9.nc"));

                using (var model = new WaterQualityModel {ModelSettings = {WorkingOutputDirectory = outputDirectory}})
                {
                    string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), "deltashell_map.nc");
                    File.Move(filePath, newFilePath);
                    model.OutputFolder = new FileBasedFolder(outputDirectory);

                    // Call
                    void Call() => WaterQualityOutputConnector.Connect(model);

                    // Assert
                    IEnumerable<string> warningMessages = TestHelper.GetAllRenderedMessages(Call, Level.Warn);
                    string expectedWarning = string.Format(
                        Resources.WaterQualityModel_File_does_not_meet_supported_UGRID_1_0_or_newer_standard,
                        Path.GetFileName(newFilePath));
                    Assert.That(warningMessages, Contains.Item(expectedWarning),
                                "When connecting the output, a warning should be given when convention of the NetCdf file is not supported.");
                    Assert.That(model.MapFileFunctionStore.Path, Is.Null,
                                "When connecting the output and the NetCdf map file has an unsupported convention, " +
                                $"then the model's {nameof(model.MapFileFunctionStore)} path should be 'null'.");
                }
            }
        }

        [Test]
        public void Connect_WhenBothHistoryFilesExist_ThenNetCdfFileIsConnected()
        {
            // Setup
            WaterQualityObservationVariableOutput observationVariableOutputForNcFile = CreateObservationVariableOutput(true);
            WaterQualityObservationVariableOutput observationVariableOutputForHisFile = CreateObservationVariableOutput(false);

            using (var tempDirectory = new TemporaryDirectory())
            {
                tempDirectory.CopyAllTestDataToTempDirectory(
                    Path.Combine("IO", "deltashell_his.nc"),
                    Path.Combine("IO", "deltashell.his"));

                using (var model = new WaterQualityModel())
                {
                    model.OutputFolder = new FileBasedFolder(tempDirectory.Path);
                    model.ObservationVariableOutputs.Add(observationVariableOutputForNcFile);
                    model.ObservationVariableOutputs.Add(observationVariableOutputForHisFile);

                    // Preconditions
                    Assert.That(observationVariableOutputForNcFile.TimeSeriesList, Has.None.Matches<TimeSeries>(IsTimeSeriesWithValues),
                                "This test is unreliable when the time series data is not empty.");
                    Assert.That(observationVariableOutputForHisFile.TimeSeriesList, Has.None.Matches<TimeSeries>(IsTimeSeriesWithValues),
                                "This test is unreliable when the time series data is not empty.");

                    // Call
                    WaterQualityOutputConnector.Connect(model);

                    // Assert
                    Assert.That(observationVariableOutputForNcFile.TimeSeriesList, Has.All.Matches<TimeSeries>(IsTimeSeriesWithValues),
                                "When connecting the output to and both history files (.nc, .his) exist, then the NetCdf file should be read.");
                    Assert.That(observationVariableOutputForHisFile.TimeSeriesList, Has.None.Matches<TimeSeries>(IsTimeSeriesWithValues),
                                "When connecting the output to and both history files (.nc, .his) exist, then the .his file should NOT be read.");
                }
            }
        }

        [TestCase(null)]
        [TestCase("this_path_does_not_exist")]
        public void Connect_WhenOutputFolderPathDoesNotExist_ThenNoExceptionIsThrown(string outputFolderPath)
        {
            // Setup
            using (var model = new WaterQualityModel())
            {
                model.OutputFolder = new FileBasedFolder(outputFolderPath);

                // Call
                void Call() => WaterQualityOutputConnector.Connect(model);

                // Assert
                Assert.DoesNotThrow(Call, $"No exception should be thrown when Path of {nameof(model.OutputFolder)} does not exist.");
            }
        }

        [TestCase("deltashell.map")]
        [TestCase("deltashell_map.nc")]
        public void Connect_WithExistingMapFile_ThenModelIsConnectedToThisFile(string fileName)
        {
            // Set-up
            using (var tempDir = new TemporaryDirectory())
            {
                string outputDirectory = tempDir.Path;
                string filePath = tempDir.CopyTestDataFileToTempDirectory(Path.Combine("IO", fileName));

                using (var model = new WaterQualityModel {ModelSettings = {WorkingOutputDirectory = outputDirectory}})
                {
                    model.OutputFolder = new FileBasedFolder(outputDirectory);

                    // Call
                    WaterQualityOutputConnector.Connect(model);

                    // Assert
                    Assert.That(model.MapFileFunctionStore.Path, Is.EqualTo(filePath),
                                $"When connecting to the output, the model should be connected to the only existing map file {filePath}");
                }
            }
        }

        [TestCase("deltashell-bal.prn", "BalanceOutputTag")]
        [TestCase("deltashell.mon", "MonitoringFileTag")]
        [TestCase("deltashell.lsp", "ProcessFileTag")]
        [TestCase("deltashell.lst", "ListFileTag")]
        public void Connect_ThenTextFilesAreConnected(string fileName, string dataItemTag)
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string outputDirectoryPath = tempDirectory.Path;
                string filePath = Path.Combine(outputDirectoryPath, fileName);
                const string content = "content";

                File.WriteAllText(filePath, content);

                using (var model = new WaterQualityModel())
                {
                    model.OutputFolder = new FileBasedFolder(outputDirectoryPath);

                    // Call
                    WaterQualityOutputConnector.Connect(model);

                    // Assert
                    IDataItem dataItem = model.GetDataItemByTag(dataItemTag);
                    Assert.That(dataItem, Is.Not.Null,
                                $"When connecting the output, the data item that belongs to the existing text file {fileName}, should be added to the model.");
                    var textDocument = (TextDocument) dataItem.Value;
                    Assert.That(textDocument.Content, Is.EqualTo(content),
                                $"When connecting the output, the content of the file {fileName} should be set on the corresponding {nameof(TextDocument)} object.");
                }
            }
        }

        [TestCase("deltashell_his.nc", true)]
        [TestCase("deltashell.his", false)]
        public void Connect_ThenHistoryFileIsConnected(string fileName, bool isNetCdfFile)
        {
            // Setup
            WaterQualityObservationVariableOutput observationVariableOutput = CreateObservationVariableOutput(isNetCdfFile);

            using (var tempDirectory = new TemporaryDirectory())
            {
                tempDirectory.CopyTestDataFileToTempDirectory(Path.Combine("IO", fileName));

                using (var model = new WaterQualityModel())
                {
                    model.OutputFolder = new FileBasedFolder(tempDirectory.Path);
                    model.ObservationVariableOutputs.Add(observationVariableOutput);

                    // Precondition
                    Assert.That(observationVariableOutput.TimeSeriesList, Has.None.Matches<TimeSeries>(IsTimeSeriesWithValues),
                                "This test is unreliable when the time series data is not empty.");

                    // Call
                    WaterQualityOutputConnector.Connect(model);

                    // Assert
                    Assert.That(observationVariableOutput.TimeSeriesList, Has.All.Matches<TimeSeries>(IsTimeSeriesWithValues),
                                "When connecting the output to a history file, then the time series data of the observation variable output should not be empty anymore.");
                }
            }
        }

        private static WaterQualityObservationVariableOutput CreateObservationVariableOutput(bool forNetCdfFile)
        {
            if (forNetCdfFile)
            {
                var outputVariableTuples = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("EColi", ""),
                    new Tuple<string, string>("Salinity", "")
                };
                return new WaterQualityObservationVariableOutput(outputVariableTuples) {Name = "Observation Point01"};
            }
            else
            {
                var outputVariableTuples = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("cTR1", ""),
                    new Tuple<string, string>("cTR2", ""),
                    new Tuple<string, string>("cTR3", ""),
                    new Tuple<string, string>("cTR4", ""),
                    new Tuple<string, string>("Continuity", "")
                };

                return new WaterQualityObservationVariableOutput(outputVariableTuples) {Name = "O2"};
            }
        }

        private static bool IsTimeSeriesWithValues(ITimeSeries ts)
        {
            return ts.Time.Values.Any();
        }
    }
}