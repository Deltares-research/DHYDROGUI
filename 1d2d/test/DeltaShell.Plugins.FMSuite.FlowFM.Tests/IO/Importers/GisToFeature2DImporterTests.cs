using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class GisToFeature2DImporterTests
    {
        [Test]
        public void PointShapeImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\points.shp");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<IPoint, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.AreEqual(6, importedFeatures.Count);
        }

        [Test]
        public void PointGeoJSONImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\points.geojson");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<IPoint, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.AreEqual(6, importedFeatures.Count);
        }

        [Test]
        public void WrongShapeTypeImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\lines.shp");
            filePath = TestHelper.CreateLocalCopy(filePath);

            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                var importer = new GisToFeature2DImporter<IPoint, Feature2D>();
                var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            }, "is not matching the expected type");
        }

        [Test]
        public void WrongGMLTypeImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\lines.gml");
            filePath = TestHelper.CreateLocalCopy(filePath);

            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                var importer = new GisToFeature2DImporter<IPoint, Feature2D>();
                var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            }, "is not matching the expected type");
        }

        [Test]
        public void WrongGeoJSONTypeImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\lines.geojson");
            filePath = TestHelper.CreateLocalCopy(filePath);

            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                var importer = new GisToFeature2DImporter<IPoint, Feature2D>();
                var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            }, "is not matching the expected type");
        }

        [Test]
        public void LineStringShapeImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\lines.shp");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<ILineString, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.AreEqual(3, importedFeatures.Count);
        }

        [Test]
        public void LineStringGMLImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\lines.gml");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<ILineString, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.AreEqual(3, importedFeatures.Count);
        }

        [Test]
        public void LineStringGeoJSONImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\lines.geojson");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<ILineString, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.AreEqual(3, importedFeatures.Count);
        }

        [Test]
        public void PolygonShapeImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\polygons.shp");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<IPolygon, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.AreEqual(3, importedFeatures.Count);
        }

        [Test]
        public void PolygonGMLImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\polygons.gml");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<IPolygon, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.AreEqual(3, importedFeatures.Count);
        }

        [Test]
        public void PolygonGeoJSONImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\polygons.geojson");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<IPolygon, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.AreEqual(3, importedFeatures.Count);
        }

        [Test]
        public void LeveeBreachShapeImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\dijkbreuklijn_Hoenzadriel.shp");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<ILineString, LeveeBreach>();
            var importedFeatures = importer.ImportItem(filePath, new List<LeveeBreach>()) as IList<LeveeBreach>;

            Assert.IsNotNull(importedFeatures);
            Assert.AreEqual(importedFeatures.Count, 1);

            var importedLeveeBreach = importedFeatures.FirstOrDefault();

            Assert.IsNotNull(importedLeveeBreach);
            Assert.IsTrue(importedLeveeBreach.GetType() == typeof(LeveeBreach));
            Assert.IsFalse(string.IsNullOrWhiteSpace(importedLeveeBreach.Name));
            Assert.IsNotNull(importedLeveeBreach.Geometry);
            Assert.IsTrue(importedLeveeBreach.Geometry is ILineString);
            Assert.Greater(importedLeveeBreach.Geometry.Coordinates.Count(),0);
        }
    }
}
