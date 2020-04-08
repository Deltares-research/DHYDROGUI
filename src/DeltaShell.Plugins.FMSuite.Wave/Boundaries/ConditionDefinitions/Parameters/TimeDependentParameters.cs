using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters
{
    /// <summary>
    /// <see cref="TimeDependentParameters{TSpreading}"/> provides the time-dependent parameters associated with
    /// a <see cref="IWaveBoundaryConditionDefinition"/> in the case of uniform data, or the
    /// parameters associated with a <see cref="GeometricDefinitions.SupportPoint"/> in the case
    /// of a spatially variant <see cref="IWaveBoundaryConditionDefinition"/>.
    /// </summary>
    /// <typeparam name="TSpreading">The type of spreading.</typeparam>
    /// <seealso cref="IBoundaryConditionParameters"/>
    public class TimeDependentParameters<TSpreading> : IBoundaryConditionParameters
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

        /// <summary>
        /// Method for accepting visitors of the visitor design pattern,
        /// used for the export.
        /// Order is important for the corresponding actions.
        /// </summary>
        /// <param name="visitor"></param>
        public void AcceptVisitor(IParametersVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}