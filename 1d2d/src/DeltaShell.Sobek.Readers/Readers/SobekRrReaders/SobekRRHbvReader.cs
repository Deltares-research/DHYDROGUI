using System.Collections.Generic;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRHbvReader: SobekReader<SobekRRHbv>
    {
        public override IEnumerable<SobekRRHbv> Parse(string text)
        {
            var hbvData = new SobekRRHbv();

            // HBV id '2' nm '2' ar 1000000 sl 20 snow '##1' soil '##1' flow '##1' hini '##1' ms 'De Bilt' aaf 1 ts 'De Bilt' hbv

            string stringValue;
            if (TryGetStringParameter("id", text, out stringValue)) hbvData.Id = stringValue;
            if (TryGetStringParameter("nm", text, out stringValue, false)) hbvData.Name = stringValue;
            if (TryGetStringParameter("snow", text, out stringValue)) hbvData.SnowId = stringValue;
            if (TryGetStringParameter("soil", text, out stringValue)) hbvData.SoilId = stringValue;
            if (TryGetStringParameter("flow", text, out stringValue)) hbvData.FlowId = stringValue;
            if (TryGetStringParameter("hini", text, out stringValue)) hbvData.HiniId = stringValue;
            if (TryGetStringParameter("ms", text, out stringValue)) hbvData.MeteoStationId = stringValue;
            if (TryGetStringParameter("ts", text, out stringValue)) hbvData.TemperatureStationId = stringValue;

            double numberValue;
            if (TryGetDoubleParameter("ar", text, out numberValue)) hbvData.Area = numberValue;
            if (TryGetDoubleParameter("sl", text, out numberValue)) hbvData.SurfaceLevel = numberValue;
            if (TryGetDoubleParameter("aaf", text, out numberValue, false)) hbvData.AreaAdjustmentFactor = numberValue;
            
            yield return hbvData;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "hbv";
        }
    }
}
