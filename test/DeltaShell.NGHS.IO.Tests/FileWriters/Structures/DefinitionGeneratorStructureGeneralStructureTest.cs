using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructureGeneralStructureTest :
        DefinitionGeneratorStructureBaseTestFixture<DefinitionGeneratorStructureGeneralStructure>
    {
        protected override DefinitionGeneratorStructureGeneralStructure CreateGenerator()
        {
            return new DefinitionGeneratorStructureGeneralStructure(StructureFileNameGeneratorSubstitute);
        }
        
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
                    CreateWeir(true), ExpectedStructureFileName)
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
            IniSection iniSection = Generator.CreateStructureRegion(structure);

            Assert.That(iniSection.Properties.Count(), Is.EqualTo(32)); 
            AssertCorrectCommonRegionElements(iniSection, 
                                              nameof(Weir), 
                                              expectedLongName,
                                              expectedBranchName, 
                                              chainageStr, 
                                              TStructureDefinitionType);
            Assert.Multiple(() =>
            {
                AssertCorrectProperty(iniSection, StructureRegion.Upstream1Width.Key, "0.000");
                AssertCorrectProperty(iniSection, StructureRegion.Upstream2Width.Key, "0.000");
                AssertCorrectProperty(iniSection, StructureRegion.CrestWidth.Key, "0.000");
                AssertCorrectProperty(iniSection, StructureRegion.Downstream1Width.Key, "0.000");
                AssertCorrectProperty(iniSection, StructureRegion.Downstream2Width.Key, "0.000");

                AssertCorrectProperty(iniSection, StructureRegion.Upstream1Level.Key, "0.000");
                AssertCorrectProperty(iniSection, StructureRegion.Upstream2Level.Key, "0.000");
                AssertCorrectProperty(iniSection, StructureRegion.CrestLevel.Key, expectedCrestLevel);
                AssertCorrectProperty(iniSection, StructureRegion.Downstream1Level.Key, "0.000");
                AssertCorrectProperty(iniSection, StructureRegion.Downstream2Level.Key, "0.000");

                AssertCorrectProperty(iniSection, StructureRegion.GateLowerEdgeLevel.Key, "11.000");

                AssertCorrectProperty(iniSection, StructureRegion.PosFreeGateFlowCoeff.Key, "1.000");
                AssertCorrectProperty(iniSection, StructureRegion.PosDrownGateFlowCoeff.Key, "1.000");
                AssertCorrectProperty(iniSection, StructureRegion.PosFreeWeirFlowCoeff.Key, "1.000");
                AssertCorrectProperty(iniSection, StructureRegion.PosDrownWeirFlowCoeff.Key, "1.000");
                AssertCorrectProperty(iniSection, StructureRegion.PosContrCoefFreeGate.Key, "1.000");

                AssertCorrectProperty(iniSection, StructureRegion.NegFreeGateFlowCoeff.Key, "1.000");
                AssertCorrectProperty(iniSection, StructureRegion.NegDrownGateFlowCoeff.Key, "1.000");
                AssertCorrectProperty(iniSection, StructureRegion.NegFreeWeirFlowCoeff.Key, "1.000");
                AssertCorrectProperty(iniSection, StructureRegion.NegDrownWeirFlowCoeff.Key, "1.000");
                AssertCorrectProperty(iniSection, StructureRegion.NegContrCoefFreeGate.Key, "1.000");

                AssertCorrectProperty(iniSection, StructureRegion.CrestLength.Key, "0.000");
                AssertCorrectProperty(iniSection, StructureRegion.ExtraResistance.Key, "0.000");

                AssertCorrectProperty(iniSection, StructureRegion.GateHeight.Key, "1.000");

                AssertCorrectProperty(iniSection, StructureRegion.GateOpeningWidth.Key, "0.000");
            });
        }
    }
}