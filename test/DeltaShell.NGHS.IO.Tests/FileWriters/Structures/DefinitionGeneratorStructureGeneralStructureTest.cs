using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructureGeneralStructureTest :
        DefinitionGeneratorStructureBaseTestFixture<DefinitionGeneratorStructureGeneralStructure>
    {
        protected override string TStructureDefinitionType => "generalstructure";

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
                WeirFormula = new GeneralStructureWeirFormula()
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

            Assert.That(category.Properties.Count, Is.EqualTo(32)); 
            AssertCorrectCommonRegionElements(category, 
                                              nameof(Weir), 
                                              expectedLongName,
                                              expectedBranchName, 
                                              chainageStr, 
                                              TStructureDefinitionType);
            Assert.Multiple(() =>
            {
                AssertCorrectProperty(category, StructureRegion.Upstream1Width.Key, "0.000");
                AssertCorrectProperty(category, StructureRegion.Upstream2Width.Key, "0.000");
                AssertCorrectProperty(category, StructureRegion.CrestWidth.Key, "0.000");
                AssertCorrectProperty(category, StructureRegion.Downstream1Width.Key, "0.000");
                AssertCorrectProperty(category, StructureRegion.Downstream2Width.Key, "0.000");

                AssertCorrectProperty(category, StructureRegion.Upstream1Level.Key, "0.000");
                AssertCorrectProperty(category, StructureRegion.Upstream2Level.Key, "0.000");
                AssertCorrectProperty(category, StructureRegion.CrestLevel.Key, expectedCrestLevel);
                AssertCorrectProperty(category, StructureRegion.Downstream1Level.Key, "0.000");
                AssertCorrectProperty(category, StructureRegion.Downstream2Level.Key, "0.000");

                AssertCorrectProperty(category, StructureRegion.GateLowerEdgeLevel.Key, "11.000");

                AssertCorrectProperty(category, StructureRegion.PosFreeGateFlowCoeff.Key, "1.000");
                AssertCorrectProperty(category, StructureRegion.PosDrownGateFlowCoeff.Key, "1.000");
                AssertCorrectProperty(category, StructureRegion.PosFreeWeirFlowCoeff.Key, "1.000");
                AssertCorrectProperty(category, StructureRegion.PosDrownWeirFlowCoeff.Key, "1.000");
                AssertCorrectProperty(category, StructureRegion.PosContrCoefFreeGate.Key, "1.000");

                AssertCorrectProperty(category, StructureRegion.NegFreeGateFlowCoeff.Key, "1.000");
                AssertCorrectProperty(category, StructureRegion.NegDrownGateFlowCoeff.Key, "1.000");
                AssertCorrectProperty(category, StructureRegion.NegFreeWeirFlowCoeff.Key, "1.000");
                AssertCorrectProperty(category, StructureRegion.NegDrownWeirFlowCoeff.Key, "1.000");
                AssertCorrectProperty(category, StructureRegion.NegContrCoefFreeGate.Key, "1.000");

                AssertCorrectProperty(category, StructureRegion.CrestLength.Key, "0.000");
                AssertCorrectProperty(category, StructureRegion.ExtraResistance.Key, "0.000");

                AssertCorrectProperty(category, StructureRegion.GateHeight.Key, "1.000");

                AssertCorrectProperty(category, StructureRegion.GateOpeningWidth.Key, "0.000");
            });
        }
    }
}