using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
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
            StructureSchema<ModelPropertyDefinition> schema = new StructureSchemaCsvFile().ReadStructureSchema(ApplicationStructuresSchemaCsvFilePath);

            Assert.AreEqual(5, schema.StructurePropertyGroups.Count);
            Assert.AreEqual(5, schema.StructurePropertyGroups["structure"].PropertyDefinitions.Count);
            Assert.AreEqual(3, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.Weir].PropertyDefinitions.Count);
            Assert.AreEqual(1, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.Pump].PropertyDefinitions.Count);
            Assert.AreEqual(6, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.Gate].PropertyDefinitions.Count);
            Assert.AreEqual(25, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.GeneralStructure].PropertyDefinitions.Count);
        }
    }
}