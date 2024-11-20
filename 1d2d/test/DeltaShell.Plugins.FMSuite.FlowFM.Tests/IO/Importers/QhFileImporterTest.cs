using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class QhFileImporterTest
    {
        private QhFileImporter importer;
        private BoundaryCondition boundaryCondition;

        [SetUp]
        public void Setup()
        {
            importer = new QhFileImporter();
        }

        [Test]
        public void GivenQhFileImporterWhenBoundaryConditionHasCorrectQuantityThenValidateTrue()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Qh);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsTrue(result, "The flowboundary quantity type is incorrect");
        }

        [Test]
        public void GivenQhFileImporterWhenBoundaryConditionHasInCorrectDataTypeThenValidateFalse()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Empty);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }

    }
}
