using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Model
{
    [TestFixture]
    public class WaqFileBasedProcessorTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void InitializeShouldClearOutputFiles()
        {
            var workDir = FileUtils.CreateTempDirectory() + @"\output\";
            FileUtils.CreateDirectoryIfNotExists(workDir);

            try
            {
                var pathMapFile = workDir + "deltashell.map";
                var pathHisFile = workDir + "deltashell.his";
                var pathBalanceOutputFile = workDir + "deltashell-bal.prn";
                var pathMonitoringFile = workDir + "deltashell.mon";
                var mapFileStream = File.Create(pathMapFile);
                var hisFileStream = File.Create(pathHisFile);
                var balanceOutputFileStream = File.Create(pathBalanceOutputFile);
                var monitoringFileStream = File.Create(pathMonitoringFile);

                mapFileStream.Close();
                hisFileStream.Close();
                balanceOutputFileStream.Close();
                monitoringFileStream.Close();

                var waqExecutionSettings = new WaqInitializationSettings { Settings = { OutputDirectory = workDir} };

                var waqFileBasedProcessor = new WaqFileBasedProcessor();

                Assert.IsTrue(File.Exists(pathMapFile));
                Assert.IsTrue(File.Exists(pathHisFile));
                Assert.IsTrue(File.Exists(pathBalanceOutputFile));
                Assert.IsTrue(File.Exists(pathMonitoringFile));

                waqFileBasedProcessor.Initialize(waqExecutionSettings);

                Assert.IsFalse(File.Exists(pathMapFile));
                Assert.IsFalse(File.Exists(pathHisFile));
                Assert.IsFalse(File.Exists(pathBalanceOutputFile));
                Assert.IsFalse(File.Exists(pathMonitoringFile));
            }
            finally
            {
                FileUtils.DeleteIfExists(workDir);
            }
        }

        [Test]
        public void AddOutputWithoutWorkingDirectory()
        {
            TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    var waqFileBasedProcessor = new WaqFileBasedProcessor();
                    waqFileBasedProcessor.AddOutput(null, Enumerable.Empty<WaterQualityObservationVariableOutput>().ToList(),(s, s1) => { }, MonitoringOutputLevel.PointsAndAreas);
                }, "Could not add output because work directory is empty.");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddOutputShouldAddTextDocuments()
        {
            var workDirectory = TestHelper.GetTestDataDirectory();
            var substanceNames = new[] { "CBOD5", "CBOD5_2", "Continuity", "NH4", "OXY", "SOD" };
            var mocks = new MockRepository();
            
            var substances = substanceNames.Select(n =>
                {
                    var substanceMock = mocks.Stub<WaterQualitySubstance>();
                    substanceMock.Name = n;
                    substanceMock.ConcentrationUnit = "";
                    return substanceMock;
                });

            var subProcLib = mocks.Stub<SubstanceProcessLibrary>(new EventedList<WaterQualitySubstance>(substances));
            var model = mocks.Stub<WaterQualityModel>();

            model.DataItems = new EventedList<IDataItem>();

            TypeUtils.SetPrivatePropertyValue(model, "ModelSettings", new WaterQualityModelSettings());

            model.ModelSettings.WorkDirectory = workDirectory;
            model.ModelSettings.MonitoringOutputLevel = MonitoringOutputLevel.PointsAndAreas;

            model.Stub(m => m.SubstanceProcessLibrary).Return(subProcLib);

            mocks.ReplayAll();

            var waqFileBasedProcessor = new WaqFileBasedProcessor();

            waqFileBasedProcessor.AddOutput(model.ModelSettings.WorkDirectory, Enumerable.Empty<WaterQualityObservationVariableOutput>().ToList(), (name, path) => model.AddTextDocument(name, path), MonitoringOutputLevel.PointsAndAreas);
            
            // make sure standard output files like ".lst" are added to the WQ model
            var textDocuments = model.DataItems.Select(di => di.Value).OfType<TextDocumentFromFile>();

            Assert.AreEqual("Test mon file", textDocuments.First(d => d.Name == "Monitoring file").Content);
            Assert.AreEqual("Test prn file", textDocuments.First(d => d.Name == "Balance output").Content);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAWaqFileBasedPreProcessorAndSettingsWithTheGridFilePath_WhenCallWriteIncludeFilesAndBinaryFiles_ThenGridIncludeFileIsWritten()
        {
            // Given
            WaqInitializationSettings settings = CreateBasicWaqInitializationSettings();

            using (var temp = new TemporaryDirectory())
            {
                string tempPath = temp.Path;
                string expectedGridFilePath = Path.Combine(tempPath, "B3_ugrid.inc");
                settings.GridFile = expectedGridFilePath;

                // When
                new WaqFileBasedPreProcessor().WriteIncludeFilesAndBinaryFiles(settings, tempPath);

                // Then
                Assert.That(File.Exists(expectedGridFilePath));
            }
        }

        private static WaqInitializationSettings CreateBasicWaqInitializationSettings()
        {
            return new WaqInitializationSettings
            {
                SubstanceProcessLibrary = new SubstanceProcessLibrary(),
                OutputLocations = new Dictionary<string, IList<int>>(),
                Dispersion = new List<IFunction> {new Function()},
                BoundaryNodeIds = new Dictionary<WaterQualityBoundary, int[]>(),
                BoundaryAliases = new Dictionary<string, IList<string>>(),
                BoundaryDataManager = new DataTableManager(),
                LoadAndIds = new ConcurrentDictionary<WaterQualityLoad, int>(),
                LoadsAliases = new Dictionary<string, IList<string>>(),
                LoadsDataManager = new DataTableManager(),
                ProcessCoefficients = new List<IFunction>()
            };
        }
    }
}