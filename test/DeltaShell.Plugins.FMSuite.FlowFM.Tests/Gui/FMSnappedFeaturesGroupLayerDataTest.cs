using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
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
    [Parallelizable(ParallelScope.None), Apartment(ApartmentState.STA)]
    public class FMSnappedFeaturesGroupLayerDataTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void GetAllAvailableSnappedFeaturesLayers()
        {
            var expectedNumberOfLayers = 17;

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                var fmModel = AddFMModelToProject(app);

                gui.CommandHandler.OpenView(fmModel, typeof(ProjectItemMapView));
                var mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                var modelLayer = (GroupLayer)mapView.MapView.GetLayerForData(fmModel);

                var snappedLayer = modelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMMapLayerProvider.GridSnappedFeaturesLayerName) as GroupLayer;
                Assert.IsNotNull(snappedLayer);
                Assert.That(snappedLayer.Layers.Count, Is.EqualTo(expectedNumberOfLayers));

                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsPoint, "Observation points (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ThinDams, "Thin dams (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.FixedWeir, "Fixed weirs (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.LeveeBreach, "Levee breaches (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsPoint, "Dry points (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.RoofArea, "Roof Areas (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsCrossSection, "Dry areas (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsCrossSection, "Enclosure (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Pump, "Pumps (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Weir, "Weirs (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Gate, "Gates (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.ObsCrossSection, "Observation cross sections (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Embankment, "Embankments (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.SourceSink, "Sources and sinks (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.Boundary, "Boundaries (snapped)"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.WaterLevelBnd, "Water level boundary points"));
                Assert.IsTrue(SnapLayerExistsForFeatureType(snappedLayer.Layers, UnstrucGridOperationApi.VelocityBnd, "Discharge / velocity boundary points"));
            }
        }

        [Test]
        public void SnappedObservationPointsFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.Area.ObservationPoints.Add(new GroupableFeature2DPoint()
                {
                    Geometry = new Point(center)
                });
                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Observation points (snapped)"));
            }
        }

        [Test]
        public void SnappedThinDamsFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull( model );

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.Area.ThinDams.Add(new ThinDam2D
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });
                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Thin dams (snapped)"));
            }
        }

        [Test]
        public void SnappedFixedWeirsFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.Area.FixedWeirs.Add(new FixedWeir()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Fixed weirs (snapped)"));
            }
        }

        [Test]
        public void SnappedDryPointsFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.Area.DryPoints.Add(new GroupablePointFeature()
                {
                    Geometry = new Point(center)
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Dry points (snapped)"));
            }
        }

        [Test]
        public void SnappedDryAreasFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.Area.DryAreas.Add(new GroupableFeature2DPolygon()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X, center.Y + 50.0), new Coordinate(center.X + 50.0, center.Y ), center.CoordinateValue })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Dry areas (snapped)"));
            }
        }

        [Test]
        public void SnappedEnclosureFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.Area.Enclosures.Add(new GroupableFeature2DPolygon()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X, center.Y + 50.0), new Coordinate(center.X + 50.0, center.Y), center.CoordinateValue })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Enclosure (snapped)"));
            }
        }

        [Test]
        public void SnappedPumpsFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.Area.Pumps.Add(new Pump2D()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Pumps (snapped)"));
            }
        }

        [Test]
        public void SnappedWeirsFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.Area.Weirs.Add(new Weir2D()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Weirs (snapped)"));
            }
        }

        [Test]
        public void SnappedGatesFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.Area.Gates.Add(new Gate2D()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Gates (snapped)"));
            }
        }

        [Test]
        public void SnappedObservationCrossSectionFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.Area.ObservationCrossSections.Add(new ObservationCrossSection2D()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Observation cross sections (snapped)"));
            }
        }

        [Test]
        public void SnappedEmbankmentsFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.Area.Embankments.Add(new Embankment()
                {
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Embankments (snapped)"));
            }
        }

        [Test]
        public void SnappedSourcesAndSinksFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.SourcesAndSinks.Add(new SourceAndSink
                {
                    Feature = new Feature2D
                    {
                        Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                    }
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Sources and sinks (snapped)"));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SnappedBoundaryFeatureIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);


                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                model.Boundaries.Add(
                    new Feature2D
                    {
                        Geometry = new LineString(new[]
                            {new Coordinate(0.0, 0.0), new Coordinate(100.0, 100.0)})
                    });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Boundaries (snapped)"));
            }
        }

        [Test]
        public void SnappedLeveeBreachIsGenerated()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var snappedLayers = SnappedLayers(gui, netFile);

                var model = gui.Application.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(model);

                var gridExtent = model.GridExtent;
                var center = gridExtent.Centre;
                model.Area.LeveeBreaches.Add(new LeveeBreach
                {
                    BreachLocationX = center.X + 50.0,
                    BreachLocationY = center.Y + 50.0,
                    Geometry = new LineString(new[] { center.CoordinateValue, new Coordinate(center.X + 100.0, center.Y + 100.0) })
                });

                Assert.IsTrue(SnapLayerHasFeatures(snappedLayers, "Levee breaches (snapped)"));
            }
        }

        private static IEventedList<ILayer> SnappedLayers(DeltaShellGui gui, string netFile)
        {
            var app = gui.Application;
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new FlowFMGuiPlugin());

            gui.Run();

            var fmModel = AddFMModelToProject(app);

            //Add a basic grid
            ImportGrid(app, netFile, fmModel);

            gui.CommandHandler.OpenView(fmModel, typeof(ProjectItemMapView));
            var mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
            var modelLayer = (GroupLayer) mapView.MapView.GetLayerForData(fmModel);

            var snappedLayer =
                modelLayer.Layers.FirstOrDefault(l => l.Name == FlowFMMapLayerProvider.GridSnappedFeaturesLayerName) as
                    GroupLayer;
            Assert.IsNotNull(snappedLayer);
            var snappedLayers = snappedLayer.Layers;

            //Make sure the layers are visible.
            snappedLayer.Visible = true;
            return snappedLayers;
        }

        private static bool SnapLayerHasFeatures(IList<ILayer> layers, string layerName)
        {
            var snappedFeature = GetSnappedFeatureCollectionFromLayers(layers, layerName);
            return snappedFeature.Features.Count > 0;
        }

        private static SnappedFeatureCollection GetSnappedFeatureCollectionFromLayers(IList<ILayer> layers, string layerName)
        {
            var layer = layers.FirstOrDefault(l => l.Name == layerName);
            return layer?.DataSource as SnappedFeatureCollection;
        }

        private static bool SnapLayerExistsForFeatureType(IList<ILayer> layers, string operationApiName, string layerName)
        {
            return layers.Any() && layers.Any(l => ((SnappedFeatureCollection) l.DataSource).SnapApiFeatureType == operationApiName && l.Name == layerName);
        }
        private static WaterFlowFMModel AddFMModelToProject(IApplication app)
        {
            // Add water flow model to project
            var project = app.Project;
            project.RootFolder.Add(new WaterFlowFMModel());

            // Check model name
            var targetModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
            Assert.IsNotNull(targetModel);
            Assert.IsFalse(targetModel.Area.LandBoundaries.Any());
            return targetModel;
        }

        private static void ImportGrid(IApplication app, string netFile, WaterFlowFMModel targetModel)
        {
            //Import grid
            var importerGrid = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
            Assert.IsNotNull(importerGrid);
            var gridImported = importerGrid.ImportItem(netFile, targetModel.Grid);
            Assert.IsNotNull(gridImported);
        }
    }
}