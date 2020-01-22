using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{

    public class DryWeatherFlow : Unique<long>
    {
        public virtual string DryWeatherFlowId { get; set; } // VER_IDE (debiet.csv)
        public virtual int NumberOfUnits { get; set; } // AVV_ENH (debiet.csv)
    }

}