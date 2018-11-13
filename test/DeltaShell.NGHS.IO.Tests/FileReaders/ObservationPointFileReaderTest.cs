using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class ObservationPointFileReaderTest
    {
        private IHydroNetwork originalNetwork;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test, Category(TestCategory.DataAccess)]
        public void GivenAnObservationPointsFile_WhenReadingIt_ThenAllObservationPointsShouldBeCreated()
        {
            var testFile = TestHelper.GetTestFilePath(@"ObservationPoints.ini");

            try
            {
                var categories = new List<DelftIniCategory>();
                var category = new DelftIniCategory(ObservationPointRegion.IniHeader);

                category.AddProperty(LocationRegion.Id.Key, "observationpoint1");
                category.AddProperty(LocationRegion.Chainage.Key, "50");
                category.AddProperty(LocationRegion.BranchId.Key, "branch");
                category.AddProperty(LocationRegion.Name.Key, "observationpoint1");

                categories.Add(category);

                new IniFileWriter().WriteIniFile(categories, testFile);

                var errorReport = new List<string>();

                Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                    errorReport.Add(
                        $"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

                var reader = new ObservationPointFileReader(CreateAndAddErrorReport);
                var allObservationPoints = reader.ReadObservationPoints(testFile, originalNetwork);

                Assert.AreEqual(1, allObservationPoints.Count);

                Assert.AreEqual("observationpoint1", allObservationPoints[0].Name);
                Assert.AreEqual("branch", allObservationPoints[0].Branch.Name);
                Assert.AreEqual(50, allObservationPoints[0].Chainage);
                Assert.AreEqual("observationpoint1", allObservationPoints[0].LongName);

                Assert.AreEqual(0, errorReport.Count);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFile);
            }
        }

        [Test, Category(TestCategory.DataAccess)]
        public void GivenAnEmptyObservationPointsFile_WhenReadingIt_ThenAnErrorReportShouldBeCreated()
        {
            var testFile = TestHelper.GetTestFilePath(@"ObservationPoints.ini");

            try
            {
                var categories = new List<DelftIniCategory>();
                
                new IniFileWriter().WriteIniFile(categories, testFile);

                var errorReport = new List<string>();

                Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                    errorReport.Add(
                        $"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

                var reader = new ObservationPointFileReader(CreateAndAddErrorReport);
                var allObservationPoints = reader.ReadObservationPoints(testFile, originalNetwork);

                Assert.AreEqual(0, allObservationPoints.Count);
                Assert.AreEqual(1, errorReport.Count);

                var expectedMessage =
                    string.Format(
                        "While reading the observation points from file, an error occured:{0} Could not read file {1} properly, it seems empty", Environment.NewLine, testFile);

                Assert.AreEqual(expectedMessage, errorReport[0]);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFile);
            }
        }
    }
}