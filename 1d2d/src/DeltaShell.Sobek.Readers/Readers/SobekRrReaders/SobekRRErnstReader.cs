using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRErnstReader : SobekReader<SobekRRErnst>
    {
        public override IEnumerable<SobekRRErnst> Parse(string fileContent)
        {
            const string pattern = @"ERNS\s+?" + IdAndOptionalNamePattern + @"(?'text'.*?)erns" + RegularExpression.EndOfLine;
            var matches = RegularExpression.GetMatches(pattern, fileContent);
            if (matches.Count == 0)
            {
                return new List<SobekRRErnst>();
            }
            return (from Match line in matches select GetSobekRRErnst(line.Value)).ToList();
        }

        private static SobekRRErnst GetSobekRRErnst(string line)
        {
            var sobekRRErnst = new SobekRRErnst();

            //id   =          alfa-factors identification 
            //nm  =          name 
            //cvi  =          Resistance value (in days) for infiltration from open water into unpaved area
            //cvo =         Resistance value (in days) for drainage from unpaved area to open water, for 3 layers 
            //cvs =          Resistance value (in days) for surface runoff
            //lv    =          three levels below surface (say lv1, lv2, lv3), separating the zones with various alfa-factors (or Ernst resistance values) for drainage.
            //                  a2 is used between surface level and lv1 m below the surface.
            //                  a3 is used between lv1 and lv2 m below the surface.
            //                  a4 is used between lv2 and lv3 m below the surface
            //                  a5 is used below lv3 m below surface.

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRErnst.Id = matches[0].Groups[label].Value;
            }

            label = "nm";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRErnst.Name = matches[0].Groups[label].Value;
            }

            label = "cvi";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRErnst.ResistanceInfiltration = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Resistance
            pattern = @"cvo\s*(?<r1>" + RegularExpression.Scientific + @")" +
                      @"\s*(?<r2>" + RegularExpression.Scientific + @")" +
                      @"\s*(?<r3>" + RegularExpression.Scientific + @")" +
                      @"\s*(?<r4>" + RegularExpression.Scientific + @")?";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRErnst.ResistanceLayer1 = Convert.ToDouble(matches[0].Groups["r1"].Value, CultureInfo.InvariantCulture);
                sobekRRErnst.ResistanceLayer2 = Convert.ToDouble(matches[0].Groups["r2"].Value, CultureInfo.InvariantCulture);
                sobekRRErnst.ResistanceLayer3 = Convert.ToDouble(matches[0].Groups["r3"].Value, CultureInfo.InvariantCulture);
                if (matches[0].Groups["r4"].Value != "")
                {
                    sobekRRErnst.ResistanceLayer4 = Convert.ToDouble(matches[0].Groups["r4"].Value, CultureInfo.InvariantCulture);
                }
            }

            label = "cvs";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRErnst.ResistanceSurface = Convert.ToDouble(matches[0].Groups[label].Value,CultureInfo.InvariantCulture);
            }

            //Level
            pattern = @"lv\s*(?<l1>" + RegularExpression.Scientific + @")" +
                      @"\s*(?<l2>" + RegularExpression.Scientific + @")" +
                      @"\s*(?<l3>" + RegularExpression.Scientific + @")";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRErnst.Level1 = Convert.ToDouble(matches[0].Groups["l1"].Value, CultureInfo.InvariantCulture);
                sobekRRErnst.Level2 = Convert.ToDouble(matches[0].Groups["l2"].Value, CultureInfo.InvariantCulture);
                sobekRRErnst.Level3 = Convert.ToDouble(matches[0].Groups["l3"].Value, CultureInfo.InvariantCulture);
            }

            return sobekRRErnst;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "erns";
        }
    }
}
