using System;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Reader for attribute files.
    /// </summary>
    /// <remarks> Only reads 'segment enabled' state data. </remarks>
    public static class AttributesFileReader
    {
        private const string CommentToken = ";";

        /// <summary>
        /// Reads all attribute data for a given size.
        /// </summary>
        /// <param name="nrOfSegmentsPerLayer"> The number of segments per layer. </param>
        /// <param name="nrOfLayers"> The number of layers. </param>
        /// <param name="attributesFile"> The attributes file (*.atr extension). </param>
        /// <returns> All data read from the attributes file reader. </returns>
        /// <exception cref="System.InvalidOperationException"> When attributes file cannot be found. </exception>
        /// <exception cref="System.FormatException"> When the data is malformatted or missing enabled state data block. </exception>
        public static AttributesFileData ReadAll(int nrOfSegmentsPerLayer, int nrOfLayers, FileInfo attributesFile)
        {
            string fullName = attributesFile == null ? "" : attributesFile.FullName;
            if (attributesFile == null || !attributesFile.Exists)
            {
                throw new InvalidOperationException(string.Format("Cannot find attributes file ({0}).", fullName));
            }

            var data = new AttributesFileData(nrOfSegmentsPerLayer, nrOfLayers);
            if (!ReadFileForSegmentEnabledState(data, attributesFile))
            {
                string message =
                    string.Format("Attributes file ({0}) does not contain data block for enabled state of segments.",
                                  fullName);
                throw new FormatException(message);
            }

            return data;
        }

        private static bool ReadFileForSegmentEnabledState(AttributesFileData data, FileInfo attributesFile)
        {
            var segmentEnabledDataRead = false;
            using (StreamReader streamReader = attributesFile.OpenText())
            {
                int numberOfDataBlocks = int.Parse(GetNextDataLine(streamReader));
                for (var i = 0; i < numberOfDataBlocks; i++)
                {
                    if (FindSegmentEnablednessBock(streamReader))
                    {
                        ParseSegmentEnablednessData(data, streamReader);
                        segmentEnabledDataRead = true;
                        break;
                    }

                    SkipDataBlock(data.IndexCount, streamReader);
                }
            }

            return segmentEnabledDataRead;
        }

        private static bool FindSegmentEnablednessBock(StreamReader streamReader)
        {
            if (GetNextDataLine(streamReader) != "1") // Number of attributes
            {
                throw new NotSupportedException(
                    "Reader does not support attribute files with multiple attributes per block.");
            }

            if (GetNextDataLine(streamReader) != "1") // Attribute type indicator
            {
                // Skip lines
                GetNextDataLine(streamReader);
                GetNextDataLine(streamReader);
                return false;
            }

            if (GetNextDataLine(streamReader) != "1") // Data not in this file
            {
                throw new NotSupportedException(
                    "Reader does not support attribute files with data outside the file itself.");
            }

            if (GetNextDataLine(streamReader) != "1") // Data without defaults
            {
                throw new NotSupportedException("Reader does not support sparse attribute files.");
            }

            return true;
        }

        private static void SkipDataBlock(int indexCount, StreamReader streamReader)
        {
            var index = 0;
            while (index < indexCount)
            {
                index += GetNextLineTextArray(streamReader).Length;
            }
        }

        private static void ParseSegmentEnablednessData(AttributesFileData data, StreamReader streamReader)
        {
            var index = 1;
            while (index <= data.IndexCount)
            {
                foreach (string booleanStringValue in GetNextLineTextArray(streamReader))
                {
                    data.SetSegmentActive(index++, GetBooleanValue(booleanStringValue));
                }
            }
        }

        private static string[] GetNextLineTextArray(StreamReader streamReader)
        {
            return GetNextDataLine(streamReader).Split(new[]
            {
                " ",
                "\t"
            }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool GetBooleanValue(string booleanStringValue)
        {
            if (booleanStringValue == "1")
            {
                return true;
            }

            if (booleanStringValue == "0")
            {
                return false;
            }

            throw new NotImplementedException();
        }

        private static string GetNextDataLine(StreamReader streamReader)
        {
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                string[] lineParts = line.Split(new[]
                {
                    CommentToken
                }, StringSplitOptions.None);
                if (!string.IsNullOrWhiteSpace(lineParts[0]))
                {
                    return lineParts[0].Trim();
                }
            }

            return null;
        }
    }
}