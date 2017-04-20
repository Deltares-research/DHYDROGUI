using DelftTools.Units;
using DelftTools.Units.Generics;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    // TODO: remove this class, it does not add any logic in addition to Parameter, it will make backward-compartibility of file format very hard
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
