using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class WasteWaterTreatmentPlantTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<WasteWaterTreatmentPlant>
    {
        private static readonly Image WWTPImage = Properties.Resources.wwtp;

        public WasteWaterTreatmentPlantTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WasteWaterTreatmentPlant nodeData)
        {
            node.Image = WWTPImage;
            UpdateNodeText(nodeData, node);
        }

        protected override bool CanRemove(WasteWaterTreatmentPlant nodeData)
        {
            return true;
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, WasteWaterTreatmentPlant source)
        {
            source.Basin.WasteWaterTreatmentPlants.Remove(source);
            return true;
        }

        private static void UpdateNodeText(WasteWaterTreatmentPlant source, ITreeNode node)
        {
            node.Text = string.IsNullOrEmpty(source.Name)
                            ? string.Format("<no name>")
                            : string.Format("{0}", source.Name);
        }
    }
}