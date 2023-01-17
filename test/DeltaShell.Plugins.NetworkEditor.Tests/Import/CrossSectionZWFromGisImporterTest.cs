using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;
using SharpMap.Api;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class CrossSectionZwFromGisImporterTest
    {
        private const string lblLevel = "Level ";
        private const string lblFlowWidth = "Flow width ";
        private const string lblStorageWidth = "Storage width ";
        private const string testFileLocation = FromGisImporterHelper.FileLocationCrossSectionZw;

        private static readonly PropertyMapping propertyMappingName = new PropertyMapping("Name", true, true);
        private static readonly PropertyMapping propertyMappingLongName = new PropertyMapping("LongName", false, false);
        private static readonly PropertyMapping propertyMappingDescription = new PropertyMapping("Description", false, false);
        private static readonly PropertyMapping propertyMappingShiftLevel = new PropertyMapping("ShiftLevel", false, false);
        private static readonly PropertyMapping propertyMappingLevel = new PropertyMapping(lblLevel, false, true) { PropertyUnit = "m" };
        private static readonly PropertyMapping propertyMappingFlowWidth = new PropertyMapping(lblFlowWidth, false, true) { PropertyUnit = "m" };
        private static readonly PropertyMapping propertyMappingStorageWidth = new PropertyMapping(lblStorageWidth, false, true) { PropertyUnit = "m" };

        private IHydroNetwork hydroNetwork;

        [Test]
        public void Constructor_InitializesCorrectly()
        {
            //Arrange & Act
            var importer = new CrossSectionZWFromGisImporter();

            //Assert
            Assert.That(importer.Name, Is.EqualTo("Tabulated river cross-section from GIS importer"));
            Assert.That(importer.NumberOfLevels, Is.EqualTo(3));
            Assert.That(importer.FeatureFromGisImporterSettings.FeatureType, Is.EqualTo("Cross Sections ZW"));
            List<PropertyMapping> propertiesMapping = importer.FeatureFromGisImporterSettings.PropertiesMapping;
            AssertThatPropertyMappingPropertiesAreCorrect(propertiesMapping[0], propertyMappingName);
            AssertThatPropertyMappingPropertiesAreCorrect(propertiesMapping[1], propertyMappingLongName);
            AssertThatPropertyMappingPropertiesAreCorrect(propertiesMapping[2], propertyMappingDescription);
            AssertThatPropertyMappingPropertiesAreCorrect(propertiesMapping[3], propertyMappingShiftLevel);

            for (var levelToCheck = 1; levelToCheck <= importer.NumberOfLevels; levelToCheck++)
            {
                AssertThatLevelPropertiesAreCorrect(propertiesMapping, levelToCheck);
            }
        }

        [Test]
        public void GivenNumberOfLevel_WhenNumberOfLevelsSet_ThenNumberOfLevelsIsAsExpected()
        {
            //Arrange
            var importer = new CrossSectionZWFromGisImporter();
            const int expectedNumberOfLevels = 2;

            //Act
            importer.NumberOfLevels = expectedNumberOfLevels;

            //Assert
            Assert.That(importer.NumberOfLevels, Is.EqualTo(expectedNumberOfLevels));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCorrectSettings_WhenValidatingNetworkWithTestFilePath_ThenReturnTrueForValidation()
        {
            //Arrange
            var importer = new CrossSectionZWFromGisImporter();
            var settings = new FeatureFromGisImporterSettings();
            AddAllExpectedProperties(settings);
            settings.PropertiesMapping.AddRange(GetLevelPropertiesMappings(importer.NumberOfLevels));

            FromGisImporterHelper.SetupAndLinkTestFilePath(settings, importer, testFileLocation);

            //Act & Assert
            Assert.That(importer.ValidateNetworkFeatureFromGisImporterSettings(settings), Is.True);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetPropertyMappingWithOneMissingProperty))]
        public void GivenIncorrectSettings_WhenValidatingNetworkWithTestFilePath_ThenReturnFalseForValidation(List<PropertyMapping> propertyMappingWithOneMissingProperty)
        {
            //Arrange
            var importer = new CrossSectionZWFromGisImporter();
            var settings = new FeatureFromGisImporterSettings();
            settings.PropertiesMapping = propertyMappingWithOneMissingProperty;
            settings.PropertiesMapping.AddRange(GetLevelPropertiesMappings(importer.NumberOfLevels));

            FromGisImporterHelper.SetupAndLinkTestFilePath(settings, importer, testFileLocation);

            //Act & Assert
            Assert.That(importer.ValidateNetworkFeatureFromGisImporterSettings(settings), Is.False);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetPropertyLevelMappingWithOneMissingProperty))]
        public void GivenIncorrectLevelSettings_WhenValidatingNetworkWithTestFilePath_ThenReturnFalseForValidation(List<PropertyMapping> propertyLevelMappingWithOneMissingProperty)
        {
            //Arrange
            var importer = new CrossSectionZWFromGisImporter { NumberOfLevels = 2 };
            var settings = new FeatureFromGisImporterSettings();

            settings.PropertiesMapping = new List<PropertyMapping>
            {
                propertyMappingName,
                propertyMappingLongName,
                propertyMappingDescription,
                propertyMappingShiftLevel,
            };
            settings.PropertiesMapping.AddRange(propertyLevelMappingWithOneMissingProperty);

            FromGisImporterHelper.SetupAndLinkTestFilePath(settings, importer, testFileLocation);

            //Act & Assert
            Assert.That(importer.ValidateNetworkFeatureFromGisImporterSettings(settings), Is.False);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenGisFile_WhenImportItem_ThenBridgeContainsDataFromGisFile()
        {
            var importer = new CrossSectionZWFromGisImporter();

            importer.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            importer.FileBasedFeatureProviders.Add(FromGisImporterHelper.GetTestFileBasedFeatureProvider(testFileLocation));
            FromGisImporterHelper.SetupAndLinkHydroNetworkWithBranchesAndHighSnappingTolerance(importer, hydroNetwork);
            MapColumnsFromGisFile(importer);

            var data = importer.ImportItem("") as HydroNetwork;

            Assert.That(data, Is.Not.Null);
            Assert.That(data.CrossSections.Count(), Is.EqualTo(1));
            var importedCrossSectionDefinition = data.CrossSections.First().Definition as CrossSectionDefinitionZW;
            Assert.That(importedCrossSectionDefinition, Is.Not.Null);
            Assert.That(importedCrossSectionDefinition.CrossSectionType, Is.EqualTo(CrossSectionType.ZW));
            FastZWDataTable zwDataTable = importedCrossSectionDefinition.ZWDataTable;
            AssertZwDataTableDataRowIsCorrect(zwDataTable[0], 4, 100);
            AssertZwDataTableDataRowIsCorrect(zwDataTable[1], 2, 20);
            AssertZwDataTableDataRowIsCorrect(zwDataTable[2], 1, 10);
        }

        private static List<PropertyMapping> GetLevelPropertiesMappings(int numberOfLevels)
        {
            var propertiesMappings = new List<PropertyMapping>();
            for (var i = 1; i <= numberOfLevels; i++)
            {
                propertiesMappings.Add(new PropertyMapping($"{lblLevel}{i}") { PropertyUnit = "m" });
                propertiesMappings.Add(new PropertyMapping($"{lblFlowWidth}{i}") { PropertyUnit = "m" });
                propertiesMappings.Add(new PropertyMapping($"{lblStorageWidth}{i}") { PropertyUnit = "m" });
            }

            return propertiesMappings;
        }

        private static void AssertThatLevelPropertiesAreCorrect(List<PropertyMapping> propertiesMapping, int level)
        {
            int startLocationToCheckLevelFrom = 4 + (3 * (level - 1));
            propertyMappingLevel.PropertyName = lblLevel + level;
            AssertThatPropertyMappingPropertiesAreCorrect(propertiesMapping[startLocationToCheckLevelFrom], propertyMappingLevel);
            propertyMappingFlowWidth.PropertyName = lblFlowWidth + level;
            AssertThatPropertyMappingPropertiesAreCorrect(propertiesMapping[startLocationToCheckLevelFrom + 1], propertyMappingFlowWidth);
            propertyMappingStorageWidth.PropertyName = lblStorageWidth + level;
            AssertThatPropertyMappingPropertiesAreCorrect(propertiesMapping[startLocationToCheckLevelFrom + 2], propertyMappingStorageWidth);
        }

        private static void AssertThatPropertyMappingPropertiesAreCorrect(PropertyMapping givenPropertyMapping, PropertyMapping expectedPropertyMapping)
        {
            Assert.That(givenPropertyMapping.PropertyName, Is.EqualTo(expectedPropertyMapping.PropertyName));
            Assert.That(givenPropertyMapping.IsUnique, Is.EqualTo(expectedPropertyMapping.IsUnique));
            Assert.That(givenPropertyMapping.IsRequired, Is.EqualTo(expectedPropertyMapping.IsRequired));
            Assert.That(givenPropertyMapping.PropertyUnit, Is.EqualTo(expectedPropertyMapping.PropertyUnit));
        }

        private static void AssertZwDataTableDataRowIsCorrect(CrossSectionDataSet.CrossSectionZWRow zwDataTable, int z, int storageWidth)
        {
            Assert.That(zwDataTable.Z, Is.EqualTo(z));
            Assert.That(zwDataTable.Width, Is.EqualTo(2 * storageWidth));
            Assert.That(zwDataTable.StorageWidth, Is.EqualTo(storageWidth));
        }

        private static void MapColumnsFromGisFile(CrossSectionZWFromGisImporter importer)
        {
            importer.FeatureFromGisImporterSettings.PropertiesMapping[0].MappingColumn.ColumnName = "Name";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[1].MappingColumn.ColumnName = "Name";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[2].MappingColumn.ColumnName = "Name";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[3].MappingColumn.ColumnName = "Bedlevel";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[4].MappingColumn.ColumnName = "LEVEL1";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[5].MappingColumn.ColumnName = "WIDTH1";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[6].MappingColumn.ColumnName = "WIDTH1";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[7].MappingColumn.ColumnName = "LEVEL2";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[8].MappingColumn.ColumnName = "WIDTH2";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[9].MappingColumn.ColumnName = "WIDTH2";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[10].MappingColumn.ColumnName = "LEVEL3";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[11].MappingColumn.ColumnName = "WIDTH3";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[12].MappingColumn.ColumnName = "WIDTH3";
        }

        private static void AddAllExpectedProperties(FeatureFromGisImporterSettings settings)
        {
            settings.PropertiesMapping.Add(propertyMappingName);
            settings.PropertiesMapping.Add(propertyMappingLongName);
            settings.PropertiesMapping.Add(propertyMappingDescription);
            settings.PropertiesMapping.Add(propertyMappingShiftLevel);
        }

        private static IEnumerable<List<PropertyMapping>> GetPropertyMappingWithOneMissingProperty()
        {
            yield return new List<PropertyMapping>
            {
                propertyMappingLongName,
                propertyMappingDescription,
                propertyMappingShiftLevel,
            };

            yield return new List<PropertyMapping>
            {
                propertyMappingName,
                propertyMappingDescription,
                propertyMappingShiftLevel,
            };

            yield return new List<PropertyMapping>
            {
                propertyMappingName,
                propertyMappingLongName,
                propertyMappingShiftLevel,
            };

            yield return new List<PropertyMapping>
            {
                propertyMappingName,
                propertyMappingLongName,
                propertyMappingDescription,
            };
        }

        private static IEnumerable<List<PropertyMapping>> GetPropertyLevelMappingWithOneMissingProperty()
        {
            yield return new List<PropertyMapping>
            {
                new PropertyMapping($"{lblFlowWidth}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblStorageWidth}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblLevel}{2}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblFlowWidth}{2}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblStorageWidth}{2}") { PropertyUnit = "m" }
            };

            yield return new List<PropertyMapping>
            {
                new PropertyMapping($"{lblLevel}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblStorageWidth}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblLevel}{2}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblFlowWidth}{2}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblStorageWidth}{2}") { PropertyUnit = "m" }
            };

            yield return new List<PropertyMapping>
            {
                new PropertyMapping($"{lblLevel}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblFlowWidth}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblLevel}{2}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblFlowWidth}{2}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblStorageWidth}{2}") { PropertyUnit = "m" }
            };

            yield return new List<PropertyMapping>
            {
                new PropertyMapping($"{lblLevel}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblFlowWidth}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblStorageWidth}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblFlowWidth}{2}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblStorageWidth}{2}") { PropertyUnit = "m" }
            };

            yield return new List<PropertyMapping>
            {
                new PropertyMapping($"{lblLevel}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblFlowWidth}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblStorageWidth}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblLevel}{2}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblStorageWidth}{2}") { PropertyUnit = "m" }
            };

            yield return new List<PropertyMapping>
            {
                new PropertyMapping($"{lblLevel}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblFlowWidth}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblStorageWidth}{1}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblLevel}{2}") { PropertyUnit = "m" },
                new PropertyMapping($"{lblFlowWidth}{2}") { PropertyUnit = "m" },
            };
        }
    }
}