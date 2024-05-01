using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using DeltaShell.IntegrationTestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;
using Control = System.Windows.Controls.Control;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class HydroRegionTreeViewTest
    {
        private MockRepository mocks = new MockRepository();

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeView()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();
            
            pluginGui.Gui = gui;
            
            var network = new HydroNetwork();
            var networkTreeView = new HydroRegionTreeView(pluginGui) {Dock = DockStyle.Fill};

            var branch = new Channel();
            var crossSection = new CrossSectionDefinitionXYZ("Nummer 1");
            var crossSection2 = new CrossSectionDefinitionXYZ("Nummer 2");
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, crossSection, 400);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, crossSection2, 0);
            
            network.Branches.Add(branch);
            networkTreeView.Region = network;

            WindowsFormsTestHelper.ShowModal(new Form { Controls = { networkTreeView } });
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeViewWithRegion()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;

            var branch = new Channel();
            var network = new HydroNetwork {Name = "network", Branches = {branch}};
            var region = new HydroRegion {Name = "region", SubRegions = {network, new DrainageBasin()}};

            var networkTreeView = new HydroRegionTreeView(pluginGui) { Dock = DockStyle.Fill };

            networkTreeView.Region = region;

            WindowsFormsTestHelper.ShowModal(new Form { Controls = { networkTreeView } });
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

            var networkTreeView = new HydroRegionTreeView(pluginGui) {Dock = DockStyle.Fill, Region = basin};

            WindowsFormsTestHelper.ShowModal(new Form {Controls = {networkTreeView}});
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeViewWithBasinAndPluginGui()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NetworkEditorGuiPlugin()
            };
            
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                gui.Run();

                var basin = new DrainageBasin
                    {
                        Catchments = {new Catchment {Name = "c1"}},
                        WasteWaterTreatmentPlants = {new WasteWaterTreatmentPlant {Name = "wwtp1"}}
                    };

                var hydroRegionTreeView = gui.ToolWindowViews.OfType<HydroRegionTreeView>().First();
                WindowsFormsTestHelper.ShowModal(
                    new Form {Controls = {hydroRegionTreeView}}, f =>
                        {
                            hydroRegionTreeView.Region = basin;
                        }
                    );
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void RemoveCrossSectionDefinitionShowsMessageBoxIfInUse()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;

            var network = new HydroNetwork();
            var networkTreeView = new HydroRegionTreeView(pluginGui) { Dock = DockStyle.Fill };

            var branch = new Channel();
            var crossSectionDef = new CrossSectionDefinitionYZ("Nummer 1");
            var proxyDef = new CrossSectionDefinitionProxy(crossSectionDef);
            var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, proxyDef, 10.0);

            network.SharedCrossSectionDefinitions.Add(crossSectionDef);
            network.Branches.Add(branch);

            networkTreeView.Dock = DockStyle.Fill;
            networkTreeView.Region = network;
            
            var definitionNodePresenter = networkTreeView.TreeView.GetTreeViewNodePresenter(crossSectionDef, null);

            var customMessageBox = mocks.StrictMock<IMessageBox>();
            DelftTools.Controls.Swf.MessageBox.CustomMessageBox = customMessageBox;

            int callcount = 0;

            //select definition to remove
            networkTreeView.TreeView.SelectedNode = GetSharedDefinitionsRootNode(networkTreeView).Nodes[0];

            customMessageBox.Expect(mb => mb.Show(null, null, MessageBoxButtons.OKCancel)).IgnoreArguments().Return(
                DialogResult.Cancel).Repeat.Any().WhenCalled(m => callcount++);

            customMessageBox.Replay();
            
            definitionNodePresenter.RemoveNodeData(null, crossSectionDef);
            Assert.AreEqual(1, callcount); //make sure a messagebox was shown

            callcount = 0;
            cs.MakeDefinitionLocal();
            definitionNodePresenter.RemoveNodeData(null, crossSectionDef);
            Assert.AreEqual(0, callcount); //make sure messagebox was not shown

            customMessageBox.VerifyAllExpectations();
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void IfUserOKsMessageBoxCrossSectionDefinitionIsMadeLocal()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;

            var network = new HydroNetwork();
            var networkTreeView = new HydroRegionTreeView(pluginGui) { Dock = DockStyle.Fill };

            var branch = new Channel();
            var crossSectionDef = new CrossSectionDefinitionYZ("Nummer 1");

            var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, new CrossSectionDefinitionProxy(crossSectionDef), 10.0);
            cs.Name = "test1";

            var cs2 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, new CrossSectionDefinitionProxy(crossSectionDef), 10.0);
            cs2.Name = "test2";

            network.SharedCrossSectionDefinitions.Add(crossSectionDef);
            network.Branches.Add(branch);

            networkTreeView.Dock = DockStyle.Fill;
            networkTreeView.Region = network;

            var definitionNodePresenter = networkTreeView.TreeView.GetTreeViewNodePresenter(crossSectionDef, null);

            var customMessageBox = mocks.StrictMock<IMessageBox>();
            DelftTools.Controls.Swf.MessageBox.CustomMessageBox = customMessageBox;
            
            //select definition to remove
            networkTreeView.TreeView.SelectedNode = GetSharedDefinitionsRootNode(networkTreeView).Nodes[0];

            customMessageBox.Expect(mb => mb.Show(null, null, MessageBoxButtons.OKCancel)).IgnoreArguments().Return(
                DialogResult.OK).Repeat.AtLeastOnce().WhenCalled(m =>
                                                                 Assert.AreEqual("The cross section definition you are trying to delete is being used. If you continue, the definition will be replaced by local copies in each cross section. Are you sure you want to continue?\n\nThe following cross sections use this definition:\ntest1\ntest2", m.Arguments[0]));

            customMessageBox.Replay();

            Assert.IsTrue(cs.Definition.IsProxy);

            definitionNodePresenter.RemoveNodeData(null, crossSectionDef);

            Assert.IsFalse(cs.Definition.IsProxy);

            customMessageBox.VerifyAllExpectations();
        }

        private static ITreeNode GetSharedDefinitionsRootNode(HydroRegionTreeView hydroRegionTreeView)
        {
            return hydroRegionTreeView.TreeView.Nodes[0].Nodes[1];
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeViewWithSharedCrossSectionDefinitions()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;
            
            var network = new HydroNetwork();
            var networkTreeView = new HydroRegionTreeView(pluginGui) { Dock = DockStyle.Fill };

            var branch = new Channel();
            var crossSectionDef1 = new CrossSectionDefinitionYZ("Nummer 1");
            var crossSectionDef2 = new CrossSectionDefinitionZW("Nummer 2");

            network.SharedCrossSectionDefinitions.Add(crossSectionDef1);
            network.SharedCrossSectionDefinitions.Add(crossSectionDef2);
            
            networkTreeView.Dock = DockStyle.Fill;
            networkTreeView.Region = network;
            
            var f = new Form();
            f.Controls.Add(networkTreeView);

            network.Branches.Add(branch);

            //enforce ordering
            Assert.AreEqual("Shared Cross Section Definitions", GetSharedDefinitionsRootNode(networkTreeView).Text);

            WindowsFormsTestHelper.ShowModal(f);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeViewWithRoutes()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;

            var network = new HydroNetwork();
            var networkTreeView = new HydroRegionTreeView(pluginGui) { Dock = DockStyle.Fill };
            
            var testName = "route1";
            
            var branch = new Channel();
            var route = new Route {Name = testName};

            var postFix = route.Locations.Values.Count == 0 ? " (empty)" : "";
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
            var networkTreeView = new HydroRegionTreeView(pluginGui) { Dock = DockStyle.Fill };

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
            var networkTreeView = new HydroRegionTreeView(pluginGui) { Dock = DockStyle.Fill };

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
        [Category(TestCategory.Integration)]
        public void MutatingSharedDefinitionsUpdatesTreeView()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;
            
            var network = new HydroNetwork();
            var networkTreeView = new HydroRegionTreeView(pluginGui) { Dock = DockStyle.Fill };

            WindowsFormsTestHelper.Show(networkTreeView);

            var crossSectionDef = new CrossSectionDefinitionZW("Nummer 1");
            network.SharedCrossSectionDefinitions.Add(crossSectionDef);

            networkTreeView.Region = network;

            var sharedList = GetSharedDefinitionsRootNode(networkTreeView);

            Assert.AreEqual(1, sharedList.Nodes.Count);

            var crossSectionDef2 = new CrossSectionDefinitionYZ("Nummer 2");
            network.SharedCrossSectionDefinitions.Add(crossSectionDef2); 
            networkTreeView.WaitUntilAllEventsAreProcessed();

            Assert.AreEqual(2, sharedList.Nodes.Count);

            network.SharedCrossSectionDefinitions.Remove(crossSectionDef);
            networkTreeView.WaitUntilAllEventsAreProcessed();

            Assert.AreEqual(1, sharedList.Nodes.Count);

            WindowsFormsTestHelper.CloseAll();
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTreeViewWithObservationPoints()
        {
            var pluginGui = mocks.Stub<GuiPlugin>();
            var gui = mocks.Stub<IGui>();

            pluginGui.Gui = gui;

            var network = new HydroNetwork();
            var networkTreeView = new HydroRegionTreeView(pluginGui) { Dock = DockStyle.Fill };

            var branch = new Channel();
            var observation1 = new ObservationPoint() {Chainage = 42.0};
            var observation2 = new ObservationPoint() { Chainage = 88.33 }; 
            NetworkHelper.AddBranchFeatureToBranch(observation1, branch, observation1.Chainage);
            NetworkHelper.AddBranchFeatureToBranch(observation2, branch, observation2.Chainage);
            network.Branches.Add(branch);
            networkTreeView.Region = network;

            var f = new Form();
            f.Controls.Add(networkTreeView);

            WindowsFormsTestHelper.ShowModal(f);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void NetworkTreeViewWithDeltaShell()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NetworkEditorApplicationPlugin()
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                IApplication app = gui.Application;

                app.UserSettings["autosaveWindowLayout"] = false;

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
            var cs1 = new CrossSection(new CrossSectionDefinitionXYZ("100")){Chainage = 100};
            var cs2 = new CrossSection(new CrossSectionDefinitionXYZ("200")){Chainage = 200};
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
            int callCount = 0;
            ((INotifyCollectionChange)(network)).CollectionChanged +=
                delegate
                    {
                    callCount++;
                };
            //add a new section type results in only one call!
            network.CrossSectionSectionTypes.Add(new CrossSectionSectionType(){Name = "test"});
            Assert.AreEqual(1, callCount);
        }
    }
}