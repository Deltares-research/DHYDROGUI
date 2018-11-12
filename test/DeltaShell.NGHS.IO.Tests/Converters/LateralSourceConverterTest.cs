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
    public class LateralSourceConverterTest
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

        [Test]
        public void GivenACategoryWithALateralSource_WhenReading_ThenTheLateralSourceShouldBeCreated()
        {
            var categories = new List<DelftIniCategory>();
            var category = CreatePerfectLateralSourceCategory();

            categories.Add(category);
                   
            var errorMessages = new List<string>();
            var allLateralSources = LateralSourceConverter.Convert(categories, originalNetwork, errorMessages);

            Assert.AreEqual(1, allLateralSources.Count);

            Assert.AreEqual("lateraldischarge1", allLateralSources[0].Name);
            Assert.AreEqual("branch", allLateralSources[0].Branch.Name);
            Assert.AreEqual(50, allLateralSources[0].Chainage);
            Assert.AreEqual("lateraldischarge1", allLateralSources[0].LongName);
            Assert.AreEqual(2, allLateralSources[0].Length);

            var coordinate = new Point(50, 0);
            Assert.AreEqual(coordinate, allLateralSources[0].Geometry);

            Assert.AreEqual(0, errorMessages.Count);
        }
        
        [TestCase("branchid")]
        [TestCase("chainage")]
        [TestCase("id")]
        public void GivenACategoryWithALateralSource_WhenAMandatoryParameterIsMissing_ThenTheLateralSourceShouldNotBeCreated(string propertyName)
        {
            var categories = new List<DelftIniCategory>();
            var category = CreatePerfectLateralSourceCategory();

            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);

            categories.Add(category);

            var errorMessages = new List<string>();

            var allLateralSources = LateralSourceConverter.Convert(categories, originalNetwork, errorMessages);

            Assert.AreEqual(0, allLateralSources.Count);
            Assert.AreEqual(1, errorMessages.Count);

            Assert.That(errorMessages.Contains($"Property {propertyName} is not found in the file"));
        }

        [TestCase("name")]
        [TestCase("length")]
        public void GivenACategoryWithALateralSource_WhenAnOptionalParameterIsMissing_ThenTheLateralSourceShouldBeCreated(string propertyName)
        {
            var categories = new List<DelftIniCategory>();
            var category = CreatePerfectLateralSourceCategory();

            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);

            categories.Add(category);
            
            var errorMessages = new List<string>();

            var allLateralSources = LateralSourceConverter.Convert(categories, originalNetwork, errorMessages);

            Assert.AreEqual(1, allLateralSources.Count);

            Assert.AreEqual("lateraldischarge1", allLateralSources[0].Name);
            Assert.AreEqual("branch", allLateralSources[0].Branch.Name);
            Assert.AreEqual(50, allLateralSources[0].Chainage);
            if (propertyName == "name")
            {
                Assert.AreEqual(2, allLateralSources[0].Length);
                Assert.AreEqual(string.Empty, allLateralSources[0].LongName);

            }
            else if (propertyName == "length")
            {
                Assert.AreEqual(0, allLateralSources[0].Length);
                Assert.AreEqual("lateraldischarge1", allLateralSources[0].LongName);
            }
            
            Assert.AreEqual(0, errorMessages.Count);
        }

        [Test]
        public void GivenACategoryWithALateralSource_WhenTheCorrespondingBranchIsNotInTheNetwork_ThenTheLateralSourceShouldNotBeCreated()
        {
            var categories = new List<DelftIniCategory>();
            var category = new DelftIniCategory(BoundaryRegion.LateralDischargeHeader);

            category.AddProperty(LocationRegion.Id.Key, "lateraldischarge1");
            category.AddProperty(LocationRegion.Chainage.Key, "50");
            category.AddProperty(LocationRegion.BranchId.Key, "branch2");
            category.AddProperty(LocationRegion.Name.Key, "lateraldischarge1");
            category.AddProperty(LateralSourceLocationRegion.Length.Key, "2");

            categories.Add(category);

            var errorMessages = new List<string>();
            
            var allLateralSources = LateralSourceConverter.Convert(categories, originalNetwork, errorMessages);

            Assert.AreEqual(0, allLateralSources.Count);
            Assert.AreEqual(1, errorMessages.Count);

            var expectedMessage = string.Format("Unable to parse {0} property: {1}, Branch not found in Network.{2}", category.Name, LocationRegion.BranchId.Key, Environment.NewLine);

            Assert.AreEqual(expectedMessage, errorMessages[0]);
        }

        private DelftIniCategory CreatePerfectLateralSourceCategory()
        {
            var category = new DelftIniCategory(BoundaryRegion.LateralDischargeHeader);

            category.AddProperty(LocationRegion.Id.Key, "lateraldischarge1");
            category.AddProperty(LocationRegion.Chainage.Key, "50");
            category.AddProperty(LocationRegion.BranchId.Key, "branch");
            category.AddProperty(LocationRegion.Name.Key, "lateraldischarge1");
            category.AddProperty(LateralSourceLocationRegion.Length.Key, "2");

            return category;
        }
    }
}