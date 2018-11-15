using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Converters
{
    [TestFixture]
    public class ObservationPointConverterTest
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

        [Test]
        public void GivenACategoryWithAnObservationPoint_WhenReading_ThenTheObservationPointShouldBeCreated()
        {
            var categories = new List<DelftIniCategory>();
            var category = CreatePerfectObservationPointCategory();
            
            categories.Add(category);
            
            var errorMessages = new List<string>();
            var allObservationPoints = ObservationPointConverter.Convert(categories, channelsList, errorMessages);
            
            Assert.AreEqual(1, allObservationPoints.Count);

            Assert.AreEqual("observationpoint1", allObservationPoints[0].Name);
            Assert.AreEqual("branch", allObservationPoints[0].Branch.Name);
            Assert.AreEqual(50, allObservationPoints[0].Chainage);
            Assert.AreEqual("observationpoint1", allObservationPoints[0].LongName);

            var coordinate = new Point(50, 0);
            Assert.AreEqual(coordinate, allObservationPoints[0].Geometry);

            Assert.AreEqual(0, errorMessages.Count);
        }

        [TestCase("branchid")]
        [TestCase("chainage")]
        [TestCase("id")]
        public void GivenACategoryWithAnObservationPoint_WhenAMandatoryParameterIsMissing_ThenTheObservationPointShouldNotBeCreated(string propertyName)
        {
            var categories = new List<DelftIniCategory>();
            var category = CreatePerfectObservationPointCategory();
            
            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);

            categories.Add(category);
            
            var errorMessages = new List<string>();

            var allObservationPoints = ObservationPointConverter.Convert(categories, channelsList, errorMessages);

            Assert.AreEqual(0, allObservationPoints.Count);
            Assert.AreEqual(1, errorMessages.Count);

            Assert.That(errorMessages.Contains($"Property {propertyName} is not found in the file"));
        }

        [TestCase("name")]
        public void GivenACategoryWithAnObservationPoint_WhenAnOptionalParameterIsMissing_ThenTheObservationPointShouldBeCreated(string propertyName)
        {
            var categories = new List<DelftIniCategory>();

            var category = CreatePerfectObservationPointCategory();

            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);

            categories.Add(category);
            
            var errorMessages = new List<string>();

            var allObservationPoints = ObservationPointConverter.Convert(categories, channelsList, errorMessages);

            Assert.AreEqual(1, allObservationPoints.Count);

            Assert.AreEqual("observationpoint1", allObservationPoints[0].Name);
            Assert.AreEqual("branch", allObservationPoints[0].Branch.Name);
            Assert.AreEqual(50, allObservationPoints[0].Chainage);
            Assert.AreEqual(string.Empty, allObservationPoints[0].LongName);
            
            Assert.AreEqual(0, errorMessages.Count);

        }

        [Test]
        public void GivenACategoryWithAnObservationPoint_WhenTheCorrespondingBranchIsNotInTheNetwork_ThenTheObservationPointShouldNotBeCreated()
        {
            var categories = new List<DelftIniCategory>();
            var category = new DelftIniCategory(ObservationPointRegion.IniHeader);

            category.AddProperty(LocationRegion.Id.Key, "observationpoint1");
            category.AddProperty(LocationRegion.Chainage.Key, "50");
            category.AddProperty(LocationRegion.BranchId.Key, "branch2");
            category.AddProperty(LocationRegion.Name.Key, "observationpoint1");

            categories.Add(category);

            var errorMessages = new List<string>();
            var allObservationPoints = ObservationPointConverter.Convert(categories, channelsList, errorMessages);

            Assert.AreEqual(0, allObservationPoints.Count);
            Assert.AreEqual(1, errorMessages.Count);

            var expectedMessage = string.Format("Unable to parse {0} property: {1}, Branch not found in Network.{2}", category.Name, LocationRegion.BranchId.Key, Environment.NewLine);
            
            Assert.AreEqual(expectedMessage, errorMessages[0]);

        }

        [Test]
        public void GivenTwoCategoriesWithTheSameObservationPointIds_WhenReading_ThenTheSecondObservationPointShouldNotBeCreated()
        {
            var categories = new List<DelftIniCategory>();

            var category = CreatePerfectObservationPointCategory();
            categories.Add(category);

            var category2 = CreatePerfectObservationPointCategory();
            category2.SetProperty(LocationRegion.Chainage.Key, "75");
            
            categories.Add(category2);

            var errorMessages = new List<string>();
            var allObservationPoints = ObservationPointConverter.Convert(categories, channelsList, errorMessages);

            Assert.AreEqual(1, allObservationPoints.Count);

            Assert.AreEqual("observationpoint1", allObservationPoints[0].Name);
            Assert.AreEqual("branch", allObservationPoints[0].Branch.Name);
            Assert.AreEqual(50, allObservationPoints[0].Chainage);
            Assert.AreEqual("observationpoint1", allObservationPoints[0].LongName);

            var coordinate = new Point(50, 0);
            Assert.AreEqual(coordinate, allObservationPoints[0].Geometry);

            Assert.AreEqual(1, errorMessages.Count);

            var expectedMessage = string.Format("Observation point with id {0} already exists, there cannot be any duplicate observation point ids.{1}", allObservationPoints[0].Name, Environment.NewLine);

            Assert.AreEqual(expectedMessage, errorMessages[0]);
        }

        private DelftIniCategory CreatePerfectObservationPointCategory()
        {
            var category = new DelftIniCategory(ObservationPointRegion.IniHeader);

            category.AddProperty(LocationRegion.Id.Key, "observationpoint1");
            category.AddProperty(LocationRegion.BranchId.Key, "branch");
            category.AddProperty(LocationRegion.Chainage.Key, "50");
            category.AddProperty(LocationRegion.Name.Key, "observationpoint1");

            return category;
        }
    }
}