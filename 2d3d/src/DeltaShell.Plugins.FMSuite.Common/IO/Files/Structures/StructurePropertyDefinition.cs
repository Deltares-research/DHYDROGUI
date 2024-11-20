using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures
{
    public sealed class StructurePropertyDefinition : ModelPropertyDefinition
    {
        public static Dictionary<string, ModelPropertyGroup> StructurePropertyGroups { get; } = new Dictionary<string, ModelPropertyGroup>();
    }
}