using System.Collections;
using System.ComponentModel;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class NetworkTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<INetwork>
    {
        private static readonly Image NetworkImage = Properties.Resources.Network;

        public NetworkTreeViewNodePresenter(GuiPlugin guiPlugin): base(guiPlugin)
        {
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override void OnNodeRenamed(INetwork network, string newName)
        {
            if (network.Name != newName)
            {
                network.Name = newName;
            }
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, INetwork network)
        {
            node.Text = network.Name;
            node.Image = NetworkImage;
        }

        public override IEnumerable GetChildNodeObjects(INetwork network, ITreeNode node)
        {
            yield return ((IHydroNetwork)network).Routes;
            yield return ((IHydroNetwork)network).SharedCrossSectionDefinitions;
            yield return ((IHydroNetwork) network).CrossSectionSectionTypes;
            
            foreach (var branch in network.Branches)
            {
                yield return branch;
            } 
        }

        public override DragOperations CanDrag(INetwork nodeData)
        {
            return DragOperations.Link | DragOperations.Move;
        }

        protected override void OnPropertyChanged(INetwork netWork, ITreeNode node, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("Name")) return;

            UpdateNode(null, TreeView.GetNodeByTag(netWork), netWork);
        }
    }
}