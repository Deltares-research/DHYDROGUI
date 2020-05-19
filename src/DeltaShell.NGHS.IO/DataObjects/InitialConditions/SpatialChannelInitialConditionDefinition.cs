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
    }
}