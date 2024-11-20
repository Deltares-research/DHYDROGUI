using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    /// <summary>
    /// Read nodes from NODES.DAT
    /// </summary>
    public class SobekNodeFileReader : SobekReader<SobekNode>
    {
        public override IEnumerable<SobekNode> Parse(string fileContent)
        {
            const string pattern = @"(NODE (?'text'.*?) node)";

            var matches = RegularExpression.GetMatches(pattern, fileContent);

            foreach (Match match in matches)
            {
                var sobekNode = GetNode(match.Value);
                if (sobekNode != null)
                {
                    yield return sobekNode;
                }
            }
        }

        private static SobekNode GetNode(string line)
        {
            var sobekNode = new SobekNode();

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekNode.ID = matches[0].Groups[label].Value;
            }

            //Interpolation over node active
            label = "ni";
            pattern = RegularExpression.GetInteger(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekNode.InterpolationOverNode = (Convert.ToInt32(matches[0].Groups[label].Value, CultureInfo.InvariantCulture) == 1);
            }

            //Interpolation from reach
            label = "r1";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekNode.InterpolationFrom = matches[0].Groups[label].Value;
            }

            //Interpolation to reach
            label = "r2";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekNode.InterpolationTo = matches[0].Groups[label].Value;
            }

            return sobekNode;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "node";
        }
    }
}
