using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr.Gui;
using DeltaShell.Gui;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.Scripting.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.Toolbox;
using DeltaShell.Plugins.Toolbox.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [Category(NghsTestCategory.AcceptanceTests)]
    [Category(TestCategory.Slow)]
    [Category(TestCategory.Wpf)]
    [TestFixture]
    public class AcceptanceModelsTest
    {
        #region AcceptanceModelTests

        [Test]
        [TestCaseSource(nameof(GetAcceptanceModels))]
        public void Delft3DFM_AcceptanceModelTest(string relativeZipFilePath, string relativeMduFilePath)
        {
            // Step 1: Unzip 
            string testDataFolder = new DirectoryInfo(TestHelper.GetTestWorkingDirectory()).Parent?.Parent?.Parent?.FullName;
            string testCaseZipFilePath = Path.Combine(testDataFolder, "AcceptanceModels", "Delft3DFM", relativeZipFilePath);

            Assert.IsTrue(File.Exists(testCaseZipFilePath), "Failed to find acceptance model test-data");

            var testDirectory = string.Empty;

            Assert.True(TryPerformAction(() => UnzipModel(relativeZipFilePath, testCaseZipFilePath, out testDirectory)),
                        string.Format("Failed to unzip file: {0}", testCaseZipFilePath));

            // Step 2: using(running GUI) add correct plugins for Delft3DFM
            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                LoadAllPlugins(gui);
                gui.Run();

                Action mainWindowShown = delegate
                {
                    IApplication app = gui.Application;
                    app.CreateNewProject();

                    exportConfig.WorkingDirectory = app.WorkDirectory;
                    exportConfig.OutputName = TestContext.CurrentContext.Test.Name;

                    // Step 3: Find root project path in zip folder
                    string projectRootPath = GetProjectRootInUnzippedFolder(testDirectory, relativeMduFilePath);
                    Assert.That(projectRootPath, Is.Not.Null);

                    // Step 4: Import MDU
                    string mduPath = Path.Combine(projectRootPath, relativeMduFilePath);
                    Assert.True(TryPerformAction(() => ImportFlowFMModelAndAddToProject(app, mduPath)),
                                string.Format("Failed to import model: {0}", mduPath));

                    // Step 5: Save Project As
                    string projectPath = Path.Combine(testDirectory, @"TestProjectFolder\TestProject.dsproj");
                    Assert.True(TryPerformAction(() => app.SaveProjectAs(projectPath)),
                                string.Format("Failed to save project before running the model: {0}", projectPath));

                    // Step 6: Close Project
                    Assert.True(TryPerformAction(() => app.CloseProject()),
                                string.Format("Failed to close project after import: {0}", projectPath));

                    // Step 7: Re-Open Project
                    Assert.True(TryPerformAction(() => app.OpenProject(projectPath)),
                                string.Format("Failed to reopen project: {0}", projectPath));

                    // Step 8: Validation of the model
                    ITimeDependentModel rootModel = null;
                    Assert.True(TryPerformAction(() => GetRootModelAndValidate(app, out rootModel)),
                                string.Format("Failed to validate model: {0}", rootModel.Name));

                    exportConfig.CurrentModelName = rootModel.Name;

                    // Step 9: Dimr Export of FM model
                    Assert.True(TryPerformAction(() => ExportDimrConfiguration(testDirectory, rootModel)),
                                string.Format("Failed to export dimr configuration for model: {0}", rootModel.Name));

                    // Step 10: Adjust Time Settings (10 time steps)
                    Assert.True(TryPerformAction(() => AdjustTimeSettings(rootModel)),
                                string.Format("Failed to adjust time settings for model: {0}", rootModel.Name));

                    // Step 11: Run model
                    Assert.True(TryPerformAction(() => RunModel(rootModel)),
                                string.Format("Failed to run model: {0}", rootModel.Name));

                    // Step 12: Save Project
                    Assert.True(TryPerformAction(() => app.SaveProject()),
                                string.Format("Failed to save project after running the model: {0}", projectPath));

                    // Step 13: Close Project
                    Assert.True(TryPerformAction(() => app.CloseProject()),
                                string.Format("Failed to close project after running the model: {0}", projectPath));
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetDimrXmlAcceptanceModels))]
        public void Delft3DFM_AcceptanceModelTestUsingDimrConfigs(string absoluteOriginalDimrXmlPath, DimrXmlConfig dimrXmlConfig)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                Assert.IsTrue(File.Exists(absoluteOriginalDimrXmlPath), "Failed to find acceptance model test-data");
                
                // Step 1: Copy model folder to temporary directory
                var fileInfo = new FileInfo(absoluteOriginalDimrXmlPath);
                tempDirectory.CopyDirectoryToTempDirectory(fileInfo.DirectoryName);
                
                // Step 2: using(running GUI) add correct plugins for Delft3DFM
                using (var gui = DeltaShellCoreFactory.CreateGui())
                {
                    LoadAllPlugins(gui);
                    gui.Run();

                    Action mainWindowShown = delegate
                    {
                        IApplication app = gui.Application;
                        app.CreateNewProject();

                        exportConfig.WorkingDirectory = app.WorkDirectory;
                        exportConfig.OutputName = TestContext.CurrentContext.Test.Name;

                        // Step 3: Import Dimr xml
                        Assert.True(TryPerformAction(() => ImportDimrXmlAndAddToProject(app, absoluteOriginalDimrXmlPath)),
                                    $"Failed to import the original model: {absoluteOriginalDimrXmlPath}");
                        
                        // Step 4: Validation of the model
                        HydroModel rootModelOriginalDimrXml = null;
                        Assert.True(TryPerformAction(() => GetHydroModelAndValidate(app, dimrXmlConfig, out rootModelOriginalDimrXml)),
                                    $"Failed to validate the original model: {rootModelOriginalDimrXml.Name}");
                        
                        // Step 5: Adjust Time Settings (10 time steps)
                        Assert.True(TryPerformAction(() => AdjustTimeSettings(rootModelOriginalDimrXml)),
                                    $"Failed to adjust time settings for the original model: {rootModelOriginalDimrXml.Name}");

                        // Step 6: Run model
                        exportConfig.CurrentModelName = rootModelOriginalDimrXml.Name;
                        Assert.True(TryPerformAction(() => RunIntegratedModel(rootModelOriginalDimrXml, 6)),
                                    $"Failed to run the original model: {rootModelOriginalDimrXml.Name}");
                        
                        CheckIfOutputHasBeenCreated(dimrXmlConfig, app, rootModelOriginalDimrXml.Name);

                        // Step 7: Save Project As
                        string saveFolderPathWithOutput = Path.Combine(tempDirectory.Path, @"SaveTestProjectFolder");
                        string projectPathWithOutput = Path.Combine(saveFolderPathWithOutput, "TestProject.dsproj");
                        
                        Assert.True(TryPerformAction(() => app.SaveProjectAs(projectPathWithOutput)),
                                    $"Failed to save as original project after running the model: {projectPathWithOutput}");

                        CheckPersistentFolderStructure(saveFolderPathWithOutput, projectPathWithOutput, rootModelOriginalDimrXml, dimrXmlConfig, 7, out Dictionary<string, DateTime> timeStampsAfterFirstSave);
                        
                        // Step 8: Run model Again
                        Assert.True(TryPerformAction(() => RunIntegratedModel(rootModelOriginalDimrXml, 8)),
                                    $"Failed to run model: {rootModelOriginalDimrXml.Name}");
                        
                        // Step 9: Save Project 
                        Assert.True(TryPerformAction(() => app.SaveProject()),
                                    $"Failed to save the new output files of the second run: {projectPathWithOutput}");

                        CheckPersistentFolderStructure(saveFolderPathWithOutput, projectPathWithOutput, rootModelOriginalDimrXml, dimrXmlConfig, 9, out Dictionary<string, DateTime> timeStampsAfterSecondSave);
                        CheckUpToDateFiles(timeStampsAfterFirstSave, timeStampsAfterSecondSave, 9);

                        // Step 10: Dimr Export of FM model
                        Assert.True(TryPerformAction(() => ExportDimrConfiguration(tempDirectory.Path, rootModelOriginalDimrXml)),
                                    $"Failed to export dimr configuration for model: {rootModelOriginalDimrXml.Name}");

                        // Step 11: Close Project
                        Assert.True(TryPerformAction(() => app.CloseProject()),
                                    $"Failed to close project after dimr import: {projectPathWithOutput}");

                        app.CreateNewProject();

                        // Step 12: Import Dimr xml
                        string exportedDimrXmlPath = Path.Combine(tempDirectory.Path, "Dimr_Export", "dimr.xml");
                        Assert.True(TryPerformAction(() => ImportDimrXmlAndAddToProject(app, exportedDimrXmlPath)),
                                    $"Failed to import exported dimr xml file: {exportedDimrXmlPath}");

                        // Step 13: Validation of the model
                        HydroModel rootModelExportedDimrXml = null;
                        Assert.True(TryPerformAction(() => GetHydroModelAndValidate(app, dimrXmlConfig, out rootModelExportedDimrXml)),
                                    $"Failed to validate the imported model after creating a new dimr xml: {rootModelExportedDimrXml.Name}");

                        // Step 14: Close Project
                        Assert.True(TryPerformAction(() => app.CloseProject()),
                                    $"Failed to close project after second import: {projectPathWithOutput}");

                        // Step 15: Re-Open Project
                        Assert.True(TryPerformAction(() => app.OpenProject(projectPathWithOutput)),
                                    $"Failed to reopen project: {projectPathWithOutput}");

                        // Step 16: Validation of the model
                        HydroModel rootModelOpenedProject = null;
                        Assert.True(TryPerformAction(() => GetHydroModelAndValidate(app, dimrXmlConfig, out rootModelOpenedProject)),
                                    $"Failed to validate model after opening the created project: {rootModelOpenedProject.Name}");
                        
                        // Step 17: Adjust Time Settings (10 time steps)
                        Assert.True(TryPerformAction(() => AdjustTimeSettings(rootModelOpenedProject)),
                                    $"Failed to adjust time settings for model after opening the created project: {rootModelOpenedProject.Name}");

                        // Step 18: Run model
                        Assert.True(TryPerformAction(() => RunIntegratedModel(rootModelOpenedProject, 18)),
                                    $"Failed to run model after opening the created project: {rootModelOpenedProject.Name}");

                        //// Step 19: Save Project
                        Assert.True(TryPerformAction(() => app.SaveProject()),
                                    $"Failed to save project after opening and running the model: {projectPathWithOutput}");

                        CheckPersistentFolderStructure(saveFolderPathWithOutput, projectPathWithOutput, rootModelOpenedProject, dimrXmlConfig, 19, out Dictionary<string, DateTime> timeStampsOutputAfterThirdSave);
                        CheckUpToDateFiles(timeStampsAfterSecondSave, timeStampsOutputAfterThirdSave, 19);

                        // Step 20: Close Project
                        Assert.True(TryPerformAction(() => app.CloseProject()),
                                    $"Failed to close project after opening and running the model: {projectPathWithOutput}");
                    };

                    WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
                }
            }
        }

        private static void LoadAllPlugins(IGui gui)
        {
            gui.Plugins.Add(new DimrGuiPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new FlowFMGuiPlugin());
            gui.Plugins.Add(new WaveGuiPlugin());
            gui.Plugins.Add(new HydroModelGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new RealTimeControlGuiPlugin());
            gui.Plugins.Add(new ScriptingGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new ToolboxGuiPlugin());
            gui.Plugins.Add(new WaterQualityModelGuiPlugin());

            IApplication application = gui.Application;
            application.Plugins.Add(new CommonToolsApplicationPlugin());
            application.Plugins.Add(new NHibernateDaoApplicationPlugin());
            application.Plugins.Add(new FlowFMApplicationPlugin());
            application.Plugins.Add(new WaveApplicationPlugin());
            application.Plugins.Add(new HydroModelApplicationPlugin());
            application.Plugins.Add(new NetCdfApplicationPlugin());
            application.Plugins.Add(new NetworkEditorApplicationPlugin());
            application.Plugins.Add(new RealTimeControlApplicationPlugin());
            application.Plugins.Add(new ScriptingApplicationPlugin());
            application.Plugins.Add(new SharpMapGisApplicationPlugin());
            application.Plugins.Add(new ToolboxApplicationPlugin());
            application.Plugins.Add(new WaterQualityModelApplicationPlugin());
        }

        private static void CheckIfOutputHasBeenCreated(DimrXmlConfig dimrXmlConfig, IApplication app, string rootModelOriginalDimrXmlName)
        {
            string integratedModelWorkingDirectoryPath = Path.Combine(app.WorkDirectory, rootModelOriginalDimrXmlName);
            var integratedModelWorkingDirectory = new DirectoryInfo(integratedModelWorkingDirectoryPath);
            Assert.IsTrue(integratedModelWorkingDirectory.GetFiles().Any(f => f.Name == "dimr.xml"));
            DirectoryInfo[] subModelsWorkingDirectories = integratedModelWorkingDirectory.GetDirectories();

            if (dimrXmlConfig.FMModelIncluded)
            {
                DirectoryInfo fmWorkingDirectory = subModelsWorkingDirectories.FirstOrDefault(d => d.Name == "dflowfm");
                Assert.IsNotNull(fmWorkingDirectory);
                Assert.IsTrue(fmWorkingDirectory.GetDirectories().Any(d => d.Name == "output"), "Output of FM has not been created in the working directory");
            }

            if (dimrXmlConfig.RtcModelIncluded)
            {
                DirectoryInfo rtcWorkingDirectory = subModelsWorkingDirectories.FirstOrDefault(d => d.Name == "rtc");
                Assert.IsNotNull(rtcWorkingDirectory);
                Assert.IsTrue(rtcWorkingDirectory.GetDirectories().Any(d => d.Name == "output"), "Output of RTC has not been created in the working directory");
            }

            if (dimrXmlConfig.WavesModelIncluded)
            {
                DirectoryInfo wavesWorkingDirectory = subModelsWorkingDirectories.FirstOrDefault(d => d.Name == "wave");
                Assert.IsNotNull(wavesWorkingDirectory);
                Assert.IsTrue(wavesWorkingDirectory.GetDirectories().Any(d => d.Name == "output"), "Output of Waves has not been created in the working directory");
            }
        }

        private static void CheckUpToDateFiles(IDictionary<string, DateTime> timeStampsFirstSet, IDictionary<string, DateTime> timeStampsSecondSet, int testStepNr)
        {
            foreach (KeyValuePair<string, DateTime> kvp in timeStampsFirstSet)
            {
                string key = kvp.Key;
                Assert.AreNotEqual(timeStampsSecondSet[key], kvp.Value, $"After saving in step {testStepNr} file {key} was not updated");
            }
        }

        private static void CheckPersistentFolderStructure(string saveFolderPath, string projectPath, 
                                                           ITimeDependentModel rootModel, DimrXmlConfig dimrXmlConfig, int testStepNr, 
                                                           out Dictionary<string, DateTime> timeStampsFiles)
        {
            string dataFolder = Path.Combine(saveFolderPath, "TestProject.dsproj_data");
            timeStampsFiles = new Dictionary<string, DateTime>();

            CheckMainPersistentFolders(saveFolderPath, projectPath, dataFolder, out DateTime timeStampProject);
            timeStampsFiles.Add(Path.GetFileName(projectPath), timeStampProject);

            var integratedModel = (ICompositeActivity) rootModel;

            CheckIntegratedModelFileStructure(dataFolder, integratedModel.Name, testStepNr);
            
            if (dimrXmlConfig.FMModelIncluded)
            {
                WaterFlowFMModel[] fmModels = integratedModel.Activities.OfType<WaterFlowFMModel>().ToArray();
                CheckFileStructureFM(dataFolder, fmModels[0].Name, testStepNr, timeStampsFiles);
            }

            if (dimrXmlConfig.RtcModelIncluded)
            {
                RealTimeControlModel[] rtcModels = integratedModel.Activities.OfType<RealTimeControlModel>().ToArray();
                CheckFileStructureRtc(dataFolder, rtcModels[0].Name, testStepNr, timeStampsFiles);
            }

            if (dimrXmlConfig.WavesModelIncluded)
            {
                WaveModel[] wavesModels = integratedModel.Activities.OfType<WaveModel>().ToArray();
                CheckFileStructureWaves(dataFolder, wavesModels[0].Name, testStepNr, timeStampsFiles);
            }
        }

        #endregion

        #region TestFixture

        private static string TestFixtureDirectory = string.Empty;

        private AcceptanceModelExportResultConfig exportConfig;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestFixtureDirectory = FileUtils.CreateTempDirectory();

            // Ensure we do not accidentally incorporate previous results
            FileUtils.DeleteIfExists(AcceptanceModelExportResultConfig.ReportFolder);
            Directory.CreateDirectory(AcceptanceModelExportResultConfig.ReportFolder);
            Directory.CreateDirectory(AcceptanceModelExportResultConfig.Delft3DfmExportDirectory);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            FileUtils.DeleteIfExists(TestFixtureDirectory);
        }

        [SetUp]
        public void SetUp()
        {
            // Clean information from previous run
            exportConfig = new AcceptanceModelExportResultConfig();
        }

        [TearDown]
        public void TearDown()
        {
            // We want to add a "No .dia file found for this run."-dia file when none has been produced this test run.
            if (exportConfig.HasExportedDiagnostics || string.IsNullOrEmpty(exportConfig.OutputName))
            {
                return;
            }

            AcceptanceModelExportHelper.ExportEmptyLogFile(exportConfig);
        }

        #endregion

        #region TestCaseDefinitions

        private static DirectoryInfo AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return new DirectoryInfo(Path.GetDirectoryName(path));
            }
        }

        private static IEnumerable<TestCaseData> GetDimrXmlAcceptanceModels()
        {
            string repoSourceFolder = AssemblyDirectory.Parent?.Parent?.FullName;
            string acceptanceModelDirectoryPath = Path.Combine(repoSourceFolder, "AcceptanceModelsRTCFMDimrXmlBased");
            var acceptanceModelDirectory = new DirectoryInfo(acceptanceModelDirectoryPath);

            if (!acceptanceModelDirectory.Exists)
            {
                return Enumerable.Empty<TestCaseData>();
            }

            IEnumerable<DirectoryInfo> modelDirectories = acceptanceModelDirectory.EnumerateDirectories().Where(x=>x.Name.StartsWith("c"));
            var testCases = new List<TestCaseData>();
            foreach (var modelDirectory in modelDirectories)
            {
                FileInfo[] files = modelDirectory.GetFiles();
                DirectoryInfo[] subDirectories = modelDirectory.GetDirectories();

                bool fmModelIncluded = subDirectories.Any(d => d.Name == "dflowfm");
                bool rtcModelIncluded = subDirectories.Any(d => d.Name == "rtc");
                bool wavesModelIncluded = subDirectories.Any(d => d.Name == "wave");

                var dimrXmlConfig = new DimrXmlConfig(fmModelIncluded, rtcModelIncluded, wavesModelIncluded);

                FileInfo dimrXml = files.FirstOrDefault(f => f.Name == "dimr.xml");

                if (dimrXml != null)
                {
                    var testCase = new TestCaseData(dimrXml.FullName, dimrXmlConfig);
                    testCase.SetName($"e105_dflowfm-drtc.f02_tutorials.{modelDirectory.Name}");
                    testCases.Add(testCase);
                }
            }
            return testCases;
        }
        private static IEnumerable<TestCaseData> GetAcceptanceModels()
        {
            string repoSourceFolder = AssemblyDirectory.Parent?.Parent?.FullName;
            string acceptanceModelDirectoryPath = Path.Combine(repoSourceFolder, "AcceptanceModels", "Delft3DFM");
            var acceptanceModelDirectory = new DirectoryInfo(acceptanceModelDirectoryPath);

            if (!acceptanceModelDirectory.Exists)
            {
                return Enumerable.Empty<TestCaseData>();
            }

            // The following directory structure is expected: <root>/AcceptanceModels/Delft3DFM/<TestSuiteFolders>/<ModelFolders>/**/*.zip
            IEnumerable<DirectoryInfo> testSuiteDirectories = acceptanceModelDirectory.EnumerateDirectories();
            IEnumerable<DirectoryInfo> modelDirectories = testSuiteDirectories.SelectMany(x => x.EnumerateDirectories());
            return modelDirectories.SelectMany(GetZipFilesInModelDirectory)
                                   .SelectMany(GetTestCaseDataInZip);
        }

        private static IEnumerable<Tuple<FileInfo, bool, DirectoryInfo>> GetZipFilesInModelDirectory(DirectoryInfo modelDirectoryInfo)
        {
            FileInfo[] candidateZipFiles = modelDirectoryInfo.EnumerateFiles("*.zip",
                                                                             SearchOption.AllDirectories)
                                                             .ToArray();
            bool hasMultipleZipFiles = candidateZipFiles.Length > 1;

            return candidateZipFiles.Select(fi => new Tuple<FileInfo, bool, DirectoryInfo>(fi, hasMultipleZipFiles, modelDirectoryInfo));
        }

        private static IEnumerable<TestCaseData> GetTestCaseDataInZip(Tuple<FileInfo, bool, DirectoryInfo> input)
        {
            return GetTestCaseDataInZip(input.Item1, input.Item3, input.Item2);
        }

        private static IEnumerable<TestCaseData> GetTestCaseDataInZip(FileInfo candidateZipFile,
                                                                      DirectoryInfo modelDirectory,
                                                                      bool hasMultipleZipFiles)
        {
            IList<string> filesInZip = ZipFileUtils.GetFilePathsInZip(candidateZipFile.FullName);

            string[] relevantMduFilesInZip =
                filesInZip.Where(p => p.EndsWith(".mdu") && !IsIgnored(p))
                          .ToArray();

            bool hasMultipleMduFiles = relevantMduFilesInZip.Length > 1;

            foreach (string candidateMduFile in relevantMduFilesInZip)
            {
                string testName = GetTestName(modelDirectory,
                                              hasMultipleZipFiles,
                                              hasMultipleMduFiles,
                                              candidateZipFile.Name,
                                              candidateMduFile);

                var testCase = new TestCaseData(candidateZipFile.FullName, candidateMduFile);
                testCase.SetName(testName);

                yield return testCase;
            }
        }

        private static bool IsIgnored(string path)
        {
            string lowerCase = path.ToLowerInvariant();
            return lowerCase.Contains("dimr_expected") || lowerCase.Contains("_output");
        }

        private static string GetTestName(DirectoryInfo testModel,
                                          bool hasMultipleZipFiles,
                                          bool hasMultipleMduFiles,
                                          string candidateZipFileName,
                                          string candidateMduFileName)
        {
            var testName = $"{testModel.Parent.Name}.{testModel.Name}";

            if (hasMultipleZipFiles && hasMultipleMduFiles)
            {
                string zipName = Path.GetFileNameWithoutExtension(candidateZipFileName);
                string mduName = Path.GetFileNameWithoutExtension(candidateMduFileName);
                testName += $" ({zipName} - {mduName})";
            }
            else if (hasMultipleZipFiles)
            {
                string zipName = Path.GetFileNameWithoutExtension(candidateZipFileName);
                testName += $" ({zipName})";
            }
            else if (hasMultipleMduFiles)
            {
                string mduName = Path.GetFileNameWithoutExtension(candidateMduFileName);
                testName += $" ({mduName})";
            }

            return testName;
        }

        #endregion

        #region HelperFunctions

        private static void CheckMainPersistentFolders(string saveFolderPath, string projectPath, string dataFolder, out DateTime timeStampProject)
        {
            Assert.True(Directory.Exists(saveFolderPath));
            Assert.True(File.Exists(projectPath));
            timeStampProject = File.GetLastWriteTime(projectPath);
            Assert.True(Directory.Exists(dataFolder));
        }

        private static void CheckIntegratedModelFileStructure(string dataFolder, string integratedModelName, int testStepNr)
        {
            string dataFolderIntegratedModel = Path.Combine(dataFolder, integratedModelName);
            Assert.True(Directory.Exists(dataFolderIntegratedModel), $"After step {testStepNr} the model directory of the integrated model can not be found in the persistent folder");
            string jsonFile = Path.Combine(dataFolderIntegratedModel, integratedModelName + ".json");
            Assert.True(File.Exists(jsonFile), $"After step {testStepNr} the coupling json file of the integrated model can not be found in the persistent folder");
        }

        private static void CheckFileStructureFM(string dataFolder, string fmModelName, int testStepNr, IDictionary<string, DateTime> timeStamps)
        {
            string dataFolderFmModel = Path.Combine(dataFolder, fmModelName);
            Assert.True(Directory.Exists(dataFolderFmModel), $"After step {testStepNr} the model directory of FM can not be found in the persistent folder.");
            string inputFmModel = Path.Combine(dataFolderFmModel, DirectoryNameConstants.InputDirectoryName);
            string outputFmModel = Path.Combine(dataFolderFmModel, DirectoryNameConstants.OutputDirectoryName);

            Assert.True(Directory.Exists(inputFmModel), $"After step {testStepNr} the input directory of FM can not be found in the persistent folder.");
            Assert.True(Directory.Exists(outputFmModel), $"After step {testStepNr} the output directory of FM can not be found in the persistent folder.");

            string mduPath = Path.Combine(inputFmModel, fmModelName + ".mdu");
            Assert.True(File.Exists(mduPath), $"After step {testStepNr} the mdu file of FM can not be found in the persistent folder.");

            string diaPath = Path.Combine(outputFmModel, fmModelName + ".dia");
            Assert.True(File.Exists(diaPath), $"After step {testStepNr} the diagnostic file of FM can not be found in the persistent folder.");
            timeStamps.Add(Path.GetFileName(diaPath), File.GetLastWriteTime(diaPath));
        }

        private static void CheckFileStructureRtc(string dataFolder, string rtcModelName, int testStepNr, IDictionary<string, DateTime> timeStamps)
        {
            string dataFolderRtcModel = Path.Combine(dataFolder, rtcModelName);
            Assert.True(Directory.Exists(dataFolderRtcModel), $"After step {testStepNr} the model directory of RTC can not be found in the persistent folder.");

            string outputRtcModel = Path.Combine(dataFolderRtcModel, DirectoryNameConstants.OutputDirectoryName);
            Assert.True(Directory.Exists(outputRtcModel), $"After step {testStepNr} the output directory of RTC can not be found in the persistent folder.");

            string diaPath = Path.Combine(outputRtcModel, "diag.xml");
            Assert.True(File.Exists(diaPath), $"After step {testStepNr} the diagnostic file of RTC can not be found in the persistent folder.");
            timeStamps.Add(Path.GetFileName(diaPath), File.GetLastWriteTime(diaPath));
        }

        private static void CheckFileStructureWaves(string dataFolder, string wavesModelName, int testStepNr, IDictionary<string, DateTime> timeStamps)
        {
            string dataFolderWavesModel = Path.Combine(dataFolder, wavesModelName);
            Assert.True(Directory.Exists(dataFolderWavesModel), $"After step {testStepNr} the model directory of Waves can not be found in the persistent folder.");

            string inputWavesModel = Path.Combine(dataFolderWavesModel, DirectoryNameConstants.InputDirectoryName);
            Assert.True(Directory.Exists(inputWavesModel), $"After step {testStepNr} the input directory of Waves can not be found in the persistent folder.");

            var outputWavesModelDirectoryInfo = new DirectoryInfo(Path.Combine(dataFolderWavesModel, DirectoryNameConstants.OutputDirectoryName));
            Assert.True(outputWavesModelDirectoryInfo.Exists, $"After step {testStepNr} the output directory of Waves can not be found in the persistent folder.");

            FileInfo diaFile = outputWavesModelDirectoryInfo.GetFiles().First(f => f.Name.StartsWith("swn-diag."));
            Assert.True(diaFile.Exists, $"After step {testStepNr} the  diagnostic file of Waves can not be found in the persistent folder.");
            timeStamps.Add(diaFile.Name, diaFile.LastWriteTime);
        }

        /// <summary>
        /// Wraps Action in a Try-Catch, returns false if Action results in an exception
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Success or Failure of Action</returns>
        private static bool TryPerformAction(Action action)
        {
            try
            {
                action.Invoke();
                return true;
            }
            catch (Exception debug)
            {
                if (!string.IsNullOrEmpty(debug.Message))
                {
                    Console.WriteLine(debug.Message);
                }

                return false;
            }
        }

        /// <summary>
        /// Get the absolute path to the folder in <paramref name="unzipFolder"/> that directly contains
        /// <paramref name="projectPath"/>. If no such path exists, null is returned.
        /// </summary>
        /// <param name="unzipFolder"> The basefolder in which to look for the projectPath. </param>
        /// <param name="projectPath"> The file path which is matched within the <paramref name="unzipFolder"/></param>
        /// <returns> The path to the parent directory of <paramref name="projectPath"/> if it exists, null otherwise. </returns>
        private static string GetProjectRootInUnzippedFolder(string unzipFolder, string projectPath)
        {
            string rootFolder = unzipFolder;
            IList<string> subFolders = new List<string>();
            while (true)
            {
                if (File.Exists(Path.Combine(rootFolder, projectPath)))
                {
                    return rootFolder;
                }

                subFolders.AddRange(Directory.GetDirectories(rootFolder));

                if (subFolders.Count > 0)
                {
                    rootFolder = subFolders.First();
                    subFolders.RemoveAt(0);
                }
                else
                {
                    return null;
                }
            }
        }
        
        private static void UnzipModel(string relativeZipFilePath, string testCaseZipFilePath, out string testDirectory)
        {
            string zipFileName = Path.GetFileName(relativeZipFilePath);
            if (string.IsNullOrEmpty(zipFileName))
            {
                throw new ArgumentException(string.Format("Unable to retrieve Zip File Name: {0}", relativeZipFilePath));
            }

            string localZipFilePath = Path.Combine(TestFixtureDirectory, zipFileName);

            FileUtils.DeleteIfExists(localZipFilePath);
            FileUtils.CopyFile(testCaseZipFilePath, localZipFilePath);

            string randomFileName = Path.GetRandomFileName().Substring(0, 5);
            testDirectory = Path.Combine(TestFixtureDirectory, randomFileName);
            FileUtils.DeleteIfExists(testDirectory);
            ZipFileUtils.Extract(localZipFilePath, testDirectory);
        }

        private static void ImportFlowFMModelAndAddToProject(IApplication app, string mduPath)
        {
            var fmModel = new WaterFlowFMModel();
            fmModel.ImportFromMdu(mduPath);

            app.Project.RootFolder.Items.Add(fmModel);
        }

        private static void ImportDimrXmlAndAddToProject(IApplication app, string dimrXmlPath)
        {
            var integratedModel = new HydroModel();
            app.Project.RootFolder.Items.Add(integratedModel);
            DHydroConfigXmlImporter importer = app.FileImporters.OfType<DHydroConfigXmlImporter>().First();
            importer.ImportItem(dimrXmlPath, integratedModel);
        }

        private static void GetHydroModelAndValidate(IApplication app, DimrXmlConfig dimrXmlConfig, out HydroModel hydroModel)
        {
            hydroModel = GetRootModel<HydroModel>(app);

            if (dimrXmlConfig.FMModelIncluded)
            {
                Assert.AreEqual(hydroModel.Activities.OfType<WaterFlowFMModel>().Count(), 1, "The integrated model is missing the FM model after the dimr import.");
            }

            if (dimrXmlConfig.RtcModelIncluded)
            {
                Assert.AreEqual(hydroModel.Activities.OfType<RealTimeControlModel>().Count(), 1, "The integrated model is missing the RTC model after the dimr import.");
            }

            if (dimrXmlConfig.WavesModelIncluded)
            {
                Assert.AreEqual(hydroModel.Activities.OfType<WaveModel>().Count(), 1, "The integrated model is missing the Waves model after the dimr import.");
            }

            ValidationReport report = hydroModel.Validate();
            
            if (report == null)
            {
                throw new NotSupportedException($"Unable to Validate Root Model: {hydroModel.Name}, did you forget to add support for this model type?");
            }

            CheckValidationErrors(report);
        }

        private static void GetRootModelAndValidate(IApplication app, out ITimeDependentModel timeDependentModel)
        {
            timeDependentModel = GetRootModel<ITimeDependentModel>(app);

            // Unfortunately there is no IValidateable, so instead we handle this for each individual model type...
            ValidationReport report = null;

            var hydroModel = timeDependentModel as HydroModel;
            if (hydroModel != null)
            {
                report = hydroModel.Validate();
            }

            var fmModel = timeDependentModel as WaterFlowFMModel;
            if (fmModel != null)
            {
                report = fmModel.Validate();
            }

            if (report == null)
            {
                throw new NotImplementedException(string.Format("Unable to Validate Root Model: {0}, did you forget to add support for this model type?", timeDependentModel.Name));
            }

            CheckValidationErrors(report);
        }

        private static void CheckValidationErrors(ValidationReport report)
        {
            if (report.ErrorCount > 0)
            {
                string debugInfo = string.Join(Environment.NewLine, report.AllErrors.Select(er => er.Message));
                throw new Exception(debugInfo);
            }
        }

        private static T GetRootModel<T>(IApplication app)
        {
            T model = app.Project.RootFolder.Models.OfType<T>().FirstOrDefault();
            if (model == null)
            {
                throw new NullReferenceException("Unable to get Model from Project Root Folder");
            }

            return model;
        }

        private static void AdjustTimeSettings(ITimeDependentModel timeDependentModel)
        {
            timeDependentModel.StopTime = timeDependentModel.StartTime.AddHours(timeDependentModel.TimeStep.TotalHours * 10);
        }

        private void RunModel(IActivity activity)
        {
            var hydroModel = activity as HydroModel;
            if (hydroModel != null)
            {
                // We get the workflow that runs all models in parallel
                ICompositeActivity workflow = hydroModel.Workflows.FirstOrDefault(w => w.Activities.Count == hydroModel.Activities.Count);
                if (workflow == null)
                {
                    throw new NullReferenceException("Unable to get Workflow (run all models in parallel)");
                }

                hydroModel.CurrentWorkflow = workflow;
            }

            // Run model
            ActivityRunner.RunActivity(activity);

            // Export the dia file for further manual inspection
            AcceptanceModelExportHelper.ExportLogFileOfFm(exportConfig);

            if (activity.Status != ActivityStatus.Cleaned)
            {
                throw new AssertionException(
                    string.Format("Unable to complete Model run{0}Expected status: Cleaned{0}Actual status: {1}",
                                  Environment.NewLine, activity.Status.ToString()));
            }
        }

        private void RunIntegratedModel(HydroModel hydroModel, int testStepNr)
        {
            // We get the workflow that runs all models in parallel
            ICompositeActivity workflow = hydroModel.Workflows.FirstOrDefault(w => w.Activities.Count == hydroModel.Activities.Count);
            if (workflow == null)
            {
                throw new NullReferenceException($"Unable to get Workflow (run all models in parallel) in step {testStepNr}");
            }

            hydroModel.CurrentWorkflow = workflow;

            // Run model
            ActivityRunner.RunActivity(hydroModel);

            // Export the dia files for further manual inspection
            AcceptanceModelExportHelper.ExportLogFilesOfIntegratedModel(exportConfig);

            if (hydroModel.Status != ActivityStatus.Cleaned)
            {
                throw new AssertionException($"Unable to complete Model run in step {testStepNr}. " +
                                             $"{Environment.NewLine} Expected status: Cleaned {Environment.NewLine} Actual status: {hydroModel.Status}");
            }
        }

        private static void ExportDimrConfiguration(string tempDir, IModel model)
        {
            string dimrExportDir = Path.Combine(tempDir, "Dimr_Export");
            FileUtils.CreateDirectoryIfNotExists(dimrExportDir);

            var exporter = new DHydroConfigXmlExporter {ExportFilePath = Path.Combine(dimrExportDir, "dimr.xml")};
            if (!exporter.Export(model, null))
            {
                throw new Exception();
            }
        }

        #endregion

        public struct DimrXmlConfig
        {
            public DimrXmlConfig(bool fmModelIncluded, bool rtcModelIncluded, bool wavesModelIncluded)
            {
                FMModelIncluded = fmModelIncluded;
                RtcModelIncluded = rtcModelIncluded;
                WavesModelIncluded = wavesModelIncluded;
            }

            public bool FMModelIncluded { get; }
            public bool RtcModelIncluded { get; }

            public bool WavesModelIncluded { get; }
        }
    }
}