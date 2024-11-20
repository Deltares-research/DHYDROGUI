using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class GreenhouseInitialConditionsDataRow : RainfallRunoffDataRow<GreenhouseData>
    {
        [Description("Area Id")]
        public string AreaName
        {
            get { return data.Name; }
        }

        [Description("Initial Roof Storage (mm)")]
        public double InitialRoofStorage
        {
            get { return data.InitialRoofStorage; }
            set { data.InitialRoofStorage = value; }
        }
    }
}