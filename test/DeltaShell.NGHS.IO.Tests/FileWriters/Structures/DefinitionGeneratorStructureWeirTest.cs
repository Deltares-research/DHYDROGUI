using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructureWeirTest :
        DefinitionGeneratorStructureBaseTestFixture<DefinitionGeneratorStructureWeir>
    {
        protected override DefinitionGeneratorStructureWeir CreateGenerator()
        {
            return new DefinitionGeneratorStructureWeir(StructureFileNameGeneratorSubstitute);
        }
        
        protected override string TStructureDefinitionType => "weir";

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
                LongName = expectedLongName
            };

        protected static TestCaseData CreateCreateStructureRegionTestData(Weir structure, string expectedCrestLevel) =>
            new TestCaseData(structure, expectedCrestLevel);

        private static IEnumerable<TestCaseData> CreateStructureRegionData()
        {
            yield return CreateCreateStructureRegionTestData(
                    CreateWeir(true), ExpectedStructureFileName)
                .SetName("With TimeSeries");
            yield return CreateCreateStructureRegionTestData(
                    CreateWeir(false), "1.000")
                .SetName("Without TimeSeries");
        }

        [Test]
        [TestCaseSource(nameof(CreateStructureRegionData))]
        public void CreateStructureRegion_TStructure_ExpectedResult(Weir structure,
                                                                    string expectedCrestLevel) 
        {
            IniSection iniSection = Generator.CreateStructureRegion(structure);

            Assert.That(iniSection.Properties.Count(), Is.EqualTo(10)); 
            AssertCorrectCommonRegionElements(iniSection, 
                                              nameof(Weir), 
                                              expectedLongName,
                                              expectedBranchName, 
                                              chainageStr, 
                                              TStructureDefinitionType); 
            AssertCorrectProperty(iniSection, StructureRegion.AllowedFlowDir.Key, "both"); 
            AssertCorrectProperty(iniSection, StructureRegion.CrestLevel.Key, expectedCrestLevel); 
            AssertCorrectProperty(iniSection, StructureRegion.CrestWidth.Key, "5.000"); 
            AssertCorrectProperty(iniSection, StructureRegion.CorrectionCoeff.Key, "1.000"); 
            AssertCorrectProperty(iniSection, StructureRegion.UseVelocityHeight.Key, "true");
        }
    }
}