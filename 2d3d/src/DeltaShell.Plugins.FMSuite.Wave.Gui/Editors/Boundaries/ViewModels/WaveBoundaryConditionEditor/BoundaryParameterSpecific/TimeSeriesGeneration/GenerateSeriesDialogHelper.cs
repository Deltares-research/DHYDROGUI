using System;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration
{
    /// <summary>
    /// <see cref="GenerateSeriesDialogHelper"/> implements the interface with
    /// which the dialogs of the generate series component can be created and shown.
    /// </summary>
    /// <seealso cref="IGenerateSeriesDialogHelper"/>
    public class GenerateSeriesDialogHelper : IGenerateSeriesDialogHelper
    {
        public TimeSeriesGeneratorDialog GetTimeSeriesGeneratorResponse(DateTime startTime,
                                                                        DateTime stopTime,
                                                                        TimeSpan timeStep)
        {
            var generateDialog = new TimeSeriesGeneratorDialog {ApplyOnAccept = false};
            generateDialog.SetData(null, startTime, stopTime, timeStep);
            generateDialog.ShowDialog(null);

            return generateDialog;
        }

        public WaveSupportPointMode GetSupportPointSelectionMode()
        {
            using (var supportPointsDialog = new WaveSupportPointSelectionForm())
            {
                supportPointsDialog.ShowDialog(null);

                return supportPointsDialog.SupportPointOperationMode;
            }
        }
    }
}