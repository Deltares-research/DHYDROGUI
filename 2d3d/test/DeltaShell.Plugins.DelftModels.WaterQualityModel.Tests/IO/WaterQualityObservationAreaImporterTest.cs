using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class WaterQualityObservationAreaImporterTest
    {
        [Test]
        public void DefaultConstructor_ExpectedValues()
        {
            // call
            var importer = new WaterQualityObservationAreaImporter();

            // assert
            Assert.IsInstanceOf<IFileImporter>(importer);
            Assert.AreEqual("Observation area from GIS importer", importer.Name);
            Assert.AreEqual("Hydro", importer.Category);
            Assert.IsNull(importer.Image);
            CollectionAssert.AreEqual(new[]
            {
                typeof(WaterQualityObservationAreaCoverage)
            }, importer.SupportedItemTypes.ToArray());
            Assert.IsFalse(importer.CanImportOnRootLevel);
            Assert.AreEqual("Shape file (*.shp)|*.shp", importer.FileFilter);
            Assert.IsNull(importer.TargetDataDirectory);
            Assert.IsFalse(importer.ShouldCancel);
            Assert.IsNull(importer.ProgressChanged);
            Assert.IsTrue(importer.OpenViewAfterImport);
            Assert.IsNull(importer.ModelCoordinateSystem);
        }

        [Test]
        public void ImportItem_NoTargetSpecified_ThrowNotSupportedException()
        {
            // setup
            string path = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "observationAreas", "3areas_epsg4326.shp");
            var importer = new WaterQualityObservationAreaImporter();

            // call
            TestDelegate call = () => importer.ImportItem(path);

            // assert
            var exception = Assert.Throws<NotSupportedException>(call);
            Assert.AreEqual("Target should be Water Quality Observation Area Spatial Data.", exception.Message);
        }

        [Test]
        public void ImportItem_TargetIncorrect_ThrowNotSupportedException()
        {
            // setup
            string path = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "observationAreas", "3areas_epsg4326.shp");
            var importer = new WaterQualityObservationAreaImporter();
            var target = new object();

            // call
            TestDelegate call = () => importer.ImportItem(path, target);

            // assert
            var exception = Assert.Throws<NotSupportedException>(call);
            Assert.AreEqual("Target should be Water Quality Observation Area Spatial Data.", exception.Message);
        }

        [Test]
        public void ImportItem_TargetIsWaterQualityObservationAreaCoverageWithoutSpatialOperations_AddSpatialOperationsBasedOnPolygonShapes()
        {
            // setup
            string path = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "observationAreas", "3areas_epsg4326.shp");

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(20, 20, 1, 1);
            var target = new WaterQualityObservationAreaCoverage(grid);
            var dataItem = new DataItem(target);

            // create a spatial operation value converter and add a set value operation
            SpatialOperationSetValueConverter valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem, target.Name);

            Assert.AreSame(valueConverter, dataItem.ValueConverter,
                           "Test Precondition: DataItem has a spatial operation ValueConverter.");
            Assert.AreEqual(0, valueConverter.SpatialOperationSet.Operations.Count,
                            "Test Precondition: ValueConverter has no spatial operations set yet.");

            var importer = new WaterQualityObservationAreaImporter();
            importer.GetDataItemForTarget = t => dataItem;
            importer.ModelCoordinateSystem = null;

            // call
            object result = importer.ImportItem(path, target);

            // assert
            Assert.AreEqual(3, valueConverter.SpatialOperationSet.Operations.Count);
            CollectionAssert.AllItemsAreInstancesOfType(valueConverter.SpatialOperationSet.Operations, typeof(SetLabelOperation));
            var secondSetLabelOperation = (SetLabelOperation) valueConverter.SpatialOperationSet.Operations[1];
            Assert.AreEqual("Set Label 2", secondSetLabelOperation.Name);
            Assert.AreEqual("TopRight".ToLower(), secondSetLabelOperation.Label);

            Assert.IsFalse(valueConverter.SpatialOperationSet.Dirty);

            Assert.AreSame(dataItem.Value, result);
        }
    }
}