using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures
{
    public class StructureSchema<TDefinition> where TDefinition : ModelPropertyDefinition
    {
        public StructureSchema()
        {
            StructurePropertyGroups = new Dictionary<string, ModelPropertyGroup>();
        }

        public IDictionary<string, ModelPropertyGroup> StructurePropertyGroups { get; private set; }

        /// <summary>
        /// Retrieves the property definition from the schema.
        /// </summary>
        /// <param name="structureType"> The type-specifier of structure. </param>
        /// <param name="name"> Name of the property found as key in a structure file. </param>
        /// <returns>
        /// The property definition corresponding to <paramref name="name"/>;
        /// Null if <paramref name="structureType"/> is unknown or <paramref name="name"/>
        /// is not among the schema properties.
        /// </returns>
        public ModelPropertyDefinition GetDefinition(string structureType, string name)
        {
            if (!StructurePropertyGroups.ContainsKey(structureType))
            {
                return null;
            }

            ModelPropertyGroup group = StructurePropertyGroups[structureType];
            return group.PropertyDefinitions.Concat(StructurePropertyGroups["structure"].PropertyDefinitions)
                        .FirstOrDefault(p => p.FilePropertyKey == name);
        }
    }
}