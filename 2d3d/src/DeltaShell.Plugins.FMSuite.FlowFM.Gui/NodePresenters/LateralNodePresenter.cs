using System.Drawing;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    /// <summary>
    /// Node presenter for <see cref="Lateral"/> objects.
    /// </summary>
    public sealed class LateralNodePresenter : FMSuiteNodePresenterBase<Lateral>
    {
        private static readonly Bitmap lateralPointIcon = Resources.LateralPoint;
        private static readonly Bitmap lateralPolygonIcon = Resources.LateralPolygon;

        /// <summary>
        /// Whether or not this node can be renamed.
        /// </summary>
        /// <param name="node"> The tree node. </param>
        /// <returns>
        ///     <c>true</c>
        /// </returns>
        public override bool CanRenameNode(ITreeNode node) => true;

        /// <summary>
        /// Rename the lateral with the new name.
        /// </summary>
        /// <param name="data"> The lateral to rename. </param>
        /// <param name="newName"> The new name to give the lateral. </param>
        public override void OnNodeRenamed(Lateral data, string newName) => data.Name = newName;

        protected override string GetNodeText(Lateral data) => data.Name;

        protected override Image GetNodeImage(Lateral data)
        {
            return data.Feature.Geometry.Coordinates.Length == 1 ? lateralPointIcon : lateralPolygonIcon;
        }

        protected override bool CanRemove(Lateral nodeData) => true;

        protected override bool RemoveNodeData(object parentNodeData, Lateral nodeData)
        {
            if (!(parentNodeData is FmModelTreeShortcut treeShortCut))
            {
                return false;
            }

            treeShortCut.FlowFmModel.LateralFeatures.Remove(nodeData.Feature);
            ResetGuiSelection();

            return false;
        }
    }
}