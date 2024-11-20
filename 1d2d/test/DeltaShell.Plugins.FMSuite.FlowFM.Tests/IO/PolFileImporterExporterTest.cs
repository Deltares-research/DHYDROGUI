using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    class PolFileImporterExporterTest
    {
        [Test]
        [TestCase("enclosureFiles\\threeEnclosuresDifferentName_enc.pol", 3)]
        [TestCase("enclosureFiles\\threeEnclosuresSameName_enc.pol", 3)]
        public void ImportMultipleEnclosuresWhenNoPreviousAreCreated(string polFileLocation, int expectedEnclosures)
        {
            var filePath = TestHelper.GetTestFilePath(polFileLocation);
            Assert.NotNull(filePath);
            Assert.IsTrue(File.Exists(filePath));
            var polFilePath = TestHelper.CreateLocalCopy(filePath);
            try
            {
                var importer = new PolFileImporterExporter();
                var area = new HydroArea();

                Assert.AreEqual(0, area.Enclosures.Count);
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
        [TestCase("enclosureFiles\\threeEnclosuresDifferentName_enc.pol", 3)] /*Enclosure01, Enclosure02, Enclosure03*/
        [TestCase("enclosureFiles\\threeEnclosuresSameName_enc.pol", 1)] /*Enclosure01, Enclosure01, Enclosure01*/
        public void ImportMultipleEnclosuresWhenThereAreAlreadyCreated(string polFileLocation, int expectedEnclosures)
        {
            var filePath = TestHelper.GetTestFilePath(polFileLocation);
            Assert.NotNull(filePath);
            Assert.IsTrue(File.Exists(filePath));
            var polFilePath = TestHelper.CreateLocalCopy(filePath);

            try
            {
                var importer = new PolFileImporterExporter();
                var area = new HydroArea();

                var newEnclosure = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
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

            var filePath = String.Concat(Path.GetTempFileName(), ".pol");
            Assert.NotNull(filePath);

            var featureName = "Enclosure01";
            var enclosurePolygonToWrite = FlowFMTestHelper.GetValidGeometryForEnclosureExample();
            var polygonFeature = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(featureName, enclosurePolygonToWrite);
            var enclosureFeatureList = new List<Feature2DPolygon> { polygonFeature };

            try
            {
                var importerExporter = new PolFileImporterExporter();
                var area = new HydroArea();

                var newEnclosure = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                    "Enclosure01" /* This name is present in all files above*/,
                    FlowFMTestHelper.GetValidGeometryForEnclosureExample());
                area.Enclosures.Add(newEnclosure);
                Assert.AreEqual(1, area.Enclosures.Count);

                Assert.IsTrue(importerExporter.Export(area.Enclosures, filePath));
                Assert.IsTrue(File.Exists(filePath));
                var writtenFile = File.ReadAllText(filePath);
                Assert.NotNull(writtenFile);
                Assert.IsNotEmpty(writtenFile);
                Assert.AreEqual(FlowFMTestHelper.GetExpectedEnclosurePolFileContent(featureName), writtenFile);

                area.Enclosures.Clear();
                Assert.AreEqual(0, area.Enclosures.Count);
                importerExporter.ImportItem(filePath, area.Enclosures);
                Assert.AreEqual(1, area.Enclosures.Count);
                
                var importedFeature = enclosureFeatureList[0];
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
