using System.Drawing;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class HeatFluxModelNodePresenter : FMSuiteNodePresenterBase<HeatFluxModel>
    {
        private static readonly Bitmap MeteoImage = Resources.meteo;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, HeatFluxModel nodeData)
        {
            node.Text = nodeData.MeteoData.Name;
            if (nodeData.Type == HeatFluxModelType.Composite || nodeData.Type == HeatFluxModelType.ExcessTemperature)
            {
                node.Image = MeteoImage;
            }
        }

        protected override string GetNodeText(HeatFluxModel data)
        {
            return data.MeteoData.Name;
        }

        protected override Image GetNodeImage(HeatFluxModel data)
        {
            return MeteoImage;
        }
    }
}