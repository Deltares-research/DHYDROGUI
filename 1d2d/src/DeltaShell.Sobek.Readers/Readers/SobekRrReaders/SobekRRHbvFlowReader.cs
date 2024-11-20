using System.Collections.Generic;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRHbvFlowReader : SobekReader<SobekRRHbvFlow>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRHbvFlowReader));

        public override IEnumerable<SobekRRHbvFlow> Parse(string text)
        {
            var flowRecord = new SobekRRHbvFlow();

            //FLOW id '##2' nm 'RESERVOIR2' kb 0.1 ki 0.1 kq 0.1 qt 50 mp 1 flow
            const string pattern = @"FLOW\s+?" + IdAndOptionalNamePattern + @"(?<text>.*?)\s+flow" + RegularExpression.EndOfLine;
            var matches = RegularExpression.GetMatches(pattern, text);
            if (matches.Count != 1)
            {
                log.ErrorFormat("Could not parse HBV flow record: {0}", text);
                yield break;
            }

            flowRecord.Id = matches[0].Groups["id"].Value;
            var subText = matches[0].Groups["text"].Value;

            double value;
            if (TryGetDoubleParameter("kb", subText, out value)) flowRecord.BaseFlowReservoirConstant = value;
            if (TryGetDoubleParameter("ki", subText, out value)) flowRecord.InterflowReservoirConstant = value;
            if (TryGetDoubleParameter("kq", subText, out value)) flowRecord.QuickFlowReservoirConstant = value;
            if (TryGetDoubleParameter("qt", subText, out value)) flowRecord.UpperZoneThreshold = value;
            if (TryGetDoubleParameter("mp", subText, out value)) flowRecord.MaximumPercolation = value;

            yield return flowRecord;
        }

        protected override void ReportParseError(string label, string record)
        {
            log.ErrorFormat("Failed to parse parameter {0} from HBV flow record: {1}", label, record);
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "flow";
        }
    }
}
