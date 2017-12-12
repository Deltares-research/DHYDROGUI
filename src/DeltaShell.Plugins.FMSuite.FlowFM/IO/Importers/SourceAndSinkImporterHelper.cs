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
        /// <param name="salinityEnabled"></param>
        /// <param name="temperatureEnabled"></param>
        /// <returns>bool based on success or failure of operation</returns>
        public static bool DetermineComponentValuesForImportedSourceAndSinkFunction(IFunction readFunction, bool salinityEnabled, bool temperatureEnabled)
        {
            if (readFunction.Arguments == null ||
                readFunction.Arguments.Count == 0 ||
                readFunction.Arguments[0].Values == null ||
                readFunction.Components == null)
            {
                Log.WarnFormat("Could not determine component values for SourceAndSink, imported function is not valid");
                return false;
            }

            var numTimeSteps = readFunction.Arguments[0].Values.Count;
            var dischargeComponent = readFunction.Components.FirstOrDefault(c => c.Name == SourceAndSink.DischargeVariableName);
            var salinityComponent = readFunction.Components.FirstOrDefault(c => c.Name == SourceAndSink.SalinityVariableName);
            var temperatureComponent = readFunction.Components.FirstOrDefault(c => c.Name == SourceAndSink.TemperatureVariableName);

            if (dischargeComponent == null || salinityComponent == null || temperatureComponent == null)
            {
                Log.ErrorFormat("Could not determine component values for SourceAndSink, imported function does not contain expected components");
                return false;
            }

            // initialise Values properties of components if not already initialised
            if (dischargeComponent.Values == null) dischargeComponent.Values = new MultiDimensionalArray<double>();
            if (salinityComponent.Values == null) salinityComponent.Values = new MultiDimensionalArray<double>();
            if (temperatureComponent.Values == null) temperatureComponent.Values = new MultiDimensionalArray<double>();

            // add missing discharge values (default)
            for (var i = dischargeComponent.Values.Count; i < numTimeSteps; i++)
            {
                dischargeComponent.Values.Add(dischargeComponent.DefaultValue);
            }

            var salinityValues = ((MultiDimensionalArray<double>)salinityComponent.Values).ToList();
            var temperatureValues = ((MultiDimensionalArray<double>)temperatureComponent.Values).ToList();

            if (!(salinityValues.Any() && temperatureValues.Any()))
            {
                // if it is not the case that both Salinity and Temperature components have values^
                if (salinityEnabled && temperatureValues.Any())
                {
                    // if salinity values were incorrectly imported into temperature component
                    salinityComponent.Values.Clear();
                    salinityComponent.Values.AddRange(temperatureValues);
                    temperatureComponent.Values.Clear();
                }
                else if (temperatureEnabled && salinityValues.Any())
                {
                    // if temperature values were incorrecly imported into salinity component
                    temperatureComponent.Values.Clear();
                    temperatureComponent.Values.AddRange(salinityValues);
                    salinityComponent.Values.Clear();
                }
            }

            // clear any imported values from function if we do not want them
            if (!salinityEnabled) salinityComponent.Values.Clear();
            if (!temperatureEnabled) temperatureComponent.Values.Clear();

            // add missing salinity values (default)
            for (var i = salinityComponent.Values.Count; i < numTimeSteps; i++)
            {
                salinityComponent.Values.Add(salinityComponent.DefaultValue);
            }

            // add missing temperature values (default)
            for (var i = temperatureComponent.Values.Count; i < numTimeSteps; i++)
            {
                temperatureComponent.Values.Add(temperatureComponent.DefaultValue);
            }

            return true;
        }
    }
}
