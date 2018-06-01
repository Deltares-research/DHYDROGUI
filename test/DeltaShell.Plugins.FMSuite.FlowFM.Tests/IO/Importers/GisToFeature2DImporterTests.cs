using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public void TestPointShapeImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\2thepoint.shp");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<IPoint, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.Greater(importedFeatures.Count, 0);
        }

        [Test]
        public void TestWrongShapeTypeImport()
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
        public void TestLineStringShapeImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\dijkbreuklijn_Hoenzadriel.shp");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<ILineString, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.Greater(importedFeatures.Count, 0);
        }

        [Test]
        public void TestPolygonShapeImport()
        {
            var filePath = TestHelper.GetTestFilePath(@"gisFiles\Gemeenten.shp");
            filePath = TestHelper.CreateLocalCopy(filePath);

            var importer = new GisToFeature2DImporter<IPolygon, Feature2D>();
            var importedFeatures = importer.ImportItem(filePath, new List<Feature2D>()) as IList<Feature2D>;

            Assert.IsNotNull(importedFeatures);
            Assert.Greater(importedFeatures.Count, 0);
        }
    }
}
