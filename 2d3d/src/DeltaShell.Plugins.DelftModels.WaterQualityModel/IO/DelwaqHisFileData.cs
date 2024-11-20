using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class DelwaqHisFileData
    {
        private readonly string observationVariable;

        private readonly Dictionary<DateTime, List<double>>
            valuesPerTimeStep = new Dictionary<DateTime, List<double>>();

        public DelwaqHisFileData(string observationVariable)
        {
            this.observationVariable = observationVariable;
        }

        /// <summary>
        /// The observation variable
        /// </summary>
        public string ObservationVariable => observationVariable;

        /// <summary>
        /// The timesteps for which observation variable values are added
        /// </summary>
        public IEnumerable<DateTime> TimeSteps => valuesPerTimeStep.Keys;

        /// <summary>
        /// The output variables (substances or output parameters) for which observation variable values should be added per time
        /// step
        /// </summary>
        /// <remarks>
        /// The order in which observation variable values are added for a timestep should reflect the order of output
        /// variables (<see cref="AddValueForTimeStep"/>)
        /// </remarks>
        public string[] OutputVariables { get; set; }

        /// <summary>
        /// Adds a observation variable value for a specific time step
        /// </summary>
        /// <param name="timeStep"> The time step to add a observation variable value for </param>
        /// <param name="value"> The observation variable value to add </param>
        /// <remarks>
        /// The order in which observation variable values are added for a timestep should reflect the output variables
        /// order (<see cref="OutputVariables"/>)
        /// </remarks>
        public void AddValueForTimeStep(DateTime timeStep, double value)
        {
            if (!valuesPerTimeStep.ContainsKey(timeStep))
            {
                valuesPerTimeStep.Add(timeStep, new List<double>());
            }

            valuesPerTimeStep[timeStep].Add(value);
        }

        /// <summary>
        /// Returns the observation variable values for a specific timestep
        /// </summary>
        /// <param name="timeStep"> The timestep to get the observation variable values for </param>
        /// <returns> A list of observation variable values </returns>
        /// <remarks>
        /// If added correctly (<see cref="AddValueForTimeStep"/>), the observation variable values order reflects the
        /// output variables order (<see cref="OutputVariables"/>)
        /// </remarks>
        public List<double> GetValuesForTimeStep(DateTime timeStep)
        {
            return valuesPerTimeStep.ContainsKey(timeStep) ? valuesPerTimeStep[timeStep] : null;
        }

        /// <summary>
        /// Returns the time series values of a given output variable.
        /// </summary>
        /// <param name="outputVariableName">Name of the Output Variable to be found.</param>
        /// <returns>Collection of double values related to the given key.</returns>
        public IEnumerable<double> GetValuesForKey(string outputVariableName)
        {
            IEnumerable<double> defaultValue = Enumerable.Empty<double>();
            if (OutputVariables == null || !OutputVariables.Any())
            {
                return defaultValue;
            }

            int variableIndex = Array.IndexOf(OutputVariables, outputVariableName);
            return variableIndex == -1 ? defaultValue : valuesPerTimeStep.Values.Select(v => v[variableIndex]);
        }
    }
}