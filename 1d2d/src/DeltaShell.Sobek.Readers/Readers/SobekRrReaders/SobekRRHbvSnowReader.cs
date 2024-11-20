using System.Collections.Generic;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRHbvSnowReader: SobekReader<SobekRRHbvSnow>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRHbvSnowReader));

        public override IEnumerable<SobekRRHbvSnow> Parse(string text)
        {
            var snowRecord = new SobekRRHbvSnow();

            // SNOW id '1' nm 'RRNF Snowmelt parameters' mc 4.0 sft 0.0 smt 0.0 tac 6.0 fe 0.005 fwf 0.1 snow 

            const string pattern = @"SNOW\s+?" + IdAndOptionalNamePattern + @"(?<text>.*?)\s+snow" + RegularExpression.EndOfLine;
            var matches = RegularExpression.GetMatches(pattern, text);
            if (matches.Count != 1)
            {
                log.ErrorFormat("Could not parse HBV snow record: {0}", text);
                yield break;
            }

            snowRecord.Id = matches[0].Groups["id"].Value;
            var subText = matches[0].Groups["text"].Value;

            double value;
            if (TryGetDoubleParameter("mc", subText, out value)) snowRecord.SnowMeltingConstant = value;
            if (TryGetDoubleParameter("sft", subText, out value)) snowRecord.SnowFallTemperature = value;
            if (TryGetDoubleParameter("smt", subText, out value)) snowRecord.SnowMeltTemperature = value;
            if (TryGetDoubleParameter("tac", subText, out value)) snowRecord.TemperatureAltitudeConstant = value;
            if (TryGetDoubleParameter("fe", subText, out value)) snowRecord.FreezingEfficiency = value;
            if (TryGetDoubleParameter("fwf", subText, out value)) snowRecord.FreeWaterFraction = value;
            yield return snowRecord;
        }

        protected override void ReportParseError(string label, string record)
        {
            log.ErrorFormat("Failed to parse parameter {0} from HBV snow record: {1}", label, record);
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "snow";
        }
    }
}
