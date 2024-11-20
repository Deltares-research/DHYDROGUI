using System.Linq;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructureBridgeStandardTest : 
        DefinitionGeneratorStructureBaseTestFixture<DefinitionGeneratorStructureBridgeStandard>
    {
        protected override DefinitionGeneratorStructureBridgeStandard CreateGenerator()
        {
            return new DefinitionGeneratorStructureBridgeStandard();
        }

        protected override string TStructureDefinitionType => "bridge";


        private const string expectedBranchName = "branch-name";
        private const double inputChainage = 5.5;
        private const double snappedChainage = 5;
        private const string chainageStr = "5.000000";
        private const string expectedLongName = "This bridge is located at rotterdam maas."; 

        private static Bridge CreateBridge() =>
            new Bridge(nameof(Bridge))
            {
                Branch = CreateBranchMock(expectedBranchName, inputChainage, snappedChainage),
                Chainage = inputChainage,
                LongName = expectedLongName,
                Shift = 80.1
            };

        [Test]
        public void CreateStructureRegion_Bridge_ExpectedResult()
        {
            Bridge structure = CreateBridge();
            IniSection iniSection = Generator.CreateStructureRegion(structure);

            Assert.That(iniSection.Properties.Count(), Is.EqualTo(11));
            AssertCorrectCommonRegionElements(iniSection,
                                              nameof(Bridge),
                                              expectedLongName,
                                              expectedBranchName,
                                              chainageStr,
                                              TStructureDefinitionType);

            AssertCorrectProperty(iniSection, StructureRegion.CsDefId.Key, "Bridge");
            AssertCorrectProperty(iniSection, StructureRegion.Length.Key, "20.000");

            AssertCorrectProperty(iniSection, StructureRegion.AllowedFlowDir.Key, "both");
            AssertCorrectProperty(iniSection, StructureRegion.Shift.Key, "80.100");

            AssertCorrectProperty(iniSection, StructureRegion.InletLossCoeff.Key, "0.000");
            AssertCorrectProperty(iniSection, StructureRegion.OutletLossCoeff.Key, "0.000");
            
        }
    }
}