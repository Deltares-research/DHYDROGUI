using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration
{
    /// <summary>
    /// <see cref="IGenerateSeries"/> defines the interface for updating the time series in
    /// <see cref="IWaveEnergyFunction{TSpreading}"/> with new data through the
    /// <see cref="Execute{TSpreading}"/> method.
    /// </summary>
    public interface IGenerateSeries
    {
        /// <summary>
        /// Executes the generation of a series given user input.
        /// </summary>
        /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
        /// <param name="selectedFunction">The currently selected and active function.</param>
        /// <param name="otherFunctions">The other functions if any.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="selectedFunction"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when the selected <see cref="Forms.WaveSupportPointMode"/> is out of range.
        /// </exception>
        void Execute<TSpreading>(IWaveEnergyFunction<TSpreading> selectedFunction,
                                 IEnumerable<IWaveEnergyFunction<TSpreading>> otherFunctions = null)
            where TSpreading : IBoundaryConditionSpreading, new();
    }
}