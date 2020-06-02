using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpTestsEx;
using Control = System.Windows.Controls.Control;
using TreeView = DelftTools.Controls.Swf.TreeViewControls.TreeView;

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
            using (var gui = new DeltaShellGui())
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

        [Test]
        public void SelectingSubElementOfNetworkWithNoNetworkViewOpenDoesNotCauseException()
        {
            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;

                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());

                gui.Run();

                app.UserSettings["autosaveWindowLayout"] = false; // skip damagin of window layout

                var network = new HydroNetwork();
                app.Project.RootFolder.Add(network);

                TreeView treeView = gui.ToolWindowViews.AllViews.OfType<HydroRegionTreeView>().First().TreeView;

                WpfTestHelper.ShowModal((Control) gui.MainWindow,
                                        () =>
                                        {
                                            gui.Selection = app.Project.RootFolder.DataItems.First(di => di.Value == network);
                                            Assert.AreEqual(network, treeView.Data);
                                            treeView.SelectedNode = treeView.AllLoadedNodes.First(n => n.Text == "Shared Cross Section Definitions");
                                            Assert.AreEqual(network, treeView.Data);
                                        });
            }
        }

        [Test]
        public void SelectingAnotherCrossSectionInNetworkTreeCleansViewCorrectly_Tools7425()
        {
            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;

                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());

                gui.Run();

                app.UserSettings["autosaveWindowLayout"] = false; // skip damagin of window layout

                var network = new HydroNetwork();
                app.Project.RootFolder.Add(network);
                var branch1 = new Branch
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(100, 0)
                    })
                };
                NetworkHelper.AddChannelToHydroNetwork(network, branch1);

                ICrossSection cs1 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, CrossSectionDefinitionYZ.CreateDefault("cs1"), 15);
                ICrossSection cs2 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, CrossSectionDefinitionYZ.CreateDefault("cs2"), 30);

                var main = new CrossSectionSectionType {Name = "Main"};
                var fp1 = new CrossSectionSectionType {Name = "FloodPlain1"};

                cs1.Definition.Sections.Add(new CrossSectionSection
                {
                    MinY = 0,
                    MaxY = 25,
                    SectionType = fp1
                });
                cs1.Definition.Sections.Add(new CrossSectionSection
                {
                    MinY = 25,
                    MaxY = 75,
                    SectionType = main
                });
                cs1.Definition.Sections.Add(new CrossSectionSection
                {
                    MinY = 75,
                    MaxY = 100,
                    SectionType = fp1
                });
                cs1.Name = "cs1";

                cs2.Definition.Sections.Add(new CrossSectionSection
                {
                    MinY = 0,
                    MaxY = 20,
                    SectionType = fp1
                });
                cs2.Definition.Sections.Add(new CrossSectionSection
                {
                    MinY = 20,
                    MaxY = 80,
                    SectionType = main
                });
                cs2.Definition.Sections.Add(new CrossSectionSection
                {
                    MinY = 80,
                    MaxY = 100,
                    SectionType = fp1
                });
                cs2.Name = "cs2";

                TreeView treeView = gui.ToolWindowViews.AllViews.OfType<HydroRegionTreeView>().First().TreeView;

                WpfTestHelper.ShowModal(
                    (Control) gui.MainWindow,
                    () =>
                    {
                        gui.CommandHandler.OpenView(cs1);
                        var csView = (CrossSectionView) gui.DocumentViews.ActiveView;
                        ITreeNode cs1Node = treeView.GetNodeByTag(cs1);
                        ITreeNode cs2Node = treeView.GetNodeByTag(cs2);

                        int propBefore = TestReferenceHelper.FindEventSubscriptions(cs2);
                        var chartView = (ChartView) csView.Controls.Find("chartView", true)[0];

                        object innerChart = TypeUtils.GetField(chartView.Chart, "chart");
                        object innerTools = TypeUtils.GetPropertyValue(innerChart, "Tools");
                        var toolsBefore = (int) TypeUtils.GetPropertyValue(innerTools, "Count");

                        treeView.SelectedNode = cs2Node;

                        Assert.AreEqual(cs2, csView.Data);

                        treeView.SelectedNode = cs1Node;
                        treeView.SelectedNode = cs2Node;
                        treeView.SelectedNode = cs1Node;

                        // give delayed event handler some time
                        Thread.Sleep(100);
                        Application.DoEvents();
                        Thread.Sleep(100);

                        int propAfter = TestReferenceHelper.FindEventSubscriptions(cs2);
                        var toolsAfter = (int) TypeUtils.GetPropertyValue(innerTools, "Count");

                        Assert.AreEqual(propBefore, propAfter, "#event leaks");
                        Assert.AreEqual(toolsBefore, toolsAfter, "#tools leaks");
                    });
            }
        }
    }
}