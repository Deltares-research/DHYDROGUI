using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructureCulvertTest : 
        DefinitionGeneratorStructureBaseTestFixture<DefinitionGeneratorStructureCulvert>
    {
        protected override DefinitionGeneratorStructureCulvert CreateGenerator()
        {
            return new DefinitionGeneratorStructureCulvert(StructureFileNameGeneratorSubstitute);
        }

        protected override string TStructureDefinitionType => "culvert";


        private const string expectedBranchName = "branch-name";
        private const double inputChainage = 5.5;
        private const double snappedChainage = 5;
        private const string chainageStr = "5.000000";
        private const string expectedLongName = "Every day we stray further from god's light."; 


        protected static TestCaseData CreateCreateStructureRegionTestData(Culvert structure, string expectedCrestLevel) =>
            new TestCaseData(structure, expectedCrestLevel);

        private static Culvert CreateCulvert(bool useGateInitialOpening) =>
            new Culvert(nameof(Culvert))
            {
                UseGateInitialOpeningTimeSeries = useGateInitialOpening,
                Branch = CreateBranchMock(expectedBranchName, inputChainage, snappedChainage),
                Chainage = inputChainage,
                LongName = expectedLongName
            };

        private static IEnumerable<TestCaseData> CreateStructureRegionData()
        {
            yield return CreateCreateStructureRegionTestData(
                    CreateCulvert(true), ExpectedStructureFileName)
                .SetName("With TimeSeries");
            yield return CreateCreateStructureRegionTestData(
                    CreateCulvert(false), "1.000")
                .SetName("Without TimeSeries");
        }

        [Test]
        [TestCaseSource(nameof(CreateStructureRegionData))]
        public void CreateStructureRegion_TStructure_ExpectedResult(Culvert structure,
                                                                    string expectedGateInitialOpening)
        {
            IniSection iniSection = Generator.CreateStructureRegion(structure);

            Assert.That(iniSection.Properties.Count(), Is.EqualTo(14));
            AssertCorrectCommonRegionElements(iniSection,
                                              nameof(Culvert),
                                              expectedLongName,
                                              expectedBranchName,
                                              chainageStr,
                                              TStructureDefinitionType);
            AssertCorrectProperty(iniSection, StructureRegion.AllowedFlowDir.Key, "both");
            AssertCorrectProperty(iniSection, StructureRegion.LeftLevel.Key, "0.000");
            AssertCorrectProperty(iniSection, StructureRegion.RightLevel.Key, "0.000");
            AssertCorrectProperty(iniSection, StructureRegion.CsDefId.Key, "Culvert");
            AssertCorrectProperty(iniSection, StructureRegion.Length.Key, "0.000");
            AssertCorrectProperty(iniSection, StructureRegion.InletLossCoeff.Key, "0.000");
            AssertCorrectProperty(iniSection, StructureRegion.OutletLossCoeff.Key, "0.000");
            AssertCorrectProperty(iniSection, StructureRegion.ValveOnOff.Key, "0");
            AssertCorrectProperty(iniSection, StructureRegion.IniValveOpen.Key, expectedGateInitialOpening);
        }
    }
}