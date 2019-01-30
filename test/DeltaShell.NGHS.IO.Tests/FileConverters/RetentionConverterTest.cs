using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Retention;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.IO.TestUtils;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileConverters
{
    [TestFixture]
    public class RetentionConverterTest
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
        public void GivenARetentionDataModel_WhenConverting_ThenAListOfRetentionsIsReturned()
        {
            // Given
            var errorReport = new List<string>();
            var retentionProperties = new List<RetentionPropertiesDTO>();

            var retentionProperty1 = GenerateDefaultRetentionPropertiesDto();

            var retentionProperty2 = new RetentionPropertiesDTO
            {
                Id = "Retention2",
                BranchName = "Channel1",
                Branch = channelsList.FirstOrDefault(c => c.Name == "Channel1"),
                Chainage = 1500.0,
                StorageType = RetentionType.Reservoir,
                UseTable = false,
                BedLevel = 3.0,
                StorageArea = 500000.0,
                StreetLevel = 3.0,
                StreetStorageArea = 500000.0
            };

            retentionProperties.Add(retentionProperty1);
            retentionProperties.Add(retentionProperty2);

            // When
            var retention = RetentionConverter.Convert(retentionProperties, errorReport);

            // Then
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
        }

        private RetentionPropertiesDTO GenerateDefaultRetentionPropertiesDto()
        {
            var retentionProperty = new RetentionPropertiesDTO
            {
                Id = "Retention1",
                BranchName = "Channel1",
                Branch = channelsList.FirstOrDefault(c => c.Name == "Channel1"),
                Chainage = 800.0,
                StorageType = RetentionType.Reservoir,
                UseTable = false,
                BedLevel = 4.0,
                StorageArea = 1000000.0,
                StreetLevel = 4.0,
                StreetStorageArea = 1000000.0
            };
            return retentionProperty;
        }

        [Test]
        public void GivenARetentionDataModelWithDuplicateRetentionIds_WhenConverting_ThenTheErrorReportIsProperlyFilled()
        {
            var errorReport = new List<string>();
            var retentionProperties = new List<RetentionPropertiesDTO>();

            var retentionProperty1 = GenerateDefaultRetentionPropertiesDto();
            var retentionProperty2 = GenerateDefaultRetentionPropertiesDto();

            retentionProperties.Add(retentionProperty1);
            retentionProperties.Add(retentionProperty2);

            RetentionConverter.Convert(retentionProperties, errorReport);

            Assert.That(errorReport.Count, Is.EqualTo(1));
            Assert.That(errorReport[0], Is.EqualTo(string.Format(
                Resources.RetentionConverter_ValidateConvertedRetention_Retention_point_with_id__0__already_exists__there_cannot_be_any_duplicate_retention_ids__1_,
                retentionProperty1.Id, Environment.NewLine)));
        }
    }
}