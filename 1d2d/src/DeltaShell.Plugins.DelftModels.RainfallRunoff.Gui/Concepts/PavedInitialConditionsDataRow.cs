using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class PavedInitialConditionsDataRow : RainfallRunoffDataRow<PavedData>
    {
        [Description("Area Id")]
        public string AreaName { get { return data.Name; } }

        [Description("Initial Street Storage (mm)")]
        public double InitialStreetStorage
        {
            get { return data.InitialStreetStorage; }
            set { data.InitialStreetStorage = value; }
        }
    }
}