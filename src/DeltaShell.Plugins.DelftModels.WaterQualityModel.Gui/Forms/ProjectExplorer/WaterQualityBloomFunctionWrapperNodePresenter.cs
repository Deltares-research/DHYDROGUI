using System.Drawing;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    public class WaterQualityBloomFunctionWrapperNodePresenter : TreeViewNodePresenterBase<WaterQualityBloomFunctionWrapper>
    {
        private static Image image = Properties.Resources.Folder;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WaterQualityBloomFunctionWrapper nodeData)
        {
            node.Text = "Bloom Algae";
            node.Image = image;
        }
    }
}