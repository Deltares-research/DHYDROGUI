using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileWriters.Structure;
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

            Assert.AreEqual(11, schema.StructurePropertyGroups.Count);
            Assert.AreEqual(9, schema.StructurePropertyGroups["structure"].PropertyDefinitions.Count);
            Assert.AreEqual(5, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.Weir].PropertyDefinitions.Count);
            Assert.AreEqual(12, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.Pump].PropertyDefinitions.Count);
            Assert.AreEqual(7, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.Gate].PropertyDefinitions.Count);
            Assert.AreEqual(28, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.GeneralStructure].PropertyDefinitions.Count);
            Assert.AreEqual(17, schema.StructurePropertyGroups[StructureRegion.StructureTypeName.LeveeBreach].PropertyDefinitions.Count);
        }
    }
}