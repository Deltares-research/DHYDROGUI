using DelftTools.Functions;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters
{
    /// <summary>
    /// <see cref="TimeDependentParameters"/> provides the time-dependent parameters associated with
    /// a <see cref="IWaveBoundaryConditionDefinition"/> in the case of uniform data, or the
    /// parameters associated with a <see cref="GeometricDefinitions.SupportPoint"/> in the case
    /// of a spatially variant <see cref="IWaveBoundaryConditionDefinition"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionParameters"/>
    public class TimeDependentParameters : IBoundaryConditionParameters
    {
        /// <summary>
        /// Creates a new <see cref="TimeDependentParameters"/>.
        /// </summary>
        /// <param name="waveEnergyFunction">The wave energy function.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveEnergyFunction"/> is <c>null</c>.
        /// </exception>
        public TimeDependentParameters(IFunction waveEnergyFunction)
        {
            Ensure.NotNull(waveEnergyFunction, nameof(waveEnergyFunction));
            WaveEnergyFunction = waveEnergyFunction;
        }

        /// <summary>
        /// Gets the wave energy function.
        /// </summary>
        public IFunction WaveEnergyFunction { get; }
    }
}