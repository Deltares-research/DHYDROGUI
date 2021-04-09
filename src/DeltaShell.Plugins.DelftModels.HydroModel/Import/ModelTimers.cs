using System;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    /// <summary>
    /// Model timers containing the start time, time step and stop time.
    /// </summary>
    public sealed class ModelTimers
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTimers"/> class.
        /// </summary>
        /// <param name="startTime"> The start time. </param>
        /// <param name="timeStep"> The time step. </param>
        /// <param name="stopTime"> The stop time. </param>
        public ModelTimers(DateTime startTime, TimeSpan timeStep, DateTime stopTime)
        {
            StartTime = startTime;
            TimeStep = timeStep;
            StopTime = stopTime;
        }

        /// <summary>
        /// The start time.
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// The time step.
        /// </summary>
        public TimeSpan TimeStep { get; }

        /// <summary>
        /// The stop time.
        /// </summary>
        public DateTime StopTime { get; }
    }
}