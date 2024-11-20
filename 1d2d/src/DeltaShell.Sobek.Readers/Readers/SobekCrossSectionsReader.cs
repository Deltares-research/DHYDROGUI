using System.Collections.Generic;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekCrossSectionsReader : SobekReader<SobekBranchLocation>
    {
        public override IEnumerable<SobekBranchLocation> Parse(string text)
        {
            const string boundaryLocationsPattern = @"(CRSN (?'text'.*?) crsn)";

            foreach (Match match in RegularExpression.GetMatches(boundaryLocationsPattern, text))
            {
                var sobekBoundaryLocation = GetCrossSectionLocation(match.Value);
                if (sobekBoundaryLocation != null)
                {
                    yield return sobekBoundaryLocation;
                }
            }
        }

        public static SobekBranchLocation GetCrossSectionLocation(string value)
        {
            var location = new SobekBranchLocation();

            // CRSN id 'c1' nm 'crossdef1' ci 10 lc 250.6 crsn
            var pattern =
                RegularExpression.GetExtendedCharacters("id") + "|" +
                RegularExpression.GetExtendedCharacters("nm") + "|" +
                RegularExpression.GetExtendedCharacters("ci") + "|" +
                RegularExpression.GetScientific("lc");

            foreach (Match match in RegularExpression.GetMatches(pattern, value))
            {
                location.ID = RegularExpression.ParseString(match, "id", location.ID);
                location.Name = RegularExpression.ParseString(match, "nm", location.Name);
                location.BranchID = RegularExpression.ParseString(match, "ci", location.BranchID);
                location.Offset = RegularExpression.ParseDouble(match, "lc", location.Offset);
            }
            return location;
        }
    }
}