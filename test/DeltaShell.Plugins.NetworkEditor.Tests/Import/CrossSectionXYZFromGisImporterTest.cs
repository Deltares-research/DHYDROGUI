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
    public class CrossSectionXYZFromGisImporterTest
    {
        private ChannelFromGisImporter channelImporter;
        private CrossSectionXYZFromGisImporter crossSectionImporter;

        [SetUp]
        public void SetUp()
        {
            channelImporter = new ChannelFromGisImporter
                {
                    FileBasedFeatureProviders =
                        new List<IFileBasedFeatureProvider> {new ShapeFile(), new OgrFeatureProvider()},
                    HydroRegion = new HydroNetwork()
                };

            crossSectionImporter = new CrossSectionXYZFromGisImporter
                {
                    FileBasedFeatureProviders =
                        new List<IFileBasedFeatureProvider> {new ShapeFile(), new OgrFeatureProvider()},
                    SnappingTolerance = 10
                };
        }
        
        [Test]
        public void ImportCrossSectionXYZFromGeodatabase()
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

            Assert.Greater(hydroNetwork.Channels.Count(), 0);

            csImporterSettings.Path = path;
            csImporterSettings.TableName = "Cross_section_definition";
            csImporterSettings.DiscriminatorColumn = "TYPE";
            csImporterSettings.DiscriminatorValue = "xyz profiel";
            csImporterSettings.ColumnNameID = "PROIDENT";
            csImporterSettings.RelatedTables.Add(new RelatedTable("points_xyz", "PROIDENT"));
            csImporterSettings.GeometryColumn = new MappingColumn("points_xyz", "SHAPE");
            crossSectionImporter.HydroRegion = hydroNetwork;

            //Name
            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Cross_section_definition";
            propertyMapping.MappingColumn.ColumnName = "PROIDENT";

            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Cross_section_definition";
            propertyMapping.MappingColumn.ColumnName = "PROIDENT";

            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Z-values");
            propertyMapping.MappingColumn.TableName = "points_xyz";
            propertyMapping.MappingColumn.ColumnName = "Z_COORD";

            hydroNetwork = (HydroNetwork)crossSectionImporter.ImportItem(null);

            Assert.AreEqual(2, hydroNetwork.CrossSections.Count());

            var xyz = new List<Coordinate>
                {
                    new Coordinate(127033.45, 465427.83, -0.27),
                    new Coordinate(127033.56, 465427.37, -0.28),
                    new Coordinate(127033.71, 465426.38, -0.28),
                    new Coordinate(127033.8, 465425.39, -0.27),
                    new Coordinate(127034.02, 465424.42, -0.3),
                    new Coordinate(127034.19, 465423.44, -0.28),
                    new Coordinate(127034.34, 465422.46, -0.22),
                    new Coordinate(127034.47, 465421.76, -0.32),
                    new Coordinate(127034.54, 465421.27, -0.52),
                    new Coordinate(127034.7, 465420.8, -0.86),
                    new Coordinate(127034.76, 465420.27, -1.06),
                    new Coordinate(127034.82, 465419.78, -1.06),
                    new Coordinate(127034.88, 465419.28, -1.11),
                    new Coordinate(127034.96, 465418.59, -1.11),
                    new Coordinate(127035.02, 465418.09, -1.06),
                    new Coordinate(127035.08, 465417.58, -0.82),
                    new Coordinate(127035.46, 465417.24, -0.22),
                    new Coordinate(127035.28, 465416.54, -0.01),
                    new Coordinate(127035.4, 465415.78, 0.2),
                    new Coordinate(127035.49, 465414.79, 0.35),
                    new Coordinate(127035.61, 465413.85, 0.46),
                    new Coordinate(127035.81, 465412.8, 0.57),
                    new Coordinate(127036.03, 465412.01, 0.63),
                    new Coordinate(127036.27, 465410.42, 0.62),
                    new Coordinate(127036.65, 465408.69, 0.52)
                };

            Assert.AreEqual(xyz.Count, hydroNetwork.CrossSections.First(cs => cs.Name == "Profiel_3").Geometry.Coordinates.Length);

            //compare z-values
            var lstZ = hydroNetwork.CrossSections.First(cs => cs.Name == "Profiel_3").Geometry.Coordinates.Select(c => c.Z).ToList();
            Assert.AreEqual(xyz.Select(c => c.Z).ToList(), lstZ);

            //compare geometry
            //all x-values are highter, shape.x -> x-values and shape.y -> y-values not the same?? not updated????
            //Assert.AreEqual(new LineString(xyz.ToArray()), hydroNetwork.CrossSections.Where(cs => cs.Name == "Profiel_3").First().Geometry);
        }

        [Test]
        public void ImportCrossSectionsXYZFromShapeFile()
        {
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var csImporterSettings = crossSectionImporter.FeatureFromGisImporterSettings;

            //First Channels
            channelImporterSettings.Path = TestHelper.GetTestFilePath("csXYZ_network_Branches.shp");
            channelImporterSettings.TableName = "Channel";
            channelImporterSettings.GeometryColumn = new MappingColumn("Channel", "Shape");

            var propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "Name";

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "LongName";


            var hydroNetwork = (HydroNetwork)channelImporter.ImportItem(null);

            Assert.Greater(hydroNetwork.Channels.Count(), 0);

            csImporterSettings.Path = TestHelper.GetTestFilePath("csXYZ_network_Cross Sections.shp");
            csImporterSettings.TableName = "Cross_section";
            crossSectionImporter.HydroRegion = hydroNetwork;

            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "Name";

            propertyMapping = csImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "LongName";

            hydroNetwork = (HydroNetwork)crossSectionImporter.ImportItem(null);

            Assert.AreEqual(3, hydroNetwork.CrossSections.Count());
        }
    }
}
