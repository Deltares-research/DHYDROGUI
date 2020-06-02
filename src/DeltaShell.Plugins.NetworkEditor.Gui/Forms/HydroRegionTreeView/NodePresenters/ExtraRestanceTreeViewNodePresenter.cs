using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    internal class ExtraRestanceTreeViewNodePresenter : StructureViewNodePresenter<IExtraResistance>
    {
        private static readonly Image ExtraResistanceSmallImage = Resources.ExtraResistanceSmall;

        public ExtraRestanceTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin) {}

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IExtraResistance data)
        {
            node.Image = ExtraResistanceSmallImage;
            base.UpdateNode(parentNode, node, data);
        }
    }
}