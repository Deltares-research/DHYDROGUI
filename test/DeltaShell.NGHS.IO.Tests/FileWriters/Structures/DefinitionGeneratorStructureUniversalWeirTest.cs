using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructureUniversalWeirTest :
        DefinitionGeneratorStructureBaseTestFixture<DefinitionGeneratorStructureUniversalWeir>
    {
        protected override string TStructureDefinitionType => "universalWeir";

        private const string expectedBranchName = "branch-name";
        private const double inputChainage = 5.5;
        private const double snappedChainage = 5;
        private const string chainageStr = "5.000000";
        private const string expectedLongName = "Every day we stray further from god's light."; 

        private static Weir CreateWeir(bool useCrestLevelTimeSeries) => 
            new Weir(nameof(Weir), true) 
            { 
                UseCrestLevelTimeSeries = useCrestLevelTimeSeries, 
                Branch = CreateBranchMock(expectedBranchName, inputChainage, snappedChainage), 
                Chainage = inputChainage, 
                LongName = expectedLongName,
                WeirFormula = new FreeFormWeirFormula()
            };

        protected static TestCaseData CreateCreateStructureRegionTestData(Weir structure, string expectedCrestLevel) =>
            new TestCaseData(structure, expectedCrestLevel);

        private static IEnumerable<TestCaseData> CreateStructureRegionData()
        {
            yield return CreateCreateStructureRegionTestData(
                    CreateWeir(true), "Weir_crest_level.tim")
                .SetName("With TimeSeries");
            yield return CreateCreateStructureRegionTestData(
                    CreateWeir(false), "0.000")
                .SetName("Without TimeSeries");
        }

        [Test]
        [TestCaseSource(nameof(CreateStructureRegionData))]
        public void CreateStructureRegion_TStructure_ExpectedResult(Weir structure,
                                                                    string expectedCrestLevel) 
        {
            IDelftIniCategory category = Generator.CreateStructureRegion(structure);

            Assert.That(category.Properties.Count, Is.EqualTo(11));

            Assert.Multiple(() =>
            {
                AssertCorrectCommonRegionElements(category,
                                                  nameof(Weir),
                                                  expectedLongName,
                                                  expectedBranchName,
                                                  chainageStr,
                                                  TStructureDefinitionType);
                AssertCorrectProperty(category, StructureRegion.AllowedFlowDir.Key, "both");
                AssertCorrectProperty(category, StructureRegion.LevelsCount.Key, "2");
                AssertCorrectProperty(category, StructureRegion.YValues.Key, "0.000 10.000");
                AssertCorrectProperty(category, StructureRegion.ZValues.Key, "10.000 10.000");
                AssertCorrectProperty(category, StructureRegion.CrestLevel.Key, expectedCrestLevel);
                AssertCorrectProperty(category, StructureRegion.DischargeCoeff.Key, "1.000");
            });
        }
    }
}