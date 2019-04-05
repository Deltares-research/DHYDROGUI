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
    [Category(TestCategory.X86)]
    public class ObservationPointFromGisImporterTest
    {
        private ChannelFromGisImporter channelImporter;
        private ObservationPointFromGisImporter observationPointImporter;

        [SetUp]
        public void SetUp()
        {
            channelImporter = new ChannelFromGisImporter();
            channelImporter.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            channelImporter.FileBasedFeatureProviders.Add(new ShapeFile());
            channelImporter.FileBasedFeatureProviders.Add(new OgrFeatureProvider());
            channelImporter.HydroRegion = new HydroNetwork();

            observationPointImporter = new ObservationPointFromGisImporter();
            observationPointImporter.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            observationPointImporter.FileBasedFeatureProviders.Add(new ShapeFile());
            observationPointImporter.FileBasedFeatureProviders.Add(new OgrFeatureProvider());
        }

        [Test]
        public void ImportObservationPointFromShape()
        {
            var pathChannels = TestHelper.GetTestFilePath("HydroBaseCF_ShapeFiles/Channels.shp");
            var pathPumps = TestHelper.GetTestFilePath("HydroBaseCF_ShapeFiles/Pumps.shp"); //pumps are used: locations for observation points
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var pumpImporterSettings = observationPointImporter.FeatureFromGisImporterSettings;

            //First Channels
            channelImporterSettings.Path = pathChannels;

            PropertyMapping propertyMapping;

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "OVKIDENT";

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "OVK_NAME";

            var hydroNetwork = (HydroNetwork)channelImporter.ImportItem(null);

            Assert.Greater(hydroNetwork.Channels.Count(), 0);

            //Observation points

            pumpImporterSettings.Path = pathPumps;
            observationPointImporter.HydroRegion = hydroNetwork;

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "KWKIDENT";

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "KWK_NAAM";


            hydroNetwork = (HydroNetwork)observationPointImporter.ImportItem(null);

            Assert.AreEqual(10, hydroNetwork.ObservationPoints.Count());

            var observationPoint = hydroNetwork.ObservationPoints.Where(p => p.Name == "GEM3").FirstOrDefault();

            Assert.IsNotNull(observationPoint);
        }

        [Test]
        public void ImportObservationPointFromGeodatabase()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var pumpImporterSettings = observationPointImporter.FeatureFromGisImporterSettings;

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

            //ObservationPoint

            pumpImporterSettings.Path = path;
            pumpImporterSettings.TableName = "Pump_station"; //We use pumps as observation points for this test
            pumpImporterSettings.ColumnNameID = "KWKIDENT";
            pumpImporterSettings.GeometryColumn = new MappingColumn("Pump_station", "Shape");
            pumpImporterSettings.RelatedTables.Add(new RelatedTable("Pump_station_def", "KWKIDENT"));
            observationPointImporter.HydroRegion = hydroNetwork;

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "KWKIDENT";
            propertyMapping.MappingColumn.TableName = "Pump_station";

            propertyMapping = pumpImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "KWK_NAAM";
            propertyMapping.MappingColumn.TableName = "Pump_station";

            hydroNetwork = (HydroNetwork)observationPointImporter.ImportItem(null);

            Assert.AreEqual(10, hydroNetwork.ObservationPoints.Count());

            var observationPoint = hydroNetwork.ObservationPoints.Where(p => p.Name == "GEM3").FirstOrDefault();

            Assert.IsNotNull(observationPoint);
        }
    }
}
