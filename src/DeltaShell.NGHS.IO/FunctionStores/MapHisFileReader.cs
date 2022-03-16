using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DeltaShell.NGHS.IO.FunctionStores
{
    public static class MapHisFileReader
    {
        /// <summary>
        /// Reads the meta data of the provided
        /// <param name="filePath"/>
        /// </summary>
        /// <param name="filePath">Path to the output map or his file</param>
        public static MapHisFileMetaData ReadMetaData(string filePath)
        {
            bool isMapFile = Path.GetExtension(filePath).ToLower() == ".map";
            return DoWithMapFileBinaryReader(filePath, r => ReadMapHisFileMetaData(r, isMapFile));
        }

        /// <summary>
        /// Gets the segmentValues for
        /// <param name="parameterName"/>
        /// and
        /// <param name="timeStepIndex"/>
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="mapFileMeta">Metadata for the map file (use <see cref="ReadMetaData"/> to get it initially)</param>
        /// <param name="timeStepIndex">Timestep index (zero based)</param>
        /// <param name="parameterName">Substances name</param>
        /// <param name="locationIndex">Filter on this segment index (default -1 - no filtering)</param>
        /// <returns>
        /// Values for the chosen
        /// <param name="timeStepIndex"/>
        /// and
        /// <param name="parameterName"/>
        /// </returns>
        public static List<double> GetTimeStepData(string filePath, MapHisFileMetaData mapFileMeta, int timeStepIndex, string parameterName, int locationIndex = -1)
        {
            return DoWithMapFileBinaryReader(filePath, binaryReader => ReadTimeStepData(binaryReader, mapFileMeta, timeStepIndex, parameterName, locationIndex));
        }

        public static List<double> GetTimeSeriesData(string filePath, MapHisFileMetaData mapFileMeta, string parameterName, int locationIndex)
        {
            return DoWithMapFileBinaryReader(filePath, binaryReader => ReadTimeSeriesData(binaryReader, mapFileMeta, parameterName, locationIndex));
        }

        private static List<double> ReadTimeSeriesData(BinaryReader reader, MapHisFileMetaData mapHisFileMetaData, string parameterName, int locationIndex)
        {
            if (!mapHisFileMetaData.Parameters.Contains(parameterName))
            {
                return new List<double>();
            }

            var data = new List<double>();

            int substanceIndex = mapHisFileMetaData.Parameters.IndexOf(parameterName);

            int timeStepDataBlockSize = GetTimeStepDataBlockSize(mapHisFileMetaData);
            int substanceByteOffset = substanceIndex * 4;
            int segmentDataByteSize = mapHisFileMetaData.NumberOfParameters * 4;

            // Skip to the right position
            long startPosition = mapHisFileMetaData.DataBlockOffsetInBytes + 4;
            reader.BaseStream.Position = startPosition + substanceByteOffset + (segmentDataByteSize * locationIndex);

            for (var i = 0; i < mapHisFileMetaData.NumberOfTimeSteps; i++)
            {
                data.Add(reader.ReadSingle());
                reader.BaseStream.Position += timeStepDataBlockSize - 4;
            }

            return data;
        }

        private static List<double> ReadTimeStepData(BinaryReader reader, MapHisFileMetaData mapHisFileMetaData, int timeStepIndex, string parameterName, int locationIndex)
        {
            if (!mapHisFileMetaData.Parameters.Contains(parameterName))
            {
                return new List<double>();
            }

            var data = new List<double>();

            int substanceIndex = mapHisFileMetaData.Parameters.IndexOf(parameterName);

            // size of 1 timestep and all substances
            int timeStepDataBlockSize = GetTimeStepDataBlockSize(mapHisFileMetaData);
            int timeStepOffset = timeStepDataBlockSize * timeStepIndex;

            int substanceByteOffset = substanceIndex * 4;
            int segmentDataByteSize = mapHisFileMetaData.NumberOfParameters * 4;

            // Skip to the right position
            long startPosition = mapHisFileMetaData.DataBlockOffsetInBytes + timeStepOffset + 4;

            reader.BaseStream.Position = startPosition;

            if (locationIndex != -1)
            {
                reader.BaseStream.Position += substanceByteOffset + (segmentDataByteSize * locationIndex);
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

        private static MapHisFileMetaData ReadMapHisFileMetaData(BinaryReader reader, bool mapFile = false)
        {
            reader.BaseStream.Position = 0;

            var mapHisFileMetaData = new MapHisFileMetaData();

            reader.ReadChars(40 * 3); // Skip the first 3 headers (these contain meta-data that we don't need)

            // Read and parse the t0 reference time string
            var timeString = new string(reader.ReadChars(40));
            DateTime t0 = DateTime.Parse(timeString.Substring(4, 19), CultureInfo.InvariantCulture);

            int timeStepUnitValue = int.Parse(timeString.Substring(30, 8));
            char timeStepUnit = timeString[38];

            // Read the number of parameters and locations
            mapHisFileMetaData.NumberOfParameters = reader.ReadInt32();
            mapHisFileMetaData.NumberOfLocations = reader.ReadInt32();

            // Read all parameter names
            for (var i = 0; i < mapHisFileMetaData.NumberOfParameters; i++)
            {
                mapHisFileMetaData.Parameters.Add(new string(reader.ReadChars(20)).Trim(' '));
            }

            if (!mapFile)
            {
                mapHisFileMetaData.Locations = new List<string>();
                for (var i = 0; i < mapHisFileMetaData.NumberOfLocations; i++)
                {
                    reader.ReadInt32(); // loc nummer: not needed
                    mapHisFileMetaData.Locations.Add(new string(reader.ReadChars(20)).Trim());
                }

                if (mapHisFileMetaData.Locations.Distinct().Count() != mapHisFileMetaData.Locations.Count)
                {
                    throw new FileLoadException("His file does not contain unique location names");
                }
            }
            else
            {
                mapHisFileMetaData.Locations = Enumerable.Range(0, mapHisFileMetaData.NumberOfLocations).Select(n => n.ToString()).ToList();
            }

            mapHisFileMetaData.DataBlockOffsetInBytes = reader.BaseStream.Position;

            long bytesLeft = reader.BaseStream.Length - reader.BaseStream.Position;
            int timeStepDataBlockSize = GetTimeStepDataBlockSize(mapHisFileMetaData);

            mapHisFileMetaData.NumberOfTimeSteps = Convert.ToInt32(bytesLeft / timeStepDataBlockSize);

            // read times (convert from seconds relative to T0 to DateTimes)
            for (var i = 0; i < mapHisFileMetaData.NumberOfTimeSteps; i++)
            {
                int timeStepValue = reader.ReadInt32();
                mapHisFileMetaData.Times.Add(t0 + GetTimeStepSpan(timeStepValue, timeStepUnitValue, timeStepUnit));
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
            // timestep size + (number of segments * number of substances * float size)
            return 4 + (mapFileMetaData.NumberOfParameters * mapFileMetaData.NumberOfLocations * 4);
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
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        private static bool IsInvalidFile(string filePath)
        {
            // Check whether the output file exits or not or is Empty
            return !File.Exists(filePath) || new FileInfo(filePath).Length == 0;
        }
    }
}