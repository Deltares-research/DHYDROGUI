using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructureOrificeTest : 
        DefinitionGeneratorStructureBaseTestFixture<DefinitionGeneratorStructureOrifice>
    {
        protected override DefinitionGeneratorStructureOrifice CreateGenerator()
        {
            return new DefinitionGeneratorStructureOrifice(StructureFileNameGeneratorSubstitute);
        }
        
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

        private static TestCaseData CreateCreateStructureRegionTestData(Orifice structure, 
                                                                        string expectedCrestLevel,
                                                                        string expectedLowerEdgeLevel)
        {
            return new TestCaseData(structure, expectedCrestLevel, expectedLowerEdgeLevel);
        }

        private static IEnumerable<TestCaseData> CreateStructureRegionData()
        {
            const string constantCrestLevel = "1.000";
            const string constantLowerEdgeLevel = "11.000";
            const string timeSeriesCrestLevel = ExpectedStructureFileName;
            const string timeSeriesLowerEdgeLevel = ExpectedStructureFileName;

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
            IniSection iniSection = Generator.CreateStructureRegion(structure);

            Assert.That(iniSection.Properties.Count(), Is.EqualTo(11)); 
            AssertCorrectCommonRegionElements(iniSection, 
                                              nameof(Orifice), 
                                              expectedLongName,
                                              expectedBranchName, 
                                              chainageStr, 
                                              TStructureDefinitionType); 
            AssertCorrectProperty(iniSection, StructureRegion.AllowedFlowDir.Key, "both"); 
            AssertCorrectProperty(iniSection, StructureRegion.CrestLevel.Key, expectedCrestLevel); 
            AssertCorrectProperty(iniSection, StructureRegion.CrestWidth.Key, "5.000"); 
            AssertCorrectProperty(iniSection, StructureRegion.GateLowerEdgeLevel.Key, expectedLowerEdgeLevel); 
            AssertCorrectProperty(iniSection, StructureRegion.CorrectionCoeff.Key, "0.630"); 
            AssertCorrectProperty(iniSection, StructureRegion.UseVelocityHeight.Key, "true");
        }
    }
}