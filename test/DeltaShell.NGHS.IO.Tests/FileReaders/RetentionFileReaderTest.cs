using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders.Retention;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.IO.TestUtils;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class RetentionFileReaderTest
    {
        private IHydroNetwork originalNetwork;
        private IList<IChannel> channelsList;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch("node1", "node2", "Channel1");
            channelsList = originalNetwork.Channels.ToList();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenARetentionIniFile_WhenReadingRetentionPointsFromFile_ThenTheyAreCorrectlySetOnTheModel()
        {
            var errorReport = new List<string>();

            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add(
                    $"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            var fileReader = new RetentionFileReader(CreateAndAddErrorReport);
            var filePath = TestHelper.GetTestFilePath(Path.Combine("FileReaders", "RetentionFileReader", "Retention.ini"));
            var testFile = TestHelper.CreateLocalCopy(filePath);

            try
            {
                var retention = fileReader.ReadRetention(testFile, channelsList);

                Assert.AreEqual(2, retention.Count);

                Assert.AreEqual("Retention1", retention[0].Name);
                Assert.AreEqual("Channel1", retention[0].Branch.Name);
                Assert.AreEqual(800.0, retention[0].Chainage);
                Assert.AreEqual(RetentionType.Reservoir, retention[0].Type);
                Assert.AreEqual(false, retention[0].UseTable);
                Assert.AreEqual(4.0, retention[0].BedLevel);
                Assert.AreEqual(1000000.0, retention[0].StorageArea);
                Assert.AreEqual(4.0, retention[0].StreetLevel);
                Assert.AreEqual(1000000.0, retention[0].StreetStorageArea);

                Assert.AreEqual("Retention2", retention[1].Name);
                Assert.AreEqual("Channel1", retention[1].Branch.Name);
                Assert.AreEqual(1500.0, retention[1].Chainage);
                Assert.AreEqual(RetentionType.Reservoir, retention[1].Type);
                Assert.AreEqual(false, retention[1].UseTable);
                Assert.AreEqual(3.0, retention[1].BedLevel);
                Assert.AreEqual(500000.0, retention[1].StorageArea);
                Assert.AreEqual(3.0, retention[1].StreetLevel);
                Assert.AreEqual(500000.0, retention[1].StreetStorageArea);

                Assert.AreEqual(0, errorReport.Count);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFile);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnEmptyRetentionIniFile_WhenReadingRetentionPointsFromFile_ThenAnErrorShouldBeCreated()
        {
            var errorReport = new List<string>();

            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add(
                    $"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            var fileReader = new RetentionFileReader(CreateAndAddErrorReport);
            var filePath =
                TestHelper.GetTestFilePath(Path.Combine("FileReaders", "RetentionFileReader", "EmptyRetention.ini"));
            var testFile = TestHelper.CreateLocalCopy(filePath);

            try
            {
                fileReader.ReadRetention(testFile, channelsList);
                var expectedMessage =
                    string.Concat(Resources.RetentionFileReader_ReadRetention_While_reading_the_retention_from_file__an_error_occured,
                        ":",
                        Environment.NewLine,
                        $" Could not read file {testFile} properly, it seems empty");

                Assert.AreEqual(expectedMessage, errorReport[0]);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFile);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenARetentionIniFileWithAnUnknownBranch_WhenReadingRetentionPointsFromFile_ThenAnErrorShouldBeCreated()
        {
            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                new List<string>().Add(
                    $"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            var fileReader = new RetentionFileReader(CreateAndAddErrorReport);
            var filePath =
                TestHelper.GetTestFilePath(Path.Combine("FileReaders", "RetentionFileReader", "RetentionUnknownBranch.ini"));
            var testFile = TestHelper.CreateLocalCopy(filePath);

            try
            {
                var expectedMessage = string.Format(
                    Resources
                        .RetentionConverter_ConvertToRetention_Unable_to_parse__0__property___1___Branch_not_found_in_Network__2_,
                    testFile,
                    RetentionRegion.BranchId.Key, Environment.NewLine);
                Assert.Throws<Exception>(() => fileReader.ReadRetention(testFile, channelsList), expectedMessage);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFile);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenARetentionIniFileWithUseTableSetToTrue_WhenReadingRetentionPointsFromFile_ThenAnErrorShouldBeCreated()
        {
            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                new List<string>().Add(
                    $"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            var fileReader = new RetentionFileReader(CreateAndAddErrorReport);
            var filePath =
                TestHelper.GetTestFilePath(Path.Combine("FileReaders", "RetentionFileReader", "RetentionUseTableTrue.ini"));
            var testFile = TestHelper.CreateLocalCopy(filePath);

            try
            {
                var expectedMessage = Resources
                    .RetentionConverterTest_GivenARetentionDataModelWhichUsesUseTable_WhenConverting_ThenTheErrorReportIsProperlyFilled_UseTable_is_not_yet_implemented_in_the_RetentionFileReader__please_set_UseTable_to_0_to_continue_with_this_model_;
                Assert.Throws<NotImplementedException>(() => fileReader.ReadRetention(testFile, channelsList), expectedMessage);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFile);
            }
        }
    }
}
