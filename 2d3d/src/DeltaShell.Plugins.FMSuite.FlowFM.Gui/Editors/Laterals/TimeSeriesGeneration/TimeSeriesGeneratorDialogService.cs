using System;
using DelftTools.Functions;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Laterals.TimeSeriesGeneration
{
    /// <summary>
    /// Implements the interface to show a dialog that allows the user to generate time series.
    /// </summary>
    public class TimeSeriesGeneratorDialogService : ITimeSeriesGeneratorDialogService
    {
        /// <inheritdoc/>
        public void Execute(DateTime startTime, DateTime stopTime, TimeSpan timeStep, TimeSeries timeSeries)
        {
            Ensure.NotNull(timeSeries, nameof(timeSeries));

            using (var generateDialog = new TimeSeriesGeneratorDialog { ApplyOnAccept = true })
            {
                generateDialog.SetData(timeSeries.Time, startTime, stopTime, timeStep);
                generateDialog.ShowDialog(null);
            }
        }
    }
}