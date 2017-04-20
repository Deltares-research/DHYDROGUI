using System;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;
using SharpTestsEx;
using Control = System.Windows.Controls.Control;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class WaterFlowModel1DNodePresenterIntegrationTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void TreeNodePointsToCorrectTagAfterLinkAndRename()
        {
            using(var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());

                
                gui.Run();

                Action mainWindowShown = delegate {
                        var network = new HydroNetwork { Name = "network" };
                        var model = new WaterFlowModel1D { Name = "model" };

                        var project = app.Project;
                        project.RootFolder.Add(network);
                        project.RootFolder.Add(model);

                        // link
                        var networkDataItem = (IDataItem)project.RootFolder["network"];
                        model.GetDataItemByValue(model.Network).LinkTo(networkDataItem);

                        // rename network using tree node
                        var projectExplorer = gui.ToolWindowViews.OfType<DeltaShell.Plugins.ProjectExplorer.ProjectExplorer>().First();
                        var treeView = projectExplorer.TreeView;

                        treeView.WaitUntilAllEventsAreProcessed(); // nodes for model and network should be added first

                        gui.Selection = networkDataItem;

                        treeView.Refresh();
                        treeView.WaitUntilAllEventsAreProcessed(); // nodes for model and network should be added first

                        var networkNode = treeView.SelectedNode;

                        // simulate node edit
                        var e = new NodeLabelEditEventArgs((TreeNode)networkNode, "network2");
                        TypeUtils.CallPrivateMethod(treeView, "OnAfterLabelEdit", e);

                        treeView.Refresh();
                        treeView.WaitUntilAllEventsAreProcessed();
                        treeView.SelectedNode.Text.Should().Be.EqualTo("network2");
                    };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.WorkInProgress)]
        [Category(TestCategory.Slow)]
        public void TreeNodeForBoundaryConditionIsNotUpdatedAfterCloneAndUnlink()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
                gui.Run();

                Action mainWindowShown = delegate
                {
                    // add:
                    //
                    // flow time series
                    // model
                    // model (clone)

                    var node1 = new HydroNode("n1");
                    var node2 = new HydroNode("n2");
                    var branch1 = new Channel("branch1", node1, node2, 100);
                    var network = new HydroNetwork { Branches = { branch1 }, Nodes = { node1, node2 } };
                    var model = new WaterFlowModel1D { Name = "model", Network = network };
                    model.BoundaryConditions[0].DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;

                    var rootFolder = app.Project.RootFolder;
                    rootFolder.Add(model);

                    var timeSeriesDataItem = new DataItem(HydroTimeSeriesFactory.CreateFlowTimeSeries());
                    rootFolder.Add(timeSeriesDataItem);

                    var modelClone = (WaterFlowModel1D)model.DeepClone();
                    modelClone.Name += " (clone)";
                    rootFolder.Add(modelClone);

                    // expand tree view to cloned model bc
                    var projectExplorer = gui.ToolWindowViews.OfType<ProjectExplorer>().First();
                    var treeView = projectExplorer.TreeView;
                    treeView.Nodes[0].Expand();
                    treeView.Nodes[0].Nodes[2].Expand(); // model (clone)
                    treeView.Nodes[0].Nodes[2].Nodes[0].Expand(); // Input
                    treeView.Nodes[0].Nodes[2].Nodes[0].Nodes[0].Expand(); // Boundary Data

                    // link
                    var boundaryCondition = (WaterFlowModel1DBoundaryNodeData)modelClone.GetDataItemByValue(modelClone.BoundaryConditions[0]).Value;
                    boundaryCondition.SeriesDataItem.LinkTo(timeSeriesDataItem);

                    treeView.WaitUntilAllEventsAreProcessed();

                    // node must show that bc is linked
                    var boundaryConditionTreeNode = treeView.Nodes[0].Nodes[2].Nodes[0].Nodes[0].Nodes[0];
                    boundaryConditionTreeNode.Text.Should().Be.EqualTo("n1 - Q(t) (flow time series)");
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }
    }
}
