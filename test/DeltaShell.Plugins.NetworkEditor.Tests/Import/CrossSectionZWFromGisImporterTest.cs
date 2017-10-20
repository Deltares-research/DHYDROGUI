using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
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
    public class CrossSectionZWFromGisImporterTest
    {
        private ChannelFromGisImporter channelImporter;
        private CrossSectionZWFromGisImporter crossSectionImporter;

        [SetUp]
        public void SetUp()
        {
             channelImporter = new ChannelFromGisImporter
                 {
                     FileBasedFeatureProviders = new List<IFileBasedFeatureProvider> {new ShapeFile(), new OgrFeatureProvider()},
                     HydroRegion = new HydroNetwork()
                 };

            crossSectionImporter = new CrossSectionZWFromGisImporter
                {
                    FileBasedFeatureProviders = new List<IFileBasedFeatureProvider> {new ShapeFile(), new OgrFeatureProvider()},
                    SnappingTolerance = 10
                };
        }

        [Test]
        public void CrossSectionZWNumberOfLevelsPropertyMapping()
         {
            var basicNumberOfPropertiesMapping = crossSectionImporter.FeatureFromGisImporterSettings.PropertiesMapping.Count - (3 * crossSectionImporter.NumberOfLevels);

            var n = 10;
            crossSectionImporter.NumberOfLevels = n;
            Assert.AreEqual((basicNumberOfPropertiesMapping + (3 * n)), crossSectionImporter.FeatureFromGisImporterSettings.PropertiesMapping.Count);

            n = 1;
            crossSectionImporter.NumberOfLevels = n;
            Assert.AreEqual((basicNumberOfPropertiesMapping + (3 * n)), crossSectionImporter.FeatureFromGisImporterSettings.PropertiesMapping.Count);

            n = 4;
            crossSectionImporter.NumberOfLevels = n;
            Assert.AreEqual((basicNumberOfPropertiesMapping + (3 * n)), crossSectionImporter.FeatureFromGisImporterSettings.PropertiesMapping.Count);
             
         }

        [Test]
        public void ImportCrossSectionsZWFromShapeFile()
        {
            var pathChannel = TestHelper.GetTestFilePath("HydroBaseCF_ShapeFiles/Channels.shp");
            var pathCS = TestHelper.GetTestFilePath("HydroBaseCF_ShapeFiles/CrossSections.shp");
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var csImporterSettings = crossSectionImporter.FeatureFromGisImporterSettings;

            //First Channels
            channelImporterSettings.Path = pathChannel;

            var propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "OVKIDENT";

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "OVK_NAME";


            var hydroNetwork = (HydroNetwork)channelImporter.ImportItem(null);

            Assert.AreEqual(172, hydroNetwork.Channels.Count());

            csImporterSettings.Path = pathCS;

            crossSectionImporter.HydroRegion = hydroNetwork;
            crossSectionImporter.NumberOfLevels = 3;

            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "PROIDENT";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "PROIDENT";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Level 1");
            propertyMapping.MappingColumn.ColumnName = "BED_LVL";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Flow width 1");
            propertyMapping.MappingColumn.ColumnName = "BED_WDTH_M";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Storage width 1");
            propertyMapping.MappingColumn.ColumnName = "BED_WDTH";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Level 2");
            propertyMapping.MappingColumn.ColumnName = "WAT_LVL";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Flow width 2");
            propertyMapping.MappingColumn.ColumnName = "WAT_WDTH_M";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Storage width 2");
            propertyMapping.MappingColumn.ColumnName = "WAT_WDTH";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Level 3");
            propertyMapping.MappingColumn.ColumnName = "SUR_LVL";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Flow width 3");
            propertyMapping.MappingColumn.ColumnName = "SUR_WDTH_M";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Storage width 3");
            propertyMapping.MappingColumn.ColumnName = "SUR_WDTH";

            hydroNetwork = (HydroNetwork)crossSectionImporter.ImportItem(null);

            Assert.AreEqual(349, hydroNetwork.CrossSections.Count());
            Assert.AreEqual(CrossSectionType.ZW, hydroNetwork.CrossSections.First().CrossSectionType);
        }

        [Test]
        public void ImportCrossSectionsZWFromGeodatabase()
         {
             var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
             var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
             var csImporterSettings = crossSectionImporter.FeatureFromGisImporterSettings;

             //First Channels
             channelImporterSettings.Path = path;
             channelImporterSettings.TableName = "Channel";
             channelImporterSettings.GeometryColumn = new MappingColumn("Channel", "Shape");

             var propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
             propertyMapping.MappingColumn.TableName = "Channel";
             propertyMapping.MappingColumn.ColumnName = "OBJECTID";

             propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
             propertyMapping.MappingColumn.TableName = "Channel";
             propertyMapping.MappingColumn.ColumnName = "OVK_NAME";


             var hydroNetwork = (HydroNetwork)channelImporter.ImportItem(null);

             Assert.Greater(hydroNetwork.Channels.Count(),0);
             
             //WHCrossSection

             csImporterSettings.Path = path;
             csImporterSettings.TableName = "Cross_section_definition";
             csImporterSettings.ColumnNameID = "PROIDENT";
             csImporterSettings.RelatedTables.Add(new RelatedTable("locations", "PROIDENT"));
             csImporterSettings.GeometryColumn = new MappingColumn("locations", "Shape");
             crossSectionImporter.HydroRegion = hydroNetwork;

             //Name
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
             propertyMapping.MappingColumn.TableName = "Locations";
             propertyMapping.MappingColumn.ColumnName = "LOCIDENT";

             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
             propertyMapping.MappingColumn.TableName = "Locations";
             propertyMapping.MappingColumn.ColumnName = "LOCIDENT";

             crossSectionImporter.NumberOfLevels = 3;

             //WH
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Level 1");
             propertyMapping.MappingColumn.TableName = "Cross_section_definition";
             propertyMapping.MappingColumn.ColumnName = "BED_LVL";
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Flow width 1");
             propertyMapping.MappingColumn.TableName = "Cross_section_definition";
             propertyMapping.MappingColumn.ColumnName = "BED_WDTH_M";
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Storage width 1");
             propertyMapping.MappingColumn.TableName = "Cross_section_definition";
             propertyMapping.MappingColumn.ColumnName = "BED_WDTH";
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Level 2");
             propertyMapping.MappingColumn.TableName = "Cross_section_definition";
             propertyMapping.MappingColumn.ColumnName = "WAT_LVL";
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Flow width 2");
             propertyMapping.MappingColumn.TableName = "Cross_section_definition";
             propertyMapping.MappingColumn.ColumnName = "WAT_WDTH_M";
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Storage width 2");
             propertyMapping.MappingColumn.TableName = "Cross_section_definition";
             propertyMapping.MappingColumn.ColumnName = "WAT_WDTH";
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Level 3");
             propertyMapping.MappingColumn.TableName = "Cross_section_definition";
             propertyMapping.MappingColumn.ColumnName = "SUR_LVL";
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Flow width 3");
             propertyMapping.MappingColumn.TableName = "Cross_section_definition";
             propertyMapping.MappingColumn.ColumnName = "SUR_WDTH_M";
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Storage width 3");
             propertyMapping.MappingColumn.TableName = "Cross_section_definition";
             propertyMapping.MappingColumn.ColumnName = "SUR_WDTH";

             hydroNetwork = (HydroNetwork)crossSectionImporter.ImportItem(null);

            Assert.AreEqual(349, hydroNetwork.CrossSections.Count());

         }

        [Test]
        public void ImportCrossSectionsZWFromGeodatabaseManyLocations()
        {
            var path = TestHelper.GetTestFilePath("testdatabase_CF.mdb");
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var csImporterSettings = crossSectionImporter.FeatureFromGisImporterSettings;

            //First Channels
            channelImporterSettings.Path = path;
            channelImporterSettings.TableName = "Channel";
            channelImporterSettings.GeometryColumn = new MappingColumn("Channel", "Shape");

            var propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OBJECTID";

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OVK_NAME";

            var hydroNetwork = (HydroNetwork)channelImporter.ImportItem(null);

            Assert.Greater(hydroNetwork.Channels.Count(), 0);

            //WHCrossSection

            csImporterSettings.Path = path;
            csImporterSettings.TableName = "Cross_section_definition";
            csImporterSettings.DiscriminatorColumn = "TYPE";
            csImporterSettings.DiscriminatorValue = "tabulated";
            csImporterSettings.ColumnNameID = "PROIDENT";
            csImporterSettings.RelatedTables.Add(new RelatedTable("locations", "PROIDENT"));
            csImporterSettings.GeometryColumn = new MappingColumn("locations", "Shape");
            crossSectionImporter.HydroRegion = hydroNetwork;

            //Name
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Locations";
            propertyMapping.MappingColumn.ColumnName = "LOCIDENT";

            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Cross_section_definition";
            propertyMapping.MappingColumn.ColumnName = "PROIDENT";

            crossSectionImporter.NumberOfLevels = 3;

            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Level 1");
            propertyMapping.MappingColumn.TableName = "Cross_section_definition";
            propertyMapping.MappingColumn.ColumnName = "BED_LVL";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Flow width 1");
            propertyMapping.MappingColumn.TableName = "Cross_section_definition";
            propertyMapping.MappingColumn.ColumnName = "BED_WDTH_M";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Storage width 1");
            propertyMapping.MappingColumn.TableName = "Cross_section_definition";
            propertyMapping.MappingColumn.ColumnName = "BED_WDTH";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Level 2");
            propertyMapping.MappingColumn.TableName = "Cross_section_definition";
            propertyMapping.MappingColumn.ColumnName = "WAT_LVL";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Flow width 2");
            propertyMapping.MappingColumn.TableName = "Cross_section_definition";
            propertyMapping.MappingColumn.ColumnName = "WAT_WDTH_M";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Storage width 2");
            propertyMapping.MappingColumn.TableName = "Cross_section_definition";
            propertyMapping.MappingColumn.ColumnName = "WAT_WDTH";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Level 3");
            propertyMapping.MappingColumn.TableName = "Cross_section_definition";
            propertyMapping.MappingColumn.ColumnName = "SUR_LVL";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Flow width 3");
            propertyMapping.MappingColumn.TableName = "Cross_section_definition";
            propertyMapping.MappingColumn.ColumnName = "SUR_LVL_M";
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Storage width 3");
            propertyMapping.MappingColumn.TableName = "Cross_section_definition";
            propertyMapping.MappingColumn.ColumnName = "SUR_WDTH";

            hydroNetwork = (HydroNetwork)crossSectionImporter.ImportItem(null);

            Assert.AreEqual(3, hydroNetwork.CrossSections.Count());
        }
    }
}

