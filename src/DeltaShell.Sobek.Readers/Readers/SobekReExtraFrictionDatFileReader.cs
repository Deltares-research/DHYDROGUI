using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    /// <summary>
    /// Only for SobekRE; for sobek 2.* see SobekFrictionDatFileReader
    /// </summary>
    public class SobekReExtraFrictionDatFileReader : SobekReader<SobekReExtraResistance>
    {
        public override IEnumerable<SobekReExtraResistance> Parse(string text)
        {
            const string patternExtraFriction = @"(XRST (?'text'.*?)xrst)";

            var matchesGlobalFriction = RegularExpression.GetMatches(patternExtraFriction, text);

            return ParseSobekExtraFriction(matchesGlobalFriction);
        }

        // XRST id '1' nm 'exr1' ci '1' lc 20 rt rs
        // TBLE ..
        // tble
        // ty 0 xrst
        public static IEnumerable<SobekReExtraResistance> ParseSobekExtraFriction(MatchCollection extraFrictionCollection)
        {
            string pattern =
                RegularExpression.GetExtendedCharacters("id") + "|" +
                RegularExpression.GetExtendedCharacters("nm") + "|" +
                RegularExpression.GetExtendedCharacters("ci") + "|" +
                RegularExpression.GetScientific("lc") + "|" +
                RegularExpression.GetScientific("ty") + "|" +
                @"(rt rs(?<table>" + RegularExpression.CharactersAndQuote + @"))\s";

            foreach (Match extraFrictionMatch in extraFrictionCollection)
            {
                var sobekExtraResistance = new SobekReExtraResistance();
                var regex = new Regex(pattern, RegexOptions.Singleline);
                var matches = regex.Matches(extraFrictionMatch.Value);
                string table;
                foreach (Match match in matches)
                {
                    sobekExtraResistance.Id = RegularExpression.ParseString(match, "id", sobekExtraResistance.Id);
                    sobekExtraResistance.Name = RegularExpression.ParseString(match, "nm", sobekExtraResistance.Name);
                    sobekExtraResistance.BranchId = RegularExpression.ParseString(match, "ci", sobekExtraResistance.BranchId);
                    sobekExtraResistance.Chainage = RegularExpression.ParseDouble(match, "lc", sobekExtraResistance.Chainage);

                    table = RegularExpression.ParseString(match, "table", "");
                    if (table.Length > 0)
                    {
                        SobekDataTableReader.GetTable(table, (DataTable) sobekExtraResistance.Table);
                    }
                }
                yield return sobekExtraResistance;
            }
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "xrst";
        }
    }
}
