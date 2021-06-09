using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using DelftTools.Utils.RegularExpressions;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRREvaporationReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRREvaporationReader));

        public static IEnumerable<DataTable> ReadEvaporationData(string filePath, DateTime? startDateTime = null, DateTime? stopDateTime = null)
        {
            //todo: refactor reading efficiently (w.r.t memory use)

            if(!File.Exists(filePath))
            {
                log.ErrorFormat("Could not find file {0}.", filePath);
                return new List<DataTable>();
            }
            return new[] {GetSobekEvaporationStation(File.ReadLines(filePath), startDateTime, stopDateTime)};
        }

        public static IEnumerable<DataTable> ParseEvaporationData(string fileContent, DateTime? startDateTime = null, DateTime? stopDateTime = null)
        {
            var lines = fileContent.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return new[] {GetSobekEvaporationStation(lines, startDateTime, stopDateTime)};
        }

        private static DataTable GetSobekEvaporationStation(IEnumerable<string> lines, DateTime? startDateTime, DateTime? stopDateTime)
        {
            const string pattern = @"\s*(?<year>\d+)\s+(?<month>\d+)\s+(?<day>\d+)\s+(?<mm_per_day>" + RegularExpression.Scientific + @"\s*)+";
            const string specialKeyPattern = @"^\*(?<special_key>[^*].*$)";

            bool tableCreated = false;
            bool longTimeAveraging = false;
            int numberOfStations = 0;

            DataTable table = null;

            foreach (var line in lines)
            {
                
                var specialKeyMatch = RegularExpression.GetMatches(specialKeyPattern, line);
                if (specialKeyMatch.Count == 1)
                {
                    var specialKey = specialKeyMatch[0].Groups["special_key"].Captures[0].Value;
                    if(!longTimeAveraging)
                        longTimeAveraging = CheckIfSpecialKeyIsLongTimeAveraging(specialKey);
                    continue;
                }
                    
                var matches = RegularExpression.GetMatches(pattern, line);

                if (matches.Count == 1)
                {
                    if (!tableCreated)
                    {
                        numberOfStations = matches[0].Groups["mm_per_day"].Captures.Count;
                        // RegularExpression.Float now also matches empty string. This means that we should check
                        // if the last entry in the match is empty and if so decrease numberOfStations by 1
                        if (matches[0].Groups["mm_per_day"].Captures[numberOfStations - 1].Value == "")
                        {
                            --numberOfStations;
                        }
                        table = CreateTimeEvaporationTable(numberOfStations);
                        tableCreated = true;
                    }
                    var row = table.NewRow();
                    row[0] = Convert.ToInt32(matches[0].Groups["year"].Value);
                    row[1] = Convert.ToInt32(matches[0].Groups["month"].Value);
                    row[2] = Convert.ToInt32(matches[0].Groups["day"].Value);
                    try
                    {
                        for (int i = 0; i < numberOfStations; i++)
                        {
                            row[3 + i] = Convert.ToDouble(matches[0].Groups["mm_per_day"].Captures[i].Value.Trim(), CultureInfo.InvariantCulture);
                        }
                    }
                    catch (Exception)
                    {
                        log.Error("number of stations in evaporation file is inconsistent");
                        throw;
                    }
                    table.Rows.Add(row);
                }
            }

            if (!longTimeAveraging || table == null )
                return table;
            
            if (!startDateTime.HasValue || !stopDateTime.HasValue) return table;
            
            DataTable longTimeAverageTable = CreateTimeEvaporationTable(numberOfStations);
            DateTime currentTime = startDateTime.Value;
            
            foreach (DataRow dataRow in table.AsEnumerable())
            {
                if (dataRow != null)
                {
                    var row = longTimeAverageTable.NewRow();
                    row[0] = currentTime.Year;
                    row[1] = currentTime.Month;
                    row[2] = currentTime.Day;
                    for (int i = 0; i < numberOfStations; i++)
                    {
                        row[3 + i] = dataRow[3 + i];
                    }
                    longTimeAverageTable.Rows.Add(row);

                    currentTime = currentTime.AddDays(1);
                }
            }

            return longTimeAverageTable;
        }

        private static bool CheckIfSpecialKeyIsLongTimeAveraging(string specialKey)
        {
            return specialKey.Equals("Longtime average", StringComparison.InvariantCultureIgnoreCase);
        }

        private static DataTable CreateTimeEvaporationTable(int numberOfStations)
        {
            var dataTable = new DataTable("Evaporation station");
            dataTable.Columns.Add(new DataColumn("Year", typeof (int)));
            dataTable.Columns.Add(new DataColumn("Month", typeof(int)));
            dataTable.Columns.Add(new DataColumn("Day", typeof(int)));
            for (int i = 0; i < numberOfStations; i++)
            {
                dataTable.Columns.Add(new DataColumn(String.Format("Station {0}", i), typeof (double)));
            }
            return dataTable;
        }

        public static bool TryReadDateTime(DataRow row, out DateTime dateTime, out bool periodic)
        {
            int year, month, day;
            try
            {
                year = Convert.ToInt32(row[0]);
                month = Convert.ToInt32(row[1]);
                day = Convert.ToInt32(row[2]);
            }
            catch (Exception e)
            {
                dateTime = DateTime.MinValue;
                periodic = false;
                log.WarnFormat("Skipped invalid evaporation date readout {0}: " + e.Message, row);
                return false;
            }
            periodic = (year == 0);
            var newYear = year != 0 ? year : DateTime.Now.Year;
            if (month < 0 || month > 12)
            {
                dateTime = DateTime.MinValue;
                periodic = false;
                log.WarnFormat("Skipped invalid evaporation month in data row {0}", row);
                return false;
            }
            if (day > DateTime.DaysInMonth(newYear, month)) // necessary if we are shifting from leap to non-leap year
            {
                dateTime = DateTime.MinValue;
                periodic = false;
                log.WarnFormat("Skipped invalid evaporation day in data row {0}", row);
                return false;
            }
            dateTime = new DateTime(newYear, month, day);
            return true;
        }
    }
}
