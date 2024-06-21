using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.Toolbox;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelApplicationPluginTest
    {
        [Test]
        public void DefaultConstructorExpectedValuesTest()
        {
            // call
            var appPlugin = new WaterQualityModelApplicationPlugin();

            // assert
            Assert.IsInstanceOf<IDataAccessListenersProvider>(appPlugin);
            Assert.AreEqual("Water quality model", appPlugin.Name,
                            "Name change detected, which impacts NHibernate persistency.");
            Assert.AreEqual("Allows to simulate water quality in rivers and channels.", appPlugin.Description);
            Assert.AreEqual(AssemblyUtils.GetAssemblyInfo(appPlugin.GetType().Assembly).Version, appPlugin.Version);
        }

        [Test]
        public void CreateDataAccessListenersTest()
        {
            // setup
            var appPlugin = new WaterQualityModelApplicationPlugin();

            // call
            IDataAccessListener[] listeners = appPlugin.CreateDataAccessListeners().ToArray();

            // assert
            Assert.AreEqual(1, listeners.Length);
            Assert.IsInstanceOf<WaterQualityModelDataAccessListener>(listeners[0]);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetFileImportersTest()
        {
            // setup
            var plugin = new WaterQualityModelApplicationPlugin();

            // call
            IFileImporter[] importers = plugin.GetFileImporters().ToArray();

            // assert
            Assert.IsTrue(importers.Any(i => i is SubFileImporter));
            Assert.IsTrue(importers.Any(i => i is HydFileImporter));
            Assert.IsTrue(importers.Any(i => i is LoadsImporter));
            Assert.IsTrue(importers.Any(i => i is ObservationPointImporter));
            Assert.IsTrue(importers.Any(i => i is BoundaryDataTableImporter));
            Assert.IsTrue(importers.Any(i => i is LoadsDataTableImporter));
            Assert.IsTrue(importers.Any(i => i is WaterQualityObservationAreaImporter));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAModel_WhenModelIsRenamed_DataDirectoryPathIsChanged()
        {
            var waqPlugin = new WaterQualityModelApplicationPlugin();
            var pluginsToAdd = new List<IPlugin>
            {
                waqPlugin,
                new CommonToolsApplicationPlugin(),
                new NHibernateDaoApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetCdfApplicationPlugin(),
                new ScriptingApplicationPlugin(),
                new ToolboxApplicationPlugin(),
            };
            using (var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var waqModel = new WaterQualityModel {Name = "WAQ1"};
               
                app.Run();

                app.CreateNewProject();

                string tempDirectory = FileUtils.CreateTempDirectory();
                app.SaveProjectAs(Path.Combine(tempDirectory, "WAQ_proj"));

                app.Project.RootFolder.Items.Add(waqModel);

                string originalOutputDirectory = waqModel.ModelSettings.OutputDirectory;
                string originalDataDirectory = waqModel.ModelDataDirectory;
                Assert.That(Path.Combine(tempDirectory, "WAQ_proj_data\\WAQ1\\output"), Is.EqualTo(originalOutputDirectory));
                Assert.That(Path.Combine(tempDirectory, "WAQ_proj_data\\WAQ1"), Is.EqualTo(originalDataDirectory));

                waqModel.Name = "WAQ2";
                string newOutputDirectory = waqModel.ModelSettings.OutputDirectory;
                string newDataDirectory = waqModel.ModelDataDirectory;
                Assert.That(Path.Combine(tempDirectory, "WAQ_proj_data\\WAQ2\\output"), Is.EqualTo(newOutputDirectory));
                Assert.That(Path.Combine(tempDirectory, "WAQ_proj_data\\WAQ2"), Is.EqualTo(newDataDirectory));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ImportCorrectSubFileAndThenCorruptItAndExpectExceptionMessage()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var app = GetRunningApplication(tempDirectory.Path))
            using (WaterQualityModel model = GetWesternscheldtModelInApplication(tempDirectory.Path, app))
            {
                string boundaryDataTableFilePath = Path.Combine(model.BoundaryDataManager.FolderPath, "bacteria.tbl");
                Assert.True(File.Exists(boundaryDataTableFilePath));

                // Simulate corruption of boundary table data
                using (StreamWriter sw = File.AppendText(boundaryDataTableFilePath))
                {
                    sw.WriteLine("The Corruption");
                    sw.WriteLine("Spreads in this file");
                }

                string expectedExceptionMsg =
                    string.Format(Resources.WaterQualityModel_OnInitializeCore_Failed_to_initialize_pre_processor__0_Please_look_at_the_List_file_for_more_information__0_List_file_found_in__Project_view____Output____List_file__0___1_,
                                  Environment.NewLine, model.ModelSettings.OutputDirectory);

                //Expect the exception message thrown as log message
                TestHelper.AssertAtLeastOneLogMessagesContains(() => ActivityRunner.RunActivity(model), expectedExceptionMsg);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void Check_When_RunningTwice_WaqModel_OutputFiles_And_Saving_TheFilesArePersisted()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var app = GetRunningApplication(tempDirectory.Path))
            using (WaterQualityModel model = GetWesternscheldtModelInApplication(tempDirectory.Path, app))
            {
                //First run
                ActivityRunner.RunActivity(model);
                Assert.AreEqual(model.Status, ActivityStatus.Cleaned);

                //save the project
                app.SaveProject();

                //Second run
                ActivityRunner.RunActivity(model);
                CheckDataItems(model);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.Integration)]
        public void GivenAWaqModelInAProject_WhenSavingTheProjectInANewLocation_ThenAllPathsShouldSwitchToNewLocation()
        {
            var modelPathsDictionary = new Dictionary<string, IList<string>>
            {
                {"model directory", new List<string>()},
                {"output directory", new List<string>()},
                {"boundary data manager folder path", new List<string>()},
                {"loads data manager folder path", new List<string>()}
            };

            using (var tempDir = new TemporaryDirectory())
            {
                // Given
                string tempDirPath = tempDir.Path;
                const string firstSaveName = "save1";
                string firstSavePath = Path.Combine(tempDirPath, $"{firstSaveName}.dsproj");
                string firstSaveFolder = firstSavePath + "_data";
                const string secondSaveName = "save2";
                string secondSavePath = Path.Combine(tempDirPath, $"{secondSaveName}.dsproj");
                string secondSaveFolder = secondSavePath + "_data";

                var model = new WaterQualityModel();

                using (var app = GetConfiguredApplication())
                {
                    app.Run();
                    app.CreateNewProject();
                    app.Project.RootFolder.Add(model);
                    AddCurrentModelPaths(model);

                    // When (saving as first time)
                    app.SaveProjectAs(firstSavePath);
                    AddCurrentModelPaths(model);

                    // When (saving as second time)
                    app.SaveProjectAs(secondSavePath);
                    AddCurrentModelPaths(model);
                }

                // Then
                foreach (KeyValuePair<string, IList<string>> kvp in modelPathsDictionary)
                {
                    string key = kvp.Key;
                    IList<string> paths = kvp.Value;

                    string pathBeforeSave = paths[0];
                    string pathAfterFirstSaveAs = paths[1];
                    string pathAfterSecondSaveAs = paths[2];

                    Assert.AreNotEqual(pathBeforeSave, pathAfterFirstSaveAs,
                                       $"Path of the {key} should have changed after saving from temp to persistent location.");
                    Assert.That(pathAfterFirstSaveAs.StartsWith(firstSaveFolder, StringComparison.InvariantCulture),
                                $"The {key} should have been located inside the folder {firstSaveFolder}, but was at {pathAfterFirstSaveAs}");
                    Assert.AreNotEqual(pathAfterFirstSaveAs, pathAfterSecondSaveAs,
                                       $"Path of the {key} should have changed after saving project at a new location when project was already saved once.");
                    Assert.That(pathAfterSecondSaveAs.StartsWith(secondSaveFolder, StringComparison.InvariantCulture),
                                $"The {key} should have been located inside the folder {secondSaveFolder}");
                }
            }

            void AddCurrentModelPaths(WaterQualityModel waqModel)
            {
                modelPathsDictionary["model directory"].Add(waqModel.ModelDataDirectory);
                modelPathsDictionary["output directory"].Add(waqModel.ModelSettings.OutputDirectory);
                modelPathsDictionary["boundary data manager folder path"].Add(waqModel.BoundaryDataManager.FolderPath);
                modelPathsDictionary["loads data manager folder path"].Add(waqModel.LoadsDataManager.FolderPath);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsCompositeActivity_ThenHelperMethodReturnsCompositeActivityAndThisWillBeUsed()
        {
            var waterQualityModelApplicationPlugin = new WaterQualityModelApplicationPlugin();
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNotNull(waterQualityModelApplicationPlugin);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsNull_ThenHelperMethodReturnsNullAndRootFolderWillBeUsed()
        {
            var waterQualityModelApplicationPlugin = new WaterQualityModelApplicationPlugin();
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNull(waterQualityModelApplicationPlugin);
        }

        [Test]
        public void CreateModelFunctionShouldSetWorkingDirectoryInModelSettingsBasedOnTheApplicationWorkingDirectory()
        {
            // Arrange
            string workingDirectoryPath = Path.Combine(Path.GetTempPath(), "test");
            WaterQualityModelApplicationPlugin applicationPlugin = SetupWaterQualityApplicationPlugin(workingDirectoryPath);

            // Act
            IEnumerable<ModelInfo> modelInfos = applicationPlugin.GetModelInfos();
            IModel model = modelInfos.First().CreateModel(null);

            // Assert
            Assert.AreEqual(Path.Combine(workingDirectoryPath, model.Name.Replace(" ", "_")), ((WaterQualityModel) model).ModelSettings.WorkDirectory);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetFileImportersShouldReturnHydFileImporterWithWorkingDirectoryOfTheApplicationAsArgument()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                string squareHydPath = TestHelper.GetTestFilePath(@"IO\square\square.hyd");
                string hydFileName = Path.GetFileName(squareHydPath);
                string squareHydTempDirectory =
                    tempDirectory.CopyDirectoryToTempDirectory(Path.GetDirectoryName(squareHydPath));
                string squareHydTempPath = Path.Combine(squareHydTempDirectory, hydFileName);

                string workingDirectoryPath = Path.Combine(Path.GetTempPath(), "test");
                WaterQualityModelApplicationPlugin applicationPlugin = SetupWaterQualityApplicationPlugin(workingDirectoryPath);

                // Act
                IEnumerable<IFileImporter> importers = applicationPlugin.GetFileImporters();
                IFileImporter hydFileImporter = importers.FirstOrDefault(i => i is HydFileImporter);

                // Assert
                Assert.NotNull(hydFileImporter);

                // Import model to check StoreWorkingDirectoryPathFunc, since it is private
                using (var model = (WaterQualityModel) hydFileImporter.ImportItem(squareHydTempPath))
                {
                    Assert.AreEqual(Path.Combine(workingDirectoryPath, model.Name.Replace(" ", "_")),
                                    model.ModelSettings.WorkDirectory);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AfterOpeningAProjectTheWorkingDirectoryInModelSettingsShouldBeBasedOnTheCurrentApplicationWorkingDirectory()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var application = GetConfiguredApplication())
            using (var model = new WaterQualityModel())
            {
                // Arrange
                string workingDirectoryPath = Path.Combine(Path.GetTempPath(), "workingDirectory");
                application.UserSettings = ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);

                model.SetWorkingDirectoryInModelSettings(() => application.WorkDirectory);
                application.Run();
                application.CreateNewProject();
                application.Project.RootFolder.Add(model);

                string savePath = Path.Combine(tempDirectory.Path, "test");

                // Create project files
                application.SaveProjectAs(savePath);
                application.CloseProject();

                string changedWorkingDirectoryPath = Path.Combine(Path.GetTempPath(), "changedWorkingDirectory");
                application.UserSettings =
                    ApplicationTestHelper.GetMockedApplicationSettingsBase(changedWorkingDirectoryPath);

                // Act
                application.OpenProject(savePath);

                // Assert
                Assert.AreEqual(Path.Combine(changedWorkingDirectoryPath, model.Name.Replace(" ", "_")),
                                ((WaterQualityModel) application.Project.RootFolder.Models.First())
                                .ModelSettings.WorkDirectory);

                application.CloseProject();
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ChangeWorkingDirectoryInApplication_ShouldUpdateTheWorkingDirectoryInModelSettings()
        {
            // Arrange
            string workingDirectoryPath = Path.Combine(Path.GetTempPath(), "workingDirectory");
            WaterQualityModelApplicationPlugin applicationPlugin = SetupWaterQualityApplicationPlugin(workingDirectoryPath);
            var model = new WaterQualityModel();
            model.SetWorkingDirectoryInModelSettings(() => applicationPlugin.Application.WorkDirectory);

            // Act
            string changedWorkingDirectoryPath = Path.Combine(Path.GetTempPath(), "changedWorkingDirectory");
            applicationPlugin.Application.UserSettings =
                ApplicationTestHelper.GetMockedApplicationSettingsBase(changedWorkingDirectoryPath);

            // Assert
            Assert.AreEqual(Path.Combine(changedWorkingDirectoryPath, model.Name.Replace(" ", "_")),
                            model.ModelSettings.WorkDirectory);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AfterProjectSave_WhenOutputFolderIsNull_ApplicationShouldDeleteAllDisconnectedOutputInPersistentFolder()
        {
            // Arrange
            using (var tempDirectory = new TemporaryDirectory())
            using (var application = GetConfiguredApplication())
            using (var model = new WaterQualityModel())
            {
                application.Run();
                application.CreateNewProject();
                application.Project.RootFolder.Add(model);

                string savePath = Path.Combine(tempDirectory.Path, "test");

                application.SaveProjectAs(savePath);

                string persistentOutputFolder = model.ModelSettings.OutputDirectory;

                CreateDirectoryAndAddFiles(persistentOutputFolder);

                // Act
                application.SaveProjectAs(savePath);

                // Assert
                Assert.IsNull(model.OutputFolder);
                Assert.IsFalse(Directory.Exists(persistentOutputFolder));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AfterProjectSave_WhenOutputFolderPathIsNull_ApplicationShouldDeleteAllDisconnectedOutputInPersistentFolder()
        {
            //Arrange
            using (var tempDirectory = new TemporaryDirectory())
            using (var application = GetConfiguredApplication())
            using (var model = new WaterQualityModel())
            {
                application.Run();
                application.CreateNewProject();
                application.Project.RootFolder.Add(model);

                string savePath = Path.Combine(tempDirectory.Path, "test");

                application.SaveProjectAs(savePath);

                string persistentOutputFolder = model.ModelSettings.OutputDirectory;

                CreateDirectoryAndAddFiles(persistentOutputFolder);
                model.OutputFolder = new FileBasedFolder();

                // Act
                application.SaveProjectAs(savePath);

                //Assert
                Assert.IsNotNull(model.OutputFolder);
                Assert.IsNull(model.OutputFolder.Path);
                Assert.IsFalse(Directory.Exists(persistentOutputFolder));
            }
        }

        [Test]
        public void GetPersistentAssemblies_ThenCorrectAssembliesAreReturned()
        {
            // Call
            Assembly[] assemblies = new WaterQualityModelApplicationPlugin().GetPersistentAssemblies().ToArray();

            // Assert
            Assert.That(assemblies.Length, Is.EqualTo(2));
            Assert.That(assemblies.Contains(typeof(FileBasedFolder).Assembly));
            Assert.That(assemblies.Contains(typeof(WaterQualityModelApplicationPlugin).Assembly));
        }

        private static void CreateDirectoryAndAddFiles(string persistentOutputFolder)
        {
            FileUtils.CreateDirectoryIfNotExists(persistentOutputFolder);
            File.WriteAllText(Path.Combine(persistentOutputFolder, "testfile"), "");
            Directory.CreateDirectory(Path.Combine(persistentOutputFolder, "testdirectory"));
        }

        private static WaterQualityModelApplicationPlugin SetupWaterQualityApplicationPlugin(
            string workingDirectoryPath)
        {
            var app = new DeltaShellApplicationBuilder().Build();
            app.UserSettings =  ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);
            
            return new WaterQualityModelApplicationPlugin
            {
                Application = app
            };
        }

        private static WaterQualityModel GetWesternscheldtModelInApplication(string tempDirectory, IApplication application)
        {
            var waqModel = new WaterQualityModel();

            application.CreateNewProject();
            application.Project.RootFolder.Add(waqModel);
            waqModel.SetWorkingDirectoryInModelSettings(() => application.WorkDirectory);

            string originalDir = TestHelper.GetTestFilePath("WaterQualityDataFiles");
            FileUtils.CopyAll(new DirectoryInfo(originalDir), new DirectoryInfo(tempDirectory), string.Empty);

            string hydFilePath = Path.Combine(tempDirectory, "flow-model", "westernscheldt01.hyd");
            Assert.True(File.Exists(hydFilePath));

            string subFilePath = Path.Combine(tempDirectory, "waq", "sub-files", "bacteria.sub");
            Assert.True(File.Exists(subFilePath));

            string boundaryConditionsFilePath = Path.Combine(tempDirectory, "waq", "boundary-conditions", "bacteria.csv");
            Assert.True(File.Exists(boundaryConditionsFilePath));

            new HydFileImporter().ImportItem(hydFilePath, waqModel);
            new SubFileImporter().Import(waqModel.SubstanceProcessLibrary, subFilePath);
            new DataTableImporter().ImportItem(boundaryConditionsFilePath, waqModel.BoundaryDataManager);

            return waqModel;
        }

        private static IApplication GetConfiguredApplication()
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
        
        private static IApplication GetRunningApplication(string tempDirectory)
        {
            string workingDirectoryPath = Path.Combine(tempDirectory, "DeltaShell_Working_Directory");
            ApplicationSettingsBase userSettings = ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);

            var pluginsToAdd = new List<IPlugin>
            {
                new SharpMapGisApplicationPlugin(),
                new WaterQualityModelApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new NHibernateDaoApplicationPlugin(),
                new NetCdfApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new ScriptingApplicationPlugin(),
                new ToolboxApplicationPlugin(),
            };
            var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
            app.UserSettings = userSettings;
            
            app.Run();
            app.CreateNewProject();
            app.SaveProjectAs(Path.Combine(tempDirectory, "WAQ_proj"));

            return app;
        }

        private static void CheckDataItems(WaterQualityModel waqModel)
        {
            //Check data items
            IList<string> dataItemTags = GetDataItemTags(waqModel);

            string[] expectedDataItemTags =
            {
                WaterQualityModel.ListFileDataItemMetaData.Tag,
                WaterQualityModel.ProcessFileDataItemMetaData.Tag,
                WaterQualityModel.MonitoringFileDataItemMetaData.Tag
            };

            foreach (string expectedTag in expectedDataItemTags)
            {
                Assert.IsTrue(dataItemTags.Any(t => t == expectedTag),
                              $"DataItem with tag {expectedTag} not found in dataItems {string.Join(", ", dataItemTags)}");
            }
        }

        private static IList<string> GetDataItemTags(WaterQualityModel waqModel)
        {
            List<IDataItem> dataItems = waqModel.DataItems.Where(di => di.Role == DataItemRole.Output
                                                                       && di.ValueType == typeof(TextDocument)).ToList();
            Assert.IsTrue(dataItems.Any());
            Assert.AreEqual(3, dataItems.Count);
            return dataItems.Select(di => di.Tag).ToList();
        }
    }
}