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
    public class BridgeRectangularFromGisImporterTest
    {
        private const string testFileLocation = FromGisImporterHelper.FileLocationBridgeRectangular;
        private static readonly PropertyMapping propertyMappingHeight = new PropertyMapping("Height") { PropertyUnit = "m" };
        private static readonly PropertyMapping propertyMappingWidth = new PropertyMapping("Width") { PropertyUnit = "m" };

        [Test]
        public void Constructor_InitializesCorrectly()
        {
            //Arrange & Act
            var importer = new BridgeRectangularFromGisImporter();
            int heightPropertyLocation = new BridgeFromGisImporterHelper.TestBaseBridge().FeatureFromGisImporterSettings.PropertiesMapping.Count;
            int widthPropertyLocation = heightPropertyLocation + 1;

            //Assert
            Assert.That(importer.Name, Is.EqualTo("Bridge from GIS importer"));
            Assert.That(importer.FeatureFromGisImporterSettings.FeatureType, Is.EqualTo("Bridges (rectangle profile)"));
            List<PropertyMapping> propertiesMapping = importer.FeatureFromGisImporterSettings.PropertiesMapping;
            Assert.That(propertiesMapping[heightPropertyLocation].PropertyName, Is.EqualTo(propertyMappingHeight.PropertyName));
            Assert.That(propertiesMapping[heightPropertyLocation].PropertyUnit, Is.EqualTo(propertyMappingHeight.PropertyUnit));
            Assert.That(propertiesMapping[widthPropertyLocation].PropertyName, Is.EqualTo(propertyMappingWidth.PropertyName));
            Assert.That(propertiesMapping[widthPropertyLocation].PropertyUnit, Is.EqualTo(propertyMappingWidth.PropertyUnit));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetPropertyMappingWithOneMissingProperty))]
        public void GivenIncorrectSettings_WhenValidatingNetworkWithTestFilePath_ThenReturnFalseForValidation(List<PropertyMapping> propertyMappingWithOneMissingProperty)
        {
            //Arrange
            var importer = new BridgeRectangularFromGisImporter();
            var settings = new FeatureFromGisImporterSettings();
            settings.PropertiesMapping.AddRange(propertyMappingWithOneMissingProperty);
            FromGisImporterHelper.SetupAndLinkTestFilePath(settings, importer, testFileLocation);

            //Act & Assert
            Assert.That(importer.ValidateNetworkFeatureFromGisImporterSettings(settings), Is.False);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCorrectSettings_WhenValidatingNetworkWithTestFilePath_ThenReturnTrueForValidation()
        {
            //Arrange
            var importer = new BridgeRectangularFromGisImporter();
            var baseImporter = new BridgeFromGisImporterHelper.TestBaseBridge();
            FeatureFromGisImporterSettings settings = baseImporter.FeatureFromGisImporterSettings;
            FromGisImporterHelper.SetupAndLinkTestFilePath(settings, importer, testFileLocation);

            settings.PropertiesMapping.Add(propertyMappingHeight);
            settings.PropertiesMapping.Add(propertyMappingWidth);

            //Act & Assert
            Assert.That(importer.ValidateNetworkFeatureFromGisImporterSettings(settings), Is.True);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenGisFile_WhenImportItem_ThenBridgeContainsDataFromGisFile()
        {
            var importer = new BridgeRectangularFromGisImporter();

            importer.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            importer.FileBasedFeatureProviders.Add(FromGisImporterHelper.GetTestFileBasedFeatureProvider(testFileLocation));
            FromGisImporterHelper.SetupAndLinkHydroNetworkWithBranchesAndHighSnappingTolerance(importer);
            MapColumnsFromGisFile(importer);
            
            var data = importer.ImportItem("") as HydroNetwork;

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Bridges.Count(), Is.EqualTo(1));
            var importedBridge = data.Bridges.First();
            Assert.That(importedBridge.IsRectangle, Is.True);
            Assert.That(importedBridge.Width, Is.EqualTo(10));
            Assert.That(importedBridge.Height, Is.EqualTo(2));
        }

        private static void MapColumnsFromGisFile(BridgeFromGisImporterBase importer)
        {
            importer.FeatureFromGisImporterSettings.PropertiesMapping[0].MappingColumn.ColumnName = "Name";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[6].MappingColumn.ColumnName = "HEIGHT";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[7].MappingColumn.ColumnName = "WIDTH";
        }

        private static IEnumerable<List<PropertyMapping>> GetPropertyMappingWithOneMissingProperty()
        {
            yield return new List<PropertyMapping>
            {
                propertyMappingHeight,
            };

            yield return new List<PropertyMapping>
            {
                propertyMappingWidth,
            };
        }
    }
}