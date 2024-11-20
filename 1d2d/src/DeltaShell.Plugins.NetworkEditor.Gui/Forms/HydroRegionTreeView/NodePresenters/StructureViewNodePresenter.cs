using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    internal class StructureViewNodePresenter<T> : TreeViewNodePresenterBaseForPluginGui<T> where T : IStructure1D
    {
        public StructureViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }

        protected override bool CanRemove(T nodeData)
        {
            return true;
        }

        public override DragOperations CanDrag(T nodeData)
        {
            return nodeData.ParentStructure.Structures.Count > 1 ? DragOperations.Move : DragOperations.None;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, T data)
        {
            node.Text = data.Name;
        }

        protected override bool RemoveNodeData(object parentNodeData, T nodeData)
        {
            HydroNetworkHelper.RemoveStructure(nodeData);
            return true;
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override void OnNodeRenamed(T data, string newName)
        {
            if (data.Name != newName)
            {
                data.Name = newName;
            }
        }

        protected override void OnPropertyChanged(T item, ITreeNode node, PropertyChangedEventArgs e)
        {
            if (node == null) return;

            if (e.PropertyName.Equals("Name"))
            {
                node.Text = item.Name;
            }
        }

        protected override void OnCollectionChanged(T childNodeData, ITreeNode parentNode, NotifyCollectionChangedEventArgs e, int newNodeIndex)
        {
            base.OnCollectionChanged(childNodeData, parentNode, e, newNodeIndex);

            // re-order structure nodes in case if this is the only one structure
            if (e.Action != NotifyCollectionChangedAction.Add || 
                childNodeData.ParentStructure != null ||
                parentNode.Nodes.Count <= 1) return;

            var node = parentNode.GetNodeByTag(e.GetRemovedOrAddedItem());
            if (node == null) return;

            var index = parentNode.Nodes.IndexOf(node);

            //index of node in sorted list
            var nodes = new List<ITreeNode>(parentNode.Nodes);
            nodes.Sort(new BranchFeatureComparer());
            var sortedIndex = nodes.IndexOf(node);

            if (sortedIndex != index)
            {
                parentNode.Nodes.Remove(node);
                parentNode.Nodes.Insert(sortedIndex, node);
            }
        }
    }
}