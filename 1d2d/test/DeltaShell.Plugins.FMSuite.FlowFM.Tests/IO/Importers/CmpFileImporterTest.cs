using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class CmpFileImporterTest
    {
        private CmpFileImporter importer;
        private BoundaryCondition boundaryCondition;

        [SetUp]
        public void Setup()
        {
            importer = new CmpFileImporter();
        }

        [Test]
        public void GivenBcmFileImporterWhenBoundaryConditionHasInCorrectQuantityTypeThenValidateFalse()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, BoundaryConditionDataType.TimeSeries);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }

        [Test]
        public void GivenBcmFileImporterWhenBoundaryConditionHasInCorrectDataTypeThenValidateFalse()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Empty);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }

        [TestCase(BoundaryConditionDataType.AstroComponents)]
        [TestCase(BoundaryConditionDataType.Harmonics)]
        [TestCase(BoundaryConditionDataType.HarmonicCorrection)]
        [TestCase(BoundaryConditionDataType.AstroCorrection)]
        public void GivenBcmFileImporterWhenBoundaryConditionHasCorrectDataTypeThenValidateTrue(BoundaryConditionDataType boundaryConditionDataType)
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, boundaryConditionDataType);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsTrue(result, "The flowboundary quantity type is incorrect");
        }
    }
}
