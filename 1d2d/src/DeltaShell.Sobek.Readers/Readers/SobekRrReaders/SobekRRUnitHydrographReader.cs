using System.Collections.Generic;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRUnitHydrographReader : SobekReader<SobekRRUnitHydrograph>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (SobekRRUnitHydrographReader));

        public override IEnumerable<SobekRRUnitHydrograph> Parse(string text)
        {
            var hydrograph = new SobekRRUnitHydrograph();
            
            const string pattern = @"UNIH\s+?" + IdAndOptionalNamePattern + @"uh\s+?(?<uhArray>.*?)\s+?dt\s+?(?<dt>" + RegularExpression.Scientific + @")\s+?unih";
            const string reversedPattern = @"UNIH\s+?" + IdAndOptionalNamePattern + @"dt\s+?(?<dt>" + RegularExpression.Scientific + @")\s+?uh\s+?(?<uhArray>.*?)\s+?unih";

            var matches = RegularExpression.GetMatches(pattern, text);
            if (matches.Count != 1)
            {
                matches = RegularExpression.GetMatches(reversedPattern, text);
                if (matches.Count != 1)
                {
                    log.ErrorFormat("Could not parse Sacramento unit hydrograph record: {0}", text);
                    yield break;
                }
            }
            hydrograph.Id = matches[0].Groups["id"].Value;

            double[] values;
            if (TryGetArrayOfNumbers("uh", text, 36, out values))
            {
                for (int i = 0; i < values.Length; ++i) hydrograph.Values[i] = values[i];
            }

            double stepsize;
            if (TryGetDoubleParameter("dt", text, out stepsize)) hydrograph.Stepsize = stepsize;

            yield return hydrograph;
        }

        protected override void ReportParseError(string label, string record)
        {
            log.ErrorFormat("Could not parse parameter {0} from Sacramento unit hydrograph record: {1}", label,record);
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "unih";
        }
    }
}