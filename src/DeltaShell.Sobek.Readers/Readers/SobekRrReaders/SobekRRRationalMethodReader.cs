using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRROpenWaterFromRationalMethodReader : SobekReader<SobekRROpenWater>
    {
        // Lateral discharge on branch:
        // FLBR id '3' sc 0 lt 0 dc lt 0 ir 0 ms 'station 1' ii 0.005 ar 600000 flbr
        // or
        // FLBR id 'Intensity from Meteostation' sc 0 lt 0 dc lt 7 ir 0.003 ms 'meteostation' ii 0.002 ar 1000 flbr

        // sc and lt will already be imported by the lateral source importer

        // dc lt 6  = rational method with constant intensity
        // dc lt 7  = with intensity from the rainfall station
        // ir = constant intensity (mm/s)
        // ms = meteo-station
        // ii = seepage/infiltration intensity (mm/s)
        // ar = runoff area (m2)

        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekRROpenWaterFromRationalMethodReader));

        public override IEnumerable<SobekRROpenWater> Parse(string text)
        {
            const string lateralConditionPattern = @"(FLBR\s+(?'text'.*?)\s+flbr)";

            foreach (Match match in RegularExpression.GetMatches(lateralConditionPattern, text))
            {
                SobekRROpenWater sobekRROpenWater = GetOpenWaterFromLateralSourceWithRationalMethod(match.Value);
                if (sobekRROpenWater != null)
                {
                    yield return sobekRROpenWater;
                }
            }
        }

        private static SobekRROpenWater GetOpenWaterFromLateralSourceWithRationalMethod(string line)
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

            pattern = RegularExpression.GetScientific("lt") + "|" +
                             RegularExpression.GetScientific("lc") + "|" +
                             @"dc\s+'?(?<dc>" + RegularExpression.ExtendedCharacters + @")'?\s?";

            foreach (Match match in RegularExpression.GetMatches(pattern, line))
            {
                if (match.Value.StartsWith("dc lt"))
                {
                    matches = RegularExpression.GetMatches(@"dc lt (?<ltype>" + RegularExpression.Integer + @")",
                                                               match.Value);
                    var type = int.Parse(matches[0].Groups["ltype"].Value);
                    switch (type)
                    {
                        case 6:
                            // note: constant intensity will be imported from flow lateral directly
                            sobekRROpenWater.MethodType = RationalMethodType.ConstantIntensity;
                            break;
                        case 7:
                            sobekRROpenWater.MethodType = RationalMethodType.RainfallStation;
                            break;
                        default:
                            //No LateralSource with Rational Method
                            return null;
                    }
                }

            }

            label = "ir";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRROpenWater.ConstantIntensity = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            label = "ms";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRROpenWater.MeteoStationId = matches[0].Groups[label].Value;
            }

            label = "ii";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRROpenWater.InfiltrationItensity = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            label = "ar";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRROpenWater.Area = Convert.ToDouble(matches[0].Groups[label].Value,CultureInfo.InvariantCulture);
            }

            return sobekRROpenWater;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "flbr";
        }
    }
}
