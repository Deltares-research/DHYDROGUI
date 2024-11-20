using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekNetworkLinkageNodeFileReader : SobekReader<SobekLinkageNode>
    {
        public override IEnumerable<SobekLinkageNode> Parse(string fileContent)
        {
            const string pattern = @"(NDLK (?'text'.*?) ndlk)";

            var matches = RegularExpression.GetMatches(pattern, fileContent);

            foreach (Match match in matches)
            {
                var sobekLinkageNode = GetLinkageNode(match.Value);
                if (sobekLinkageNode != null)
                {
                    yield return sobekLinkageNode;
                }
            }
        }

        private static SobekLinkageNode GetLinkageNode(string line)
        {
            var sobekLinkageNode = new SobekLinkageNode();

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekLinkageNode.ID = matches[0].Groups[label].Value;
            }

            //BrancheID
            label = "ci";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekLinkageNode.BranchID = matches[0].Groups[label].Value;
            }

            //Chainage/ReachLocation
            label = "lc";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekLinkageNode.ReachLocation = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //X
            label = "px";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekLinkageNode.X = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Y
            label = "py";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekLinkageNode.Y = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            return sobekLinkageNode;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "ndlk";
        }
    }
}