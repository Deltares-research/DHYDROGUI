using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class StructureFMPropertiesFile : StructureSchemaCsvFile
    {
        public StructureSchema<ModelPropertyDefinition> ReadProperties(string propertiesDefinitionFile)
        {
            var schema = ReadStructureSchema(propertiesDefinitionFile);

            StructurePropertyDefinition.StructurePropertyGroups.Clear();
            foreach (var group in schema.StructurePropertyGroups)
            {
                StructurePropertyDefinition.StructurePropertyGroups.Add(group.Key, group.Value);
            }

            return schema;
        }
    }
}