using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Import;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Data.Providers;
using SharpMap.Extensions.Data.Providers;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class NetworkCoverageFromGisImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportPumpCapacityToNetworkCoverageFromShapeFile()
        {
            // shapefile with 5 pumps on branch from 0,0 to 1000,1000
            var filePath = TestHelper.GetTestFilePath(@"shapefiles_CoverageImport\Pumps.shp");

            // build target network, shifted with dx = -15 from original
            var network = new HydroNetwork();
            var node1 = new HydroNode("node1") { Geometry = new Point(-15, 0) };
            var node2 = new HydroNode("node2") { Geometry = new Point(985, 1000) };
            var branch1 = new Channel(node1, node2)
            {
                Name = "channel1",
                Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
            };
            network.Branches.Add(branch1);

            var networkCoverage = new NetworkCoverage {Name = "coverage", Network = network};
            
            // importers
            var importer = new NetworkCoverageFromGisImporter();

            var coordinatesValueImporter = new PointValuePairsFromGisImporter
                {
                    FileBasedFeatureProviders = new EventedList<IFileBasedFeatureProvider> {new ShapeFile()},
                    SnappingPrecision = 10
                };

            importer.PointValuePairsFromGisImporter = coordinatesValueImporter;
            var settings = coordinatesValueImporter.FeatureFromGisImporterSettings;
            settings.PropertiesMapping.First(m => m.PropertyName == "Value").MappingColumn.ColumnName = "Capacity";
            settings.Path = filePath;
            Assert.IsTrue(coordinatesValueImporter.ValidateNetworkFeatureFromGisImporterSettings(settings));
            
            var resultingCoverage = (INetworkCoverage)importer.ImportItem(filePath, networkCoverage);
            Assert.IsTrue(resultingCoverage != null);
            Assert.AreEqual(0, resultingCoverage.Locations.Values.Count, "number of location in network coverage with small tolerance");

            resultingCoverage.Clear();

            coordinatesValueImporter.SnappingPrecision = 20;

            resultingCoverage = (INetworkCoverage)importer.ImportItem(filePath, networkCoverage);
            Assert.IsTrue(resultingCoverage != null);
            Assert.AreEqual(5, resultingCoverage.Locations.Values.Count, "number of location in network coverage with larger tolerance");
            Assert.AreEqual(new object[]{1.0,2.0,3.0,4.0,5.0}, resultingCoverage.GetValues(), "values on network locations");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportBranchesFromGeodatabaseAndSnapWeirCrestLevelToCoverage()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");

            var channelFromGisImporter = new ChannelFromGisImporter
                {
                    FileBasedFeatureProviders = new EventedList<IFileBasedFeatureProvider> {new OgrFeatureProvider()},
                    HydroRegion = new HydroNetwork()
                };
            
            var importerSettings = channelFromGisImporter.FeatureFromGisImporterSettings;
            importerSettings.Path = path;
            importerSettings.TableName = "Channel";
            importerSettings.GeometryColumn = new MappingColumn("Channel", "Shape");

            var propertyMapping = importerSettings.PropertiesMapping.First(p => p.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OBJECTID";
            propertyMapping = importerSettings.PropertiesMapping.First(p => p.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OVK_NAME";

            var hydroNetwork = (IHydroNetwork)channelFromGisImporter.ImportItem(null);
            Assert.AreEqual(181, hydroNetwork.Channels.Count(), "nr. of imported branches");

            var networkCoverage = new NetworkCoverage { Name = "coverage", Network = hydroNetwork };

            var pointValueImporter = new PointValuePairsFromGisImporter
                {
                    FileBasedFeatureProviders = new List<IFileBasedFeatureProvider> {new OgrFeatureProvider()},
                    SnappingPrecision = 20
                };
            var networkCoverageImporter = new NetworkCoverageFromGisImporter
                {
                    PointValuePairsFromGisImporter = pointValueImporter,
                };

            importerSettings = pointValueImporter.FeatureFromGisImporterSettings;
            importerSettings.Path = path;
            importerSettings.TableName = "Weir";
            importerSettings.GeometryColumn = new MappingColumn("Weir","Shape");
            propertyMapping = importerSettings.PropertiesMapping.First(p => p.PropertyName == "Value");
            propertyMapping.MappingColumn.TableName = "Weir";
            propertyMapping.MappingColumn.ColumnName = "CREST_LVL";
            
            var coverage = (INetworkCoverage)networkCoverageImporter.ImportItem("", networkCoverage);
            Assert.IsNotNull(coverage);
            Assert.AreEqual(40, coverage.Locations.Values.Count, "nr. of locations snapped to coverage");
        }
    }
}
