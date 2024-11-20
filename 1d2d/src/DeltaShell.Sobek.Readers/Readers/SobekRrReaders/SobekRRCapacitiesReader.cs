using System.Collections.Generic;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRCapacitiesReader: SobekReader<SobekRRCapacities>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRCapacitiesReader));

        public override IEnumerable<SobekRRCapacities> Parse(string text)
        {
            var sobekCapacity = new SobekRRCapacities();

            //CAPC id ’Capacities&Contents’ uztwm 50 uztwc 50 uzfwm 150 uzfwc 100 lztwm 150 lztwc
            // 150 lzfsm 200 lzfsc 100 lzfpm 150 lzfpc 150 uzk .08 lzsk .05 lzpk .003 capc   

            const string pattern = @"CAPS\s+?" + IdAndOptionalNamePattern + @"(?<text>.*?)\s+caps" + RegularExpression.EndOfLine;
            var matches = RegularExpression.GetMatches(pattern, text);
            if (matches.Count != 1)
            {
                log.ErrorFormat("Could not parse Sacramento capacity record: {0}", text);
                yield break;
            }

            sobekCapacity.Id = matches[0].Groups["id"].Value;
            var subText = matches[0].Groups["text"].Value;

            double value;
            if (TryGetDoubleParameter("uztwm", subText, out value)) sobekCapacity.UpperZoneTensionWaterStorageCapacity = value;
            if (TryGetDoubleParameter("uztwc", subText, out value)) sobekCapacity.UpperZoneTensionWaterInitialContent = value;
            if (TryGetDoubleParameter("uzfwm", subText, out value)) sobekCapacity.UpperZoneFreeWaterStorageCapacity = value;
            if (TryGetDoubleParameter("uzfwc", subText, out value)) sobekCapacity.UpperZoneFreeWaterInitialContent = value;
            if (TryGetDoubleParameter("lztwm", subText, out value)) sobekCapacity.LowerZoneTensionWaterStorageCapacity = value;
            if (TryGetDoubleParameter("lztwc", subText, out value)) sobekCapacity.LowerZoneTensionWaterInitialContent = value;
            if (TryGetDoubleParameter("lzfsm", subText, out value)) sobekCapacity.LowerZoneSupplementalFreeWaterStorageCapacity = value;
            if (TryGetDoubleParameter("lzfsc", subText, out value)) sobekCapacity.LowerZoneSupplementalFreeWaterInitialContent = value;
            if (TryGetDoubleParameter("lzfpm", subText, out value)) sobekCapacity.LowerZonePrimaryFreeWaterStorageCapacity = value;
            if (TryGetDoubleParameter("lzfpc", subText, out value)) sobekCapacity.LowerZonePrimaryFreeWaterInitialContent = value;
            if (TryGetDoubleParameter("uzk", subText, out value)) sobekCapacity.UpperZoneFreeWaterDrainageRate = value;
            if (TryGetDoubleParameter("lzsk", subText, out value)) sobekCapacity.LowerZoneSupplementalFreeWaterDrainageRate = value;
            if (TryGetDoubleParameter("lzpk", subText, out value)) sobekCapacity.LowerZonePrimaryFreeWaterDrainageRate = value;

            yield return sobekCapacity;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "caps";
        }

        protected override void ReportParseError(string label, string record)
        {
            log.ErrorFormat("Failed to parse parameter {0} from capacity record; {1}", label, record);
        }
    }
}
