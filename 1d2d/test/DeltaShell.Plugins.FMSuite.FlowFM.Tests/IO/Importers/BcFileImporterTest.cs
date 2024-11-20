using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class BcFileImporterTest
    {
        private BcFileImporter importer;
        private BoundaryCondition boundaryCondition;

        [SetUp]
        public void Setup()
        {
            importer = new BcFileImporter();
        }

        [TestCase(FlowBoundaryQuantityType.WaterLevel)]
        [TestCase(FlowBoundaryQuantityType.Discharge)]
        [TestCase(FlowBoundaryQuantityType.Neumann)]
        [TestCase(FlowBoundaryQuantityType.NormalVelocity)]
        [TestCase(FlowBoundaryQuantityType.Salinity)]
        [TestCase(FlowBoundaryQuantityType.TangentVelocity)]
        [TestCase(FlowBoundaryQuantityType.Velocity)]
        public void GivenBcFileImporterWhenBoundaryConditionHasCorrectQuantityThenValidateTrue(FlowBoundaryQuantityType flowBoundaryQuantityType)
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, BoundaryConditionDataType.TimeSeries);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsTrue(result, "The flowboundary quantity type is incorrect");
        }

        [Test]
        public void GivenBcFileImporterWhenBoundaryConditionHasInCorrectQuantityTypeThenValidateFalse()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, BoundaryConditionDataType.TimeSeries);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }

        [TestCase(BoundaryConditionDataType.AstroComponents)]
        [TestCase(BoundaryConditionDataType.Harmonics)]
        [TestCase(BoundaryConditionDataType.HarmonicCorrection)]
        [TestCase(BoundaryConditionDataType.AstroCorrection)]
        [TestCase(BoundaryConditionDataType.TimeSeries)]
        public void GivenBcFileImporterWhenBoundaryConditionHasCorrectDataTypeThenValidateTrue(BoundaryConditionDataType boundaryConditionDataType)
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, boundaryConditionDataType);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsTrue(result, "The flowboundary quantity type is incorrect");
        }

        [Test]
        public void GivenBcFileImporterWhenBoundaryConditionHasInCorrectDataTypeThenValidateFalse()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Empty);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }

    }
}
