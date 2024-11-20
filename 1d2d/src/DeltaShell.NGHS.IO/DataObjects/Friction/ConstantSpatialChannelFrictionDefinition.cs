using DelftTools.Hydro;

namespace DeltaShell.NGHS.IO.DataObjects.Friction
{
    /// <summary>
    /// Spatial friction definition for a location on a <see cref="IChannel"/>.
    /// </summary>
    public class ConstantSpatialChannelFrictionDefinition
    {
        public double Chainage { get; set; }

        public double Value { get; set; }
    }
}