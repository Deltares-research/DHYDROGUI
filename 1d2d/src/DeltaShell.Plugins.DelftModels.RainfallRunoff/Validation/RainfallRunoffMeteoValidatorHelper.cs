using System;
using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    /// <summary>
    /// <see cref="RainfallRunoffMeteoValidatorHelper"/> provides some common 
    /// predicates used in the validation of Meteo data.
    /// </summary>
    internal static class RainfallRunoffMeteoValidatorHelper
    {
        /// <summary>
        /// Whether <paramref name="timeArgument"/> has the correct number of values.
        /// </summary>
        /// <param name="timeArgument">The time argument to check.</param>
        /// <param name="startTime">The model start time.</param>
        /// <param name="stopTime">The model stop time.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="timeArgument"/> has at least two values, or 
        /// one value and the <paramref name="startTime"/> date is equal to the 
        /// <paramref name="stopTime"/> date; <c>false</c> otherwise .
        /// </returns>
        internal static bool HasCorrectNumberValues(this IVariable<DateTime> timeArgument,
                                                    DateTime startTime,
                                                    DateTime stopTime) =>
            timeArgument.Values.Count >= 2 ||
            timeArgument.Values.Count == 1 && startTime.Date.Equals(stopTime.Date);

        /// <summary>
        /// Whether <paramref name="timeArgument"/> has a correct starting time.
        /// </summary>
        /// <param name="timeArgument">The time argument to check.</param>
        /// <param name="startTime">The model start time.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="timeArgument"/> has values before or at the model 
        /// <paramref name="startTime"/>; <c>false</c> otherwise.
        /// </returns>
        internal static bool HasCorrectStartingTime(this IVariable<DateTime> timeArgument,
                                                    DateTime startTime) =>
            timeArgument.Values[0] <= startTime;

        /// <summary>
        /// Whether <paramref name="timeArgument"/> has a correct stop time.
        /// </summary>
        /// <param name="timeArgument">The time argument to check.</param>
        /// <param name="stopTime">The model stop time.</param>
        /// <param name="addTimeStep">Whether to add an additional time step.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="timeArgument"/> has values at or after the model 
        /// <paramref name="stopTime"/>; <c>false</c> otherwise.
        /// </returns>
        internal static bool HasCorrectStopTime(this IVariable<DateTime> timeArgument,
                                                DateTime stopTime,
                                                bool addTimeStep = false) =>
            timeArgument.Values.Count == 1 || timeArgument.GetMeteoEnd(addTimeStep) >= stopTime;

        /// <summary>
        /// Whether <paramref name="timeArgument"/> has a correct time step.
        /// </summary>
        /// <param name="timeArgument">The time argument to check.</param>
        /// <param name="timeStep">The model time step.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="timeArgument"/> has a correct time step, i.e.
        /// either it is a single value, or meteo data time step is some multiple of the
        /// model time step.
        /// </returns>
        internal static bool HasCorrectTimeStep(this IVariable<DateTime> timeArgument,
                                                TimeSpan timeStep) =>
            timeArgument.Values.Count == 1 || 
            timeStep.TotalSeconds == 0 || 
            timeArgument.GetMeteoTimeStep().TotalSeconds % timeStep.TotalSeconds == 0;

        /// <summary>
        /// Calculate the last value of <paramref name="timeArgument"/>.
        /// </summary>
        /// <param name="timeArgument">The time argument.</param>
        /// <param name="addTimeStep">Whether to add an additional time step.</param>
        /// <returns>The last value of <paramref name="timeArgument"/>.</returns>
        internal static DateTime GetMeteoEnd(this IVariable<DateTime> timeArgument,
                                             bool addTimeStep)
        {
            DateTime timeSeriesEnd = timeArgument.Values[timeArgument.Values.Count - 1];

            if (addTimeStep) timeSeriesEnd += timeArgument.GetMeteoTimeStep();

            return timeSeriesEnd;
        }

        /// <summary>
        /// Calculate the time step of the provided <paramref name="timeArgument"/>
        /// </summary>
        /// <param name="timeArgument">The time argument.</param>
        /// <returns>
        /// The time step of the provided <paramref name="timeArgument"/>.
        /// </returns>
        /// <remarks>
        /// This method assumes the <paramref name="timeArgument"/> has at least two values,
        /// and all values of the <paramref name="timeArgument"/> are equidistant.
        /// </remarks>
        internal static TimeSpan GetMeteoTimeStep(this IVariable<DateTime> timeArgument) =>
            (timeArgument.Values[1] - timeArgument.Values[0]);
    }
}
