using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Data.Providers;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    [Category("Geodatabase_x86")]
    public class CrossSectionYZFromGisImporterTest
    {
        private ChannelFromGisImporter channelImporter;
        private CrossSectionYZFromGisImporter crossSectionImporter;

        [SetUp]
        public void SetUp()
         {
             channelImporter = new ChannelFromGisImporter
                 {
                     FileBasedFeatureProviders =
                         new List<IFileBasedFeatureProvider> {new ShapeFile(), new OgrFeatureProvider()},
                     HydroRegion = new HydroNetwork()
                 };

            crossSectionImporter = new CrossSectionYZFromGisImporter
                {
                    FileBasedFeatureProviders =
                        new List<IFileBasedFeatureProvider> {new ShapeFile(), new OgrFeatureProvider()}
                };
         }

        [Test]
        public void ImportYZCrossSectionFromGeodatabase()
         {
             var path = TestHelper.GetTestFilePath("testdataBase_CF.mdb");
             var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
             var csImporterSettings = crossSectionImporter.FeatureFromGisImporterSettings;

             //First Channels
             channelImporterSettings.Path = path;
             channelImporterSettings.TableName = "Channel";
             channelImporterSettings.GeometryColumn = new MappingColumn("Channel", "Shape");

             PropertyMapping propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
             propertyMapping.MappingColumn.TableName = "Channel";
             propertyMapping.MappingColumn.ColumnName = "OVKIDENT";

             propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
             propertyMapping.MappingColumn.TableName = "Channel";
             propertyMapping.MappingColumn.ColumnName = "OVK_NAME";

             var hydroNetwork = (HydroNetwork)channelImporter.ImportItem(null);

             Assert.Greater(hydroNetwork.Channels.Count(),0);
             
             //YZCrossSection

             csImporterSettings.Path = path;
             csImporterSettings.TableName = "Cross_section_definition";
             csImporterSettings.DiscriminatorColumn = "TYPE";
             csImporterSettings.DiscriminatorValue = "yz profiel";
             csImporterSettings.ColumnNameID = "PROIDENT";
             csImporterSettings.RelatedTables.Add(new RelatedTable("locations", "PROIDENT"));
             csImporterSettings.RelatedTables.Add(new RelatedTable("Cross_section_yz", "PROIDENT"));
             csImporterSettings.GeometryColumn = new MappingColumn("locations", "Shape");
             crossSectionImporter.HydroRegion = hydroNetwork;
             crossSectionImporter.SnappingTolerance = 10;

             //Name
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
             propertyMapping.MappingColumn.TableName = "locations";
             propertyMapping.MappingColumn.ColumnName = "LOCIDENT";

             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
             propertyMapping.MappingColumn.TableName = "Cross_section_definition";
             propertyMapping.MappingColumn.ColumnName = "PROIDENT";

             //YZ Values
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Y'-values");
             propertyMapping.MappingColumn.ColumnName = "DIST_MID";
             propertyMapping.MappingColumn.TableName = "Cross_section_yz";

             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Z-values");
             propertyMapping.MappingColumn.ColumnName = "BED_LVL";
             propertyMapping.MappingColumn.TableName = "Cross_section_yz";

             hydroNetwork = (HydroNetwork)crossSectionImporter.ImportItem(null);

             Assert.AreEqual(3,hydroNetwork.CrossSections.Count());

             var yz = new List<Coordinate>
                 {
                     new Coordinate(-2.5, 0.23),
                     new Coordinate(-2.3, -1.5),
                     new Coordinate(0, -1.6),
                     new Coordinate(2.2, -1.3),
                     new Coordinate(2.5, 0.27)
                 };

             var cs1 = hydroNetwork.CrossSections.First();

             Assert.AreEqual(yz, cs1.Definition.Profile);

         }

         [Test]
         public void ImportYZCrossSectionFromGeodatabase_NelenSchuurmans()
         {
             var path = TestHelper.GetTestFilePath("HydroBaseCF_GIOV_1DFlow.mdb");
             var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
             var csImporterSettings = crossSectionImporter.FeatureFromGisImporterSettings;

             //First Channels
             channelImporterSettings.Path = path;
             channelImporterSettings.TableName = "Channel";
             channelImporterSettings.GeometryColumn = new MappingColumn("Channel", "Shape");

             var propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
             propertyMapping.MappingColumn.TableName = "Channel";
             propertyMapping.MappingColumn.ColumnName = "OVKIDENT";

             propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
             propertyMapping.MappingColumn.TableName = "Channel";
             propertyMapping.MappingColumn.ColumnName = "OVK_NAME";

             var hydroNetwork = (HydroNetwork)channelImporter.ImportItem(null);

             Assert.Greater(hydroNetwork.Channels.Count(), 0);

             //YZCrossSection

             csImporterSettings.Path = path;
             csImporterSettings.TableName = "Cross_section_definition";
             csImporterSettings.DiscriminatorColumn = "TYPE";
             csImporterSettings.DiscriminatorValue = "yz profile";
             csImporterSettings.ColumnNameID = "PROIDENT";
             csImporterSettings.RelatedTables.Add(new RelatedTable("locations", "PROIDENT"));
             csImporterSettings.RelatedTables.Add(new RelatedTable("Cross_section_yz", "PROIDENT"));
             csImporterSettings.GeometryColumn = new MappingColumn("locations", "Shape");

             crossSectionImporter.HydroRegion = hydroNetwork;
             crossSectionImporter.SnappingTolerance = 30;

             //Name
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
             propertyMapping.MappingColumn.TableName = "locations";
             propertyMapping.MappingColumn.ColumnName = "LOCIDENT";

             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
             propertyMapping.MappingColumn.TableName = "Cross_section_definition";
             propertyMapping.MappingColumn.ColumnName = "PROIDENT";

             //ShiftLevel
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "ShiftLevel");
             propertyMapping.MappingColumn.TableName = "locations";
             propertyMapping.MappingColumn.ColumnName = "REF_LEVEL";

             //YZ Values
             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Y'-values");
             propertyMapping.MappingColumn.ColumnName = "DIST_MID";
             propertyMapping.MappingColumn.TableName = "Cross_section_yz";

             propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Z-values");
             propertyMapping.MappingColumn.ColumnName = "BED_LVL";
             propertyMapping.MappingColumn.TableName = "Cross_section_yz";

             hydroNetwork = (HydroNetwork)crossSectionImporter.ImportItem(null);

             Assert.AreEqual(537, hydroNetwork.CrossSections.Count());

             ////cross-section Loc 1
            //Chainage = 8574.36659298727

            //ShiftLevel = 12.69

            //Y   Z (no shift)
            //-220.1	17.76
            //0	17.76
            //27	16.46
            //28	16.16
            //32	15.22
            //34	14.79
            //36	14.19
            //38	13.49
            //39	12.89
            //40	12.79
            //41	12.79
            //41.5	12.69
            //42	12.69
            //43	12.89
            //45	13.39
            //48	13.99
            //53	14.19
            //54	14.79
            //55	15.11
            //65	15.52
            //70	15.75
            //80	16.34
            //100	16.48
            //150	16.81
            //200	17
            //215	17.03
            //250	17.33


             var yz = new List<Coordinate>
                 {
                     new Coordinate(-220.1, 30.45),
                     new Coordinate(0, 30.45),
                     new Coordinate(27, 29.15),
                     new Coordinate(28, 28.85),
                     new Coordinate(32, 27.91),
                     new Coordinate(34, 27.48),
                     new Coordinate(36, 26.88),
                     new Coordinate(38, 26.18),
                     new Coordinate(39, 25.58),
                     new Coordinate(40, 25.48),
                     new Coordinate(41, 25.48),
                     new Coordinate(41.5, 25.38),
                     new Coordinate(42, 25.38),
                     new Coordinate(43, 25.58),
                     new Coordinate(45, 26.08),
                     new Coordinate(48, 26.68),
                     new Coordinate(53, 26.88),
                     new Coordinate(54, 27.48),
                     new Coordinate(55, 27.8),
                     new Coordinate(65, 28.21),
                     new Coordinate(70, 28.44),
                     new Coordinate(80, 29.03),
                     new Coordinate(100, 29.17),
                     new Coordinate(150, 29.5),
                     new Coordinate(200, 29.69),
                     new Coordinate(215, 29.72),
                     new Coordinate(250, 30.02)
                 };

             var cs1 = hydroNetwork.CrossSections.First(cs => cs.Name == "Loc_1");
             var profile = cs1.Definition.Profile.ToArray();

             Assert.AreEqual(8574.367, cs1.Chainage, 0.01);
             Assert.AreEqual(yz.Count, profile.Count());

             for(int i = 0; i < yz.Count; i++)
             {
                 Assert.AreEqual(yz[i].X, profile[i].X);
                 Assert.AreEqual(yz[i].Y, profile[i].Y, 0.01);
             }

            // crosssection Loc_2
            //Chainage 4822.7222784043333

            //ShiftLevel 14.13

             //Y   Z (no shift)
            //-248	18.5
            //22	17.83
            //25	17.24
            //27	16.96
            //28	16.23
            //29	15.93
            //30	15.43
            //31	14.73
            //32	14.63
            //34	14.53
            //36	14.33
            //38	14.13
            //40	14.33
            //42	15.13
            //43	15.53
            //44	15.93
            //44.5	16.23
            //45	16.53
            //46	17.1
            //48	17.54
            //50	18.21
            //375	19
             var cs2 = hydroNetwork.CrossSections.First(cs => cs.Name == "Loc_2");

             yz = new List<Coordinate>
                 {
                     new Coordinate(-248, 32.63),
                     new Coordinate(22, 31.96),
                     new Coordinate(25, 31.37),
                     new Coordinate(27, 31.09),
                     new Coordinate(28, 30.36),
                     new Coordinate(29, 30.06),
                     new Coordinate(30, 29.56),
                     new Coordinate(31, 28.86),
                     new Coordinate(32, 28.76),
                     new Coordinate(34, 28.66),
                     new Coordinate(36, 28.46),
                     new Coordinate(38, 28.26),
                     new Coordinate(40, 28.46),
                     new Coordinate(42, 29.26),
                     new Coordinate(43, 29.66),
                     new Coordinate(44, 30.06),
                     new Coordinate(44.5, 30.36),
                     new Coordinate(45, 30.66),
                     new Coordinate(46, 31.23),
                     new Coordinate(48, 31.67),
                     new Coordinate(50, 32.34),
                     new Coordinate(375, 33.13)
                 };


             profile = cs2.Definition.Profile.ToArray();

             Assert.AreEqual(4822.7222784043333, cs2.Chainage, 0.01);
             Assert.AreEqual(yz.Count, profile.Count());

             for (int i = 0; i < yz.Count; i++)
             {
                 Assert.AreEqual(yz[i].X, profile[i].X);
                 Assert.AreEqual(yz[i].Y, profile[i].Y, 0.01);
             }

         }
    }
}
