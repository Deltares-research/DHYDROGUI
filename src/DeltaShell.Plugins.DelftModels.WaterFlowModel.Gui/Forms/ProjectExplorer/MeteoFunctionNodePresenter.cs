using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.ProjectExplorer
{
    public class MeteoFunctionNodePresenter : TreeViewNodePresenterBaseForPluginGui<MeteoFunction>
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, MeteoFunction nodeData)
        {
            node.Text = "Meteo data";
            node.Image = Properties.Resources.Meteo;
        }
    }
}