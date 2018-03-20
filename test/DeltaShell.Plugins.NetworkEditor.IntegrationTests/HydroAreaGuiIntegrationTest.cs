using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.UI.Tools;
using Control = System.Windows.Controls.Control;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.Integration)]

    public class HydroAreaGuiIntegrationTest
    {
        [Test]
        [Category(TestCategory.Slow)]
        public void ShowFMWeirShouldDisplayFMWeirView()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;

                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());

                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Run();

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
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;

                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());

                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Run();

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
        
        /// <summary>
        /// test to check improvement importing many drypoints (TOOLS-21888) 
        /// & TOOLS-22796:'Selecting area feature type in project tree selects all features in map which can be extremely slow'
        /// </summary>
        /// 
        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ImportingOfDryPointsWithProjectItemMapViewOpenShouldBeFast()
        {
            using (var gui = new DeltaShellGui())
            {
                //setup env
                var app = gui.Application;

                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Run();

                //create and add a HydroRegion with a HydroArea with DryPoints
                var project = app.Project;
                var area = new HydroArea();
                var hydroRegion = new HydroRegion
                {
                    Name = "Hydro region",
                    SubRegions = {area}
                };
                var dataItem = new DataItem(hydroRegion);
                project.RootFolder.Add(hydroRegion);

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    //load needed views
                    gui.CommandHandler.OpenView(dataItem, typeof(ProjectItemMapView));
                    var projectItemMapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                    Assert.NotNull(projectItemMapView);
                    
                    //importing harlingen point ~ 28800 points... this took over 15 min to load
                    var fmtestPath = TestHelper.GetTestDataPath(typeof(WaterFlowFMModelTest).Assembly);
                    var xyzPath = Path.Combine(fmtestPath, @"harlingen_model_3d\har_V3.xyz");
                    var selection = new DataItem(area.DryPoints);

                    gui.Selection = selection;

                    //start the import and check the speed (TOOLS-21888)
                    TestHelper.AssertIsFasterThan(20000 , () =>
                    {
                        gui.CommandHandler.ImportFilesToGuiSelection(new[] {xyzPath});
                        while (gui.Application.ActivityRunner.IsRunning)
                        {
                            Application.DoEvents();
                        }
                    });

                    //zoom to extend for fun
                    projectItemMapView.MapView.Map.ZoomToExtents();
                    
                    //switch from layer
                    gui.Selection = area.DryAreas;

                    //switch back to drypoints layer and check speed of selection (<4000ms!) & selection count (== SelectTool.MaxSelectedFeatures)
                    TestHelper.AssertIsFasterThan(4000, () => gui.Selection = area.DryPoints);
                    Assert.AreEqual(SelectTool.MaxSelectedFeatures, projectItemMapView.MapView.MapControl.SelectedFeatures.Count());
                }); 
            }
        }
    }
}
