using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.Toolbox;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class HydroModelApplicationPluginTest
    {
        private static IApplication CreateApplication(ApplicationPlugin appPlugin)
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new CommonToolsApplicationPlugin(),
                new NHibernateDaoApplicationPlugin(),
                new NetCdfApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new ScriptingApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new ToolboxApplicationPlugin(),
                appPlugin
            };
            var application = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
            application.Run();
            application.CreateNewProject();
            return application;
        }

        [Test]
        public void AdditionalOwnerCheckTest_HydroModel()
        {
            var appPlugin = new HydroModelApplicationPlugin();
            using (var app = CreateApplication(appPlugin))
            {
                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_RainfallRunoff()
        {
            var appPlugin = new RainfallRunoffApplicationPlugin();
            using (var app = CreateApplication(appPlugin))
            {
                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_RealTimeControl()
        {
            var appPlugin = new RealTimeControlApplicationPlugin();
            using (var app = CreateApplication(appPlugin))
            {
                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }


        [Test]
        public void AdditionalOwnerCheckTest_FlowFM()
        {
            var appPlugin = new FlowFMApplicationPlugin();
            using (var app = CreateApplication(appPlugin))
            {
                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), true);
            }
        }

        [Test]
        public void Constructor_DefaultsCorrectlyInitialized()
        {
            var hydroModelApplicationPlugin = new HydroModelApplicationPlugin();
            
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
    }
}
