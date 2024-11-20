using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    /// <summary>
    /// SobekRROpenwWaterReader will be used for testing. The OWRR tag in Openwate.3B is added for D-RR
    /// </summary>
    public class SobekRROpenWaterReader : SobekReader<SobekRROpenWater>
    {
        public override IEnumerable<SobekRROpenWater> Parse(string text)
        {
            // The tag OWRR is for (historical) testing purposes, the open water nodes
            // in OPENWATE.3B have the tag OPWA, and are currently not imported
            const string pattern = @"OWRR (?'text'.*?)owrr" + RegularExpression.EndOfLine;

            var matches = RegularExpression.GetMatches(pattern, text);

            foreach (Match match in matches)
            {
                SobekRROpenWater sobekRROpenWater = GetOpenWater(match.Value);
                if (sobekRROpenWater != null)
                {
                    yield return sobekRROpenWater;
                }
            }
        }

        private static SobekRROpenWater GetOpenWater(string line)
        {

            var sobekRROpenWater = new SobekRROpenWater();

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRROpenWater.Id = matches[0].Groups[label].Value;
            }



            label = "ms";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRROpenWater.MeteoStationId = matches[0].Groups[label].Value;
            }

            label = "ar";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRROpenWater.Area = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            label = "aaf";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRROpenWater.AreaAjustmentFactor = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            return sobekRROpenWater;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "owrr";
        }
    }
}
