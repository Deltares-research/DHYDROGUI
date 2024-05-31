using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Services;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class FlowFMApplicationPluginTest
    {
        private FlowFMApplicationPlugin plugin;

        [SetUp]
        public void SetUp()
        {
            plugin = new FlowFMApplicationPlugin();
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
        public void GetFileImporters_ContainsExpectedImporters()
        {
            // Call
            IFileImporter[] importers = plugin.GetFileImporters().ToArray();

            // Assert
            Assert.That(importers, Has.Length.EqualTo(36));
            ContainsImporter<WaterFlowFMFileImporter>(importers);
            ContainsImporter<Area2DStructuresImporter>(importers);
            ContainsImporter<StructuresListImporter>(importers, 2);
            ContainsImporter<FMMapFileImporter>(importers);
            ContainsImporter<FMHisFileImporter>(importers);
            ContainsImporter<FMRestartFileImporter>(importers);
            ContainsImporter<BcFileImporter>(importers);
            ContainsImporter<BcmFileImporter>(importers);
            ContainsImporter<GroupablePointCloudImporter>(importers);
            ContainsImporter<PlizFileImporterExporter<FixedWeir, FixedWeir>>(importers);
            ContainsImporter<PlizFileImporterExporter<BridgePillar, BridgePillar>>(importers);
            ContainsImporter<PliFileImporterExporter<ThinDam2D, ThinDam2D>>(importers);
            ContainsImporter<PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>>(importers);
            ContainsImporter<PliFileImporterExporter<Structure, Structure>>(importers);
            ContainsImporter<PliFileImporterExporter<Pump, Pump>>(importers);
            ContainsImporter<PliFileImporterExporter<SourceAndSink, Feature2D>>(importers);
            ContainsImporter<PliFileImporterExporter<BoundaryConditionSet, Feature2D>>(importers);
            ContainsImporter<PointFileImporterExporter>(importers);
            ContainsImporter<ObsFileImporterExporter<GroupableFeature2DPoint>>(importers);
            ContainsImporter<PolFileImporterExporter>(importers);
            ContainsImporter<LdbFileImporterExporter>(importers);
            ContainsImporter<FlowFMNetFileImporter>(importers);
            ContainsImporter<TimFileImporter>(importers, 2);
            ContainsImporter<ShapeFileImporter<ILineString, LandBoundary2D>>(importers);
            ContainsImporter<ShapeFileImporter<IPoint, GroupablePointFeature>>(importers);
            ContainsImporter<ShapeFileImporter<IPolygon, GroupableFeature2DPolygon>>(importers);
            ContainsImporter<ShapeFileImporter<ILineString, ThinDam2D>>(importers);
            ContainsImporter<ShapeFileImporter<ILineString, FixedWeir>>(importers);
            ContainsImporter<ShapeFileImporter<IPoint, GroupableFeature2DPoint>>(importers);
            ContainsImporter<ShapeFileImporter<ILineString, ObservationCrossSection2D>>(importers);
            ContainsImporter<ShapeFileImporter<ILineString, BridgePillar>>(importers);
            ContainsImporter<ShapeFileImporter<ILineString, Pump>>(importers);
            ContainsImporter<ShapeFileImporter<ILineString, Structure>>(importers);
            ContainsImporter<SamplesImporter>(importers);
        }

        private static void ContainsImporter<T>(IFileImporter[] source, int n = 1)
        {
            Assert.That(source.OfType<T>().ToList(), Has.Count.EqualTo(n),
                        $"Collection should contain {n} of {typeof(T).Name}");
        }

        [Test]
        public void GetFileExporters_ContainsExpectedExporters()
        {
            // Call
            IFileExporter[] exporters = plugin.GetFileExporters().ToArray();

            // Assert
            AssertContainsExpectedFileExporters(exporters);
        }

        private void AssertContainsExpectedFileExporters(IFileExporter[] exporters)
        {
            ContainsExporter<FMModelFileExporter>(exporters);
            ContainsExporter<Area2DStructuresExporter>(exporters);
            ContainsExporter<StructuresListExporter>(exporters, 2);
            ContainsExporter<BcFileExporter>(exporters);
            ContainsExporter<BcmFileExporter>(exporters);
            ContainsExporter<SamplesExporter>(exporters);
            ContainsExporter<PlizFileImporterExporter<FixedWeir, FixedWeir>>(exporters);
            ContainsExporter<PlizFileImporterExporter<BridgePillar, BridgePillar>>(exporters);
            ContainsExporter<PliFileImporterExporter<ThinDam2D, ThinDam2D>>(exporters);
            ContainsExporter<PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>>(exporters);
            ContainsExporter<PliFileImporterExporter<Structure, Structure>>(exporters);
            ContainsExporter<PliFileImporterExporter<Pump, Pump>>(exporters);
            ContainsExporter<PliFileImporterExporter<SourceAndSink, Feature2D>>(exporters);
            ContainsExporter<PliFileImporterExporter<BoundaryConditionSet, Feature2D>>(exporters);
            ContainsExporter<PointFileImporterExporter>(exporters);
            ContainsExporter<ObsFileImporterExporter<GroupableFeature2DPoint>>(exporters);
            ContainsExporter<PolFileImporterExporter>(exporters);
            ContainsExporter<LdbFileImporterExporter>(exporters);
            ContainsExporter<FlowFMNetFileExporter>(exporters);
            ContainsExporter<FMModelPartitionExporter>(exporters);
            ContainsExporter<FMGridPartitionExporter>(exporters);
            ContainsExporter<GeometryZipExporter>(exporters);
            ContainsExporter<TimFileExporter>(exporters);
            ContainsExporter<ImportedNetFileXyzExporter>(exporters, 2);
            ContainsExporter<WindItemExporter>(exporters, 6);
        }

        private static void ContainsExporter<T>(IFileExporter[] source, int n = 1)
        {
            Assert.That(source.OfType<T>().ToList(), Has.Count.EqualTo(n),
                        $"Collection should contain {n} of {typeof(T).Name}");
        }
        
                
        [Test]
        public void OnProjectCollectionChangingEventIsRaised_FileExportersIsSetOnDimrRunner()
        {
            // Setup
            var model = new WaterFlowFMModel();
            var project = new Project();

            IApplication application = GetApplication(project);
            plugin.Application = application;

            application.FileExportService.FileExporters.Returns(plugin.GetFileExporters());
            application.ProjectOpened += Raise.Event<Action<Project>>(project);

            // Call
            project.RootFolder.Add(model);

            // Assert
            IFileExportService fileExportService = model.DimrRunner.FileExportService;
            AssertContainsExpectedFileExporters(fileExportService.FileExporters.ToArray());
        }

        [Test]
        public void OnProjectOpenedEventIsRaised_FileExportersIsSetOnDimrRunner()
        {
            // Setup
            var model = new WaterFlowFMModel();

            Project project = GetProject(model);
            IApplication application = GetApplication(project);

            application.FileExportService.FileExporters.Returns(plugin.GetFileExporters());
            plugin.Application = application;

            // Call
            application.ProjectOpened += Raise.Event<Action<Project>>(project);

            // Assert
            IFileExportService fileExportService = model.DimrRunner.FileExportService;
            AssertContainsExpectedFileExporters(fileExportService.FileExporters.ToArray());
        }
        
        private static IApplication GetApplication(Project project)
        {
            var application = Substitute.For<IApplication>();
            application.Project.Returns(project);
            application.GetAllModelsInProject().Returns(project.RootFolder.GetAllModelsRecursive());

            return application;
        }

        private static Project GetProject(WaterFlowFMModel model)
        {
            var project = new Project();
            var folder = new Folder();
            project.RootFolder = folder;
            folder.Items.Add(model);

            return project;
        }
    }
}