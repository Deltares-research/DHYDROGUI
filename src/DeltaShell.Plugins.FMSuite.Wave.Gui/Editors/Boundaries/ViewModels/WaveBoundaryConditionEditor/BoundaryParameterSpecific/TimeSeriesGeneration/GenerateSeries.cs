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
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration
{
    /// <summary>
    /// <see cref="GenerateSeries"/> is responsible for updating the time series in
    /// <see cref="IWaveEnergyFunction{TSpreading}"/> with new data through the
    /// <see cref="Execute{TSpreading}"/> method.
    /// </summary>
    public class GenerateSeries : IGenerateSeries
    {
        private readonly IGenerateSeriesDialogHelper dialogHelper;
        private readonly IReferenceDateTimeProvider referenceDateTimeProvider;

        /// <summary>
        /// Creates a new <see cref="GenerateSeries"/>.
        /// </summary>
        /// <param name="dialogHelper">The dialog helper.</param>
        /// <param name="referenceDateTimeProvider">The reference date time provider.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dialogHelper"/> or
        /// <paramref name="referenceDateTimeProvider"/> is <c>null</c>.
        /// </exception>
        public GenerateSeries(IGenerateSeriesDialogHelper dialogHelper,
                              IReferenceDateTimeProvider referenceDateTimeProvider)
        {
            Ensure.NotNull(dialogHelper, nameof(dialogHelper));
            Ensure.NotNull(referenceDateTimeProvider, nameof(referenceDateTimeProvider));
            
            this.dialogHelper = dialogHelper;
            this.referenceDateTimeProvider = referenceDateTimeProvider;
        }

        public void Execute<TSpreading>(IWin32Window owner,
                                        IWaveEnergyFunction<TSpreading> selectedFunction,
                                        IEnumerable<IWaveEnergyFunction<TSpreading>> otherFunctions = null)
            where TSpreading : IBoundaryConditionSpreading, new()
        {
            Ensure.NotNull(owner, nameof(owner));
            Ensure.NotNull(selectedFunction, nameof(selectedFunction));

            using (TimeSeriesGeneratorDialog response =
                dialogHelper.GetTimeSeriesGeneratorResponse(owner,
                                                            referenceDateTimeProvider.ModelReferenceDateTime,
                                                            referenceDateTimeProvider.ModelReferenceDateTime + TimeSpan.FromDays(1.0),
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
                        throw new ArgumentOutOfRangeException(nameof(GetSupportPointMode));
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