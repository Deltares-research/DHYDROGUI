using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using DeltaShell.Plugins.NetworkEditor.Tests.EqualityComparers;
using DeltaShell.Plugins.NetworkEditor.Tests.Helpers;
using NUnit.Framework;
using SharpMap.Api;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class CrossSectionYZFromGisImporterTest
    {
        private const string testFileLocation = FromGisImporterHelper.FileLocationYz;
        private IHydroNetwork hydroNetwork;

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var importer = new CrossSectionYZFromGisImporter();

            // Assert
            Assert.That(importer, Is.InstanceOf<NetworkFeatureFromGisImporterBase>());
            Assert.That(importer.Name, Is.EqualTo("Y'Z cross-section from GIS importer")); 

            FeatureFromGisImporterSettings settings = importer.FeatureFromGisImporterSettings;
            Assert.That(settings.FeatureType, Is.EqualTo("Cross Sections Y'Z"));

            var expectedImporterType = typeof(CrossSectionYZFromGisImporter).ToString();
            Assert.That(settings.FeatureImporterFromGisImporterType, Is.EqualTo(expectedImporterType));

            AssertPropertyMappingsAreCorrect(settings.PropertiesMapping);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCorrectSettings_WhenValidatingNetworkWithTestFilePath_ThenReturnTrueForValidation()
        {
            var importer = new CrossSectionYZFromGisImporter();
            var settings = new FeatureFromGisImporterSettings();
            AddAllExpectedProperties(settings);
            FromGisImporterHelper.SetupAndLinkTestFilePath(settings, importer, testFileLocation);

            Assert.That(importer.ValidateNetworkFeatureFromGisImporterSettings(settings), Is.True);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetPropertyMappingWithOneMissingProperty))]
        public void GivenIncorrectSettings_WhenValidatingNetworkWithTestFilePath_ThenReturnFalseForValidation(List<PropertyMapping> propertyMappingWithOneMissingProperty)
        {
            var importer = new CrossSectionYZFromGisImporter();
            var settings = new FeatureFromGisImporterSettings();
            FromGisImporterHelper.SetupAndLinkTestFilePath(settings, importer, testFileLocation);

            Assert.That(importer.ValidateNetworkFeatureFromGisImporterSettings(settings), Is.False);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenGisFile_WhenImportItem_ThenCrossSectionContainsDataFromGisFile()
        {
            var importer = new CrossSectionYZFromGisImporter();
            importer.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            importer.FileBasedFeatureProviders.Add(FromGisImporterHelper.GetTestFileBasedFeatureProvider(testFileLocation));
            FromGisImporterHelper.SetupAndLinkHydroNetworkWithBranchesAndHighSnappingTolerance(importer);
            MapColumnsFromGisFile(importer);

            var data = importer.ImportItem("") as HydroNetwork;

            Assert.That(data, Is.Not.Null);
            Assert.That(data.CrossSections.Count(), Is.EqualTo(1));
            ICrossSection importedCrossSection = data.CrossSections.First();
            Assert.That(importedCrossSection.CrossSectionType, Is.EqualTo(CrossSectionType.YZ));
            var importedYzCrossSectionDefinition = importedCrossSection.Definition as CrossSectionDefinitionYZ;
            Assert.That(importedYzCrossSectionDefinition, Is.Not.Null);
            FastYZDataTable yzDataTable = importedYzCrossSectionDefinition.YZDataTable;
            Assert.That(yzDataTable[0].Z, Is.EqualTo(2));
            Assert.That(yzDataTable[0].Yq, Is.EqualTo(-5));
            Assert.That(yzDataTable[1].Z, Is.EqualTo(0));
            Assert.That(yzDataTable[1].Yq, Is.EqualTo(-5));
            Assert.That(yzDataTable[2].Z, Is.EqualTo(0));
            Assert.That(yzDataTable[2].Yq, Is.EqualTo(5));
            Assert.That(yzDataTable[3].Z, Is.EqualTo(2));
            Assert.That(yzDataTable[3].Yq, Is.EqualTo(5));
        }

        private static void AddAllExpectedProperties(FeatureFromGisImporterSettings settings)
        {
            settings.PropertiesMapping.Add(CrossSectionDefaultGisPropertyMappings.Name);
            settings.PropertiesMapping.Add(CrossSectionDefaultGisPropertyMappings.LongName);
            settings.PropertiesMapping.Add(CrossSectionDefaultGisPropertyMappings.Description);
            settings.PropertiesMapping.Add(CrossSectionDefaultGisPropertyMappings.ShiftLevel);
            settings.PropertiesMapping.Add(CrossSectionDefaultGisPropertyMappings.YValues);
            settings.PropertiesMapping.Add(CrossSectionDefaultGisPropertyMappings.ZValues);
        }

        private static void AssertPropertyMappingsAreCorrect(IReadOnlyList<PropertyMapping> propertyMapping)
        {
            var comparer = new PropertyMappingEqualityComparer();

            Assert.That(comparer.Equals(propertyMapping[0], CrossSectionDefaultGisPropertyMappings.Name), Is.True);
            Assert.That(comparer.Equals(propertyMapping[1], CrossSectionDefaultGisPropertyMappings.LongName), Is.True);
            Assert.That(comparer.Equals(propertyMapping[2], CrossSectionDefaultGisPropertyMappings.Description), Is.True);
            Assert.That(comparer.Equals(propertyMapping[3], CrossSectionDefaultGisPropertyMappings.ShiftLevel), Is.True);
            Assert.That(comparer.Equals(propertyMapping[4], CrossSectionDefaultGisPropertyMappings.YValues), Is.True);
            Assert.That(comparer.Equals(propertyMapping[5], CrossSectionDefaultGisPropertyMappings.ZValues), Is.True);
        }

        private static void MapColumnsFromGisFile(CrossSectionYZFromGisImporter importer)
        {
            importer.FeatureFromGisImporterSettings.PropertiesMapping[0].MappingColumn.ColumnName = "Name";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[1].MappingColumn.ColumnName = "Name";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[2].MappingColumn.ColumnName = "Name";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[3].MappingColumn.ColumnName = "Bedlevel";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[4].MappingColumn.ColumnName = "Yvalues";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[5].MappingColumn.ColumnName = "Zvalues";
        }

        private static IEnumerable<List<PropertyMapping>> GetPropertyMappingWithOneMissingProperty()
        {
            yield return new List<PropertyMapping>
            {
                CrossSectionDefaultGisPropertyMappings.LongName,
                CrossSectionDefaultGisPropertyMappings.Description,
                CrossSectionDefaultGisPropertyMappings.ShiftLevel,
                CrossSectionDefaultGisPropertyMappings.YValues,
                CrossSectionDefaultGisPropertyMappings.ZValues,
            };

            yield return new List<PropertyMapping>
            {
                CrossSectionDefaultGisPropertyMappings.Name,
                CrossSectionDefaultGisPropertyMappings.Description,
                CrossSectionDefaultGisPropertyMappings.ShiftLevel,
                CrossSectionDefaultGisPropertyMappings.YValues,
                CrossSectionDefaultGisPropertyMappings.ZValues,
            };

            yield return new List<PropertyMapping>
            {
                CrossSectionDefaultGisPropertyMappings.Name,
                CrossSectionDefaultGisPropertyMappings.LongName,
                CrossSectionDefaultGisPropertyMappings.ShiftLevel,
                CrossSectionDefaultGisPropertyMappings.YValues,
                CrossSectionDefaultGisPropertyMappings.ZValues,
            };

            yield return new List<PropertyMapping>
            {
                CrossSectionDefaultGisPropertyMappings.Name,
                CrossSectionDefaultGisPropertyMappings.LongName,
                CrossSectionDefaultGisPropertyMappings.Description,
                CrossSectionDefaultGisPropertyMappings.YValues,
                CrossSectionDefaultGisPropertyMappings.ZValues,
            };

            yield return new List<PropertyMapping>
            {
                CrossSectionDefaultGisPropertyMappings.Name,
                CrossSectionDefaultGisPropertyMappings.LongName,
                CrossSectionDefaultGisPropertyMappings.Description,
                CrossSectionDefaultGisPropertyMappings.ShiftLevel,
                CrossSectionDefaultGisPropertyMappings.ZValues,
            };

            yield return new List<PropertyMapping>
            {
                CrossSectionDefaultGisPropertyMappings.Name,
                CrossSectionDefaultGisPropertyMappings.LongName,
                CrossSectionDefaultGisPropertyMappings.Description,
                CrossSectionDefaultGisPropertyMappings.ShiftLevel,
                CrossSectionDefaultGisPropertyMappings.YValues,
            };
        }
    }
}