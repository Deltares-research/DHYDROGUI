namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters
{
    /// <summary>
    /// <see cref="BoundaryParametersFactory"/> provides the interface with which to construct
    /// <see cref="IBoundaryConditionParameters"/>.
    /// </summary>
    public sealed class BoundaryParametersFactory : IBoundaryParametersFactory
    {
        public ConstantParameters ConstructDefaultConstantParameters() =>
            ConstructConstantParameters(0.0, 1.0, 0.0, 4.0);

        public ConstantParameters ConstructConstantParameters(double height, double period, double direction, double spreading)
        {
            return new ConstantParameters(height, period, direction, spreading);
        }
    }
}