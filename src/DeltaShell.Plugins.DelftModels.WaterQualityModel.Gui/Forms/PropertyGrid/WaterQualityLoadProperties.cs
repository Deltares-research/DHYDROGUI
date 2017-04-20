using System.ComponentModel;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Load")]
    public class WaterQualityLoadProperties : NameblePointFeatureProperties
    {
        private WaterQualityLoad WaterQualityLoad
        {
            get { return (WaterQualityLoad)data; }
        }

        [Category("General")]
        [DisplayName("Load type")]
        [PropertyOrder(2)]
        public string LoadType
        {
            get { return WaterQualityLoad.LoadType; }
            set { WaterQualityLoad.LoadType = value; }
        }

        [Category("General")]
        [DisplayName("Location Aliases")]
        [Description("Comma separated list of location aliases. Example: bouy 1, bouy 2, factory")]
        [PropertyOrder(3)]
        public string LocationAliases
        {
            get { return WaterQualityLoad.LocationAliases; }
            set { WaterQualityLoad.LocationAliases = value; }
        }
    }
}