using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.IO;
using TreeView = DelftTools.Controls.Swf.TreeViewControls.TreeView;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView
{
    public partial class HydroRegionTreeView : UserControl, IView
    {
        private readonly IGui gui;
        private IContainer components;
        private ContextMenuStrip contextMenuFeature;
        private ClonableToolStripMenuItem buttonMenuFeatureOpen;
        private ToolStripSeparator toolStripSeparator8;
        private ClonableToolStripMenuItem buttonMenuFeatureCut;
        private ClonableToolStripMenuItem buttonMenuFeatureDelete;
        private ClonableToolStripMenuItem buttonDataItemRename;
        private ContextMenuStrip contextMenuNetwork;
        private ClonableToolStripMenuItem buttonMenuNetworkAddBranch;
        private ContextMenuStrip contextMenuBranch;
        private ClonableToolStripMenuItem buttonMenuBranchDelete;
        private ClonableToolStripMenuItem buttonMenuBranchRename;
        private IHydroRegion region;

        public HydroRegionTreeView(GuiPlugin guiPlugin)
        {
            InitializeComponent();

            gui = guiPlugin.Gui;
            gui.SelectionChanged += GuiSelectionChanged;

            TreeView = new TreeView
            {
                AllowDrop = true,
                Dock = DockStyle.Fill
            };

            AddNodePresenters(guiPlugin);

            Controls.Add(TreeView);
        }

        public bool SynchronizingGuiSelection { get; private set; }

        public IMenuItem GetContextMenu(object node, object tag)
        {
            var treeNode = (ITreeNode) node;
            SelectedRegion = GetParentRegionFromNode(node);

            bool isActiveViewMapView = gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().Any();
            if (tag is IHydroNetwork)
            {
                return new MenuItemContextMenuStripAdapter(contextMenuNetwork);
            }

            if (tag is IEventedList<Route>)
            {
                return new MenuItemContextMenuStripAdapter(contextMenuRoutes);
            }

            if (tag is IChannel)
            {
                buttonMenuBranchZoomTo.Enabled = isActiveViewMapView;
                return new MenuItemContextMenuStripAdapter(contextMenuBranch);
            }

            if (tag is HydroRegion)
            {
                return NetworkEditorGuiPlugin.Instance.GetContextMenu(node, tag);
            }

            if (tag is IFeature && TreeView.SelectedNode.Parent != null)
            {
                ITreeNodePresenter p = treeNode.Presenter;

                object parentNodeData = TreeView.SelectedNode.Parent.Tag;
                buttonMenuFeatureDelete.Enabled = p.CanRemove(parentNodeData, tag);
                buttonMenuFeatureCut.Enabled = p.CanRemove(parentNodeData, tag);
                buttonMenuFeatureZoomTo.Enabled = isActiveViewMapView;
                var contextMenuAdapter = new MenuItemContextMenuStripAdapter(contextMenuFeature);

                return contextMenuAdapter;
            }

            return null;
        }

        public new void Dispose()
        {
            gui.SelectionChanged -= GuiSelectionChanged;
            base.Dispose();
        }

        private IHydroRegion SelectedRegion { get; set; }
        
        private void AddNodePresenters(GuiPlugin guiPlugin)
        {
            var treeNodePresenters = new ITreeNodePresenter[]
            {
                new HydroRegionTreeViewNodePresenter(guiPlugin),
                new NetworkTreeViewNodePresenter(guiPlugin),
                new HydroAreaTreeViewNodePresenter(guiPlugin),
                new LateralSourceTreeViewNodePresenter(guiPlugin),
                new RetentionNodePresenter(guiPlugin),
                new ObservationPointTreeViewNodePresenter(guiPlugin),
                new NetworkRoutesTreeViewNodePresenter(guiPlugin),
                new NetworkRouteTreeViewNodePresenter(guiPlugin),
                new WasteWaterTreatmentPlantsTreeViewNodePresenter(guiPlugin),
                new WasteWaterTreatmentPlantTreeViewNodePresenter(guiPlugin),
                new RunoffBoundariesTreeViewNodePresenter(guiPlugin),
                new RunoffBoundaryTreeViewNodePresenter(guiPlugin)
            };

            foreach (ITreeNodePresenter treeNodePresenter in treeNodePresenters)
            {
                TreeView.NodePresenters.Add(treeNodePresenter);
            }
        }

        private void GuiSelectionChanged(object sender, SelectedItemChangedEventArgs e)
        {
            IFeature selectedFeature = null;

            // Try to select the network object in the treeview
            if (gui.Selection is IFeature)
            {
                selectedFeature = (IFeature) gui.Selection;
            }
            else if (gui.Selection is IEnumerable<IFeature>)
            {
                List<IFeature> features = ((IEnumerable<IFeature>) gui.Selection).ToList();
                if (features.Count == 0)
                {
                    return;
                }

                selectedFeature = features[0];
            }

            if (selectedFeature == null)
            {
                return;
            }

            // Search by comparing the feature to all the node tags
            ITreeNode treeNode = TreeView.GetNodeByTag(selectedFeature);
            if (treeNode == null)
            {
                return;
            }

            // The node correspoinding to the feature was found: select it
            SynchronizingGuiSelection = true;
            TreeView.SelectedNode = treeNode;
            SynchronizingGuiSelection = false;
        }

        private IHydroRegion GetParentRegionFromNode(object obj)
        {
            var node = obj as ITreeNode;
            while (node != null)
            {
                if (node.Tag is IHydroRegion)
                {
                    return node.Tag as IHydroRegion;
                }

                node = node.Parent;
            }

            return null;
        }

        private void handleButtonOpen_Click(object sender, EventArgs e)
        {
            gui.CommandHandler.OpenDefaultViewForSelection();
        }

        private void handleButtonAddBranch_Click(object sender, EventArgs e)
        {
            object selectedObject = TreeView.SelectedNode.Tag;
            var network = selectedObject as IHydroNetwork;

            if (network != null)
            {
                var channel = Channel.CreateDefault(network);

                // HACK: add geometry manually, TODO: probably it should happen automatically in topology rule, and also for nodes
                channel.Geometry =
                    new WKTReader().Read(string.Format("LINESTRING({0} 0,{0} 100)", network.Branches.Count * 100));

                NetworkHelper.AddChannelToHydroNetwork(network, channel);
                channel.Name = HydroNetworkHelper.GetUniqueFeatureName(network, channel);
            }
        }

        private void handleButtonDelete_Click(object sender, EventArgs e)
        {
            TreeView.DeleteNodeData();
            return;
        }

        private void handleButtonRename_Click(object sender, EventArgs e)
        {
            TreeView.StartLabelEdit();
        }

        private void handleButtonOpenWith_Click(object sender, EventArgs e)
        {
            gui.CommandHandler.OpenSelectViewDialog();
        }

        private void handleButtonAddLateralSource_Click(object sender, EventArgs e)
        {
            var channel = TreeView.SelectedNode.Tag as IChannel;
            if (channel != null)
            {
                channel.BranchFeatures.Add(LateralSource.CreateDefault(channel));
            }
        }
        
        private void handleButtonAddWeir_Click(object sender, EventArgs e)
        {
            var channel = TreeView.SelectedNode.Tag as IChannel;
            if (channel != null)
            {
                var branchFeature = new Weir();
                BranchStructure.AddStructureToNetwork(branchFeature, channel);
            }
        }
        
        private void handleButtonAddObservationPoint_Click(object sender, EventArgs e)
        {
            var channel = TreeView.SelectedNode.Tag as IChannel;
            if (channel != null)
            {
                channel.BranchFeatures.Add(ObservationPoint.CreateDefault(channel));
            }
        }

        private void handleButtonProperties_Click(object sender, EventArgs e)
        {
            gui.CommandHandler.ShowProperties();
        }
        
        private void handleButtonZoomToItem_Click(object sender, EventArgs e)
        {
            var feature = TreeView.SelectedNode.Tag as IFeature;
            if (feature != null)
            {
                var cmd = new MapZoomToFeatureCommand();
                cmd.Execute(feature);
            }
        }

        private void ButtonMenuFeatureCutClick(object sender, EventArgs e) {}
        
        #region IView Members

        object IView.Data
        {
            get
            {
                return region;
            }
            set
            {
                Region = (IHydroNetwork) value;
            }
        }

        public IHydroRegion Region
        {
            get
            {
                return region;
            }
            set
            {
                if (region == value)
                {
                    return;
                }

                SynchronizingGuiSelection = true;
                region = value;

                TreeView.Data = Region;

                SynchronizingGuiSelection = false;
            }
        }

        public Image Image
        {
            get
            {
                return Resources.network_branches;
            }
            set {}
        }

        public void EnsureVisible(object item) {}
        public ViewInfo ViewInfo { get; set; }

        public TreeView TreeView { get; }

        #endregion
    }
}