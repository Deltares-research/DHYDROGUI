using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class HydroModelApplicationPluginTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void AdditionalOwnerCheckTest_HydroModel()
        {
            using (IApplication app = new DHYDROApplicationBuilder().WithHydroModel().Build())
            {
                app.Run();
                IProjectService projectService = GetProjectServiceWithProject(app);

                ModelInfo modelInfos = GetPlugin<HydroModelApplicationPlugin>(app).GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(projectService.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AdditionalOwnerCheckTest_RainfallRunoff()
        {
            using (IApplication app = new DHYDROApplicationBuilder().WithRainfallRunoff().Build())
            {
                app.Run();
                IProjectService projectService = GetProjectServiceWithProject(app);

                ModelInfo modelInfos = GetPlugin<RainfallRunoffApplicationPlugin>(app).GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(projectService.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AdditionalOwnerCheckTest_RealTimeControl()
        {
            using (IApplication app = new DHYDROApplicationBuilder().WithRealTimeControl().Build())
            {
                app.Run();
                IProjectService projectService = GetProjectServiceWithProject(app);

                ModelInfo modelInfos = GetPlugin<RealTimeControlApplicationPlugin>(app).GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(projectService.Project.RootFolder), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }


        [Test]
        [Category(TestCategory.Integration)]
        public void AdditionalOwnerCheckTest_FlowFM()
        {
            using (IApplication app = new DHYDROApplicationBuilder().WithFlowFM().Build())
            {
                app.Run();
                IProjectService projectService = GetProjectServiceWithProject(app);

                ModelInfo modelInfos = GetPlugin<FlowFMApplicationPlugin>(app).GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(projectService.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), true);
            }
        }

        [Test]
        public void Constructor_DefaultsCorrectlyInitialized()
        {
            var hydroModelApplicationPlugin = new HydroModelApplicationPlugin { Application = Substitute.For<IApplication>() };

            StringAssert.AreEqualIgnoringCase("Hydro Model",hydroModelApplicationPlugin.Name);
            StringAssert.AreEqualIgnoringCase("Hydro Model Plugin",hydroModelApplicationPlugin.DisplayName);
            StringAssert.AreEqualIgnoringCase(hydroModelApplicationPlugin.Description,"Provides functionality to create and run integrated models.");
            StringAssert.AreEqualIgnoringCase(hydroModelApplicationPlugin.FileFormatVersion, "1.1.1.0");

            List<ModelInfo> modelInfos = hydroModelApplicationPlugin.GetModelInfos().ToList();
            Assert.AreEqual(2, modelInfos.Count);

            ModelInfo first = modelInfos[0];
            StringAssert.AreEqualIgnoringCase(first.Name, "Empty Integrated Model");

            ModelInfo second = modelInfos[1];
            StringAssert.AreEqualIgnoringCase(second.Name, "1D-2D Integrated Model (RHU)");

            List<ProjectTemplate> projectTemplates = hydroModelApplicationPlugin.ProjectTemplates().ToList();
            Assert.AreEqual(2,projectTemplates.Count);

            ProjectTemplate firstProjectTemplate = projectTemplates[0];
            StringAssert.AreEqualIgnoringCase("Integrated model", firstProjectTemplate.Name);
            
            ProjectTemplate secondProjectTemplate = projectTemplates[1];
            StringAssert.AreEqualIgnoringCase("Dimr import", secondProjectTemplate.Name);
            
            Assert.AreEqual(1, hydroModelApplicationPlugin.GetFileExporters().ToList().Count);
            Assert.AreEqual(1, hydroModelApplicationPlugin.GetFileImporters().ToList().Count);
            Assert.IsInstanceOf<DHydroConfigXmlExporter>(hydroModelApplicationPlugin.GetFileExporters().First());
            Assert.IsInstanceOf<DHydroConfigXmlImporter>(hydroModelApplicationPlugin.GetFileImporters().First());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAnApplicationWithHydroModelPluginLoaded_WhenAHydroModelIsAdded_ThenTheRegisteredFileExportersShouldBeSet()
        {
            // Setup
            using (IApplication app = new DHYDROApplicationBuilder().WithHydroModel().Build())
            {
                app.Run();
                
                // Given
                var model = new HydroModel();

                IProjectService projectService = GetProjectServiceWithProject(app);

                // Call
                projectService.Project.RootFolder.Add(model);

                // Assert
                IFileExportService fileExportService = model.HydroModelExporter.FileExportService;
                Assert.That(fileExportService.FileExporters, Has.One.InstanceOf<DHydroConfigXmlExporter>());
            }
        }
        
        
        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAnApplicationWithHydroModelAndFlowFmPluginLoaded_WhenGettingFileImporters_ThenADimrImporterShouldBeReturnedThatCanImportOnWaterFlowFMModel()
        {
            // Given
            using (IApplication app = new DHYDROApplicationBuilder().WithFlowFM().WithHydroModel().Build())
            {
                app.Run();
                
                // When 
                IEnumerable<IFileImporter> applicationFileImporters = GetPlugin<HydroModelApplicationPlugin>(app).GetFileImporters().ToArray();
                int fileImportersCounter = applicationFileImporters.Count();

                // Then
                Assert.AreEqual(1, fileImportersCounter, $"Expected only 1 Dimr Importer, but {fileImportersCounter} importers were found");
                var dimrImporter = applicationFileImporters.First() as DHydroConfigXmlImporter;
                Assert.IsNotNull(dimrImporter, "The retrieved importer is not a Dimr Importer");
                Assert.IsTrue(dimrImporter.CanImportOn(new WaterFlowFMModel()), "The Dimr importer is missing the WaterFlowFMFileImporter");
            }
        }
        
        [Test]
        public void GivenHydroModelApplicationPlugin_HydroModelInitialize_ShouldLogPluginVersion()
        {
            //Arrange
            var plugin = new HydroModelApplicationPlugin();
            var app = Substitute.For<IApplication>();
            var activityRunner = Substitute.For<IActivityRunner>();

            var hydroModel = new HydroModel();

            app.ActivityRunner.Returns(activityRunner);

            plugin.Application = app;

            // Act & Assert
            var messages = TestHelper.GetAllRenderedMessages(() =>
            {
                activityRunner.ActivityStatusChanged += Raise.Event<EventHandler<ActivityStatusChangedEventArgs>>(hydroModel, new ActivityStatusChangedEventArgs(ActivityStatus.None, ActivityStatus.Initializing));
            });

            Assert.IsTrue(messages.Any(m => m.StartsWith("DeltaShell version")), "HydroModel plugin version should be logged");
        }
        
        private ProjectTemplate ConfigurePluginProjectTemplate()
        {
            // Setup
            var applicationPlugin = new HydroModelApplicationPlugin();
            // Call
            IEnumerable<ProjectTemplate> projectTemplates = applicationPlugin.ProjectTemplates();

            // Assert
            var projectTemplate = projectTemplates.FirstOrDefault(
                template => template.Id.Equals(HydroModelApplicationPlugin.RHUINTEGRATEDMODEL_TEMPLATE_ID));
            
            Assert.That(projectTemplate, Is.Not.Null);
            Assert.That(projectTemplate.ExecuteTemplateOpenView, Is.Not.Null);
            return projectTemplate;
        }

        [Test]
        public void ProjectTemplate_IfUseModelNameForProjectIsTrue_ProjectNameIsEqualToModelName()
        {
            string projectName = "hello-world-this-is-a-project-name";

            var project = new Project();
            var modelSettings = new HydroModelProjectTemplateSettings();
            var projectTemplate = ConfigurePluginProjectTemplate();
            project.Name = projectName;
            
            object returnValue = projectTemplate.ExecuteTemplateOpenView.Invoke(project, modelSettings);
            var model = returnValue as HydroModel;
            Assert.IsNotNull(model);

            Assert.AreEqual(modelSettings.ModelName,model.Name);
            Assert.AreEqual(model.Name, project.Name);
        }

        [Test]
        public void ProjectTemplate_IfUseModelNameForProjectIsFalse_ProjectNameIsNotChanged()
        {
            string projectName = "hello-world-this-is-a-project-name";

            var project = new Project();
            var modelSettings = new HydroModelProjectTemplateSettings();
            var projectTemplate = ConfigurePluginProjectTemplate();
            project.Name = projectName;

            modelSettings.UseModelNameForProject = false;
            
            object returnValue = projectTemplate.ExecuteTemplateOpenView.Invoke(project, modelSettings);
            var model = returnValue as HydroModel;
            Assert.IsNotNull(model);

            Assert.AreEqual(modelSettings.ModelName,model.Name);
            Assert.AreEqual(projectName, project.Name);
        }

        private static IProjectService GetProjectServiceWithProject(IApplication application)
        {
            IProjectService projectService = application.ProjectService;
            projectService.CreateProject();
            return projectService;
        }

        private static T GetPlugin<T>(IApplication application) where T : IPlugin
        {
            return application.Plugins.OfType<T>().Single();
        }
    }
}
