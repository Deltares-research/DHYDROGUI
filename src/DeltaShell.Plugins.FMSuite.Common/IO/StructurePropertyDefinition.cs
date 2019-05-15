using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public sealed class StructurePropertyDefinition : ModelPropertyDefinition
    {
        public static readonly Dictionary<string, ModelPropertyGroup> StructurePropertyGroups =
            new Dictionary<string, ModelPropertyGroup>();
    }
}