using System.Drawing;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    public class
        WaterQualityObservationPointNodePresenter : TreeViewNodePresenterBase<IEventedList<WaterQualityObservationPoint>
        >
    {
        private static readonly Bitmap ObservationImage = Resources.observation;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node,
                                        IEventedList<WaterQualityObservationPoint> nodeData)
        {
            node.Text = "Observation Points";
            node.Image = ObservationImage;
        }
    }
}