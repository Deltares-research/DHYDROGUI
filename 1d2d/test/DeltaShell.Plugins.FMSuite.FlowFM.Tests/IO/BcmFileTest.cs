using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class BcmFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestGroupBoundaryConditions()
        {
            // setup
            var boundaryConditionSet1 = new BoundaryConditionSet();
            boundaryConditionSet1.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet1.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet1.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet1.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries));

            var boundaryConditionSet2 = new BoundaryConditionSet();
            boundaryConditionSet2.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLoadTransport, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet2.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet2.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries));
            boundaryConditionSet2.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.TimeSeries));

            var boundaryConditionSet3 = new BoundaryConditionSet();
            boundaryConditionSet3.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelFixed, BoundaryConditionDataType.Empty));

            var boundaryConditions = new List<BoundaryConditionSet>() { boundaryConditionSet1, boundaryConditionSet2, boundaryConditionSet3 };

            // group boundary conditions
            var bcmFile = new BcmFile();
            var groupings = bcmFile.GroupBoundaryConditions(boundaryConditions).ToList();

            // check that non-Morphology related boundary conditions are filtered out
            var groupedBoundaryConditions = groupings.SelectMany(g => g).Select(g => g.Item1).OfType<FlowBoundaryCondition>().ToList();
            // there are 3 conditions but 1 of them is without boundary data... so that is also not in the count!
            Assert.AreEqual(2, groupedBoundaryConditions.Count);

            Assert.AreEqual(1, groupedBoundaryConditions.Count(bc => bc.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLevelPrescribed));
            Assert.AreEqual(0, groupedBoundaryConditions.Count(bc => bc.FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration));
            Assert.AreEqual(0, groupedBoundaryConditions.Count(bc => bc.FlowQuantity == FlowBoundaryQuantityType.WaterLevel));
            Assert.AreEqual(1, groupedBoundaryConditions.Count(bc => bc.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport));
            Assert.AreEqual(0, groupedBoundaryConditions.Count(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Discharge));
        }

        [TestCase(@"BcmFiles\MorphologyBedLevelPrescribed.bcm", new [] { "time", BcmFileFlowBoundaryDataBuilder.BedLevelAtBound }, 23)]
        [TestCase(@"BcmFiles\MorphologyBedLevelChangePrescribed.bcm", new[] { "time", BcmFileFlowBoundaryDataBuilder.BedLevelChangeAtBound }, 12)]
        [TestCase(@"BcmFiles\MorphologyBedLoadTransport.bcm", new[] { "time", BcmFileFlowBoundaryDataBuilder.BedLoadAtBound+"abc", BcmFileFlowBoundaryDataBuilder.BedLoadAtBound + "def" }, 289)]
        [Category(TestCategory.DataAccess)]
        public void TestReadMorphologyBoundaryConditions(string testFile, string[] quantityNames, int numValues)
        {
            var filePath = TestHelper.GetTestFilePath(testFile);
            var bcmFile = new BcmFile();
            var blockData = bcmFile.Read(filePath).ToList();

            Assert.AreEqual(1, blockData.Count);
            var quantities = blockData[0].Quantities;

            Assert.AreEqual(quantityNames.Length, quantities.Count);

            for (var i = 0; i < quantityNames.Length; i++)
            {
                Assert.AreEqual(quantityNames[i], quantities[i].Quantity);
                Assert.AreEqual(numValues, quantities[i].Values.Count);
            }
        }
    }
}
