using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekStructureDefFileReader : SobekReader<SobekStructureDefinition>
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(SobekStructureDefFileReader));

        private readonly IList<ISobekStructureReader> readers;

        public SobekStructureDefFileReader(SobekType sobekType)
        {
            readers = new List<ISobekStructureReader>
                          {
                              new SobekWeirReader(),
                              new SobekRiverWeirReader(),
                              new SobekRiverAdvancedWeirReader(),
                              new SobekUniversalWeirReader(),
                              new SobekOrificeReader(),
                              new SobekGeneralStructureReader(sobekType),
                              new SobekPumpReader(),
                              new SobekRiverPumpReader(),
                              new SobekBridgeReader(),
                              new SobekCulvertReader()
                          };
            // Sobek's Pump and RiverPump are both covered by SobekPump
        }

        public override IEnumerable<SobekStructureDefinition> Parse(string defFileText)
        {
            const string structurePattern =
                @"STDS\s(?'text'.*?)\sstds";
            foreach (Match structureMatch in RegularExpression.GetMatches(structurePattern, defFileText))
            {
                SobekStructureDefinition definition = GetStructure(structureMatch.Value);
                if (definition != null)
                    yield return definition;
            }
        }


        private SobekStructureDefinition GetStructure(string line)
        {
                var structureDefinition = new SobekStructureDefinition();

                //Id
                var label = "id";
                var pattern = RegularExpression.GetExtendedCharacters(label);
                var matches = RegularExpression.GetMatches(pattern, line);
                if (matches.Count == 1)
                {
                    structureDefinition.Id = matches[0].Groups[label].Value;
                }

                //Name
                label = "nm";
                pattern = RegularExpression.GetExtendedCharacters(label);
                matches = RegularExpression.GetMatches(pattern, line);
                if (matches.Count == 1)
                {
                    structureDefinition.Name = matches[0].Groups[label].Value;
                }

                //Type
                label = "ty";
                pattern = RegularExpression.GetInteger(label);
                matches = RegularExpression.GetMatches(pattern, line);
                var type = Convert.ToInt32(matches[0].Groups[label].Value);
                if (matches.Count == 1)
                {
                    structureDefinition.Type = type;
                }

                var reader = readers.FirstOrDefault(r => r.Type == type);
                //no reader found
                if (reader == null)
                {
                    Log.WarnFormat("Structure of type {0} not yet supported; skipping...", type);
                    return null;
                }

                label = "Structure";
                pattern = @"\sty\s" + RegularExpression.Integer + @"\s(?<Structure>" + RegularExpression.CharactersAndQuote + @"?)\s+stds";
                matches = RegularExpression.GetMatches(pattern, line);
                if (matches.Count == 1)
                {
                    var structure = reader.GetStructure(matches[0].Groups[label].Value);
                    if (structure == null)
                    {
                        Log.ErrorFormat("Could not parse structure definition info of {0}.", structureDefinition.Id);
                        return null;
                    }
                    structureDefinition.Definition = structure;
                }
                else
                {
                    Log.ErrorFormat("Could not parse structure definition info of {0}.", structureDefinition.Id);
                    return null;
                }

                return structureDefinition;

        }

        public override IEnumerable<string> GetTags()
        {
            yield return "stds";
        }
    }
}