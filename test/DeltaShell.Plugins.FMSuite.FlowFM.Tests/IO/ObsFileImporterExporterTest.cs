using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    [Category(TestCategory.DataAccess)]
    internal class ObsFileImporterExporterTest
    {
        [Test]
        public void ObsFileExportImportForGroupableFeaturesTest()
        {
            string filePath = string.Concat(Path.GetTempFileName(), ".xyn");

            List<GroupableFeature2DPoint> points = CreateObservationPoints<GroupableFeature2DPoint>(10);

            var obsFileImporterExporter = new ObsFileImporterExporter<GroupableFeature2DPoint>();

            try
            {
                obsFileImporterExporter.Export(points, filePath);
                var importedPoints = (List<GroupableFeature2DPoint>) obsFileImporterExporter.ImportItem(filePath);

                Assert.AreEqual(points.Count, importedPoints.Count);

                for (var i = 0; i < importedPoints.Count; ++i)
                {
                    Assert.AreEqual(points[i].Name, importedPoints[i].Name);
                    Assert.AreEqual(points[i].Geometry, importedPoints[i].Geometry);
                    Assert.That(importedPoints[i].GroupName, Is.EqualTo(filePath.Replace(@"\", "/")));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(filePath);
            }
        }

        [Test]
        public void ObsFileExportImportFeaturePoint2DTest()
        {
            string filePath = string.Concat(Path.GetTempFileName(), ".xyn");

            List<Feature2DPoint> points = CreateObservationPoints<Feature2DPoint>(10);

            var obsFileImporterExporter = new ObsFileImporterExporter<Feature2DPoint>();

            try
            {
                obsFileImporterExporter.Export(points, filePath);
                var importedPoints = (List<Feature2DPoint>) obsFileImporterExporter.ImportItem(filePath);

                Assert.AreEqual(points.Count, importedPoints.Count);

                for (var i = 0; i < importedPoints.Count; ++i)
                {
                    Assert.AreEqual(points[i].Name, importedPoints[i].Name);
                    Assert.AreEqual(points[i].Geometry, importedPoints[i].Geometry);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(filePath);
            }
        }

        [Test]
        public void ObsFileExportImportGroupableFeature2DPointAndReplaceDuplicatesWithEqualGroupNameAndPointName()
        {
            string filePathGroupA = TestHelper.GetTestFilePath("observationpointGroups\\GroupA.xyn");
            Assert.NotNull(filePathGroupA);
            Assert.IsTrue(File.Exists(filePathGroupA));
            string obsFileGroupA = TestHelper.CreateLocalCopy(filePathGroupA).Replace(@"\", "/");

            string filePathGroupB = TestHelper.GetTestFilePath("observationpointGroups\\GroupB.xyn");
            Assert.NotNull(filePathGroupB);
            Assert.IsTrue(File.Exists(filePathGroupB));
            string obsFileGroupB = TestHelper.CreateLocalCopy(filePathGroupB).Replace(@"\", "/");

            List<GroupableFeature2DPoint> pointsGroupA = CreateObservationPoints<GroupableFeature2DPoint>(5);
            foreach (GroupableFeature2DPoint point in pointsGroupA)
            {
                point.GroupName = obsFileGroupA;
            }

            List<GroupableFeature2DPoint> pointsGroupB = CreateObservationPoints<GroupableFeature2DPoint>(5);
            foreach (GroupableFeature2DPoint point in pointsGroupB)
            {
                point.GroupName = obsFileGroupB;
            }

            var allPoints = new List<GroupableFeature2DPoint>();
            allPoints.AddRange(pointsGroupA);
            allPoints.AddRange(pointsGroupB);

            var obsFileImporterExporter = new ObsFileImporterExporter<GroupableFeature2DPoint>() {EqualityComparer = new GroupableFeatureComparer<GroupableFeature2DPoint>()};

            try
            {
                var pointsAfterImportFromGroupA =
                    (List<GroupableFeature2DPoint>) obsFileImporterExporter.ImportItem(obsFileGroupA, allPoints);
                Assert.AreEqual(11, pointsAfterImportFromGroupA.Count);

                var pointsAfterImportFromGroupB =
                    (List<GroupableFeature2DPoint>) obsFileImporterExporter.ImportItem(obsFileGroupB, allPoints);
                Assert.AreEqual(13, pointsAfterImportFromGroupB.Count);

                // check if there are indeed no duplicates names
                Assert.AreEqual(1, pointsAfterImportFromGroupB.Count(p => p.Name == "ObservationPoint1" && p.GroupName == obsFileGroupA));
                Assert.AreEqual(13, pointsAfterImportFromGroupB.Select(i => i.Name).Distinct().Count());
            }
            finally
            {
                FileUtils.DeleteIfExists(obsFileGroupA);
                FileUtils.DeleteIfExists(obsFileGroupB);
            }
        }
        
        [Test]
        [TestCase("(20,40)")]
        [TestCase("Name with spaces")]
        [TestCase("\tname_with_tab")]
        [TestCase("\t name with tab and space and a comma")]
        [TestCase("\"NameStartingWithADoubleQuote")]
        [TestCase("NameEndingWithADoubleQuote\"")]
        [TestCase("NameEndingWithAWithADoubleQuote\"InTheMiddle")]
        [TestCase("\"")]
        public void ObsFile_names_exports_names_with_whitespace_or_commas_in_quotes(string name)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string filePath = Path.Combine(tempDir.Path, ".xyn");

                List<Feature2DPoint> points = CreateObservationPoints<Feature2DPoint>(9);
                var point = new Feature2DPoint()
                {
                    Geometry = new Point(0.123, 0.456),
                    Name = name
                };
                points.Add(point);

                var obsFileImporterExporter = new ObsFileImporterExporter<Feature2DPoint>();

                obsFileImporterExporter.Export(points, filePath);
                string fileContent = File.ReadAllText(filePath);

                const char quote = '\'';
                Assert.AreEqual(2, fileContent.Count(f => (f == quote)), "More or less than one name is quoted");
                Assert.IsTrue(fileContent.Contains("'" + name + "'"), "Name is not found in quotes");
            }
        }
        
        [Test]
        [TestCase("NormalName")]
        [TestCase(@"NameWithéóÄ")]
        public void ObsFile_names_exports_names_without_whitespace_or_commas_without_quotes(string name)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string filePath = Path.Combine(tempDir.Path, ".xyn");

                List<Feature2DPoint> points = CreateObservationPoints<Feature2DPoint>(9);
                var point = new Feature2DPoint()
                {
                    Geometry = new Point(0.123, 0.456),
                    Name = name
                };
                points.Add(point);

                var obsFileImporterExporter = new ObsFileImporterExporter<Feature2DPoint>();

                obsFileImporterExporter.Export(points, filePath);
                string fileContent = File.ReadAllText(filePath);

                const char quote = '\'';

                Assert.AreEqual(0, fileContent.Count(f => (f == quote)), "One or more names are quoted");
                Assert.IsTrue(fileContent.Contains(name), "Name is not found");
            }
        }


        private List<T> CreateObservationPoints<T>(int numberOfPoints) where T : Feature2DPoint, new()
        {
            var rnd = new Random();
            var list = new List<T>();

            for (var i = 0; i < numberOfPoints; ++i)
            {
                list.Add(
                    new T
                    {
                        Geometry = new Point(rnd.Next(0, 1000), rnd.Next(0, 1000)),
                        Name = "ObservationPoint" + i
                    }
                );
            }

            return list;
        }
    }
}