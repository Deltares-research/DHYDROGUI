using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers
{
    [TestFixture]
    [Parallelizable(ParallelScope.None), Apartment(ApartmentState.STA)]
    public class EstimatedSnappedFeaturesIntegrationTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllAvailableSnappedFeaturesLayers()
        {
            const int expectedNumberOfLayers = 17;

            using (var gui = CreateGui())
            {
                gui.Run();

                WaterFlowFMModel fmModel = AddFMModelToProject(gui.Application.ProjectService);

                gui.CommandHandler.OpenView(fmModel, typeof(ProjectItemMapView));
                ProjectItemMapView mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                ILayer modelLayer = mapView.MapView.GetLayerForData(fmModel);

                var snappedLayer = LayerTestUtils.FindLayerByNameRecursively(modelLayer, FlowFMLayerNames.EstimatedSnappedFeaturesLayerName) as IGroupLayer;
                Assert.IsNotNull(snappedLayer);
                Assert.That(snappedLayer.Layers.Count, Is.EqualTo(expectedNumberOfLayers));

                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsPoint, FlowFMLayerNames.EstimatedSnappedObservationPoints));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ThinDams, FlowFMLayerNames.EstimatedSnappedThinDams));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.FixedWeir, FlowFMLayerNames.EstimatedSnappedFixedWeirs));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.LeveeBreach, FlowFMLayerNames.EstimatedSnappedLeveeBreaches));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsPoint, FlowFMLayerNames.EstimatedSnappedDryPoints));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.RoofArea, FlowFMLayerNames.EstimatedSnappedRoofAreas));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsCrossSection, FlowFMLayerNames.EstimatedSnappedDryAreas));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsCrossSection, FlowFMLayerNames.EstimatedSnappedEnclosures));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Pump, FlowFMLayerNames.EstimatedSnappedPumps));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Weir, FlowFMLayerNames.EstimatedSnappedWeirs));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Gate, FlowFMLayerNames.EstimatedSnappedGates));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsCrossSection, FlowFMLayerNames.EstimatedSnappedObservationCrossSections));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Embankment, FlowFMLayerNames.EstimatedSnappedEmbankments));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.SourceSink, FlowFMLayerNames.EstimatedSnappedSourcesAndSinks));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Boundary, FlowFMLayerNames.EstimatedSnappedBoundaries));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.WaterLevelBnd, FlowFMLayerNames.EstimatedSnappedBoundariesWaterLevel));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.VelocityBnd, FlowFMLayerNames.EstimatedSnappedBoundariesVelocity));
            } 
        }

        private static IEnumerable<TestCaseData> GetSnappedFeatureIsGeneratedData()
        {
            TestCaseData ToCase(Action<WaterFlowFMModel, Coordinate> addFeature, string layerName) =>
                new TestCaseData(addFeature, layerName) { TestName = layerName };

            TestCaseData ToCaseIgnore(Action<WaterFlowFMModel, Coordinate> addFeature, string layerName, string reason) =>
                new TestCaseData(addFeature, layerName) { TestName = layerName }
                    .Ignore(reason);

            void ObservationPoint(WaterFlowFMModel model, Coordinate center) =>
                model.Area.ObservationPoints.Add(new ObservationPoint2D
                {
                    Geometry = new Point(center)
                });

            yield return ToCase(ObservationPoint, FlowFMLayerNames.EstimatedSnappedObservationPoints);
            

            void ThinDams(WaterFlowFMModel model, Coordinate center) =>
                model.Area.ThinDams.Add(new ThinDam2D
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

            yield return ToCase(ThinDams, FlowFMLayerNames.EstimatedSnappedThinDams);

            void FixedWeir(WaterFlowFMModel model, Coordinate center) =>
                model.Area.FixedWeirs.Add(new FixedWeir()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

            yield return ToCase(FixedWeir, FlowFMLayerNames.EstimatedSnappedFixedWeirs);

            void DryPoint(WaterFlowFMModel model, Coordinate center) =>
                model.Area.DryPoints.Add(new GroupablePointFeature()
                {
                    Geometry = new Point(center)
                });

            yield return ToCase(DryPoint, FlowFMLayerNames.EstimatedSnappedDryPoints);

            void DryArea(WaterFlowFMModel model, Coordinate center) =>
                model.Area.DryAreas.Add(new GroupableFeature2DPolygon()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X, center.Y + 50.0), new Coordinate(center.X + 50.0, center.Y ), center.CoordinateValue })
                });

            yield return ToCase(DryArea, FlowFMLayerNames.EstimatedSnappedDryAreas);

            void Enclosure(WaterFlowFMModel model, Coordinate center) =>
                model.Area.Enclosures.Add(new GroupableFeature2DPolygon()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X, center.Y + 50.0), new Coordinate(center.X + 50.0, center.Y), center.CoordinateValue })
                });

            yield return ToCase(Enclosure, FlowFMLayerNames.EstimatedSnappedEnclosures);

            void Pump(WaterFlowFMModel model, Coordinate center) =>
                model.Area.Pumps.Add(new Pump2D()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

            yield return ToCase(Pump, FlowFMLayerNames.EstimatedSnappedPumps);

            void Weir(WaterFlowFMModel model, Coordinate center) =>
                model.Area.Weirs.Add(new Weir2D()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

            yield return ToCase(Weir, FlowFMLayerNames.EstimatedSnappedWeirs);

            void Gate(WaterFlowFMModel model, Coordinate center) =>
                model.Area.Gates.Add(new Gate2D()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

            yield return ToCase(Gate, FlowFMLayerNames.EstimatedSnappedGates);

            void ObservationCrossSection(WaterFlowFMModel model, Coordinate center) =>
                model.Area.ObservationCrossSections.Add(new ObservationCrossSection2D()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

            yield return ToCase(ObservationCrossSection, FlowFMLayerNames.EstimatedSnappedObservationCrossSections);

            void Embankment(WaterFlowFMModel model, Coordinate center) =>
                model.Area.Embankments.Add(new Embankment()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

            yield return ToCase(Embankment, FlowFMLayerNames.EstimatedSnappedEmbankments);

            void SourceAndSink(WaterFlowFMModel model, Coordinate center) =>
                model.SourcesAndSinks.Add(new SourceAndSink
                {
                    Feature = new Feature2D
                    {
                        Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                    }
                });

            yield return ToCase(SourceAndSink, FlowFMLayerNames.EstimatedSnappedSourcesAndSinks);

            void Boundary(WaterFlowFMModel model, Coordinate center) =>
                model.Boundaries.Add(
                    new Feature2D
                    {
                        Geometry = new LineString(new[]
                            {new Coordinate(0.0, 0.0), new Coordinate(100.0, 100.0)})
                    });

            yield return ToCase(Boundary, FlowFMLayerNames.EstimatedSnappedBoundaries);

            void LeveeBreach(WaterFlowFMModel model, Coordinate center) =>
                model.Area.LeveeBreaches.Add(new LeveeBreach
                {
                    BreachLocationX = center.X + 50.0,
                    BreachLocationY = center.Y + 50.0,
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

            yield return ToCaseIgnore(LeveeBreach, FlowFMLayerNames.EstimatedSnappedLeveeBreaches, 
                                      "FM1D2D-1898: Estimated levee breaches are broken, and as such this test does not work.");
        }

        [Test]
        [TestCaseSource(nameof(GetSnappedFeatureIsGeneratedData))]
        public void SnappedFeatureIsGenerated(Action<WaterFlowFMModel, Coordinate> addFeature, string expectedLayerName)
        {
            string netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = CreateGui())
            {
                IEventedList<ILayer> snappedLayers = SnappedLayers(gui, netFile);

                WaterFlowFMModel model = gui.Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                Envelope gridExtent = model.GridExtent;
                Coordinate center = gridExtent.Centre;
                addFeature(model, center);
                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, expectedLayerName));
            }
        }
        
        private static IGui CreateGui()
        {
            return new DHYDROGuiBuilder().WithFlowFM().Build();
        }

        private static IEventedList<ILayer> SnappedLayers(IGui gui, string netFile)
        {
            IApplication app = gui.Application;
            
            gui.Run();

            WaterFlowFMModel fmModel = AddFMModelToProject(app.ProjectService);

            //Add a basic grid
            ImportGrid(app, netFile, fmModel);

            gui.CommandHandler.OpenView(fmModel, typeof(ProjectItemMapView));
            ProjectItemMapView mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
            ILayer modelLayer = mapView.MapView.GetLayerForData(fmModel);

            var snappedLayer = LayerTestUtils.FindLayerByNameRecursively(modelLayer, FlowFMLayerNames.EstimatedSnappedFeaturesLayerName) as IGroupLayer;
            Assert.IsNotNull(snappedLayer);
            IEventedList<ILayer> snappedLayers = snappedLayer.Layers;

            //Make sure the layers are visible.
            snappedLayer.Visible = true;
            return snappedLayers;
        }

        private static bool SnapLayerHasFeatures(IList<ILayer> layers, string layerName)
        {
            SnappedFeatureCollection snappedFeature = GetSnappedFeatureCollectionFromLayers(layers, layerName);
            return snappedFeature.Features.Count > 0;
        }

        private static SnappedFeatureCollection GetSnappedFeatureCollectionFromLayers(IList<ILayer> layers, string layerName)
        {
            ILayer layer = layers.FirstOrDefault(l => l.Name == layerName);
            return layer?.DataSource as SnappedFeatureCollection;
        }

        private static bool SnapLayerExistsForFeatureType(IList<ILayer> layers, string operationApiName, string layerName)
        {
            return layers.Any() && layers.Any(l => ((SnappedFeatureCollection) l.DataSource).SnapApiFeatureType == operationApiName && l.Name == layerName);
        }
        private static WaterFlowFMModel AddFMModelToProject(IProjectService projectService)
        {
            Project project = projectService.CreateProject();

            // Add water flow model to project
            project.RootFolder.Add(new WaterFlowFMModel());

            // Check model name
            WaterFlowFMModel targetModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
            Assert.IsNotNull(targetModel);
            Assert.IsFalse(targetModel.Area.LandBoundaries.Any());
            return targetModel;
        }

        private static void ImportGrid(IApplication app, string netFile, WaterFlowFMModel targetModel)
        {
            //Import grid
            FlowFMNetFileImporter importerGrid = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
            Assert.IsNotNull(importerGrid);
            object gridImported = importerGrid.ImportItem(netFile, targetModel.Grid);
            Assert.IsNotNull(gridImported);
        }
    }
}