using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class CompositeStructureTreeViewNodePresenter : BranchFeatureTreeViewNodePresenterBase<ICompositeBranchStructure>
    {
        public CompositeStructureTreeViewNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        protected override bool CanRemove(ICompositeBranchStructure nodeData)
        {
            return false;
        }

        public override System.Collections.IEnumerable GetChildNodeObjects(ICompositeBranchStructure parentNodeData, ITreeNode node)
        {
            return parentNodeData.Structures.Cast<object>();
        }

        public override DragOperations CanDrop(object item, ITreeNode sourceNode, ITreeNode targetNode, DragOperations validOperations)
        {
            return item.GetType().IsSubclassOf(typeof (IStructure1D)) ? DragOperations.Move : DragOperations.None;
        }

        public override void OnDragDrop(object item, object sourceParentNodeData, ICompositeBranchStructure target, DragOperations operation, int position)
        {
            ((ICompositeBranchStructure)sourceParentNodeData).Structures.Remove((IStructure1D)item);
            target.Structures.Insert(position, (IStructure1D)item);
        }

        protected override void OnPropertyChanged(ICompositeBranchStructure feature, ITreeNode node, PropertyChangedEventArgs e)
        {
            if (node == null) return;

            base.OnPropertyChanged(feature, node, e);

            if (!e.PropertyName.Equals("Count", StringComparison.Ordinal) || node.Nodes.Count != feature.Structures.Count)
                return;
        }

        protected override Image GetImage(ICompositeBranchStructure compositeBranchStructure)
        {
            return Properties.Resources.StructureFeatureSmall;
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }
    }
}