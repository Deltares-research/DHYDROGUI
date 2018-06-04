using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\2thepoint.shp");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<IPoint, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.Greater(importedFeatures.Count, 0);
        }

        [Test]
        public void WrongShapeTypeImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\dijkbreuklijn_Hoenzadriel.shp");
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
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\dijkbreuklijn_Hoenzadriel.shp");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<ILineString, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.Greater(importedFeatures.Count, 0);
        }

        [Test]
        public void PolygonShapeImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\Gemeenten.shp");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<IPolygon, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.Greater(importedFeatures.Count, 0);
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
