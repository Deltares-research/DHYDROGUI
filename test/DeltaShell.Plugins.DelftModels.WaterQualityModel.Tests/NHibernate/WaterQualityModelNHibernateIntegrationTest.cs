using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.NHibernate
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class WaterQualityModelNHibernateIntegrationTest
    {
        
        
        /// <summary>
        /// Test if a WAQ Model can be saved in an WAQ only environment.
        /// Then read it in an environment that contains extra plugins with backwards compatibility mappings.
        /// This breaks currently, because the mapping is upgraded while it shouldn't.
        /// </summary>
        [Test]
        public void ReadWaterQualityModelWithDifferentPluginConfiguration()
        {
            var dsprojName = "WAQ_Only.dsproj";
            // the temporary project is required in order to set the path on the model. Else, it saves null in the Path property of the waq model.
            using (var app = CreateApplication())
            {
                app.Run();

                app.CreateNewProject();
                
                var model = new WaterQualityModel();
                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            using (var app = CreateApplicationWithWAQ())
            {
                app.Run();

                app.OpenProject(dsprojName);
            }
        }

        [Test]
        public void ReadWaterQualityModelWithDifferentPluginConfigurationGui()
        {
            string dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(WaterQualityModelNHibernateIntegrationTest)).Location);
            string dsprojName = Path.Combine(dir, "WAQ_Only.dsproj");
            var pluginsToAdd = new List<IPlugin>
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new WaterQualityModelApplicationPlugin(),
                new CommonToolsGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new WaterQualityModelGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                IApplication app = gui.Application;

                gui.Run();

                app.CreateNewProject();
                
                var model = new WaterQualityModel();
                gui.Application.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            var pluginsToAdd2 = new List<IPlugin>
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new WaterQualityModelApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new CommonToolsGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new WaterQualityModelGuiPlugin(),
                new NetworkEditorGuiPlugin(),

            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd2).Build())
            {
                IApplication app = gui.Application;
                
                gui.Run();

                app.OpenProject(dsprojName);
            }
        }

        [Test]
        public void GivenValidWaqModel_WhenRunningWithInvalidData_SavingProject_OpeningProject_CorrectData_ThenRunningModelIsSuccessFull()
        {
            string testDir = FileUtils.CreateTempDirectory();
            string originalDir = TestHelper.GetTestFilePath("WaterQualityDataFiles");
            FileUtils.CopyAll(new DirectoryInfo(originalDir), new DirectoryInfo(testDir), string.Empty);

            string modelFilePath = Path.Combine(testDir, "myWaqModel.dsproj");
            string hydFilePath = Path.Combine(testDir, "flow-model", "westernscheldt01.hyd");
            string subFilePath = Path.Combine(testDir, "waq", "sub-files", "bacteria.sub");
            string boundaryConditionsFilePath = Path.Combine(testDir, "waq", "boundary-conditions", "bacteria.csv");

            Func<IDataItem, bool> isWaqOutputFileDataItem = di => di.Role == DataItemRole.Output &&
                                                                  di.ValueType == typeof(TextDocument) &&
                                                                  di.Tag != WaterQualityModel.ListFileDataItemMetaData.Tag;

            string workingDirectoryPath = Path.Combine(Path.GetTempPath(), "DeltaShell_Working_Directory");
            ApplicationSettingsBase userSettings =
                ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);
            try
            {
                var app = CreateApplication();
                app.UserSettings = userSettings;
                using (app)
                {
                    app.Run();

                    app.CreateNewProject();

                    // model setup
                    var waqModel = new WaterQualityModel();
                    app.Project.RootFolder.Add(waqModel);
                    new HydFileImporter().ImportItem(hydFilePath, waqModel);
                    new SubFileImporter().Import(waqModel.SubstanceProcessLibrary, subFilePath);
                    new DataTableImporter().ImportItem(boundaryConditionsFilePath, waqModel.BoundaryDataManager);
                    Assert.IsEmpty(waqModel.DataItems.Where(di => isWaqOutputFileDataItem(di)));

                    // Put incorrect data in the boundary conditions file
                    TextDocumentFromFile dataFile = waqModel.BoundaryDataManager.DataTables.FirstOrDefault()?.DataFile;
                    Assert.IsNotNull(dataFile);
                    dataFile.Content = dataFile.Content.Replace("2014/01/01-00:00:00 0.1", "2014/01/01-00:00:00 wrongValue");

                    // Run the model again (which will fail) and check that the output data items connected to the .lsp & .mor-files
                    // are removed from the model.
                    ActivityRunner.RunActivity(waqModel);
                    Assert.IsEmpty(waqModel.DataItems.Where(di => isWaqOutputFileDataItem(di)));

                    // Save, close and open the project
                    app.SaveProjectAs(modelFilePath);
                    app.CloseProject();
                    app.OpenProject(modelFilePath);

                    // Check that the output data items connected to the .lsp & .mor-files are removed from the model.
                    var openedWaqModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterQualityModel;
                    Assert.IsNotNull(openedWaqModel);
                    Assert.IsEmpty(openedWaqModel.DataItems.Where(di => isWaqOutputFileDataItem(di)));

                    // Put correct data in the boundary conditions file
                    dataFile = openedWaqModel.BoundaryDataManager.DataTables.FirstOrDefault()?.DataFile;
                    Assert.IsNotNull(dataFile);
                    dataFile.Content = dataFile.Content.Replace("2014/01/01-00:00:00 wrongValue", "2014/01/01-00:00:00 0.1");

                    // Run the model again 
                    ActivityRunner.RunActivity(openedWaqModel);
                    Assert.IsTrue(openedWaqModel.Status == ActivityStatus.Cleaned);
                    Assert.That(openedWaqModel.DataItems.Count(di => isWaqOutputFileDataItem(di)), Is.EqualTo(2));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir); // cleanup of created files
            }
        }

        [Test]
        public void GivenAProjectWithWaqPluginFileVersionNumber352_WhenOpening_TheProjectIsCorrectlyMigratedToTheNewFormat()
        {
            // Given
            const string modelDirectoryName = "Water_Quality";
            const string workingDirectoryName = modelDirectoryName + "_output";

            string projectFolderPath = TestHelper.GetTestFilePath(@"BackwardsCompatibility\MigrateFileVersion352.dsproj_data");
            string projectFilePath = TestHelper.GetTestFilePath(@"BackwardsCompatibility\MigrateFileVersion352.dsproj");

            using (var tempDirectory = new TemporaryDirectory())
            {
                projectFolderPath = tempDirectory.CopyDirectoryToTempDirectory(projectFolderPath);
                projectFilePath = tempDirectory.CopyTestDataFileToTempDirectory(projectFilePath);

                string workingDirectoryPath = Path.Combine(projectFolderPath, workingDirectoryName);
                string outputDirectoryPath = Path.Combine(projectFolderPath, modelDirectoryName, "output");

                IList<string> filesWorkingDirBeforeMigration = GetAllFileNames(workingDirectoryPath);
                IList<string> filesOutputDirBeforeMigration = GetAllFileNames(outputDirectoryPath);

                // Precondition
                Assert.That(filesOutputDirBeforeMigration.Any() && filesWorkingDirBeforeMigration.Any(),
                            "Precondition violated.");

                using (var app = CreateRunningApplication(tempDirectory.Path))
                {
                    // When
                    app.OpenProject(projectFilePath);
                    app.SaveProjectAs(projectFilePath);

                    // Then
                    Assert.That(Directory.Exists(workingDirectoryPath),
                                "Working directory should not be removed.");

                    IList<string> filesWorkDirAfterMigration = GetAllFileNames(workingDirectoryPath);
                    IList<string> filesOutputDirAfterMigration = GetAllFileNames(outputDirectoryPath);

                    Assert.That(filesWorkDirAfterMigration.Any(), Is.False,
                                "Working directory should be empty.");
                    Assert.That(filesOutputDirBeforeMigration.Except(filesOutputDirAfterMigration).Any(), Is.False,
                                "All files that were in the working directory before migrating, should be moved to the output directory.");
                    Assert.That(filesWorkingDirBeforeMigration.Except(filesOutputDirAfterMigration).Any(), Is.False,
                                "All files that were in the output directory before migrating, should still be there.");

                    WaterQualityModel model = app.Project.RootFolder.GetAllModelsRecursive().OfType<WaterQualityModel>().Single();
                    Assert.That(model.OutputFolder, Is.Not.Null,
                                "Output folder should be set after migration.");
                    Assert.That(model.OutputFolder.Path, Is.EqualTo(outputDirectoryPath),
                                "Path of output folder should be set correctly after migration.");

                    IDataItem dataItem = null;

                    Assert.DoesNotThrow(() => dataItem = model.DataItems.Single(di => di.Tag == "MonitoringFileTag"));
                    Assert.IsTrue(dataItem.Value is TextDocument,
                                  $"Expected DataItem value type is {typeof(TextDocument)}, but was {dataItem.Value.GetType()}");

                    Assert.DoesNotThrow(() => dataItem = model.DataItems.Single(di => di.Tag == "ListFileTag"));
                    Assert.IsTrue(dataItem.Value is TextDocument,
                                  $"Expected DataItem value type is {typeof(TextDocument)}, but was {dataItem.Value.GetType()}");

                    Assert.DoesNotThrow(() => dataItem = model.DataItems.Single(di => di.Tag == "ProcessFileTag"));
                    Assert.IsTrue(dataItem.Value is TextDocument,
                                  $"Expected DataItem value type is {typeof(TextDocument)}, but was {dataItem.Value.GetType()}");
                }
            }
        }

        [Test]
        public void GivenAProjectWithAModel_WhenDoingVariousSubsequentActions_ThenOutputIsAlwaysPlacedCorrectly()
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            using (var app = CreateRunningApplication(tempDirectory.Path))
            {
                try
                {
                    app.CreateNewProject();
                    
                    WaterQualityModel model = CreateValidWaqModel();
                    model.SetWorkingDirectoryInModelSettings(() => app.WorkDirectory);
                    app.Project.RootFolder.Add(model);

                    // Run
                    IEnumerable<string> outputFilesAfterRun = RunModelAndGetOutputFiles(model);

                    // Save (first time saving from temporary project after running a model)
                    const string save1ProjectFileName = "Save1.dsproj";
                    app.SaveProjectAs(Path.Combine(tempDirectory.Path, save1ProjectFileName));

                    string outputFolderAfterSave1Path = model.OutputFolder.FullPath;
                    IList<string> outputFilesAfterSave1 = GetAllFileNames(outputFolderAfterSave1Path);

                    Assert.That(outputFolderAfterSave1Path.Contains(save1ProjectFileName),
                                "After saving after model run, output folder should be switched to correct location.");
                    Assert.That(!outputFilesAfterRun.Except(outputFilesAfterSave1).Any(),
                                "After saving after model run, all output files that were in the working directory should be in the save directory.");

                    // Run
                    outputFilesAfterRun = RunModelAndGetOutputFiles(model);

                    // Save as (with unsaved output from a model run)
                    const string saveAs1ProjectFileName = "SaveAs1.dsproj";
                    app.SaveProjectAs(Path.Combine(tempDirectory.Path, saveAs1ProjectFileName));

                    string outputFolderAfterSaveAs1Path = model.OutputFolder.FullPath;
                    IList<string> outputFilesAfterSaveAs1 = GetAllFileNames(outputFolderAfterSaveAs1Path);

                    Assert.That(outputFolderAfterSaveAs1Path.Contains(saveAs1ProjectFileName),
                                "After saving as after model run, output folder should be switched to correct location.");
                    Assert.That(!outputFilesAfterRun.Except(outputFilesAfterSaveAs1).Any(),
                                "After saving as after model run, all output files that were in the working directory should be in the save directory.");
                    Assert.That(!outputFilesAfterSave1.Except(GetAllFileNames(outputFolderAfterSave1Path)).Any(),
                                "After saving as after model run, all output files that were saved in the previous project should still be there.");

                    // Save as (with saved output)
                    const string saveAs2ProjectFileName = "SaveAs2.dsproj";
                    app.SaveProjectAs(Path.Combine(tempDirectory.Path, saveAs2ProjectFileName));

                    string outputFolderAfterSaveAs2Path = model.OutputFolder.FullPath;
                    IList<string> outputFilesAfterSaveAs2 = GetAllFileNames(outputFolderAfterSaveAs2Path);

                    Assert.That(outputFolderAfterSaveAs2Path.Contains(saveAs2ProjectFileName),
                                "After saving a saved project to a new location, output folder should be switched to correct location.");
                    Assert.That(!outputFilesAfterSaveAs1.Except(outputFilesAfterSaveAs2).Any(),
                                "After saving a saved project to a new location, all output files that were at the original location should be at the new location.");
                    Assert.That(!outputFilesAfterSaveAs1.Except(GetAllFileNames(outputFolderAfterSaveAs1Path)).Any(),
                                "After saving a saved project to a new location, all output files that were saved in the previous project should still be there.");

                    // Save (with saved output)
                    app.SaveProject();

                    string outputFolderAfterSave2Path = model.OutputFolder.FullPath;
                    IList<string> outputFilesAfterSave2 = GetAllFileNames(outputFolderAfterSave2Path);

                    Assert.That(outputFolderAfterSave2Path, Is.EqualTo(outputFolderAfterSaveAs2Path),
                                "After saving,  output folder path should not be changed.");
                    Assert.That(!outputFilesAfterSaveAs2.Except(outputFilesAfterSave2).Any(),
                                "After saving, output files that were at the location should not be deleted.");

                    // Run
                    outputFilesAfterRun = RunModelAndGetOutputFiles(model);

                    // Save (with unsaved output from a model run)
                    app.SaveProject();

                    string outputFolderAfterSave3Path = model.OutputFolder.FullPath;
                    IList<string> outputFilesAfterSave3 = GetAllFileNames(outputFolderAfterSave3Path);

                    Assert.That(outputFolderAfterSave3Path, Is.EqualTo(outputFolderAfterSave2Path),
                                "After saving,  output folder path should not be changed.");
                    Assert.That(!outputFilesAfterRun.Except(outputFilesAfterSave3).Any(),
                                "After saving, output files that were at the location should not be deleted.");
                }
                finally
                {
                    app.CloseProject();
                }
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void GivenAProjectWithAModelAndOutput_WhenOpeningThisProjectAndClosingWithOrWithoutClearModelOutput_TheOutputFilesShouldNotBeRemoved(bool performClearModelOutput)
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            using (var app = CreateRunningApplication(tempDirectory.Path))
            using (WaterQualityModel model = CreateValidWaqModel())
            {
                app.CreateNewProject();
                
                model.SetWorkingDirectoryInModelSettings(() => app.WorkDirectory);
                app.Project.RootFolder.Add(model);

                // Run
                IEnumerable<string> outputFilesAfterRun = RunModelAndGetOutputFiles(model);

                const string savedProjectFileName = "SavedProject.dsproj";
                string savedProjectPath = Path.Combine(tempDirectory.Path, savedProjectFileName);

                // Save 
                app.SaveProjectAs(savedProjectPath);
                string outputFolderAfterSavePath = model.OutputFolder.FullPath;

                Assert.AreEqual(outputFilesAfterRun, GetAllFileNames(outputFolderAfterSavePath));

                // Close
                app.CloseProject();

                // Open
                app.OpenProject(savedProjectPath);

                // Optionally, clear model output
                if (performClearModelOutput)
                {
                    model.ClearOutput();
                }

                // Close
                app.CloseProject();

                string errorMessage = performClearModelOutput
                                          ? "After opening, clear model output and closing a project, all output files that were saved in the project should still be there"
                                          : "After opening and closing a project, all output files that were saved in the project should still be there";
                Assert.AreEqual(outputFilesAfterRun, GetAllFileNames(outputFolderAfterSavePath), errorMessage);
            }
        }

        private static IEnumerable<string> RunModelAndGetOutputFiles(WaterQualityModel model)
        {
            ActivityRunner.RunActivity(model);

            Assert.That(model.Status, Is.EqualTo(ActivityStatus.Cleaned),
                        "Precondition violated. Model did nut run successfully.");

            string outputFolderPath = model.OutputFolder.FullPath;

            Assert.That(outputFolderPath.Contains("DeltaShell_Working_Directory"),
                        "When running a model, output should be placed in the application working directory.");

            IList<string> outputFiles = GetAllFileNames(outputFolderPath);

            Assert.That(outputFiles.Any(),
                        "Precondition violated. Model should have output after running.");

            return outputFiles;
        }

        private static WaterQualityModel CreateValidWaqModel()
        {
            string testDataDirPath = TestHelper.GetTestFilePath("ValidWaqModels");
            string hydFilePath = Path.Combine(testDataDirPath, "UGrid", "f34.hyd");
            string subFilePath = Path.Combine(testDataDirPath, "coli_04.sub");

            var model = new WaterQualityModel();
            new HydFileImporter().ImportItem(hydFilePath, model);
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

            // Make sure both binary (.map/.his) and NetCDF (.nc) output files are produced. See: D3DFMIQ-1272
            EditInputFileToCreateBinaryFiles(model);

            ValidationReport report = new WaterQualityModelValidator().Validate(model);
            Assert.That(report.ErrorCount, Is.EqualTo(0),
                        $"Precondition violated. Model is not valid and has errors: {string.Join(" - ", report.AllErrors.Select(e => e.Message))}");

            return model;
        }

        private static void EditInputFileToCreateBinaryFiles(WaterQualityModel model)
        {
            string inputFile = model.InputFile.Content;
            string editedInputFile = inputFile.Replace(
                                                  "0                                                  ; Switch on binary Map file",
                                                  "1                                                  ; Switch on binary Map file")
                                              .Replace(
                                                  "0                                                  ; Switch on binary History file",
                                                  "1                                                  ; Switch on binary History file");

            model.InputFile.Content = editedInputFile;
        }

        private static IList<string> GetAllFileNames(string directory)
        {
            return new DirectoryInfo(directory).GetFiles("*", SearchOption.AllDirectories)
                                               .Select(f => f.Name).ToList();
        }

        private static IApplication CreateRunningApplication(string tempDirectoryPath)
        {
            string workingDirectoryPath = Path.Combine(tempDirectoryPath, "DeltaShell_Working_Directory");
            ApplicationSettingsBase userSettings = ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);
            var application = CreateApplication();
            application.UserSettings = userSettings;
            application.Run();

            return application;
        }
        
        private static IApplication CreateApplication()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new WaterQualityModelApplicationPlugin(),
            };
            return new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
        }
        
        private static IApplication CreateApplicationWithWAQ()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new WaterQualityModelApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
            };
            return new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
        }
    }
}