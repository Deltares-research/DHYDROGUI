using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    class BridgeTreeViewNodePresenter : StructureViewNodePresenter<IBridge>
    {
        private static readonly Image BridgeSmallImage = Properties.Resources.BridgeSmall;
        public BridgeTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IBridge data)
        {
            node.Image = BridgeSmallImage;
            base.UpdateNode(parentNode, node, data);
        }
    }
}