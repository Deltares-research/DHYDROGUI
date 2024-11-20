using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRTableReader : SobekReader<DataTable>
    {
        static string patternInterpolation = @"PDIN\s*(?<interpolation>" + RegularExpression.Integer + ")";
        static readonly Regex regexInterpolation = new Regex(patternInterpolation, RegexOptions.Singleline);

        private string parseTag;
        private DataTable formatTable;

        public SobekRRTableReader(string parseTag)
        {
            formatTable = new DataTable();
            formatTable.Columns.Add(new DataColumn("Time", typeof(DateTime)));
            formatTable.Columns.Add(new DataColumn("Value", typeof(double)));


            this.parseTag = parseTag;
        }

        public SobekRRTableReader(string parseTag, DataTable format)
        {
            this.parseTag = parseTag;
            this.formatTable = format;
        }

        public override IEnumerable<DataTable> Parse(string fileContent)
        {
            var pattern = @"(" + parseTag.ToUpper() + @" (?'text'.*?)" + parseTag.ToLower() + @")";

            return Enumerable.ToList<DataTable>((from Match line in RegularExpression.GetMatches(pattern, fileContent)
                                                                        select GetDataTable(line.Value)));
        }

        private DataTable GetDataTable(string line)
        {
            var dataTable = formatTable.Clone();

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                dataTable.TableName = matches[0].Groups[label].Value;
            }

            var match = regexInterpolation.Match(line);
            if (match.Success)
            {
                dataTable.ExtendedProperties.Add("block-interpolation", match.Groups[1].Value == "1");
            }

            SobekDataTableReader.GetTable(line, dataTable);

            return dataTable;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return parseTag.ToLower();
        }
    }
}
