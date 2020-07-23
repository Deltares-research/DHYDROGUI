using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;
using Control = System.Windows.Controls.Control;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class HydroRegionTreeViewTest
    {
        private MockRepository mocks = new MockRepository();

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeViewWithRegion()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;

            var branch = new Channel();
            var network = new HydroNetwork
            {
                Name = "network",
                Branches = {branch}
            };
            var region = new HydroRegion
            {
                Name = "region",
                SubRegions =
                {
                    network,
                    new DrainageBasin()
                }
            };

            var networkTreeView = new HydroRegionTreeView(pluginGui) {Dock = DockStyle.Fill};

            networkTreeView.Region = region;

            WindowsFormsTestHelper.ShowModal(new Form {Controls = {networkTreeView}});
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeViewWithBasin()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;

            var basin = new DrainageBasin
            {
                Catchments = {new Catchment {Name = "c1"}},
                WasteWaterTreatmentPlants = {new WasteWaterTreatmentPlant {Name = "wwtp1"}}
            };

            var networkTreeView = new HydroRegionTreeView(pluginGui)
            {
                Dock = DockStyle.Fill,
                Region = basin
            };

            WindowsFormsTestHelper.ShowModal(new Form {Controls = {networkTreeView}});
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeViewWithBasinAndPluginGui()
        {
            using (var gui = new DeltaShellGui())
            {
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Run();

                var basin = new DrainageBasin
                {
                    Catchments = {new Catchment {Name = "c1"}},
                    WasteWaterTreatmentPlants = {new WasteWaterTreatmentPlant {Name = "wwtp1"}}
                };

                HydroRegionTreeView hydroRegionTreeView = gui.ToolWindowViews.OfType<HydroRegionTreeView>().First();
                WindowsFormsTestHelper.ShowModal(
                    new Form {Controls = {hydroRegionTreeView}}, f => { hydroRegionTreeView.Region = basin; }
                );
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeViewWithRoutes()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;

            var network = new HydroNetwork();
            var networkTreeView = new HydroRegionTreeView(pluginGui) {Dock = DockStyle.Fill};

            var testName = "route1";

            var branch = new Channel();
            var route = new Route {Name = testName};

            string postFix = route.Locations.Values.Count == 0 ? " (empty)" : "";
            postFix = route.Locations.Values.Count == 1 ? " (one node)" : postFix;

            network.Routes.Add(route);

            networkTreeView.Dock = DockStyle.Fill;
            networkTreeView.Region = network;

            var f = new Form();
            f.Controls.Add(networkTreeView);

            network.Branches.Add(branch);

            //enforce ordering
            Assert.AreEqual("Routes", networkTreeView.TreeView.Nodes[0].Nodes[0].Text);
            Assert.AreEqual(testName + postFix, networkTreeView.TreeView.Nodes[0].Nodes[0].Nodes[0].Text);

            WindowsFormsTestHelper.ShowModal(f);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeViewWithCatchments()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;

            var basin = new DrainageBasin();
            var networkTreeView = new HydroRegionTreeView(pluginGui) {Dock = DockStyle.Fill};

            basin.Catchments.Add(Catchment.CreateDefault());
            basin.Catchments.Add(Catchment.CreateDefault());
            var testname = "testName";
            basin.Catchments[0].Name = testname;
            basin.Catchments[0].CatchmentType = CatchmentType.Unpaved;

            networkTreeView.Dock = DockStyle.Fill;
            networkTreeView.Region = basin;

            var f = new Form();
            f.Controls.Add(networkTreeView);

            //enforce ordering
            Assert.AreEqual("Catchments", networkTreeView.TreeView.Nodes[0].Nodes[1].Text);
            Assert.AreEqual("testName (Unpaved)", networkTreeView.TreeView.Nodes[0].Nodes[1].Nodes[0].Text);

            WindowsFormsTestHelper.ShowModal(f);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeViewWithWasteWaterTreatmentPlants()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;

            var basin = new DrainageBasin();
            var networkTreeView = new HydroRegionTreeView(pluginGui) {Dock = DockStyle.Fill};

            basin.WasteWaterTreatmentPlants.Add(WasteWaterTreatmentPlant.CreateDefault());
            basin.WasteWaterTreatmentPlants.Add(WasteWaterTreatmentPlant.CreateDefault());
            var testname = "testName";
            basin.WasteWaterTreatmentPlants[0].Name = testname;

            networkTreeView.Dock = DockStyle.Fill;
            networkTreeView.Region = basin;

            var f = new Form();
            f.Controls.Add(networkTreeView);

            //enforce ordering
            Assert.AreEqual("Wastewater Treatment Plants", networkTreeView.TreeView.Nodes[0].Nodes[2].Text);
            Assert.AreEqual(testname, networkTreeView.TreeView.Nodes[0].Nodes[2].Nodes[0].Text);

            WindowsFormsTestHelper.ShowModal(f);
        }
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeViewWithObservationPoints()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;

            var network = new HydroNetwork();
            var networkTreeView = new HydroRegionTreeView(pluginGui) {Dock = DockStyle.Fill};

            var branch = new Channel();
            var observation1 = new ObservationPoint() {Chainage = 42.0};
            var observation2 = new ObservationPoint() {Chainage = 88.33};
            NetworkHelper.AddBranchFeatureToBranch(observation1, branch, observation1.Chainage);
            NetworkHelper.AddBranchFeatureToBranch(observation2, branch, observation2.Chainage);
            network.Branches.Add(branch);
            networkTreeView.Region = network;

            var f = new Form();
            f.Controls.Add(networkTreeView);

            WindowsFormsTestHelper.ShowModal(f);
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void NetworkTreeViewWithDeltaShell()
        {
            using (var gui = new DeltaShellGui())
            {
                IApplication app = gui.Application;

                app.UserSettings["autosaveWindowLayout"] = false;

                // add networkeditor plugin
                var networkEditorPlugin = new NetworkEditorApplicationPlugin {Application = app};
                app.Plugins.Add(networkEditorPlugin);

                // run delta shell
                gui.Run();

                WpfTestHelper.ShowModal((Control) gui.MainWindow);
            }
        }

        [Test]
        public void CheckCrossSectionSort()
        {
            //stubs
            var parentNode = mocks.Stub<ITreeNode>();
            var childNode1 = mocks.Stub<ITreeNode>();
            var childNode2 = mocks.Stub<ITreeNode>();
            var pluginGui = mocks.Stub<GuiPlugin>();
            var treeView = mocks.Stub<ITreeView>();

            var nodes = new List<ITreeNode>();
            var cs1 = new CrossSection(new CrossSectionDefinitionXYZ("100")) {Chainage = 100};
            var cs2 = new CrossSection(new CrossSectionDefinitionXYZ("200")) {Chainage = 200};
            var channel = new Channel();
            childNode1.Tag = cs1;
            childNode2.Tag = cs2;

            var channelpresenter = new ChannelTreeViewNodePresenter(pluginGui);

            Expect.Call(parentNode.Nodes).Return(nodes).Repeat.Any();
            Expect.Call(parentNode.IsLoaded).Return(false);
            Expect.Call(parentNode.GetNodeByTag(cs1)).Return(childNode1);
            Expect.Call(treeView.GetNodeByTag(cs1)).Return(childNode1);
            Expect.Call(childNode1.Parent).Return(parentNode).Repeat.Any();
            mocks.ReplayAll();
            parentNode.Nodes.Add(childNode2);
            parentNode.Nodes.Add(childNode1);

            //actual test, presenter should change the order of nodes
            Assert.AreEqual(nodes[0].Tag, cs2); //before sort
            channelpresenter.UpdateNode(null, parentNode, channel);
            Assert.AreEqual(nodes[0].Tag, cs1); // after first sort

            //offset updated, presenter should change the order of nodes once again
            cs1.Chainage = 300;
            channelpresenter.UpdateNode(null, parentNode, channel);
            Assert.AreEqual(nodes[0].Tag, cs2); //after second sort
        }

        [Test]
        public void NotifyNetworkCollectionChangedAfterAddingSectionType()
        {
            var network = new HydroNetwork();
            //register to collectionchanged of network
            var callCount = 0;
            ((INotifyCollectionChange) network).CollectionChanged +=
                delegate { callCount++; };
            //add a new section type results in only one call!
            network.CrossSectionSectionTypes.Add(new CrossSectionSectionType() {Name = "test"});
            Assert.AreEqual(1, callCount);
        }

        private static ITreeNode GetSharedDefinitionsRootNode(HydroRegionTreeView hydroRegionTreeView)
        {
            return hydroRegionTreeView.TreeView.Nodes[0].Nodes[1];
        }
    }
}