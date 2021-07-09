using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Editing;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    class LateralSourceTreeViewNodePresenter : BranchFeatureTreeViewNodePresenterBase<LateralSource>
    {
        private readonly Image image = Properties.Resources.LateralSourceSmall;

        public LateralSourceTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }

        #region ITreeNodePresenter Members

        protected override bool CanRemove(LateralSource nodeData)
        {
            return true;
        }

        protected override Image GetImage(LateralSource feature) => image;

        protected override bool RemoveNodeData(object parentNodeData, LateralSource source)
        {
            var network = source.Network;
            network.BeginEdit("Remove " + source.Name);
            
            var links = source.Links.ToArray();
            foreach (var link in links)
            {
                HydroRegion.RemoveLink(link);
            }

            source.Branch.BranchFeatures.Remove(source);

            network.EndEdit();
            return true;
        }

        /// <summary>
        /// Lateral sources can be renamed by the user
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        #endregion
    }
}