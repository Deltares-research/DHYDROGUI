using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class StructureSchemaCsvFileTest
    {
        public static readonly string ApplicationStructuresSchemaCsvFilePath = Path.Combine("plugins",
                                                                                            "DeltaShell.Plugins.FMSuite.FlowFM",
                                                                                            "CsvFiles",
                                                                                            "structure-properties.csv");

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadApplicationCsvFile()
        {
            var schema = new StructureSchemaCsvFile().ReadStructureSchema(ApplicationStructuresSchemaCsvFilePath);

            Assert.AreEqual(5, schema.StructurePropertyGroups.Count);
            Assert.AreEqual(5, schema.StructurePropertyGroups["structure"].PropertyDefinitions.Count);
            Assert.AreEqual(3, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.Weir].PropertyDefinitions.Count);
            Assert.AreEqual(1, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.Pump].PropertyDefinitions.Count);
            Assert.AreEqual(6, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.Gate].PropertyDefinitions.Count);
            Assert.AreEqual(25, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.GeneralStructure].PropertyDefinitions.Count);
        }
    }
}