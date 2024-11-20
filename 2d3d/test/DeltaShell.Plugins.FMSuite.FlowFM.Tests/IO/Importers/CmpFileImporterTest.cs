using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class CmpFileImporterTest
    {
        private CmpFileImporter importer;

        [SetUp]
        public void Setup()
        {
            importer = new CmpFileImporter();
        }

        [Test]
        public void GivenBcmFileImporterWhenBoundaryConditionHasInCorrectQuantityTypeThenValidateFalse()
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, BoundaryConditionDataType.TimeSeries);
            bool result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }

        [Test]
        public void GivenBcmFileImporterWhenBoundaryConditionHasInCorrectDataTypeThenValidateFalse()
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Empty);
            bool result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }

        [TestCase(BoundaryConditionDataType.AstroComponents)]
        [TestCase(BoundaryConditionDataType.Harmonics)]
        [TestCase(BoundaryConditionDataType.HarmonicCorrection)]
        [TestCase(BoundaryConditionDataType.AstroCorrection)]
        public void GivenBcmFileImporterWhenBoundaryConditionHasCorrectDataTypeThenValidateTrue(BoundaryConditionDataType boundaryConditionDataType)
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, boundaryConditionDataType);
            bool result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsTrue(result, "The flowboundary quantity type is incorrect");
        }
    }
}