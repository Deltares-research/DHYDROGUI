using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.NGHS.IO.DataObjects.InitialConditions
{
    /// <summary>
    /// Spatial initial condition definition for a <see cref="IChannel"/>.
    /// </summary>
    [Entity]
    public class SpatialChannelInitialConditionDefinition
    {
        public SpatialChannelInitialConditionDefinition()
        {
            ConstantSpatialChannelInitialConditionDefinitions = new EventedList<ConstantSpatialChannelInitialConditionDefinition>();
        }
        
        public InitialConditionQuantity Quantity { get; set; }

        public IEventedList<ConstantSpatialChannelInitialConditionDefinition> ConstantSpatialChannelInitialConditionDefinitions { get; private set; }

        /// <summary>
        /// Copies the properties from another definition to this definition.
        /// </summary>
        /// <param name="otherDefinition">The definition to copy from.</param>
        public void CopyFrom(SpatialChannelInitialConditionDefinition otherDefinition)
        {
            Quantity = otherDefinition.Quantity;
            ConstantSpatialChannelInitialConditionDefinitions = otherDefinition.ConstantSpatialChannelInitialConditionDefinitions;
        }
    }
}