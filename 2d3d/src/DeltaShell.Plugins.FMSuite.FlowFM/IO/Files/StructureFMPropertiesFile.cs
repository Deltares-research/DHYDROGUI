using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public class StructureFMPropertiesFile : StructureSchemaCsvFile
    {
        public StructureSchema<ModelPropertyDefinition> ReadProperties(string propertiesDefinitionFile)
        {
            StructureSchema<ModelPropertyDefinition> schema = ReadStructureSchema(propertiesDefinitionFile);

            StructurePropertyDefinition.StructurePropertyGroups.Clear();
            foreach (KeyValuePair<string, ModelPropertyGroup> group in schema.StructurePropertyGroups)
            {
                StructurePropertyDefinition.StructurePropertyGroups.Add(group.Key, group.Value);
            }

            return schema;
        }
    }
}