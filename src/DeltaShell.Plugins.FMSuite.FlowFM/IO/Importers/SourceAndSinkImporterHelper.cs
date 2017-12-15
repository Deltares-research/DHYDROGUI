using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public static class SourceAndSinkImporterHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SourceAndSinkImporterHelper));

        /// <summary>
        /// SourceAndSink timeseries information can be imported with only some components specified in the file
        /// This method is designed to determine which components the imported data belongs to and to fill the 'missing' data with default values
        /// </summary>
        /// <param name="readFunction"></param>
        /// <param name="componentSettings"></param>
        /// <returns>bool based on success or failure of operation</returns>
        public static bool AdaptComponentValuesFromFileToSourceAndSinkFunction(IFunction readFunction, IDictionary<string, bool> componentSettings)
        {
            var timeVariable = readFunction.Arguments?.FirstOrDefault(a => a.Name.Equals(SourceAndSink.TimeVariableName, StringComparison.InvariantCultureIgnoreCase));
            var dischargeComponent = readFunction.Components?.FirstOrDefault(a => a.Name.Equals(SourceAndSink.DischargeVariableName, StringComparison.InvariantCultureIgnoreCase));
            var salinityComponent = readFunction.Components?.FirstOrDefault(a => a.Name.Equals(SourceAndSink.SalinityVariableName, StringComparison.InvariantCultureIgnoreCase));
            var temperatureComponent = readFunction.Components?.FirstOrDefault(a => a.Name.Equals(SourceAndSink.TemperatureVariableName, StringComparison.InvariantCultureIgnoreCase));

            if (timeVariable?.Values == null || dischargeComponent?.Values == null || salinityComponent?.Values == null || temperatureComponent?.Values == null)
            {    
                Log.ErrorFormat("Invalid Variables detected in imported SourceAndSink Function: {0}", readFunction.Name);
                return false;
            }
           
            ProcessSalinityAndTemperatureComponents(salinityComponent, temperatureComponent, componentSettings);

            var numTimeSteps = timeVariable.Values.Count;
            ValidateComponentValues<double>(dischargeComponent, numTimeSteps);
            ValidateComponentValues<double>(salinityComponent, numTimeSteps);
            ValidateComponentValues<double>(temperatureComponent, numTimeSteps);

            return true;
        }

        private static void ProcessSalinityAndTemperatureComponents(IVariable salinityComponent, IVariable temperatureComponent, IDictionary<string, bool> componentSettings)
        {          
            var salinityValues = ((MultiDimensionalArray<double>)salinityComponent.Values).ToList();
            var temperatureValues = ((MultiDimensionalArray<double>)temperatureComponent.Values).ToList();

            bool salinityEnabled;
            if (!componentSettings.TryGetValue(SourceAndSink.SalinityVariableName, out salinityEnabled))
                salinityEnabled = true; // Assume enabled by default, only disabled if explicitly set to false

            bool temperatureEnabled;
            if (!componentSettings.TryGetValue(SourceAndSink.TemperatureVariableName, out temperatureEnabled))
                temperatureEnabled = true; // Assume enabled by default, only disabled if explicitly set to false

            if (salinityValues.Any() && temperatureValues.Any()) // if both Salinity and Temperature components have values
            {
                if (!salinityEnabled) salinityComponent.Values.Clear();
                if (!temperatureEnabled) temperatureComponent.Values.Clear();
            }
            else // either one of the components have values or none of them
            {
                if (salinityEnabled && temperatureValues.Any()) // so Salinity values were imported into the Temperature Component
                {
                    salinityComponent.Values.Clear();
                    salinityComponent.Values.AddRange(temperatureValues);
                    temperatureComponent.Values.Clear();
                }
                else if (temperatureEnabled && salinityValues.Any()) // so Temperature values were imported into the Salinity Component
                {
                    temperatureComponent.Values.Clear();
                    temperatureComponent.Values.AddRange(salinityValues);
                    salinityComponent.Values.Clear();
                }
            }
        }

        private static void ValidateComponentValues<T>(IVariable componentVariable, int numExpectedValues)
        {
            var componentValues = ((MultiDimensionalArray<T>)componentVariable.Values).ToList();
            var numExistingValues = componentValues.Count;
            if (numExistingValues == numExpectedValues) return;

            if (numExistingValues > numExpectedValues)
            {
                componentValues = componentValues.GetRange(0, numExpectedValues);
            }
            else if(numExistingValues < numExpectedValues)
            {
                componentValues.AddRange(Enumerable.Repeat((T)componentVariable.DefaultValue, numExpectedValues - numExistingValues));
            }

            componentVariable.Values.Clear();
            componentVariable.Values.AddRange(componentValues);
        }
        
    }
}
