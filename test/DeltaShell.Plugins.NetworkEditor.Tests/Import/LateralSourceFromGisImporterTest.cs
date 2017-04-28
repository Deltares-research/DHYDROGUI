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
    public class LateralSourceFromGisImporterTest
    {
        private ChannelFromGisImporter channelImporter;
        private LateralSourceFromGisImporter lateralSourceImporter;

        [SetUp]
        public void SetUp()
        {
            channelImporter = new ChannelFromGisImporter();
            channelImporter.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            channelImporter.FileBasedFeatureProviders.Add(new ShapeFile());
            channelImporter.FileBasedFeatureProviders.Add(new OgrFeatureProvider());
            channelImporter.HydroRegion = new HydroNetwork();

            lateralSourceImporter = new LateralSourceFromGisImporter();
            lateralSourceImporter.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            lateralSourceImporter.FileBasedFeatureProviders.Add(new ShapeFile());
            lateralSourceImporter.FileBasedFeatureProviders.Add(new OgrFeatureProvider());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportLateralSourceFromGeodatabase()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var lateralSourceImporterSettings = lateralSourceImporter.FeatureFromGisImporterSettings;

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

            //LateralSource

            lateralSourceImporterSettings.Path = path;
            lateralSourceImporterSettings.TableName = "Lateral_Flow";
            lateralSourceImporterSettings.GeometryColumn = new MappingColumn("Lateral_Flow", "Shape");
            lateralSourceImporter.HydroRegion = hydroNetwork;
            lateralSourceImporter.SnappingTolerance = 500;

            propertyMapping = lateralSourceImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Lateral_Flow";
            propertyMapping.MappingColumn.ColumnName = "OBJECTID";

            propertyMapping = lateralSourceImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Lateral_Flow";
            propertyMapping.MappingColumn.ColumnName = "LAT_IDENT";

            hydroNetwork = (HydroNetwork)lateralSourceImporter.ImportItem(null);

            Assert.AreEqual(118, hydroNetwork.LateralSources.Count());

        }
    }
}