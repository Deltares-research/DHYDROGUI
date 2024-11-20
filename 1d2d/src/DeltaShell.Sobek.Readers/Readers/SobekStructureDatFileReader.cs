using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekStructureDatFileReader : SobekReader<SobekStructureMapping>
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(SobekStructureDatFileReader));

        public override IEnumerable<SobekStructureMapping> Parse(string datFileText)
        {
            const string structurePattern = @"STRU\sid\s'(?<Id>" + RegularExpression.Characters + 
                @")('\snm\s'(?<Name>" + RegularExpression.ExtendedCharacters +
                @"))?'\sdd\s'(?<DefinitionId>" + RegularExpression.ExtendedCharacters +
                @")(?<OptionalData>(?'text'.*?))\sstru";

            foreach (Match structureMatch in RegularExpression.GetMatches(structurePattern, datFileText))
            {
                SobekStructureMapping mapping = GetStructure(structureMatch);
                if (mapping != null)
                    yield return mapping;
            }
        }

        private static SobekStructureMapping GetStructure(Match match)
        {
            try
            {
                SobekStructureMapping sobekStructureMapping = new SobekStructureMapping();

                sobekStructureMapping.StructureId = match.Groups["Id"].Value.Replace("##", "~~");
                sobekStructureMapping.Name = match.Groups["Name"].Value;
                sobekStructureMapping.DefinitionId = match.Groups["DefinitionId"].Value;
                var optionalData = match.Groups["OptionalData"].Value;

                sobekStructureMapping.ControllerIDs = GetControllerIDs(optionalData);

                return sobekStructureMapping;
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Error parsing structure data {0}, reason {1}", match.Value, exception.Message);
                return null;
            }
        }

        private static IList<string> GetControllerIDs(string optionalData)
        {
            const string controllersPattern = @"ca\s((?<a1>" + RegularExpression.Integer + 
            @"))?\s*((?<a2>" + RegularExpression.Integer + 
            @"))?\s*((?<a3>" + RegularExpression.Integer + 
            @"))?\s*((?<a4>" + RegularExpression.Integer + 
            @"))?\scj\s('(?<c1>" + RegularExpression.Characters + 
            @")')?\s*('(?<c2>" + RegularExpression.Characters + 
            @")')?\s*('(?<c3>" + RegularExpression.Characters + 
            @")')?\s*('(?<c4>" + RegularExpression.Characters + @")')?";
            var controllerIDsList = new List<string>();

            var matches = RegularExpression.GetMatches(controllersPattern, optionalData);

            if(matches.Count == 1)
            {
                var a1 = matches[0].Groups["a1"].Value;
                var a2 = matches[0].Groups["a2"].Value;
                var a3 = matches[0].Groups["a3"].Value;
                var a4 = matches[0].Groups["a4"].Value;

                var c1 = matches[0].Groups["c1"].Value;
                var c2 = matches[0].Groups["c2"].Value;
                var c3 = matches[0].Groups["c3"].Value;
                var c4 = matches[0].Groups["c4"].Value;

                AddValidControllerID(c1, a1, controllerIDsList);
                AddValidControllerID(c2, a2, controllerIDsList);
                AddValidControllerID(c3, a3, controllerIDsList);
                AddValidControllerID(c4, a4, controllerIDsList);
            }

            return controllerIDsList.Count > 0 ? controllerIDsList : null;
        }

        private static void AddValidControllerID(string id, string active, IList<string> controllerIDsList)
        {
            if (!string.IsNullOrEmpty(id) && id != "-1" && active == "1")
            {
                controllerIDsList.Add("CTR_" + id);
            }
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "stru";
        }
    }
}
