using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Services;
using DelftTools.TestUtils;
using DelftTools.Utils;
using Deltares.Infrastructure.API.DependencyInjection;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl.NHibernate;
using NSubstitute;
using NUnit.Framework;
using LifeCycle = Deltares.Infrastructure.API.DependencyInjection.LifeCycle;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlApplicationPluginTest
    {
        private RealTimeControlApplicationPlugin plugin;

        [SetUp]
        public void SetUp()
        {
            plugin = new RealTimeControlApplicationPlugin();
        }

        [Test]
        public void FileFormatVersion_ShouldReturnCurrentVersionNumber()
        {
            Assert.AreEqual("3.8.0.0", plugin.FileFormatVersion);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsCompositeActivity_ThenHelperMethodReturnsCompositeActivityAndThisWillBeUsed()
        {
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNotNull<RealTimeControlApplicationPlugin>(
                b => b.WithRealTimeControl());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsNull_ThenHelperMethodReturnsNullAndRootFolderWillBeUsed()
        {
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNull<RealTimeControlApplicationPlugin>(
                b => b.WithRealTimeControl());
        }

        [Test]
        [TestCase(typeof(RealTimeControlModelExporter))]
        [TestCase(typeof(RealTimeControlRestartFileExporter))]
        public void GetFileExporters_ContainsExpectedExporter(Type exporterType)
        {
            // Call
            IEnumerable<IFileExporter> exporters = plugin.GetFileExporters();

            // Assert
            Assert.NotNull(exporters.SingleOrDefault(e => e.GetType() == exporterType));
        }

        [Test]
        [TestCase(typeof(RealTimeControlModelImporter))]
        [TestCase(typeof(RealTimeControlRestartFileImporter))]
        public void GetFileImporters_ContainsExpectedImporter(Type importerType)
        {
            // Call
            IEnumerable<IFileImporter> importers = plugin.GetFileImporters();

            // Assert
            Assert.NotNull(importers.SingleOrDefault(e => e.GetType() == importerType));
        }

        [Test]
        public void OnProjectCollectionChangingEventIsRaised_FileExportersIsSetOnDimrRunner()
        {
            // Setup
            var model = new RealTimeControlModel();
            var project = new Project();

            IApplication application = GetApplication(project);
            plugin.Application = application;

            application.FileExportService.FileExporters.Returns(plugin.GetFileExporters());
            application.ProjectService.ProjectOpened += Raise.EventWith(this, new EventArgs<Project>(project));

            // Call
            project.RootFolder.Add(model);

            // Assert
            IFileExportService fileExportService = model.DimrRunner.FileExportService;
            Assert.That(fileExportService.FileExporters, Has.One.InstanceOf<RealTimeControlModelExporter>().And
                                                            .One.InstanceOf<RealTimeControlRestartFileExporter>());
        }

        [Test]
        public void OnProjectOpenedEventIsRaised_FileExportersIsSetOnDimrRunner()
        {
            // Setup
            var model = new RealTimeControlModel();

            Project project = GetProject(model);
            IApplication application = GetApplication(project);

            application.FileExportService.FileExporters.Returns(plugin.GetFileExporters());
            plugin.Application = application;

            // Call
            application.ProjectService.ProjectOpened += Raise.EventWith(this, new EventArgs<Project>(project));

            // Assert
            IFileExportService fileExportService = model.DimrRunner.FileExportService;
            Assert.That(fileExportService.FileExporters, Has.One.InstanceOf<RealTimeControlModelExporter>().And
                                                            .One.InstanceOf<RealTimeControlRestartFileExporter>());
        }

        [Test]
        public void AddRegistrations_RegistersServicesCorrectly()
        {
            var container = Substitute.For<IDependencyInjectionContainer>();

            plugin.AddRegistrations(container);

            container.Received(1).Register<IDataAccessListenersProvider, RealTimeControlDataAccessListenersProvider>(LifeCycle.Transient);
        }

        private static IApplication GetApplication(Project project)
        {
            var application = Substitute.For<IApplication>();
            application.ProjectService.Project.Returns(project);

            return application;
        }

        private static Project GetProject(RealTimeControlModel model)
        {
            var project = new Project();
            var folder = new Folder();
            project.RootFolder = folder;
            folder.Items.Add(model);

            return project;
        }
    }
}