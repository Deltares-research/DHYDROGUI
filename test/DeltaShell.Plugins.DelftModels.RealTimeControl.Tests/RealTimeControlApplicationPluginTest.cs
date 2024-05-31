using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Services;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlApplicationPluginTest
    {
        [Test]
        public void FileFormatVersion_ShouldReturnCurrentVersionNumber()
        {
            var applicationPlugin = new RealTimeControlApplicationPlugin();
            Assert.AreEqual("3.8.0.0", applicationPlugin.FileFormatVersion);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsCompositeActivity_ThenHelperMethodReturnsCompositeActivityAndThisWillBeUsed()
        {
            var realTimeControlApplicationPlugin = new RealTimeControlApplicationPlugin();
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNotNull(realTimeControlApplicationPlugin);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetParentProjectItem_WhenSelectionIsNull_ThenHelperMethodReturnsNullAndRootFolderWillBeUsed()
        {
            var realTimeControlApplicationPlugin = new RealTimeControlApplicationPlugin();
            ApplicationPluginTestHelper.TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNull(realTimeControlApplicationPlugin);
        }

        [Test]
        [TestCase(typeof(RealTimeControlModelExporter))]
        [TestCase(typeof(RealTimeControlRestartFileExporter))]
        public void GetFileExporters_ContainsExpectedExporter(Type exporterType)
        {
            // Setup
            var plugin = new RealTimeControlApplicationPlugin();

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
            // Setup
            var plugin = new RealTimeControlApplicationPlugin();

            // Call
            IEnumerable<IFileImporter> importers = plugin.GetFileImporters();

            // Assert
            Assert.NotNull(importers.SingleOrDefault(e => e.GetType() == importerType));
        }

        [Test]
        public void OnProjectCollectionChangingEventIsRaised_FileExportersIsSetOnDimrRunner()
        {
            // Setup
            var plugin = new RealTimeControlApplicationPlugin();
            var model = new RealTimeControlModel();
            var project = new Project();

            IApplication application = GetApplication(project);
            plugin.Application = application;

            application.FileExportService.FileExporters.Returns(plugin.GetFileExporters());
            application.ProjectOpened += Raise.Event<Action<Project>>(project);

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
            var plugin = new RealTimeControlApplicationPlugin();
            var model = new RealTimeControlModel();

            Project project = GetProject(model);
            IApplication application = GetApplication(project);

            application.FileExportService.FileExporters.Returns(plugin.GetFileExporters());
            plugin.Application = application;

            // Call
            application.ProjectOpened += Raise.Event<Action<Project>>(project);

            // Assert
            IFileExportService fileExportService = model.DimrRunner.FileExportService;
            Assert.That(fileExportService.FileExporters, Has.One.InstanceOf<RealTimeControlModelExporter>().And
                                                            .One.InstanceOf<RealTimeControlRestartFileExporter>());
        }

        private static IApplication GetApplication(Project project)
        {
            var application = Substitute.For<IApplication>();
            application.Project.Returns(project);
            application.GetAllModelsInProject().Returns(project.RootFolder.GetAllModelsRecursive());

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