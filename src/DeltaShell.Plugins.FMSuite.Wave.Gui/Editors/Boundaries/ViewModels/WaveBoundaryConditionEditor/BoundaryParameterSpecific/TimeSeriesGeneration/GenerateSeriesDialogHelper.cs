using System;
using System.Windows.Forms;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration
{
    /// <summary>
    /// <see cref="GenerateSeriesDialogHelper"/> implements the interface with
    /// which the dialogs of the generate series component can be created and shown.
    /// </summary>
    /// <seealso cref="IGenerateSeriesDialogHelper" />
    public class GenerateSeriesDialogHelper : IGenerateSeriesDialogHelper
    {
        public TimeSeriesGeneratorDialog GetTimeSeriesGeneratorResponse(IWin32Window owner, 
                                                                        DateTime startTime, 
                                                                        DateTime stopTime,
                                                                        TimeSpan timeStep)
        {
            var generateDialog = new TimeSeriesGeneratorDialog { ApplyOnAccept = false};
            generateDialog.SetData(null, startTime, stopTime, timeStep);
            generateDialog.ShowDialog(owner);

            return generateDialog;
        }

        public SupportPointSelectionForm GetSupportPointSelectionResponse(IWin32Window owner)
        {
            throw new NotImplementedException();
        }
    }
}