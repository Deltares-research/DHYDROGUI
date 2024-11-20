using System.Collections.Generic;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{

    public class SobekRRHbvSoilReader : SobekReader<SobekRRHbvSoil>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRHbvSoilReader));
        public override IEnumerable<SobekRRHbvSoil> Parse(string text)
        {
            var soilRecord = new SobekRRHbvSoil();

            // SOIL id '##1' nm 'SOIL1' be 3.5 fc 200 ef 0.75 soil
            const string pattern = @"SOIL\s+?" + IdAndOptionalNamePattern + @"(?<text>.*?)\s+soil" + RegularExpression.EndOfLine;
            var matches = RegularExpression.GetMatches(pattern, text);
            if (matches.Count != 1)
            {
                log.ErrorFormat("Could not parse HBV soil record: {0}", text);
                yield break;
            }

            soilRecord.Id = matches[0].Groups["id"].Value;
            var subText = matches[0].Groups["text"].Value;

            double value;
            if (TryGetDoubleParameter("be", subText, out value)) soilRecord.Beta = value;
            if (TryGetDoubleParameter("fc", subText, out value)) soilRecord.FieldCapacity = value;
            if (TryGetDoubleParameter("ef", subText, out value)) soilRecord.FieldCapacityThreshold = value;
            
            yield return soilRecord;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "soil";
        }

        protected override void ReportParseError(string label, string record)
        {
            log.ErrorFormat("Failed to parse parameter {0} from HBV soil record: {1}", label, record);
        }
    }
}
