namespace DeltaShell.NGHS.IO.DataObjects.InitialConditions
{
    /// <summary>
    /// Spatial initial condition definition for a location on a <see cref="IChannel"/>.
    /// </summary>
    public class ConstantSpatialChannelInitialConditionDefinition
    {
        public double Chainage { get; set; }
        public double Value { get; set; }
    }
}