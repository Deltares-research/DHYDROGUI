using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.NGHS.IO.DataObjects.InitialConditions
{
    /// <summary>
    /// Constant initial condition definition for <see cref="IChannel"/>.
    /// </summary>
    [Entity]
    public class ConstantChannelInitialConditionDefinition
    {
        public InitialConditionQuantity Quantity { get; set; }

        public double Value { get; set; }
    }
}