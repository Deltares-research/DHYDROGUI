using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.CrossSections
{
    [TestFixture]
    public class CrossSectionLocationConverterTest
    {
        [Test]
        public void GivenListOfIniCategoriesWithRightData_WhenConvertingToCrossSectionLocations_ThenAListOfCrossSectionLocationsIsReturnedWithoutErrors()
        {
            var categories = new List<DelftIniCategory>();

            var crossSectionName1 = "CrossSection1";
            var crossSectionName2 = "CrossSection2";

            categories.Add(CreateGeneralCategory());
            categories.Add(CreateCrossSectionLocationCategory(crossSectionName1));
            categories.Add(CreateCrossSectionLocationCategory(crossSectionName2));

            var errorMessages = new List<string>();

            var crossSectionLocations = CrossSectionLocationConverter.Convert(categories, errorMessages).ToList();

            Assert.NotNull(crossSectionLocations);
            Assert.AreEqual(2, crossSectionLocations.Count);
            Assert.AreEqual(0, errorMessages.Count);

            Assert.That(crossSectionLocations.Exists(csl => csl.Name == crossSectionName1));
            Assert.That(crossSectionLocations.Exists(csl => csl.Name == crossSectionName2));

            var crossSectionLocation = crossSectionLocations.FirstOrDefault();

            Assert.NotNull(crossSectionLocation);

            Assert.AreEqual("Channel1", crossSectionLocation.BranchName);
            Assert.AreEqual(123.456, crossSectionLocation.Chainage);
            Assert.AreEqual(1.234, crossSectionLocation.Shift);
            Assert.AreEqual("CrossSection1", crossSectionLocation.Definition);
            Assert.AreEqual("CrossSection1Channel1", crossSectionLocation.LongName);
        }

        [Test]
        public void GivenTwoIniCategoriesWithDuplicateIds_WhenConvertingToCrossSectionLocations_ThenOnlyFirstOneIsAddedAndErrorIsGiven()
        {
            var categories = new List<DelftIniCategory>();

            var crossSectionName = "CrossSection1";

            categories.Add(CreateGeneralCategory());
            categories.Add(CreateCrossSectionLocationCategory(crossSectionName));
            categories.Add(CreateCrossSectionLocationCategory(crossSectionName));

            var errorMessages = new List<string>();

            var crossSectionLocations = CrossSectionLocationConverter.Convert(categories, errorMessages).ToList();

            Assert.NotNull(crossSectionLocations);
            Assert.AreEqual(1, crossSectionLocations.Count);
            Assert.AreEqual(1, errorMessages.Count);

            Assert.That(errorMessages.Any(e => e.Equals($"Cross section location with id {crossSectionName} already exists, there cannot be any duplicate cross section location ids")));

        }

        [Test]
        [TestCase("id")]
        [TestCase("branchid")]
        [TestCase("chainage")]
        [TestCase("shift")]
        [TestCase("definition")]
        public void GivenListOfIniCategoriesWithMissingProperty_WhenConvertingToCrossSectionLocations_ThenErrorIsGiven(string missingPropertyName)
        {
            var categories = new List<DelftIniCategory>();

            var crossSectionName = "CrossSection1";

            categories.Add(CreateCrossSectionLocationCategory(crossSectionName));
            RemovePropertyByName(missingPropertyName, categories.FirstOrDefault());

            Assert.NotNull(categories);
            Assert.IsFalse(categories.First().Properties.Any(p => p.Name == missingPropertyName));

            var errorMessages = new List<string>();

            var crossSectionLocations = CrossSectionLocationConverter.Convert(categories, errorMessages).ToList();

            Assert.NotNull(crossSectionLocations);
            Assert.AreEqual(0, crossSectionLocations.Count);
            Assert.AreEqual(1, errorMessages.Count);

            Assert.That(errorMessages.Any(e => e.Equals($"Property {missingPropertyName} is not found in the file")));
        }

        [Test]
        public void GivenListOfIniCategoriesWithOptionalMissingProperty_WhenConvertingToCrossSectionLocations_ThenNoErrorIsGiven()
        {
            var categories = new List<DelftIniCategory>();

            var crossSectionName = "CrossSection1";

            var missingPropertyName = "name";

            categories.Add(CreateCrossSectionLocationCategory(crossSectionName));
            RemovePropertyByName(missingPropertyName, categories.FirstOrDefault());

            Assert.NotNull(categories);
            Assert.IsFalse(categories.First().Properties.Any(p => p.Name == missingPropertyName));

            var errorMessages = new List<string>();

            var crossSectionLocations = CrossSectionLocationConverter.Convert(categories, errorMessages).ToList();

            Assert.NotNull(crossSectionLocations);
            Assert.AreEqual(1, crossSectionLocations.Count);
            Assert.AreEqual(0, errorMessages.Count);

            Assert.That(!errorMessages.Any(e => e.Equals($"Property {missingPropertyName} is not found in the file")));
        }

        private DelftIniCategory CreateCrossSectionLocationCategory(string id)
        {
            var category = new DelftIniCategory("CrossSection");

            category.AddProperty("id", id);
            category.AddProperty("branchid", "Channel1");
            category.AddProperty("chainage", 123.456);
            category.AddProperty("shift", 1.234);
            category.AddProperty("definition", "CrossSection1");
            category.AddProperty("name", "CrossSection1Channel1");

            return category;
        }

        private DelftIniCategory CreateGeneralCategory()
        {
            var category = new DelftIniCategory("General");

            category.AddProperty("majorVersion", 1);
            category.AddProperty("minorVersion", 0);
            category.AddProperty("fileType", "crossLoc");

            return category;
        }

        private static void RemovePropertyByName(string missingProperty, DelftIniCategory category)
        {
            category.RemoveProperty(category.Properties.FirstOrDefault(p => p.Name == missingProperty));
        }
    }
}
