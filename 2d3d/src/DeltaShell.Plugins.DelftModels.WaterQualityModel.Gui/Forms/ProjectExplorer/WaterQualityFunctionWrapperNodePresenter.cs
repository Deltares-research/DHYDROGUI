using System.Drawing;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    /// <summary>
    /// Water quality function wrapper node presenter
    /// </summary>
    public class WaterQualityFunctionWrapperNodePresenter : TreeViewNodePresenterBase<WaterQualityFunctionWrapper>
    {
        private static readonly Bitmap TimeSeriesImage = Resources.TimeSeries;
        private static readonly Bitmap LocationFunctionImage = Resources.LocationFunction;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WaterQualityFunctionWrapper nodeData)
        {
            node.Text = nodeData.Function.Name;
            node.Image = GetImage(nodeData.Function);
        }

        private static Image GetImage(IFunction function)
        {
            if (function.IsTimeSeries())
            {
                return TimeSeriesImage;
            }

            if (function.IsNetworkCoverage())
            {
                return LocationFunctionImage;
            }

            if (function.IsUnstructuredGridCellCoverage())
            {
                return LocationFunctionImage;
            }

            return null;
        }
    }
}