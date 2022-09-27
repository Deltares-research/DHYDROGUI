using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
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
            IDelftIniCategory category = Generator.CreateStructureRegion(structure);

            Assert.That(category.Properties.Count, Is.EqualTo(14));
            AssertCorrectCommonRegionElements(category,
                                              nameof(Culvert),
                                              expectedLongName,
                                              expectedBranchName,
                                              chainageStr,
                                              TStructureDefinitionType);
            AssertCorrectProperty(category, StructureRegion.AllowedFlowDir.Key, "both");
            AssertCorrectProperty(category, StructureRegion.LeftLevel.Key, "0.000");
            AssertCorrectProperty(category, StructureRegion.RightLevel.Key, "0.000");
            AssertCorrectProperty(category, StructureRegion.CsDefId.Key, "Culvert");
            AssertCorrectProperty(category, StructureRegion.Length.Key, "0.000");
            AssertCorrectProperty(category, StructureRegion.InletLossCoeff.Key, "0.000");
            AssertCorrectProperty(category, StructureRegion.OutletLossCoeff.Key, "0.000");
            AssertCorrectProperty(category, StructureRegion.ValveOnOff.Key, "0");
            AssertCorrectProperty(category, StructureRegion.IniValveOpen.Key, expectedGateInitialOpening);
        }
    }
}