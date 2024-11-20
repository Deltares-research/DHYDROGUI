using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class BcmFileFlowBoundaryDataBuilderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportMorphologyBedLoadBoundaryConditions()
        {
            var filePath = TestHelper.GetTestFilePath(@"BcmFiles\MorphologyBedLoadTransport.bcm");
            var fileReader = new BcmFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            var feature = new Feature2D
            {
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1)}),
                Name = "Boundary01"
            };

            var boundaryConditionSet = new BoundaryConditionSet {Feature = feature};

            var builder = new BcmFileFlowBoundaryDataBuilder();

            builder.InsertBoundaryData(new[] {boundaryConditionSet}, dataBlocks.ElementAt(0));

            Assert.AreEqual(1, boundaryConditionSet.BoundaryConditions.Count);

            var boundaryCondition = boundaryConditionSet.BoundaryConditions.FirstOrDefault() as FlowBoundaryCondition;

            Assert.IsNotNull(boundaryCondition);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, boundaryCondition.DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.MorphologyBedLoadTransport, boundaryCondition.FlowQuantity);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportMorphologyBedLevelPrescribedBoundaryConditions()
        {
            var filePath = TestHelper.GetTestFilePath(@"BcmFiles\MorphologyBedLevelPrescribed.bcm");
            var fileReader = new BcmFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            var feature = new Feature2D
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1) }),
                Name = "Boundary01"
            };

            var boundaryConditionSet = new BoundaryConditionSet { Feature = feature };

            var builder = new BcmFileFlowBoundaryDataBuilder();

            builder.InsertBoundaryData(new[] { boundaryConditionSet }, dataBlocks.ElementAt(0));

            Assert.AreEqual(1, boundaryConditionSet.BoundaryConditions.Count);

            var boundaryCondition = boundaryConditionSet.BoundaryConditions.FirstOrDefault() as FlowBoundaryCondition;

            Assert.IsNotNull(boundaryCondition);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, boundaryCondition.DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, boundaryCondition.FlowQuantity);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportMorphologyBedLevelChangedPrescribedBoundaryConditions()
        {
            var filePath = TestHelper.GetTestFilePath(@"BcmFiles\MorphologyBedLevelChangePrescribed.bcm");
            var fileReader = new BcmFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            var feature = new Feature2D
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1) }),
                Name = "Boundary01"
            };

            var boundaryConditionSet = new BoundaryConditionSet { Feature = feature };

            var builder = new BcmFileFlowBoundaryDataBuilder();

            builder.InsertBoundaryData(new[] { boundaryConditionSet }, dataBlocks.ElementAt(0));

            Assert.AreEqual(1, boundaryConditionSet.BoundaryConditions.Count);

            var boundaryCondition = boundaryConditionSet.BoundaryConditions.FirstOrDefault() as FlowBoundaryCondition;

            Assert.IsNotNull(boundaryCondition);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, boundaryCondition.DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed, boundaryCondition.FlowQuantity);
        }

        [Test]
        public void GetBcmBlockDataTest()
        {
            var bcmBlockData = new BcmBlockData();
            Assert.NotNull( bcmBlockData );
            Assert.NotNull( bcmBlockData.Quantities );
            Assert.IsEmpty( bcmBlockData.Quantities );
        }
    }
}

