using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Wizard;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class HydroRegionFromGisImporterTest
    {
        private HydroRegionFromGisImporter importer;
        private ImportHydroNetworkFromGisWizardDialog wizard;

        [SetUp]
        public void SetUp()
         {
             importer = new HydroRegionFromGisImporter();
             importer.HydroRegion = new HydroNetwork();
         }

         [Test]
        [Category(TestCategory.DataAccess)]
         public void OgrAndSQL()
         {
             var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
             var ogrFeatureProvider = new OgrFeatureProvider(path);
             ogrFeatureProvider.OpenLayerWithSQL("Select Cross_section_definition.*,locations.Shape FROM Cross_section_definition, locations WHERE locations.PROIDENT = Cross_section_definition.PROIDENT");

             Assert.AreEqual(349, ogrFeatureProvider.Features.Count);
         }

         [Test]
         [Category(TestCategory.DataAccess)]
         public void OgrAndSQLWithRelatedTable()
         {
             var path = TestHelper.GetTestFilePath("testdatabase_CF.mdb");
             var sql =
                 "SELECT Cross_section_definition.*, [locations.SHAPE],[locations.SOURCE] AS 'datasource' FROM Cross_section_definition, [locations] WHERE [locations.PROIDENT] = [Cross_section_definition.PROIDENT]";
             var ogrFeatureProvider = new OgrFeatureProvider(path);
             ogrFeatureProvider.OpenLayerWithSQL(sql);

             Assert.AreEqual(16, ogrFeatureProvider.Features.Count);
         }

         [Test]
         [Category(TestCategory.DataAccess)]
         public void OgrFeatureProviderOpenWithSQLTest()
         {
             var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
             var provider = new OgrFeatureProvider();
             var sql = "SELECT Cross_section_definition.*, [locations.Shape] FROM Cross_section_definition, locations WHERE [Cross_section_definition.TYPE] = 'tabulated' AND [locations.PROIDENT] = [Cross_section_definition.PROIDENT]";
             provider.Path = path;
             provider.OpenLayerWithSQL(sql);

             var features = provider.Features;

             Assert.AreEqual("No Error", "No Error");
         }

         [Test]
         [Category(TestCategory.DataAccess)]
         [Category(TestCategory.Slow)]
         public void SerializeAndDeserialize()
         {
             HydroRegionFromGisImporter hydroRegionFromGisImporter = new HydroRegionFromGisImporter();

             var pathXML = TestHelper.GetTestFilePath("SerializeAndDeserializeChannelFromGisImporter.xml");
             Assert.IsFalse(String.IsNullOrEmpty(pathXML), "Invalid path");
             var channelImporter = new ChannelFromGisImporter();
             channelImporter.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>() { new OgrFeatureProvider() };
             var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;

             channelImporterSettings.Path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
             channelImporterSettings.DiscriminatorColumn = "hahaColumn";
             channelImporterSettings.DiscriminatorValue = "hihiColumn";
             channelImporterSettings.ColumnNameID = "ChannelID";
             channelImporterSettings.TableName = "Channel";
             channelImporterSettings.GeometryColumn = new MappingColumn("Channel", "Shape");
             channelImporterSettings.RelatedTables.Add(new RelatedTable("haha", "hihi"));
             channelImporterSettings.RelatedTables.Add(new RelatedTable("hoho", "hehe"));

             PropertyMapping propertyMapping;

             propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
             propertyMapping.MappingColumn.TableName = "Channel";
             propertyMapping.MappingColumn.ColumnName = "OVKIDENT";

             // longName not set, so IsNullValue = true

             hydroRegionFromGisImporter.FeatureFromGisImporters.Add(channelImporter);

             HydroNetworkFromGisImporterXmlSerializer.Serialize(hydroRegionFromGisImporter, pathXML + "_test_output");

             var loadedHydroNetworkFromGisImporter = new HydroRegionFromGisImporter();
             HydroNetworkFromGisImporterXmlSerializer.Deserialize(loadedHydroNetworkFromGisImporter, pathXML + "_test_output");
             var loadedChannelImporter = loadedHydroNetworkFromGisImporter.FeatureFromGisImporters.OfType<ChannelFromGisImporter>().First();
             var loadedChannelImporterSettings = loadedChannelImporter.FeatureFromGisImporterSettings;

             Assert.AreEqual(channelImporterSettings.Path, loadedChannelImporterSettings.Path);
             Assert.AreEqual(channelImporterSettings.TableName, loadedChannelImporterSettings.TableName);
             Assert.AreEqual(channelImporterSettings.DiscriminatorColumn, loadedChannelImporterSettings.DiscriminatorColumn);
             Assert.AreEqual(channelImporterSettings.DiscriminatorValue, loadedChannelImporterSettings.DiscriminatorValue);
             Assert.AreEqual(channelImporterSettings.GeometryColumn.ColumnName, loadedChannelImporterSettings.GeometryColumn.ColumnName);
             Assert.AreEqual(channelImporterSettings.GeometryColumn.TableName, loadedChannelImporterSettings.GeometryColumn.TableName);

             Assert.AreEqual(channelImporterSettings.RelatedTables[0].TableName, loadedChannelImporterSettings.RelatedTables[0].TableName);
             Assert.AreEqual(channelImporterSettings.RelatedTables[0].ForeignKeyColumnName, loadedChannelImporterSettings.RelatedTables[0].ForeignKeyColumnName);
             Assert.AreEqual(channelImporterSettings.RelatedTables[1].TableName, loadedChannelImporterSettings.RelatedTables[1].TableName);
             Assert.AreEqual(channelImporterSettings.RelatedTables[1].ForeignKeyColumnName, loadedChannelImporterSettings.RelatedTables[1].ForeignKeyColumnName);

             Assert.AreEqual(channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name").MappingColumn.ColumnName,
                 loadedChannelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name").MappingColumn.ColumnName
                 );
             Assert.AreEqual(channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name").MappingColumn.TableName,
              loadedChannelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name").MappingColumn.TableName
              );
             Assert.AreEqual(channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName").MappingColumn.IsNullValue,
              loadedChannelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName").MappingColumn.IsNullValue
              );

         }
    }
}
