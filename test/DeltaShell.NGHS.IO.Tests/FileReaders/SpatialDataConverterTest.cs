using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileReaders.SpatialData;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.IO.TestUtils;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class SpatialDataConverterTest
    {
        private IHydroNetwork originalNetwork;
        private IList<IChannel> channelsList;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch("node1", "node2", "Maasmond");
            channelsList = originalNetwork.Channels.ToList();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void GivenAListOfDelftIniCategories_WhenConverting_ThenANetworkCoverageIsReturned()
        {
            var categories = new List<DelftIniCategory>();

            var validContentIniHeader = CreateValidContentIniHeader("InitialWaterLevel", "1");
            categories.Add(validContentIniHeader);

            var firstCategory = CreateValidDefinitionIniHeader("Maasmond", 0.0, 25.0);
            var secondCategory = CreateValidDefinitionIniHeader("Maasmond", 270.0, 30.2);
            categories.Add(firstCategory);
            categories.Add(secondCategory);

            var errorMessages = new List<string>();
            var networkCoverage = SpatialDataConverter.Convert(categories, channelsList, errorMessages);

            Assert.IsNotNull(networkCoverage);
            Assert.AreEqual(typeof(NetworkCoverage), networkCoverage.GetType());
            Assert.AreEqual(2, networkCoverage.Arguments[0].Values.Count);
            Assert.AreEqual(2, networkCoverage.Components[0].Values.Count);
            Assert.AreEqual(0, errorMessages.Count);
        }

        [TestCase("branchId")]
        [TestCase("chainage")]
        public void
            GivenACategoryWithAnSpatialRegion_WhenAMandatoryParameterIsMissing_ThenTheSpatialRegionShouldNotBeCreated(
                string propertyName)
        {
            var categories = new List<DelftIniCategory>();

            var contentIniHeader = CreateValidContentIniHeader("InitialWaterLevel", "1");
            categories.Add(contentIniHeader);
            var definitionIniHeader = CreateValidDefinitionIniHeader("Maasmond", 20.0, 10.0);

            definitionIniHeader.Properties.RemoveAllWhere(p => p.Name == propertyName);

            categories.Add(definitionIniHeader);

            var errorMessages = new List<string>();

            var networkCoverage = SpatialDataConverter.Convert(categories, channelsList, errorMessages);

            Assert.AreEqual(0, networkCoverage.Arguments[0].Values.Count);
            Assert.AreEqual(0, networkCoverage.Components[0].Values.Count);
            Assert.AreEqual(1, errorMessages.Count);

            Assert.That(errorMessages.Contains($"Property {propertyName} is not found in the file"));
        }

        [Test]
        public void
            GivenACategoryWithAnSpatialRegion_WhenTheValuePropertyIsMissing_ThenTheSpatialRegionShouldBeCreatedButAnErrorMessageShouldBeThrown()
        {
            var categories = new List<DelftIniCategory>();
            const string propertyName = "value";

            var contentIniHeader = CreateValidContentIniHeader("InitialWaterLevel", "1");
            categories.Add(contentIniHeader);
            var definitionIniHeader = CreateValidDefinitionIniHeader("Maasmond", 20.0, 10.0);

            var removeProperty = definitionIniHeader.Properties.FirstOrDefault(p => p.Name == propertyName);
            definitionIniHeader.RemoveProperty(removeProperty);

            categories.Add(definitionIniHeader);

            var errorMessages = new List<string>();

            var networkCoverage = SpatialDataConverter.Convert(categories, channelsList, errorMessages);

            Assert.AreEqual(1, networkCoverage.Arguments[0].Values.Count);
            Assert.AreEqual(1, networkCoverage.Components[0].Values.Count);
            Assert.AreEqual(1, errorMessages.Count);

            Assert.That(errorMessages.Contains($"Property {propertyName} is not found in the file"));
        }

        [Test]
        public void
            GivenACategoryWithAnSpatialRegion_WhenDuringConvertToSpatialDataTheBranchIsNull_ThenAnErrorMessageIsThrown()
        {
            var incorrectChannelsList = new List<IChannel>();

            var categories = new List<DelftIniCategory>();

            var contentIniHeader = CreateValidContentIniHeader("InitialWaterLevel", "1");
            categories.Add(contentIniHeader);
            var definitionIniHeader = CreateValidDefinitionIniHeader("Maasmond", 20.0, 10.0);

            categories.Add(definitionIniHeader);

            var errorMessages = new List<string>();

            SpatialDataConverter.Convert(categories, incorrectChannelsList, errorMessages);

            Assert.That(errorMessages.Contains(string.Format(
                Resources.SpatialDataConverter_ConvertToSpatialData_Unable_to_parse__0__property___1___Branch_not_found_in_Network__2_,
                categories[1].Name, LocationRegion.BranchId.Key, Environment.NewLine)));
        }

        private static DelftIniCategory CreateValidDefinitionIniHeader(string branchId, double chainage, double value)
        {
            var firstCategory = new DelftIniCategory(SpatialDataRegion.DefinitionIniHeader);
            firstCategory.AddProperty(SpatialDataRegion.BranchId.Key, branchId);
            firstCategory.AddProperty(SpatialDataRegion.Chainage.Key, chainage);
            firstCategory.AddProperty(SpatialDataRegion.Value.Key, value);
            return firstCategory;
        }

        private static DelftIniCategory CreateValidContentIniHeader(string quantity, string interpolation)
        {
            var validContentIniHeader = new DelftIniCategory(SpatialDataRegion.ContentIniHeader);
            validContentIniHeader.AddProperty(SpatialDataRegion.Quantity.Key, quantity);
            validContentIniHeader.AddProperty(SpatialDataRegion.Interpolate.Key, interpolation);
            return validContentIniHeader;
        }
    }
}