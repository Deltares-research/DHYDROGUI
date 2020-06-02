using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
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
        public void AdditionalOwnerCheckTest_HydroModel()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new HydroModelApplicationPlugin();
                SetUpApplication(app, appPlugin);

                ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_RealTimeControl()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new RealTimeControlApplicationPlugin();
                SetUpApplication(app, appPlugin);

                ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_WaterQuality()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new WaterQualityModelApplicationPlugin();
                SetUpApplication(app, appPlugin);

                ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void AdditionalOwnerCheckTest_FlowFM()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new FlowFMApplicationPlugin();
                SetUpApplication(app, appPlugin);

                ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_Wave()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new WaveApplicationPlugin();
                SetUpApplication(app, appPlugin);

                ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsCompositeActivity_ThenHelperMethodReturnsCompositeActivityAndThisWillBeUsed()
        {
            var hydroModelApplicationPlugin = new HydroModelApplicationPlugin();
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNotNull(hydroModelApplicationPlugin);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsNull_ThenHelperMethodReturnsNullAndRootFolderWillBeUsed()
        {
            var hydroModelApplicationPlugin = new HydroModelApplicationPlugin();
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNull(hydroModelApplicationPlugin);
        }

        [Test]
        public void GivenAnApplicationWithHydroModelAndFlowFmPluginLoaded_WhenGettingFileImporters_ThenADimrImporterShouldBeReturnedThatCanImportOnWaterFlowFMModel()
        {
            using (var application = new DeltaShellApplication())
            {
                // Given
                var hydroModelAppPlugin = new HydroModelApplicationPlugin();

                application.Plugins.Add(hydroModelAppPlugin);
                application.Plugins.Add(new FlowFMApplicationPlugin());

                application.Run();

                // When 
                IEnumerable<IFileImporter> applicationFileImporters = hydroModelAppPlugin.GetFileImporters().ToArray();
                int fileImportersCounter = applicationFileImporters.Count();

                // Then
                Assert.AreEqual(1, fileImportersCounter,
                                $"Expected only 1 Dimr Importer, but {fileImportersCounter} importers were found");
                var dimrImporter = applicationFileImporters.First() as DHydroConfigXmlImporter;
                Assert.IsNotNull(dimrImporter, "The retrieved importer is not a Dimr Importer");
                Assert.IsTrue(dimrImporter.CanImportOn(new WaterFlowFMModel()), "The Dimr importer is missing the WaterFlowFMFileImporter");
            }
        }

        [Test]
        public void GivenAProjectWithAModelThatHasAChildModel_WhenAddingAModelTheSameNameAsTheChildModel_ThenModelIsRenamed()
        {
            // Setup
            using (DeltaShellApplication app = CreateApplicationWithModel("parent_model"))
            {
                IModel integratedModel = app.GetAllModelsInProject().Single();

                var childModel = Substitute.For<IModel>();
                childModel.Name = "Unique";

                integratedModel.GetDirectChildren().Returns(new[]
                {
                    childModel
                });

                var model = Substitute.For<IModel>();
                model.Name = "Unique";

                // Call
                app.Project.RootFolder.Add(model);

                // Assert
                Assert.That(model.Name, Is.EqualTo("Unique (1)"),
                            "When project contains model with same name, model should be renamed.");
            }
        }

        [Test]
        public void GivenAProject_WhenAModelIsAdded_ThenModelNameShouldBeTrimmed()
        {
            // Setup
            using (DeltaShellApplication app = CreateApplication())
            {
                var model = Substitute.For<IModel>();
                model.Name = "  Name  ";

                // Call
                app.Project.RootFolder.Add(model);

                // Assert
                Assert.That(model.Name, Is.EqualTo("Name"),
                            "When adding a model to the project, model name should be trimmer.");
            }
        }

        [Test]
        public void GivenAProjectWithAModel_WhenRenamingTheModel_TheModelNameShouldBeTrimmed()
        {
            // Setup
            using (DeltaShellApplication app = CreateApplication())
            {
                // ModelBase implements INotifyPropertyChange
                var model = Substitute.ForPartsOf<ModelBase>();
                model.Name = "original_name";
                app.Project.RootFolder.Add(model);

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
            using (DeltaShellApplication app = CreateApplication())
            {
                var parentModel = Substitute.For<IModel>();
                parentModel.Name = "  parent  ";

                var childModel = Substitute.For<IModel>();
                childModel.Name = "  child  ";

                parentModel.GetDirectChildren().Returns(new[]
                {
                    childModel
                });

                // Call
                app.Project.RootFolder.Add(parentModel);

                // Assert
                Assert.That(parentModel.Name, Is.EqualTo("parent"),
                            "When model is added to the project, then model name should be trimmed.");
                Assert.That(childModel.Name, Is.EqualTo("child"),
                            "When model with a child model is added to the project, then the child model name should also be trimmed.");
            }
        }

        private static void SetUpApplication(DeltaShellApplication app, ApplicationPlugin appPlugin)
        {
            app.Project = new Project();
            appPlugin.Application = app;
        }

        [TestCase("Unique", "Unique (1)")]
        [TestCase("  Unique ", "Unique (1)")]
        [TestCase("Unique (1)", "Unique (1) (1)")]
        [TestCase("  Unique (1)   ", "Unique (1) (1)")]
        public void GivenAProjectWithAModel_WhenAddingANewModelWithTheSameName_ThenModelIsRenamed(string duplicateModelName, string expectedModelName)
        {
            // Setup
            using (DeltaShellApplication app = CreateApplicationWithModel(duplicateModelName))
            {
                var model = Substitute.For<IModel>();
                model.Name = duplicateModelName;

                // Call
                app.Project.RootFolder.Add(model);

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
            using (DeltaShellApplication app = CreateApplicationWithModel(originalName))
            {
                var model = Substitute.For<IModel>();
                model.Name = duplicateModelName;

                // Call
                app.Project.RootFolder.Add(model);

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
            using (DeltaShellApplication app = CreateApplicationWithModel("Unique"))
            {
                var parentModel = Substitute.For<IModel>();
                parentModel.Name = "parent_model";

                var childModel = Substitute.For<IModel>();
                childModel.Name = childModelName;

                parentModel.GetDirectChildren().Returns(new[]
                {
                    childModel
                });

                // Call
                app.Project.RootFolder.Add(parentModel);

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
            using (DeltaShellApplication app = CreateApplicationWithModel("Unique"))
            {
                // ModelBase implements INotifyPropertyChange
                var model = Substitute.ForPartsOf<ModelBase>();

                app.Project.RootFolder.Add(model);

                // Call
                model.Name = duplicateName;

                // Assert
                Assert.That(model.Name, Is.EqualTo("Unique (1)"),
                            "When project contains model with same name, model should be renamed.");
            }
        }

        private static DeltaShellApplication CreateApplicationWithModel(string modelName)
        {
            DeltaShellApplication application = CreateApplication();

            var model = Substitute.For<IModel>();
            model.Name = modelName;
            application.Project.RootFolder.Add(model);

            return application;
        }

        private static DeltaShellApplication CreateApplication()
        {
            var plugin = new HydroModelApplicationPlugin();
            var application = new DeltaShellApplication();
            plugin.Application = application;

            application.Run();

            application.CreateNewProject();
            return application;
        }
    }
}