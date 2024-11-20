using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class ObservationPointImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportLocationsTest()
        {
            IEventedList<WaterQualityObservationPoint> monitorPoints = new EventedList<WaterQualityObservationPoint>();

            string path = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "dry_loads_shp_file", "sfb_testlocationsinput.shp");
            var importer = new ObservationPointImporter();
            importer.ImportItem(path, monitorPoints);

            Assert.AreEqual(3, monitorPoints.Count);
            Assert.AreEqual("Point 1", monitorPoints[0].Name);
            Assert.AreEqual("Point 2", monitorPoints[1].Name);
            Assert.AreEqual("Point 3", monitorPoints[2].Name);

            const double expectedXFromShapeFile = -122.19199999999999d;
            const double expectedYFromShapeFile = 37.567d;

            Assert.AreEqual(expectedXFromShapeFile, monitorPoints[0].X);
            Assert.AreEqual(expectedYFromShapeFile, monitorPoints[0].Y);
            Assert.AreEqual(double.NaN, monitorPoints[0].Z);

            monitorPoints.Clear();
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            importer.ModelCoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(28992); // RD new
            importer.ImportItem(path, monitorPoints);

            Assert.AreEqual(3, monitorPoints.Count);
            Assert.AreEqual("Point 1", monitorPoints[0].Name);
            Assert.AreEqual("Point 2", monitorPoints[1].Name);
            Assert.AreEqual("Point 3", monitorPoints[2].Name);
            Assert.AreNotEqual(expectedXFromShapeFile, monitorPoints[0].X);
            Assert.AreNotEqual(expectedYFromShapeFile, monitorPoints[0].Y);
            Assert.AreEqual(double.NaN, monitorPoints[0].Z);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportLocationsTestWithZandObservationPointType()
        {
            IEventedList<WaterQualityObservationPoint> monitorPoints = new EventedList<WaterQualityObservationPoint>();

            string path = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "shape_files", "observation points", "observationPoints.shp");
            var importer = new ObservationPointImporter();
            importer.ImportItem(path, monitorPoints);

            Assert.AreEqual(8, monitorPoints.Count);
            Assert.AreEqual("ObsPoint 1", monitorPoints[0].Name);
            Assert.AreEqual("ObsPoint 2", monitorPoints[1].Name);
            Assert.AreEqual("ObsPoint 3", monitorPoints[2].Name);
            Assert.AreEqual("ObsPoint 4", monitorPoints[3].Name);
            Assert.AreEqual("ObsPoint 5", monitorPoints[4].Name);
            Assert.AreEqual("ObsPoint 6", monitorPoints[5].Name);
            Assert.AreEqual("ObsPoint 7", monitorPoints[6].Name);
            Assert.AreEqual("ObsPoint 8", monitorPoints[7].Name);

            Assert.AreEqual(-2.1, monitorPoints[0].Z);
            Assert.AreEqual(0.1, monitorPoints[1].Z);
            Assert.AreEqual(0.9, monitorPoints[2].Z);
            Assert.AreEqual(0.0, monitorPoints[3].Z);
            Assert.AreEqual(0.8, monitorPoints[4].Z);
            Assert.AreEqual(0.0, monitorPoints[5].Z);
            Assert.AreEqual(0.0, monitorPoints[6].Z);
            Assert.AreEqual(0.0, monitorPoints[7].Z);

            Assert.AreEqual(ObservationPointType.OneOnEachLayer, monitorPoints[0].ObservationPointType);
            Assert.AreEqual(ObservationPointType.OneOnEachLayer, monitorPoints[1].ObservationPointType);
            Assert.AreEqual(ObservationPointType.SinglePoint, monitorPoints[2].ObservationPointType);
            Assert.AreEqual(ObservationPointType.SinglePoint, monitorPoints[3].ObservationPointType);
            Assert.AreEqual(ObservationPointType.SinglePoint, monitorPoints[4].ObservationPointType);
            Assert.AreEqual(ObservationPointType.SinglePoint, monitorPoints[5].ObservationPointType);
            Assert.AreEqual(ObservationPointType.SinglePoint, monitorPoints[6].ObservationPointType);
            Assert.AreEqual(ObservationPointType.Average, monitorPoints[7].ObservationPointType);
        }

        [Test]
        public void ImportNonExistingFileTest()
        {
            IEventedList<WaterQualityObservationPoint> loads = new EventedList<WaterQualityObservationPoint>();

            string path = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "idontexist.shp");
            var importer = new ObservationPointImporter();
            importer.ImportItem(path, loads);

            Assert.AreEqual(0, loads.Count);
        }
    }
}