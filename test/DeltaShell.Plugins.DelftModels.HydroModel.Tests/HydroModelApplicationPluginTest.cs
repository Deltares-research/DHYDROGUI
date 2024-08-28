using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.Wave;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class HydroModelApplicationPluginTest
    {
        [Test]
        public void Constructor_DefaultsCorrectlyInitialized()
        {
            var hydroModelApplicationPlugin = new HydroModelApplicationPlugin { Application = Substitute.For<IApplication>() };

            StringAssert.AreEqualIgnoringCase("Hydro Model",hydroModelApplicationPlugin.Name);
            StringAssert.AreEqualIgnoringCase("Hydro Model Plugin",hydroModelApplicationPlugin.DisplayName);
            StringAssert.AreEqualIgnoringCase(hydroModelApplicationPlugin.Description,"Provides functionality to create and run integrated models.");
            StringAssert.AreEqualIgnoringCase(hydroModelApplicationPlugin.FileFormatVersion, "1.3.0.0");

            List<ModelInfo> modelInfos = hydroModelApplicationPlugin.GetModelInfos().ToList();
            Assert.AreEqual(2, modelInfos.Count);

            ModelInfo first = modelInfos[0];
            StringAssert.AreEqualIgnoringCase(first.Name, "Empty Integrated Model");

            ModelInfo second = modelInfos[1];
            StringAssert.AreEqualIgnoringCase(second.Name, "2D-3D Integrated Model");

            List<ProjectTemplate> projectTemplates = hydroModelApplicationPlugin.ProjectTemplates().ToList();
            Assert.AreEqual(0,projectTemplates.Count);

            Assert.AreEqual(1, hydroModelApplicationPlugin.GetFileExporters().ToList().Count);
            Assert.AreEqual(1, hydroModelApplicationPlugin.GetFileImporters().ToList().Count);
            Assert.IsInstanceOf<DHydroConfigXmlExporter>(hydroModelApplicationPlugin.GetFileExporters().First());
            Assert.IsInstanceOf<DHydroConfigXmlImporter>(hydroModelApplicationPlugin.GetFileImporters().First());
        }
        [Test]
        public void AdditionalOwnerCheckTest_HydroModel()
        {
            using (var app = CreateRunningApplication())
            {
                var appPlugin = app.Plugins.OfType<HydroModelApplicationPlugin>().Single();
                Project project = app.ProjectService.CreateProject();
                
                ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_RealTimeControl()
        {
            using (IApplication app = CreateRunningApplication(b => b.WithRealTimeControl()))
            {
                var appPlugin = app.Plugins.OfType<RealTimeControlApplicationPlugin>().Single();
                Project project = app.ProjectService.CreateProject();
                ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(project.RootFolder), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_WaterQuality()
        {
            using (IApplication app = CreateRunningApplication())
            {
                var appPlugin = app.Plugins.OfType<HydroModelApplicationPlugin>().Single();
                Project project = app.ProjectService.CreateProject();
                ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void AdditionalOwnerCheckTest_FlowFM()
        {
            using (var app = CreateRunningApplication(b => b.WithFlowFM()))
            {
                var appPlugin = app.Plugins.OfType<FlowFMApplicationPlugin>().Single();
                Project project = app.ProjectService.CreateProject();

                ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_Wave()
        {
            var appPlugin = new WaveApplicationPlugin();
            using (IApplication app = CreateRunningApplication(b => b.WithWaves()))
            {
                Project project = app.ProjectService.CreateProject();
                ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsCompositeActivity_ThenHelperMethodReturnsCompositeActivityAndThisWillBeUsed()
        {
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNotNull<HydroModelApplicationPlugin>(
                b => b.WithHydroModel());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsNull_ThenHelperMethodReturnsNullAndRootFolderWillBeUsed()
        {
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNull<HydroModelApplicationPlugin>(
                b => b.WithHydroModel());
        }
        
        [Test]
        public void GivenAnApplicationWithHydroModelPluginLoaded_WhenAHydroModelIsAdded_ThenTheRegisteredFileExportersShouldBeSet()
        {
            // Setup
            using (var application = CreateRunningApplication())
            {
                // Given
                var model = new HydroModel();

                Project project = application.ProjectService.CreateProject();

                // Call
                project.RootFolder.Add(model);

                // Assert
                IFileExportService fileExportService = model.HydroModelExporter.FileExportService;
                Assert.That(fileExportService.FileExporters, Has.One.InstanceOf<DHydroConfigXmlExporter>());
            }
        }

        [Test]
        public void GivenAnApplicationWithHydroModelAndFlowFmPluginLoaded_WhenGettingFileImporters_ThenADimrImporterShouldBeReturnedThatCanImportOnWaterFlowFMModel()
        {
            // Given
            using (var application = CreateRunningApplication(b => b.WithHydroModel().WithFlowFM()))
            {
                var hydroModelAppPlugin = application.Plugins.OfType<HydroModelApplicationPlugin>().Single();

                // When 
                IEnumerable<IFileImporter> applicationFileImporters = hydroModelAppPlugin.GetFileImporters().ToArray();
                int fileImportersCounter = applicationFileImporters.Count();

                // Then
                Assert.AreEqual(1, fileImportersCounter, $"Expected only 1 Dimr Importer, but {fileImportersCounter} importers were found");
                var dimrImporter = applicationFileImporters.First() as DHydroConfigXmlImporter;
                Assert.IsNotNull(dimrImporter, "The retrieved importer is not a Dimr Importer");
                Assert.IsTrue(dimrImporter.CanImportOn(new WaterFlowFMModel()), "The Dimr importer is missing the WaterFlowFMFileImporter");
            }
        }

        [Test]
        public void GivenAProjectWithAModelThatHasAChildModel_WhenAddingAModelTheSameNameAsTheChildModel_ThenModelIsRenamed()
        {
            // Setup
            using (var app = CreateApplicationWithModel("parent_model"))
            {
                Project project = app.ProjectService.Project;

                IModel integratedModel = project.RootFolder.GetAllModelsRecursive().Single();

                var childModel = Substitute.For<IModel>();
                childModel.Name = "Unique";

                integratedModel.GetDirectChildren().Returns(new[]
                {
                    childModel
                });

                var model = Substitute.For<IModel>();
                model.Name = "Unique";

                // Call
                project.RootFolder.Add(model);

                // Assert
                Assert.That(model.Name, Is.EqualTo("Unique (1)"),
                            "When project contains model with same name, model should be renamed.");
            }
        }

        [Test]
        public void GivenAProject_WhenAModelIsAdded_ThenModelNameShouldBeTrimmed()
        {
            // Setup
            using (var app = CreateRunningApplication())
            {
                Project project = app.ProjectService.CreateProject();
                
                var model = Substitute.For<IModel>();
                model.Name = "  Name  ";

                // Call
                project.RootFolder.Add(model);

                // Assert
                Assert.That(model.Name, Is.EqualTo("Name"),
                            "When adding a model to the project, model name should be trimmer.");
            }
        }

        [Test]
        public void GivenAProjectWithAModel_WhenRenamingTheModel_TheModelNameShouldBeTrimmed()
        {
            // Setup
            using (var app = CreateRunningApplication())
            {
                Project project = app.ProjectService.CreateProject();
                
                // ModelBase implements INotifyPropertyChange
                var model = Substitute.ForPartsOf<ModelBase>();
                model.Name = "original_name";
                project.RootFolder.Add(model);

                // Call
                model.Name = "  Name  ";

                // Assert
                Assert.That(model.Name, Is.EqualTo("Name"),
                            "When a model in the project is renamed, the model name should be trimmed.");
            }
        }

        [Test]
        public void GivenAProject_WhenAddingAModelWithAChildModel_ThenModelNamesAreTrimmed()
        {
            // Setup
            using (var app = CreateRunningApplication())
            {
                Project project = app.ProjectService.CreateProject();
                
                var parentModel = Substitute.For<IModel>();
                parentModel.Name = "  parent  ";

                var childModel = Substitute.For<IModel>();
                childModel.Name = "  child  ";

                parentModel.GetDirectChildren().Returns(new[]
                {
                    childModel
                });

                // Call
                project.RootFolder.Add(parentModel);

                // Assert
                Assert.That(parentModel.Name, Is.EqualTo("parent"),
                            "When model is added to the project, then model name should be trimmed.");
                Assert.That(childModel.Name, Is.EqualTo("child"),
                            "When model with a child model is added to the project, then the child model name should also be trimmed.");
            }
        }

        [Test]
        public void GetModelInfos_ShouldReturnModelInfoWithFuncCreateModelWhichSetWorkingDirectoryPathFuncInHydroModel()
        {
            // Arrange
            var appPlugin = new HydroModelApplicationPlugin();
            var application = Substitute.For<IApplication>();
            const string applicationWorkingDirectory = "WorkingDirectory";
            application.WorkDirectory.Returns(applicationWorkingDirectory);
            appPlugin.Application = application;

            // Act
            ModelInfo modelInfos = appPlugin.GetModelInfos().First();

            // Assert
            var hydroModel = modelInfos.CreateModel(Substitute.For<IProjectItem>()) as HydroModel;
            Assert.IsNotNull(hydroModel);
            Assert.AreEqual(applicationWorkingDirectory, hydroModel.WorkingDirectoryPathFunc());

            const string applicationWorkingDirectory2 = "WorkingDirectory2";
            application.WorkDirectory.Returns(applicationWorkingDirectory2);
            Assert.AreEqual(applicationWorkingDirectory2, hydroModel.WorkingDirectoryPathFunc());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetFileImporters_ShouldReturnDimrXmlImporterWhichCreatesAHydroModelUsingApplicationWorkingDirectory()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                string dimrFilePathInTemp = tempDirectory.CopyTestDataFileAndDirectoryToTempDirectory(Path.Combine("FileReader", "dimr.xml"));

                var appPlugin = new HydroModelApplicationPlugin();
                var application = Substitute.For<IApplication>();
                const string applicationWorkingDirectory = "TestWorkingDirectory";
                application.WorkDirectory.Returns(applicationWorkingDirectory);
                appPlugin.Application = application;

                // Act
                IFileImporter importer = appPlugin.GetFileImporters().Single();

                // Assert

                // Check type of importer
                Assert.IsInstanceOf(typeof(DHydroConfigXmlImporter), importer);

                // Check if the correct constructor argument was provided in the HydroModelApplicationPlugin
                object importedModel = importer.ImportItem(dimrFilePathInTemp);
                Assert.AreEqual(applicationWorkingDirectory, ((HydroModel) importedModel).WorkingDirectoryPathFunc());
            }
        }

        [Test]
        public void ProjectOpened_ShouldSetWorkingDirectoryPathFuncInHydroModel()
        {
            // Arrange
            var appPlugin = new HydroModelApplicationPlugin();
            var application = Substitute.For<IApplication>();
            IProjectService projectService = application.ProjectService;
            const string applicationWorkingDirectory = "TestWorkingDirectory";
            application.WorkDirectory.Returns(applicationWorkingDirectory);
            appPlugin.Application = application;

            var hydroModel = new HydroModel();
            AddToProject(hydroModel, projectService);

            // Act
            projectService.ProjectOpened += Raise.EventWith(this, new EventArgs<Project>(projectService.Project));

            // Assert
            Assert.AreEqual(applicationWorkingDirectory, hydroModel.WorkingDirectoryPathFunc());
        }

        [TestCase("Unique", "Unique (1)")]
        [TestCase("  Unique ", "Unique (1)")]
        [TestCase("Unique (1)", "Unique (1) (1)")]
        [TestCase("  Unique (1)   ", "Unique (1) (1)")]
        public void GivenAProjectWithAModel_WhenAddingANewModelWithTheSameName_ThenModelIsRenamed(string duplicateModelName, string expectedModelName)
        {
            // Setup
            using (var app = CreateApplicationWithModel(duplicateModelName))
            {
                Project project = app.ProjectService.Project;
                
                var model = Substitute.For<IModel>();
                model.Name = duplicateModelName;

                // Call
                project.RootFolder.Add(model);

                // Assert
                Assert.That(model.Name, Is.EqualTo(expectedModelName),
                            "When project contains model with same name, model should be renamed.");
            }
        }

        [TestCase("Unique", "Unique", "Unique (1)")]
        [TestCase("Unique", " Unique ", "Unique (1)")]
        [TestCase("Unique (1)", "Unique (1)", "Unique (1) (1)")]
        [TestCase("Unique (1)", " Unique (1) ", "Unique (1) (1)")]
        public void GivenAProjectWithAModel_WhenAddingANewModelWithTheSameName_ThenModelIsRenamed(string originalName, string duplicateModelName, string expectedModelName)
        {
            // Setup
            using (var app = CreateApplicationWithModel(originalName))
            {
                Project project = app.ProjectService.Project;
                
                var model = Substitute.For<IModel>();
                model.Name = duplicateModelName;

                // Call
                project.RootFolder.Add(model);

                // Assert
                Assert.That(model.Name, Is.EqualTo(expectedModelName),
                            "When project contains model with same name, model should be renamed.");
            }
        }

        [TestCase("Unique")]
        [TestCase("  Unique  ")]
        public void GivenAProjectWithAModel_WhenAddingAModelWithAChildModelWithTheSameName_ThenModelIsRenamed(string childModelName)
        {
            // Setup
            using (var app = CreateApplicationWithModel("Unique"))
            {
                Project project = app.ProjectService.Project;
                
                var parentModel = Substitute.For<IModel>();
                parentModel.Name = "parent_model";

                var childModel = Substitute.For<IModel>();
                childModel.Name = childModelName;

                parentModel.GetDirectChildren().Returns(new[]
                {
                    childModel
                });

                // Call
                project.RootFolder.Add(parentModel);

                // Assert
                Assert.That(childModel.Name, Is.EqualTo("Unique (1)"),
                            "When project contains model with same name, model should be renamed.");
            }
        }

        [TestCase("Unique")]
        [TestCase(" Unique ")]
        public void GivenAProjectWithAModel_WhenRenamingAnotherModelToTheSameName_ThenModelIsRenamed(string duplicateName)
        {
            // Setup
            using (var app = CreateApplicationWithModel("Unique"))
            {
                Project project = app.ProjectService.Project;
                
                // ModelBase implements INotifyPropertyChange
                var model = Substitute.ForPartsOf<ModelBase>();

                project.RootFolder.Add(model);

                // Call
                model.Name = duplicateName;

                // Assert
                Assert.That(model.Name, Is.EqualTo("Unique (1)"),
                            "When project contains model with same name, model should be renamed.");
            }
        }

        private static IApplication CreateApplicationWithModel(string modelName)
        {
            IApplication application = CreateRunningApplication();

            var model = Substitute.For<IModel>();
            model.Name = modelName;

            Project project = application.ProjectService.CreateProject();
            project.RootFolder.Add(model);

            return application;
        }

        private static IApplication CreateRunningApplication()
        {
            return CreateRunningApplication(b => b.WithHydroModel());
        }

        private static IApplication CreateRunningApplication(Func<DHYDROApplicationBuilder, DHYDROApplicationBuilder> function)
        {
            var builder = new DHYDROApplicationBuilder();
            IApplication application = function(builder).Build();
            application.Run();
            return application;
        }

        private static void AddToProject(object obj, IProjectService projectService)
        {
            var project = new Project();
            project.RootFolder.Add(obj);
            projectService.Project.Returns(project);
        }
    }
}