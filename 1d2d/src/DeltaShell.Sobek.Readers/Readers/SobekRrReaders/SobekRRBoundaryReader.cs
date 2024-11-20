using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRBoundaryReader : SobekReader<SobekRRBoundary>
    {
        public override IEnumerable<SobekRRBoundary> Parse(string fileContent)
        {
            const string bounPattern = @"BOUN (?'text'.*?)boun" + RegularExpression.EndOfLine;

            return (from Match boundaryLine in RegularExpression.GetMatches(bounPattern, fileContent)
                    select GetRRBoundary(boundaryLine.Value)).ToList();
        }

        private static SobekRRBoundary GetRRBoundary(string line)
        {
            //id   =          node identification
            //bl   =          boundary level  type
            //                bl 0 0.5        = fixed boundary level, of 0.5 m NAP
            //                bl 1  'bound_1' = variable boundary level with table identification ‘bound_1’
            //                                  (data in Bound3B.Tbl file) 
            //                bl 2  '3'       = variable boundary level, with on-line coupling Sobek. 
            //                                  Data taken from id ‘3’ in the Sobek-HIS-file. 
            //is   =          initial salt concentration (mg/l)

            var sobekRRBoundary = new SobekRRBoundary();

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRBoundary.Id = matches[0].Groups[label].Value;
            }

            //Fixed level
            label = "bl";
            pattern = @"bl\s*0\s*(?<" + label + ">" + RegularExpression.Scientific + ")";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRBoundary.FixedLevel = Convert.ToDouble(matches[0].Groups[label].Value,
                                                              CultureInfo.InvariantCulture);
            }

            //Table id
            pattern = @"bl\s*1\s*'(?<" + label + ">" + RegularExpression.ExtendedCharacters + ")'";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRBoundary.TableId = matches[0].Groups[label].Value;
            }

            //Variable level
            label = "bl";
            pattern = @"bl\s*2\s*'(?<" + label + ">" + RegularExpression.ExtendedCharacters + ")'";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRBoundary.VariableLevel = matches[0].Groups[label].Value;
            }

            //Initial salt concentration
            label = "is";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRBoundary.InitialSaltConcentration = Convert.ToDouble(matches[0].Groups[label].Value,
                                                                            CultureInfo.InvariantCulture);
            }

            return sobekRRBoundary;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "boun";
        }
    }
}