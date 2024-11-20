using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructureUniversalWeirTest :
        DefinitionGeneratorStructureBaseTestFixture<DefinitionGeneratorStructureUniversalWeir>
    {
        protected override DefinitionGeneratorStructureUniversalWeir CreateGenerator()
        {
            return new DefinitionGeneratorStructureUniversalWeir(StructureFileNameGeneratorSubstitute);
        }
        
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
                    CreateWeir(true), "0.000")
                .SetName("FreeformWeir should not use TimeSeries for crest level even if useCrestLevelTimeSeries is true");
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

            Assert.That(iniSection.Properties.Count(), Is.EqualTo(11));

            Assert.Multiple(() =>
            {
                AssertCorrectCommonRegionElements(iniSection,
                                                  nameof(Weir),
                                                  expectedLongName,
                                                  expectedBranchName,
                                                  chainageStr,
                                                  TStructureDefinitionType);
                AssertCorrectProperty(iniSection, StructureRegion.AllowedFlowDir.Key, "both");
                AssertCorrectProperty(iniSection, StructureRegion.LevelsCount.Key, "2");
                AssertCorrectProperty(iniSection, StructureRegion.YValues.Key, "0.000 10.000");
                AssertCorrectProperty(iniSection, StructureRegion.ZValues.Key, "10.000 10.000");
                AssertCorrectProperty(iniSection, StructureRegion.CrestLevel.Key, expectedCrestLevel);
                AssertCorrectProperty(iniSection, StructureRegion.DischargeCoeff.Key, "1.000");
            });
        }
    }
}
