using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekDataTableReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekDataTableReader));

        public static DataTable GetTable(string text, DataTable table)
        {
            // truncate input to avoid too greedy quantifier in regex
            var endTableIndex = text.IndexOf("tble", StringComparison.Ordinal);
            if (-1 == endTableIndex)
            {
                throw new ArgumentException("table string in incorrect format; should end with 'tble'");
            }
            
            var beginTableIndex = text.IndexOf("TBLE", StringComparison.Ordinal);
            if (beginTableIndex == -1)
            {
                throw new ArgumentException("table string in incorrect format; should start with 'TBLE'");
            }
            beginTableIndex += "TBLE".Length;

            if (endTableIndex < beginTableIndex)
            {
                throw new ArgumentException("table string in incorrect format; 'tble' appears before 'TBLE'");
            }

            var columnConvertionLookup = table.Columns.Cast<DataColumn>().ToDictionary(c => table.Columns.IndexOf(c), ColumnParseFunction);

            foreach (var rowText in SplitInRows(text, beginTableIndex, endTableIndex))
            {
                try
                {
                    var columnValues = GetColumnValues(rowText);
                    if (columnValues.Length == 0) continue;

                    if (table.Columns.Count == 0)
                    {
                        for (var i = 0; i < columnValues.Length; i++)
                        {
                            table.Columns.Add(string.Format("column{0}", i), typeof(double));
                        }
                    }

                    var newRow = table.NewRow();

                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        newRow[i] = columnConvertionLookup[i](columnValues[i]);
                    }
                  
                    table.Rows.Add(newRow);
                }
                catch (Exception)
                {
                    log.ErrorFormat("Failure parsing table row, unexpected format: {0}", rowText);
                }
            }

            return table;
        }

        private static IEnumerable<string> SplitInRows(string text, int beginIndex, int endIndex)
        {
            int index = -1;
            var isInString = false;
            var sb = new StringBuilder();
            foreach (var c in text)
            {
                index++;
                
                if (index < beginIndex) continue;
                if (index >= endIndex) break;

                if (c == '\'') isInString = !isInString;
                if (c == '\r' || c == '\n') continue;
                
                if (c == '<' && !isInString)
                {
                    yield return sb.ToString();
                    sb.Clear();
                    continue;
                }

                sb.Append(c);
            }

            // last one possibly doesn't have separator character??
            if (sb.Length > 0)
            {
                yield return sb.ToString();
            }
        }

        private static string[] GetColumnValues(string rowText)
        {
            var stringList = new List<string>();

            var array = rowText.ToCharArray();
            var sb = new StringBuilder();
            var isInString = false;

            foreach (char c in array)
            {
                if (c == '\'')
                {
                    isInString = !isInString;
                }

                var isSpaceBetweenValues = (c == ' ' && !isInString);
                if (isSpaceBetweenValues)
                {
                    if (sb.Length > 0)
                    {
                        stringList.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }

                sb.Append(c);
            }

            // add last
            if (sb.Length > 0) stringList.Add(sb.ToString());
            return stringList.ToArray();
        }

        private static Func<string, object> ColumnParseFunction(DataColumn dataColumn)
        {
            if (dataColumn.DataType == typeof(Single))
            {
                return s => ConversionHelper.ToSingle(s);
            }
            if (dataColumn.DataType == typeof(Double))
            {
                return s => ConversionHelper.ToDouble(s);
            }
            if (dataColumn.DataType == typeof(DateTime))
            {
                return s =>
                    {
                        var value = s.Trim('\'').Replace(';',' ');
                        return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
                    };
            }
            if (dataColumn.DataType == typeof(Boolean))
            {
                return s => Convert.ToBoolean(Convert.ToInt32(s));
            }
            if (dataColumn.DataType == typeof(string))
            {
                return s => s;
            }
            throw new ArgumentException("");
        }

        public static DataTable GetTable(string text, IEnumerable<KeyValuePair<string, Type>> columns)
        {
            var table = new DataTable();
            table.BeginLoadData();
            foreach (var column in columns)
            {
                table.Columns.Add(column.Key, column.Value);
            }
            return GetTable(text, table);
        }

        /// <summary>
        /// Creates a datatable based on the names given in the SobekMDB CLTT record
        /// e.g. CLTT 'Q' '0' '26861.89539' '27410.09734' '37825.93433' '38374.13628' '61946.81999' '62495.02194' cltt
        /// </summary>
        /// <param name="tableDef"></param>
        /// <returns></returns>
        public static DataTable CreateDataTableDefinitionFromColumNames(string tableDef)
        {
            // truncate input to avoid too greedy quantifier in regex
            tableDef = tableDef.Substring(0, tableDef.IndexOf("cltt") + 4);

            const string columns = @"CLTT(?<columns>" + RegularExpression.CharactersAndQuote + @")cltt";
            var regex = new Regex(columns, RegexOptions.Singleline);
            var fmatches = regex.Matches(tableDef);
            var names = fmatches[0].Groups["columns"].Value.Split(new [] {' ', '\''}, StringSplitOptions.RemoveEmptyEntries);
            var dataTable = new DataTable();
            dataTable.BeginLoadData(); 
            foreach (var name in names)
            {
                dataTable.Columns.Add(name, typeof(double));
            }
            return dataTable;
        }
    }
}