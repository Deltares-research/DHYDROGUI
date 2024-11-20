using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekNetworkBranchFileReader : SobekReader<SobekBranch>
    {
        public override IEnumerable<SobekBranch> Parse(string fileContent)
        {
            const string pattern = @"(BRCH (?'text'.*?) brch)";

            var matches = RegularExpression.GetMatches(pattern, fileContent);

            foreach (Match match in matches)
            {
                var sobekBranch = GetBranch(match.Value);
                if (sobekBranch != null)
                {
                    yield return sobekBranch;
                }
            }
        }

        private static SobekBranch GetBranch(string line)
        {
            var sobekBranch = new SobekBranch();

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                var id = matches[0].Groups[label].Value;
                
                sobekBranch.TextID = id;
            }

            //Name
            label = "nm";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                var name = matches[0].Groups[label].Value;

                sobekBranch.Name = name;
            }

            //StartNode
            label = "bn";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                var startNodeId = matches[0].Groups[label].Value;

                sobekBranch.StartNodeID = startNodeId;
            }

            //EndNode
            label = "en";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                var endNodeId = matches[0].Groups[label].Value;
                sobekBranch.EndNodeID = endNodeId;
            }

            //Length
            label = "al";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekBranch.Length = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            return sobekBranch;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "brch";
        }
    }
}
