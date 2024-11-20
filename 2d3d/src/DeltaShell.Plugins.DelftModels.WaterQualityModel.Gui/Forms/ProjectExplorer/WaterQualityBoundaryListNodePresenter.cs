using System.Drawing;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    public class WaterQualityBoundaryListNodePresenter : TreeViewNodePresenterBase<IEventedList<WaterQualityBoundary>>
    {
        private static readonly Bitmap bordderImage = Resources.border_left;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node,
                                        IEventedList<WaterQualityBoundary> nodeData)
        {
            node.Text = "Boundaries";
            node.Image = bordderImage;
        }
    }
}