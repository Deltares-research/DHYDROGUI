using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.NetCdf;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Reader for reading WAQ history file data from NetCDF files."
    /// </summary>
    public static class DelwaqNcHisFileReader
    {
        private const string timeVariableName = "nhistory_dlwq_time";
        private const string numberOfStationsDimensionName = "nStations";
        private const string fillValueAttributeName = "_FillValue";
        private const string stationNameVariableName = "station_name";
        private const string nameLengthDimensionName = "name_len";

        private static readonly ILog log = LogManager.GetLogger(typeof(DelwaqNcHisFileReader));

        /// <summary>
        /// Reads <see cref="DelwaqHisFileData" /> objects from a file at the specified <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"> The file path. </param>
        /// <returns>The read <see cref="DelwaqHisFileData" /> objects.</returns>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="filePath"/> is <c>null</c> or empty.</exception>
        public static List<DelwaqHisFileData> Read(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            NetCdfFile netCdfFile = null;
            try
            {
                netCdfFile = NetCdfFile.OpenExisting(filePath);
                return Read(netCdfFile).ToList();
            }
            catch (FileNotFoundException)
            {
                log.Error($"File was not found: {filePath}.");
                return new List<DelwaqHisFileData>();
            }
            finally
            {
                netCdfFile?.Close();
            }
        }

        private static IEnumerable<DelwaqHisFileData> Read(NetCdfFile file)
        {
            NetCdfVariable[] outputVariables = GetOutputVariables(file).ToArray();

            var origins = new int[2];
            var shapes = new int[2];

            DateTime[] timeSteps = NcFileReaderHelper.GetDateTimes(file, timeVariableName).ToArray();
            string[] outputVariableNames = outputVariables.Select(file.GetVariableName).ToArray();
            string[] locationNames = GetLocationNames(file).ToArray();
            int timeStepCount = timeSteps.Length;

            for (var locationIndex = 0; locationIndex < locationNames.Length; locationIndex++)
            {
                string observationPointName = locationNames[locationIndex];
                var data = new DelwaqHisFileData(observationPointName) {OutputVariables = outputVariableNames};

                origins[1] = locationIndex;
                shapes[1] = 1;

                for (var timeIndex = 0; timeIndex < timeStepCount; timeIndex++)
                {
                    DateTime timeStep = timeSteps.ElementAt(timeIndex);

                    origins[0] = timeIndex;
                    shapes[0] = 1;

                    foreach (NetCdfVariable outputVariable in outputVariables)
                    {
                        double value = GetValueAtIndices(file, outputVariable, origins, shapes);
                        data.AddValueForTimeStep(timeStep, value);
                    }
                }

                yield return data;
            }
        }

        private static double GetValueAtIndices(NetCdfFile file, NetCdfVariable outputVariable, int[] origins,
                                                int[] shapes)
        {
            return file.Read(outputVariable, origins, shapes)
                       .OfType<float>()
                       .First();
        }

        private static IEnumerable<NetCdfVariable> GetOutputVariables(NetCdfFile file)
        {
            return file.GetVariables()
                       .Where(v => file.GetAttributes(v).ContainsKey(fillValueAttributeName));
        }

        private static IEnumerable<string> GetLocationNames(NetCdfFile file)
        {
            int numberOfStations = file.GetDimensionLength(numberOfStationsDimensionName);
            int locationNameLength = file.GetDimensionLength(nameLengthDimensionName);

            var origins = new int[2];
            var shapes = new int[2];
            origins[1] = 0;
            shapes[0] = 1;
            shapes[1] = locationNameLength;

            for (var i = 0; i < numberOfStations; i++)
            {
                origins[0] = i;

                yield return GetLocationName(file, origins, shapes);
            }
        }

        private static string GetLocationName(NetCdfFile file, int[] origins, int[] shapes)
        {
            IEnumerable<char> nameChars = file.Read(file.GetVariableByName(stationNameVariableName), origins, shapes)
                                              .OfType<IEnumerable<char>>().First();
            return string.Concat(nameChars).Trim();
        }
    }
}