namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    /// <summary>
    /// <see cref="BoundaryConditionShapeFactory"/> implements the interface to construct
    /// the different <see cref="IBoundaryConditionShape"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionShapeFactory"/>
    public class BoundaryConditionShapeFactory : IBoundaryConditionShapeFactory
    {
        public GaussShape ConstructDefaultGaussShape()
        {
            return new GaussShape() {GaussianSpread = 0.1};
        }

        public JonswapShape ConstructDefaultJonswapShape()
        {
            return new JonswapShape() {PeakEnhancementFactor = 3.3};
        }

        public PiersonMoskowitzShape ConstructDefaultPiersonMoskowitzShape()
        {
            return new PiersonMoskowitzShape();
        }
    }
}