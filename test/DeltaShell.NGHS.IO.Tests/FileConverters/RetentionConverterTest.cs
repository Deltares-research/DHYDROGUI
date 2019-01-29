using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Retention;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;
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
            var categories = new List<DelftIniCategory>();

            var category1 = PopulateDelftIniCategory();
            var category2 = new DelftIniCategory(RetentionRegion.Header);

            category2.AddProperty(RetentionRegion.Id.Key, "Retention2");
            category2.AddProperty(RetentionRegion.BranchId.Key, "Channel1");
            category2.AddProperty(RetentionRegion.Chainage.Key, 1500.0);
            category2.AddProperty(RetentionRegion.StorageType.Key, "Reservoir");
            category2.AddProperty(RetentionRegion.UseTable.Key, 0);
            category2.AddProperty(RetentionRegion.BedLevel.Key, 3.0);
            category2.AddProperty(RetentionRegion.Area.Key, 500000.0);
            category2.AddProperty(RetentionRegion.StreetLevel.Key, 3.0);
            category2.AddProperty(RetentionRegion.StreetStorageArea.Key, 500000.0);

            categories.Add(category1);
            categories.Add(category2);

            // When
            var retention = RetentionConverter.Convert(categories, channelsList, errorReport);

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

        [Test]
        public void GivenARetentionDataModelWithANonExistingBranch_WhenConverting_ThenTheErrorReportIsProperlyFilled()
        {
            var errorReport = new List<string>();
            var categories = new List<DelftIniCategory>();

            var category1 = PopulateDelftIniCategory();
            
            //Remove and re-add the property that we want to test.
            category1.RemoveProperty(category1.Properties.Single(p => p.Name == RetentionRegion.BranchId.Key));
            category1.AddProperty(RetentionRegion.BranchId.Key, "AllYourBranchAreBelongToUs");

            categories.Add(category1);

            RetentionConverter.Convert(categories, channelsList, errorReport);

            Assert.That(errorReport.Count, Is.EqualTo(1));
            Assert.That(errorReport[0],
                Is.EqualTo(string.Format(Resources.RetentionConverter_ConvertToRetention_Unable_to_parse__0__property___1___Branch_not_found_in_Network__2_, category1.Name,
                        RetentionRegion.BranchId.Key, Environment.NewLine)));
        }

        [Test]
        public void GivenARetentionDataModelWhichUsesUseTable_WhenConverting_ThenTheErrorReportIsProperlyFilled()
        {
            var errorReport = new List<string>();
            var categories = new List<DelftIniCategory>();

            var category1 = PopulateDelftIniCategory();

            //Remove and re-add the property that we want to test.
            category1.RemoveProperty(category1.Properties.Single(p => p.Name == RetentionRegion.UseTable.Key));
            category1.AddProperty(RetentionRegion.UseTable.Key, 1);

            categories.Add(category1);

            RetentionConverter.Convert(categories, channelsList, errorReport);

            Assert.That(errorReport.Count, Is.EqualTo(1));
            Assert.That(errorReport[0],
                Is.EqualTo(Resources.RetentionConverterTest_GivenARetentionDataModelWhichUsesUseTable_WhenConverting_ThenTheErrorReportIsProperlyFilled_UseTable_is_not_yet_implemented_in_the_RetentionFileReader__please_set_UseTable_to_0_to_continue_with_this_model_));
        }

        [Test]
        public void GivenARetentionDataModelWithDuplicateRetentionIds_WhenConverting_ThenTheErrorReportIsProperlyFilled()
        {
            var errorReport = new List<string>();
            var categories = new List<DelftIniCategory>();

            var category1 = PopulateDelftIniCategory();
            var category2 = PopulateDelftIniCategory();

            categories.Add(category1);
            categories.Add(category2);

            RetentionConverter.Convert(categories, channelsList, errorReport);

            Assert.That(errorReport.Count, Is.EqualTo(1));
            Assert.That(errorReport[0], Is.EqualTo(string.Format(
                Resources.RetentionConverter_ValidateConvertedRetention_Retention_point_with_id__0__already_exists__there_cannot_be_any_duplicate_retention_ids__1_,
                category1.Properties[0].Value, Environment.NewLine)));
        }

        private static DelftIniCategory PopulateDelftIniCategory()
        {
            var category = new DelftIniCategory(RetentionRegion.Header);

            category.AddProperty(RetentionRegion.Id.Key, "Retention1");
            category.AddProperty(RetentionRegion.BranchId.Key, "Channel1");
            category.AddProperty(RetentionRegion.Chainage.Key, 800.0);
            category.AddProperty(RetentionRegion.StorageType.Key, "Reservoir");
            category.AddProperty(RetentionRegion.UseTable.Key, 0);
            category.AddProperty(RetentionRegion.BedLevel.Key, 4.0);
            category.AddProperty(RetentionRegion.Area.Key, 1000000.0);
            category.AddProperty(RetentionRegion.StreetLevel.Key, 4.0);
            category.AddProperty(RetentionRegion.StreetStorageArea.Key, 1000000.0);
            return category;
        }
    }
}