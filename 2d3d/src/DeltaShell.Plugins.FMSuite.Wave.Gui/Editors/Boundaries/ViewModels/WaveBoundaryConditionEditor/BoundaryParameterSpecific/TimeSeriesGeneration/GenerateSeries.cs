using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Collections;
using Deltares.Infrastructure.API.Guards;
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

        public void Execute<TSpreading>(IWaveEnergyFunction<TSpreading> selectedFunction,
                                        IEnumerable<IWaveEnergyFunction<TSpreading>> otherFunctions = null)
            where TSpreading : IBoundaryConditionSpreading, new()
        {
            Ensure.NotNull(selectedFunction, nameof(selectedFunction));

            using (TimeSeriesGeneratorDialog response =
                dialogHelper.GetTimeSeriesGeneratorResponse(referenceDateTimeProvider.ModelReferenceDateTime,
                                                            referenceDateTimeProvider.ModelReferenceDateTime + TimeSpan.FromDays(1.0),
                                                            TimeSpan.FromDays(1.0)))
            {
                if (response.DialogResult != DialogResult.OK)
                {
                    return;
                }

                switch (GetSupportPointMode(otherFunctions != null))
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
                        throw new NotSupportedException("The selected support point mode is not supported.");
                }
            }
        }

        private WaveSupportPointMode GetSupportPointMode(bool hasOtherFunctions) =>
            hasOtherFunctions
                ? dialogHelper.GetSupportPointSelectionMode()
                : WaveSupportPointMode.SelectedActiveSupportPoint;

        private static void GenerateTimeSeries<TSpreading>(TimeSeriesGeneratorDialog dialog,
                                                           params IWaveEnergyFunction<TSpreading>[] functions)
            where TSpreading : IBoundaryConditionSpreading, new()
        {
            foreach (IWaveEnergyFunction<TSpreading> waveEnergyFunction in functions)
            {
                dialog.Apply(waveEnergyFunction.TimeArgument);
            }
        }
    }
}