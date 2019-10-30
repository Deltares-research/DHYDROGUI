using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DeltaShell.Gui;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpTestsEx;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class UndoRedoWaterFlowModel1DCentralMapIntegrationTest : UndoRedoCentralMapTestBase
    {
        private WaterFlowModel1D model;

        [SetUp]
        public void SetUp()
        {
            gui = new DeltaShellGui();
            var app = gui.Application;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());

            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());

            gui.Run();

            project = app.Project;

            // add data
            model = new WaterFlowModel1D();
            project.RootFolder.Add(model);

            // show gui main window
            mainWindow = (Window)gui.MainWindow;

            // wait until gui starts
            mainWindowShown = () =>
                {
                    network = model.Network;
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));

                    mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();

                    gui.UndoRedoManager.TrackChanges = true;

                    onMainWindowShown();
                };
        }

        [TearDown]
        public void TearDown()
        {
            gui.UndoRedoManager.TrackChanges = false;
            gui.Dispose();
            onMainWindowShown = null;
            mainWindowShown = null;
            LogHelper.ResetLogging();
        }

        [Test]
        public void BranchAdd()
        {
            onMainWindowShown = () =>
                {
                    AssertNumUndoableActions("before", 0);

                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });

                    Assert.AreEqual(2, model.BoundaryConditions.Count, "bcs after branch add");

                    AssertNumUndoableActions("after branch add", 1);

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(0, model.BoundaryConditions.Count, "bcs after undo");
                    AssertNetworkAsExpected("after undo", 0, 0);
                    AssertNumUndoableActions("after undo", 0);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(2, model.BoundaryConditions.Count, "bcs after redo");
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

                    DeleteFeature(network.Branches[0]);

                    AssertNetworkAsExpected("after delete", 2, 1);
                    AssertNumUndoableActions("after delete", 3);

                    gui.UndoRedoManager.Undo();

                    AssertNetworkAsExpected("after undo", 3, 2);

                    gui.UndoRedoManager.Redo();

                    AssertNetworkAsExpected("after redo", 2, 1);
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
        public void MergeBranchWithRoute()
        {
            onMainWindowShown = () =>
                {
                    // add branch with route
                    var branch1 = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var branch2 = AddBranchToNetwork(new[] { new Coordinate(50, 0), new Coordinate(100, 0) });
                    var route = HydroNetworkHelper.AddNewRouteToNetwork(network);
                    route.Locations.Values.Add(new NetworkLocation(branch1, 10));
                    route.Locations.Values.Add(new NetworkLocation(branch2, 40));

                    // custom message box
                    DelftTools.Controls.Swf.MessageBox.CustomMessageBox = new StubNoMessageBox();

                    // merge branch (is cancelled)
                    NetworkHelper.MergeNodeBranches(network.Nodes[1], network);
                    Assert.AreEqual(2, route.Segments.Values.Count);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        private class StubNoMessageBox : IMessageBox
        {
            public DialogResult Show(string text, string caption, MessageBoxButtons buttons)
            {
                return DialogResult.No;
            }
        }

        [Test]
        public void BranchReverse()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });

                    var initialWaterDepth = model.InitialConditions;
                    var loc1 = new NetworkLocation(branch, 10);
                    var coordinate = loc1.Geometry.Coordinate;
                    initialWaterDepth[loc1] = 5.0;
                    initialWaterDepth[new NetworkLocation(branch, 15)] = 1.0;

                    gui.UndoRedoManager.TrackChanges = true;

                    Assert.AreEqual(5, (double)initialWaterDepth.Evaluate(coordinate), "initial");

                    HydroNetworkHelper.ReverseBranch(branch);

                    Assert.AreEqual(5, (double)initialWaterDepth.Evaluate(coordinate), "after reverse");
                    AssertNumUndoableActions("after reverse", 1);

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(5, (double)initialWaterDepth.Evaluate(coordinate), "after undo");
                    AssertNumUndoableActions("after undo", 0);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(5, (double)initialWaterDepth.Evaluate(coordinate), "after redo");
                    AssertNumUndoableActions("after redo", 1);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BranchReverseMinimalCoverages()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });

                    foreach (var engineParam in model.OutputSettings.EngineParameters)
                    {
                        engineParam.AggregationOptions = AggregationOptions.None;
                    }

                    var initialWaterDepth = model.InitialConditions;
                    var loc1 = new NetworkLocation(branch, 10);
                    var coordinate = loc1.Geometry.Coordinate;
                    initialWaterDepth[loc1] = 5.0;
                    initialWaterDepth[new NetworkLocation(branch, 15)] = 1.0;

                    gui.UndoRedoManager.TrackChanges = true;

                    Assert.AreEqual(5, (double)initialWaterDepth.Evaluate(coordinate), "initial");

                    HydroNetworkHelper.ReverseBranch(branch);

                    Assert.AreEqual(5, (double)initialWaterDepth.Evaluate(coordinate), "after reverse");
                    AssertNumUndoableActions("after reverse", 1);

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(5, (double)initialWaterDepth.Evaluate(coordinate), "after undo");
                    AssertNumUndoableActions("after undo", 0);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(5, (double)initialWaterDepth.Evaluate(coordinate), "after redo");
                    AssertNumUndoableActions("after redo", 1);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void EditRoughnessFunction()
        {
            onMainWindowShown = () =>
                {
                    LogHelper.ConfigureLogging();
                    LogHelper.SetLoggingLevel(Level.Debug);

                    var branch = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });

                    var mainRoughness = model.RoughnessSections[0].RoughnessNetworkCoverage;

                    mainRoughness.Locations.Values.Add(new NetworkLocation(branch, 15));

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(0, mainRoughness.Locations.Values.Count);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(1, mainRoughness.Locations.Values.Count);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void BranchMerge()
        {
            onMainWindowShown = () =>
                {
                    LogHelper.ConfigureLogging();
                    LogHelper.SetLoggingLevel(Level.Debug);

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
        public void ChangeBoundaryDischargeValue()
        {
            onMainWindowShown = () =>
                {
                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var boundaryCondition = model.BoundaryConditions[0];

                    //change boundary type to q_constant
                    boundaryCondition.DataType = Model1DBoundaryNodeDataType.FlowConstant;

                    //set q_const to value
                    boundaryCondition.Flow = 5.0;

                    gui.UndoRedoManager.Undo();

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(5.0, boundaryCondition.Flow);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ChangeBoundaryDischargeFunction()
        {
            onMainWindowShown = () =>
                {
                    var t0 = new DateTime(2000, 1, 1);
                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(50, 0) });
                    var boundaryCondition = model.BoundaryConditions[0];

                    //change boundary type to q_constant
                    boundaryCondition.DataType = Model1DBoundaryNodeDataType.FlowTimeSeries;

                    //set q_const to value
                    boundaryCondition.Data[t0] = 5.0;

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(0, boundaryCondition.Data.Arguments[0].Values.Count);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(5.0, boundaryCondition.Data[t0]);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ChangeCulvertToSiphon()
        {
            onMainWindowShown = () =>
                {
                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });
                    var culvert = AddCulvert(new Coordinate(50, 0));

                    culvert.CulvertType = CulvertType.Siphon;

                    gui.UndoRedoManager.Undo(); //threw exception
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void AddModelFromScratch()
        {
            onMainWindowShown = () =>
                {
                    var model = new WaterFlowModel1D();

                    LogHelper.ConfigureLogging(Level.Debug);

                    // action!
                    project.BeginEdit("Add model: " + model);
                    project.RootFolder.Items.Add(model);
                    project.EndEdit();

                    gui.UndoRedoManager.UndoStack.Count()
                       .Should("only add model memento added to undo stack").Be.EqualTo(1);

                    gui.UndoRedoManager.Undo();

                    project.RootFolder.Items.Contains(model)
                           .Should("model is removed after undo").Be.False();
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ModifyModelStartTime()
        {
            onMainWindowShown = () =>
                {
                    model.StartTime = new DateTime(2000, 1, 1);

                    gui.UndoRedoManager.UndoStack.Count()
                       .Should("only add model memento added to undo stack").Be.EqualTo(1);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test] //todo: move to ProjectUndoRedoTests?
        public void UndoItemMoveAndChangeIt()
        {
            var folder = new Folder();
            project.RootFolder.Add(folder);
            var url = new Url();
            var urlDataItem = new DataItem(url);
            project.RootFolder.Add(urlDataItem);

            onMainWindowShown = () =>
                {
                    // action!
                    project.BeginEdit("Move url to folder: " + url);
                    project.RootFolder.Items.Remove(urlDataItem);
                    folder.Items.Add(urlDataItem);
                    project.EndEdit();

                    gui.UndoRedoManager.UndoStack.Count()
                       .Should("only move url memento added to undo stack").Be.EqualTo(1);

                    gui.UndoRedoManager.Undo();

                    project.RootFolder.Items.Contains(urlDataItem)
                           .Should("url should be present again in old location after undo").Be.True();

                    gui.UndoRedoManager.Redo();

                    folder.Items.Contains(urlDataItem)
                          .Should("url should be present again in new location after redo").Be.True();

                    url.Name = "newName";
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void UndoUnlinkNetworkFromModelAndCheckSync()
        {
            // add external network
            var externalNetwork = new HydroNetwork();
            project.RootFolder.Add(externalNetwork);

            // link model to external data item
            var modelNetworkDataItem = model.DataItems.First(di => di.ValueType == typeof(HydroNetwork));
            var dataItem = project.RootFolder.DataItems.First();
            modelNetworkDataItem.LinkTo(dataItem);

            onMainWindowShown = () =>
                {
                    // add branch & lateral
                    AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });
                    AddLateral(new Coordinate(50, 0));

                    Assert.AreEqual(1, model.LateralSourceData.Count, "lateral data initial");

                    // unlink: network goes back to empty
                    model.BeginEdit("Unlinking");
                    modelNetworkDataItem.Unlink();
                    model.EndEdit();

                    Assert.AreEqual(0, model.LateralSourceData.Count, "lateral data when unlinked");

                    // undo unlink
                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(1, model.LateralSourceData.Count, "lateral data after undo");

                    // open network editor again
                    gui.CommandHandler.OpenView(dataItem, typeof(ProjectItemMapView));
                    mapView = gui.DocumentViews.OfType<ProjectItemMapView>().Last();

                    // add lateral to network
                    AddLateral(new Coordinate(80, 0));

                    Assert.AreEqual(2, model.LateralSourceData.Count, "lateral data after adding another lateral");
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void UndoMergingBranchesAfterImportOfRoughnessShouldNotCrash_JIRA9059()
        {
            onMainWindowShown = () =>
                {
                    var channel = AddBranchToNetwork(new[] { new Coordinate(0, 0), new Coordinate(100, 0) });
                    channel.IsLengthCustom = true;
                    channel.Length = 1000;

                    gui.Selection = model;
                    gui.DocumentViewsResolver.OpenViewForData(model.RoughnessSections.First());

                    model.RoughnessSections[0].RoughnessNetworkCoverage[new NetworkLocation(channel, 250)]
                        = new object[] { 1, RoughnessType.Chezy };

                    var node1 = HydroNetworkHelper.SplitChannelAtNode(network.Branches.Last() as IChannel, new Coordinate(50, 0));

                    var customMessageBox = MessageBox.CustomMessageBox;
                    MessageBox.CustomMessageBox = new StubYesMessageBox();
                    NetworkHelper.MergeNodeBranches(node1, network);
                    MessageBox.CustomMessageBox = customMessageBox;

                    gui.UndoRedoManager.Undo();
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        private class StubYesMessageBox : IMessageBox
        {
            public DialogResult Show(string text, string caption, MessageBoxButtons buttons)
            {
                return DialogResult.Yes;
            }
        }
    }
}