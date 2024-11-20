using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using Deltares.Infrastructure.API.Guards;
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
        private static readonly ILog log = LogManager.GetLogger(typeof(WaqHistoryFileParser));

        /// <summary>
        /// Parses his file data and sets the data on the <paramref name="observationVariableOutputs"/>.
        /// </summary>
        /// <param name="filePath"> The history file path to parse the data from. </param>
        /// <param name="monitoringOutputLevel"> The monitoring output level. </param>
        /// <param name="observationVariableOutputs"> The observationVariableOutputs that are declared. </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="filePath"/> is null. </exception>
        /// <remarks> An error message is provided if errors occur while reading data from <paramref name="filePath"/>.</remarks>
        /// <remarks>
        /// The his data from <paramref name="filePath"/> is not parsed if:
        /// * The <paramref name="monitoringOutputLevel"/> equals <c>None</c>
        /// * The list <paramref name="observationVariableOutputs"/> is empty
        /// </remarks>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="filePath"/> is <c>null</c>.</exception>
        public static void Parse(string filePath,
                                 IList<WaterQualityObservationVariableOutput> observationVariableOutputs,
                                 MonitoringOutputLevel monitoringOutputLevel)
        {
            Ensure.NotNullOrEmpty(filePath, nameof(filePath),
                                  $"Argument '{nameof(filePath)}' cannot be null or empty.");

            if (monitoringOutputLevel == MonitoringOutputLevel.None || 
                observationVariableOutputs == null ||
                !observationVariableOutputs.Any())
            {
                return; // No HIS file data will be present
            }

            DelwaqHisFileData[] hisFileData = ReadHisFileData(filePath);
            if (!hisFileData.Any())
            {
                log.ErrorFormat(Resources.WaqProcessorHelper_ParseHisFileData_An_error_occurred_while_reading_file, filePath);
                return;
            }

            foreach (WaterQualityObservationVariableOutput observationVariableOutput in observationVariableOutputs)
            {
                SetDataOnObservationVariableOutput(observationVariableOutput, hisFileData);
            }
        }

        private static DelwaqHisFileData[] ReadHisFileData(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath);
            switch (fileExtension)
            {
                case ".nc":
                    return DelwaqNetCdfHistoryFileReader.Read(filePath);
                case ".his":
                    return DelwaqHistoryFileReader.Read(filePath);
                default:
                    log.ErrorFormat(Resources.WaqProcessorHelper_ParseHisFileData_Invalid_file_format, filePath);
                    return new DelwaqHisFileData[0];
            }
        }

        private static void SetDataOnObservationVariableOutput(WaterQualityObservationVariableOutput observationVariableOutput,
                                                               DelwaqHisFileData[] hisFileVariableDataList)
        {
            if (!observationVariableOutput.TimeSeriesList.Any())
            {
                return;
            }

            DelwaqHisFileData hisFileVariableData = 
                hisFileVariableDataList.FirstOrDefault(data => string.Equals(data.ObservationVariable, 
                                                                             observationVariableOutput.Name, 
                                                                             StringComparison.OrdinalIgnoreCase));

            if (hisFileVariableData == null)
            {
                return;
            }

            foreach (TimeSeries timeSeries in observationVariableOutput.TimeSeriesList)
            {
                double[] variableTimeSeriesValues = 
                    GetTimeSeriesValuesFromHisFileData(timeSeries.Name,
                                                       hisFileVariableData);

                if (!HasConsistentTimeStepsForData(hisFileVariableData, 
                                                   variableTimeSeriesValues))
                {
                    log.ErrorFormat(Resources.WaqHistoryFileParser_SetDataOnObservationVariableOutput_Time_steps_are_inconsistent_for_the_data_related_to_variable__0__, 
                                    timeSeries.Name);
                    continue;
                }

                timeSeries.Time.AddValues(hisFileVariableData.TimeSteps);
                timeSeries.SetValues(variableTimeSeriesValues);
            }
        }

        private static bool HasConsistentTimeStepsForData(DelwaqHisFileData data, 
                                                          IReadOnlyCollection<double> timeSeriesValues) =>
            timeSeriesValues.Any() && data.TimeSteps.Count() == timeSeriesValues.Count;

        private static double[] GetTimeSeriesValuesFromHisFileData(string timeSeriesName,
                                                                   DelwaqHisFileData hisFileData)
        {
            double[] result = hisFileData.GetValuesForKey(timeSeriesName).ToArray();

            if (result.Any())
            {
                return result;
            }

            // If the file was read from an .nc file, then any spaces will have
            // been replaced with underscores. Thus we will also need to verify
            // that no element exists with underscores.
            string adjustedTimeSeriesName = timeSeriesName.Replace(' ', '_');

            if (!string.Equals(adjustedTimeSeriesName,
                               timeSeriesName))
            {
                result = hisFileData.GetValuesForKey(adjustedTimeSeriesName).ToArray();
            }

            return result;
        }
    }
}