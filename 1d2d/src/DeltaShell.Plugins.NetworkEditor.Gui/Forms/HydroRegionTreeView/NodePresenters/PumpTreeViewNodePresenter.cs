using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    class PumpTreeViewNodePresenter : StructureViewNodePresenter<IPump>
    {
        private static readonly Image PumpSmallImage= Properties.Resources.PumpSmall;
        public PumpTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IPump data)
        {
            node.Image = PumpSmallImage;
            base.UpdateNode(parentNode, node, data);
        }
    }
}