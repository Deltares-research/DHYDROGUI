using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    /// <summary>
    /// <see cref="TimeDependentParameters{TSpreading}"/> provides the time-dependent parameters associated with
    /// a <see cref="IWaveBoundaryConditionDefinition"/> in the case of uniform data, or the
    /// parameters associated with a <see cref="GeometricDefinitions.SupportPoint"/> in the case
    /// of a spatially variant <see cref="IWaveBoundaryConditionDefinition"/>.
    /// </summary>
    /// <typeparam name="TSpreading">The type of spreading.</typeparam>
    /// <seealso cref="IForcingTypeDefinedParameters"/>
    public class TimeDependentParameters<TSpreading> : IForcingTypeDefinedParameters
        where TSpreading : IBoundaryConditionSpreading, new()
    {
        /// <summary>
        /// Creates a new <see cref="TimeDependentParameters{TSpreading}"/>.
        /// </summary>
        /// <param name="waveEnergyFunction">The wave energy function.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveEnergyFunction"/> is <c>null</c>.
        /// </exception>
        public TimeDependentParameters(IWaveEnergyFunction<TSpreading> waveEnergyFunction)
        {
            Ensure.NotNull(waveEnergyFunction, nameof(waveEnergyFunction));
            WaveEnergyFunction = waveEnergyFunction;
        }

        /// <summary>
        /// Gets the wave energy function.
        /// </summary>
        public IWaveEnergyFunction<TSpreading> WaveEnergyFunction { get; }

        public void AcceptVisitor(IForcingTypeDefinedParametersVisitor visitor)
        {
            Ensure.NotNull(visitor, nameof(visitor));
            visitor.Visit(this);
        }
    }
}