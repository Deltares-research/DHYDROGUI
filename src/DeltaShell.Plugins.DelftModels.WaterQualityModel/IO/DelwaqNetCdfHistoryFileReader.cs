using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.NetCdf;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// A map file reader designed for NetCdf files (_his.nc) created by D-Water Quality.
    /// </summary>
    public static class DelwaqNetCdfHistoryFileReader
    {
        private const string timeVariableName = "nhistory_dlwq_time";
        private const string nStationsDimensionName = "nStations";
        private const string fillValueAttributeName = "_FillValue";
        private const string stationNameVariableName = "station_name";
        private const string nameLengthDimension = "name_len";

        /// <summary>
        /// Reads the specified path and creates a <see cref="DelwaqHisFileData" /> for each monitoring point.
        /// </summary>
        /// <param name="path"> The file path. </param>
        /// <returns> </returns>
        public static List<DelwaqHisFileData> Read(string path)
        {
            List<DelwaqHisFileData> data;
            NetCdfFile netCdfFile = null;
            try
            {
                netCdfFile = NetCdfFile.OpenExisting(path);
                data = Read(netCdfFile).ToList();
            }
            finally
            {
                netCdfFile?.Close();
            }

            return data;
        }

        private static IEnumerable<DelwaqHisFileData> Read(NetCdfFile file)
        {
            NetCdfVariable[] outputVariables = GetOutputVariables(file).ToArray();

            var origins = new int[2];
            var shapes = new int[2];

            IEnumerable<DateTime> timeSteps = NcFileReaderHelper.GetDateTimes(file, timeVariableName).ToList();
            string[] outputVariableNames = outputVariables.Select(file.GetVariableName).ToArray();
            string[] locationNames = GetLocationNames(file).ToArray();

            for (var locationIndex = 0; locationIndex < locationNames.Length; locationIndex++)
            {
                string observationPointName = locationNames[locationIndex];
                var data = new DelwaqHisFileData(observationPointName) {OutputVariables = outputVariableNames};

                origins[1] = locationIndex;
                shapes[1] = 1;

                for (var timeIndex = 0; timeIndex < timeSteps.Count(); timeIndex++)
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
            float value = file.Read(outputVariable, origins, shapes)
                              .OfType<float>()
                              .First();
            return value;
        }

        private static IEnumerable<NetCdfVariable> GetOutputVariables(NetCdfFile file)
        {
            return file.GetVariables()
                       .Where(v => file.GetAttributes(v).ContainsKey(fillValueAttributeName));
        }

        private static IEnumerable<string> GetLocationNames(NetCdfFile file)
        {
            int nLocations = file.GetDimensionLength(nStationsDimensionName);
            int locationNameLength = file.GetDimensionLength(nameLengthDimension);

            var origins = new int[2];
            var shapes = new int[2];
            origins[1] = 0;
            shapes[0] = 1;
            shapes[1] = locationNameLength;

            for (var i = 0; i < nLocations; i++)
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