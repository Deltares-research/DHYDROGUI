using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class CompositeStructureTreeViewNodePresenter : BranchFeatureTreeViewNodePresenterBase<ICompositeBranchStructure>
    {
        public CompositeStructureTreeViewNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin) {}

        public override IEnumerable GetChildNodeObjects(ICompositeBranchStructure parentNodeData, ITreeNode node)
        {
            return parentNodeData.Structures.Cast<object>();
        }

        public override DragOperations CanDrop(object item, ITreeNode sourceNode, ITreeNode targetNode, DragOperations validOperations)
        {
            return item.GetType().IsSubclassOf(typeof(IStructure1D)) ? DragOperations.Move : DragOperations.None;
        }

        public override void OnDragDrop(object item, object sourceParentNodeData, ICompositeBranchStructure target, DragOperations operation, int position)
        {
            ((ICompositeBranchStructure) sourceParentNodeData).Structures.Remove((IStructure1D) item);
            target.Structures.Insert(position, (IStructure1D) item);
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        protected override bool CanRemove(ICompositeBranchStructure nodeData)
        {
            return false;
        }

        protected override void OnPropertyChanged(ICompositeBranchStructure feature, ITreeNode node, PropertyChangedEventArgs e)
        {
            if (node == null)
            {
                return;
            }

            base.OnPropertyChanged(feature, node, e);
        }

        protected override Image GetImage(ICompositeBranchStructure compositeBranchStructure)
        {
            return Resources.StructureFeatureSmall;
        }
    }
}