using System;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpTestsEx;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    [Category(TestCategory.Wpf)]
    [Category(TestCategory.Slow)]
    public class NetworkEditorGuiPluginTest
    {
        [Test]
        public void RenamingNetworkCoverageNodesWrappedWithDataItems()
        {
            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                IApplication app = gui.Application;

                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());

                gui.Run();

                app.CreateNewProject();
                
                app.UserSettings["autosaveWindowLayout"] = false; // skip damagin of window layout

                var networkCoverage = new NetworkCoverage {Name = "coverage1"};
                app.Project.RootFolder.Add(networkCoverage);

                Action afterShow = () =>
                {
                    IDataItem networkCoverageDataItem = app.Project.RootFolder.DataItems.Last();

                    ITreeView treeView = ProjectExplorerGuiPlugin.Instance.ProjectExplorer.TreeView;
                    treeView.Refresh();
                    ITreeNodePresenter nodePresenter = treeView.GetTreeViewNodePresenter(networkCoverageDataItem, null);

                    treeView.WaitUntilAllEventsAreProcessed();
                    ITreeNode node = treeView.GetNodeByTag(networkCoverageDataItem);

                    nodePresenter.CanRenameNode(node).Should("Renaming network coverages in project explorer is off by default").Be.False();
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, afterShow);
            }
        }
    }
}