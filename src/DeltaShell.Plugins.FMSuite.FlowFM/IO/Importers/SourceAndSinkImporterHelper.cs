using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
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
                Log.ErrorFormat(Resources.Invalid_Variables_detected_in_imported_SourceAndSink_Function___0_, readFunction.Name);
                return false;
            }
           
            if (!TryAdjustSalinityAndTemperatureComponents(salinityComponent, temperatureComponent, componentSettings))
            {
                return false;
            }

            var numTimeSteps = timeVariable.Values.Count;
            ValidateComponentValues<double>(dischargeComponent, numTimeSteps);
            ValidateComponentValues<double>(salinityComponent, numTimeSteps);
            ValidateComponentValues<double>(temperatureComponent, numTimeSteps);

            return true;
        }

        private static bool TryAdjustSalinityAndTemperatureComponents(IVariable salinityComponent, IVariable temperatureComponent, IDictionary<string, bool> componentSettings)
        {          
            bool salinityEnabled;
            if (!componentSettings.TryGetValue(SourceAndSink.SalinityVariableName, out salinityEnabled))
                salinityEnabled = true; // Assume enabled by default, only disabled if explicitly set to false

            bool temperatureEnabled;
            if (!componentSettings.TryGetValue(SourceAndSink.TemperatureVariableName, out temperatureEnabled))
                temperatureEnabled = true; // Assume enabled by default, only disabled if explicitly set to false

            var additionalValuesDetected = false;

            if (temperatureEnabled && !salinityEnabled) // Temperature values will have been imported into the Salinity Component
            {
                additionalValuesDetected |= !VariableContainsDefaultValuesOnly(temperatureComponent);
                temperatureComponent.Values.Clear();

                var salinityValues = ((MultiDimensionalArray<double>)salinityComponent.Values).ToList();
                temperatureComponent.Values.AddRange(salinityValues);
                salinityComponent.Values.Clear();
            }

            if (!salinityEnabled)
            {
                additionalValuesDetected |= !VariableContainsDefaultValuesOnly(salinityComponent);
                salinityComponent.Values.Clear();
            }
            if (!temperatureEnabled)
            {
                additionalValuesDetected |= !VariableContainsDefaultValuesOnly(temperatureComponent);
                temperatureComponent.Values.Clear();
            }

            if (additionalValuesDetected)
            {
                Log.Error(Resources.SourceAndSinkImporterHelper_TryAdjustSalinityAndTemperatureComponents_Additional_values_detected_for_one_or_more_physical_processes);
            }

            return !additionalValuesDetected;
        }

        private static bool VariableContainsDefaultValuesOnly(IVariable variable)
        {
            if (variable == null) return false;

            return ((MultiDimensionalArray<double>)variable.Values)
                .All(v => Math.Abs(v - (double)variable.DefaultValue) < double.Epsilon);
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
