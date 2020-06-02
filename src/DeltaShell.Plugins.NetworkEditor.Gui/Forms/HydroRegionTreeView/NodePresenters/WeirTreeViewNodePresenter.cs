using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    internal class WeirTreeViewNodePresenter : StructureViewNodePresenter<IWeir>
    {
        private static readonly Image WeirSmallImage = Resources.WeirSmall;

        public WeirTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin) {}

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IWeir data)
        {
            node.Image = WeirSmallImage;
            base.UpdateNode(parentNode, node, data);
        }
    }
}