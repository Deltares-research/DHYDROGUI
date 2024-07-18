using System;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    /// <summary>
    /// Loads importer test also tests <see cref="NameablePointFeatureImporter{T}"/>.
    /// </summary>
    [TestFixture]
    public class LoadsImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportLoadsTest()
        {
            IEventedList<WaterQualityLoad> loads = new EventedList<WaterQualityLoad>();

            string path = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "dry_loads_shp_file", "sfb_testlocationsinput.shp");
            var importer = new LoadsImporter();
            importer.ImportItem(path, loads);

            Assert.AreEqual(3, loads.Count);
            Assert.AreEqual("Point 1", loads[0].Name);
            Assert.AreEqual("Point 2", loads[1].Name);
            Assert.AreEqual("Point 3", loads[2].Name);
            Assert.AreEqual("Factory", loads[0].LoadType);
            Assert.AreEqual("Factory", loads[1].LoadType);
            Assert.AreEqual("River", loads[2].LoadType);
            Assert.AreEqual("rhine_1", loads[0].LocationAliases);
            Assert.AreEqual("rhine_2", loads[1].LocationAliases);
            Assert.AreEqual("rhine_1, rhine_2", loads[2].LocationAliases);

            const double expectedXFromShapeFile = -122.19199999999999d;
            const double expectedYFromShapeFile = 37.567d;

            Assert.AreEqual(expectedXFromShapeFile, loads[0].X);
            Assert.AreEqual(expectedYFromShapeFile, loads[0].Y);
            Assert.AreEqual(double.NaN, loads[0].Z);

            loads.Clear();
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            importer.ModelCoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(28992); // RD new
            importer.ImportItem(path, loads);

            Assert.AreEqual(3, loads.Count);
            Assert.AreEqual("Point 1", loads[0].Name);
            Assert.AreEqual("Point 2", loads[1].Name);
            Assert.AreEqual("Point 3", loads[2].Name);
            Assert.AreNotEqual(expectedXFromShapeFile, loads[0].X);
            Assert.AreNotEqual(expectedYFromShapeFile, loads[0].Y);
            Assert.AreEqual(double.NaN, loads[0].Z);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportLoadWithZandLoadTypeAttributes()
        {
            IEventedList<WaterQualityLoad> loads = new EventedList<WaterQualityLoad>();

            string path = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "shape_files", "loads", "load.shp");
            var importer = new LoadsImporter();
            importer.ImportItem(path, loads);

            Assert.AreEqual(5, loads.Count);
            Assert.AreEqual("Load 1", loads[0].Name);
            Assert.AreEqual("Load 2", loads[1].Name);
            Assert.AreEqual("Load 3", loads[2].Name);
            Assert.AreEqual("Load 4", loads[3].Name);
            Assert.AreEqual("Load 5", loads[4].Name);

            Assert.AreEqual(0.5, loads[0].Z);
            Assert.AreEqual(0.5, loads[1].Z);
            Assert.AreEqual(0.0, loads[2].Z);
            Assert.AreEqual(0.0, loads[3].Z);
            Assert.AreEqual(0.0, loads[4].Z);

            Assert.AreEqual(string.Empty, loads[0].LoadType);
            Assert.AreEqual(string.Empty, loads[1].LoadType);
            Assert.AreEqual("Pipe (surface)", loads[2].LoadType);
            Assert.AreEqual("Pipe (surface)", loads[3].LoadType);
            Assert.AreEqual("Pipe (surface)", loads[4].LoadType);
        }

        [Test]
        public void ImportNonExistingFileTest()
        {
            IEventedList<WaterQualityLoad> loads = new EventedList<WaterQualityLoad>();

            string path = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "idontexist.shp");
            var importer = new LoadsImporter();
            importer.ImportItem(path, loads);

            Assert.AreEqual(0, loads.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ImporterGetsConfiguredByApplicationPlugin()
        {
            var app = Substitute.For<IApplication>();
            var project = Substitute.For<Project>();
            var runner = Substitute.For<IActivityRunner>();
            var modelService = Substitute.For<IModelService>();

            var projectService = Substitute.For<IProjectService>();
            projectService.Project.Returns(project);
            app.ProjectService.Returns(projectService);

            var loadsImporter = new LoadsImporter();
            var waqModel = new WaterQualityModel();
            var applicationPlugin = new WaterQualityModelApplicationPlugin();

            project.RootFolder.Add(waqModel);

            waqModel.Grid.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(3857);

            app.ModelService = modelService;
            app.ActivityRunner.Returns(runner);

            var fileImportActivity = new FileImportActivity(loadsImporter, waqModel.Loads);

            applicationPlugin.Application = app;

            projectService.Received(1).ProjectOpened += Arg.Any<EventHandler<EventArgs<Project>>>();
            projectService.Received(1).ProjectCreated += Arg.Any<EventHandler<EventArgs<Project>>>();
            projectService.Received(1).ProjectClosing += Arg.Any<EventHandler<EventArgs<Project>>>();
            projectService.Received(1).ProjectSaving += Arg.Any<EventHandler<EventArgs<Project>>>();
            projectService.Received(1).ProjectSaved += Arg.Any<EventHandler<EventArgs<Project>>>();

            runner.ActivityStatusChanged += Raise.EventWith(fileImportActivity,
                                                            new ActivityStatusChangedEventArgs(ActivityStatus.None, ActivityStatus.Initializing));

            Assert.AreEqual(loadsImporter.ModelCoordinateSystem, waqModel.Grid.CoordinateSystem);
            Assert.NotNull(loadsImporter.GetDefaultZValue);

            Assert.AreEqual(LayerType.Undefined, waqModel.LayerType);
            Assert.AreEqual(double.NaN, loadsImporter.GetDefaultZValue());
        }
    }
}