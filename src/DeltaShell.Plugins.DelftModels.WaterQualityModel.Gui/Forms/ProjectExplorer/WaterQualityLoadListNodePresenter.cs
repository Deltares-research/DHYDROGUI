using System.Drawing;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    public class WaterQualityLoadListNodePresenter : TreeViewNodePresenterBase<IEventedList<WaterQualityLoad>>
    {
        private static readonly Bitmap WeightImage = Properties.Resources.weight;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<WaterQualityLoad> nodeData)
        {
            node.Text = "Loads";
            node.Image = WeightImage;
        }
    }
}