using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    internal class PumpTreeViewNodePresenter : StructureViewNodePresenter<IPump>
    {
        private static readonly Image PumpSmallImage = Resources.PumpSmall;

        public PumpTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin) {}

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IPump data)
        {
            node.Image = PumpSmallImage;
            base.UpdateNode(parentNode, node, data);
        }
    }
}