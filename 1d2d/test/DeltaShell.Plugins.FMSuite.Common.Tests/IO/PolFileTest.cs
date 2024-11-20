using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class PolFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadGroupableFeaturePolFileAssignsGroupName()
        {
            var groupName = "DryGroup1_dry.pol";
            var filePath = TestHelper.GetTestFilePath(Path.Combine(@"HydroAreaCollection", groupName));
            Assert.IsTrue(File.Exists(filePath));
            filePath = TestHelper.CreateLocalCopy(filePath);
            try
            {
                var polFile = new PolFile<GroupableFeature2DPolygon>();
                var readObjects = polFile.Read(filePath);
                var groups = readObjects.GroupBy(g => g.GroupName).ToList();
                Assert.That(groups.Count, Is.EqualTo(1));
                Assert.That(groups.First().Key, Is.EqualTo(filePath.Replace(@"\", "/")));
            }
            finally
            {
                FileUtils.DeleteIfExists(filePath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadEnclosurePolFile()
        {
            var polFilePath = TestHelper.GetTestFilePath(@"enclosureFiles\FlowFM_enc.pol");
            Assert.IsTrue(File.Exists(polFilePath));
            polFilePath = TestHelper.CreateLocalCopy(polFilePath);

            var polFile = new PolFile<Feature2DPolygon>();
            var enclosures = polFile.Read(polFilePath);
            Assert.AreEqual(1, enclosures.Count);

            AssertGeometryReadAsExpected(
                enclosures,
                FMSuiteCommonTestHelper.GetValidGeometryForEnclosureExample());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadThreeEnclosuresWithSameNameFromPolFile()
        {
            var polFilePath = TestHelper.GetTestFilePath(@"enclosureFiles\threeEnclosuresSameName_enc.pol");
            try
            {
                Assert.IsTrue(File.Exists(polFilePath));
                polFilePath = TestHelper.CreateLocalCopy(polFilePath);

                var polFile = new PolFile<Feature2DPolygon>();
                var enclosures = polFile.Read(polFilePath);
                Assert.AreEqual(3, enclosures.Count);
                AssertGeometryReadAsExpected(
                    enclosures,
                    FMSuiteCommonTestHelper.GetValidGeometryForEnclosureExample());
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
        [Category(TestCategory.DataAccess)]
        public void ReadThreeEnclosuresWithDifferentNameFromPolFile()
        {
            var polFilePath = TestHelper.GetTestFilePath(@"enclosureFiles\threeEnclosuresDifferentName_enc.pol");
            try
            {
                Assert.IsTrue(File.Exists(polFilePath));
                polFilePath = TestHelper.CreateLocalCopy(polFilePath);

                var polFile = new PolFile<Feature2DPolygon>();
                var enclosures = polFile.Read(polFilePath);
                Assert.AreEqual(3, enclosures.Count);
                AssertGeometryReadAsExpected(
                    enclosures,
                    FMSuiteCommonTestHelper.GetValidGeometryForEnclosureExample());
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

        private void AssertGeometryReadAsExpected(IList<Feature2DPolygon> enclosures, Polygon expectedGeometry)
        {
            foreach (var enclosure in enclosures)
            {
                var enclosurePolygon = enclosure.Geometry as Polygon;
                Assert.NotNull(enclosurePolygon);

                Assert.IsTrue(enclosurePolygon.Holes.Length == 0);
                Assert.AreEqual(expectedGeometry, enclosurePolygon);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteEnclosurePolFile()
        {
            var filePath = String.Concat(Path.GetTempFileName(), ".pol");
            try
            {
                var featureName = "Enclosure01";
                var enclosurePolygonToWrite = FMSuiteCommonTestHelper.GetValidGeometryForEnclosureExample();
                var polygonFeature = FMSuiteCommonTestHelper.CreateFeature2DPolygonFromGeometry(featureName, enclosurePolygonToWrite);
                var featuresList = new List<Feature2DPolygon>{ polygonFeature };

                var polFile = new PolFile<Feature2DPolygon>();
                polFile.Write(filePath, featuresList);
                Assert.IsTrue(File.Exists(filePath));

                var writtenFile = File.ReadAllText(filePath);
                Assert.NotNull(writtenFile);
                Assert.IsNotEmpty(writtenFile);
                //We have other tests to check the object being read.
                Assert.AreEqual(FMSuiteCommonTestHelper.GetExpectedEnclosurePolFileContent(featureName), writtenFile);
            }
            finally 
            {
                FileUtils.DeleteIfExists(filePath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteValidEnclosurePolFileThenReadIt()
        {
            var filePath = String.Concat(Path.GetTempFileName(), ".pol");
            try
            {
                var enclosurePolygonToWrite = FMSuiteCommonTestHelper.GetValidGeometryForEnclosureExample();
                var polygonFeature = FMSuiteCommonTestHelper.CreateFeature2DPolygonFromGeometry("Enclosure01", enclosurePolygonToWrite);
                var featuresList = new List<Feature2DPolygon> { polygonFeature };

                /* Note, the write and read itself of the files are tested above, this is for the round-trip. */
                var polFile = new PolFile<Feature2DPolygon>();
                polFile.Write(filePath, featuresList);
                var listOfFeatures = polFile.Read(filePath);

                Assert.NotNull(listOfFeatures);
                var readFeature = listOfFeatures[0];
                Assert.AreNotEqual(readFeature, polygonFeature);

                var enclosurePolygonRead = listOfFeatures[0].Geometry;
                Assert.AreEqual(enclosurePolygonRead.Coordinates, enclosurePolygonToWrite.Coordinates);
            }
            finally
            {
                FileUtils.DeleteIfExists(filePath);
            }
        }
    }
}