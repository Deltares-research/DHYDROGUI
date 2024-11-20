using System;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;

namespace DeltaShell.Plugins.FMSuite.Common.Tests
{
    public static class StructuresTestHelper
    {
        public static void AddProperty(this StructureDAO structureDataAccessObject, string attributeName, Type type, string value)
        {
            structureDataAccessObject.Properties.Add(new StructureProperty(new StructurePropertyDefinition
            {
                FilePropertyKey = attributeName,
                DataType = type
            }, value));
        }
    }
}