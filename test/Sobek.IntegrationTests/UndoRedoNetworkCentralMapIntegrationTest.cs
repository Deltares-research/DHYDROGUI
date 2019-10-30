using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils.Aop;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMap.Layers;
using Point = NetTopologySuite.Geometries.Point;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    [Category(TestCategory.UndoRedo)]
    public class UndoRedoNetworkCentralMapIntegrationTest : UndoRedoCentralMapTestBase
    {
        [SetUp]
        public void SetUp()
        {
            gui = new DeltaShellGui();
            var app = gui.Application;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());

            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());


            gui.Run();

            project = app.Project;

            // add data

            network = new HydroNetwork();
            basin = new DrainageBasin();
            region = new HydroRegion { Name = "hr", SubRegions = { network, basin } };

            project.RootFolder.Add(region);

            // show gui main window
            mainWindow = (Window)gui.MainWindow;

            // wait until gui starts
            mainWindowShown = () =>
                {
                    var regionDataItem = project.RootFolder.DataItems.First();
                    gui.CommandHandler.OpenView(regionDataItem, typeof(ProjectItemMapView));

                    mapView = gui.DocumentViews.OfType<ProjectItemMapView>().First();

                    gui.UndoRedoManager.TrackChanges = true;

                    onMainWindowShown();
                };
        }

        [TearDown]
        public void TearDown()
        {
            LogHelper.ResetLogging();
            gui.Dispose();
            onMainWindowShown = null;
            mainWindowShown = null;
            project = null;
            basin = null;
            network = null;
            region = null;
            mainWindow = null;
            GC.Collect();
        }

        [Test]
        public void CreateRunoffLink()
        {
            var catchment = new Catchment { IsGeometryDerivedFromAreaSize = true, Geometry = new Point(0, 0) };
            catchment.SetAreaSize(1000);
            basin.Catchments.Add(catchment);

            onMainWindowShown = () =>
                {
                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });

                    var node = network.HydroNodes.First();

                    catchment.LinkTo(node);

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(0, node.Links.Count);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(1, node.Links.Count);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BasinRemove()
        {
            onMainWindowShown = () =>
                {
                    var drainageBasin = region.SubRegions.First(sr => sr is DrainageBasin);
                    region.SubRegions.Remove(drainageBasin);

                    Assert.IsFalse(region.SubRegions.Contains(drainageBasin));
                    gui.UndoRedoManager.Undo();
                    Assert.IsTrue(region.SubRegions.Contains(drainageBasin));
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BranchAdd()
        {
            onMainWindowShown = () =>
                {
                    AssertNumUndoableActions("before", 0);

                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });

                    AssertNumUndoableActions("after branch add", 1);

                    gui.UndoRedoManager.Undo();

                    AssertNetworkAsExpected("after undo", 0, 0);
                    AssertNumUndoableActions("after undo", 0);

                    gui.UndoRedoManager.Redo();

                    AssertNetworkAsExpected("after redo", 2, 1);
                    AssertNumUndoableActions("at end", 1);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BranchRemove()
        {
            onMainWindowShown = () =>
                {
                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    AddBranchToNetwork(new[] { new Coordinate(50, 0), new Coordinate(100, 0) });

                    AssertNetworkAsExpected("initial", 3, 2);
                    AssertNumUndoableActions("initial", 2);

                    DeleteFeature(network.Branches[1]); //delete 2nd branch

                    AssertNetworkAsExpected("after delete", 2, 1);
                    AssertNumUndoableActions("after delete", 3);

                    gui.UndoRedoManager.Undo();

                    AssertNetworkAsExpected("after undo", 3, 2);
                    Assert.AreEqual(1, network.Nodes[0].OutgoingBranches.Count, "out n0");
                    Assert.AreEqual(1, network.Nodes[1].IncomingBranches.Count, "in n1");
                    Assert.AreEqual(1, network.Nodes[1].OutgoingBranches.Count, "out n1");
                    Assert.AreEqual(1, network.Nodes[2].IncomingBranches.Count, "in n2");

                    gui.UndoRedoManager.Redo();

                    AssertNetworkAsExpected("after redo", 2, 1);
                    Assert.AreEqual(1, network.Nodes[0].OutgoingBranches.Count, "redone: out n0");
                    Assert.AreEqual(1, network.Nodes[1].IncomingBranches.Count, "redone: in n1");
                    Assert.AreEqual(0, network.Nodes[1].OutgoingBranches.Count, "redone: out n1");
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BranchSplit()
        {
            onMainWindowShown = () =>
                {
                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });

                    HydroNetworkHelper.SplitChannelAtNode(branch, new Coordinate(50, 0));

                    AssertNetworkAsExpected("after split", 3, 2);
                    AssertNumUndoableActions("after split", 2);

                    gui.UndoRedoManager.Undo();

                    AssertNetworkAsExpected("after undo", 2, 1);
                    AssertNumUndoableActions("after undo", 1);

                    gui.UndoRedoManager.Redo();

                    AssertNetworkAsExpected("after redo", 3, 2);
                    AssertNumUndoableActions("at end", 2);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BranchSplitBetweenStructures()
        {
            onMainWindowShown = () =>
                {
                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });

                    NetworkHelper.AddBranchFeatureToBranch(new CrossSection(CrossSectionDefinitionYZ.CreateDefault()),
                                                           branch, 0.25 * branch.Length);

                    var compositeBranchStructure = new CompositeBranchStructure("weir", 0);
                    compositeBranchStructure.Structures.Add(new Weir());

                    NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, branch, 0.75 * branch.Length);

                    NetworkHelper.SplitBranchAtNode(branch, 0.5 * branch.Length);

                    AssertNetworkAsExpected("after split", 3, 2);
                    AssertNumUndoableActions("after split", 4);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BranchSplitTwiceWithRoute()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });
                    var route = HydroNetworkHelper.AddNewRouteToNetwork(network);
                    route.Locations.Values.Add(new NetworkLocation(branch, 10));
                    route.Locations.Values.Add(new NetworkLocation(branch, 90));

                    gui.UndoRedoManager.TrackChanges = true;

                    HydroNetworkHelper.SplitChannelAtNode(branch, new Coordinate(50, 0));
                    HydroNetworkHelper.SplitChannelAtNode(branch, new Coordinate(30, 0));

                    AssertNetworkAsExpected("after split", 4, 3);
                    AssertNumUndoableActions("after split", 2);

                    gui.UndoRedoManager.Undo();
                    gui.UndoRedoManager.Undo();

                    AssertNetworkAsExpected("after undo", 2, 1);
                    AssertNumUndoableActions("after undo", 0);

                    gui.UndoRedoManager.Redo();
                    gui.UndoRedoManager.Redo();

                    AssertNetworkAsExpected("after redo", 4, 3);
                    AssertNumUndoableActions("at end", 2);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void NetworkCoverageDeleteBranchesAndUndo()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    // set up network coverage
                    var branch1 = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(500, 0) });
                    var branch2 = AddBranchToNetwork(new[] { new Coordinate(500, 0), new Coordinate(1000, 0) });
                    var networkCoverage = new NetworkCoverage("test", false) { Network = network };
                    networkCoverage[new NetworkLocation(branch1, 100)] = 1.0;
                    networkCoverage[new NetworkLocation(branch1, 400)] = 2.0;
                    networkCoverage[new NetworkLocation(branch2, 100)] = 3.0;
                    networkCoverage[new NetworkLocation(branch2, 400)] = 4.0;
                    gui.Application.Project.RootFolder.Add(networkCoverage);

                    gui.UndoRedoManager.TrackChanges = true;

                    // action (delete branch)
                    DeleteFeature(branch1);

                    // undo, redo, and undo (to spice things up a bit)
                    AssertNumUndoableActions("after delete", 1);
                    gui.UndoRedoManager.Undo();
                    gui.UndoRedoManager.Redo();
                    gui.UndoRedoManager.Undo();

                    // make sure the order of locations is unchanged
                    Assert.AreEqual(4, networkCoverage.Locations.Values.Count);
                    Assert.AreEqual(branch1, networkCoverage.Locations.Values[0].Branch);
                    Assert.AreEqual(100, networkCoverage.Locations.Values[0].Chainage);
                    Assert.AreEqual(3.0, networkCoverage[new NetworkLocation(branch2, 100)]);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BranchesWithRouteUndoDeleteBranches()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    var branch1 = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(500, 0) });
                    var branch2 = AddBranchToNetwork(new[] { new Coordinate(500, 0), new Coordinate(1000, 0) });
                    var route = HydroNetworkHelper.AddNewRouteToNetwork(network);
                    route.Locations.Values.Add(new NetworkLocation(branch1, 100));
                    route.Locations.Values.Add(new NetworkLocation(branch1, 400));
                    route.Locations.Values.Add(new NetworkLocation(branch2, 100));
                    route.Locations.Values.Add(new NetworkLocation(branch2, 400));

                    gui.UndoRedoManager.TrackChanges = true;

                    // action
                    DeleteFeature(branch1);
                    DeleteFeature(branch2);

                    // undo
                    AssertNumUndoableActions("after delete", 2);
                    gui.UndoRedoManager.Undo();
                    gui.UndoRedoManager.Undo();

                    // assert route is still valid
                    Assert.IsFalse(RouteHelper.RouteContainLoops(route), "Route should not contain loops");
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void UndoAddRouteWhileSideViewOpen()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    var branch1 = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(500, 0) });

                    gui.UndoRedoManager.TrackChanges = true;

                    var route = HydroNetworkHelper.AddNewRouteToNetwork(network);
                    route.Locations.Values.Add(new NetworkLocation(branch1, 100));
                    route.Locations.Values.Add(new NetworkLocation(branch1, 400));

                    // open side view
                    gui.CommandHandler.OpenView(route, typeof(NetworkSideView));

                    // undo add route
                    AssertNumUndoableActions("after delete", 3);
                    gui.UndoRedoManager.Undo();
                    gui.UndoRedoManager.Undo();
                    gui.UndoRedoManager.Undo();
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void MoveNodeThroughPropertyGrid()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    var branch1 = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });

                    var centralNode = branch1.Target as HydroNode;
                    var propertiesOfCentralNode = new HydroNodeProperties { Data = centralNode };
                    gui.UndoRedoManager.TrackChanges = true;

                    propertiesOfCentralNode.X = 25;

                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void CheckingIfRouteIsValidShouldNotFailInGuiDueToDisabledSideEffectsDuringUndoRedoTools7423()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    // add branch + bridge
                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var bridge = AddBridge(new Coordinate(25, 0));

                    // open view for bridge (with small route + sideview)
                    gui.CommandHandler.OpenView(bridge);

                    gui.UndoRedoManager.TrackChanges = true;

                    // trigger collection change action not in a BeginEdit/EndEdit which when undone triggers a sideview update
                    network.SharedCrossSectionDefinitions.Add(new CrossSectionDefinitionYZ("test"));

                    gui.UndoRedoManager.Undo();
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void CheckingIfRouteIsValidShouldNotFailDueToDisabledSideEffectsDuringUndoRedoTools7441()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var route = HydroNetworkHelper.AddNewRouteToNetwork(network);
                    route.Locations.Values.Add(new NetworkLocation(branch, 10));
                    route.Locations.Values.Add(new NetworkLocation(branch, 40));

                    try
                    {
                        // fake undo/redo being in progress
                        EditActionSettings.Disabled = true;

                        // assert IsDisconnected works fine
                        Assert.IsFalse(RouteHelper.IsDisconnected(route), "RouteHelper is relying on side-effects");
                    }
                    finally
                    {
                        EditActionSettings.Disabled = false;
                    }
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BranchMerge()
        {
            onMainWindowShown = () =>
                {
                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    AddBranchToNetwork(new[] { new Coordinate(50, 0), new Coordinate(100, 0) });

                    AssertNetworkAsExpected("initial", 3, 2);
                    AssertNumUndoableActions("initial", 2);

                    var node2 = network.HydroNodes.Skip(1).First();
                    NetworkHelper.MergeNodeBranches(node2, network);

                    AssertNetworkAsExpected("after merge", 2, 1);
                    AssertNumUndoableActions("after merge", 3);

                    gui.UndoRedoManager.Undo();

                    AssertNetworkAsExpected("after undo", 3, 2);
                    AssertNumUndoableActions("after undo", 2);

                    gui.UndoRedoManager.Redo();

                    AssertNetworkAsExpected("after redo", 2, 1);
                    AssertNumUndoableActions("at end", 3);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BranchRemoveWithFeature()
        {
            onMainWindowShown = () =>
                {
                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var bridge = AddBridge(new Coordinate(25, 0));

                    AssertNetworkAsExpected("initial", 2, 1, 1);
                    AssertNumUndoableActions("initial", 2);

                    DeleteFeature(network.Branches[0]);

                    AssertNetworkAsExpected("after delete", 0, 0, 0);
                    AssertNumUndoableActions("after delete", 3);

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(branch, bridge.Branch);

                    AssertNetworkAsExpected("after undo", 2, 1, 1);

                    gui.UndoRedoManager.Redo();

                    AssertNetworkAsExpected("after redo", 0, 0, 0);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BranchFeatureRemove()
        {
            onMainWindowShown = () =>
                {
                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var bridge = AddBridge(new Coordinate(25, 0));

                    AssertNetworkAsExpected("initial", 2, 1, 1);
                    AssertNumUndoableActions("initial", 2);

                    DeleteFeature(network.Bridges.ElementAt(0));

                    AssertNetworkAsExpected("after delete", 2, 1, 0);
                    AssertNumUndoableActions("after delete", 3);

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(branch, bridge.Branch);

                    AssertNetworkAsExpected("after undo", 2, 1, 1);

                    gui.UndoRedoManager.Redo();

                    AssertNetworkAsExpected("after redo", 2, 1, 0);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void LateralRemove()
        {
            onMainWindowShown = () =>
                {
                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var lateral = AddLateral(new Coordinate(25, 0));

                    AssertNumUndoableActions("initial", 2);

                    DeleteFeature(network.LateralSources.ElementAt(0));

                    AssertNumUndoableActions("after delete", 3);

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(branch, lateral.Branch);
                    Assert.AreEqual(1, network.LateralSources.Count());

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(0, network.LateralSources.Count());
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BranchFeatureMove()
        {
            onMainWindowShown = () =>
                {
                    // LogHelper.ConfigureLogging(Level.Debug);

                    var branch1 = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var branch2 = AddBranchToNetwork(new[] { new Coordinate(50, 0), new Coordinate(100, 0) });
                    var bridge = AddBridge(new Coordinate(25, 0));
                    var geometryInitial = bridge.Geometry.Clone();

                    AssertNetworkAsExpected("initial", 3, 2, 1);
                    AssertNumUndoableActions("initial", 3);
                    Assert.AreEqual(branch1, bridge.Branch, "initial");

                    // zoom to extents before moving objects, otherwise snapping logic will fail
                    mapView.MapView.Map.ZoomToExtents();

                    MoveFeature(network.Bridges.ElementAt(0), new Coordinate(75, 0));

                    AssertNetworkAsExpected("after move", 3, 2, 1);
                    AssertNumUndoableActions("after move", 4);
                    Assert.AreEqual(branch2, bridge.Branch, "after move");
                    var geometryAfterMove = bridge.Geometry.Clone();

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(geometryInitial, bridge.Geometry, "after undo");
                    AssertNetworkAsExpected("after undo", 3, 2, 1);

                    Assert.AreEqual(branch1, bridge.Branch, "after undo");

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(geometryAfterMove, bridge.Geometry, "after redo");
                    AssertNetworkAsExpected("after redo", 3, 2, 1);
                    Assert.AreEqual(branch2, bridge.Branch, "after redo");
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void CrossSectionRename()
        {
            onMainWindowShown = () =>
                {
                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var cs = CrossSection.CreateDefault();
                    cs.Branch = branch;
                    cs.Chainage = 10;
                    branch.BranchFeatures.Add(cs);
                    cs.Name = "oldName";

                    cs.Name = "newName";

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual("oldName", cs.Name);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual("newName", cs.Name);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void CrossSectionDefaultDefinitionRename()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    // create branch with cross section
                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var cs = CrossSection.CreateDefault();
                    cs.Branch = branch;
                    cs.Chainage = 10;
                    branch.BranchFeatures.Add(cs);
                    cs.Name = "oldName";

                    gui.UndoRedoManager.TrackChanges = true;

                    // share cs definition
                    cs.ShareDefinitionAndChangeToProxy();
                    var sharedDefinition = network.SharedCrossSectionDefinitions.First();

                    // set as default in network
                    network.DefaultCrossSectionDefinition = sharedDefinition;

                    sharedDefinition.Name = "newName";

                    gui.UndoRedoManager.Undo();
                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual("oldName", sharedDefinition.Name);

                    gui.UndoRedoManager.Redo();
                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual("newName", sharedDefinition.Name);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void CrossSectionDefinitionRename()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    // create branch with cross section
                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var cs = CrossSection.CreateDefault();
                    cs.Branch = branch;
                    cs.Chainage = 10;
                    branch.BranchFeatures.Add(cs);
                    cs.Name = "oldName";

                    gui.UndoRedoManager.TrackChanges = true;

                    cs.ShareDefinitionAndChangeToProxy(); // share cs definition
                    var sharedDefinition = network.SharedCrossSectionDefinitions.First();

                    sharedDefinition.Name = "newName";

                    gui.UndoRedoManager.Undo();
                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual("oldName", sharedDefinition.Name);

                    gui.UndoRedoManager.Redo();
                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual("newName", sharedDefinition.Name);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void CrossSectionUndoShareDefinition()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    // create branch with cross section
                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var cs = CrossSection.CreateDefault();
                    cs.Branch = branch;
                    cs.Chainage = 10;
                    branch.BranchFeatures.Add(cs);
                    cs.Name = "oldName";

                    gui.UndoRedoManager.TrackChanges = true;

                    cs.ShareDefinitionAndChangeToProxy(); // share cs definition

                    gui.UndoRedoManager.Undo();

                    Assert.IsFalse(cs.Definition.IsProxy);
                    Assert.AreEqual(0, network.SharedCrossSectionDefinitions.Count);

                    gui.UndoRedoManager.Redo();

                    Assert.IsTrue(cs.Definition.IsProxy);
                    Assert.AreEqual(1, network.SharedCrossSectionDefinitions.Count);

                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }
        
        [Test]
        public void CrossSectionUndoChangesToStandardCrossSection()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    // create branch with cross section
                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var cs = CrossSection.CreateDefault(CrossSectionType.Standard, branch);
                    cs.Branch = branch;
                    cs.Chainage = 10;
                    branch.BranchFeatures.Add(cs);
                    cs.Name = "oldName";

                    gui.UndoRedoManager.TrackChanges = true;

                    cs.ShareDefinitionAndChangeToProxy(); // share cs definition
                    var sharedDefinition = (CrossSectionDefinitionStandard)network.SharedCrossSectionDefinitions.First();
                    sharedDefinition.ShapeType = CrossSectionStandardShapeType.Trapezium;
                    sharedDefinition.ShapeType = CrossSectionStandardShapeType.SteelCunette;

                    while (gui.UndoRedoManager.CanUndo)
                    {
                        gui.UndoRedoManager.Undo();
                    }

                    while (gui.UndoRedoManager.CanRedo)
                    {
                        gui.UndoRedoManager.Redo();
                    }
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void CrossSectionYZMove()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    var branch1 = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var branch2 = AddBranchToNetwork(new[] { new Coordinate(50, 0), new Coordinate(100, 0) });
                    var cs = AddCrossSection(new Coordinate(25, 0), CrossSection.CreateDefault(CrossSectionType.YZ, null));
                    var geometryInitial = cs.Geometry.Clone();

                    gui.UndoRedoManager.TrackChanges = true;

                    AssertNumUndoableActions("initial", 0);
                    Assert.AreEqual(branch1, cs.Branch, "initial");

                    // zoom to extents before moving objects, otherwise snapping logic will fail
                    mapView.MapView.Map.ZoomToExtents();

                    MoveFeature(network.CrossSections.ElementAt(0), new Coordinate(80, 0));

                    AssertNumUndoableActions("after move", 1);
                    Assert.AreEqual(branch2, cs.Branch, "after move");
                    Assert.AreEqual(30, cs.Chainage, "after move");
                    var geometryAfterMove = cs.Geometry.Clone();

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(25, cs.Chainage, "after undo");
                    Assert.AreEqual(branch1, cs.Branch, "after undo");
                    Assert.AreEqual(geometryInitial, cs.Geometry, "after undo");

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(geometryAfterMove, cs.Geometry, "after redo");
                    Assert.AreEqual(30, cs.Chainage, "after redo");
                    Assert.AreEqual(branch2, cs.Branch, "after redo");
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void UndoAddRoute()
        {
            onMainWindowShown = () =>
                {
                    var route = HydroNetworkHelper.AddNewRouteToNetwork(network);

                    AssertNumUndoableActions("before undo", 1);

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(0, network.Routes.Count);
                    Assert.AreEqual(0, mapView.MapView.Map.GetAllLayers(true).OfType<NetworkCoverageGroupLayer>().Count(
                                                                                                                               l => ReferenceEquals(l.Coverage, route)));
                    AssertNumUndoableActions("before redo", 0);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(1, network.Routes.Count);
                    Assert.AreEqual(network, network.Routes[0].Network);
                    Assert.AreEqual(1, mapView.MapView.Map.GetAllLayers(true).OfType<NetworkCoverageGroupLayer>().Count(
                                                                                                                               l => ReferenceEquals(l.Coverage, route)));
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void UndoDeleteRoute()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    var route = HydroNetworkHelper.AddNewRouteToNetwork(network);

                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    route.Locations.AddValues(new[]
                                                  {
                                                      new NetworkLocation(network.Branches[0], 5),
                                                      new NetworkLocation(network.Branches[0], 15),
                                                      new NetworkLocation(network.Branches[0], 25),
                                                  });

                    gui.UndoRedoManager.TrackChanges = true;
                    network.Routes.Remove(route);

                    AssertNumUndoableActions("before undo", 1);

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(1, network.Routes.Count);
                    Assert.AreEqual(network, network.Routes[0].Network);
                    Assert.AreEqual(2, route.Segments.Values.Count);
                    Assert.AreEqual(1, mapView.MapView.Map.GetAllLayers(true).OfType<NetworkCoverageGroupLayer>().Count(
                                                                                                                               l => ReferenceEquals(l.Coverage, route)));
                    AssertNumUndoableActions("before redo", 0);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(0, network.Routes.Count);
                    Assert.AreEqual(0, mapView.MapView.Map.GetAllLayers(true).OfType<NetworkCoverageGroupLayer>().Count(
                                                                                                                               l => ReferenceEquals(l.Coverage, route)));
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ComplexBranchAdd()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;
                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    AddBranchToNetwork(new[] { new Coordinate(50, 0), new Coordinate(100, 0) });
                    AddBranchToNetwork(new[] { new Coordinate(50, 0), new Coordinate(50, 50) });
                    gui.UndoRedoManager.TrackChanges = true;

                    AddBranchToNetwork(new[] { new Coordinate(50, 0), new Coordinate(50, -50) });

                    AssertNetworkAsExpected("after branch add", 5, 4);
                    AssertNumUndoableActions("after branch add", 1);

                    gui.UndoRedoManager.Undo();

                    AssertNetworkAsExpected("after undo", 4, 3);
                    AssertNumUndoableActions("after undo", 0);

                    gui.UndoRedoManager.Redo();

                    AssertNetworkAsExpected("after redo", 5, 4);
                    AssertNumUndoableActions("at end", 1);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        [Category(TestCategory.WorkInProgress)] // Add view context to central map
        public void AddWmsLayer()
        {
            onMainWindowShown = () =>
                {
                    /* 
                    mapView.MapView.Map.Layers.Add(new OpenStreetMapLayer());

                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(0, mapView.MapView.Map.Layers.OfType<OpenStreetMapLayer>().Count());

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(1, mapView.MapView.Map.Layers.OfType<OpenStreetMapLayer>().Count());*/
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test] //TOOLS-7056
        public void HashOverflowWhileEditingWeir()
        {
            onMainWindowShown = () =>
                {
                    var channel =
                        AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var weir = new Weir();
                    channel.BranchFeatures.Add(weir);
                    var weirFormula = new GeneralStructureWeirFormula();
                    weir.WeirFormula = weirFormula;
                    weirFormula.GateOpening = 15;
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        private class StubMessageBox : IMessageBox
        {
            public DialogResult Show(string text, string caption, MessageBoxButtons buttons)
            {
                return DialogResult.OK;
            }
        }
    }
}