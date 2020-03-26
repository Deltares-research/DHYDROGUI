using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration
{
    /// <summary>
    /// <see cref="GenerateSeries"/> is responsible for updating the time series in
    /// <see cref="IWaveEnergyFunction{TSpreading}"/> with new data through the
    /// <see cref="Execute{TSpreading}"/> method.
    /// </summary>
    public class GenerateSeries
    {
        private readonly IGenerateSeriesDialogHelper dialogHelper;

        /// <summary>
        /// Creates a new <see cref="GenerateSeries"/>.
        /// </summary>
        /// <param name="dialogHelper">The dialog helper.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dialogHelper"/> is <c>null</c>.
        /// </exception>
        public GenerateSeries(IGenerateSeriesDialogHelper dialogHelper)
        {
            Ensure.NotNull(dialogHelper, nameof(dialogHelper));
            this.dialogHelper = dialogHelper;
        }

        /// <summary>
        /// Executes the generation of a series given user input.
        /// </summary>
        /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
        /// <param name="owner">The owning window required for user prompts.</param>
        /// <param name="selectedFunction">The currently selected and active function.</param>
        /// <param name="otherFunctions">The other functions if any.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// Thrown when <paramref name="owner"/> or
        /// <paramref name="selectedFunction"/> are <c>null</c>.
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the selected <see cref="WaveSupportPointMode"/> is out of range.
        /// </exception>
        public void Execute<TSpreading>(IWin32Window owner,
                                        IWaveEnergyFunction<TSpreading> selectedFunction,
                                        IEnumerable<IWaveEnergyFunction<TSpreading>> otherFunctions = null)
            where TSpreading : IBoundaryConditionSpreading, new()
        {
            Ensure.NotNull(owner, nameof(owner));
            Ensure.NotNull(selectedFunction, nameof(selectedFunction));

            using (TimeSeriesGeneratorDialog response =
                dialogHelper.GetTimeSeriesGeneratorResponse(owner,
                                                            DateTime.Today,
                                                            DateTime.Today + TimeSpan.FromDays(1.0),
                                                            TimeSpan.FromHours(1.0)))
            {
                if (response.DialogResult != DialogResult.OK)
                {
                    return;
                }

                switch (GetSupportPointMode(owner, otherFunctions != null))
                {
                    case WaveSupportPointMode.SelectedActiveSupportPoint:
                        GenerateTimeSeries(response, selectedFunction);
                        break;
                    case WaveSupportPointMode.AllActiveSupportPoints:
                        GenerateTimeSeries(response, otherFunctions.Plus(selectedFunction).ToArray());
                        break;
                    case WaveSupportPointMode.NoSupportPoints:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private WaveSupportPointMode GetSupportPointMode(IWin32Window owner,
                                                         bool hasOtherFunctions) =>
            hasOtherFunctions
                ? dialogHelper.GetSupportPointSelectionMode(owner)
                : WaveSupportPointMode.SelectedActiveSupportPoint;

        private static void GenerateTimeSeries<TSpreading>(TimeSeriesGeneratorDialog dialog,
                                                           params IWaveEnergyFunction<TSpreading>[] functions)
            where TSpreading : IBoundaryConditionSpreading, new()
        {
            foreach (IWaveEnergyFunction<TSpreading> waveEnergyFunction in functions)
                dialog.Apply(waveEnergyFunction.TimeArgument);
        }
    }
}