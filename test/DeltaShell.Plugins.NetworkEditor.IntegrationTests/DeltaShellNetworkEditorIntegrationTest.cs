using System;
using System.Linq;
using System.Threading;
using System.Windows;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.PropertyBag.Dynamic;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.LayerPropertiesEditor;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Layers;
using SharpMap.Rendering.Thematics;
using SharpTestsEx;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DeltaShellNetworkEditorIntegrationTest
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DeltaShellNetworkEditorIntegrationTest));

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowGatedWeirInPropertyGrid()
        {
            var mockrepos = new MockRepository();
            var guiMock = mockrepos.Stub<IGui>();

            var grid = new DeltaShell.Gui.Forms.PropertyGrid.PropertyGrid(guiMock)
                {
                    Data = new DynamicPropertyBag(new Gui.Forms.PropertyGrid.WeirProperties
                        {
                            Data = new Weir("gated")
                                {
                                    WeirFormula = new GatedWeirFormula(),
                                    ParentStructure = new CompositeBranchStructure()
                                }
                        })
                };

            WindowsFormsTestHelper.ShowModal(grid);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category("ToCheck")]
        public void CheckIfHydroNetworkEditorViewContextIsRestoredAfterViewIsClosed()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;

                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());

                gui.Run();

                var project = app.Project;

                // add data
                var network = new HydroNetwork();
                project.RootFolder.Add(network);

                // show gui main window

                // wait until gui starts
                Action mainWindowShown = delegate
                    {
                        LogHelper.SetLoggingLevel(Level.Debug);

                        var networkDataItem = project.RootFolder.DataItems.First();
                         gui.CommandHandler.OpenView(networkDataItem);

                         var networkEditor = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                        networkEditor.MapView.Map.Layers.Add(new GroupLayer { Name = "test group layer" });

                        // close view
                        gui.DocumentViews.Clear();


                        // reopening view should restore view context
                        gui.CommandHandler.OpenView(networkDataItem);

                        networkEditor = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();

                        networkEditor.MapView.Map.Layers.Count.Should(
                            "map should contain 2 layers, network and group layer (remembered as part of a view context)")
                            .Be.EqualTo(2);

                        // now remove network from the project
                        project.RootFolder.Items.Remove(networkDataItem);


                    };

                WpfTestHelper.ShowModal((Window)gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void PerformanceOfTableCellsSelectionShouldBeFast()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;

                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Run();

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
                        }
                                                         ));

                project.RootFolder.Add(network);

                // show gui main window

                // wait until gui starts
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

                WpfTestHelper.ShowModal((Window)gui.MainWindow, mainWindowShown);
            }
        }
        
        [Test]
        [Category(TestCategory.Jira)] //TOOLS-6594
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
        [Category(TestCategory.WindowsForms)]
        public void ShowThemeEditorForWeirLayer()
        {
            IHydroNetwork hydroNetwork = GetHydroNetworkWithPumpAndWeir();

            var weirLayer = new VectorLayer
                                {
                                    DataSource = new HydroNetworkFeatureCollection
                                                     {
                                                         Network = hydroNetwork,
                                                         FeatureType = typeof (Weir)
                                                     }
                                };
            var editor = new ThemeEditor {ThemeType = ThemeType.Categorial, Layer = weirLayer};
            WindowsFormsTestHelper.ShowModal(editor);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void OpenHydroNetworkEditorForDataItemThatWasUnlinked()
        {
            //replays http://issues/browse/TOOLS-2646
            using (var gui = new DeltaShellGui())
            {
                gui.Application.Plugins.Add(new CommonToolsApplicationPlugin());
                gui.Application.Plugins.Add(new NHibernateDaoApplicationPlugin());
                gui.Application.Plugins.Add(new SharpMapGisApplicationPlugin());
                gui.Application.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Application.Plugins.ForEach(p => p.Application = gui.Application);

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                //add a network to the project
                var project = gui.Application.Project;
                var hydroNetwork = new HydroNetwork();
                var dataItem = new DataItem(hydroNetwork);
                project.RootFolder.Add(dataItem);    

                //open view for the object
                gui.DocumentViewsResolver.OpenViewForData(dataItem.Value);
                //close all views
                gui.DocumentViews.Clear();
                
                //link unlink the DI
                dataItem.LinkTo(new DataItem(new HydroNetwork()));
                //unlink create a new value for the item..
                dataItem.Unlink();

                //open again a view for the network of the DI
                gui.DocumentViewsResolver.OpenViewForData(dataItem.Value);
            }
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

        private static IHydroNetwork GetHydroNetworkWithPumpAndWeir()
        {
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var compositeBranchStructure = new CompositeBranchStructure();
            var pump = new Pump("pump1") {OffsetY = 1000,StopDelivery = 18,StartDelivery = 12,StopSuction = 12,StartSuction = 15};
            var weir = new Weir("weri1"){CrestLevel = 15,CrestWidth = 50};
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, hydroNetwork.Branches[0], 50);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);
            return hydroNetwork;
        }

    }
}