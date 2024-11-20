using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.NGHS.IO.DataObjects.Friction
{
    /// <summary>
    /// Constant friction definition for <see cref="IChannel"/>.
    /// </summary>
    [Entity]
    public class ConstantChannelFrictionDefinition
    {
        public RoughnessType Type { get; set; }

        public double Value { get; set; }
    }
}
