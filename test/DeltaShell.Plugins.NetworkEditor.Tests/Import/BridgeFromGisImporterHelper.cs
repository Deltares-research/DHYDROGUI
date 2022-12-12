using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NSubstitute;
using SharpMap.Api;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    public static class BridgeFromGisImporterHelper
    {
        /// <summary>
        /// TestName used for test class <see cref="TestBaseBridge"/>.
        /// </summary>
        public const string TestName = "TestName";
        
        /// <summary>
        /// Method to setup a hydro network with branches and a high snapping tolerance.
        /// </summary>
        /// <param name="importer">Importer under test.</param>
        /// <param name="hydroNetwork">network to be used by test.</param>
        public static void SetupAndLinkHydroNetworkWithBranchesAndHighSnappingTolerance(BridgeFromGisImporterBase importer, IHydroNetwork hydroNetwork)
        {
            hydroNetwork = new HydroNetwork();
            
            var node1 = new Node
            {
                Name = "node1",
                Geometry = new Point(0, 0)
            };
            var node2 = new Node
            {
                Name = "node2",
                Geometry = new Point(1000, 0)
            };
            var node3 = new Node
            {
                Name = "node3",
                Geometry = new Point(1000, 1000)
            };

            hydroNetwork.Nodes.AddRange(new[]
            {
                node1,
                node2,
                node3
            });

            var branch1 = new Branch("branch1", node1, node2, 1000);
            var branch2 = new Branch("branch2", node2, node3, 1000);
            Coordinate[] points = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(50, 50)
            };
            branch1.Geometry = new LineString(points);
            hydroNetwork.Branches.AddRange(new[]
            {
                branch1,
                branch2
            });
            importer.HydroRegion = hydroNetwork;
            importer.SnappingTolerance = 50000000;
        }

        /// <summary>
        /// Method to get fileBasedFeatureProvider for import item test.
        /// </summary>
        /// <param name="testFileLocation">Test file path.</param>
        /// <returns>Shapefile with the location set based on the given path.</returns>
        public static ShapeFile GetTestFileBasedFeatureProvider(string testFileLocation)
        {
            var fileBasedFeatureProvider = new ShapeFile();
            fileBasedFeatureProvider.Path = TestHelper.GetTestFilePath(testFileLocation);

            var shapefile = new ShapeFileFeature();
            var attributeCollection = new DictionaryFeatureAttributeCollection();

            shapefile.Attributes = attributeCollection;
            fileBasedFeatureProvider.Features.Add(shapefile);
            return fileBasedFeatureProvider;
        }

        /// <summary>
        /// Method to setup and link the test file path for validation test.
        /// </summary>
        /// <param name="settings">settings for FeatureFromGisImporterSettings.</param>
        /// <param name="importer">Importer under test.</param>
        /// <param name="filePath">Test file path.</param>
        public static void SetupAndLinkTestFilePath(FeatureFromGisImporterSettings settings, BridgeFromGisImporterBase importer, string filePath)
        {
            settings.Path = TestHelper.GetTestFilePath(filePath);
            var fileBasedFeatureProvider = Substitute.For<IFileBasedFeatureProvider>();
            fileBasedFeatureProvider.FileFilter.Returns(".shp");

            importer.FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>();
            importer.FileBasedFeatureProviders.Add(fileBasedFeatureProvider);
        }
        
        /// <summary>
        /// TestClass to test the base of the bridgeFromGisImporterBase.
        /// </summary>
        public class TestBaseBridge : BridgeFromGisImporterBase
        {
            public override string Name => TestName;
            protected override BridgeType BridgeType { get; }
        }
    }
}