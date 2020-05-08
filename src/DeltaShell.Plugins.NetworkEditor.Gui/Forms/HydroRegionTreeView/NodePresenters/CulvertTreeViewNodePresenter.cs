using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    internal class CulvertTreeViewNodePresenter : StructureViewNodePresenter<Culvert>
    {
        private static readonly Image CulvertSmallImage = Resources.CulvertSmall;

        public CulvertTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin) {}

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, Culvert data)
        {
            node.Image = CulvertSmallImage;
            base.UpdateNode(parentNode, node, data);
        }
    }
}