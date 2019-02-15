using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Reader;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.CrossSections
{
    [TestFixture]
    class CrossSectionDefinitionConverterTest
    {
        [Test]
        public void GivenListOfIniCategoriesWithRightDataAndTypeYZ_WhenConvertingToCrossSectionDefinitions_ThenAListOfCrossSectionDefinitionsIsReturnedWithoutErrors()
        {
            var categories = new List<DelftIniCategory>();

            var crossSectionName1 = "CrossSection1";
            var crossSectionName2 = "CrossSection2";

            categories.Add(CreateGeneralCategory());
            categories.Add(CreateCrossSectionDefinitionCategory_YZ(crossSectionName1));
            categories.Add(CreateCrossSectionDefinitionCategory_YZ(crossSectionName2));

            var errorMessages = new List<string>();

            var crossSectionDefinitions = CrossSectionDefinitionConverter.Convert(categories, errorMessages).ToList();

            Assert.NotNull(crossSectionDefinitions);
            Assert.AreEqual(2, crossSectionDefinitions.Count);
            Assert.AreEqual(0, errorMessages.Count);

            Assert.That(crossSectionDefinitions.Exists(csl => csl.Name == crossSectionName1));
            Assert.That(crossSectionDefinitions.Exists(csl => csl.Name == crossSectionName2));

            var crossSectionDefinition = crossSectionDefinitions.FirstOrDefault();

            Assert.NotNull(crossSectionDefinition);

            Assert.AreEqual(CrossSectionType.YZ, crossSectionDefinition.CrossSectionType);
            Assert.AreEqual(false, crossSectionDefinition.GeometryBased);
            Assert.AreEqual(6.0, crossSectionDefinition.HighestPoint);
            Assert.AreEqual(false, crossSectionDefinition.IsProxy);
            Assert.AreEqual(5.0, crossSectionDefinition.LeftEmbankment);
            Assert.AreEqual(4.0, crossSectionDefinition.LowestPoint);
            Assert.AreEqual(3, crossSectionDefinition.FlowProfile.Count());

            // Sections are added after conversion
            Assert.AreEqual(0, crossSectionDefinition.Sections.Count);

            Assert.AreEqual(3, crossSectionDefinition.FlowProfile.Count());
        }

        [Test]
        public void GivenListOfIniCategoriesWithRightDataAndTypeZW_WhenConvertingToCrossSectionDefinitions_ThenAListOfCrossSectionDefinitionsIsReturnedWithoutErrors()
        {
            var categories = new List<DelftIniCategory>();

            var crossSectionName = "CrossSection1";

            categories.Add(CreateGeneralCategory());
            categories.Add(CreateCrossSectionDefinitionCategory_ZW(crossSectionName));

            var errorMessages = new List<string>();

            var crossSectionDefinitions = CrossSectionDefinitionConverter.Convert(categories, errorMessages).ToList();

            Assert.NotNull(crossSectionDefinitions);
            Assert.AreEqual(1, crossSectionDefinitions.Count);
            Assert.AreEqual(0, errorMessages.Count);

            Assert.That(crossSectionDefinitions.Exists(csl => csl.Name == crossSectionName));

            var crossSectionDefinition = crossSectionDefinitions.FirstOrDefault();

            Assert.NotNull(crossSectionDefinition);

            Assert.AreEqual(CrossSectionType.ZW, crossSectionDefinition.CrossSectionType);
            Assert.AreEqual(false, crossSectionDefinition.GeometryBased);
            Assert.AreEqual(0.0, crossSectionDefinition.HighestPoint);
            Assert.AreEqual(false, crossSectionDefinition.IsProxy);
            Assert.AreEqual(-10.0, crossSectionDefinition.LowestPoint);
            Assert.AreEqual(4, crossSectionDefinition.FlowProfile.Count());

            // Sections are added after conversion
            Assert.AreEqual(0, crossSectionDefinition.Sections.Count);
        }

        [Test]
        public void GivenListOfIniCategoriesWithRightDataAndTypeStandard_WhenConvertingToCrossSectionDefinitions_ThenAListOfCrossSectionDefinitionsIsReturnedWithoutErrors()
        {
            var categories = new List<DelftIniCategory>();

            var crossSectionName = "CrossSection1";

            categories.Add(CreateGeneralCategory());
            categories.Add(CreateCrossSectionDefinitionCategory_Standard(crossSectionName));

            var errorMessages = new List<string>();

            var crossSectionDefinitions = CrossSectionDefinitionConverter.Convert(categories, errorMessages).ToList();

            Assert.NotNull(crossSectionDefinitions);
            Assert.AreEqual(1, crossSectionDefinitions.Count);
            Assert.AreEqual(0, errorMessages.Count);

            Assert.That(crossSectionDefinitions.Exists(csl => csl.Name == crossSectionName));

            var crossSectionDefinition = crossSectionDefinitions.FirstOrDefault();

            Assert.NotNull(crossSectionDefinition);

            Assert.AreEqual(CrossSectionType.Standard, crossSectionDefinition.CrossSectionType);
            Assert.AreEqual(false, crossSectionDefinition.GeometryBased);
            Assert.AreEqual(2.0, crossSectionDefinition.HighestPoint);
            Assert.AreEqual(false, crossSectionDefinition.IsProxy);
            Assert.AreEqual(0.0, crossSectionDefinition.LowestPoint);

            // 2 * number of level - 1 (bottom level only has 1 point)
            Assert.AreEqual(85, crossSectionDefinition.FlowProfile.Count());

            // Sections are added after conversion
            Assert.AreEqual(0, crossSectionDefinition.Sections.Count);
        }

        [Test]
        public void GivenListOfIniCategoriesWithIsSharedProperty_WhenConvertingToCrossSectionDefinition_ThenAProxyCrossSectionDefinitionsIsReturnedWithoutErrors()
        {
            var categories = new List<DelftIniCategory>();

            var crossSectionName = "CrossSection1";

            var category = CreateCrossSectionDefinitionCategory_YZ(crossSectionName);
            AddSharedDefinitionProperty(category);
            categories.Add(CreateGeneralCategory());
            categories.Add(category);

            var errorMessages = new List<string>();

            var crossSectionDefinitions = CrossSectionDefinitionConverter.Convert(categories, errorMessages).ToList();

            Assert.NotNull(crossSectionDefinitions);
            Assert.AreEqual(1, crossSectionDefinitions.Count);
            Assert.AreEqual(0, errorMessages.Count);

            Assert.That(crossSectionDefinitions.Exists(csl => csl.Name == crossSectionName));

            var crossSectionDefinition = crossSectionDefinitions.FirstOrDefault();

            Assert.NotNull(crossSectionDefinition);

            Assert.AreEqual(true, crossSectionDefinition.IsProxy);
            Assert.That(crossSectionDefinition is CrossSectionDefinitionProxy);
        }

        [Test]
        public void GivenTwoIniCategoriesWithDuplicateIds_WhenConvertingToCrossSectionDefinitions_ThenOnlyFirstOneIsAddedAndErrorIsGiven()
        {
            var categories = new List<DelftIniCategory>();

            var crossSectionName = "CrossSection1";

            categories.Add(CreateGeneralCategory());
            categories.Add(CreateCrossSectionDefinitionCategory_YZ(crossSectionName));
            categories.Add(CreateCrossSectionDefinitionCategory_YZ(crossSectionName));

            var errorMessages = new List<string>();

            var crossSectionDefinitions = CrossSectionDefinitionConverter.Convert(categories, errorMessages).ToList();

            Assert.NotNull(crossSectionDefinitions);
            Assert.AreEqual(1, crossSectionDefinitions.Count);
            Assert.AreEqual(1, errorMessages.Count);

            Assert.That(errorMessages.Any(e => e.Equals($"Cross section definition with id {crossSectionName} already exists, there cannot be any duplicate cross section definition ids")));
        }

        [TestCase("id")]
        [TestCase("type")]
        [TestCase("thalweg")]
        [TestCase("yzCount")]
        [TestCase("yValues")]
        [TestCase("zValues")]
        [TestCase("deltaZStorage")]
        public void GivenListOfIniCategoriesWithMissingProperty_WhenConvertingToCrossSectionDefinitions_ThenErrorIsGiven(string missingPropertyName)
        {
            var categories = new List<DelftIniCategory>();

            var crossSectionName = "CrossSection1";

            var category = CreateCrossSectionDefinitionCategory_YZ(crossSectionName);

            categories.Add(CreateGeneralCategory());
            categories.Add(category);
            RemovePropertyByName(missingPropertyName, category);

            Assert.NotNull(categories);
            Assert.IsFalse(categories.FirstOrDefault().Properties.Any(p => p.Name == missingPropertyName));

            var errorMessages = new List<string>();

            var crossSectionDefinitions = CrossSectionDefinitionConverter.Convert(categories, errorMessages).ToList();

            Assert.NotNull(crossSectionDefinitions);
            Assert.AreEqual(0, crossSectionDefinitions.Count);
            Assert.AreEqual(1, errorMessages.Count);

            Assert.That(errorMessages.Any(e => e.Equals($"Property {missingPropertyName} is not found in the file"))); 
        }

        [Test]
        public void GivenCrossSectionDefinitionDelftIniCategory_WhenConvertingToGroundLayerData_ThenCorrectGroundLayerDataObjectIsReturned()
        {
            // Given
            var id = "myCrossSectionId";
            var category = new DelftIniCategory(DefinitionRegion.Header);
            category.AddProperty(DefinitionRegion.Id.Key, id);
            category.AddProperty(DefinitionRegion.GroundlayerUsed.Key, "1");
            category.AddProperty(DefinitionRegion.Groundlayer.Key, "20.0");
            var categories = new List<DelftIniCategory> { category };

            // When
            var groundLayerDataObjects = CrossSectionDefinitionConverter.ConvertToGroundLayerData(categories).ToArray();

            // Then
            Assert.That(groundLayerDataObjects.Length, Is.EqualTo(1));

            var groundLayerData = groundLayerDataObjects.FirstOrDefault();
            Assert.IsNotNull(groundLayerData);
            Assert.That(groundLayerData.CrossSectionDefinitionId, Is.EqualTo(id));
            Assert.That(groundLayerData.GroundLayerUsed, Is.EqualTo(true), "GroundLayer is not used");
            Assert.That(groundLayerData.GroundLayerThickness, Is.EqualTo(20.0));
        }

        [Test]
        public void GivenCrossSectionDefinitionCategoryWithoutGroundLayerProperties_WhenConvertingToGroundLayerObject_ThenNoExceptionIsThrownAndNoObjectHasBeenRead()
        {
            // Given
            var id = "myCrossSectionId";
            var category = new DelftIniCategory(DefinitionRegion.Header);
            var categories = new List<DelftIniCategory> { category };

            // When
            var groundLayerDataObjects = CrossSectionDefinitionConverter.ConvertToGroundLayerData(categories).ToArray();

            // Then
            Assert.IsEmpty(groundLayerDataObjects);
        }

        private DelftIniCategory CreateCrossSectionDefinitionCategory_YZ(string id)
        {
            var category = new DelftIniCategory("Definition");

            category.AddProperty("id", id);
            category.AddProperty("type", "yz");
            category.AddProperty("thalweg", 1.000);
            category.AddProperty("yzCount", 3);
            category.AddProperty("yValues", new List<double> { 1.0, 2.0, 3.0 });
            category.AddProperty("zValues", new List<double> { 5.0, 6.0, 4.0 });
            category.AddProperty("deltaZStorage", new List<double> { 7.0, 8.0, 9.0 });
            category.AddProperty("sectionCount", 2);
            category.AddProperty("roughnessNames", "Main;Main");
            category.AddProperty("roughnessPositions", new List<double> { 10.0, 11.0, 12.0 });
            category.AddProperty("roughnessTypesPos", new List<int> { 1, 1 });
            category.AddProperty("roughnessValuesPos", new List<double> { 13.000 });
            category.AddProperty("roughnessTypesNeg", new List<int> { 1, 1 });
            category.AddProperty("roughnessValuesNeg", new List<double> { 13.000 });

            return category;
        }

        private DelftIniCategory CreateCrossSectionDefinitionCategory_ZW(string id)
        {
            var category = new DelftIniCategory("Definition");

            category.AddProperty("id", id);
            category.AddProperty("type", "tabulated");
            category.AddProperty("thalweg", new List<double> { 0.000 });
            category.AddProperty("numLevels", 2);
            category.AddProperty("levels", new List<double> { -10.0, 0.0 });
            category.AddProperty("flowWidths", new List<double> { 20.0, 100.0 });
            category.AddProperty("totalWidths", new List<double> { 20.0, 100.0 });
            category.AddProperty("sd_crest", new List<double> { 1.0 });
            category.AddProperty("sd_flowArea", new List<double> { 2.0 });
            category.AddProperty("sd_totalArea", new List<double> { 3.0 });
            category.AddProperty("sd_baseLevel", new List<double> { 4.0 });
            category.AddProperty("main", new List<double> { 20 });
            category.AddProperty("floodPlain1", new List<double> { 30 });
            category.AddProperty("floodPlain2", new List<double> { 50 });
            category.AddProperty("groundlayerUsed", new List<double> { 0 });
            category.AddProperty("groundlayer", new List<double> { 0.0 });
            return category;
        }

        private DelftIniCategory CreateCrossSectionDefinitionCategory_Standard(string id)
        {
            var category = new DelftIniCategory("Definition");

            category.AddProperty("id", id);
            category.AddProperty("type", "steelcunette");
            category.AddProperty("thalweg", 1.0);
            category.AddProperty("roughnessNames", "FloodPlain1");
            category.AddProperty("height", 2.0);
            category.AddProperty("r", 3.0);
            category.AddProperty("r1", 4.0);
            category.AddProperty("r2", 5.0);
            category.AddProperty("r3", 6.0);
            category.AddProperty("a", 7.0);
            category.AddProperty("a1", 8.0);
            category.AddProperty("numLevels", 43);
            category.AddProperty("levels", CreateListOfDoubles(1.0, 43));
            category.AddProperty("flowWidths", CreateListOfDoubles(0.0, 43));
            category.AddProperty("groundlayerUsed", 0);
            category.AddProperty("groundlayer", 0);

            return category;
        }

        private void AddSharedDefinitionProperty(DelftIniCategory category)
        {
            category.AddProperty("isShared", 1);
        }

        private IList<double> CreateListOfDoubles(double start, int numberOfDoubles)
        {
            var doubles = new List<double>();

            for (var i = 0; i < numberOfDoubles; i++)
            {
                doubles.Add(Math.Round(start + i * 0.1, 1));
            }

            return doubles;

        }

        private static void RemovePropertyByName(string missingProperty, DelftIniCategory category)
        {
            category.RemoveProperty(category.Properties.FirstOrDefault(p => p.Name == missingProperty));
        }

        private DelftIniCategory CreateGeneralCategory()
        {
            var category = new DelftIniCategory("General");

            category.AddProperty("majorVersion", 1);
            category.AddProperty("minorVersion", 0);
            category.AddProperty("fileType", "crossDef");

            return category;
        }


    }
}
