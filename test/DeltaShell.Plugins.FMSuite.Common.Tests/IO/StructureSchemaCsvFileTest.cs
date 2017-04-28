using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class StructureSchemaCsvFileTest
    {
        public static readonly string ApplicationStructuresSchemaCsvFilePath = @"plugins\DeltaShell.Plugins.FMSuite.FlowFM\structure-properties.csv";

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadApplicationCsvFile()
        {
            var schema = new StructureSchemaCsvFile().ReadStructureSchema(ApplicationStructuresSchemaCsvFilePath);

            Assert.AreEqual(4, schema.StructurePropertyGroups.Count);
            Assert.AreEqual(5, schema.StructurePropertyGroups["structure"].PropertyDefinitions.Count);
            Assert.AreEqual(3, schema.StructurePropertyGroups["weir"].PropertyDefinitions.Count);
            Assert.AreEqual(1, schema.StructurePropertyGroups["pump"].PropertyDefinitions.Count);
            Assert.AreEqual(6, schema.StructurePropertyGroups["gate"].PropertyDefinitions.Count);
        }
    }
}