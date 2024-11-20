using System.Collections;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class WasteWaterTreatmentPlantsTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<IEventedList<WasteWaterTreatmentPlant>>
    {
        public WasteWaterTreatmentPlantsTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }

        private static readonly Image WWTPImage = Properties.Resources.wwtp;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<WasteWaterTreatmentPlant> nodeData)
        {
            node.Text = "Wastewater Treatment Plants";
            node.Image = WWTPImage;
        }

        public override IEnumerable GetChildNodeObjects(IEventedList<WasteWaterTreatmentPlant> parentNodeData, ITreeNode node)
        {
            return parentNodeData.Cast<object>();
        }
    }
}