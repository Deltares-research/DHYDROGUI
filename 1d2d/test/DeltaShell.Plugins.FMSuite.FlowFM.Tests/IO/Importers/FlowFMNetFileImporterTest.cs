using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class FlowFMNetFileImporterTest
    {
        private FlowFMNetFileImporter importer;

        [SetUp]
        public void SetUp()
        {
            importer = new FlowFMNetFileImporter();
        }

        #region Properties

        [Test]
        public void GivenFlowFmNetFileImporterWhenGettingSupportedItemTypesThenReturnIEnumerableOfLengthOne()
        {
            Assert.That(importer.SupportedItemTypes.Count(), Is.EqualTo(1));
            Assert.That(importer.SupportedItemTypes.First(), Is.EqualTo(typeof(UnstructuredGrid)));
        }
        #endregion

        #region Methods

        [Test]
        public void GivenCanImportOnMethodWhenInvokingWithTargetObjectEqualToNullThenReturnTrue()
        {
            Assert.That(importer.CanImportOn(null), Is.EqualTo(true));
        }

        [Test]
        public void GivenCanImportOnMethodWhenInvokingWithNonUnstructuredGridObjectThenReturnFalse()
        {
            string targetObject = "";
            Assert.That(importer.CanImportOn(targetObject), Is.EqualTo(false));
        }

        [Test]
        public void GivenCanImportOnMethodWhenInvokingWithUnstructuredGridObjectAndNonNullWaterFlowFmModelThenReturnTrue()
        {
            var unstructuredGrid = new UnstructuredGrid();
            importer.GetModelForGrid = g => new WaterFlowFMModel();

            Assert.That(importer.CanImportOn(unstructuredGrid), Is.EqualTo(true));
        }

        [Test]
        public void GivenCanImportOnMethodWhenInvokingWithUnstructuredGridObjectAndNullWaterFlowFmModelThenReturnTrue()
        {
            var unstructuredGrid = new UnstructuredGrid();
            importer.GetModelForGrid = g => null;

            Assert.That(importer.CanImportOn(unstructuredGrid), Is.EqualTo(false));
        }
        #endregion
    }
}