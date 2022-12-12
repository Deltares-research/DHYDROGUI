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
    public class BridgeTabulatedZwFromGisImporterTest
    {
        private const string testFileLocation = "BridgeFromGisImporter_ShapeFiles/bridge_zw_test.shp";
        private IHydroNetwork hydroNetwork;
        
        [Test]
        public void Constructor_InitializesCorrectly()
        {
            //Arrange & Act
            var importer = new BridgeZwFromGisImporter();

            //Assert
            Assert.That(importer.Name, Is.EqualTo("Bridge ZW from GIS importer"));
            Assert.That(importer.NumberOfLevels, Is.EqualTo(3));
            Assert.That(importer.FeatureFromGisImporterSettings.FeatureType, Is.EqualTo("Bridges (ZW profile)"));
        }

        [Test]
        public void GivenNumberOfLevel_WhenNumberOfLevelsSet_ThenNumberOfLevelsIsAsExpected()
        {
            //Arrange
            var importer = new BridgeZwFromGisImporter();
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
            var importer = new BridgeZwFromGisImporter();
            var baseImporter = new BridgeFromGisImporterHelper.TestBaseBridge();
            FeatureFromGisImporterSettings settings = baseImporter.FeatureFromGisImporterSettings;
            BridgeFromGisImporterHelper.SetupAndLinkTestFilePath(settings, importer, testFileLocation);

            for (var i = 0; i <= importer.NumberOfLevels; i++)
            {
                settings.PropertiesMapping.Add(new PropertyMapping($"Width {i}") { PropertyUnit = "m" });
                settings.PropertiesMapping.Add(new PropertyMapping($"{ZwFromGisImporter.LblLevel} {i}"));
            }

            Assert.That(importer.ValidateNetworkFeatureFromGisImporterSettings(settings), Is.True);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenIncorrectSettings_WhenValidatingNetworkWithTestFilePath_ThenReturnFalseForValidation()
        {
            var importer = new BridgeZwFromGisImporter();
            var baseImporter = new BridgeFromGisImporterHelper.TestBaseBridge();
            FeatureFromGisImporterSettings settings = baseImporter.FeatureFromGisImporterSettings;
            BridgeFromGisImporterHelper.SetupAndLinkTestFilePath(settings, importer, testFileLocation);

            settings.PropertiesMapping.Add(new PropertyMapping($"Width {1}") { PropertyUnit = "m" });
            settings.PropertiesMapping.Add(new PropertyMapping($"{ZwFromGisImporter.LblLevel} {1}"));

            Assert.That(importer.ValidateNetworkFeatureFromGisImporterSettings(settings), Is.False);
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenGisFile_WhenImportItem_ThenBridgeContainsDataFromGisFile()
        {
            var importer = new BridgeZwFromGisImporter();

            importer.NumberOfLevels = 2;
            importer.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            importer.FileBasedFeatureProviders.Add(BridgeFromGisImporterHelper.GetTestFileBasedFeatureProvider(testFileLocation));
            BridgeFromGisImporterHelper.SetupAndLinkHydroNetworkWithBranchesAndHighSnappingTolerance(importer, hydroNetwork);
            MapColumnsFromGisFile(importer);
            
            var data = importer.ImportItem("") as HydroNetwork;

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Bridges.Count(), Is.EqualTo(1));
            var importedBridge = data.Bridges.First();
            Assert.That(importedBridge.IsTabulated, Is.True);
            var zwDataTable = importedBridge.TabulatedCrossSectionDefinition.ZWDataTable;
            Assert.That(zwDataTable[0].Z, Is.EqualTo(2));
            Assert.That(zwDataTable[0].Width, Is.EqualTo(10.01));
            Assert.That(zwDataTable[1].Z, Is.EqualTo(0));
            Assert.That(zwDataTable[1].Width, Is.EqualTo(10));
        }

        private static void MapColumnsFromGisFile(BridgeFromGisImporterBase importer)
        {
            importer.FeatureFromGisImporterSettings.PropertiesMapping[0].MappingColumn.ColumnName = "Name";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[6].MappingColumn.ColumnName = "LEVEL1";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[7].MappingColumn.ColumnName = "WIDTH1";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[8].MappingColumn.ColumnName = "LEVEL2";
            importer.FeatureFromGisImporterSettings.PropertiesMapping[9].MappingColumn.ColumnName = "WIDTH2";
        }
    }
}