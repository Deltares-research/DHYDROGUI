using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class StructureFMPropertiesFileTest
    {
        [Test]
        public void ReadPropertiesTest()
        {
            // Structure2DType,   attributeName,  Caption,            Type,       Default,    Min,    Max,    StructureFileOnly,  Description
            // structure,       id,             Name,               String,     ,           ,       ,       FALSE,              Name of the structure
            // weir,            crest_level,    Crest level,        Double,     ,           ,       ,       FALSE,              Crest height in [m]
            // pump,            nrstages,       Number of stages,   Integer,    1,          1,      ,       FALSE,              Number of pump stages
            // gate,            sill_level,     Sill level,         Double,     ,           ,       ,       FALSE,              Sill level in [m]

            var filePath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(WaterFlowFMModelTest).Assembly,
                                                      @"structures\structure-properties_TEST.csv");
            var structFile = new StructureFMPropertiesFile();
            structFile.ReadProperties(filePath);

            Assert.AreEqual(4, structFile.StructurePropertyGroups.Count);

            var supportedStructures = new[] {"structure", "weir", "gate", "pump"};
            foreach (var supportedStructure in supportedStructures)
            {
                Assert.Contains(supportedStructure, structFile.StructurePropertyGroups.Keys);
                Assert.AreEqual(1, structFile.StructurePropertyGroups[supportedStructure].PropertyDefinitions.Count);
            }
        }
    }
}