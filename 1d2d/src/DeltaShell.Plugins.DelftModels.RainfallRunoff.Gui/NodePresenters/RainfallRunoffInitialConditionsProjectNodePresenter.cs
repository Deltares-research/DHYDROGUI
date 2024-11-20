using System.Drawing;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.NodePresenters
{
    public class RainfallRunoffInitialConditionsProjectNodePresenter : TreeViewNodePresenterBase<RRInitialConditionsWrapper>
    {
        private static readonly Bitmap UnpavedIcon = Resources.unpaved;
        private static readonly Bitmap PavedIcon = Resources.paved;
        private static readonly Bitmap GreenhouseIcon = Resources.greenhouse;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, RRInitialConditionsWrapper nodeData)
        {
            node.Text = nodeData.Name;
            Bitmap image = null;
            switch (nodeData.Type)
            {
                case RRInitialConditionsWrapper.InitialConditionsType.Unpaved:
                    image = UnpavedIcon;
                    break;
                case RRInitialConditionsWrapper.InitialConditionsType.Paved:
                    image = PavedIcon;
                    break;
                case RRInitialConditionsWrapper.InitialConditionsType.Greenhouse:
                    image = GreenhouseIcon;
                    break;
            }
            node.Image = image;
        }
    }
}