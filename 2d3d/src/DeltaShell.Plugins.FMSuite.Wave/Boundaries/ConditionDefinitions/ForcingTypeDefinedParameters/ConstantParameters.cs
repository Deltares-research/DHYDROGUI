using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    /// <summary>
    /// <see cref="ConstantParameters{TSpreading}"/> provides the parameters
    /// associated with a <see cref="IWaveBoundaryConditionDefinition"/>
    /// in the case of uniform data, or the parameters associated with a
    /// <see cref="GeometricDefinitions.SupportPoint"/> in the case of a spatially variant
    /// <see cref="IWaveBoundaryConditionDefinition"/>.
    /// </summary>
    /// <seealso cref="IForcingTypeDefinedParameters"/>
    public class ConstantParameters<TSpreading> : IForcingTypeDefinedParameters
        where TSpreading : IBoundaryConditionSpreading, new()

    {
        private TSpreading spreading;

        /// <summary>
        /// Creates a new <see cref="ConstantParameters{TSpreading}"/>.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="period">The period.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="spreading">The spreading.</param>
        public ConstantParameters(double height,
                                  double period,
                                  double direction,
                                  TSpreading spreading)
        {
            Ensure.NotNull((IBoundaryConditionSpreading) spreading, nameof(spreading));

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
        public TSpreading Spreading
        {
            get => spreading;
            set
            {
                Ensure.NotNull((IBoundaryConditionSpreading) value, nameof(value));
                spreading = value;
            }
        }

        public void AcceptVisitor(IForcingTypeDefinedParametersVisitor visitor)
        {
            Ensure.NotNull(visitor, nameof(visitor));
            visitor.Visit(this);
        }
    }
}