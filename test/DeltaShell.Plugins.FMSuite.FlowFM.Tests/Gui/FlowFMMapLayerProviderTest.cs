using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.Structures;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Forms;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class FlowFMMapLayerProviderTest
    {
        private FlowFMMapLayerProvider mapLayerProvider;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            mapLayerProvider = new FlowFMMapLayerProvider();
        }

        [Test]
        public void ShowLayersForFMModel()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            ShowModelLayers(new WaterFlowFMModel(mduPath));
        }
        
        [Test]
        public void ShowLayersForIvoorkust()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            ShowModelLayers(new WaterFlowFMModel(mduPath));
        }

        [Test]
        public void ShowLayersAdjustedModel()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            model.Area.DredgingLocations.Add(new GroupableFeature2D
                {
                    Geometry = new Polygon(new LinearRing(new[]
                        {
                            new Coordinate(-135, -105), new Coordinate(-85, -100), 
                            new Coordinate(-75, -205), new Coordinate(-125, -200),  
                            new Coordinate(-135, -105)
                        }))
                });

            ShowModelLayers(model);
        }

/*        [Test]
        public void CheckLayerIsSetCorrectlyWhenOpeningFMItems()
        {
            var mduPath = TestHelper.GetTestFilePath(@"roughness\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            using (var gui = new DeltaShellGui())
            {
                var fmGuiPlugin = new FlowFMGuiPlugin();

                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(fmGuiPlugin);

                gui.Run();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(model);

                    var modelNodePresenter = new WaterFlowFMModelNodePresenter(fmGuiPlugin);
                    var fmTreeShortcut = modelNodePresenter.GetChildNodeObjects(model, null)
                                          .OfType<FlowFMTreeShortcut>()
                                          .First(s => s.Text == "General");

                    gui.CommandHandler.OpenView(fmTreeShortcut);

                    var activeView = (ProjectItemMapView)gui.DocumentViews.ActiveView;
                    var activeTab = activeView.MapView.TabControl.ActiveView;

                    Assert.IsInstanceOf<WaterFlowFMModelView>(activeTab);
                    Assert.IsNotNull(((ILayerEditorView) activeTab).Layer);
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }*/

        [Test]
        public void CheckFMEnclosureLayerIsCreated()
        {
            var model = new WaterFlowFMModel();

            using (var gui = new DeltaShellGui())
            {
                var fmGuiPlugin = new FlowFMGuiPlugin();

                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(fmGuiPlugin);

                gui.Run();

                var project = app.Project;
                project.RootFolder.Add(model);

                var enclosureFeature =
                    FlowFMTestHelper.CreateFeature2DPolygonFromGeometry("Enclosure01",
                        FlowFMTestHelper.GetValidGeometryForEnclosureExample());

                model.Area.Enclosures.Add(enclosureFeature);
                var layer = new NetworkEditorMapLayerProvider().CreateLayer(model.Area.Enclosures, model.Area);

                Assert.IsNotNull(layer); //asssert it got injected               
                Assert.AreEqual(1, layer.CustomRenderers.Count);
                Assert.AreEqual(typeof(EnclosureRenderer), layer.CustomRenderers[0].GetType());
            }
        }

        [Test]
        public void CheckFMBridgePillarLayerIsCreated()
        {
            var model = new WaterFlowFMModel();

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());

                var networkEditorGuiPlugin = new NetworkEditorGuiPlugin();
                gui.Plugins.Add(networkEditorGuiPlugin);
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                var project = app.Project;
                project.RootFolder.Add(model);

                //Create a new layer
                var result = networkEditorGuiPlugin.MapLayerProvider.CanCreateLayerFor(model.Area.BridgePillars, model.Area);
                Assert.IsTrue(result);
                Assert.IsNotNull(model.Area);
                var layer = networkEditorGuiPlugin.MapLayerProvider.CreateLayer(model.Area.BridgePillars, model.Area);

                Assert.IsNotNull(layer); //assert it got injected 
                Assert.AreEqual(typeof(BridgePillar), layer.DataSource.FeatureType);
            }
        }

        [Test]
        public void CheckFMLayerProviderGivesAWarningWithInvalidGeometryForEnclosure()
        {
            var model = new WaterFlowFMModel();

            using (var gui = new DeltaShellGui())
            {
                var fmGuiPlugin = new FlowFMGuiPlugin();

                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(fmGuiPlugin);

                gui.Run();

                var project = app.Project;
                project.RootFolder.Add(model);

                var featureName = "Enclosure01";
                var enclosureFeature = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                                        featureName,
                                        FlowFMTestHelper.GetInvalidGeometryForEnclosureExample());

                model.Area.Enclosures.Add(enclosureFeature);

                /* Make sure the method works first */
                var layerProvider = fmGuiPlugin.MapLayerProvider;
                var areaChildren = layerProvider.ChildLayerObjects(model).OfType<HydroArea>();
                Assert.AreEqual(1, areaChildren.ToList().Count);
                
                /* Now check there are log messages instantiating the enum to list. */
                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => areaChildren.ToList(),
                    String.Format(Resources.WaterFlowFMEnclosureValidator_Validate_Drawn_polygon_not__0__not_valid, featureName));
            }
        }

        [Test]
        public void FlowFmMapLayerProviderCanCreateLayerForListOfWaterFlowFm1D2DLinks()
        {
            var canCreateLayerFor = mapLayerProvider.CanCreateLayerFor(new EventedList<Link1D2D>(), new WaterFlowFMModel());
            Assert.IsTrue(canCreateLayerFor);
        }

        [Test]
        public void GivenWaterFlowFmModel_WhenGettingChildLayerObjects_ThenIncludesModelLinks()
        {
            var fromCell = 0;
            var toCell = 1;
            var fmModel = new WaterFlowFMModel
            {
                Links = new EventedList<ILink1D2D> { mocks.Stub<Link1D2D>(fromCell, toCell) }
            };
            var childObjects = mapLayerProvider.ChildLayerObjects(fmModel);

            Assert.IsNotEmpty(childObjects.Where(c => c is EventedList<Link1D2D>));
        }

        [Test]
        public void GivenWaterFlowFmModelLinks_WhenCreatingLayer_ThenReturnVectorLayer()
        {
            var fromCell = 0;
            var toCell = 1;
            var fmModel = new WaterFlowFMModel
            {
                Links = new EventedList<ILink1D2D> { mocks.Stub<Link1D2D>(fromCell, toCell) }
            };
            var layer = mapLayerProvider.CreateLayer(fmModel.Links, fmModel);

            Assert.That(layer.GetType(), Is.EqualTo(typeof(VectorLayer)));
            Assert.That(layer.Name, Is.EqualTo("1D/2D links"));
            Assert.NotNull(layer.DataSource);
        }

        #region Test helper methods
        private static void ShowModelLayers(WaterFlowFMModel model)
        {
            var providers = new IMapLayerProvider[] { new FlowFMMapLayerProvider(), new SharpMapLayerProvider() };

            var layer = (IGroupLayer) MapLayerProviderHelper.CreateLayersRecursive(model, null, providers);

            layer.Layers.ForEach(l => l.Visible = true);

            var map = new Map {Layers = {layer}, Size = new Size {Width = 800, Height = 800}};
            map.ZoomToExtents();

            var mapControl = new MapControl {Map = map, Dock = DockStyle.Fill};

            WindowsFormsTestHelper.ShowModal(mapControl);
        }
        
        #endregion
    }
}