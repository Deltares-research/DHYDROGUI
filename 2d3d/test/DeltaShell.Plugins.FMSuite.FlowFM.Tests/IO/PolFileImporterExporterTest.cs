using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    internal class PolFileImporterExporterTest
    {
        [Test]
        [TestCase("enclosureFiles\\threeEnclosuresDifferentName_enc.pol", 3)]
        [TestCase("enclosureFiles\\threeEnclosuresSameName_enc.pol", 3)]
        public void ImportMultipleEnclosuresWhenNoPreviousAreCreated(string polFileLocation, int expectedEnclosures)
        {
            string filePath = TestHelper.GetTestFilePath(polFileLocation);
            Assert.NotNull(filePath);
            Assert.IsTrue(File.Exists(filePath));
            string polFilePath = TestHelper.CreateLocalCopy(filePath);
            try
            {
                var importer = new PolFileImporterExporter();
                var area = new HydroArea();

                Assert.AreEqual(0, area.Enclosures.Count);
                importer.ImportItem(polFilePath, area.Enclosures);
                Assert.AreEqual(expectedEnclosures, area.Enclosures.Count);
                Assert.AreEqual(expectedEnclosures, area.Enclosures.Select(w => w.Name).Distinct().Count(), "All names should be unique");
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            finally
            {
                FileUtils.DeleteIfExists(polFilePath);
            }
        }

        [Test]
        [TestCase("enclosureFiles\\threeEnclosuresDifferentName_enc.pol", 3)] /*Enclosure01, Enclosure02, Enclosure03*/
        [TestCase("enclosureFiles\\threeEnclosuresSameName_enc.pol", 1)]      /*Enclosure01, Enclosure01, Enclosure01*/
        public void ImportMultipleEnclosuresWhenThereAreAlreadyCreated(string polFileLocation, int expectedEnclosures)
        {
            string filePath = TestHelper.GetTestFilePath(polFileLocation);
            Assert.NotNull(filePath);
            Assert.IsTrue(File.Exists(filePath));
            string polFilePath = TestHelper.CreateLocalCopy(filePath);

            try
            {
                var importer = new PolFileImporterExporter();
                var area = new HydroArea();

                GroupableFeature2DPolygon newEnclosure = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                    "Enclosure01" /* This name is present in all files above*/,
                    FlowFMTestHelper.GetValidGeometryForEnclosureExample());
                area.Enclosures.Add(newEnclosure);
                Assert.AreEqual(1, area.Enclosures.Count);

                importer.ImportItem(polFilePath, area.Enclosures);
                Assert.AreEqual(expectedEnclosures, area.Enclosures.Count);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            finally
            {
                FileUtils.DeleteIfExists(polFilePath);
            }
        }

        [Test]
        public void ExportImportEnclosure()
        {
            string filePath = string.Concat(Path.GetTempFileName(), ".pol");
            Assert.NotNull(filePath);

            var featureName = "Enclosure01";
            Polygon enclosurePolygonToWrite = FlowFMTestHelper.GetValidGeometryForEnclosureExample();
            GroupableFeature2DPolygon polygonFeature = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(featureName, enclosurePolygonToWrite);
            var enclosureFeatureList = new List<Feature2DPolygon> {polygonFeature};

            try
            {
                var importerExporter = new PolFileImporterExporter();
                var area = new HydroArea();

                GroupableFeature2DPolygon newEnclosure = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                    "Enclosure01" /* This name is present in all files above*/,
                    FlowFMTestHelper.GetValidGeometryForEnclosureExample());
                area.Enclosures.Add(newEnclosure);
                Assert.AreEqual(1, area.Enclosures.Count);

                Assert.IsTrue(importerExporter.Export(area.Enclosures, filePath));
                Assert.IsTrue(File.Exists(filePath));
                string writtenFile = File.ReadAllText(filePath);
                Assert.NotNull(writtenFile);
                Assert.IsNotEmpty(writtenFile);
                Assert.AreEqual(FlowFMTestHelper.GetExpectedEnclosurePolFileContent(featureName), writtenFile);

                area.Enclosures.Clear();
                Assert.AreEqual(0, area.Enclosures.Count);
                importerExporter.ImportItem(filePath, area.Enclosures);
                Assert.AreEqual(1, area.Enclosures.Count);

                Feature2DPolygon importedFeature = enclosureFeatureList[0];
                Assert.AreEqual(featureName, importedFeature.Name);
                Assert.AreEqual(newEnclosure.Geometry, importedFeature.Geometry);
            }
            finally
            {
                FileUtils.DeleteIfExists(filePath);
            }
        }
    }
}