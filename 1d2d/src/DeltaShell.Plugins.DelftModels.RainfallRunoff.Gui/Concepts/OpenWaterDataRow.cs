using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class OpenWaterDataRow : RainfallRunoffDataRow<OpenWaterData>
    {
        [Description("Area Id")]
        public string AreaName
        {
            get { return data.Name; }
        }


        [Description("Total area (m²)")]
        public double TotalArea
        {
            get { return data.CalculationArea; }
            set { data.CalculationArea = value; }
        }

        [Description("Meteo station")]
        public string MeteoStationName
        {
            get { return data.MeteoStationName; }
            set { data.MeteoStationName = value; }
        }

        [Description("Area adjustment factor")]
        public double AreaAdjustmentFactor
        {
            get { return data.AreaAdjustmentFactor; }
            set { data.AreaAdjustmentFactor = value; }
        }
    }
}