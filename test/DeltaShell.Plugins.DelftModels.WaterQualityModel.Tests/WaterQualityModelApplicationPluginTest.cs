using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.DependencyInjection;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.Toolbox;
using NSubstitute;
using NUnit.Framework;
using LifeCycle = Deltares.Infrastructure.API.DependencyInjection.LifeCycle;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelApplicationPluginTest
    {
        private WaterQualityModelApplicationPlugin plugin;

        [SetUp]
        public void SetUp()
        {
            plugin = new WaterQualityModelApplicationPlugin();
        }

        [Test]
        public void DefaultConstructorExpectedValuesTest()
        {
            // assert
            Assert.IsInstanceOf<ApplicationPlugin>(plugin);
            Assert.AreEqual("Water quality model", plugin.Name,
                            "Name change detected, which impacts NHibernate persistency.");
            Assert.AreEqual("Allows to simulate water quality in rivers and channels.", plugin.Description);
            Assert.AreEqual(AssemblyUtils.GetAssemblyInfo(plugin.GetType().Assembly).Version, plugin.Version);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetFileImportersTest()
        {
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
            var pluginsToAdd = new List<IPlugin>
            {
                plugin,
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
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNotNull(plugin);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsNull_ThenHelperMethodReturnsNullAndRootFolderWillBeUsed()
        {
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNull(plugin);
        }

        [Test]
        public void CreateModelFunctionShouldSetWorkingDirectoryInModelSettingsBasedOnTheApplicationWorkingDirectory()
        {
            // Arrange
            string workingDirectoryPath = Path.Combine(Path.GetTempPath(), "test");
            SetupWaterQualityApplicationPlugin(workingDirectoryPath);

            // Act
            IEnumerable<ModelInfo> modelInfos = plugin.GetModelInfos();
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
                SetupWaterQualityApplicationPlugin(workingDirectoryPath);

                // Act
                IEnumerable<IFileImporter> importers = plugin.GetFileImporters();
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
            SetupWaterQualityApplicationPlugin(workingDirectoryPath);
            var model = new WaterQualityModel();
            model.SetWorkingDirectoryInModelSettings(() => plugin.Application.WorkDirectory);

            // Act
            string changedWorkingDirectoryPath = Path.Combine(Path.GetTempPath(), "changedWorkingDirectory");
            plugin.Application.UserSettings =
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
            Assembly[] assemblies = plugin.GetPersistentAssemblies().ToArray();

            // Assert
            Assert.That(assemblies.Length, Is.EqualTo(2));
            Assert.That(assemblies.Contains(typeof(FileBasedFolder).Assembly));
            Assert.That(assemblies.Contains(typeof(WaterQualityModelApplicationPlugin).Assembly));
        }

        [Test]
        public void AddRegistrations_RegistersServicesCorrectly()
        {
            var container = Substitute.For<IDependencyInjectionContainer>();

            plugin.AddRegistrations(container);

            container.Received(1).Register<IDataAccessListenersProvider, WaterQualityDataAccessListenersProvider>(LifeCycle.Transient);
        }

        private static void CreateDirectoryAndAddFiles(string persistentOutputFolder)
        {
            FileUtils.CreateDirectoryIfNotExists(persistentOutputFolder);
            File.WriteAllText(Path.Combine(persistentOutputFolder, "testfile"), "");
            Directory.CreateDirectory(Path.Combine(persistentOutputFolder, "testdirectory"));
        }

        private void SetupWaterQualityApplicationPlugin(
            string workingDirectoryPath)
        {
            var app = new DeltaShellApplicationBuilder().Build();
            app.UserSettings =  ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);
            plugin.Application = app;
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

        private IApplication GetConfiguredApplication()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                plugin,

            };
            return new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
        }
    }
}