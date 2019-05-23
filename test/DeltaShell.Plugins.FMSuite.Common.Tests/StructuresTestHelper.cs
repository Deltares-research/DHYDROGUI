using System;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;

namespace DeltaShell.Plugins.FMSuite.Common.Tests
{
    public static class StructuresTestHelper
    {
        public static void AddProperty(this Structure2D structure, string attributeName, Type type, string value)
        {
            structure.Properties.Add(new StructureProperty(new StructurePropertyDefinition
            {
                FilePropertyName = attributeName,
                DataType = type
            }, value));
        }
    }
}