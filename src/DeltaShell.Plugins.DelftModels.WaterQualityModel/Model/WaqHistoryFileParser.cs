using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    /// <summary>
    /// Parser for history files for Water Quality models.
    /// </summary>
    public static class WaqHistoryFileParser
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaqHistoryFileParser));

        /// <summary>
        /// Parses his file data and adds it to WaterQualityModel.
        /// </summary>
        /// <param name="filePath"> The history file path to parse the data from. </param>
        /// <param name="monitoringOutputLevel"> The monitoring output level. </param>
        /// <param name="observationVariableOutputs"> The observationVariableOutputs that are declared. </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="filePath" /> is null. </exception>
        /// <remarks> An error message is provided if errors occur while reading data from <paramref name="filePath"/>.</remarks>
        /// <remarks>
        /// The his data from <paramref name="filePath" /> is not parsed if:
        /// * The
        /// <param name="monitoringOutputLevel" />
        /// equals "None"
        /// * The list
        /// <param name="observationVariableOutputs" />
        /// is empty
        /// </remarks>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="filePath"/> is <c>null</c>.</exception>
        public static void Parse(string filePath,
                                 IList<WaterQualityObservationVariableOutput> observationVariableOutputs,
                                 MonitoringOutputLevel monitoringOutputLevel)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (monitoringOutputLevel == MonitoringOutputLevel.None || observationVariableOutputs == null ||
                !observationVariableOutputs.Any())
            {
                return; // No HIS file data will be present
            }

            DelwaqHisFileData[] hisFileVariableDataList;

            string fileExtension = Path.GetExtension(filePath);
            switch (fileExtension)
            {
                case ".nc":
                    hisFileVariableDataList = DelwaqNetCdfHistoryFileReader.Read(filePath);
                    break;
                case ".his":
                    hisFileVariableDataList = DelwaqHistoryFileReader.Read(filePath);
                    break;
                default:
                    Log.ErrorFormat(Resources.WaqProcessorHelper_ParseHisFileData_Invalid_file_format, filePath);
                    return;
            }

            if (!hisFileVariableDataList.Any())
            {
                Log.ErrorFormat(Resources.WaqProcessorHelper_ParseHisFileData_An_error_occurred_while_reading_file, filePath);
                return;
            }

            foreach (WaterQualityObservationVariableOutput observationVariableOutput in observationVariableOutputs)
            {
                SetDataOnObservationVariableOutput(observationVariableOutput, hisFileVariableDataList);
            }
        }

        private static void SetDataOnObservationVariableOutput(WaterQualityObservationVariableOutput observationVariableOutput,
                                                               DelwaqHisFileData[] hisFileVariableDataList)
        {
            int outputVariableCount = observationVariableOutput.TimeSeriesList.Count();

            DelwaqHisFileData hisFileVariableData = hisFileVariableDataList
                .FirstOrDefault(data => data.ObservationVariable == observationVariableOutput.Name
                                        && data.OutputVariables.Length == outputVariableCount);

            if (hisFileVariableData == null)
            {
                return;
            }

            IEnumerable<DateTime> timeSteps = hisFileVariableData.TimeSteps.ToArray();
            List<List<double>> allValues = observationVariableOutput.TimeSeriesList
                                                                    .Select(ov => new List<double>())
                                                                    .ToList();

            // Parse all values on per output variable basis (TODO: Improve performance by parsing the values for the relevant output variables only)
            foreach (DateTime timeStep in timeSteps)
            {
                List<double> timeStepValues = hisFileVariableData.GetValuesForTimeStep(timeStep);

                for (var j = 0; j < outputVariableCount; j++)
                {
                    allValues[j].Add(timeStepValues[j]);
                }
            }

            for (var i = 0; i < outputVariableCount; i++)
            {
                TimeSeries timeSeries = observationVariableOutput.TimeSeriesList.ElementAt(i);

                // Add all output times to the output variable time series
                timeSeries.Time.AddValues(timeSteps);

                // Add the parsed values to the to the output variable time series
                observationVariableOutput.TimeSeriesList.ElementAt(i).SetValues(allValues[i]);
            }
        }
    }
}