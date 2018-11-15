using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class LateralSourceFileReaderTest
    {
        private IHydroNetwork originalNetwork;
        private IList<IChannel> channelsList;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
            channelsList = originalNetwork.Channels.ToList();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test, Category(TestCategory.DataAccess)]
        public void GivenALateralSourcesFile_WhenReadingIt_ThenAllLateralSourcesShouldBeCreated()
        {
            var testFile = TestHelper.GetTestFilePath(@"LateralDischargeLocations.ini");

            try
            {
                var categories = new List<DelftIniCategory>();
                var Category = new DelftIniCategory(BoundaryRegion.LateralDischargeHeader);

                Category.AddProperty(LocationRegion.Id.Key, "lateraldischarge1");
                Category.AddProperty(LocationRegion.Chainage.Key, "50");
                Category.AddProperty(LocationRegion.BranchId.Key, "branch");
                Category.AddProperty(LocationRegion.Name.Key, "lateraldischarge1");
                Category.AddProperty(LateralSourceLocationRegion.Length.Key, "2");

                categories.Add(Category);

                new IniFileWriter().WriteIniFile(categories, testFile);

                var errorReport = new List<string>();

                Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                    errorReport.Add(
                        $"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

                var reader = new LateralSourceFileReader(CreateAndAddErrorReport);
                var filePath = TestHelper.GetTestFilePath(Path.Combine("LateralSourcesAndObservationPoints",
                    "LateralDischargeLocations.ini"));
                var allLateralSources = reader.ReadLateralSources(testFile, channelsList);

                Assert.AreEqual(1, allLateralSources.Count);

                Assert.AreEqual("lateraldischarge1", allLateralSources[0].Name);
                Assert.AreEqual("branch", allLateralSources[0].Branch.Name);
                Assert.AreEqual(50, allLateralSources[0].Chainage);
                Assert.AreEqual("lateraldischarge1", allLateralSources[0].LongName);
                Assert.AreEqual(2, allLateralSources[0].Length);

                Assert.AreEqual(0, errorReport.Count);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFile);
            }
        }

        [Test, Category(TestCategory.DataAccess)]
        public void GivenAnEmptyLateralSourcesFile_WhenReadingIt_ThenAnErrorReportShouldBeCreated()
        {
            var testFile = TestHelper.GetTestFilePath(@"LateralDischargeLocations.ini");

            try
            {
                var categories = new List<DelftIniCategory>();

                new IniFileWriter().WriteIniFile(categories, testFile);

                var errorReport = new List<string>();

                Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                    errorReport.Add(
                        $"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

                var reader = new LateralSourceFileReader(CreateAndAddErrorReport);
                var allLateralSources = reader.ReadLateralSources(testFile, channelsList);

                Assert.AreEqual(0, allLateralSources.Count);
                Assert.AreEqual(1, errorReport.Count);

                var expectedMessage =
                    string.Format(
                        "While reading the lateral sources from file, an error occured:{0} Could not read file {1} properly, it seems empty", Environment.NewLine, testFile);

                Assert.AreEqual(expectedMessage, errorReport[0]);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFile);
            }
        }
    }
}