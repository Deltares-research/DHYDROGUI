using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekObjectTypeReader : SobekReader<SobekObjectTypeData>
    {
        public override IEnumerable<SobekObjectTypeData> Parse(string text)
        {
            const string boundaryLocationsPattern = @"(OBID (?'text'.*?) obid)";

            foreach (Match match in RegularExpression.GetMatches(boundaryLocationsPattern, text))
            {
                var sobekBoundaryLocation = GetSobekObjectTypeData(match.Value);
                if (sobekBoundaryLocation != null)
                {
                    yield return sobekBoundaryLocation;
                }
            }
        }

        public static SobekObjectTypeData GetSobekObjectTypeData(string value)
        {
            var objectTypeData = new SobekObjectTypeData();
            var type = "";

            // OBID id '339' ci 'SBK_GRIDPOINTFIXED' obid
            var pattern =
                RegularExpression.GetExtendedCharacters("id") + "|" +
                RegularExpression.GetExtendedCharacters("ci");

            foreach (Match match in RegularExpression.GetMatches(pattern, value))
            {
                objectTypeData.ID = RegularExpression.ParseString(match, "id", objectTypeData.ID);
                type = RegularExpression.ParseString(match, "ci", type);
            }

            SobekObjectType z;
            if (Enum.TryParse(type, out z))
            {
                objectTypeData.Type = z;
                return objectTypeData;
            }
            return null;
        }
    }
}