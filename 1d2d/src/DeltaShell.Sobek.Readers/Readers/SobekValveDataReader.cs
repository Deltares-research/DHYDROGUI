using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekValveDataReader : SobekReader<SobekValveData>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekValveDataReader));

        public override IEnumerable<SobekValveData> Parse(string tabFileText)
        {
            const string structurePattern = @"(VLVE (?'text'.*?)vlve)";
            IList<SobekValveData> result = new List<SobekValveData>();

            foreach (Match structureMatch in RegularExpression.GetMatches(structurePattern, tabFileText))
            {
                var sobekValveData = GetSobekValveData(structureMatch.Value);
                if (sobekValveData != null)
                {
                    result.Add(sobekValveData);
                }
            }

            return result;
        }

        private static SobekValveData GetSobekValveData(string text)
        {
            const string pattern = @"id\s'(?<Id>" + RegularExpression.ExtendedCharacters + @")'\s" +
                                   @"nm\s'(?<Name>" + RegularExpression.ExtendedCharacters + @")'\s" +
                                   @"lt\slc\s" +
                                   @"(?<Table>" + RegularExpression.ExtendedCharacters + @")"; //the rest will be the table

            var match = RegularExpression.GetFirstMatch(pattern, text);
            if (match == null)
            {
                Log.WarnFormat("Could not parse valve definition (\"{0}\")",text);
                return null;
            }

            var dataTable = SobekDataTableReader.GetTable(
                match.Groups["Table"].Value,
                new Dictionary<string, Type>
                    {
                        {
                            "gateopeningfactor", typeof (double)
                            },
                        {
                            "losscoefficient", typeof (double)
                            },
                    });

            return new SobekValveData
                       {
                           Id = match.Groups["Id"].Value,
                           DataTable = dataTable
                       };
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "vlve";
        }
    }
}