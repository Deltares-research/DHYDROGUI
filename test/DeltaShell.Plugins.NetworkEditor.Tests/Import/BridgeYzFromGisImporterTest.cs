using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;
using SharpMap.Api;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class BridgeYzFromGisImporterTest
    {
        private const string testFileLocation = "BridgeFromGisImporter_ShapeFiles/bridge_yz_test.shp";
        private static readonly PropertyMapping propertyMappingYValues = new PropertyMapping("Y'-values");
        private static readonly PropertyMapping propertyMappingZValues = new PropertyMapping("Z'-values");
        private IHydroNetwork hydroNetwork;

        [Test]
        public void Constructor_InitializesCorrectly()
        {
            //Arrange & Act
            var importer = new BridgeYzFromGisImporter();
            int yPropertyLocation = new BridgeFromGisImporterHelper.TestBaseBridge().FeatureFromGisImporterSettings.PropertiesMapping.Count;
            int zPropertyLocation = yPropertyLocation + 1;

            //Assert
            Assert.That(importer.Name, Is.EqualTo("Bridge YZ from GIS importer"));
            Assert.That(importer.FeatureFromGisImporterSettings.FeatureType, Is.EqualTo("Bridges (YZ profile)"));
            List<PropertyMapping> propertiesMapping = importer.FeatureFromGisImporterSettings.PropertiesMapping;
            Assert.That(propertiesMapping[yPropertyLocation].PropertyName, Is.EqualTo(propertyMappingYValues.PropertyName));
            Assert.That(propertiesMapping[zPropertyLocation].PropertyName, Is.EqualTo(propertyMappingZValues.PropertyName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenCorrectSettings_WhenValidatingNetworkWithTestFilePath_ThenReturnTrueForValidation()
        {
            var importer = new BridgeYzFromGisImporter();
            var baseImporter = new BridgeFromGisImporterHelper.TestBaseBridge();
            FeatureFromGisImporterSettings settings = baseImporter.FeatureFromGisImporterSettings;
            BridgeFromGisImporterHelper.SetupAndLinkTestFilePath(settings, importer, testFileLocation);

            settings.PropertiesMapping.Add(propertyMappingYValues);
            settings.PropertiesMapping.Add(propertyMappingZValues);

            Assert.That(importer.ValidateNetworkFeatureFromGisImporterSettings(settings), Is.True);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetPropertyMappingWithOneMissingProperty))]
        public void GivenIncorrectSettings_WhenValidatingNetworkWithTestFilePath_ThenReturnFalseForValidation(List<PropertyMapping> propertyMappingWithOneMissingProperty)
        {
            //Arrange
            var importer = new BridgeYzFromGisImporter();
            var settings = new FeatureFromGisImporterSettings();
            settings.PropertiesMapping.AddRange(propertyMappingWithOneMissingProperty);
            BridgeFromGisImporterHelper.SetupAndLinkTestFilePath(settings, importer, testFileLocation);

            //Act & Assert
            Assert.That(importer.ValidateNetworkFeatureFromGisImporterSettings(settings), Is.False);
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenGisFile_WhenImportItem_ThenBridgeContainsDataFromGisFile()
        {
            var importer = new BridgeYzFromGisImporter();
            
            importer.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            importer.FileBasedFeatureProviders.Add(BridgeFromGisImporterHelper.GetTestFileBasedFeatureProvider(testFileLocation));
            BridgeFromGisImporterHelper.SetupAndLinkHydroNetworkWithBranchesAndHighSnappingTolerance(importer, hydroNetwork);
            MapColumnsFromGisFile(importer);
            
            var data = importer.ImportItem("") as HydroNetwork;

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Bridges.Count(), Is.EqualTo(1));
            var importedBridge = data.Bridges.First();
            Assert.That(importedBridge.IsYz, Is.True);
            var yzDataTable = importedBridge.YZCrossSectionDefinition.YZDataTable;
            Assert.That(yzDataTable[0].Z, Is.EqualTo(2));
            Assert.That(yzDataTable[0].Yq, Is.EqualTo(-5));
            Assert.That(yzDataTable[1].Z, Is.EqualTo(0));
            Assert.That(yzDataTable[1].Yq, Is.EqualTo(-5));
            Assert.That(yzDataTable[2].Z, Is.EqualTo(0));
            Assert.That(yzDataTable[2].Yq, Is.EqualTo(5));
            Assert.That(yzDataTable[3].Z, Is.EqualTo(2));
            Assert.That(yzDataTable[3].Yq, Is.EqualTo(5));
            
        }
        
        private static void MapColumnsFromGisFile(BridgeFromGisImporterBase importer)
        {
            importer.FeatureFromGisImporterSettings.PropertiesMapping[0].MappingColumn.ColumnName = "Name";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[6].MappingColumn.ColumnName = "Yvalues";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[7].MappingColumn.ColumnName = "Zvalues";
        }

        private static IEnumerable<List<PropertyMapping>> GetPropertyMappingWithOneMissingProperty()
        {
            yield return new List<PropertyMapping>
            {
                propertyMappingYValues,
            };

            yield return new List<PropertyMapping>
            {
                propertyMappingZValues,
            };
        }
    }
}