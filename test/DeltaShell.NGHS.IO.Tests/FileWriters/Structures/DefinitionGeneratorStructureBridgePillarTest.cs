using System.Linq;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructureBridgePillarTest : 
        DefinitionGeneratorStructureBaseTestFixture<DefinitionGeneratorStructureBridgePillar>
    {
        protected override DefinitionGeneratorStructureBridgePillar CreateGenerator()
        {
            return new DefinitionGeneratorStructureBridgePillar();
        }

        protected override string TStructureDefinitionType => "bridge";


        private const string expectedBranchName = "branch-name";
        private const double inputChainage = 5.5;
        private const double snappedChainage = 5;
        private const string chainageStr = "5.000000";
        private const string expectedLongName = "This bridge is located at rotterdam habour."; 

        private static Bridge CreatePillarBridge() =>
            new Bridge(nameof(Bridge))
            {
                Branch = CreateBranchMock(expectedBranchName, inputChainage, snappedChainage),
                Chainage = inputChainage,
                LongName = expectedLongName,
            };

        [Test]
        public void CreateStructureRegion_BridgePillar_ExpectedResult()
        {
            Bridge structure = CreatePillarBridge();
            IniSection iniSection = Generator.CreateStructureRegion(structure);

            Assert.That(iniSection.Properties.Count(), Is.EqualTo(13));
            AssertCorrectCommonRegionElements(iniSection,
                                              nameof(Bridge),
                                              expectedLongName,
                                              expectedBranchName,
                                              chainageStr,
                                              TStructureDefinitionType);

            AssertCorrectProperty(iniSection, StructureRegion.PillarWidth.Key, "3.000");
            AssertCorrectProperty(iniSection, StructureRegion.FormFactor.Key, "1.500");
            
        }
    }
}