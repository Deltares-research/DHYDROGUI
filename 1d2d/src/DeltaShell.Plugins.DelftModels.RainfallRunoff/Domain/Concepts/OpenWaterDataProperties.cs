using System.ComponentModel;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    public class OpenWaterDataProperties : ObjectProperties<OpenWaterData>
    {
        [Category("Area")]
        [DisplayName("Runoff area [m²]")]
        public double Area
        {
            get { return data.CalculationArea; }
            set { data.CalculationArea = value; }
        }

        [Category("Meteo")]
        [DisplayName("Meteo station")]
        public string MeteoStationName
        {
            get { return data.MeteoStationName; }
            set { data.MeteoStationName = value; }
        }

        [Category("Meteo")]
        [DisplayName("Area adjustment factor")]
        public double AreaAdjustmentFactor
        {
            get { return data.AreaAdjustmentFactor; }
            set { data.AreaAdjustmentFactor = value; }
        }
    }
}