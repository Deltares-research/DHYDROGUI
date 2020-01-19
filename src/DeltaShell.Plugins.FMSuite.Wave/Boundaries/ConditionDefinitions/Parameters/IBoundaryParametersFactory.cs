namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters
{
    /// <summary>
    /// <see cref="IBoundaryParametersFactory"/> defines the interface with which to construct
    /// <see cref="IBoundaryConditionParameters"/>.
    /// </summary>
    public interface IBoundaryParametersFactory
    {
        /// <summary>
        /// Construct a new <see cref="ConstantParameters"/> instance with default values.
        /// </summary>
        ConstantParameters ConstructDefaultConstantParameters();

        /// <summary>
        /// Construct a new <see cref="ConstantParameters"/> instance.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="period">The period.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="spreading">The spreading.</param>
        ConstantParameters ConstructConstantParameters(double height,
                                                       double period,
                                                       double direction,
                                                       double spreading);
    }
}