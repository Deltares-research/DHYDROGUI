using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Services;
using DelftTools.Utils;
using Deltares.Infrastructure.API.DependencyInjection;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.NHibernate;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;
using LifeCycle = Deltares.Infrastructure.API.DependencyInjection.LifeCycle;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class FlowFMApplicationPluginTest
    {
        private ProjectTemplate fmProjectTemplate;
        private FlowFMApplicationPlugin applicationPlugin;
        private WaterFlowFMModel model;
        private ModelSettings modelSettings;
        private Project project;

        [SetUp]
        public void SetUp()
        {
            applicationPlugin = new FlowFMApplicationPlugin();
            model = new WaterFlowFMModel();
            modelSettings = new ModelSettings();
            project = new Project();
            
            var folder = new Folder();
            project.RootFolder = folder;
            folder.Items.Add(model);
        }

        [SetUp]
        public void ProjectTemplates_ContainsWaterFlowFMModelTemplate()
        {
            // Call
            IEnumerable<ProjectTemplate> projectTemplates = applicationPlugin.ProjectTemplates();

            // Assert
            fmProjectTemplate = projectTemplates.FirstOrDefault(
                template => template.Id.EqualsCaseInsensitive("FMModel"));

            Assert.That(fmProjectTemplate, Is.Not.Null);
            Assert.That(fmProjectTemplate.ExecuteTemplateOpenView, Is.Not.Null);
        }

        [Test]
        public void ProjectTemplates_ContainsTemplateThatOpensDefaultViewForWaterFlowFMModel()
        {
            object returnValue = fmProjectTemplate.ExecuteTemplateOpenView.Invoke(project, modelSettings);
            Assert.That(returnValue, Is.TypeOf<WaterFlowFMModel>());
        }

        [Test]
        public void ModelSettings_UseModelNameForProject_DefaultIsTrue()
        {
            var settings = new ModelSettings();
            Assert.IsTrue(settings.UseModelNameForProject);
        }

        [Test]
        public void ProjectTemplate_IfUseModelNameForProjectIsTrue_ProjectNameIsSetToModelName()
        {
            project.Name = "";

            object returnValue = fmProjectTemplate.ExecuteTemplateOpenView.Invoke(project, modelSettings);
            var model = returnValue as WaterFlowFMModel;
            Assert.IsNotNull(model);

            Assert.AreEqual(modelSettings.ModelName, model.Name);
            Assert.AreEqual(model.Name, project.Name);
        }

        [Test]
        public void ProjectTemplate_IfUseModelNameForProjectIsFalse_ProjectNameIsNotChanged()
        {
            // Change the name of this project to show that the name is not changed rather than being set to default
            var projectName = "MyProject";
            project.Name = projectName;

            modelSettings.UseModelNameForProject = false;
            object returnValue = fmProjectTemplate.ExecuteTemplateOpenView.Invoke(project, modelSettings);
            var model = returnValue as WaterFlowFMModel;
            Assert.IsNotNull(model);

            Assert.AreEqual(modelSettings.ModelName, model.Name);
            Assert.AreEqual(projectName, project.Name);
        }

        [TestCase(FlowFMApplicationPlugin.FM_MODEL_DEFAULT_PROJECT_TEMPLATE_ID)]
        [TestCase(FlowFMApplicationPlugin.FM_MODEL_MDU_IMPORT_PROJECT_TEMPLATE_ID)]
        public void FMAppProjectTemplate_Expected(string projectTemplateName)
        {
            // Arrange
            applicationPlugin = new FlowFMApplicationPlugin();

            // Act
            IEnumerable<ProjectTemplate> projectTemplates = applicationPlugin.ProjectTemplates();

            // Assert
            Assert.That(projectTemplates, Has.One.With.Property(nameof(ProjectTemplate.Id)).EqualTo(projectTemplateName));
        }

        [Test]
        public void GetFileImporters_ContainsExpectedImporters()
        {
            // Call
            IFileImporter[] importers = applicationPlugin.GetFileImporters().ToArray();

            // Assert
            Assert.That(importers, Has.Length.EqualTo(39));
            ContainsImporter<LateralSourceImporter>(importers);
            ContainsImporter<WaterFlowFMFileImporter>(importers);
            ContainsImporter<WaterFlowFMIntoWaterFlowFMFileImporter>(importers);
            ContainsImporter<Area2DStructuresImporter>(importers);
            ContainsImporter<StructuresListImporter>(importers, 3);
            ContainsImporter<FMMapFileImporter>(importers);
            ContainsImporter<FMHisFileImporter>(importers);
            ContainsImporter<BcFileImporter>(importers);
            ContainsImporter<BcFile1DImporter>(importers);
            ContainsImporter<BcmFileImporter>(importers);
            ContainsImporter<GroupablePointCloudImporter>(importers);
            ContainsImporter<PliFileImporterExporter<Embankment, Embankment>>(importers);
            ContainsImporter<GisToFeature2DImporter<ILineString, Embankment>>(importers);
            ContainsImporter<PliFileImporterExporter<FixedWeir, FixedWeir>>(importers);
            ContainsImporter<GisToFeature2DImporter<ILineString, FixedWeir>>(importers);
            ContainsImporter<PlizFileImporterExporter<BridgePillar, BridgePillar>>(importers);
            ContainsImporter<PliFileImporterExporter<ThinDam2D, ThinDam2D>>(importers);
            ContainsImporter<GisToFeature2DImporter<ILineString, ThinDam2D>>(importers);
            ContainsImporter<PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>>(importers);
            ContainsImporter<GisToFeature2DImporter<IPoint, ObservationCrossSection2D>>(importers);
            ContainsImporter<PliFileImporterExporter<Weir2D, Weir2D>>(importers);
            ContainsImporter<PliFileImporterExporter<Pump2D, Pump2D>>(importers);
            ContainsImporter<PliFileImporterExporter<Gate2D, Gate2D>>(importers);
            ContainsImporter<PliFileImporterExporter<SourceAndSink, Feature2D>>(importers);
            ContainsImporter<PliFileImporterExporter<BoundaryConditionSet, Feature2D>>(importers);
            ContainsImporter<PointFileImporterExporter>(importers);
            ContainsImporter<ObsFileImporterExporter<GroupableFeature2DPoint>>(importers);
            ContainsImporter<PolFileImporterExporter>(importers);
            ContainsImporter<LdbFileImporterExporter>(importers);
            ContainsImporter<GisToFeature2DImporter<ILineString, LandBoundary2D>>(importers);
            ContainsImporter<FlowFMNetFileImporter>(importers);
            ContainsImporter<TimFileImporter>(importers, 2);
            ContainsImporter<PliFileImporterExporter<Feature2D, Feature2D>>(importers);
            ContainsImporter<GisToFeature2DImporter<ILineString, Feature2D>>(importers);
            ContainsImporter<GisToFeature2DImporter<IPoint, Gully>>(importers);
            ContainsImporter<GisToFeature2DImporter<IPolygon, GroupableFeature2DPolygon>>(importers);
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
            IFileExporter[] exporters = applicationPlugin.GetFileExporters().ToArray();

            // Assert
            AssertContainsExpectedFileExporters(exporters);
        }

        private void AssertContainsExpectedFileExporters(IFileExporter[] exporters)
        {
            ContainsExporter<FMModelFileExporter>(exporters);
            ContainsExporter<Area2DStructuresExporter>(exporters);
            ContainsExporter<StructuresListExporter>(exporters, 3);
            ContainsExporter<BcFileExporter>(exporters);
            ContainsExporter<BcmFileExporter>(exporters);
            ContainsExporter<PliFileImporterExporter<Embankment, Embankment>>(exporters);
            ContainsExporter<PlizFileImporterExporter<BridgePillar, BridgePillar>>(exporters);
            ContainsExporter<PliFileImporterExporter<ThinDam2D, ThinDam2D>>(exporters);
            ContainsExporter<PliFileImporterExporter<ObservationCrossSection2D, ObservationCrossSection2D>>(exporters);
            ContainsExporter<PliFileImporterExporter<IWeir, Feature2D>>(exporters);
            ContainsExporter<PliFileImporterExporter<IPump, Feature2D>>(exporters);
            ContainsExporter<PliFileImporterExporter<IGate, Feature2D>>(exporters);
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
            IApplication application = GetApplication(project);
            applicationPlugin.Application = application;

            application.FileExportService.FileExporters.Returns(applicationPlugin.GetFileExporters());
            application.ProjectService.ProjectOpened += Raise.EventWith(this, new EventArgs<Project>(project));

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
            IApplication application = GetApplication(project);

            application.FileExportService.FileExporters.Returns(applicationPlugin.GetFileExporters());
            applicationPlugin.Application = application;

            // Call
            application.ProjectService.ProjectOpened += Raise.EventWith(this, new EventArgs<Project>(project));

            // Assert
            IFileExportService fileExportService = model.DimrRunner.FileExportService;
            AssertContainsExpectedFileExporters(fileExportService.FileExporters.ToArray());
        }

        [Test]
        public void AddRegistrations_RegistersServicesCorrectly()
        {
            var container = Substitute.For<IDependencyInjectionContainer>();

            applicationPlugin.AddRegistrations(container);

            container.Received(1).Register<IDataAccessListenersProvider, FlowFMDataAccessListenersProvider>(LifeCycle.Transient);
        }

        private static IApplication GetApplication(Project project)
        {
            var application = Substitute.For<IApplication>();
            application.ProjectService.Project.Returns(project);
            application.ProjectService.IsProjectOpen.Returns(true);

            return application;
        }
    }
}