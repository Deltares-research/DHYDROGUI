using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Layers;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests
{
    [TestFixture]
    public class DeltaShellNetworkEditorIntegrationTest
    {
        [Test]
        [Category(TestCategory.Performance)]
        public void PerformanceOfTableCellsSelectionShouldBeFast()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),                
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;

                
                gui.Run();

                app.CreateNewProject();

                var project = app.Project;

                // add data
                var network = new HydroNetwork
                                  {
                                      Nodes = new EventedList<INode>
                                                  {
                                                      new HydroNode {Name = "node1", Geometry = new Point(0, 0)},
                                                      new HydroNode {Name = "node2", Geometry = new Point(1, 1)}
                                                  }
                                  };

                var points = new [] {new Coordinate(0, 0), new Coordinate(1, 1)};

                Enumerable.Range(1, 100).ForEach(i => network.Branches.Add(
                    new Channel
                        {
                            Name = "channel" + i,
                            Source = network.Nodes[0],
                            Target = network.Nodes[1],
                            Geometry = new LineString(points)
                        }));

                project.RootFolder.Add(network);
                
                Action mainWindowShown = delegate
                                        {
                                            var networkDataItem = project.RootFolder.DataItems.First(di => di.Value is HydroNetwork);
                                            gui.CommandHandler.OpenView(networkDataItem, typeof(ProjectItemMapView));

                                            var networkEditor = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                                            var channelLayer = networkEditor.MapView.GetLayerForData(network.Channels);
                                            networkEditor.MapView.OpenLayerAttributeTable(channelLayer);

                                            var channelTableView = (VectorLayerAttributeTableView)networkEditor.MapView.TabControl.ChildViews.First();

                                            TestHelper.AssertIsFasterThan(3000, "time required to select 20 table view cells, synchronized with map and tree view",() => 
                                                channelTableView.TableView.SelectCells(0, 0, 99, 1));
                                        };

                // show gui main window
                WpfTestHelper.ShowModal((Window)gui.MainWindow, mainWindowShown);
            }
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void DeleteLocationFromCoverageWithoutSegmentLayerDoesNotCauseCrash()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);

            INetworkCoverage networkCoverage = new NetworkCoverage { Network = network };

            // set values
            var location = new NetworkLocation(network.Branches[0], 0.0);
            networkCoverage[location] = 0.1;
            networkCoverage[new NetworkLocation(network.Branches[0], 100.0)] = 0.2;
            networkCoverage[new NetworkLocation(network.Branches[1], 0.0)] = 0.3;
            networkCoverage[new NetworkLocation(network.Branches[1], 50.0)] = 0.4;
            networkCoverage[new NetworkLocation(network.Branches[1], 200.0)] = 0.5;

            var coverageView = new CoverageView { Data = networkCoverage };

            var mapView = coverageView.ChildViews.OfType<MapView>().First();
            var networkCoverageLayer = mapView.Map.GetAllLayers(true).OfType<NetworkCoverageGroupLayer>().First();

            //remove segment layer
            networkCoverageLayer.LayersReadOnly = false;
            networkCoverageLayer.Layers.Remove(networkCoverageLayer.SegmentLayer);
            networkCoverageLayer.LayersReadOnly = true;

            var mapControl = mapView.MapControl;
            mapControl.Visible = false; //prevent rendering

            var hydroNetworkEditorMapTool = new HydroRegionEditorMapTool
            {
                IsActive = true
            };
            mapControl.Tools.Add(hydroNetworkEditorMapTool);

            hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer = networkCoverageLayer;

            //remove a location
            mapControl.SelectTool.Select(networkCoverage.Locations.Values.First());
            mapControl.DeleteTool.DeleteSelection();

            coverageView.Dispose();
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void DeleteBranchShouldRemoveBoundaryNodes()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0), new Point(100, 100));
            network.Branches.Remove(network.Branches[0]);
            NetworkHelper.RemoveUnusedNodes(network);
            Assert.AreEqual(2, network.HydroNodes.Count(n => !n.IsConnectedToMultipleBranches));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GridEditorPluginDeployment()
        {
            string pluginsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "plugins");
            string gridEditorDllPath = Path.Combine(pluginsDir, "DeltaShell.Plugins.GridEditor", "DeltaShell.Plugins.GridEditor.dll");
            string gridEditorGuiDllPath = Path.Combine(pluginsDir, "DeltaShell.Plugins.GridEditor.Gui", "DeltaShell.Plugins.GridEditor.Gui.dll");
            
            Assert.That(gridEditorDllPath, Does.Exist);
            Assert.That(gridEditorGuiDllPath, Does.Exist);
        }
    }
}