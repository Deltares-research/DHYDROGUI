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
    [Category("Geodatabase_x86")]
    public class BridgeFromGisImporterTest
    {
        private ChannelFromGisImporter channelImporter;
        private BridgeFromGisImporter bridgeImporter;

        [SetUp]
        public void SetUp()
        {
            channelImporter = new ChannelFromGisImporter();
            channelImporter.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            channelImporter.FileBasedFeatureProviders.Add(new ShapeFile());
            channelImporter.FileBasedFeatureProviders.Add(new OgrFeatureProvider());
            channelImporter.HydroRegion = new HydroNetwork();

            bridgeImporter = new BridgeFromGisImporter();
            bridgeImporter.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            bridgeImporter.FileBasedFeatureProviders.Add(new ShapeFile());
            bridgeImporter.FileBasedFeatureProviders.Add(new OgrFeatureProvider());
        }

        [Test]
        public void ImportBridgeFromGeodatabase()
        {
            var path = TestHelper.GetTestFilePath("testdataBase_CF.mdb");
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var bridgeImporterSettings = bridgeImporter.FeatureFromGisImporterSettings;

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

            //Bridge
            bridgeImporter.HydroRegion = hydroNetwork;
            bridgeImporter.SnappingTolerance = 1000;

            bridgeImporterSettings.Path = path;
            bridgeImporterSettings.TableName = "Bridge";
            bridgeImporterSettings.DiscriminatorColumn = "Type";
            bridgeImporterSettings.DiscriminatorValue = "Abutment";
            bridgeImporterSettings.GeometryColumn = new MappingColumn("Bridge", "Shape");

            propertyMapping = bridgeImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Bridge";
            propertyMapping.MappingColumn.ColumnName = "KWKIDENT";

            propertyMapping = bridgeImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Bridge";
            propertyMapping.MappingColumn.ColumnName = "KWK_NAME";

            propertyMapping = bridgeImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Bed level");
            propertyMapping.MappingColumn.TableName = "Bridge";
            propertyMapping.MappingColumn.ColumnName = "BED_LVL";

            propertyMapping = bridgeImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Width");
            propertyMapping.MappingColumn.TableName = "Bridge";
            propertyMapping.MappingColumn.ColumnName = "WIDTH";

            // no height mapping: TOP_LVL - BED_LVL :( not possible

            propertyMapping = bridgeImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Length");
            propertyMapping.MappingColumn.TableName = "Bridge";
            propertyMapping.MappingColumn.ColumnName = "LENGTH";

            propertyMapping = bridgeImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Roughness");
            propertyMapping.MappingColumn.TableName = "Bridge";
            propertyMapping.MappingColumn.ColumnName = "FRICTION";


            hydroNetwork = (HydroNetwork)bridgeImporter.ImportItem(null);
 
            Assert.AreEqual(1, hydroNetwork.Bridges.Count());

            var bridge = hydroNetwork.Bridges.First();

            //OBJECTID	Shape	KWKIDENT	KWK_NAME	TYPE	LENGTH	WIDTH	BED_LVL	TOP_LVL	PILL_WIDTH	PILL_FF	FRICTION	SOURCE	DATE_TIME	COMMENTS
            //1		KBR_1	Sobekbrug	Abutment	5.6	3.1	-2.411	-0.23	-99999	-99999	71	

            Assert.AreEqual("KBR_1", bridge.Name);
            Assert.AreEqual("Sobekbrug", bridge.LongName);
            Assert.AreEqual(5.6, bridge.Length);
            Assert.AreEqual(3.1, bridge.Width);
            Assert.AreEqual(-2.411, bridge.BottomLevel);
            Assert.AreEqual(71, bridge.Friction);

        }
    }
}

