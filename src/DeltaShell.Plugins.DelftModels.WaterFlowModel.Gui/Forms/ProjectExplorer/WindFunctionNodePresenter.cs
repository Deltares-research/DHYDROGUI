using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.ProjectExplorer
{
    public class WindFunctionNodePresenter : TreeViewNodePresenterBaseForPluginGui<WindFunction>
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WindFunction nodeData)
        {
            node.Text = "Wind";
            node.Image = Properties.Resources.Wind;
        }
    }
}
