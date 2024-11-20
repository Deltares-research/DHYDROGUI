using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class QhFileImporterTest
    {
        private QhFileImporter importer;

        [SetUp]
        public void Setup()
        {
            importer = new QhFileImporter();
        }

        [Test]
        public void GivenQhFileImporterWhenBoundaryConditionHasCorrectQuantityThenValidateTrue()
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Qh);
            bool result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsTrue(result, "The flowboundary quantity type is incorrect");
        }

        [Test]
        public void GivenQhFileImporterWhenBoundaryConditionHasInCorrectDataTypeThenValidateFalse()
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Empty);
            bool result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }
    }
}