using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr.Gui;
using DeltaShell.Gui;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
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
            using (var gui = new DeltaShellGui())
            {
                //load the plugins
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

                IApplication app = gui.Application;
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new WaveApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new ToolboxApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
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
            string testName = $"{testModel.Parent.Name}.{testModel.Name}";

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

        #endregion

        #region CommonOperations

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
            AcceptanceModelExportHelper.ExportLogFile(exportConfig);

            if (activity.Status != ActivityStatus.Cleaned)
            {
                throw new AssertionException(
                    string.Format("Unable to complete Model run{0}Expected status: Cleaned{0}Actual status: {1}",
                                  Environment.NewLine, activity.Status.ToString()));
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
    }
}