using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests
{
    [TestFixture]
    public class DeltaShellNetworkEditorIntegrationTest
    {
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
                Project project = gui.Application.Project;
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
            IHydroNetwork network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0), new Point(100, 100));
            network.Branches.Remove(network.Branches[0]);
            NetworkHelper.RemoveUnusedNodes(network);
            Assert.AreEqual(2, network.HydroNodes.Count(n => !n.IsConnectedToMultipleBranches));
        }
    }
}