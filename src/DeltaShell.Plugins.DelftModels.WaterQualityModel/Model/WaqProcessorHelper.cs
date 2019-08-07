using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public static class WaqProcessorHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaqProcessorHelper));

        /// <summary>
        /// Parses his file data and adds it to WaterQualityModel
        /// </summary>
        /// <param name="filePath"> The history file path to parse the data from </param>
        /// <param name="monitoringOutputLevel"> The monitoring output level </param>
        /// <param name="observationPointsToSkipOutputVariablesFor">
        /// The observation point outputs to skip the
        /// <paramref name="outputVariablesToSkip" /> for during parsing (list of names, with for instance "O1" and "O2")
        /// </param>
        /// <param name="outputVariablesToSkip">
        /// The substance output that should be skipped during parsing output for
        /// <paramref name="observationPointsToSkipOutputVariablesFor" />(list of names, with for instance "NH4" and "OXY")
        /// </param>
        /// <param name="observationVariableOutputs"> The observationVariableOutputs that are declared </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="filePath" /> is null </exception>
        /// <remarks> An error message is provided if errors occur while reading data from <paramref name="filePath" /> </remarks>
        /// <remarks>
        /// The his data from <paramref name="filePath" /> is not parsed if:
        /// * The
        /// <param name="monitoringOutputLevel" />
        /// equals "None"
        /// * The list
        /// <param name="observationVariableOutputs" />
        /// is empty
        /// </remarks>
        public static void ParseHisFileData(string filePath,
                                            IList<WaterQualityObservationVariableOutput> observationVariableOutputs,
                                            MonitoringOutputLevel monitoringOutputLevel,
                                            IEnumerable<string> observationPointsToSkipOutputVariablesFor = null,
                                            IEnumerable<string> outputVariablesToSkip = null)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (observationPointsToSkipOutputVariablesFor == null)
            {
                observationPointsToSkipOutputVariablesFor = Enumerable.Empty<string>();
            }

            if (outputVariablesToSkip == null)
            {
                outputVariablesToSkip = Enumerable.Empty<string>();
            }

            if (monitoringOutputLevel == MonitoringOutputLevel.None || observationVariableOutputs == null ||
                !observationVariableOutputs.Any())
            {
                return; // No HIS file data will be present
            }

            var hisFileVariableDataList = new List<DelwaqHisFileData>();

            string fileExtension = Path.GetExtension(filePath);

            if (fileExtension.Equals(".nc"))
            {
                hisFileVariableDataList = DelwaqNcHisFileReader.Read(filePath);
            }
            else if (fileExtension.Equals(".his"))
            {
                hisFileVariableDataList = DelwaqHisFileReader.Read(filePath);
            }

            if (hisFileVariableDataList.Count == 0)
            {
                Log.Error(
                    "An error occurred while reading the his file: check the textual output files for more information");
                return;
            }

            foreach (WaterQualityObservationVariableOutput observationVariableOutput in observationVariableOutputs)
            {
                int outputVariableCount = observationVariableOutput.TimeSeriesList.Count();
                DelwaqHisFileData hisFileVariableData = hisFileVariableDataList.FirstOrDefault(
                    data => data.ObservationVariable == observationVariableOutput.Name
                            && data.OutputVariables.Count() == outputVariableCount);

                if (hisFileVariableData == null)
                {
                    continue;
                }

                IEnumerable<DateTime> outputTimes = hisFileVariableData.TimeSteps;
                List<List<double>> allValues = observationVariableOutput.TimeSeriesList
                                                                        .Select(ov => new List<double>())
                                                                        .ToList();

                // Parse all values on per output variable basis (TODO: Improve performance by parsing the values for the relevant output variables only)
                foreach (DateTime timeStep in outputTimes)
                {
                    List<double> timeStepValues = hisFileVariableData.GetValuesForTimeStep(timeStep);

                    for (var j = 0; j < outputVariableCount; j++)
                    {
                        allValues[j].Add(timeStepValues[j]);
                    }
                }

                bool skipOutputVariables =
                    observationPointsToSkipOutputVariablesFor.Any(name => observationVariableOutput.Name == name);

                for (var i = 0; i < outputVariableCount; i++)
                {
                    TimeSeries timeSeries = observationVariableOutput.TimeSeriesList.ElementAt(i);
                    if (skipOutputVariables && outputVariablesToSkip.Any(name => name == timeSeries.Name))
                    {
                        continue; // Skip the output variables that should be skipped
                    }

                    // Add all output times to the output variable time series
                    timeSeries.Time.AddValues(outputTimes);

                    // Add the parsed values to the to the output variable time series
                    observationVariableOutput.TimeSeriesList.ElementAt(i).SetValues(allValues[i]);
                }
            }
        }
    }
}