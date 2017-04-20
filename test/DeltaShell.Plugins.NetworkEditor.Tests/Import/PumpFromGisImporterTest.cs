using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Data.Providers;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class PumpFromGisImporterTest
    {
        private ChannelFromGisImporter channelImporter;
        private PumpFromGisImporter pumpImporter;

        [SetUp]
        public void SetUp()
        {
            channelImporter = new ChannelFromGisImporter();
            channelImporter.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            channelImporter.FileBasedFeatureProviders.Add(new ShapeFile());
            channelImporter.FileBasedFeatureProviders.Add(new OgrFeatureProvider());
            channelImporter.HydroRegion = new HydroNetwork();

            pumpImporter = new PumpFromGisImporter();
            pumpImporter.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            pumpImporter.FileBasedFeatureProviders.Add(new ShapeFile());
            pumpImporter.FileBasedFeatureProviders.Add(new OgrFeatureProvider());
        }

        [Test]
        public void ImportPumpFromShape()
        {
            var pathChannels = TestHelper.GetTestFilePath("HydroBaseCF_ShapeFiles/Channels.shp");
            var pathPumps = TestHelper.GetTestFilePath("HydroBaseCF_ShapeFiles/Pumps.shp");
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var pumpImporterSettings = pumpImporter.FeatureFromGisImporterSettings;

            //First Channels
            channelImporterSettings.Path = pathChannels;

            PropertyMapping propertyMapping;

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "OVKIDENT";

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "OVK_NAME";

            var hydroNetwork = (HydroNetwork)channelImporter.ImportItem(null);

            Assert.Greater(hydroNetwork.Channels.Count(), 0);

            //Pump

            pumpImporterSettings.Path = pathPumps;
            pumpImporter.HydroRegion = hydroNetwork;

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "KWKIDENT";

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "KWK_NAAM";

            propertyMapping = pumpImporterSettings.PropertiesMapping.Where(property => property.PropertyName == "Capacity").First();
            propertyMapping.MappingColumn.ColumnName = "CAPACITY";

            propertyMapping = pumpImporterSettings.PropertiesMapping.Where(property => property.PropertyName == "Suction start").First();
            propertyMapping.MappingColumn.ColumnName = "SUC_START";

            propertyMapping = pumpImporterSettings.PropertiesMapping.Where(property => property.PropertyName == "Suction stop").First();
            propertyMapping.MappingColumn.ColumnName = "SUC_STOP";

            propertyMapping = pumpImporterSettings.PropertiesMapping.Where(property => property.PropertyName == "Delivery start").First();
            propertyMapping.MappingColumn.ColumnName = "PRS_START";

            propertyMapping = pumpImporterSettings.PropertiesMapping.Where(property => property.PropertyName == "Delivery stop").First();
            propertyMapping.MappingColumn.ColumnName = "PRS_STOP";

            hydroNetwork = (HydroNetwork)pumpImporter.ImportItem(null);

            Assert.AreEqual(10, hydroNetwork.Pumps.Count());

            var pump = hydroNetwork.Pumps.Where(p => p.Name == "GEM3").First();

            Assert.AreEqual(270, pump.Capacity,0.00001);
            Assert.AreEqual(-0.55, pump.StartSuction, 0.00001);
            Assert.AreEqual(-0.65, pump.StopSuction, 0.00001);
            Assert.AreEqual(0, pump.StartDelivery, 0.00001);
            Assert.AreEqual(0, pump.StopDelivery, 0.00001);
        }

        [Test]
        public void ImportPumpFromGeodatabase()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var pumpImporterSettings = pumpImporter.FeatureFromGisImporterSettings;

            //First Channels
            channelImporterSettings.Path = path;
            channelImporterSettings.TableName = "Channel";
            channelImporterSettings.GeometryColumn = new MappingColumn("Channel", "Shape");

            PropertyMapping propertyMapping;

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OVKIDENT";

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OVK_NAME";
            
            var hydroNetwork = (HydroNetwork)channelImporter.ImportItem(null);

            Assert.Greater(hydroNetwork.Channels.Count(), 0);

            //Pump

            pumpImporterSettings.Path = path;
            pumpImporterSettings.TableName = "Pump_station";
            pumpImporterSettings.ColumnNameID = "KWKIDENT";
            pumpImporterSettings.GeometryColumn = new MappingColumn("Pump_station", "Shape");
            pumpImporterSettings.RelatedTables.Add(new RelatedTable("Pump_station_def", "KWKIDENT"));
            pumpImporter.HydroRegion = hydroNetwork;

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "KWKIDENT";
            propertyMapping.MappingColumn.TableName = "Pump_station";

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "KWK_NAAM";
            propertyMapping.MappingColumn.TableName = "Pump_station";

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Capacity");
            propertyMapping.MappingColumn.ColumnName = "CAPACITY";
            propertyMapping.MappingColumn.TableName = "Pump_station_def";

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Suction start");
            propertyMapping.MappingColumn.ColumnName = "SUC_START";
            propertyMapping.MappingColumn.TableName = "Pump_station_def";

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Suction stop");
            propertyMapping.MappingColumn.ColumnName = "SUC_STOP";
            propertyMapping.MappingColumn.TableName = "Pump_station_def";

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Delivery start");
            propertyMapping.MappingColumn.ColumnName = "PRS_START";
            propertyMapping.MappingColumn.TableName = "Pump_station_def";

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Delivery stop");
            propertyMapping.MappingColumn.ColumnName = "PRS_STOP";
            propertyMapping.MappingColumn.TableName = "Pump_station_def";

            hydroNetwork = (HydroNetwork)pumpImporter.ImportItem(null);

            Assert.AreEqual(10, hydroNetwork.Pumps.Count());

            var pump = hydroNetwork.Pumps.First(p => p.Name == "GEM3");

            Assert.AreEqual(270, pump.Capacity);
            Assert.AreEqual(-0.55, pump.StartSuction);
            Assert.AreEqual(-0.65, pump.StopSuction);
            Assert.AreEqual(0, pump.StartDelivery);
            Assert.AreEqual(0, pump.StopDelivery);
        }
    }
}
