using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class StructureFMPropertiesFileTest
    {
        [Test]
        public void ReadPropertiesTest()
        {
            // StructureType,   attributeName,  Caption,            Type,       Default,    Min,    Max,    StructureFileOnly,  Description
            // structure,       id,             Name,               String,     ,           ,       ,       FALSE,              Name of the structure
            // weir,            CrestLevel,     Crest level,        Double,     ,           ,       ,       FALSE,              Crest height in [m]
            // pump,            nrstages,       Number of stages,   Integer,    1,          1,      ,       FALSE,              Number of pump stages
            // gate,            CrestLevel,     Sill level,         Double,     ,           ,       ,       FALSE,              Sill level in [m]

            string filePath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(WaterFlowFMModelTest).Assembly,
                                                                             @"structures\structure-properties_TEST.csv");
            var structFile = new StructureFMPropertiesFile();
            structFile.ReadProperties(filePath);

            Assert.AreEqual(4, StructurePropertyDefinition.StructurePropertyGroups.Count);

            var supportedStructures = new[]
            {
                "structure",
                "weir",
                "gate",
                "pump"
            };
            foreach (string supportedStructure in supportedStructures)
            {
                Assert.Contains(supportedStructure, StructurePropertyDefinition.StructurePropertyGroups.Keys);
                Assert.AreEqual(1, StructurePropertyDefinition.StructurePropertyGroups[supportedStructure].PropertyDefinitions.Count);
            }
        }
    }
}