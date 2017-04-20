using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui.Swf;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.ProjectExplorer
{
    public class ObservationPointNodePresenter : TreeViewNodePresenterBaseForPluginGui<ObservationPoint>
    {
        private static readonly Bitmap ObservationIcon = Properties.Resources.Observation;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ObservationPoint nodeData)
        {
            node.Text = nodeData.Name;
            node.Tag = nodeData;
            node.Image = ObservationIcon;
      
        }
    }
}
