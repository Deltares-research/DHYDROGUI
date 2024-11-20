using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Export;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpTestsEx;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]

    public class NetworkEditorGuiPluginTest
    {
        [Test]
        public void GetFileExporters_ContainsExpectedExporterForShapes()
        {
            // Arrange
            var plugin = new NetworkEditorGuiPlugin();
            
            // Act
            IEnumerable<IFileExporter> exporters = plugin.GetFileExporters();

            // Assert
            Assert.That(exporters, Has.Exactly(1).TypeOf<HydroRegionShapeFileExporter>());
        }
        
        [Test]
        [Category(TestCategory.Wpf)]
        [Category(TestCategory.Slow)]
        public void RenamingNetworkCoverageNodesWrappedWithDataItems()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new CommonToolsGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                IApplication app = gui.Application;
                IProjectService projectService = app.ProjectService;

                gui.Run();

                Project project = projectService.CreateProject();
                
                app.UserSettings["autosaveWindowLayout"] = false; // skip damagin of window layout

                var networkCoverage = new NetworkCoverage {Name = "coverage1"};
                project.RootFolder.Add(networkCoverage);

                Action afterShow = () =>
                {
                    IDataItem networkCoverageDataItem = project.RootFolder.DataItems.Last();

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