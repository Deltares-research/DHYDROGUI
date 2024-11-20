using System.Collections.Generic;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRHbvHiniReader : SobekReader<SobekRRHbvHini>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRHbvHiniReader));

        public override IEnumerable<SobekRRHbvHini> Parse(string text)
        {
            var hiniRecord = new SobekRRHbvHini();

            // HINI id '1' nm 'RRNF initialisation parameters' ds 0.0 fw 0.0 sm 0.6 uz 0.0 lz 180.0 hini 
            const string pattern = @"HINI\s+?" + IdAndOptionalNamePattern + @"(?<text>.*?)\s+hini" + RegularExpression.EndOfLine;
            var matches = RegularExpression.GetMatches(pattern, text);
            if (matches.Count != 1)
            {
                log.ErrorFormat("Could not parse HBV snow record: {0}", text);
                yield break;
            }

            hiniRecord.Id = matches[0].Groups["id"].Value;
            var subText = matches[0].Groups["text"].Value;

            double value;
            if (TryGetDoubleParameter("ds", subText, out value)) hiniRecord.InitialDrySnowContent = value;
            if (TryGetDoubleParameter("fw", subText, out value)) hiniRecord.InitialFreeWaterContent = value;
            if (TryGetDoubleParameter("sm", subText, out value)) hiniRecord.InitialSoilMoistureContents = value;
            if (TryGetDoubleParameter("uz", subText, out value)) hiniRecord.InitialUpperZoneContent = value;
            if (TryGetDoubleParameter("lz", subText, out value)) hiniRecord.InitialLowerZoneContent = value;
            
            yield return hiniRecord;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "hini";
        }

        protected override void ReportParseError(string label, string record)
        {
            log.ErrorFormat("Failed to parse parameter {0} from HBV HINI record: {1}", label, record);
        }
    }
}