using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using DeltaShell.Plugins.NetworkEditor.Tests.Helpers;
using NUnit.Framework;
using SharpMap.Api;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class BridgeFromGisImporterBaseTest
    {
        private const string testFileLocation = FromGisImporterHelper.FileLocationBridgeRectangular;
        private static readonly PropertyMapping propertyMappingName = new PropertyMapping("Name", true, true);
        private static readonly PropertyMapping propertyMappingLongName = new PropertyMapping("LongName");
        private static readonly PropertyMapping propertyMappingDescription = new PropertyMapping("Description");
        private static readonly PropertyMapping propertyMappingLevel = new PropertyMapping("Bed level") { PropertyUnit = "m" };
        private static readonly PropertyMapping propertyMappingLength = new PropertyMapping("Length") { PropertyUnit = "m" };
        private static readonly PropertyMapping propertyMappingFrictionValue = new PropertyMapping("Roughness") { PropertyUnit = "Chezy (C) m^1/2*s^-1" };
        private IHydroNetwork hydroNetwork;

        [Test]
        public void Constructor_InitializesCorrectly()
        {
            //Arrange & Act
            var importer = new BridgeFromGisImporterHelper.TestBaseBridge();

            //Assert
            Assert.That(importer.Name, Is.EqualTo(BridgeFromGisImporterHelper.TestName));
            List<PropertyMapping> propertiesMapping = importer.FeatureFromGisImporterSettings.PropertiesMapping;
            Assert.That(propertiesMapping[0].PropertyName, Is.EqualTo(propertyMappingName.PropertyName));
            Assert.That(propertiesMapping[0].IsUnique, Is.True);
            Assert.That(propertiesMapping[0].IsRequired, Is.True);
            Assert.That(propertiesMapping[1].PropertyName, Is.EqualTo(propertyMappingLongName.PropertyName));
            Assert.That(propertiesMapping[2].PropertyName, Is.EqualTo(propertyMappingDescription.PropertyName));
            Assert.That(propertiesMapping[3].PropertyName, Is.EqualTo(propertyMappingLevel.PropertyName));
            Assert.That(propertiesMapping[3].PropertyUnit, Is.EqualTo(propertyMappingLevel.PropertyUnit));
            Assert.That(propertiesMapping[4].PropertyName, Is.EqualTo(propertyMappingLength.PropertyName));
            Assert.That(propertiesMapping[4].PropertyUnit, Is.EqualTo(propertyMappingLength.PropertyUnit));
            Assert.That(propertiesMapping[5].PropertyName, Is.EqualTo(propertyMappingFrictionValue.PropertyName));
            Assert.That(propertiesMapping[5].PropertyUnit, Is.EqualTo(propertyMappingFrictionValue.PropertyUnit));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCorrectSettings_WhenValidatingNetworkWithTestFilePath_ThenReturnTrueForValidation()
        {
            //Arrange
            var importer = new BridgeFromGisImporterHelper.TestBaseBridge();
            var settings = new FeatureFromGisImporterSettings();
            AddAllExpectedProperties(settings);
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
            var importer = new BridgeFromGisImporterHelper.TestBaseBridge();
            var settings = new FeatureFromGisImporterSettings();
            settings.PropertiesMapping = propertyMappingWithOneMissingProperty;
            FromGisImporterHelper.SetupAndLinkTestFilePath(settings, importer, testFileLocation);

            //Act & Assert
            Assert.That(importer.ValidateNetworkFeatureFromGisImporterSettings(settings), Is.False);
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenGisFile_WhenImportItem_ThenBridgeContainsDataFromGisFile()
        {
            var importer = new BridgeFromGisImporterHelper.TestBaseBridge();
            
            importer.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            importer.FileBasedFeatureProviders.Add(FromGisImporterHelper.GetTestFileBasedFeatureProvider(testFileLocation));
            FromGisImporterHelper.SetupAndLinkHydroNetworkWithBranchesAndHighSnappingTolerance(importer, hydroNetwork);
            MapColumnsFromGisFile(importer);
            
            var data = importer.ImportItem("") as HydroNetwork;

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Bridges.Count(), Is.EqualTo(1));
            var importedBridge = data.Bridges.First();
            Assert.That(importedBridge.Name, Is.EqualTo("Bridge_rectangular"));
            Assert.That(importedBridge.LongName, Is.EqualTo("Bridge_rectangular"));
            Assert.That(importedBridge.Description, Is.EqualTo("Bridge_rectangular"));
            Assert.That(importedBridge.Length, Is.EqualTo(20));
            Assert.That(importedBridge.Friction, Is.EqualTo(45));
            Assert.That(importedBridge.Shift, Is.EqualTo(0));
        }

        private static void MapColumnsFromGisFile(BridgeFromGisImporterBase importer)
        {
            importer.FeatureFromGisImporterSettings.PropertiesMapping[0].MappingColumn.ColumnName = "Name";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[1].MappingColumn.ColumnName = "Name";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[2].MappingColumn.ColumnName = "Name";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[3].MappingColumn.ColumnName = "Bedlevel";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[4].MappingColumn.ColumnName = "Length";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[5].MappingColumn.ColumnName = "Roughness";
        }

        private static void AddAllExpectedProperties(FeatureFromGisImporterSettings settings)
        {
            settings.PropertiesMapping.Add(propertyMappingName);
            settings.PropertiesMapping.Add(propertyMappingLongName);
            settings.PropertiesMapping.Add(propertyMappingDescription);
            settings.PropertiesMapping.Add(propertyMappingLevel);
            settings.PropertiesMapping.Add(propertyMappingLength);
            settings.PropertiesMapping.Add(propertyMappingFrictionValue);
        }

        private static IEnumerable<List<PropertyMapping>> GetPropertyMappingWithOneMissingProperty()
        {
            yield return new List<PropertyMapping>
            {
                propertyMappingLongName,
                propertyMappingDescription,
                propertyMappingLevel,
                propertyMappingLength,
                propertyMappingFrictionValue
            };

            yield return new List<PropertyMapping>
            {
                propertyMappingName,
                propertyMappingDescription,
                propertyMappingLevel,
                propertyMappingLength,
                propertyMappingFrictionValue
            };

            yield return new List<PropertyMapping>
            {
                propertyMappingName,
                propertyMappingLongName,
                propertyMappingLevel,
                propertyMappingLength,
                propertyMappingFrictionValue
            };

            yield return new List<PropertyMapping>
            {
                propertyMappingName,
                propertyMappingLongName,
                propertyMappingDescription,
                propertyMappingLength,
                propertyMappingFrictionValue
            };

            yield return new List<PropertyMapping>
            {
                propertyMappingName,
                propertyMappingLongName,
                propertyMappingDescription,
                propertyMappingLevel,
                propertyMappingFrictionValue
            };

            yield return new List<PropertyMapping>
            {
                propertyMappingName,
                propertyMappingLongName,
                propertyMappingDescription,
                propertyMappingLevel,
                propertyMappingLength,
            };
        }
    }
}