using System.Drawing;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    public class
        WaterQualityBloomFunctionWrapperNodePresenter : TreeViewNodePresenterBase<WaterQualityBloomFunctionWrapper>
    {
        private static Image image = Resources.Folder;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WaterQualityBloomFunctionWrapper nodeData)
        {
            node.Text = "Bloom Algae";
            node.Image = image;
        }
    }
}