using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Reads data from a csv file to create <see cref="DataTable"/> objects from.
    /// </summary>
    /// <example>
    /// Accepted fileformat, for linear- or constant interpolated respectively:
    /// <para>
    /// timeLinear,LocationHeaderName,substance,value
    /// <para>
    /// yyyy-MM-dd hh:mm:ss,Text,Text,Number with '.' as decimal separator
    /// </para>
    /// </para>
    /// or
    /// <para>
    /// timeBlock,LocationHeaderName,substance,value
    /// <para>
    /// yyyy-MM-dd hh:mm:ss,Text,Text,Number with '.' as decimal separator
    /// </para>
    /// </para>
    /// </example>
    public static class DataTableCsvFileReader
    {
        private const string LocationHeaderName = "location";
        private const char CsvDelimiter = ',';
        private const string TimeLinear = "timeLinear";
        private const string TimeBlock = "timeBlock";

        private static ILog Log = LogManager.GetLogger(typeof(DataTableCsvFileReader));

        /// <summary>
        /// Reads a water quality DataTable form a csv file.
        /// </summary>
        /// <param name="path"> The file-path. </param>
        /// <param name="useforsFolderPath"> Folder path to where the usefors includes are written to. </param>
        /// <returns> Data from the file. </returns>
        /// <exception cref="FormatException"> When the file is not in the expected format. </exception>
        public static DataTableCsvContents Read(string path, string useforsFolderPath)
        {
            if (!File.Exists(path))
            {
                string message = string.Format("Not a valid file-path ({0}) specified.", path);
                throw new ArgumentException(message, nameof(path));
            }

            DataTableInterpolationType type = GetInterpolationTypeFromHeader(path);

            DataTable dataTable = ReadCsv(path);

            return CreateDataTableCsvContents(path, type, dataTable, useforsFolderPath);
        }

        private static DataTableInterpolationType GetInterpolationTypeFromHeader(string path)
        {
            using (var stream = new StreamReader(path))
            {
                string header = stream.ReadLine();
                if (header != null)
                {
                    string[] headerElements = header.Split(CsvDelimiter);
                    if (headerElements.Length == 4 &&
                        string.Equals(headerElements[1], LocationHeaderName,
                                      StringComparison.InvariantCultureIgnoreCase) &&
                        string.Equals(headerElements[2], "substance", StringComparison.InvariantCultureIgnoreCase) &&
                        string.Equals(headerElements[3], "value", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (string.Equals(headerElements[0], TimeLinear, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return DataTableInterpolationType.Linear;
                        }

                        if (string.Equals(headerElements[0], TimeBlock, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return DataTableInterpolationType.Block;
                        }
                    }
                }

                string message = string.Format("No valid header was found; First line: {0}" + Environment.NewLine +
                                               "Expected: time[Block/Linear],location,substance,value",
                                               header ?? "<missing>");
                throw new FormatException(message);
            }
        }

        private static DataTable ReadCsv(string path)
        {
            var csvImporter = new CsvImporter();
            var csvMappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = CsvDelimiter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {new CsvRequiredField("time", typeof(DateTime)), new CsvColumnInfo(0, new DateTimeFormatInfo {FullDateTimePattern = "yyyy-MM-dd HH:mm:ss"})},
                    {new CsvRequiredField(LocationHeaderName, typeof(string)), new CsvColumnInfo(1, CultureInfo.InvariantCulture)},
                    {new CsvRequiredField("substance", typeof(string)), new CsvColumnInfo(2, CultureInfo.InvariantCulture)},
                    {new CsvRequiredField("value", typeof(string)), new CsvColumnInfo(3, CultureInfo.InvariantCulture)}
                }
            };

            DataTable dataTable = csvImporter.ImportCsv(path, csvMappingData);
            if (dataTable.HasErrors && dataTable.Rows.Count > 0)
            {
                var i = 0;
                DataRow row = dataTable.Rows[i];
                while (!row.HasErrors)
                {
                    i++;
                    if (i == dataTable.Rows.Count)
                    {
                        return dataTable;
                    }

                    row = dataTable.Rows[i];
                }

                if (row.HasErrors && row.IsNull(0))
                {
                    Log.ErrorFormat(
                        "Time column could not be parsed correctly, please ensure it is formatted as yyyy-MM-dd HH:mm:ss");
                }

                if (row.HasErrors && row.IsNull(3))
                {
                    Log.ErrorFormat(
                        "Value column could not be parsed correctly, please ensure it is formatted correctly");
                }
            }

            return dataTable;
        }

        private static DataTableCsvContents CreateDataTableCsvContents(string path, DataTableInterpolationType type,
                                                                       DataTable dataTable, string useforsFolderPath)
        {
            var result = new DataTableCsvContents
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Interpolation = type,
                UseforIncludeFolderPath = useforsFolderPath
            };

            LocationData locationData = null;
            List<DataRow> dataRows = dataTable.Select(null, string.Format("{0} ASC", LocationHeaderName)).ToList();
            foreach (DataRow row in dataRows) // Presort rows on location without filtering any entries
            {
                var location = (string) row[1];
                if (locationData == null || locationData.Name != location)
                {
                    locationData = new LocationData {Name = location};
                    result.DataRows.Add(locationData);
                }

                var time = (DateTime) row[0];
                if (!locationData.TimeDependentSubstanceData.ContainsKey(time))
                {
                    locationData.TimeDependentSubstanceData[time] = new Dictionary<string, string>();
                }

                var substance = (string) row[2];
                var substanceValue = (string) row[3];
                if (!double.TryParse(substanceValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double _))
                {
                    Log.ErrorFormat(
                        Resources
                            .DataTableCsvFileReader_CreateDataTableCsvContents_Line__0__contains_wrong_substance_value___1_,
                        dataRows.IndexOf(row) + 1, substanceValue);
                }

                locationData.TimeDependentSubstanceData[time][substance] = substanceValue;
            }

            return result;
        }
    }
}