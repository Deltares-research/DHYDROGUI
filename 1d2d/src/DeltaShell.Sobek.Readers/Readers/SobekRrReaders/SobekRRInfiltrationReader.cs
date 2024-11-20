using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRInfiltrationReader : SobekReader<SobekRRInfiltration>
    {
        public override IEnumerable<SobekRRInfiltration> Parse(string fileContent)
        {
            const string pattern = @"INFC (?'text'.*?)infc" + RegularExpression.EndOfLine;

            return (from Match line in RegularExpression.GetMatches(pattern, fileContent)
                    select GetSobekRRInfiltration(line.Value)).ToList();
        }

        private static SobekRRInfiltration GetSobekRRInfiltration(string line)
        {
            var sobekRRInfiltration = new SobekRRInfiltration();


            //id   =          infiltration identification
            //nm  =          name
            //ic    =          infiltration capacity of the soil, constant. (mm/hour)
            //                  Remark: no variable infiltration capacity implemented yet.

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRInfiltration.Id = matches[0].Groups[label].Value;
            }

            //Name
            label = "nm";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRInfiltration.Name = matches[0].Groups[label].Value;
            }

            //Infiltration capacity
            label = "ic";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRInfiltration.InfiltrationCapacity = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            return sobekRRInfiltration;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "infc";
        }
    }
}
