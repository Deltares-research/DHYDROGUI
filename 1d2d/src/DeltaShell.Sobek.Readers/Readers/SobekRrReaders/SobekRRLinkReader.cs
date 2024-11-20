using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRLinkReader : SobekReader<SobekRRLink>
    {
        public override IEnumerable<SobekRRLink> Parse(string fileContent)
        {
            const string rrLinkPattern = @"BRCH(?'text'.*?)brch" + RegularExpression.EndOfLine;

            return (from Match rrLinkLine in RegularExpression.GetMatches(rrLinkPattern, fileContent)
                    select GetSobekRRLink(rrLinkLine.Value)).ToList();
        }

        private static SobekRRLink GetSobekRRLink(string line)
        {

            //id   =          link identification 
            //nm  =          name of the link
            //ri    =          reach identification
            //mt  =          model type 
            //bt   =          branch type
            //ObID=       Object identification
            //bn  =          identification of begin node (‘from’ node)
            //en   =          identification of end node (‘to’ node)

            //The model type, branch type en Object Id are not used by the RR-computational core, but are used by user-interface programs (Netter).

            var sobekRRLink = new SobekRRLink();

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRLink.Id = matches[0].Groups[label].Value;
            }

            //Name
            label = "nm";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRLink.Name = matches[0].Groups[label].Value;
            }

            //Reach Id
            label = "ri";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRLink.ReachId = matches[0].Groups[label].Value;
            }

            //Node From Id
            label = "bn";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRLink.NodeFromId = matches[0].Groups[label].Value;
            }

            //NodeToId
            label = "en";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRLink.NodeToId = matches[0].Groups[label].Value;
            }

            return sobekRRLink;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "brch";
        }
    }
}
