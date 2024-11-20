namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    /// <summary>
    /// <see cref="IBoundaryConditionShapeFactory"/> defines the interface to construct
    /// the different <see cref="IBoundaryConditionShape"/>.
    /// </summary>
    public interface IBoundaryConditionShapeFactory
    {
        /// <summary>
        /// Constructs the default <see cref="GaussShape"/>.
        /// </summary>
        /// <returns>
        /// The default <see cref="GaussShape"/>.
        /// </returns>
        GaussShape ConstructDefaultGaussShape();

        /// <summary>
        /// Constructs the default <see cref="JonswapShape"/>.
        /// </summary>
        /// <returns>
        /// The default <see cref="JonswapShape"/>.
        /// </returns>
        JonswapShape ConstructDefaultJonswapShape();

        /// <summary>
        /// Constructs the default <see cref="PiersonMoskowitzShape"/>.
        /// </summary>
        /// <returns>
        /// The default <see cref="PiersonMoskowitzShape"/>.
        /// </returns>
        PiersonMoskowitzShape ConstructDefaultPiersonMoskowitzShape();
    }
}