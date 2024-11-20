using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.NodePresenters
{
    public class MeteoDataProjectNodePresenter :
        TreeViewNodePresenterBaseForPluginGui<MeteoData>
    {
        private static readonly Image imgPrecipitation = Properties.Resources.Precipitation;
        private static readonly Image imgEvaporation = Properties.Resources.Evaporation;
        private static readonly Image imgTemperature = Properties.Resources.thermometer;

        public MeteoDataProjectNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node,
                                        MeteoData nodeData)
        {
            node.Image = GetImageForType(nodeData);

            node.Text = nodeData.Name + " (" + nodeData.DataDistributionType.GetDescription() + ")";
        }

        private static Image GetImageForType(MeteoData meteoData)
        {
            if (meteoData == null)
            {
                return null;
            }

            if (meteoData.Name == "Precipitation")
            {
                return imgPrecipitation;
            }

            if (meteoData.Name == "Evaporation")
            {
                return imgEvaporation;
            }

            if (meteoData.Name == "Temperature")
            {
                return imgTemperature;
            }

            return null;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            return GuiPlugin == null ? null : GuiPlugin.GetContextMenu(null, nodeData);
        }
    }
}