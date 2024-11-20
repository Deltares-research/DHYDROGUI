using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.FMSuite.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class FMSnappedFeaturesGroupLayerDataTest
    {
        [Test]
        public void GetAllAvailableSnappedFeaturesLayers()
        {
            const int expectedNumberOfLayers = 13;

            using (var gui = CreateGui())
            {
                gui.Run();

                Project project = gui.Application.ProjectService.CreateProject();
                WaterFlowFMModel fmModel = AddFMModelToProject(project);

                gui.CommandHandler.OpenView(fmModel, typeof(ProjectItemMapView));
                ProjectItemMapView mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                Assert.IsNotNull(mapView);
                var modelLayer = (GroupLayer)mapView.MapView.GetLayerForData(fmModel);

                var snappedLayer = modelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMMapLayerProvider.GridSnappedFeaturesLayerName) as GroupLayer;
                Assert.IsNotNull(snappedLayer);
                Assert.AreEqual(expectedNumberOfLayers, snappedLayer.Layers.Count);

                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsPoint, "Observation points (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ThinDams, "Thin dams (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.FixedWeir, "Fixed weirs (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsPoint, "Dry points (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsCrossSection, "Dry areas (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsCrossSection, "Enclosure (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Pump, "Pumps (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Weir, "Structures (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsCrossSection, "Observation cross sections (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.SourceSink, "Sources and sinks (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Boundary, "Boundaries (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.WaterLevelBnd, "Water level boundary points"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.VelocityBnd, "Discharge / velocity boundary points"));
            }
        }

        [Test]
        public void SnappedObservationPointsFeatureIsGenerated()
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
                model.Area.ObservationPoints.Add(new GroupableFeature2DPoint() { Geometry = new Point(center) });
                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Observation points (snapped)"));
            }
        }

        [Test]
        public void SnappedThinDamsFeatureIsGenerated()
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
                model.Area.ThinDams.Add(new ThinDam2D
                {
                    Geometry = new LineString(new[]
                    {
                        center.CoordinateValue,
                        new Coordinate(center.X + 100.0, center.Y + 100.0)
                    })
                });
                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Thin dams (snapped)"));
            }
        }

        [Test]
        public void SnappedFixedWeirsFeatureIsGenerated()
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
                model.Area.FixedWeirs.Add(new FixedWeir()
                {
                    Geometry = new LineString(new[]
                    {
                        center.CoordinateValue,
                        new Coordinate(center.X + 100.0, center.Y + 100.0)
                    })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Fixed weirs (snapped)"));
            }
        }

        [Test]
        public void SnappedDryPointsFeatureIsGenerated()
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
                model.Area.DryPoints.Add(new GroupablePointFeature() { Geometry = new Point(center) });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Dry points (snapped)"));
            }
        }

        [Test]
        public void SnappedDryAreasFeatureIsGenerated()
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
                model.Area.DryAreas.Add(new GroupableFeature2DPolygon()
                {
                    Geometry = new LineString(new[]
                    {
                        center.CoordinateValue,
                        new Coordinate(center.X, center.Y + 50.0),
                        new Coordinate(center.X + 50.0, center.Y),
                        center.CoordinateValue
                    })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Dry areas (snapped)"));
            }
        }

        [Test]
        public void SnappedEnclosureFeatureIsGenerated()
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
                model.Area.Enclosures.Add(new GroupableFeature2DPolygon()
                {
                    Geometry = new LineString(new[]
                    {
                        center.CoordinateValue,
                        new Coordinate(center.X, center.Y + 50.0),
                        new Coordinate(center.X + 50.0, center.Y),
                        center.CoordinateValue
                    })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Enclosure (snapped)"));
            }
        }

        [Test]
        public void SnappedPumpsFeatureIsGenerated()
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
                model.Area.Pumps.Add(new Pump()
                {
                    Geometry = new LineString(new[]
                    {
                        center.CoordinateValue,
                        new Coordinate(center.X + 100.0, center.Y + 100.0)
                    })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Pumps (snapped)"));
            }
        }

        [Test]
        public void SnappedWeirsFeatureIsGenerated()
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
                model.Area.Structures.Add(new Structure()
                {
                    Geometry = new LineString(new[]
                    {
                        center.CoordinateValue,
                        new Coordinate(center.X + 100.0, center.Y + 100.0)
                    })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Structures (snapped)"));
            }
        }

        [Test]
        public void SnappedObservationCrossSectionFeatureIsGenerated()
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
                model.Area.ObservationCrossSections.Add(new ObservationCrossSection2D()
                {
                    Geometry = new LineString(new[]
                    {
                        center.CoordinateValue,
                        new Coordinate(center.X + 100.0, center.Y + 100.0)
                    })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Observation cross sections (snapped)"));
            }
        }

        [Test]
        public void SnappedSourcesAndSinksFeatureIsGenerated()
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
                model.SourcesAndSinks.Add(new SourceAndSink
                {
                    Feature = new Feature2D
                    {
                        Geometry = new LineString(new[]
                        {
                            center.CoordinateValue,
                            new Coordinate(center.X + 100.0, center.Y + 100.0)
                        })
                    }
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Sources and sinks (snapped)"));
            }
        }

        [Test]
        public void SnappedBoundaryFeatureIsGenerated()
        {
            string netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = CreateGui())
            {
                IEventedList<ILayer> snappedLayers = SnappedLayers(gui, netFile);

                WaterFlowFMModel model = gui.Application.ProjectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                model.Boundaries.Add(
                    new Feature2D
                    {
                        Geometry = new LineString(new[]
                        {
                            new Coordinate(0.0, 0.0),
                            new Coordinate(100.0, 100.0)
                        })
                    });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Boundaries (snapped)"));
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

            Project project = app.ProjectService.CreateProject();
            WaterFlowFMModel fmModel = AddFMModelToProject(project);

            //Add a basic grid
            ImportGrid(app, netFile, fmModel);

            gui.CommandHandler.OpenView(fmModel, typeof(ProjectItemMapView));
            ProjectItemMapView mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
            var modelLayer = (GroupLayer)mapView.MapView.GetLayerForData(fmModel);

            var snappedLayer =
                modelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMMapLayerProvider.GridSnappedFeaturesLayerName) as
                    GroupLayer;
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
            return layers.Any() && layers.Any(l => ((SnappedFeatureCollection)l.DataSource).SnapApiFeatureType == operationApiName && l.Name == layerName);
        }

        private static WaterFlowFMModel AddFMModelToProject(Project project)
        {
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