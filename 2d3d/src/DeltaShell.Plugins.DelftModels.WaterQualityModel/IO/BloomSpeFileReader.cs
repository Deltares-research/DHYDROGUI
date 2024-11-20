using System;
using System.Collections.Generic;
using System.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public static class BloomSpeFileReader
    {
        // word index of KORT. There are 2 KORT headers, so it cannot be determined from the file while reading.
        private const int KORT_INDEX = 3;

        private const int DESCRIPTION_CHAR_INDEX = 34;
        private const int DESCRIPTION_CHAR_LENGTH = 31;
        private const char COMMENTCHAR = ';';

        public static BloomInfo Read(string path)
        {
            if (!File.Exists(path))
            {
                string message = string.Format("Not a valid file-path ({0}) specified.", path);
                throw new ArgumentException(message, nameof(path));
            }

            using (var streamReader = new StreamReader(path))
            {
                int numberOfRows = ParseNumberOfRows(streamReader.ReadLine()); // number of algae types
                int numberOfColumns = ParseNumberOfColumns(streamReader.ReadLine());

                List<string> algHeaders = ParseHeaders(streamReader.ReadLine(), numberOfColumns);

                var korts = new List<string>();
                var descriptions = new List<string>();

                for (var i = 0; i < numberOfRows; i++)
                {
                    string row = streamReader.ReadLine();
                    if (string.IsNullOrEmpty(row))
                    {
                        continue;
                    }

                    korts.Add(ParseKortFromRow(row));
                    descriptions.Add(ParseDescriptionFromRow(row));
                }

                return new BloomInfo(algHeaders, korts, descriptions);
            }
        }

        private static string ParseDescriptionFromRow(string row)
        {
            return row.Substring(DESCRIPTION_CHAR_INDEX, DESCRIPTION_CHAR_LENGTH).TrimEnd();
        }

        private static string ParseKortFromRow(string row)
        {
            return row.Split(new[]
            {
                ' '
            }, StringSplitOptions.RemoveEmptyEntries)[KORT_INDEX];
        }

        private static List<string> ParseHeaders(string headerRow, int numberOfColumns)
        {
            string[] splitColumnHeaders = headerRow.Split(new[]
            {
                ' '
            }, StringSplitOptions.RemoveEmptyEntries);

            var result = new List<string>();

            int firstHeader = splitColumnHeaders.Length - numberOfColumns;
            for (int i = firstHeader; i < splitColumnHeaders.Length; i++)
            {
                result.Add(splitColumnHeaders[i]);
            }

            return result;
        }

        private static int ParseNumberOfColumns(string numberOfColumnsString)
        {
            return int.Parse(ReadValueBeforeComment(numberOfColumnsString));
        }

        private static int ParseNumberOfRows(string numberOfRowsString)
        {
            return int.Parse(ReadValueBeforeComment(numberOfRowsString));
        }

        private static string ReadValueBeforeComment(string numberOfRowsString)
        {
            int commentStart = numberOfRowsString.IndexOf(COMMENTCHAR);
            string numberRaw = numberOfRowsString.Substring(0, commentStart);

            return numberRaw.Trim();
        }
    }
}