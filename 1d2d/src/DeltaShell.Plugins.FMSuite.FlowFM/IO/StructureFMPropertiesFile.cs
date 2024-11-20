using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class StructureFMPropertiesFile : StructureSchemaCsvFile
    {
        public Dictionary<string, ModelPropertyGroup> StructurePropertyGroups { get; } = new Dictionary<string, ModelPropertyGroup>();

        public StructureSchema<ModelPropertyDefinition> ReadProperties(string propertiesDefinitionFile)
        {
            var schema = ReadStructureSchema(propertiesDefinitionFile);

            StructurePropertyGroups.Clear();
            foreach (var group in schema.StructurePropertyGroups)
            {
                StructurePropertyGroups.Add(group.Key, group.Value);
            }

            return schema;
        }
    }
}