using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions
{
    /// <summary>
    /// <see cref="BoundaryConditionParameters"/> provides the parameters
    /// associated with a <see cref="IWaveBoundaryConditionDefinition"/>
    /// in the case of uniform data, or the parameters associated with a
    /// <see cref="SupportPoint"/> in the case of a spatially variant
    /// <see cref="IWaveBoundaryConditionDefinition"/>.
    /// </summary>
    public class BoundaryConditionParameters
    {
        /// <summary>
        /// Creates a new <see cref="BoundaryConditionParameters"/>.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="period">The period.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="spreading">The spreading.</param>
        public BoundaryConditionParameters(double height,
                                           double period,
                                           double direction,
                                           double spreading)
        {
            // TODO (MWT) verify what the correct values are for doubles in here
            Height = height;
            Period = period;
            Direction = direction;
            Spreading = spreading;
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        public double Period { get; set; }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        public double Direction { get; set; }

        /// <summary>
        /// Gets or sets the spreading.
        /// </summary>
        public double Spreading { get; set; }
    }
}