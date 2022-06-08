using System.Collections.Generic;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructureOrificeTest : 
        DefinitionGeneratorStructureBaseTestFixture<DefinitionGeneratorStructureOrifice>
    {
        protected override string TStructureDefinitionType => "orifice";

        private const string expectedBranchName = "branch-name";
        private const double inputChainage = 5.5;
        private const double snappedChainage = 5;
        private const string chainageStr = "5.000000";
        private const string expectedLongName = "Every day we stray further from god's light.";

        private static Orifice CreateOrifice(bool useCrestLevelTimeSeries, 
                                             bool useLowerEdgeLevelTimeSeries)
        {
            var structure = new Orifice(nameof(Orifice), true) 
            { 
                UseCrestLevelTimeSeries = useCrestLevelTimeSeries, 
                Branch = CreateBranchMock(expectedBranchName, inputChainage, snappedChainage), 
                Chainage = inputChainage, 
                LongName = expectedLongName
            };
            ((GatedWeirFormula)structure.WeirFormula).UseLowerEdgeLevelTimeSeries = useLowerEdgeLevelTimeSeries;

            return structure;
        }

        protected static TestCaseData CreateCreateStructureRegionTestData(Orifice structure, 
                                                                          string expectedCrestLevel,
                                                                          string expectedLowerEdgeLevel) =>
            new TestCaseData(structure, expectedCrestLevel, expectedLowerEdgeLevel);

        private static IEnumerable<TestCaseData> CreateStructureRegionData()
        {
            const string constantCrestLevel = "1.000";
            const string constantLowerEdgeLevel = "2.000";
            const string timeSeriesCrestLevel = "Orifice_crest_level.tim";
            const string timeSeriesLowerEdgeLevel = "Orifice_gate_opening.tim";

            yield return CreateCreateStructureRegionTestData(
                    CreateOrifice(true, true), 
                    timeSeriesCrestLevel,
                    timeSeriesLowerEdgeLevel)
                .SetName("With Crest Level TimeSeries and Gate Opening TimeSeries");
            yield return CreateCreateStructureRegionTestData(
                    CreateOrifice(true, false), 
                    timeSeriesCrestLevel, 
                    constantLowerEdgeLevel)
                .SetName("With Crest Level TimeSeries");
            yield return CreateCreateStructureRegionTestData(
                    CreateOrifice(false, true), 
                    constantCrestLevel, 
                    timeSeriesLowerEdgeLevel)
                .SetName("With Gate Opening TimeSeries");
            yield return CreateCreateStructureRegionTestData(
                    CreateOrifice(false, false), 
                    constantCrestLevel, 
                    constantLowerEdgeLevel)
                .SetName("Without TimeSeries");
        }

        [Test]
        [TestCaseSource(nameof(CreateStructureRegionData))]
        public void CreateStructureRegion_TStructure_ExpectedResult(Weir structure,
                                                                    string expectedCrestLevel,
                                                                    string expectedLowerEdgeLevel) 
        {
            IDelftIniCategory category = Generator.CreateStructureRegion(structure);

            Assert.That(category.Properties.Count, Is.EqualTo(11)); 
            AssertCorrectCommonRegionElements(category, 
                                              nameof(Orifice), 
                                              expectedLongName,
                                              expectedBranchName, 
                                              chainageStr, 
                                              TStructureDefinitionType); 
            AssertCorrectProperty(category, StructureRegion.AllowedFlowDir.Key, "both"); 
            AssertCorrectProperty(category, StructureRegion.CrestLevel.Key, expectedCrestLevel); 
            AssertCorrectProperty(category, StructureRegion.CrestWidth.Key, "5.000"); 
            AssertCorrectProperty(category, StructureRegion.GateLowerEdgeLevel.Key, expectedLowerEdgeLevel); 
            AssertCorrectProperty(category, StructureRegion.CorrectionCoeff.Key, "0.630"); 
            AssertCorrectProperty(category, StructureRegion.UseVelocityHeight.Key, "true");
        }
    }
}