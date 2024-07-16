using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils;
using Deltares.Infrastructure.API.DependencyInjection;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Exporters;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using DeltaShell.Plugins.FMSuite.Wave.NHibernate;
using NSubstitute;
using NUnit.Framework;
using LifeCycle = Deltares.Infrastructure.API.DependencyInjection.LifeCycle;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveApplicationPluginTest
    {
        private WaveApplicationPlugin plugin;

        [SetUp]
        public void SetUp()
        {
            plugin = new WaveApplicationPlugin();
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
        public void GetModelInfos_ReturnsCorrectCollection()
        {
            // Setup
            var application = Substitute.For<IApplication>();
            plugin.Application = application;

            // Call
            List<ModelInfo> modelInfos = plugin.GetModelInfos().ToList();

            // Assert
            Assert.That(modelInfos, Has.Count.EqualTo(1));

            ModelInfo modelInfo = modelInfos.First();
            Assert.That(modelInfo.Name, Is.EqualTo("Waves Model"));
            Assert.That(modelInfo.Category, Is.EqualTo("1D / 2D / 3D Standalone Models"));
            Assert.That(modelInfo.Image, Is.Not.Null);
            Assert.That(modelInfo.GetParentProjectItem, Is.Not.Null);
            Assert.That(modelInfo.AdditionalOwnerCheck, Is.Not.Null);

            Func<IProjectItem, IModel> createModel = modelInfo.CreateModel;
            Assert.That(createModel, Is.Not.Null);
            var createdModel = createModel(null) as WaveModel;
            Assert.That(createdModel, Is.Not.Null);
            Func<string> workingDirPathFunc = createdModel.WorkingDirectoryPathFunc;
            Assert.That(workingDirPathFunc, Is.Not.Null);

            application.WorkDirectory.Returns("original_working_directory");
            Assert.That(workingDirPathFunc(), Is.EqualTo("original_working_directory"));

            application.WorkDirectory.Returns("changed_working_directory");
            Assert.That(workingDirPathFunc(), Is.EqualTo("changed_working_directory"));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SetApplication_SubscribesApplication()
        {
            // Setup
            var model = new WaveModel();
            Project project = GetProject(model);
            IApplication application = GetApplication(project, "application_working_directory");

            // Call
            plugin.Application = application;

            // Assert
            application.ProjectService.Received(1).ProjectOpened += Arg.Any<EventHandler<EventArgs<Project>>>();
            application.ProjectService.DidNotReceiveWithAnyArgs().ProjectOpened -= Arg.Any<EventHandler<EventArgs<Project>>>();

            Assert.That(plugin.Application, Is.SameAs(application));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenWaveApplicationPluginWithAnApplication_WhenProjectOpenedEventIsRaised_WorkingDirectoryPathFuncIsSetOnWaveModel()
        {
            // Setup
            var model = new WaveModel();
            
            Project project = GetProject(model);
            IApplication application = GetApplication(project, "application_working_directory");

            string defaultWorkingDir = model.WorkingDirectoryPathFunc();

            plugin.Application = application;

            // Call
            application.ProjectService.ProjectOpened += Raise.EventWith(this, new EventArgs<Project>(project));

            // Assert
            string appWorkingDir = model.WorkingDirectoryPathFunc();
            Assert.That(appWorkingDir, Is.Not.EqualTo(defaultWorkingDir));
            Assert.That(appWorkingDir, Is.EqualTo("application_working_directory"));
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void GivenWaveApplicationPluginWithAnApplication_WhenProjectCollectionChangingEventIsRaised_FileExportersIsSetOnDimrRunner()
        {
            // Setup
            var project = new Project();
            var model = new WaveModel();

            IApplication application = GetApplication(project, "application_working_directory");
            plugin.Application = application;

            application.FileExportService.FileExporters.Returns(plugin.GetFileExporters());
            application.ProjectService.ProjectOpened += Raise.EventWith(this, new EventArgs<Project>(project));

            // Call
            project.RootFolder.Add(model);

            // Assert
            IFileExportService fileExportService = model.DimrRunner.FileExportService;
            Assert.That(fileExportService.FileExporters, Has.One.InstanceOf<WaveModelFileExporter>().And
                                                            .One.InstanceOf<Delft3DDepthFileExporter>().And
                                                            .One.InstanceOf<Delft3DGridFileExporter>());
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void GivenWaveApplicationPluginWithAnApplication_WhenProjectOpenedEventIsRaised_FileExportersIsSetOnDimrRunner()
        {
            // Setup
            var model = new WaveModel();
            
            Project project = GetProject(model);
            IApplication application = GetApplication(project, "application_working_directory");
            
            application.FileExportService.FileExporters.Returns(plugin.GetFileExporters());
            plugin.Application = application;

            // Call
            application.ProjectService.ProjectOpened += Raise.EventWith(this, new EventArgs<Project>(project));

            // Assert
            IFileExportService fileExportService = model.DimrRunner.FileExportService;
            Assert.That(fileExportService.FileExporters, Has.One.InstanceOf<WaveModelFileExporter>().And
                                                            .One.InstanceOf<Delft3DDepthFileExporter>().And
                                                            .One.InstanceOf<Delft3DGridFileExporter>());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SetApplication_UnsubscribesOriginalApplication()
        {
            // Setup
            var model = new WaveModel();
            Project project = GetProject(model);
            IApplication application1 = GetApplication(project, "application1_working_directory");
            IApplication application2 = GetApplication(GetProject(new WaveModel()), "application2_working_directory");

            plugin.Application = application1;
            application1.ProjectService.ClearReceivedCalls();

            // Calls
            plugin.Application = application2;

            // Assert
            Assert.That(plugin.Application, Is.SameAs(application2));

            application1.ProjectService.DidNotReceiveWithAnyArgs().ProjectOpened += Arg.Any<EventHandler<EventArgs<Project>>>();
            application1.ProjectService.Received(1).ProjectOpened -= Arg.Any<EventHandler<EventArgs<Project>>>();
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenWaveApplicationPluginWithAnApplication_WhenApplicationIsSetWithNewApplication_AndProjectOpenedEventIsRaised_WorkingDirectoryPathFuncNotSetOnOriginalWaveModel()
        {
            // Setup
            var model = new WaveModel();
            Project project = GetProject(model);
            IApplication application1 = GetApplication(project, "application1_working_directory");
            IApplication application2 = GetApplication(GetProject(new WaveModel()), "application2_working_directory");

            string defaultWorkingDir = model.WorkingDirectoryPathFunc();

            plugin.Application = application1;
            plugin.Application = application2;

            // Call
            application1.ProjectService.ProjectOpened += Raise.EventWith(this, new EventArgs<Project>(project));

            // Assert
            Assert.That(model.WorkingDirectoryPathFunc(), Is.EqualTo(defaultWorkingDir));
        }

        [Test]
        public void GetFileImporters_ReturnsCorrectCollection()
        {
            // Call
            List<IFileImporter> importers = plugin.GetFileImporters().ToList();

            // Assert
            Assert.That(importers, Has.Count.EqualTo(5));
            Contains<WaveModelFileImporter>(importers);
            Contains<WaveGridFileImporter>(importers);
            Contains<WaveDepthFileImporter>(importers);
            Contains<WaveBoundaryFileImporter>(importers);
            Contains<WavmFileImporter>(importers);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetFileImporters_ReturnsCorrectWaveModelFileImporter()
        {
            // Given
            IApplication application = GetApplication(new Project(), "application_working_directory");

            plugin.Application = application;

            // When
            IEnumerable<IFileImporter> importers = plugin.GetFileImporters();

            // Then
            WaveModelFileImporter importer = importers.OfType<WaveModelFileImporter>().FirstOrDefault();
            Assert.That(importer, Is.Not.Null);

            AssertCorrectWaveModelFileImporter(importer, "application_working_directory");
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Assert
            Assert.That(plugin.Name, Is.EqualTo("Delft3D Wave"));
            Assert.That(plugin.DisplayName, Is.EqualTo("D-Waves Plugin"));
            Assert.That(plugin.Description, Is.EqualTo("A 2D/3D Wave module"));
            Assert.That(plugin.Version, Is.Not.Null.Or.Empty);
            Assert.That(plugin.FileFormatVersion, Is.EqualTo("1.3.0.0"));
        }

        [Test]
        public void AddRegistrations_RegistersServicesCorrectly()
        {
            var container = Substitute.For<IDependencyInjectionContainer>();

            plugin.AddRegistrations(container);

            container.Received(1).Register<IDataAccessListenersProvider, WaveDataAccessListenersProvider>(LifeCycle.Transient);
        }

        private static void AssertCorrectWaveModelFileImporter(WaveModelFileImporter importer, string expectedWorkingDirectory)
        {
            string testDataPath = TestHelper.GetTestFilePath("WaveModelSaveLoadTest");
            using (var temp = new TemporaryDirectory())
            {
                string localDataPath = temp.CopyDirectoryToTempDirectory(testDataPath);

                var model = (WaveModel) importer.ImportItem(Path.Combine(localDataPath, "input", "Waves.mdw"));

                Assert.That(model.WorkingDirectoryPathFunc, Is.Not.Null);
                Assert.That(model.WorkingDirectoryPathFunc(), Is.EqualTo(expectedWorkingDirectory));
            }
        }

        private static IApplication GetApplication(Project project, string workingDir)
        {
            var application = Substitute.For<IApplication>();
            application.Project.Returns(project);
            application.GetAllModelsInProject().Returns(project.RootFolder.GetAllModelsRecursive());
            application.WorkDirectory.Returns(workingDir);

            return application;
        }

        private static Project GetProject(WaveModel model)
        {
            var project = new Project();
            var folder = new Folder();
            project.RootFolder = folder;
            folder.Items.Add(model);

            return project;
        }

        private static void Contains<T>(IEnumerable source)
        {
            List<T> items = source.OfType<T>().ToList();
            Assert.That(items, Has.Count.EqualTo(1), $"Collection should contain one {typeof(T).Name}");
        }
    }
}