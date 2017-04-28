using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.ImportExport.Sobek.Readers;
using DeltaShell.Plugins.ImportExport.Sobek.SobekData;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekValveDataReader
    {
        public static IList<SobekValveData> ReadValveData(string filePath)
        {
            //TODO: get this file stuff out..
            string defFileText = File.ReadAllText(filePath, Encoding.Default);

            return ParseValveData(defFileText);
        }

        public static IList<SobekValveData> ParseValveData(string tabFileText)
        {
            const string structurePattern = @"(VLVE (?'text'.*?)vlve)";
            IList<SobekValveData> result = new List<SobekValveData>();

            foreach (Match structureMatch in RegularExpression.GetMatches(structurePattern, tabFileText))
            {
                result.Add(GetSobekValveData(structureMatch.Value));
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
            var valveData = new SobekValveData();
            valveData.Id = match.Groups["Id"].Value;
            
            string table = match.Groups["Table"].Value;
            valveData.DataTable = SobekDataTableReader.GetTable(table, new Dictionary<string, Type>
                                                                           {
                                                                               {"gateopeningfactor", typeof (double)},
                                                                               {"losscoefficient", typeof (double)},
                                                                           });
            

            return valveData;
        }
    }
}