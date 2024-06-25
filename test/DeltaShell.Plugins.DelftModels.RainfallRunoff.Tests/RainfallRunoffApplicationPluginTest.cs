using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.DependencyInjection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.NHibernate;
using NSubstitute;
using NUnit.Framework;
using LifeCycle = Deltares.Infrastructure.API.DependencyInjection.LifeCycle;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffApplicationPluginTest
    {
        private RainfallRunoffApplicationPlugin plugin;

        [SetUp]
        public void SetUp()
        {
            plugin = new RainfallRunoffApplicationPlugin();
        }
        
        [Test]
        public void GivenRainfallRunoffApplicationPlugin_RainfallRunoffModelInitialize_ShouldLogPluginVersion()
        {
            //Arrange
            var app = Substitute.For<IApplication>();
            var activityRunner = Substitute.For<IActivityRunner>();

            var rainfallRunoffModel = new RainfallRunoffModel();

            app.ActivityRunner.Returns(activityRunner);

            plugin.Application = app;

            // Act & Assert
            var messages = TestHelper.GetAllRenderedMessages(() =>
            {
                activityRunner.ActivityStatusChanged += Raise.Event<EventHandler<ActivityStatusChangedEventArgs>>(rainfallRunoffModel, new ActivityStatusChangedEventArgs(ActivityStatus.None, ActivityStatus.Initializing));
            });

            Assert.IsTrue(messages.Any(m => m.StartsWith("DeltaShell version")), "RainfallRunoffModel plugin version should be logged");
        }
        
        [Test]
        public void GetFileExporters_ContainsExpectedExporters()
        {
            // Call
            IEnumerable<IFileExporter> exporters = plugin.GetFileExporters();

            // Assert
            Assert.That(exporters, Has.One.InstanceOf<MeteoDataExporter>().And
                                      .One.InstanceOf<RainfallRunoffModelExporter>());
        }

        [Test]
        public void GetFileImporters_ContainsExpectedImporters()
        {
            // Call
            IEnumerable<IFileImporter> exporters = plugin.GetFileImporters();

            // Assert
            Assert.That(exporters, Has.One.InstanceOf<RainfallRunoffModelImporter>().And
                                      .One.InstanceOf<MeteoDataImporter>().And
                                      .One.InstanceOf<NWRWCatchmentFrom3BImporter>());
        }

        [Test]
        public void OnProjectCollectionChangingEventIsRaised_FileExportersIsSetOnDimrRunner()
        {
            // Setup
            var model = new RainfallRunoffModel();
            var project = new Project();

            IApplication application = GetApplication(project);
            plugin.Application = application;

            application.FileExportService.FileExporters.Returns(plugin.GetFileExporters());
            application.ProjectOpened += Raise.Event<Action<Project>>(project);

            // Call
            project.RootFolder.Add(model);

            // Assert
            IFileExportService fileExportService = model.DimrRunner.FileExportService;
            Assert.That(fileExportService.FileExporters, Has.One.InstanceOf<MeteoDataExporter>().And
                                                            .One.InstanceOf<RainfallRunoffModelExporter>());
        }

        [Test]
        public void OnProjectOpenedEventIsRaised_FileExportersIsSetOnDimrRunner()
        {
            // Setup
            var model = new RainfallRunoffModel();

            Project project = GetProject(model);
            IApplication application = GetApplication(project);

            application.FileExportService.FileExporters.Returns(plugin.GetFileExporters());
            plugin.Application = application;

            // Call
            application.ProjectOpened += Raise.Event<Action<Project>>(project);

            // Assert
            IFileExportService fileExportService = model.DimrRunner.FileExportService;
            Assert.That(fileExportService.FileExporters, Has.One.InstanceOf<MeteoDataExporter>().And
                                                            .One.InstanceOf<RainfallRunoffModelExporter>());
        }

        [Test]
        public void AddRegistrations_RegistersServicesCorrectly()
        {
            var container = Substitute.For<IDependencyInjectionContainer>();

            plugin.AddRegistrations(container);

            container.Received(1).Register<IDataAccessListenersProvider, RainfallRunoffDataAccessListenersProvider>(LifeCycle.Transient);
        }

        private static IApplication GetApplication(Project project)
        {
            var application = Substitute.For<IApplication>();
            application.Project.Returns(project);
            application.GetAllModelsInProject().Returns(project.RootFolder.GetAllModelsRecursive());

            return application;
        }

        private static Project GetProject(RainfallRunoffModel model)
        {
            var project = new Project();
            var folder = new Folder();
            project.RootFolder = folder;
            folder.Items.Add(model);

            return project;
        }
    }
}