using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms.Integration;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.Integration)]

    public class HydroAreaGuiIntegrationTest
    {
        [Test]
        [Category(TestCategory.Slow)]
        public void ShowFMWeirShouldDisplayFMWeirView()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new CommonToolsApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;
                gui.Run();

                app.CreateNewProject();

                var project = app.Project;
                var network = new HydroNetwork();
                var area = new HydroArea();
                project.RootFolder.Add(new IHydroRegion[] {network, area});

                network.Nodes = new EventedList<INode>
                {
                    new HydroNode {Name = "node1", Geometry = new Point(0, 0)},
                    new HydroNode {Name = "node2", Geometry = new Point(1, 1)}
                };
                var branchGeometry = new LineString(new [] {new Coordinate(0, 0), new Coordinate(1, 1)});
                var channel = new Channel
                {
                    Name = "branch",
                    Source = network.Nodes[0],
                    Target = network.Nodes[1],
                    Geometry = branchGeometry
                };
                network.Branches.Add(channel);

                var networkWeir = new Weir("networkweir") {Chainage = 0.5};

                HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(networkWeir, network.Branches[0]);

                area.Weirs.Add(new Weir2D("fmweir"));

                gui.Selection = network.Weirs.First();
                gui.CommandHandler.OpenDefaultViewForSelection();

                Assert.IsTrue(
                    gui.DocumentViews.AllViews.OfType<CompositeStructureView>().First().ChildViews.First() is WeirView);

                gui.CommandHandler.RemoveAllViewsForItem(networkWeir);

                gui.Selection = area.Weirs.First();
                gui.CommandHandler.OpenDefaultViewForSelection();

                Assert.IsTrue(gui.DocumentViews.OfType<AreaStructureView>().Any());
                var view = gui.DocumentViews.OfType<AreaStructureView>().First().StructureControl as ElementHost;
                Assert.IsNotNull(view);
                Assert.IsTrue(view.Child is WeirViewWpf);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ShowFMPumpShouldDisplaySreaStructureViewWithPumpView()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new CommonToolsApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;
                
                gui.Run();

                app.CreateNewProject();

                var project = app.Project;
                var network = new HydroNetwork();
                var area = new HydroArea();
                project.RootFolder.Add(new IHydroRegion[] { network, area });

                network.Nodes = new EventedList<INode>
                {
                    new HydroNode {Name = "node1", Geometry = new Point(0, 0)},
                    new HydroNode {Name = "node2", Geometry = new Point(1, 1)}
                };
                var branchGeometry = new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 1) });
                var channel = new Channel
                {
                    Name = "branch",
                    Source = network.Nodes[0],
                    Target = network.Nodes[1],
                    Geometry = branchGeometry
                };
                network.Branches.Add(channel);

                var networkPump = new Pump("networkpump") { Chainage = 0.5 };

                HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(networkPump, network.Branches[0]);

                area.Pumps.Add(new Pump2D("fmpump"));

                gui.Selection = network.Pumps.First();
                gui.CommandHandler.OpenDefaultViewForSelection();

                Assert.IsTrue(
                    gui.DocumentViews.AllViews.OfType<CompositeStructureView>().First().ChildViews.First() is PumpView);

                gui.CommandHandler.RemoveAllViewsForItem(networkPump);

                gui.Selection = area.Pumps.First();
                gui.CommandHandler.OpenDefaultViewForSelection();

                Assert.IsTrue(gui.DocumentViews.OfType<AreaStructureView>().Any());
                Assert.IsTrue(
                    gui.DocumentViews.OfType<AreaStructureView>().First().StructureControl is PumpView);
            }
        }
    }
}
