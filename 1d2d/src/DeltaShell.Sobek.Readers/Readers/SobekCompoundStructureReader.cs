using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekCompoundStructureReader : SobekReader<SobekCompoundStructure>
    {
        public override IEnumerable<SobekCompoundStructure> Parse(string text)
        {
            const string compoundstructuresPattern = @"(STCM(?'text'.*?)stcm)";

            foreach (Match match in RegularExpression.GetMatches(compoundstructuresPattern, text))
            {
                var sobekBoundaryLocation = GetCompoundStructure(match.Value);
                if (sobekBoundaryLocation != null)
                {
                    yield return sobekBoundaryLocation;
                }
            }
        }

        public static SobekCompoundStructure GetCompoundStructure(string record)
        {
            var sobekCompoundStructure = new SobekCompoundStructure();

            sobekCompoundStructure.Id = RegularExpression.ParseFieldAsString("id", record);

            const string listPattern = @"(DLST(?'text'.*?)dlst)";

            var matches = RegularExpression.GetMatches(listPattern, record);
            if (matches.Count != 1)
            {
                return null;
            }
            var ids = matches[0].Groups["text"].Value.Split(new[] { ' ', '\'', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var id in ids)
            {
                sobekCompoundStructure.Structures.Add(id.Replace("##", "~~"));
            }
            return sobekCompoundStructure;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "stcm";
        }
    }

    public class SobekCompoundStructure
    {
        public string Id { get; set;}
        public IList<string> Structures { get; set; }

        public SobekCompoundStructure()
        {
            Structures = new List<string>();
        }
    }
}
