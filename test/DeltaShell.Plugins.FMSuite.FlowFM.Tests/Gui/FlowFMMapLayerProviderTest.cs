using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Forms;
using Control = System.Windows.Controls.Control;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture, Apartment(ApartmentState.STA)]
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
        
        [Test]
        [TestCase("FlowFM_clm.nc")]
        [TestCase("T2_FlowFM_clm.nc")]
        public void OpenClassMapFileAndInpectFunctions(string flowfmClmNc)
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_clmfiles");
            
            flowfmClmNc = TestHelper.CreateLocalCopy(Path.Combine(testDataFilePath, flowfmClmNc));
            var store = new FMClassMapFileFunctionStore(flowfmClmNc);
                
            
            using (var gui = new DeltaShellGui())
            {
                var fmModel = new WaterFlowFMModel();
                TypeUtils.SetPrivatePropertyValue(fmModel, nameof(WaterFlowFMModel.OutputClassMapFileStore), store);
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Application.UserSettings["ShowStartUpScreen"] = false;
                gui.Run();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(fmModel);
                    gui.CommandHandler.OpenView(fmModel);
                    
                    var waterLevelFunction = (NetworkCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh1d_s1");

                    SharpMapGisGuiPlugin.Instance.Gui.CommandHandler.OpenView(waterLevelFunction);
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }
        [Test]
        public void OpenHisFileCheckFunctions()
        {
            LogHelper.ConfigureLogging(Level.Error);

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1, true);
            var branch = network.Branches[0];
            var pump = new Pump("PMP1", true) { Chainage = branch.Length / 4 };
            var pump2 = new Pump("PMP2", true) {Chainage = branch.Length / 4 * 3};
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(pump, branch);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(pump2, branch);
            var area = new HydroArea();
            var store = new FMHisFileFunctionStore(network, area)
                {Path = TestHelper.GetTestFilePath("output_hisfiles\\pump_his.nc")};
            
            using (var gui = new DeltaShellGui())
            {
                var fmModel = new WaterFlowFMModel(){Area = area,Network = network};
                TypeUtils.SetPrivatePropertyValue(fmModel, nameof(WaterFlowFMModel.OutputHisFileStore), store);
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Application.UserSettings["ShowStartUpScreen"] = false;
                gui.Run();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(fmModel);
                    gui.CommandHandler.OpenView(fmModel);
                    var mapView = gui.DocumentViews.GetViewsOfType<MapView>().FirstOrDefault();
                    mapView.MapControl.SelectTool.Select(pump);

                    //mapView.MapControl.GetToolByType<QueryTimeSeriesMapTool>().Execute();

                    var timeSeriesList = new List<IFunction>();
                    var coverages = GetTimeDependentCoverages(new[] {pump});
                    foreach (var coverage in coverages)
                    {
                        var dictionary = mapView.MapControl.SelectedFeatures.ToDictionary(GetLocationName, sf => coverage.GetTimeSeries(sf));
                        foreach (var kvp in dictionary)
                        {
                            var timeSeries = kvp.Value;
                            if (timeSeries == null)
                            {
                                continue;
                            }
                            if (coverage != null)
                            {
                                timeSeries.Components[0].Name = string.Format("{0}: {1}, {2}", kvp.Key, timeSeries.Components[0].Name, coverage);
                            }

                            timeSeries.IsEditable = false;
                            timeSeriesList.Add(timeSeries);
                        }
                    }
                    if (timeSeriesList.Count > 0)
                    {
                        SharpMapGisGuiPlugin.Instance.Gui.CommandHandler.OpenView(timeSeriesList);
                    }
                    //SharpMapGisGuiPlugin.Instance.Gui.CommandHandler.OpenView(coverages);
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
                LogHelper.ResetLogging();
            }
        }

        [Test]
        public void OpenHisFileCheckCulvertFunctions()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1, true);
            var branch = network.Branches[0];
            branch.Name = "Culvert_1D_1";
            var culvert = new Culvert("Culvert_1D_1") { Chainage = branch.Length / 4 };
            var culvert2 = new Culvert("Culvert_1D_2") {Chainage = branch.Length / 4 * 3};
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(culvert, branch);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(culvert2, branch);
            var area = new HydroArea();
            var store = new FMHisFileFunctionStore(network, area)
                {Path = TestHelper.GetTestFilePath("output_hisfiles\\culvert_his.nc")};
            var featuresByCoverage =  TypeUtils.GetField<FMHisFileFunctionStore, IDictionary<string, IEnumerable<IFeature>>>(store, "FeaturesByCoverage");
            Assert.That(featuresByCoverage["culvert"].Count(), Is.EqualTo(2));
            using (var gui = new DeltaShellGui())
            {
                var fmModel = new WaterFlowFMModel(){Area = area,Network = network};
                TypeUtils.SetPrivatePropertyValue(fmModel, nameof(WaterFlowFMModel.OutputHisFileStore), store); 
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Application.UserSettings["ShowStartUpScreen"] = false;
                gui.Run();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(fmModel);
                    gui.CommandHandler.OpenView(fmModel);
                    var mapView = gui.DocumentViews.GetViewsOfType<MapView>().FirstOrDefault();
                    mapView.MapControl.SelectTool.Select(culvert);

                    //mapView.MapControl.GetToolByType<QueryTimeSeriesMapTool>().Execute();

                    var timeSeriesList = new List<IFunction>();
                    var coverages = GetTimeDependentCoverages(new[] {culvert});
                    foreach (var coverage in coverages)
                    {
                        var dictionary = mapView.MapControl.SelectedFeatures.ToDictionary(GetLocationName, sf => coverage.GetTimeSeries(sf));
                        foreach (var kvp in dictionary)
                        {
                            var timeSeries = kvp.Value;
                            if (timeSeries == null)
                            {
                                continue;
                            }
                            if (coverage != null)
                            {
                                timeSeries.Components[0].Name = string.Format("{0}: {1}, {2}", kvp.Key, timeSeries.Components[0].Name, coverage);
                            }

                            timeSeries.IsEditable = false;
                            timeSeriesList.Add(timeSeries);
                        }
                    }
                    if (timeSeriesList.Count > 0)
                    {
                        SharpMapGisGuiPlugin.Instance.Gui.CommandHandler.OpenView(timeSeriesList);
                    }
                    //SharpMapGisGuiPlugin.Instance.Gui.CommandHandler.OpenView(coverages);
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }
        private static string GetLocationName(IFeature feature)
        {
            var networkLocation = feature as INetworkLocation;
            if (networkLocation != null) // Grid point
            {
                return String.Format("{0}_{1}", networkLocation.Branch, networkLocation.Chainage);
            }

            var nameable = feature as INameable;
            if (nameable != null && nameable.Name != null)
            {
                return nameable.Name;
            }

            return feature.ToString();
        }

        private static IEnumerable<ICoverage> GetTimeDependentCoverages(IEnumerable<IFeature> selectedFeatures)
        {
            var app = SharpMapGisGuiPlugin.Instance.Gui.Application;
            var networks = selectedFeatures.OfType<INetworkFeature>().Select(nf => nf.Network).Distinct();

            return app.Project.GetAllItemsRecursive()
                .OfType<ICoverage>()
                .Where(c => IsValidCoverage(c, networks, selectedFeatures));
        }
        private static bool IsValidCoverage(ICoverage coverage, IEnumerable<INetwork> networks, IEnumerable<IFeature> selectedFeatures)
        {
            if (!coverage.IsTimeDependent)
            {
                return false;
            }

            var networkCoverage = coverage as INetworkCoverage;
            if (networkCoverage != null)
            {
                return networks.Contains(networkCoverage.Network);
            }

            var featureCoverage = coverage as IFeatureCoverage;
            if (featureCoverage != null)
            {
                return selectedFeatures.All(feature =>
                {
                    try
                    {
                        var features = featureCoverage.FeatureVariable.Values.OfType<IFeature>();
                        return features.Contains(feature);
                    }
                    catch (ArgumentException e)
                    {
                        return false;
                    }
                });
            }

            var cellCoverage = coverage as UnstructuredGridCellCoverage;
            if (cellCoverage != null)
            {
                var grids = selectedFeatures.OfType<UnstructuredGridFeature>().Where(f => f.Type == UnstructuredGridFeatureType.Cell).Select(c => c.UnstructuredGrid).Distinct();
                return grids.Contains(cellCoverage.Grid);
            }

            var vertexCoverage = coverage as UnstructuredGridVertexCoverage;
            if (vertexCoverage != null)
            {
                var grids = selectedFeatures.OfType<UnstructuredGridFeature>().Where(f => f.Type == UnstructuredGridFeatureType.Vertex).Select(c => c.UnstructuredGrid).Distinct();
                return grids.Contains(vertexCoverage.Grid);
            }

            var flowLinkCoverage = coverage as UnstructuredGridFlowLinkCoverage;
            if (flowLinkCoverage != null)
            {
                var grids = selectedFeatures.OfType<UnstructuredGridFeature>().Where(f => f.Type == UnstructuredGridFeatureType.Edge).Select(c => c.UnstructuredGrid).Distinct();
                return grids.Contains(flowLinkCoverage.Grid);
            }

            var regularGridCoverage = coverage as IRegularGridCoverage;
            if (regularGridCoverage != null)
            {
                return selectedFeatures.OfType<IRegularGridCoverageCell>().All(c => Equals(c.RegularGridCoverage, regularGridCoverage));
            }

            return false;
        }
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
            var canCreateLayerFor = mapLayerProvider.CanCreateLayerFor(new EventedList<ILink1D2D>(), new WaterFlowFMModel());
            Assert.IsTrue(canCreateLayerFor);
        }

        [Test]
        [Category("Quarantine")]
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
        [Category("Quarantine")]
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

        [Test]
        public void GivenFlowFMMapLayerProvider_CreatingModelLayer_ShouldSetRenderOrder()
        {
            //Arrange
            var fmModel = new WaterFlowFMModel();

            // Act
            var mapLayerProviders = new List<IMapLayerProvider>
            {
                new NetworkEditorMapLayerProvider(),
                new SharpMapLayerProvider(),
                new FlowFMMapLayerProvider()
            };

            var layer = (IGroupLayer)MapLayerProviderHelper.CreateLayersRecursive(fmModel, null, mapLayerProviders, new Dictionary<ILayer, object>());

            // Assert
            var layersWithoutOrder = layer.Layers
                .Where(l => !(l is IGroupLayer))
                .Count(l => l.RenderOrder == 0);

            Assert.AreEqual(2, layersWithoutOrder);
        }

        [Test]
        public void ChildLayerObjects_ContainsCorrectObjects()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                var layerProvider = new FlowFMMapLayerProvider();
            
                // Precondition
                Assert.That(model.Infiltration, Is.Not.Null);
            
                // Call
                object[] objects = layerProvider.ChildLayerObjects(model).ToArray();
            
                // Assert
                Assert.That(objects, Does.Contain(model.Bathymetry));
                Assert.That(objects, Does.Contain(model.InitialWaterLevel));
                Assert.That(objects, Does.Contain(model.Roughness));
                Assert.That(objects, Does.Contain(model.Viscosity));
                Assert.That(objects, Does.Contain(model.Diffusivity));
                Assert.That(objects, Does.Contain(model.Infiltration));
            }
        }
        
        [Test]
        public void ChildLayerObjects_ModelDoesNotUseInfiltration_DoesNotContainInfiltration()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                var layerProvider = new FlowFMMapLayerProvider();
            
                // Set to: no infiltration
                model.ModelDefinition.GetModelProperty("infiltrationmodel").SetValueAsString("0");
            
                // Precondition
                Assert.That(model.Infiltration, Is.Not.Null);
            
                // Call
                object[] objects = layerProvider.ChildLayerObjects(model).ToArray();
            
                // Assert
                Assert.That(objects, Does.Not.Contain(model.Infiltration));
            }
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