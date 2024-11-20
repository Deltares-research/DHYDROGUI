using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class BoundaryConditionSetNodePresenter : FMSuiteNodePresenterBase<BoundaryConditionSet>
    {
        private static readonly Bitmap BoundaryImage = Common.Gui.Properties.Resources.boundary;

        protected override string GetNodeText(BoundaryConditionSet data)
        {
            return data.Feature != null ? data.Feature.Name : "<error>";
        }

        protected override Image GetNodeImage(BoundaryConditionSet data)
        {
            return BoundaryImage;
        }

        protected override bool CanRemove(BoundaryConditionSet nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, BoundaryConditionSet nodeData)
        {
            var boundaryConditionSets = parentNodeData as IList<BoundaryConditionSet>;
            if (boundaryConditionSets != null)
            {
                return boundaryConditionSets.Remove(nodeData);
            }

            var treeShortCut = parentNodeData as FmModelTreeShortcut;
            if (treeShortCut != null)
            {
                boundaryConditionSets = treeShortCut.Data as IList<BoundaryConditionSet>;
                if (boundaryConditionSets != null)
                {
                    return boundaryConditionSets.Remove(nodeData);
                }
            }
            return false;
        }

        public override bool CanRenameNode(DelftTools.Controls.ITreeNode node)
        {
            return true;
        }

        public override void OnNodeRenamed(BoundaryConditionSet data, string newName)
        {
            data.Feature.Name = newName;
        }
    }
}