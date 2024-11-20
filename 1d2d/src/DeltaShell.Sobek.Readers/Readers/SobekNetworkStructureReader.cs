using System.Collections.Generic;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekNetworkStructureReader : SobekReader<SobekStructureLocation>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekNetworkStructureReader));

        public override IEnumerable<SobekStructureLocation> Parse(string text)
        {
            const string boundaryLocationsPattern = @"(STRU (?'text'.*?) stru)|(STCM (?'text'.*?)\sstcm)";

            foreach (Match match in RegularExpression.GetMatches(boundaryLocationsPattern, text))
            {
                var sobekBoundaryLocation = GetStructureLocation(match.Value);
                if (sobekBoundaryLocation != null)
                {
                    yield return sobekBoundaryLocation;
                }
            }
        }

        public static SobekStructureLocation GetStructureLocation(string record)
        {
            var sobekBranchLocation = new SobekStructureLocation();
            if (record.StartsWith("STRU"))
            {
                sobekBranchLocation.IsCompound = false;
            }
            else if (record.StartsWith("STCM"))
            {
                sobekBranchLocation.IsCompound = true;
            }
            else
            {
                Log.WarnFormat("Structure location record {0} unsupported skipped.", record);
                return null;
            }

            // example: @"STRU id '13' nm 'steelcun' ci '1' lc 18270.969411203 stru";

            string pattern =
                RegularExpression.GetExtendedCharacters("id") + "|" +
                RegularExpression.GetExtendedCharacters("nm") + "|" +
                RegularExpression.GetExtendedCharacters("ci") + "|" +
                RegularExpression.GetScientific("lc");

            foreach (Match match in RegularExpression.GetMatches(pattern, record))
            {
                sobekBranchLocation.ID = RegularExpression.ParseString(match, "id", sobekBranchLocation.ID);
                sobekBranchLocation.BranchID = RegularExpression.ParseString(match, "ci", sobekBranchLocation.BranchID);
                sobekBranchLocation.Name = RegularExpression.ParseString(match, "nm", sobekBranchLocation.Name);
                sobekBranchLocation.Offset = RegularExpression.ParseDouble(match, "lc", sobekBranchLocation.Offset);
            }
            return sobekBranchLocation;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "stru";
            yield return "stcm";
        }
    }
}
