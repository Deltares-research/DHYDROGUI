using System.Collections.Generic;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRSacramentoReader : SobekReader<SobekRRSacramento>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRSacramentoReader));

        public override IEnumerable<SobekRRSacramento> Parse(string text)
        {
            var sacramentoData = new SobekRRSacramento();

            // SACR id ’5’ ar 319000000 ms ’1’ ca ’Capacities&Contents’ uh ’UnitHydrograph’ op ’OtherParameters’ sacr
            string stringValue;
            if (TryGetStringParameter("id", text, out stringValue)) sacramentoData.Id = stringValue;
            if (TryGetStringParameter("nm", text, out stringValue, false)) sacramentoData.Name= stringValue;
            if (TryGetStringParameter("ms", text, out stringValue)) sacramentoData.MeteoStationId= stringValue;
            if (TryGetStringParameter("ca", text, out stringValue)) sacramentoData.CapacityId= stringValue;
            if (TryGetStringParameter("uh", text, out stringValue)) sacramentoData.UnitHydrographId= stringValue;
            if (TryGetStringParameter("op", text, out stringValue)) sacramentoData.OtherParamsId= stringValue;
            
            double numberValue;
            if (TryGetDoubleParameter("ar", text, out numberValue)) sacramentoData.Area = numberValue;
            if (TryGetDoubleParameter("aaf", text, out numberValue, false)) sacramentoData.AreaAdjustmentFactor = numberValue;

            yield return sacramentoData;
        }

        protected override void ReportParseError(string label, string record)
        {
            log.ErrorFormat("Could not parse parameter {0} from Sacramento record: {1}", label, record);
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "sacr";
        }
    }
}
