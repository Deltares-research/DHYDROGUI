using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public static class DelwaqMapFileReader
    {
        /// <summary>
        /// Reads the meta data of the provided
        /// <param name="delwaqOutputFile"/>
        /// </summary>
        /// <param name="delwaqOutputFile"> Path to the delwaq output map file </param>
        public static MapFileMetaData ReadMetaData(string delwaqOutputFile)
        {
            return DoWithMapFileBinairyReader(delwaqOutputFile, ReadMapFileMetaData);
        }

        /// <summary>
        /// Gets the segmentValues for
        /// <param name="substanceName"/>
        /// and
        /// <param name="timeStepIndex"/>
        /// </summary>
        /// <param name="delwaqOutputFile"> </param>
        /// <param name="mapFileMeta"> Metadata for the map file (use <see cref="ReadMetaData"/> to get it initially) </param>
        /// <param name="timeStepIndex"> Timestep index (zero based) </param>
        /// <param name="substanceName"> Substances name </param>
        /// <param name="segmentIndex"> Filter on this segment index (default -1 - no filtering) </param>
        /// <returns>
        /// Values for the chosen
        /// <param name="timeStepIndex"/>
        /// and
        /// <param name="substanceName"/>
        /// </returns>
        public static List<double> GetTimeStepData(string delwaqOutputFile, MapFileMetaData mapFileMeta,
                                                   int timeStepIndex, string substanceName, int segmentIndex = -1)
        {
            return DoWithMapFileBinairyReader(delwaqOutputFile,
                                              binaryReader => ReadTimeStepData(binaryReader,
                                                                               mapFileMeta,
                                                                               timeStepIndex,
                                                                               substanceName,
                                                                               segmentIndex));
        }

        public static List<double> GetTimeSeriesData(string delwaqOutputFile, MapFileMetaData mapFileMeta,
                                                     string substanceName, int segmentIndex)
        {
            return DoWithMapFileBinairyReader(delwaqOutputFile,
                                              binaryReader => ReadTimeSeriesData(binaryReader,
                                                                                 mapFileMeta,
                                                                                 substanceName,
                                                                                 segmentIndex));
        }

        private static MapFileMetaData ReadMapFileMetaData(BinaryReader delwaqOutputFileReader)
        {
            delwaqOutputFileReader.BaseStream.Position = 0;

            var mapFileMetaData = new MapFileMetaData();

            delwaqOutputFileReader
                .ReadChars(40 * 3); // Skip the first 3 headers (these contain meta-data that we don't need)

            // Read and parse the t0 reference time string
            string timeString = new string(delwaqOutputFileReader.ReadChars(40)).Substring(4, 19);
            DateTime t0 = DateTime.Parse(timeString, CultureInfo.InvariantCulture);

            // Read the number of substances and segments
            mapFileMetaData.NumberOfSubstances = delwaqOutputFileReader.ReadInt32();
            mapFileMetaData.NumberOfSegments = delwaqOutputFileReader.ReadInt32();

            // Read all substance names
            for (var i = 0; i < mapFileMetaData.NumberOfSubstances; i++)
            {
                mapFileMetaData.Substances.Add(new string(delwaqOutputFileReader.ReadChars(20)).Trim(' '));
            }

            mapFileMetaData.DataBlockOffsetInBytes = delwaqOutputFileReader.BaseStream.Position;

            long bytesLeft = delwaqOutputFileReader.BaseStream.Length - delwaqOutputFileReader.BaseStream.Position;
            int timeStepDataBlockSize = GetTimeStepDataBlockSize(mapFileMetaData);

            mapFileMetaData.NumberOfTimeSteps = Convert.ToInt32(bytesLeft / timeStepDataBlockSize);

            // read times (convert from seconds relative to T0 to DateTimes)
            for (var i = 0; i < mapFileMetaData.NumberOfTimeSteps; i++)
            {
                mapFileMetaData.Times.Add(t0.AddSeconds(delwaqOutputFileReader.ReadInt32()));
                delwaqOutputFileReader.BaseStream.Position += timeStepDataBlockSize - 4;
            }

            return mapFileMetaData;
        }

        private static List<double> ReadTimeSeriesData(BinaryReader delwaqOutputFileReader, MapFileMetaData mapFileMeta,
                                                       string substanceName, int segmentIndex)
        {
            if (!mapFileMeta.Substances.Contains(substanceName))
            {
                return null;
            }

            var data = new List<double>();

            int substanceIndex = mapFileMeta.Substances.IndexOf(substanceName);

            int timeStepDataBlockSize = GetTimeStepDataBlockSize(mapFileMeta);
            int substanceByteOffset = substanceIndex * 4;
            int segmentDataByteSize = mapFileMeta.NumberOfSubstances * 4;

            // Skip to the right position
            long startPosition = mapFileMeta.DataBlockOffsetInBytes + 4;
            delwaqOutputFileReader.BaseStream.Position =
                startPosition + substanceByteOffset + (segmentDataByteSize * segmentIndex);

            for (var i = 0; i < mapFileMeta.NumberOfTimeSteps; i++)
            {
                data.Add(delwaqOutputFileReader.ReadSingle());
                delwaqOutputFileReader.BaseStream.Position += timeStepDataBlockSize - 4;
            }

            return data;
        }

        private static List<double> ReadTimeStepData(BinaryReader delwaqOutputFileReader, MapFileMetaData mapFileMeta,
                                                     int timeStepIndex, string substanceName, int segmentIndex)
        {
            if (!mapFileMeta.Substances.Contains(substanceName))
            {
                return null;
            }

            var data = new List<double>();

            int substanceIndex = mapFileMeta.Substances.IndexOf(substanceName);

            // size of 1 timestep and all substances
            int timeStepDataBlockSize = GetTimeStepDataBlockSize(mapFileMeta);
            int timeStepOffset = timeStepDataBlockSize * timeStepIndex;

            int substanceByteOffset = substanceIndex * 4;
            int segmentDataByteSize = mapFileMeta.NumberOfSubstances * 4;

            // Skip to the right position
            long startPosition = mapFileMeta.DataBlockOffsetInBytes + timeStepOffset + 4;

            delwaqOutputFileReader.BaseStream.Position = startPosition;

            if (segmentIndex != -1)
            {
                delwaqOutputFileReader.BaseStream.Position +=
                    substanceByteOffset + (segmentDataByteSize * segmentIndex);
                data.Add(delwaqOutputFileReader.ReadSingle());
            }
            else
            {
                for (var i = 0; i < mapFileMeta.NumberOfSegments; i++)
                {
                    delwaqOutputFileReader.BaseStream.Position +=
                        i == 0 ? substanceByteOffset : segmentDataByteSize - 4;
                    data.Add(delwaqOutputFileReader.ReadSingle());
                }
            }

            return data;
        }

        private static T DoWithMapFileBinairyReader<T>(string delwaqOutputFile, Func<BinaryReader, T> readerFunction)
            where T : class
        {
            if (IsInvalidDelwaqMapFile(delwaqOutputFile))
            {
                return null;
            }

            BinaryReader reader = null;

            try
            {
                reader = new BinaryReader(new FileStream(delwaqOutputFile, FileMode.Open, FileAccess.Read,
                                                         FileShare.Read));
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

        private static int GetTimeStepDataBlockSize(MapFileMetaData mapFileMetaData)
        {
            // timestep size + (number of segments * number of substances * float size)
            return 4 + (mapFileMetaData.NumberOfSubstances * mapFileMetaData.NumberOfSegments * 4);
        }

        private static bool IsInvalidDelwaqMapFile(string delwaqOutputFile)
        {
            // Check whether the output file exits or not or is Empty
            return !File.Exists(delwaqOutputFile) || new FileInfo(delwaqOutputFile).Length == 0;
        }
    }
}