using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructurePumpTest : 
        DefinitionGeneratorStructureBaseTestFixture<DefinitionGeneratorStructurePump>
    {
        protected override string TStructureDefinitionType => "pump";
        private const string expectedBranchName = "branch-name";
        private const double inputChainage = 5.5;
        private const double snappedChainage = 5;
        private const string chainageStr = "5.000000";
        private const string expectedLongName = "Every day we stray further from god's light."; 


        protected static TestCaseData CreateCreateStructureRegionTestData(Pump structure, string expectedCrestLevel) =>
            new TestCaseData(structure, expectedCrestLevel);

        private static Pump CreatePump(bool useCapacityTimeSeries) =>
            new Pump(nameof(Pump), true)
            {
                UseCapacityTimeSeries = useCapacityTimeSeries,
                Branch = CreateBranchMock(expectedBranchName, inputChainage, snappedChainage),
                Chainage = inputChainage,
                LongName = expectedLongName
            };

        private static IEnumerable<TestCaseData> CreateStructureRegionData()
        {
            yield return CreateCreateStructureRegionTestData(
                    CreatePump(true), "Pump_capacity.tim")
                .SetName("With TimeSeries");
            yield return CreateCreateStructureRegionTestData(
                    CreatePump(false), "1.0000")
                .SetName("Without TimeSeries");
        }

        [Test]
        [TestCaseSource(nameof(CreateStructureRegionData))]
        public void CreateStructureRegion_TStructure_ExpectedResult(Pump structure,
                                                                    string expectedCapacity)
        {
            IDelftIniCategory category = Generator.CreateStructureRegion(structure);

            Assert.That(category.Properties.Count, Is.EqualTo(16));
            AssertCorrectCommonRegionElements(category, 
                                              nameof(Pump),
                                              expectedLongName,
                                              expectedBranchName,
                                              chainageStr,
                                              TStructureDefinitionType);
            AssertCorrectProperty(category, StructureRegion.Orientation.Key, "positive");
            AssertCorrectProperty(category, "controlSide", "suctionSide");
            AssertCorrectProperty(category, "numStages", "1");
            AssertCorrectProperty(category, StructureRegion.Capacity.Key, expectedCapacity);
            AssertCorrectProperty(category, StructureRegion.StartLevelSuctionSide.Key, "3.000");
            AssertCorrectProperty(category, StructureRegion.StopLevelSuctionSide.Key, "2.000");
            AssertCorrectProperty(category, StructureRegion.StartLevelDeliverySide.Key, "0.000");
            AssertCorrectProperty(category, StructureRegion.StopLevelDeliverySide.Key, "0.000");
            AssertCorrectProperty(category, StructureRegion.ReductionFactorLevels.Key, "0");
            AssertCorrectProperty(category, StructureRegion.Head.Key, "0.0000000e+000");
            AssertCorrectProperty(category, StructureRegion.ReductionFactor.Key, "1.0000000e+000");
        } 
    }
}