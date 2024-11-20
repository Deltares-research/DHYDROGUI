using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class OpenWaterInitialConditionsDataRow : RainfallRunoffDataRow<OpenWaterData>
    {
        [Description("Area Id")]
        public string AreaName 
        { 
            get { return data.Name; }
        }

        [Description("Total Area (m²)")]
        public double TotalArea
        {
            get { return data.CalculationArea; }
            set { data.CalculationArea = value; }
        }
    }
}