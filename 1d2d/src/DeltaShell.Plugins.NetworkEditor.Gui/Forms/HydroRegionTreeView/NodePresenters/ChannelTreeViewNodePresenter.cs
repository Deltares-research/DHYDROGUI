using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class ChannelTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<IBranch>
    {
        private static readonly Image NetworkBranchesImage = Properties.Resources.network_branches;
        
        public ChannelTreeViewNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override void OnNodeRenamed(IBranch branch   , string newName)
        {
            if (branch.Name != newName)
            {
                branch.Name = newName;
            }
        }

        /// <summary>
        /// Since property changed events are no longer sent to the nodepresenter we must always check if a sort is necessary.
        /// Possible optimization are:
        /// - only when node is expanded but must then add logis to OnExpand.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="node"></param>
        /// <param name="model"></param>
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IBranch model)
        {
            node.Text = model.Name;
            node.Image = NetworkBranchesImage;

            // hack to solve sorting problem caused by delayed event handler
            var sortedNodes = new List<ITreeNode>(node.Nodes);
            sortedNodes.Sort(new BranchFeatureComparer());
            var sortIsOutOfDate = sortedNodes.Where((sortedNode, index) => index != node.Nodes.IndexOf(sortedNode)).Any();
            if (!sortIsOutOfDate)
            {
                return;
            }
            node.Nodes.Clear();
            sortedNodes.ForEach(n => node.Nodes.Add(n));
        }

        public override IEnumerable GetChildNodeObjects(IBranch branch, ITreeNode node)
        {
            var branchFeatures = new List<IBranchFeature>(branch.BranchFeatures);
            branchFeatures.Sort();
            foreach (var branchFeature in branchFeatures)
            {
                // skip elements of the composite structure.
                if(branchFeature is IStructure1D && !(branchFeature is ICompositeBranchStructure))
                {
                    //Trace.WriteLine(string.Format("child {0}", branchFeature.Name));
                    continue;
                }

                yield return branchFeature;
            }
        }

        protected override bool CanRemove(IBranch nodeData)
        {
            return true;
        }

        protected override void OnPropertyChanged(IBranch item, ITreeNode node, PropertyChangedEventArgs e)
        {
            if (node == null) return;

            if (e.PropertyName.Equals("Name", StringComparison.Ordinal))
            {
                node.Text = item.Name;
            }
        }


        protected override bool RemoveNodeData(object parentNodeData, IBranch branch)
        {
            //remove the branch from the network
            INetwork network = branch.Network;

            network.BeginEdit(String.Format("Removing Branch {0} from network {1}", branch, network));

            network.Branches.Remove(branch);

            //specifically remove the unused nodes
            var removedNodes = NetworkHelper.RemoveUnusedNodes(network);

            var linksToRemove = removedNodes.OfType<HydroNode>().SelectMany(n => n.Links)
                .Concat(branch.BranchFeatures.OfType<IHydroObject>().Where(o => o.Links != null).SelectMany(o => o.Links))
                .ToArray();

            foreach (var link in linksToRemove)
            {
                HydroRegion.RemoveLink(link);
            }

            network.EndEdit();

            return true;
        }
    }
}