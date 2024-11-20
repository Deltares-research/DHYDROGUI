using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRGreenhouseSiloDefinitionReader : SobekReader<SobekRRGreenhouseSiloDefinition>
    {
        public override IEnumerable<SobekRRGreenhouseSiloDefinition> Parse(string fileContent)
        {
            const string pattern = @"SILO\s+(?'text'.*?)\s+silo" + RegularExpression.EndOfLine;

            return (from Match line in RegularExpression.GetMatches(pattern, fileContent)
                    select GetSobekRRGreenhouseSiloDefinition(line.Value)).ToList();
        }

        private static SobekRRGreenhouseSiloDefinition GetSobekRRGreenhouseSiloDefinition(string line)
        {
            var sobekRRGreenhouseSiloDefinition = new SobekRRGreenhouseSiloDefinition();
            //id   =          silo identification
            //nm   =          name (optional)
            //sc   =          silo capacity (m3/ha). Default 0.
            //pc   =          silo pump capacity (m3/s). Default 0.

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRGreenhouseSiloDefinition.Id = matches[0].Groups[label].Value;
            }

            label = "nm";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRGreenhouseSiloDefinition.Name = matches[0].Groups[label].Value;
            }

            label = "sc";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRGreenhouseSiloDefinition.SiloCapacity = Convert.ToDouble(matches[0].Groups[label].Value,
                                                                                CultureInfo.InvariantCulture);
            }

            label = "pc";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRGreenhouseSiloDefinition.SiloPumpCapacity = Convert.ToDouble(matches[0].Groups[label].Value,
                                                                                    CultureInfo.InvariantCulture);
            }

            return sobekRRGreenhouseSiloDefinition;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "silo";
        }
    }
}