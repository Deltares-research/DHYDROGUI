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

        /// <summary>
        /// Copies the properties from another definition to this definition.
        /// </summary>
        /// <param name="otherDefinition">The definition to copy from.</param>
        public void CopyFrom(ConstantChannelInitialConditionDefinition otherDefinition)
        {
            Quantity = otherDefinition.Quantity;
            Value = otherDefinition.Value;
        }
    }
}