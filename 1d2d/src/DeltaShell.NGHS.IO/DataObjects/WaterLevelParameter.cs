using DelftTools.Units;
using DelftTools.Units.Generics;

namespace DeltaShell.NGHS.IO.DataObjects
{
    // Class is now needed to provide for linking based on type. Change this framework logic to work on instances instead of types
    public class WaterLevelParameter:Parameter<double>
    {
        public WaterLevelParameter()
        {
            Name = "water level";
            Unit = new Unit("m", "m");
        }
    }
}
