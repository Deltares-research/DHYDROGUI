using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Forms;
using Control = System.Windows.Controls.Control;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class FlowFmMapLayerProviderTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowLayersForFmModel()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var waterFlowFmModel = new WaterFlowFMModel();
            waterFlowFmModel.ImportFromMdu(mduPath);

            ShowModelLayers(waterFlowFmModel);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowLayersForIvoorkust()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var waterFlowFmModel = new WaterFlowFMModel();
            waterFlowFmModel.ImportFromMdu(mduPath);

            ShowModelLayers(waterFlowFmModel);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowLayersAdjustedModel()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            model.Area.DredgingLocations.Add(new GroupableFeature2D
            {
                Geometry = new Polygon(new LinearRing(new[]
                {
                    new Coordinate(-135, -105),
                    new Coordinate(-85, -100),
                    new Coordinate(-75, -205),
                    new Coordinate(-125, -200),
                    new Coordinate(-135, -105)
                }))
            });

            ShowModelLayers(model);
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void CheckLayerIsSetCorrectlyWhenOpeningFmItems()
        {
            string mduPath = TestHelper.GetTestFilePath(@"roughness\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            using (var gui = new DeltaShellGui())
            {
                var fmGuiPlugin = new FlowFMGuiPlugin();

                IApplication app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(fmGuiPlugin);

                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = app.Project;
                    project.RootFolder.Add(model);

                    var modelNodePresenter = new WaterFlowFMModelNodePresenter(fmGuiPlugin);
                    IEnumerable shortcut = modelNodePresenter.GetChildNodeObjects(model, null);
                    FmModelTreeShortcut fmModelTreeShortCut = shortcut.OfType<FmModelTreeShortcut>().First(s => s.Text == "General");
                    gui.CommandHandler.OpenView(fmModelTreeShortCut);
                    IView activeView = gui.DocumentViews.ActiveView;

                    var providers = new IMapLayerProvider[]
                    {
                        new FlowFMMapLayerProvider(),
                        new SharpMapLayerProvider()
                    };

                    var layer = (IGroupLayer) MapLayerProviderHelper.CreateLayersRecursive(fmModelTreeShortCut.FlowFmModel, null, providers);

                    Assert.IsInstanceOf<IView>(activeView);
                    Assert.IsNotNull(layer.Layers);
                    Assert.IsNotNull(layer.Layers.Any());
                    Assert.That(layer.ShowInLegend, Is.EqualTo(true));
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void CheckFmBridgePillarLayerIsCreated()
        {
            var model = new WaterFlowFMModel();

            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());

                var networkEditorGuiPlugin = new NetworkEditorGuiPlugin();
                gui.Plugins.Add(networkEditorGuiPlugin);
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Project project = app.Project;
                project.RootFolder.Add(model);

                //Create a new layer
                bool result = networkEditorGuiPlugin.MapLayerProvider.CanCreateLayerFor(model.Area.BridgePillars, model.Area);
                Assert.IsTrue(result);
                Assert.IsNotNull(model.Area);
                ILayer layer = networkEditorGuiPlugin.MapLayerProvider.CreateLayer(model.Area.BridgePillars, model.Area);

                Assert.IsNotNull(layer); //assert it got injected 
                Assert.AreEqual(typeof(BridgePillar), layer.DataSource.FeatureType);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void GivenAFlowFMMapLayerProviderAndAClassMapFileFunctionStore_WhenCreateLayerIsCalled_ThenCorrectLayerIsCreated()
        {
            // Given
            var mapLayerProvider = new FlowFMMapLayerProvider();
            var fmClassMapFileFunctionStore = Substitute.For<IFMClassMapFileFunctionStore>();

            // When
            ILayer layer = mapLayerProvider.CreateLayer(fmClassMapFileFunctionStore, null);

            // Then
            Assert.IsNotNull(layer);
            Assert.AreEqual("Output (class)", layer.Name);
            Assert.IsTrue(layer is IGroupLayer);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void GivenAFlowFMMapLayerProviderAndAClassMapFileFunctionStore_WhenCanCreateLayerForIsCalle_ThenTrueIsReturned()
        {
            // Given
            var mapLayerProvider = new FlowFMMapLayerProvider();
            var fmClassMapFileFunctionStore = Substitute.For<IFMClassMapFileFunctionStore>();

            // When
            bool result = mapLayerProvider.CanCreateLayerFor(fmClassMapFileFunctionStore, null);

            // Then
            Assert.IsTrue(result);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAFlowFmMapLayerProviderAndAModelWithAClassMapFileFunctionStore_WhenChildLayerObjectsIsCalled_ThenTheFunctionStoreIsReturned()
        {
            // Given
            string testDirectoryPath = TestHelper.GetTestFilePath("output_classmapfiles");
            string outputDirectoryPath = Path.Combine(testDirectoryPath, "output");
            string filePath = Path.Combine(outputDirectoryPath, "FlowFM_clm.nc");
            Assert.IsTrue(File.Exists(filePath));

            var model = new WaterFlowFMModel();
            model.ConnectOutput(outputDirectoryPath);
            IFMClassMapFileFunctionStore outputClassMapFileStore = model.OutputClassMapFileStore;
            Assert.NotNull(outputClassMapFileStore);
            Assert.AreEqual(filePath, outputClassMapFileStore.Path);

            var mapLayerProvider = new FlowFMMapLayerProvider();

            // When
            object[] childLayerObjects = mapLayerProvider.ChildLayerObjects(model).ToArray();

            // Then
            IFMClassMapFileFunctionStore classMapFileFunctionStoreLayer = childLayerObjects.OfType<IFMClassMapFileFunctionStore>().SingleOrDefault();
            Assert.IsNotNull(classMapFileFunctionStoreLayer);
            Assert.AreSame(classMapFileFunctionStoreLayer, outputClassMapFileStore);
        }

        [Test]
        public void GivenAFlowFmMapLayerProviderAndAClassMapFileFunctionStore_WhenChildLayerObjectsIsCalled_ThenTheFunctionsAndGridAreReturned()
        {
            // Given
            var classMapFileStore = Substitute.For<IFMClassMapFileFunctionStore>();
            classMapFileStore.Functions = new EventedList<IFunction>(new []
            {
                Substitute.For<IFunction>(),
                Substitute.For<IFunction>()
            });
            classMapFileStore.Grid.Returns(new UnstructuredGrid());
            Assert.NotNull(classMapFileStore);
            Assert.IsNotEmpty(classMapFileStore.Functions);
            Assert.IsNotNull(classMapFileStore.Grid);
            var mapLayerProvider = new FlowFMMapLayerProvider();

            // When
            object[] childLayerObjects = mapLayerProvider.ChildLayerObjects(classMapFileStore).ToArray();

            // Then
            Assert.IsTrue(classMapFileStore.Functions.All(f => childLayerObjects.Contains(f)));
            Assert.IsTrue(childLayerObjects.Contains(classMapFileStore.Grid));
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void CheckFmLayerProviderGivesAWarningWithInvalidGeometryForEnclosure()
        {
            var model = new WaterFlowFMModel();

            using (var gui = new DeltaShellGui())
            {
                var fmGuiPlugin = new FlowFMGuiPlugin();

                IApplication app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(fmGuiPlugin);

                gui.Run();

                Project project = app.Project;
                project.RootFolder.Add(model);

                var featureName = "Enclosure01";
                GroupableFeature2DPolygon enclosureFeature = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                    featureName,
                    FlowFMTestHelper.GetInvalidGeometryForEnclosureExample());

                model.Area.Enclosures.Add(enclosureFeature);

                /* Make sure the method works first */
                IMapLayerProvider layerProvider = fmGuiPlugin.MapLayerProvider;
                IEnumerable<HydroArea> areaChildren = layerProvider.ChildLayerObjects(model).OfType<HydroArea>();
                IEnumerable<HydroArea> hydroAreas = areaChildren as HydroArea[] ?? areaChildren.ToArray();
                List<HydroArea> listOfHydroAreas = hydroAreas.ToList();
                Assert.AreEqual(1, listOfHydroAreas.Count);

                /* Now check there are log messages instantiating the enum to list. */
                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => areaChildren.ToList(),
                    string.Format(Resources.WaterFlowFMEnclosureValidator_Validate_Drawn_polygon_not__0__not_valid, featureName));
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void GivenAFlowFmMapLayProvider_WhenCreatingAMapLayerBoundaryConditions_ThenBoundaryConditionLayerIsNotEnabledInLegend()
        {
            //Given
            var mapLayerProvider = new FlowFMMapLayerProvider();
            var boundaryConditionSets = new EventedList<BoundaryConditionSet>() {new BoundaryConditionSet()};
            var waterFlowFmModel = new WaterFlowFMModel();

            //When
            ILayer layer = mapLayerProvider.CreateLayer(boundaryConditionSets, waterFlowFmModel);

            //Then
            Assert.That(layer.Name, Is.EqualTo("Boundary Conditions"));
            Assert.That(layer.ShowInLegend, Is.EqualTo(false));
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void GivenAFlowFmMapLayProvider_WhenCreatingAMapLayerSourceAndSinks_ThenSourceAndSinksLayerIsNotEnabledInLegend()
        {
            //Given
            var mapLayerProvider = new FlowFMMapLayerProvider();
            var sourceAndSinks = new EventedList<Feature2D>();
            var waterFlowFmModel = new WaterFlowFMModel();
            TypeUtils.SetPrivatePropertyValue(waterFlowFmModel, "Pipes", sourceAndSinks);

            //When
            ILayer layer = mapLayerProvider.CreateLayer(sourceAndSinks, waterFlowFmModel);

            //Then
            Assert.That(layer.Name, Is.EqualTo("Sources and Sinks"));
            Assert.That(layer.ShowInLegend, Is.EqualTo(false));
        }

        [Test]
        public void CreateLayer_DataIsSamples_ReturnsReadOnlyPointCloudLayer()
        {
            // Setup
            const string name = "randomName";
            var samples = new Samples(name);
            var provider = new FlowFMMapLayerProvider();

            // Call
            ILayer layer = provider.CreateLayer(samples, null);
            
            // Assert
            Assert.That(layer, Is.TypeOf<PointCloudLayer>());

            var pointCloudLayer = (PointCloudLayer)layer;
            Assert.That(pointCloudLayer.ReadOnly, Is.True);
            Assert.That(pointCloudLayer.Name, Is.EqualTo(name));
        }

        private static void ShowModelLayers(WaterFlowFMModel model)
        {
            var providers = new IMapLayerProvider[]
            {
                new FlowFMMapLayerProvider(),
                new SharpMapLayerProvider()
            };

            var layer = (IGroupLayer) MapLayerProviderHelper.CreateLayersRecursive(model, null, providers);

            layer.Layers.ForEach(l => l.Visible = true);

            var map = new Map
            {
                Layers = {layer},
                Size = new Size
                {
                    Width = 800,
                    Height = 800
                }
            };
            map.ZoomToExtents();

            var mapControl = new MapControl
            {
                Map = map,
                Dock = DockStyle.Fill
            };

            WindowsFormsTestHelper.ShowModal(mapControl);
        }
    }
}