using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekProfileDatFileReader : SobekReader<SobekCrossSectionMapping>
    {
        public override IEnumerable<SobekCrossSectionMapping> Parse(string text)
        {
            return RegularExpression.GetMatches(@"(CRSN (?'text'.*?)crsn)", text)
                .Cast<Match>()
                .Select(structureMatch => ReadOneMapping(structureMatch.Value))
                .Where(definition => definition != null);
        }

        private static SobekCrossSectionMapping ReadOneMapping(string line)
        {
            line = line.Trim();

            if (String.IsNullOrEmpty(line))
                return null;
            
            var match = Regex.Match(line, "CRSN (.*) crsn");

            if (!match.Success)
                return null;

            line = match.Groups[1].Value;

            var csMap = new SobekCrossSectionMapping();
            csMap.LocationId = RegularExpression.ParseFieldAsString("id", line);

            var idRemovedLine = RemoveTextBetweenQuotes(line, true); //prevents regex to react on tags inside id

            csMap.DefinitionId = RegularExpression.ParseFieldAsString("di", idRemovedLine);

            var cleanedLine = RemoveTextBetweenQuotes(line); //prevents regex to react on tags inside id's

            csMap.RefLevel1 = RegularExpression.ParseFieldAsDouble("rl", cleanedLine);
            csMap.RefLevel2 = RegularExpression.ParseFieldAsDouble("ll", cleanedLine);
            csMap.UpstreamSlope = RegularExpression.ParseFieldAsDouble("us", cleanedLine);
            csMap.DownstreamSlope = RegularExpression.ParseFieldAsDouble("ds", cleanedLine);
            csMap.SurfaceLevelLeft = RegularExpression.ParseFieldAsDouble("ls", cleanedLine);
            csMap.SurfaceLevelRight = RegularExpression.ParseFieldAsDouble("rs", cleanedLine);
            return csMap;
        }

        private static string RemoveTextBetweenQuotes(string line, bool onlyFirst=false)
        {
            var newline = new StringBuilder();
            var insideQuotes = false;
            var firstDone = false;
            foreach(var c in line)
            {
                if (!insideQuotes || (onlyFirst&&firstDone))
                {
                    newline.Append(c);
                }

                if (c == '\'')
                {
                    insideQuotes = !insideQuotes;
                    if (!insideQuotes)
                    {
                        firstDone = true;
                    }
                }
            }

            return newline.ToString();
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "crsn";
        }
    }
}