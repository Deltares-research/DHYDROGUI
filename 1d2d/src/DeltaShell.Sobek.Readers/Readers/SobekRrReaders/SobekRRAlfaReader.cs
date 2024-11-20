using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRAlfaReader : SobekReader<SobekRRAlfa>
    {
        public override IEnumerable<SobekRRAlfa> Parse(string fileContent)
        {
            const string pattern = @"ALFA\s+?" + IdAndOptionalNamePattern + @"(?'text'.*?)alfa" + RegularExpression.EndOfLine;

            return (from Match line in RegularExpression.GetMatches(pattern, fileContent)
                    select GetSobekRRAlfa(line.Value)).ToList();
        }

        private static SobekRRAlfa GetSobekRRAlfa(string line)
        {
            var sobekRRAlfa = new SobekRRAlfa();

            //ALFA id 'alfa_1'   nm 'set1 alfa factors'   af  5.0 0.9 0.7 0.6 0.3 0.03 lv 0. 1.0 2.0  alfa


            // id   =          alfa-factors identification 
            // nm  =          name 
            // af   =          alfa factors (say a1 to a6) for Hellinga-de Zeeuw formula (1/day). 
            //   a1 = alfa factor surface runoff 
            //   a2 = alfa factor drainage to open water, top soil layer
            //   a3 = alfa factor drainage to open water, second layer
            //   a4 = alfa factor drainage to open water, third layer
            //  a5 = alfa factor drainage to open water, last layer
            //  a6 = alfa factor infiltration 

            //lv = three levels below surface (say lv1, lv2, lv3), separating the zones with various alfa-factors (or Ernst resistance values) for drainage.
            //a2 is used between surface level and lv1 m below the surface.
            //a3 is used between lv1 and lv2 m below the surface.
            //a4 is used between lv2 and lv3 m below the surface
            //a5 is used below lv3 m below surface.

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRAlfa.Id  = matches[0].Groups[label].Value;
            }

            //Name
            label = "nm";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRAlfa.Name = matches[0].Groups[label].Value;
            }

            //Alfa's
            pattern = @"af\s*(?<a1>" + RegularExpression.Scientific + @")" +
                      @"\s*(?<a2>" + RegularExpression.Scientific + @")" +
                      @"\s*(?<a3>" + RegularExpression.Scientific + @")" +
                      @"\s*(?<a4>" + RegularExpression.Scientific + @")" +
                      @"\s*(?<a5>" + RegularExpression.Scientific + @")" +
                      @"\s*(?<a6>" + RegularExpression.Scientific + @")";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRAlfa.FactorSurface = Convert.ToDouble(matches[0].Groups["a1"].Value, CultureInfo.InvariantCulture);
                sobekRRAlfa.FactorTopSoil = Convert.ToDouble(matches[0].Groups["a2"].Value, CultureInfo.InvariantCulture);
                sobekRRAlfa.FactorSecondLayer = Convert.ToDouble(matches[0].Groups["a3"].Value, CultureInfo.InvariantCulture);
                sobekRRAlfa.FactorThirdLayer = Convert.ToDouble(matches[0].Groups["a4"].Value, CultureInfo.InvariantCulture);
                sobekRRAlfa.FactorLastLayer = Convert.ToDouble(matches[0].Groups["a5"].Value, CultureInfo.InvariantCulture);
                sobekRRAlfa.FactorInfiltration = Convert.ToDouble(matches[0].Groups["a6"].Value, CultureInfo.InvariantCulture);
            }

            //Levels
            pattern = @"lv\s*(?<l1>" + RegularExpression.Scientific + @")" +
                      @"\s*(?<l2>" + RegularExpression.Scientific + @")" +
                      @"\s*(?<l3>" + RegularExpression.Scientific + @")";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRAlfa.Level1 = Convert.ToDouble(matches[0].Groups["l1"].Value, CultureInfo.InvariantCulture);
                sobekRRAlfa.Level2 = Convert.ToDouble(matches[0].Groups["l2"].Value, CultureInfo.InvariantCulture);
                sobekRRAlfa.Level3 = Convert.ToDouble(matches[0].Groups["l3"].Value, CultureInfo.InvariantCulture);
            }

            return sobekRRAlfa; 
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "alfa";
        }
    }
}
