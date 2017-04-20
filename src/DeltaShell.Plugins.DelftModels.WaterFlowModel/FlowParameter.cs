using DelftTools.Units;
using DelftTools.Units.Generics;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    // TODO: remove this class, it does not add any logic in addition to Parameter, it will make backward-compartibility of file format very hard
    public class FlowParameter : Parameter<double>
    {
        public FlowParameter()
        {
            Name = "flow";
            Unit = new Unit("m³/s", "m³/s");
        }


    }
}