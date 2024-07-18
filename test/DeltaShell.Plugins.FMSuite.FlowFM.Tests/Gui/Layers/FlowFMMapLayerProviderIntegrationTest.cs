using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.IntegrationTestUtils.Builders;
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
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class FlowFMMapLayerProviderIntegrationTest
    {
        private static IGui CreateGui()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new SharpMapGisApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new CommonToolsGuiPlugin(),
                new FlowFMGuiPlugin(),
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }
        
        [Test]
        [TestCase("FlowFM_clm.nc")]
        [TestCase("T2_FlowFM_clm.nc")]
        public void OpenClassMapFileAndInspectFunctions(string flowfmClmNc)
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_clmfiles");
            
            flowfmClmNc = TestHelper.CreateLocalCopy(Path.Combine(testDataFilePath, flowfmClmNc));
            var store = new FMClassMapFileFunctionStore(flowfmClmNc);
                
            
            using (var gui = CreateGui())
            {
                var fmModel = new WaterFlowFMModel();
                TypeUtils.SetPrivatePropertyValue(fmModel, nameof(WaterFlowFMModel.OutputClassMapFileStore), store);

                gui.Application.UserSettings["ShowStartUpScreen"] = false;
                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = gui.Application.ProjectService.CreateProject();
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
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1, true);
            var branch = network.Branches[0];
            var pump = new Pump("PMP1", true) { Chainage = branch.Length / 4 };
            var pump2 = new Pump("PMP2", true) {Chainage = branch.Length / 4 * 3};
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(pump, branch);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(pump2, branch);
            var area = new HydroArea();
            var store = new FMHisFileFunctionStore(network, area)
                {Path = TestHelper.GetTestFilePath("output_hisfiles\\pump_his.nc")};
            
            using (var gui = CreateGui())
            {
                var fmModel = new WaterFlowFMModel(){Area = area,Network = network};
                TypeUtils.SetPrivatePropertyValue(fmModel, nameof(WaterFlowFMModel.OutputHisFileStore), store);
                gui.Application.UserSettings["ShowStartUpScreen"] = false;
                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = gui.Application.ProjectService.CreateProject();
                    project.RootFolder.Add(fmModel);
                    gui.CommandHandler.OpenView(fmModel);
                    var mapView = gui.DocumentViews.GetViewsOfType<MapView>().FirstOrDefault();
                    mapView.MapControl.SelectTool.Select(pump);

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
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
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
            using (var gui = CreateGui())
            {
                var fmModel = new WaterFlowFMModel(){Area = area,Network = network};
                TypeUtils.SetPrivatePropertyValue(fmModel, nameof(WaterFlowFMModel.OutputHisFileStore), store);
                gui.Application.UserSettings["ShowStartUpScreen"] = false;
                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = gui.Application.ProjectService.CreateProject();
                    project.RootFolder.Add(fmModel);
                    gui.CommandHandler.OpenView(fmModel);
                    var mapView = gui.DocumentViews.GetViewsOfType<MapView>().FirstOrDefault();
                    mapView.MapControl.SelectTool.Select(culvert);

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

            return app.ProjectService.Project.GetAllItemsRecursive()
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

            var fmGuiPlugin = new FlowFMGuiPlugin();
            var plugins = new List<IPlugin>() { fmGuiPlugin };
            using (var gui = CreateGui(plugins))
            {
                gui.Run();

                Project project = gui.Application.ProjectService.CreateProject();
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

            var networkEditorGuiPlugin = new NetworkEditorGuiPlugin();
            var plugins = new List<IPlugin>()
            {
                new FlowFMGuiPlugin(), 
                networkEditorGuiPlugin
            };
            using (var gui = CreateGui(plugins))
            {
                gui.Run();

                Project project = gui.Application.ProjectService.CreateProject();
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

            var fmGuiPlugin = new FlowFMGuiPlugin();
            var plugins = new List<IPlugin>() { fmGuiPlugin };
            using (var gui = CreateGui(plugins))
            {
                gui.Run();

                Project project = gui.Application.ProjectService.CreateProject();
                project.RootFolder.Add(model);

                const string featureName = "Enclosure01";
                GroupableFeature2DPolygon enclosureFeature = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                    featureName,
                    FlowFMTestHelper.GetInvalidGeometryForEnclosureExample());

                model.Area.Enclosures.Add(enclosureFeature);

                /* Make sure the method works first */
                IMapLayerProvider layerProvider = fmGuiPlugin.MapLayerProvider;
                IEnumerable<HydroArea> areaChildren = Enumerable.Empty<HydroArea>(); 

                /* Now check there are log messages instantiating the enum to list. */
                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => areaChildren = GenerateChildrenRecursively(layerProvider, model).OfType<HydroArea>(),
                    string.Format(Resources.WaterFlowFMEnclosureValidator_Validate_Drawn_polygon_not__0__not_valid, featureName));

                Assert.That(areaChildren.ToList(), Has.Count.EqualTo(1));
            }
        }
        
        private static IGui CreateGui(IEnumerable<IPlugin> plugins)
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new SharpMapGisApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new SharpMapGisGuiPlugin(),
            };
            pluginsToAdd.AddRange(plugins);
            
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }

        private static IEnumerable<object> GenerateChildrenRecursively(IMapLayerProvider provider, object baseElement)
        {
            var results = new List<object>();
            var toGenerate = new Queue<object>();
            toGenerate.Enqueue(baseElement);

            while (toGenerate.Any())
            {
                object next = toGenerate.Dequeue();
                results.Add(next);

                provider.ChildLayerObjects(next).ForEach(toGenerate.Enqueue);
            }

            return results;
        }
    }
}