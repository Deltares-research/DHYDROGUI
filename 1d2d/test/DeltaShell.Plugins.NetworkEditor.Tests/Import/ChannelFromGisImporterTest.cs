using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api;
using SharpMap.Data.Providers;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class ChannelFromGisImporterTest
    {
        private HydroRegionFromGisImporter importer;
        private const string nameAlias = "Name";

        [SetUp]
        public void SetUp()
        {
            importer = new HydroRegionFromGisImporter();
            importer.FileBasedFeatureProviders.Add(new ShapeFile());
            importer.FileBasedFeatureProviders.Add(new OgrFeatureProvider());
        }

        [TearDown]
        public void TearDown()
        {
            importer = null;
        }

        [Test]
        public void ReadChannelUsingChannelFromGisImporter_WhenImportedInHydroNetworkWithCoordinateSystemIs3857_ThenChannelGeodeticLengthIsSet()
        {
            //Arrange
            //Network with mercator cs
            IHydroNetwork hydroNetwork = SetupHydroNetworkWithMercatorCoordinateSystem();

            //Channel feature to be read and placed in network    
            
            IFeature channel = GenerateChannelAsAFeature();

            // Simulate the feature provider
            IFileBasedFeatureProvider fileBasedFeatureProvider = SetUpFileBasedFeatureProvider(channel);

            // Setup the importer
            ChannelFromGisImporter myImporter = SetUpChannelFromGisImporter(fileBasedFeatureProvider, hydroNetwork);

            //Act
            var readNetwork = myImporter.ImportItem("") as IHydroNetwork;

            //Asserts
            var importedChannel = readNetwork?.Channels?.SingleOrDefault();
            Assert.That(importedChannel, Is.Not.Null);
            Assert.That(importedChannel.GeodeticLength, Is.Not.NaN);
        }
        
        [Test]
        public void ReadChannelUsingChannelFromGisImporter_WhenImportedInHydroNetworkWithoutCoordinateSystem_ThenChannelGeodeticLengthIsNan()
        {
            //Arrange
            //Network without cs
            IHydroNetwork hydroNetwork = new HydroNetwork();

            //Channel feature to be read and placed in network
            IFeature channel = GenerateChannelAsAFeature();
            
            // Simulate the feature provider
            IFileBasedFeatureProvider fileBasedFeatureProvider = SetUpFileBasedFeatureProvider(channel);

            // Setup the importer
            ChannelFromGisImporter myImporter = SetUpChannelFromGisImporter(fileBasedFeatureProvider, hydroNetwork);

            //Act
            myImporter.ImportItem("");

            //Asserts
            var importedChannel = hydroNetwork.Channels?.SingleOrDefault();
            Assert.That(importedChannel, Is.Not.Null);
            Assert.That(importedChannel.GeodeticLength, Is.NaN);
        }

        private static ChannelFromGisImporter SetUpChannelFromGisImporter(IFileBasedFeatureProvider fileBasedFeatureProvider, IHydroNetwork hydroNetwork)
        {
            ChannelFromGisImporter myImporter = new ChannelFromGisImporter
            {
                FeatureFromGisImporterSettings = { Path = "myShapeFile.shp" },
                FileBasedFeatureProviders = new List<IFileBasedFeatureProvider> { fileBasedFeatureProvider },
                HydroRegion = hydroNetwork
            };
            var propertyMappingName = myImporter.FeatureFromGisImporterSettings.PropertiesMapping.SingleOrDefault(pm => pm.PropertyName == nameAlias);
            Assert.NotNull(propertyMappingName);
            propertyMappingName.MappingColumn = new MappingColumn("", nameAlias);
            return myImporter;
        }

        private static IFileBasedFeatureProvider SetUpFileBasedFeatureProvider(IFeature channel)
        {
            var listOfFeatures = new List<IFeature> { channel };
            var fileBasedFeatureProvider = Substitute.For<IFileBasedFeatureProvider>();
            fileBasedFeatureProvider.FileFilter.Returns("*.shp");
            fileBasedFeatureProvider.Features.Returns(listOfFeatures);
            return fileBasedFeatureProvider;
        }

        private static IFeature GenerateChannelAsAFeature()
        {
            var channel = Substitute.For<IFeature>();
            var channelGeometry = Substitute.For<ILineString>();
            channelGeometry.Coordinates.Returns(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 100)
            });
            channel.Geometry = channelGeometry;
            var channelAttributes = Substitute.For<IFeatureAttributeCollection>();
            channelAttributes[nameAlias] = "myChannel";
            channel.Attributes = channelAttributes;
            return channel;
        }

        private static IHydroNetwork SetupHydroNetworkWithMercatorCoordinateSystem()
        {
            IHydroNetwork hydroNetwork = new HydroNetwork();
            if (Map.CoordinateSystemFactory == null)
            {
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            }
            var mercator3857CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(3857);

            hydroNetwork.CoordinateSystem = mercator3857CoordinateSystem;
            return hydroNetwork;
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadBranchFromShapeFile_WhenImportedInHydroNetworkWithCoordinateSystemIs3857_ThenChannelGeodeticLengthIsSet()
        {
            var path = TestHelper.GetTestFilePath("model_Branches.shp");
            IHydroNetwork hydroNetwork = new HydroNetwork();
            if (Map.CoordinateSystemFactory == null)
            {
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            }
            var mercator3857CoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(3857);
            hydroNetwork.CoordinateSystem = mercator3857CoordinateSystem;


            FeatureFromGisImporterBase featureImporter = FeatureFromGisImporterBase.CreateNetworkFeatureFromGisImporter(typeof(ChannelFromGisImporter));
            featureImporter.FeatureFromGisImporterSettings.Path = path;
            var hydroRegionFromGisImporter = new HydroRegionFromGisImporter();
            featureImporter.FileBasedFeatureProviders = hydroRegionFromGisImporter.FileBasedFeatureProviders;
            var propertyMappingName = featureImporter.FeatureFromGisImporterSettings.PropertiesMapping.SingleOrDefault(pm => pm.PropertyName == "Name");
            Assert.NotNull(propertyMappingName);
            propertyMappingName.MappingColumn = new MappingColumn("", "Name");

            importer.FeatureFromGisImporters.Add(featureImporter);

            var hydroRegion = importer.ImportItem(path, hydroNetwork) as IHydroNetwork;
            Assert.NotNull(hydroRegion);
            var importedChannel = hydroRegion.Channels?.SingleOrDefault();
            Assert.That(importedChannel, Is.Not.Null);
            Assert.That(importedChannel.GeodeticLength, Is.Not.NaN);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadBranchFromShapeFile_WhenImportedInHydroNetworkWithoutCoordinateSystem_ThenChannelGeodeticLengthIsNan()
        {
            var path = TestHelper.GetTestFilePath("model_Branches.shp");

            FeatureFromGisImporterBase featureImporter = FeatureFromGisImporterBase.CreateNetworkFeatureFromGisImporter(typeof(ChannelFromGisImporter));
            featureImporter.FeatureFromGisImporterSettings.Path = path;
            var hydroRegionFromGisImporter = new HydroRegionFromGisImporter();
            featureImporter.FileBasedFeatureProviders = hydroRegionFromGisImporter.FileBasedFeatureProviders;
            var propertyMappingName = featureImporter.FeatureFromGisImporterSettings.PropertiesMapping.SingleOrDefault(pm => pm.PropertyName == "Name");
            Assert.NotNull(propertyMappingName);
            propertyMappingName.MappingColumn = new MappingColumn("", "Name");

            importer.FeatureFromGisImporters.Add(featureImporter);

            var hydroRegion = importer.ImportItem(path) as IHydroRegion;
            Assert.NotNull(hydroRegion);
            var importedChannel = hydroRegion.SubRegions.OfType<HydroNetwork>().SingleOrDefault()?.Channels?.SingleOrDefault();
            Assert.That(importedChannel, Is.Not.Null);
            Assert.That(importedChannel.GeodeticLength, Is.NaN);
        }

    }
}