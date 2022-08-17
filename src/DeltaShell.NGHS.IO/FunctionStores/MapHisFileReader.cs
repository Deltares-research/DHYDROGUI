using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using DeltaShell.NGHS.Utils.Extensions;

namespace DeltaShell.NGHS.IO.FunctionStores
{
    public static class MapHisFileReader
    {
        private const int maximumParameterNameLength = 20;
        private const int maximumLocationNameLength = 20;

        /// <summary>
        /// Reads the meta data of the provided <param name="filePath"/> (could be null)
        /// </summary>
        /// <param name="filePath">Path to the output map or his file</param>
        /// <exception cref="IOException">If reading from <paramref name="filePath"/> fails</exception>
        public static MapHisFileMetaData ReadMetaData(string filePath)
        {
            var isMapFile = filePath != null && Path.GetExtension(filePath).ToLower() == ".map";
            return DoWithMapFileBinaryReader(filePath, r => ReadMapHisFileMetaData(r, isMapFile));
        }

        /// <summary>
        /// Gets the segmentValues for <param name="parameterName"/> and <param name="timeStepIndex"/> (could be null)
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="mapFileMeta">Metadata for the map file (use <see cref="ReadMetaData"/> to get it initially)</param>
        /// <param name="timeStepIndex">Time step index (zero based)</param>
        /// <param name="parameterName">Substances name</param>
        /// <param name="locationIndex">Filter on this segment index (default -1 - no filtering)</param>
        /// <returns>Values for the chosen <param name="timeStepIndex"/> and <param name="parameterName"/> </returns>
        /// <exception cref="IOException">If reading from <paramref name="filePath"/> fails</exception>
        public static List<double> GetTimeStepData(string filePath, MapHisFileMetaData mapFileMeta, int timeStepIndex, string parameterName, int locationIndex = -1)
        {
            return DoWithMapFileBinaryReader(filePath, binaryReader => ReadTimeStepData(binaryReader, mapFileMeta, timeStepIndex, parameterName, locationIndex));
        }

        /// <summary>
        /// Gets the data for all the time steps (<paramref name="mapFileMeta"/> <see cref="MapHisFileMetaData.Times"/>) at a
        /// single location for the provided <paramref name="parameterName"/> (could be null)
        /// </summary>
        /// <param name="filePath">Path to the map/his file</param>
        /// <param name="mapFileMeta">Meta data for the file</param>
        /// <param name="parameterName">Name of the parameter to get data for</param>
        /// <param name="locationIndex">Index of the location (in <see cref="MapHisFileMetaData.Locations"/>)</param>
        /// <returns>Values for provided parameter at provided location</returns>
        /// <exception cref="IOException">If reading from <paramref name="filePath"/> fails</exception>
        public static List<double> GetTimeSeriesData(string filePath, MapHisFileMetaData mapFileMeta, string parameterName, int locationIndex)
        {
            return DoWithMapFileBinaryReader(filePath, binaryReader => ReadTimeSeriesData(binaryReader, mapFileMeta, parameterName, locationIndex));
        }

        private static List<double> ReadTimeSeriesData(BinaryReader reader, MapHisFileMetaData mapHisFileMetaData, string parameterName, int locationIndex)
        {
            string parameterToSearch = GetTruncatedName(parameterName, maximumParameterNameLength);
            if (!mapHisFileMetaData.Parameters.Contains(parameterToSearch))
                return null;

            var data = new List<double>();

            var substanceIndex = mapHisFileMetaData.Parameters.IndexOf(parameterToSearch);

            var timeStepDataBlockSize = GetTimeStepDataBlockSize(mapHisFileMetaData);
            var substanceByteOffset = substanceIndex * 4;
            var segmentDataByteSize = mapHisFileMetaData.NumberOfParameters * 4;

            // Skip to the right position
            var startPosition = mapHisFileMetaData.DataBlockOffsetInBytes + 4;
            reader.BaseStream.Position = startPosition + substanceByteOffset + segmentDataByteSize * locationIndex;

            for (int i = 0; i < mapHisFileMetaData.NumberOfTimeSteps; i++)
            {
                data.Add(reader.ReadSingle());
                reader.BaseStream.Position += timeStepDataBlockSize - 4;
            }

            return data;
        }

        private static List<double> ReadTimeStepData(BinaryReader reader, MapHisFileMetaData mapHisFileMetaData, int timeStepIndex, string parameterName, int locationIndex)
        {
            string parameterToSearch = GetTruncatedName(parameterName, maximumParameterNameLength);
            if (!mapHisFileMetaData.Parameters.Contains(parameterToSearch))
                return null;

            var data = new List<double>();

            var substanceIndex = mapHisFileMetaData.Parameters.IndexOf(parameterToSearch);

            // size of 1 time step and all substances
            var timeStepDataBlockSize = GetTimeStepDataBlockSize(mapHisFileMetaData);
            var timeStepOffset = timeStepDataBlockSize * timeStepIndex;

            var substanceByteOffset = substanceIndex * 4;
            var segmentDataByteSize = mapHisFileMetaData.NumberOfParameters * 4;

            // Skip to the right position
            var startPosition = mapHisFileMetaData.DataBlockOffsetInBytes + timeStepOffset + 4;

            reader.BaseStream.Position = startPosition;

            if (locationIndex != -1)
            {
                reader.BaseStream.Position += substanceByteOffset + segmentDataByteSize * locationIndex;
                data.Add(reader.ReadSingle());
            }
            else
            {
                for (var i = 0; i < mapHisFileMetaData.NumberOfLocations; i++)
                {
                    reader.BaseStream.Position += i == 0 ? substanceByteOffset : segmentDataByteSize - 4;
                    data.Add(reader.ReadSingle());
                }
            }

            return data;
        }

        /// <summary>
        /// Truncates the <paramref name="name"/> to the maximum allowed length (<paramref name="maximumLength"/>)
        /// </summary>
        /// <param name="name">Original parameter name (name to truncate)</param>
        /// <param name="maximumLength">Maximum length to truncate to</param>
        /// <returns>Truncated parameter name</returns>
        private static string GetTruncatedName(string name, int maximumLength)
        {
            return name.Substring(0, Math.Min(name.Length, maximumLength));
        }

        private static MapHisFileMetaData ReadMapHisFileMetaData(BinaryReader reader, bool mapFile = false)
        {
            reader.BaseStream.Position = 0;

            var mapHisFileMetaData = new MapHisFileMetaData();

            reader.ReadChars(40 * 3); // Skip the first 3 headers (these contain meta-data that we don't need)

            // Read and parse the t0 reference time string
            var timeString = new string(reader.ReadChars(40));
            var t0 = DateTime.Parse(timeString.Substring(4, 19), CultureInfo.InvariantCulture);

            var timeStepUnitValue = int.Parse(timeString.Substring(30, 8));
            var timeStepUnit = timeString[38];

            // Read the number of parameters and locations
            mapHisFileMetaData.NumberOfParameters = reader.ReadInt32();
            mapHisFileMetaData.NumberOfLocations = reader.ReadInt32();

            // Read all parameter names
            for (var i = 0; i < mapHisFileMetaData.NumberOfParameters; i++)
            {
                mapHisFileMetaData.Parameters.Add(new string(reader.ReadChars(maximumParameterNameLength)).Trim(' '));
            }

            if (!mapFile)
            {
                mapHisFileMetaData.Locations = new List<string>();
                for (int i = 0; i < mapHisFileMetaData.NumberOfLocations; i++)
                {
                    reader.ReadInt32(); // loc number: not needed
                    mapHisFileMetaData.Locations.Add(new string(reader.ReadChars(maximumLocationNameLength)).Trim());
                }
                if (!mapHisFileMetaData.Locations.AllUnique())
                {
                    throw new FileLoadException("His file does not contain unique location names");
                }
            }
            else
            {
                mapHisFileMetaData.Locations = Enumerable.Range(0, mapHisFileMetaData.NumberOfLocations).Select(n => n.ToString()).ToList();
            }

            mapHisFileMetaData.DataBlockOffsetInBytes = reader.BaseStream.Position;

            var bytesLeft = reader.BaseStream.Length - reader.BaseStream.Position;
            var timeStepDataBlockSize = GetTimeStepDataBlockSize(mapHisFileMetaData);

            mapHisFileMetaData.NumberOfTimeSteps = Convert.ToInt32(bytesLeft / timeStepDataBlockSize);

            // read times (convert from seconds relative to T0 to DateTimes)
            for (int i = 0; i < mapHisFileMetaData.NumberOfTimeSteps; i++)
            {
                var timeStepValue = reader.ReadInt32();
                mapHisFileMetaData.Times.Add(t0 + GetTimeStepSpan(timeStepValue,timeStepUnitValue, timeStepUnit));
                reader.BaseStream.Position += timeStepDataBlockSize - 4;
            }

            return mapHisFileMetaData;
        }

        private static TimeSpan GetTimeStepSpan(int timeStepValue, int timeStepUnitValue, char timeStepUnit)
        {
            switch (timeStepUnit)
            {
                case 's':
                    return new TimeSpan(0, 0, timeStepUnitValue * timeStepValue);
                case 'm':
                    return new TimeSpan(0, timeStepUnitValue * timeStepValue, 0);
                case 'h':
                    return new TimeSpan(timeStepUnitValue * timeStepValue, 0, 0);
                case 'd':
                    return new TimeSpan(timeStepUnitValue * timeStepValue, 0, 0, 0);
                default:
                    throw new ArgumentException(timeStepUnit + " is not supported as time unit argument");
            }
        }

        private static int GetTimeStepDataBlockSize(MapHisFileMetaData mapFileMetaData)
        {
            // time step size + (number of segments * number of substances * float size)
            return 4 + ( mapFileMetaData.NumberOfParameters * mapFileMetaData.NumberOfLocations * 4 );
        }

        private static T DoWithMapFileBinaryReader<T>(string filePath, Func<BinaryReader, T> readerFunction) where T : class
        {
            if (IsInvalidFile(filePath))
            {
                return null;
            }

            BinaryReader reader = null;

            try
            {
                reader = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                return readerFunction(reader);
            }
            catch (Exception ex) when (ex is IOException || 
                                       ex is UnauthorizedAccessException ||
                                       ex is SecurityException ||
                                       ex is NotSupportedException ||
                                       ex is ArgumentOutOfRangeException)
            {
                throw new IOException($"Could not read file {filePath}", ex);
            }
            finally
            {
                reader?.Close();
            }
        }

        private static bool IsInvalidFile(string filePath)
        {
            // Check whether the output file exits or not or is Empty
            return !File.Exists(filePath) || new FileInfo(filePath).Length == 0;
        }
    }
}
