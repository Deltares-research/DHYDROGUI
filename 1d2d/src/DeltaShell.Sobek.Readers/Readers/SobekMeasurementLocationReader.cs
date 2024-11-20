using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekMeasurementLocationReader : SobekReader<SobekMeasurementLocation>
    {
        public override IEnumerable<SobekMeasurementLocation> Parse(string datFileText)
        {
            const string pattern = @"(MEAS\s(?<TwoTwelveMeasurementLocation>(?'text'.*?))\smeas|TRGR\s(?<REMeasurementLocationFromTrigger>(?'text'.*?))\strgr|CNTL\s(?<REMeasurementLocationFromController>(?'text'.*?))\scntl)";

            foreach (Match measurementLocationMatch in RegularExpression.GetMatches(pattern, datFileText))
            {
                SobekMeasurementLocation measurementLocation = null;

                if (!string.IsNullOrEmpty(measurementLocationMatch.Groups["TwoTwelveMeasurementLocation"].Value))
                {
                    measurementLocation =
                        GetSobek212MeasurementLocation(measurementLocationMatch.Groups["TwoTwelveMeasurementLocation"].Value);

                    if (measurementLocation != null)
                        yield return measurementLocation;
                }

                if (!string.IsNullOrEmpty(measurementLocationMatch.Groups["REMeasurementLocationFromTrigger"].Value))
                {
                    measurementLocation =
                        GetSobekREMeasurementLocationFromTrigger(measurementLocationMatch.Groups["REMeasurementLocationFromTrigger"].Value);

                    if (measurementLocation != null)
                        yield return measurementLocation;
                }

                if (!string.IsNullOrEmpty(measurementLocationMatch.Groups["REMeasurementLocationFromController"].Value))
                {
                    var measurementLocations =
                        GetSobekREMeasurementLocationFromController(measurementLocationMatch.Groups["REMeasurementLocationFromController"].Value);

                    foreach (var sobekMeasurementLocation in measurementLocations.ToList())
                    {
                        yield return sobekMeasurementLocation;
                    }

                }

            }

        }

        private static SobekMeasurementLocation GetSobek212MeasurementLocation(string rowText)
        {
            //MEAS id '28_200' nm '28_200' ObID 'SBK_MEASSTAT' ci 'R_28' lc 199.999999999806 meas
            const string pattern = @"id\s'(?<id>" + RegularExpression.Characters + @")'\s" +
                                   @"(nm\s'(?<name>" + RegularExpression.ExtendedCharacters + @")'\s)?" +
                                   @"(?'text'.*?)" +
                                   @"ci\s'(?<branchId>" + RegularExpression.Characters + @")'\s" +
                                   @"lc\s(?<chainage>" + RegularExpression.Scientific + @")";

            var matches = RegularExpression.GetMatches(pattern, rowText);

            if (matches.Count == 1)
            {
                var id = matches[0].Groups["id"].Value;
                var name = matches[0].Groups["name"].Value;
                var branchId = matches[0].Groups["branchId"].Value;
                var chainage = ConversionHelper.ToDouble(matches[0].Groups["chainage"].Value);
                return new SobekMeasurementLocation { Id = id, Name = name, BranchId = branchId, Chainage = chainage};
            }

            return null;
        }

        private static SobekMeasurementLocation GetSobekREMeasurementLocationFromTrigger(string rowText)
        {
            //TRGR id '5' nm 'HY-OPEN-TR' ty 1 tp 0 tb '28' tl 200 ts '24' ch 0 ql '-1' tt tr 'Trigger' PDIN 1 0 '' pdin CLTT 'Time' 'On/Off' 'And/Or' 'Operation' 'Water Level [m]' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE 
            //'1975/12/25;00:00:00' -1 -1 0 1.8 < 
            //tble
            //trgr
            const string pattern = @"tb\s'(?<branchId>" + RegularExpression.ExtendedCharacters + @")'\s" +
                                   @"tl\s(?<chainage>" + RegularExpression.Scientific + @")\s";

            var matches = RegularExpression.GetMatches(pattern, rowText);

            if (matches.Count == 1)
            {
                var branchId = matches[0].Groups["branchId"].Value;
                var chainage = ConversionHelper.ToDouble(matches[0].Groups["chainage"].Value);
                var id = MeasurementLocationIdGenerator.GetMeasurementLocationId(branchId, chainage);
                return new SobekMeasurementLocation { Id = id, Name = id, BranchId = branchId, Chainage = chainage };
            }

            return null;
        }

        private static IEnumerable<SobekMeasurementLocation> GetSobekREMeasurementLocationFromController(string rowText)
        {
            //CNTL id '68' nm 'HY_Sluit_Contr' ta 1 1 0 0 gi '4' '20770' '-1' '-1' ao 1 1 1 1 ct 0 ac 1 ca 2 cf 1 cb '28' '-1' '-1' '-1' '-1' cl 0 9.9999e+009 9.9999e+009 9.9999e+009 9.9999e+009 cp 0 ti tv 'Time Controller' PDIN 0 0 '' pdin CLTT 'Time' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE 
            //'1991/01/01;00:00:00' 0 <
            //tble
            //mp 20000 mc 0.0025 sp tc 0 9.9999e+009 9.9999e+009 ui 9.9999e+009 ua 9.9999e+009 u0 9.9999e+009 pf 9.9999e+009 if 9.9999e+009 df 9.9999e+009 va 9.9999e+009 si '24' hc ht 5 9.9999e+009 9.9999e+009 'Hydraulic Controller' PDIN 0 0 '' pdin CLTT 'Water Level [m]' 'Gate Height [m]' cltt CLID '(null)' '(null)' clid TBLE " + Environment.NewLine +
            //-10 8 < 
            //2 8 < 
            //2 0 < 
            //10 0 < 
            //tble
            //bl 1 0 0 0 0 ps 9.9999e+009 ns 9.9999e+009 cn 0 du 9.9999e+009 cv 9.9999e+009 dt 0 pe 9.9999e+009 d_ 9.9999e+009 di 9.9999e+009 da 9.9999e+009 cntl

            const string pattern = @"cb\s'(?<branchId1>" + RegularExpression.ExtendedCharacters + @")'\s'(?<branchId2>" + RegularExpression.ExtendedCharacters + @")'\s'(?<branchId3>" + RegularExpression.ExtendedCharacters + @")'\s'(?<branchId4>" + RegularExpression.ExtendedCharacters + @")'\s'(?<branchId5>" + RegularExpression.ExtendedCharacters + @")'\s" +
                                   @"cl\s(?<chainage1>" + RegularExpression.Scientific + @")\s(?<chainage2>" + RegularExpression.Scientific + @")\s(?<chainage3>" + RegularExpression.Scientific + @")\s(?<chainage4>" + RegularExpression.Scientific + @")\s(?<chainage5>" + RegularExpression.Scientific + @")\s";


            string id,branchId;
            double chainage;
            var matches = RegularExpression.GetMatches(pattern, rowText);

            if (matches.Count == 1)
            {
                if(!string.IsNullOrEmpty(matches[0].Groups["branchId1"].Value) && matches[0].Groups["branchId1"].Value != "-1")
                {
                    branchId = matches[0].Groups["branchId1"].Value;
                    chainage = ConversionHelper.ToDouble(matches[0].Groups["chainage1"].Value);
                    id = MeasurementLocationIdGenerator.GetMeasurementLocationId(branchId, chainage);
                    yield return new SobekMeasurementLocation { Id = id, Name = id, BranchId = branchId, Chainage = chainage };
                }

                if (!string.IsNullOrEmpty(matches[0].Groups["branchId2"].Value) && matches[0].Groups["branchId2"].Value != "-1")
                {
                    branchId = matches[0].Groups["branchId2"].Value;
                    chainage = ConversionHelper.ToDouble(matches[0].Groups["chainage2"].Value);
                    id = MeasurementLocationIdGenerator.GetMeasurementLocationId(branchId, chainage);
                    yield return new SobekMeasurementLocation { Id = id, Name = id, BranchId = branchId, Chainage = chainage };
                }

                if (!string.IsNullOrEmpty(matches[0].Groups["branchId3"].Value) && matches[0].Groups["branchId3"].Value != "-1")
                {
                    branchId = matches[0].Groups["branchId3"].Value;
                    chainage = ConversionHelper.ToDouble(matches[0].Groups["chainage3"].Value);
                    id = MeasurementLocationIdGenerator.GetMeasurementLocationId(branchId, chainage);
                    yield return new SobekMeasurementLocation { Id = id, Name = id, BranchId = branchId, Chainage = chainage };
                }

                if (!string.IsNullOrEmpty(matches[0].Groups["branchId4"].Value) && matches[0].Groups["branchId4"].Value != "-1")
                {
                    branchId = matches[0].Groups["branchId4"].Value;
                    chainage = ConversionHelper.ToDouble(matches[0].Groups["chainage4"].Value);
                    id = MeasurementLocationIdGenerator.GetMeasurementLocationId(branchId, chainage);
                    yield return new SobekMeasurementLocation { Id = id, Name = id, BranchId = branchId, Chainage = chainage };
                }

                if (!string.IsNullOrEmpty(matches[0].Groups["branchId5"].Value) && matches[0].Groups["branchId5"].Value != "-1")
                {
                    branchId = matches[0].Groups["branchId5"].Value;
                    chainage = ConversionHelper.ToDouble(matches[0].Groups["chainage5"].Value);
                    id = MeasurementLocationIdGenerator.GetMeasurementLocationId(branchId, chainage);
                    yield return new SobekMeasurementLocation { Id = id, Name = id, BranchId = branchId, Chainage = chainage };
                }
            }

        }

        public override IEnumerable<string> GetTags()
        {
            yield return "meas";
            yield return "trgr";
            yield return "cntl";
        }
    }
}
