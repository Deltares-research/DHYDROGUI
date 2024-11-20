using System.ComponentModel;

namespace DeltaShell.NGHS.IO.DataObjects.InitialConditions
{
    /// <summary>
    /// Enumeration containing the different channel initial condition specification types.
    /// Each type specifies the "source" of the initial condition values that will be used
    /// from a per <see cref="IChannel"/> perspective.
    /// </summary>
    public enum ChannelInitialConditionSpecificationType
    {
        [Description("Use Global Value")]
        ModelSettings,
        [Description("Branch Constant")]
        ConstantChannelInitialConditionDefinition,
        [Description("Branch Chainages")]
        SpatialChannelInitialConditionDefinition
    }
}