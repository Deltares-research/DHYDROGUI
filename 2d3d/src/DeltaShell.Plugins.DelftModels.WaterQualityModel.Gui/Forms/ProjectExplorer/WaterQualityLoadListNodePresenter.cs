using System.Drawing;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    public class WaterQualityLoadListNodePresenter : TreeViewNodePresenterBase<IEventedList<WaterQualityLoad>>
    {
        private static readonly Bitmap WeightImage = Resources.weight;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<WaterQualityLoad> nodeData)
        {
            node.Text = "Loads";
            node.Image = WeightImage;
        }
    }
}