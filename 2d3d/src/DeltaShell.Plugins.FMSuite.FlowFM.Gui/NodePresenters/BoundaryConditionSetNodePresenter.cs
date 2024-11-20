using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class BoundaryConditionSetNodePresenter : FMSuiteNodePresenterBase<BoundaryConditionSet>
    {
        private static readonly Bitmap BoundaryImage = Resources.boundary;

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override void OnNodeRenamed(BoundaryConditionSet data, string newName)
        {
            data.Feature.Name = newName;
        }

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
            if (boundaryConditionSets != null && boundaryConditionSets.Remove(nodeData))
            {
                ResetGuiSelection();
                return true;
            }

            var treeShortCut = parentNodeData as FmModelTreeShortcut;
            if (treeShortCut != null)
            {
                boundaryConditionSets = treeShortCut.Value as IList<BoundaryConditionSet>;
                if (boundaryConditionSets != null && boundaryConditionSets.Remove(nodeData))
                {
                    ResetGuiSelection();
                    return true;
                }
            }

            return false;
        }
    }
}