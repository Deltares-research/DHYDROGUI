using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Api;
using SharpMap.Converters.WellKnownText;
using SharpMap.Data.Providers;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    [Category(TestCategory.X86)]
    public class ChannelFromGisImporterTest
    {
        private readonly MockRepository mocks = new MockRepository();
        private ChannelFromGisImporter importer;

        [SetUp]
        public void SetUp()
        {
            importer = new ChannelFromGisImporter
                {
                    FileBasedFeatureProviders = new List<IFileBasedFeatureProvider> {new ShapeFile(), new OgrFeatureProvider()},
                    HydroRegion = new HydroNetwork()
                };
        }

        [Test]
        public void GeometryIsAdjustedWhenItDoesNotFitNodeLocations()
        {
            var featureProvider = mocks.StrictMock<IFileBasedFeatureProvider>();
            importer.FileBasedFeatureProviders.Add(featureProvider);
            
            var settings = importer.FeatureFromGisImporterSettings;
            settings.Path = "*.test";

            var propertyMapping = settings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "name";

            var branch1 = mocks.StrictMock<IFeature>();
            var branch2 = mocks.StrictMock<IFeature>();

            var attributes = new DictionaryFeatureAttributeCollection();
            attributes["name"] = "branch";

            branch1.Expect(b => b.Attributes).Return(attributes);
            branch1.Expect(b => b.Geometry).Return(
                new LineString(new [] {new Coordinate(0, 0), new Coordinate(0, 100)}));
            branch2.Expect(b => b.Attributes).Return(attributes);
            branch2.Expect(b => b.Geometry).Return(
                new LineString(new [] {new Coordinate(0, 0), new Coordinate(100, 100)}));

            var features = new[] {branch1, branch2};

            featureProvider.Expect(fp => fp.FileFilter).Repeat.Any().Return("*.test");
            featureProvider.Expect(fp => fp.Open(null)).IgnoreArguments();
            featureProvider.Expect(fp => fp.Close());
            featureProvider.Expect(fp => fp.Features).Return(features);

            mocks.ReplayAll();

            var hydronetWork = (IHydroNetwork)importer.ImportItem(null);

            var realBranch = hydronetWork.Branches.First();
            Assert.AreEqual(new [] {new Coordinate(0, 0), new Coordinate(100, 100), new Coordinate(0, 100)},
                            realBranch.Geometry.Coordinates);
        }

        [Test]
        public void ImportChannelsFromShape()
        {
            var path = TestHelper.GetTestFilePath("1-watergangen-WGS.shp");
            var importerSettings = importer.FeatureFromGisImporterSettings;

            importerSettings.Path = path;

            var propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "OVK_ID";

            propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "OVKIDENT";
            
            var hydronetWork = (IHydroNetwork)importer.ImportItem(null);
            
            Assert.AreEqual(611, hydronetWork.Channels.Count());
            Assert.AreEqual(603,hydronetWork.Nodes.Count);
            Assert.AreEqual("117742", hydronetWork.Channels.First().Name);
            Assert.AreEqual("VO0230-VO0240", hydronetWork.Channels.First().LongName);
        }

        [Test]
        public void ImportChannelsFromGeodatabase()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
            var importerSettings = importer.FeatureFromGisImporterSettings;

            importerSettings.Path = path;
            importerSettings.TableName = "Channel";
            importerSettings.GeometryColumn = new MappingColumn("Channel", "Shape");

            var propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OBJECTID";

            propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OVK_NAME";


            var hydronetWork = (IHydroNetwork)importer.ImportItem(null);

            Assert.AreEqual(181, hydronetWork.Channels.Count());
            Assert.AreEqual("VO0230-VO0240", hydronetWork.Channels.First().LongName);
        }

        [Test]
        public void ImportChannelsFromShapeWithNodeNames()
        {
            var path = TestHelper.GetTestFilePath("Reach.shp");
            var importerSettings = importer.FeatureFromGisImporterSettings;

            importerSettings.Path = path;
            
            var propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "ID";

            propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "NAME";

            propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "Source node");
            propertyMapping.MappingColumn.ColumnName = "ID_FROM";

            propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "Target node");
            propertyMapping.MappingColumn.ColumnName = "ID_TO";

            var hydronetWork = (IHydroNetwork)importer.ImportItem(null);

            Assert.AreEqual(77, hydronetWork.Channels.Count());
            Assert.IsNotNull(hydronetWork.Nodes.FirstOrDefault(n => n.Name == "dki310_23"));
            Assert.IsNotNull(hydronetWork.Nodes.FirstOrDefault(n => n.Name == "27"));
            Assert.IsNotNull(hydronetWork.Nodes.FirstOrDefault(n => n.Name == "MGSpill_6"));
            Assert.IsNotNull(hydronetWork.Nodes.FirstOrDefault(n => n.Name == "MGSpill_7"));
        }

        [Test]
        public void ImportChannelsFromShapeWithCustomLength()
        {
            var path = TestHelper.GetTestFilePath("shapefiles_customlength\\ReachesCustomLengthImportShape.shp");
            var importerSettings = importer.FeatureFromGisImporterSettings;

            importerSettings.Path = path;

            var propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "ID_SOBEK";

            propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "ID_SOBEK";

            propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "Is custom length");
            propertyMapping.MappingColumn.ColumnName = "CUSTOML_TF";

            propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "Custom length");
            propertyMapping.MappingColumn.ColumnName = "CUSTOMLENG";

            var hydronetWork = (IHydroNetwork)importer.ImportItem(null);

            Assert.AreEqual(52, hydronetWork.Channels.Count());
            Assert.AreEqual(2, hydronetWork.Channels.Count(c => c.IsLengthCustom));
            var customLengthChannel = hydronetWork.Channels.First(c => c.Name == "WTG_86072"); 
            Assert.AreEqual(1234.0, customLengthChannel.Length, 0.01);
        }

        [Test]
        public void ImportChannelsFromBaseLineCovergaConvertedToShapeFile()
        {
            var path = TestHelper.GetTestFilePath("rivieras_arc.shp");
            var importerSettings = importer.FeatureFromGisImporterSettings;

            importerSettings.Path = path;

            var propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "RIVIERAS_";

            propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "RIVIERAS_";

            var hydronetWork = (IHydroNetwork)importer.ImportItem(null);

            Assert.AreEqual(35, hydronetWork.Channels.Count());
            Assert.AreEqual(36, hydronetWork.Nodes.Count);
        }

        [Test]
        public void ImportChannelsFromShapeFileRelocateExistingCrossSections()
        {
            var network = new HydroNetwork();
            var node1 = new HydroNode("Node1") {Geometry = new Point(0, 0)};
            var node2 = new HydroNode("Node2") { Geometry = new Point(0, 100) };
            var branch = new Channel(node1,node2)
                             {
                                 Name = "25", 
                                 IsLengthCustom = true,
                                 Geometry = new LineString(new []
                                                               {
                                                                   new Coordinate(0,0), 
                                                                   new Coordinate(0,100) 
                                                               })
                             };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(branch);
            var crossSection = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch,
                                                                 CrossSectionDefinitionZW.CreateDefault(), 70);

            var coordinatesbefore = crossSection.Geometry.Coordinate;
            importer.FeatureFromGisImporterSettings.Path = TestHelper.GetTestFilePath("rivieras_arc.shp");

            var propertyMapping = importer.FeatureFromGisImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "RIVIERAS_";

            importer.HydroRegion = network;
            importer.ImportItem(null);

            var coordinatesafter = crossSection.Geometry.Coordinate;

            Assert.AreEqual(35, network.Channels.Count());
            Assert.AreNotEqual(coordinatesbefore, coordinatesafter);
            Assert.IsTrue(branch.Geometry.Intersects(crossSection.Geometry));
        }

        [Test]
        public void BranchFeatureForCustomLength()
        {
            var channel = new Channel
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 0 100)"),
                IsLengthCustom = true,
                Length = 200
            };
            var lateral = new LateralSource
            {
                Geometry = GeometryFromWKT.Parse("Point (0 50)"),
                Chainage = 50.0
            };
            NetworkHelper.AddBranchFeatureToBranch(lateral, channel, lateral.Chainage);
            ChannelFromGisImporter.UpdateGeometry(channel,
                                                        (ILineString)GeometryFromWKT.Parse("LINESTRING (0 0, 0 150)"));
            Assert.AreEqual(50.0, lateral.Chainage, 1.0e-6);
            Assert.AreEqual(37.5, lateral.Geometry.Coordinates[0].Y, 1.0e-6);
        }

        [Test]
        public void BranchFeatureForGeometryLength()
        {
            var channel = new Channel
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 0 100)"),
            };
            var lateral = new LateralSource
            {
                Geometry = GeometryFromWKT.Parse("Point (0 50)"),
                Chainage = 50.0
            };
            NetworkHelper.AddBranchFeatureToBranch(lateral, channel, lateral.Chainage);
            ChannelFromGisImporter.UpdateGeometry(channel,
                                                        (ILineString)GeometryFromWKT.Parse("LINESTRING (0 0, 0 150)"));
            Assert.AreEqual(75.0, lateral.Chainage, 1.0e-6);
            Assert.AreEqual(75.0, lateral.Geometry.Coordinates[0].Y, 1.0e-6);
        }

        [Test]
        public void CrossSectionForCustomLength()
        {
            var channel = new Channel
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 0 100)"),
                IsLengthCustom = true,
                Length = 200
            };
            var crossSection = new CrossSectionDefinitionYZ();
            var cs1 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, crossSection, 50.0);

            crossSection.SetDefaultYZTableAndUpdateThalWeg(10.0);

            ChannelFromGisImporter.UpdateGeometry(channel,
                                                        (ILineString)GeometryFromWKT.Parse("LINESTRING (0 0, 0 150)"));
            Assert.AreEqual(50.0, cs1.Chainage, 1.0e-6);
            Assert.AreEqual(37.5, cs1.Geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(37.5, cs1.Geometry.Coordinates[1].Y, 1.0e-6);
        }

        [Test]
        public void CrossSectionForGeometryLength()
        {
            var channel = new Channel
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 0 100)"),
            };
            var crossSection = new CrossSectionDefinitionYZ();
            var cs1 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, crossSection, 50.0);
            crossSection.SetDefaultYZTableAndUpdateThalWeg(10);

            ChannelFromGisImporter.UpdateGeometry(channel,
                                                        (ILineString)GeometryFromWKT.Parse("LINESTRING (0 0, 0 150)"));
            Assert.AreEqual(75.0, cs1.Chainage, 1.0e-6);
            Assert.AreEqual(75.0, cs1.Geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(75.0, cs1.Geometry.Coordinates[1].Y, 1.0e-6);
        }

        [Test]
        public void ImportWrongShapeGeometryShouldNotThrow_Jira9548()
        {
            var path = TestHelper.GetTestFilePath("Duikers.shp");
            var importerSettings = importer.FeatureFromGisImporterSettings;

            importerSettings.Path = path;

            var propertyMapping = importerSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "OVK_ID";

            var result = (IHydroNetwork)importer.ImportItem(null);
            Assert.IsNotNull(result);
        }
    }
}

