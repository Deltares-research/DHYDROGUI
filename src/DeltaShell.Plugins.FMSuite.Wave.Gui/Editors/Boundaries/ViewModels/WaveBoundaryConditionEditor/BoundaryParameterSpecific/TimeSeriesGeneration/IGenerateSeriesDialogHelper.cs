using System;
using System.Windows.Forms;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration
{
    /// <summary>
    /// <see cref="IGenerateSeriesDialogHelper"/> defines the interface with
    /// which the dialogs of the generate series component can be created and shown.
    /// </summary>
    public interface IGenerateSeriesDialogHelper
    {
        /// <summary>
        /// Prompts and returns the user for a generate time series action.
        /// </summary>
        /// <param name="owner">The owning window</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="stopTime">The stop time.</param>
        /// <param name="timeStep">The time step.</param>
        /// <returns>
        /// The <see cref="TimeSeriesGeneratorDialog"/> after querying the user.
        /// </returns>
        TimeSeriesGeneratorDialog GetTimeSeriesGeneratorResponse(IWin32Window owner,
                                                                 DateTime startTime, 
                                                                 DateTime stopTime, 
                                                                 TimeSpan timeStep);

        /// <summary>
        /// Prompts and returns the user for a support point selection.
        /// </summary>
        /// <returns>
        /// The <see cref="WaveSupportPointSelectionForm"/> after querying the user.
        /// </returns>
        WaveSupportPointSelectionForm GetSupportPointSelectionResponse(IWin32Window owner);
    }
}