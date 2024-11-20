using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    public class WaterQualityObservationVariableOutputNodePresenter :
        TreeViewNodePresenterBase<WaterQualityObservationVariableOutput>
    {
        private static readonly Bitmap TimeSeriesFunctionEmptyIcon = Resources.TimeSeriesEmpty;
        private static readonly Bitmap TimeSeriesFunctionIcon = Resources.TimeSeries;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node,
                                        WaterQualityObservationVariableOutput nodeData)
        {
            node.Text = nodeData.Name;
            node.Image = nodeData.TimeSeriesList.Any(ts => ts.GetValues().Count == 0)
                             ? TimeSeriesFunctionEmptyIcon
                             : TimeSeriesFunctionIcon;
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return false;
        }
    }
}