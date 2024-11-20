using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class UnpavedInitialConditionsDataRow : RainfallRunoffDataRow<UnpavedData>
    {
        [Description("Area Id")]
        public string AreaName
        {
            get { return data.Name; }
        }

        [Description("Initial Land Storage (mm)")]
        public double InitialLandStorage
        {
            get { return data.InitialLandStorage; }
            set { data.InitialLandStorage = value; }
        }

        [Description("Initial Groundwater Level (m. bel. surf)")]
        public double InitialGroundwaterLevel
        {
            get { return data.InitialGroundWaterLevelConstant; }
            set { data.InitialGroundWaterLevelConstant = value; }
        }
    }
}