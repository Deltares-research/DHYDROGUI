using System.Collections.Generic;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRSacramentoParametersReader: SobekReader<SobekRRSacramentoParameters>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRSacramentoParametersReader));
        public override IEnumerable<SobekRRSacramentoParameters> Parse(string text)
        {
            var sobekRRSacramentoParameters = new SobekRRSacramentoParameters();

            // OPAR id 'OtherParameters' zperc 5.0 rexp 9.0 pfree 0.2 rserv 0.95 pctim 0 adimp 0.5 sarva
            // 0.0 side 0.0 ssout 0.0 pm 0.1 pt1 500 pt2 500 opar

            const string pattern = @"OPAR\s+?id\s+?'(?<id>.*?)'\s+(?<text>.*?)\s+opar" + RegularExpression.EndOfLine;
            var matches = RegularExpression.GetMatches(pattern, text);
            if (matches.Count != 1)
            {
                log.ErrorFormat("Could not parse Sacramento parameters text: {0}", text);
                yield break;
            }

            sobekRRSacramentoParameters.Id = matches[0].Groups["id"].Value;
            var subText = matches[0].Groups["text"].Value;

            double value;
            if (TryGetDoubleParameter("zperc", subText, out value)) sobekRRSacramentoParameters.PercolationIncrease = value;
            if (TryGetDoubleParameter("rexp", subText, out value)) sobekRRSacramentoParameters.PercolationExponent = value;
            if (TryGetDoubleParameter("pfree", subText, out value)) sobekRRSacramentoParameters.PercolatedWaterFraction = value;
            if (TryGetDoubleParameter("rserv", subText, out value)) sobekRRSacramentoParameters.FreeWaterFraction = value;
            if (TryGetDoubleParameter("pctim", subText, out value)) sobekRRSacramentoParameters.PermanentlyImperviousFraction = value;
            if (TryGetDoubleParameter("adimp", subText, out value)) sobekRRSacramentoParameters.RainfallImperviousFraction = value;
            if (TryGetDoubleParameter("sarva", subText, out value)) sobekRRSacramentoParameters.WaterAndVegetationAreaFraction = value;
            if (TryGetDoubleParameter("side", subText, out value)) sobekRRSacramentoParameters.RatioUnobservedToObservedBaseFlow = value;
            if (TryGetDoubleParameter("ssout", subText, out value)) sobekRRSacramentoParameters.SubSurfaceOutflow = value;
            if (TryGetDoubleParameter("pm", subText, out value)) sobekRRSacramentoParameters.TimeIntervalIncrement = value;
            if (TryGetDoubleParameter("pt1", subText, out value)) sobekRRSacramentoParameters.LowerRainfallThreshold = value;
            if (TryGetDoubleParameter("pt2", subText, out value)) sobekRRSacramentoParameters.UpperRainfallThreshold = value;

            yield return sobekRRSacramentoParameters;
        }

        protected override void ReportParseError(string label, string record)
        {
            log.ErrorFormat("Could not parse parameter {0} from Sacramento OPAR record: {1}", label, record);
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "opar";
        }
    }
}