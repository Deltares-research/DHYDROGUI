using DelftTools.Units;
using DelftTools.Units.Generics;

namespace DeltaShell.NGHS.IO.DataObjects
{
    public class FlowParameter : Parameter<double>
    {
        public FlowParameter()
        {
            Name = "flow";
            Unit = new Unit("m³/s", "m³/s");
        }


    }
}