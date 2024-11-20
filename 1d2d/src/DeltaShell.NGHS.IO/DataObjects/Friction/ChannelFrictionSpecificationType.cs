using System.ComponentModel;
using DelftTools.Hydro;

namespace DeltaShell.NGHS.IO.DataObjects.Friction
{
    /// <summary>
    /// Enumeration containing the different channel friction specification types.
    /// Each type specifies the "source" of the friction values that will be used
    /// from a per <see cref="IChannel"/> perspective.
    /// </summary>
    public enum ChannelFrictionSpecificationType
    {
        [Description("Use Global Value")]
        ModelSettings,
        [Description("Branch Constant")]
        ConstantChannelFrictionDefinition,
        [Description("Branch Chainages")]
        SpatialChannelFrictionDefinition,
        [Description("On Lanes")]
        RoughnessSections,
        [Description("On Cross Sections")]
        CrossSectionFrictionDefinitions
    }
}