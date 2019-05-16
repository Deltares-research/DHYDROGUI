using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.UI.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.FunctionStores;
using Control = System.Windows.Controls.Control;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class FlowFMMapLayerProviderTest
    {
        [Test]
        public void ShowLayersForFMModel()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            ShowModelLayers(new WaterFlowFMModel.WaterFlowFMModel(mduPath));
        }
        
        [Test]
        public void ShowLayersForIvoorkust()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            ShowModelLayers(new WaterFlowFMModel.WaterFlowFMModel(mduPath));
        }

        [Test]
        public void ShowLayersAdjustedModel()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel.WaterFlowFMModel(mduPath);

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

        [Test]
        public void CheckLayerIsSetCorrectlyWhenOpeningFMItems()
        {
            var mduPath = TestHelper.GetTestFilePath(@"roughness\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel.WaterFlowFMModel(mduPath);

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
                    var shortcut = modelNodePresenter.GetChildNodeObjects(model, null);
                    var fmModelTreeShortCut = shortcut.OfType<FmModelTreeShortcut>().First(s => s.Text == "General");
                    gui.CommandHandler.OpenView(fmModelTreeShortCut);
                    var activeView = gui.DocumentViews.ActiveView;

                    var providers = new IMapLayerProvider[] { new FlowFMMapLayerProvider(), new SharpMapLayerProvider() };

                    var layer = (IGroupLayer)MapLayerProviderHelper.CreateLayersRecursive(fmModelTreeShortCut.FlowFmModel, null, providers);

                    Assert.IsInstanceOf<IView>(activeView);
                    Assert.IsNotNull((layer.Layers));
                    Assert.IsNotNull((layer.Layers.Any()));
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        public void CheckFMEnclosureLayerIsCreated()
        {
            var model = new WaterFlowFMModel.WaterFlowFMModel();

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
            var model = new WaterFlowFMModel.WaterFlowFMModel();

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
        public void GivenAFlowFMMapLayerProviderAndAClassMapFileFunctionStore_WhenCreateLayerIsCalled_ThenCorrectLayerIsCreated()
        {
            // Given
            var mapLayerProvider = new FlowFMMapLayerProvider();
            var fmClassMapFileFunctionStore = new FMClassMapFileFunctionStore(string.Empty);

            // When
            var layer = mapLayerProvider.CreateLayer(fmClassMapFileFunctionStore, null);

            // Then
            Assert.IsNotNull(layer);
            Assert.AreEqual("Output (class)", layer.Name);
            Assert.IsTrue(layer is IGroupLayer);
        }

        [Test]
        public void GivenAFlowFMMapLayerProviderAndAClassMapFileFunctionStore_WhenCanCreateLayerForIsCalle_ThenTrueIsReturned()
        {
            // Given
            var mapLayerProvider = new FlowFMMapLayerProvider();
            var fmClassMapFileFunctionStore = new FMClassMapFileFunctionStore(string.Empty);

            // When
            var result = mapLayerProvider.CanCreateLayerFor(fmClassMapFileFunctionStore, null);

            // Then
            Assert.IsTrue(result);
        }

        [Test, Category(TestCategory.DataAccess)]
        public void GivenAFlowFmMapLayerProviderAndAModelWithAClassMapFileFunctionStore_WhenChildLayerObjectsIsCalled_ThenTheFunctionStoreIsReturned()
        {
            // Given
            var testDirectoryPath = TestHelper.GetTestFilePath("output_classmapfiles");
            var outputDirectoryPath = Path.Combine(testDirectoryPath, "output");
            var filePath = Path.Combine(outputDirectoryPath, "FlowFM_clm.nc");
            Assert.IsTrue(File.Exists(filePath));

            var model = new WaterFlowFMModel.WaterFlowFMModel();
            model.ConnectOutput(outputDirectoryPath);
            var outputClassMapFileStore = model.OutputClassMapFileStore;
            Assert.NotNull(outputClassMapFileStore);
            Assert.AreEqual(filePath, outputClassMapFileStore.Path);

            var mapLayerProvider = new FlowFMMapLayerProvider();

            // When
            var childLayerObjects = mapLayerProvider.ChildLayerObjects(model).ToArray();

            // Then
            var classMapFileFunctionStoreLayer = childLayerObjects.OfType<FMClassMapFileFunctionStore>().SingleOrDefault();
            Assert.IsNotNull(classMapFileFunctionStoreLayer);
            Assert.AreSame(classMapFileFunctionStoreLayer, outputClassMapFileStore);
        }

        [Test, Category(TestCategory.DataAccess)]
        public void GivenAFlowFmMapLayerProviderAndAClassMapFileFunctionStore_WhenChildLayerObjectsIsCalled_ThenTheFunctionsAndGridAreReturned()
        {
            // Given
            var testDirectoryPath = TestHelper.GetTestFilePath("output_classmapfiles");
            var outputDirectoryPath = Path.Combine(testDirectoryPath, "output");
            var filePath = Path.Combine(outputDirectoryPath, "FlowFM_clm.nc");
            Assert.IsTrue(File.Exists(filePath));

            var classMapFileStore = new FMClassMapFileFunctionStore(filePath);
            Assert.NotNull(classMapFileStore);
            Assert.IsNotEmpty(classMapFileStore.Functions);
            Assert.IsNotNull(classMapFileStore.Grid);
            var mapLayerProvider = new FlowFMMapLayerProvider();

            // When
            var childLayerObjects = mapLayerProvider.ChildLayerObjects(classMapFileStore).ToArray();

            // Then
            Assert.IsTrue(classMapFileStore.Functions.All(f=> childLayerObjects.Contains(f)));
            Assert.IsTrue(childLayerObjects.Contains(classMapFileStore.Grid));
        }

        [Test]
        public void CheckFMLayerProviderGivesAWarningWithInvalidGeometryForEnclosure()
        {
            var model = new WaterFlowFMModel.WaterFlowFMModel();

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

        private static void ShowModelLayers(WaterFlowFMModel.WaterFlowFMModel model)
        {
            var providers = new IMapLayerProvider[] { new FlowFMMapLayerProvider(), new SharpMapLayerProvider() };

            var layer = (IGroupLayer) MapLayerProviderHelper.CreateLayersRecursive(model, null, providers);

            layer.Layers.ForEach(l => l.Visible = true);

            var map = new Map {Layers = {layer}, Size = new Size {Width = 800, Height = 800}};
            map.ZoomToExtents();

            var mapControl = new MapControl {Map = map, Dock = DockStyle.Fill};

            WindowsFormsTestHelper.ShowModal(mapControl);
        }
    }
}