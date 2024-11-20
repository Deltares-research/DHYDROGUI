using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class BcFileImporterTest
    {
        private BcFileImporter importer;

        [SetUp]
        public void Setup()
        {
            importer = new BcFileImporter();
        }

        [Test]
        public void GivenBcFileImporterWhenBoundaryConditionHasInCorrectQuantityTypeThenValidateFalse()
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, BoundaryConditionDataType.TimeSeries);
            bool result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }

        [Test]
        public void GivenBcFileImporterWhenBoundaryConditionHasInCorrectDataTypeThenValidateFalse()
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Empty);
            bool result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
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
            var flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, BoundaryConditionDataType.TimeSeries);
            bool result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsTrue(result, "The flowboundary quantity type is incorrect");
        }

        [TestCase(BoundaryConditionDataType.AstroComponents)]
        [TestCase(BoundaryConditionDataType.Harmonics)]
        [TestCase(BoundaryConditionDataType.HarmonicCorrection)]
        [TestCase(BoundaryConditionDataType.AstroCorrection)]
        [TestCase(BoundaryConditionDataType.TimeSeries)]
        public void GivenBcFileImporterWhenBoundaryConditionHasCorrectDataTypeThenValidateTrue(BoundaryConditionDataType boundaryConditionDataType)
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, boundaryConditionDataType);
            bool result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsTrue(result, "The flowboundary quantity type is incorrect");
        }
    }
}