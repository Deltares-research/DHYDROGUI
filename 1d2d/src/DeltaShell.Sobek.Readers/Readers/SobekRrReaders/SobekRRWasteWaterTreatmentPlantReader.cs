using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRWasteWaterTreatmentPlantReader : SobekReader<SobekRRWasteWaterTreatmentPlant>
    {
        public override IEnumerable<SobekRRWasteWaterTreatmentPlant> Parse(string fileContent)
        {
            const string wwtpPattern = @"WWTP\s+?" + IdAndOptionalNamePattern + @"(?'text'.*?)wwtp" + RegularExpression.EndOfLine;

            return (from Match unpavedLine in RegularExpression.GetMatches(wwtpPattern, fileContent)
                    select GetSobekWasteWaterTreatmentPlant(unpavedLine.Value)).ToList();
        }

        private static SobekRRWasteWaterTreatmentPlant GetSobekWasteWaterTreatmentPlant(string line)
        {

                  //id   =          node identification
                  //tb   =          table used yes/no
                  //tb 0 = no table of measured data; the WWTP outflow is equal to the sum of the inflows.
                  //tb 1 ‘WWTPTable’ = table of measured data with id ‘WWTPTable’

            var sobekWasteWaterTreatmentPlant = new SobekRRWasteWaterTreatmentPlant();

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekWasteWaterTreatmentPlant.Id = matches[0].Groups[label].Value;
            }

            //Table id
            label = "tb";
            pattern =  @"tb\s*1\s*'(?<" + label + ">" + RegularExpression.ExtendedCharacters + ")'";
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekWasteWaterTreatmentPlant.TableId = matches[0].Groups[label].Value;
            }
            return sobekWasteWaterTreatmentPlant;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "wwtp";
        }
    }
}
