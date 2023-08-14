using System;
using DelftTools.Functions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Laterals.TimeSeriesGeneration
{
    /// <summary>
    /// Provides the interface to show a dialog that allows the user to generate time series.
    /// </summary>
    public interface ITimeSeriesGeneratorDialogService
    {
        /// <summary>
        /// When called, this method will show a time series generation dialog, with the provided default start time, stop time
        /// and time step.
        /// </summary>
        /// <param name="startTime"> The time series start time. </param>
        /// <param name="stopTime"> The time series stop time. </param>
        /// <param name="timeStep"> The time series time step. </param>
        /// <param name="timeSeries"> The time series to set the generated time series on. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="timeSeries"/> is <c>null</c>.
        /// </exception>
        void Execute(DateTime startTime,
                     DateTime stopTime,
                     TimeSpan timeStep,
                     TimeSeries timeSeries);
    }
}