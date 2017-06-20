using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class UGridToNetworkAdapterTest
    {
        private const string UGRID_TEST_FOLDER = @"ugrid\";
        private const string UGRID_TEST_FILE = @"ugrid\Empty_UGrid.nc";

        private IHydroNetwork CreateSimpleNetwork()
        {
            var network = new HydroNetwork() { Name = "my Network" };
            var hydroNode1 = new HydroNode() { Name = "my Node1", Geometry = new Point(1, 4), Network = network };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode() { Name = "myNode2", Geometry = new Point(5, 1), Network = network };
            network.Nodes.Add(hydroNode2);
            var branch1 = new Branch()
            {
                Name = "my Branch 1",
                Network = network,
                Source = hydroNode1,
                Target = hydroNode2,
                Geometry = new LineString(new[]
                {
                    new Coordinate(1, 4),
                    new Coordinate(6, 12),
                    new Coordinate(5, 1)
                })
            };
            network.Branches.Add(branch1);

            var networkDiscretisation = new Discretization
            {
                Name = "my Discretisation",
                Network = network
            };

            // add source node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 0));
            // add calculation points
            var location1 = new NetworkLocation(branch1, 1);
            networkDiscretisation.Locations.Values.Add(location1);
            var location2 = new NetworkLocation(branch1, 2.5);
            networkDiscretisation.Locations.Values.Add(location2);
            var location3 = new NetworkLocation(branch1, 4);
            networkDiscretisation.Locations.Values.Add(location3);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 5));

            return network;
        }

        private IDiscretization CreateNetworkDiscretisation()
        {
            var network = new HydroNetwork() { Name = "my Network" };
            var hydroNode1 = new HydroNode() { Name = "my Node 1", Description = "node 1 description", Geometry = new Point(-187.96667, 720.81667), Network = network };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode() { Name = "my Node 2", Description = "node 2 description", Geometry = new Point(2195.7333, 708.71667), Network = network };
            network.Nodes.Add(hydroNode2);
            var hydroNode3 = new HydroNode() { Name = "my Node 3", Description = "node 3 description", Geometry = new Point(4071.4928, 690.94861), Network = network };
            network.Nodes.Add(hydroNode3);
            var hydroNode4 = new HydroNode() { Name = "my Node  4", Description = "node 4 description", Geometry = new Point(3445.4246, 1540.1838), Network = network };
            network.Nodes.Add(hydroNode4);

            var branch1 = new Branch()
            {
                Name = "my Branch 1",
                Description = "branch 1 description",
                Network = network,
                Source = hydroNode1,
                Target = hydroNode2,
                Geometry = new LineString(new[]
                {
                    new Coordinate(-187.96667, 720.81667),
                    new Coordinate(187.13333, 1039.45),
                    new Coordinate(828.43333, 861.98333),
                    new Coordinate(1219.6667, 406.21667),
                    new Coordinate(1712.2164, 273.32123),
                    new Coordinate(2094.9, 547.38333),
                    new Coordinate(2195.7333, 708.71667)
                })
            };
            network.Branches.Add(branch1);

            var branch2 = new Branch()
            {
                Name = "my Branch 2",
                Description = "branch 2 description",
                Network = network,
                Source = hydroNode2,
                Target = hydroNode3,
                Geometry = new LineString(new[]
                {
                    new Coordinate(2195.7333, 708.71667),
                    new Coordinate(2577.8276, 567.00618),
                    new Coordinate(3235.6759, 576.54021),
                    new Coordinate(4071.4928, 690.94861)
                })
            };
            network.Branches.Add(branch2);

            var branch3 = new Branch()
            {
                Name = "my Branch 3",
                Description = "branch 3 description",
                Network = network,
                Source = hydroNode2,
                Target = hydroNode4,
                Geometry = new LineString(new[]
                {
                    new Coordinate(2195.7333, 708.71667),
                    new Coordinate(2739.9061, 938.83347),
                    new Coordinate(3226.4949, 1367.1587),
                    new Coordinate(3445.4246, 1540.1838)
                })
            };
            network.Branches.Add(branch3);

            var networkDiscretisation = new Discretization
            {
                Name = "my Discretisation",
                Network = network
            };

            // Branch 1
            // add source node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 0));
            // add calculation points
            var location1 = new NetworkLocation(branch1, 500);
            networkDiscretisation.Locations.Values.Add(location1);
            var location2 = new NetworkLocation(branch1, 1000);
            networkDiscretisation.Locations.Values.Add(location2);
            var location3 = new NetworkLocation(branch1, 1500);
            networkDiscretisation.Locations.Values.Add(location3);
            var location4 = new NetworkLocation(branch1, 2000);
            networkDiscretisation.Locations.Values.Add(location4);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 2500));

            // Branch3
            // add calculation points
            var location5 = new NetworkLocation(branch3, 700);
            networkDiscretisation.Locations.Values.Add(location5);
            var location6 = new NetworkLocation(branch3, 1400);
            networkDiscretisation.Locations.Values.Add(location6);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch3, 2100));

            // Branch 2
            // add calculation points
            var location7 = new NetworkLocation(branch2, 400);
            networkDiscretisation.Locations.Values.Add(location7);
            var location8 = new NetworkLocation(branch2, 800);
            networkDiscretisation.Locations.Values.Add(location8);
            var location9 = new NetworkLocation(branch2, 1200);
            networkDiscretisation.Locations.Values.Add(location9);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch2, 1600));

            return networkDiscretisation;
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveAndLoadSimpleNetworkTest()
        {
            var testFilePath =
            TestHelper.GetTestFilePath(UGRID_TEST_FOLDER + "simple_network_testFile.nc");
            var testFolderPath = Path.GetDirectoryName(testFilePath);
            FileUtils.CreateDirectoryIfNotExists(testFolderPath);
            FileUtils.DeleteIfExists(testFilePath);
            try
            {
                var storedNetwork = CreateSimpleNetwork();

                UGridGlobalMetaData metaData = new UGridGlobalMetaData(storedNetwork.Name, "PluginName", "PluginVersion");
                
                UGridToNetworkAdapter.SaveNetwork(storedNetwork, testFilePath, metaData);
                //UGridToNetworkAdapter.SaveNetworkDiscretisation(, testFilePath, metaData);

                var loadedNetwork = UGridToNetworkAdapter.LoadNetwork(testFilePath);

                Assert.AreEqual(loadedNetwork.Name, "DummyNetworkName"); // TODO: Implement the read/get network name functionality

                CompareAndAssertNetworks(storedNetwork, loadedNetwork);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFilePath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveAndLoadNetworkTest()
        {

            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FOLDER + "save_load_network_testFile.nc");
            var testFolderPath = Path.GetDirectoryName(testFilePath);
            FileUtils.CreateDirectoryIfNotExists(testFolderPath);
            FileUtils.DeleteIfExists(testFilePath);

            try
            {
                var networkDiscretization = CreateNetworkDiscretisation();
                var storedNetwork = networkDiscretization.Network;
                
                UGridGlobalMetaData metaData = new UGridGlobalMetaData(storedNetwork.Name, "PluginName", "PluginVersion");

                UGridToNetworkAdapter.SaveNetwork((HydroNetwork)storedNetwork, testFilePath, metaData);
                //UGridToNetworkAdapter.SaveNetworkDiscretisation(networkDiscretization, localCopyOfTestFile);

                var loadedNetwork = UGridToNetworkAdapter.LoadNetwork(testFilePath);

                Assert.AreEqual(loadedNetwork.Name, "DummyNetworkName"); // TODO: Implement the read/get network name functionality

                CompareAndAssertNetworks((HydroNetwork)storedNetwork, loadedNetwork);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFilePath);
            }
        }

        private static void CompareAndAssertNetworks(IHydroNetwork storedNetwork, IHydroNetwork loadedNetwork)
        {
            var storedNodes = storedNetwork.Nodes;
            var loadedNodes = loadedNetwork.Nodes;
            var storedBranches = storedNetwork.Branches;
            var loadedBranches = loadedNetwork.Branches;

            Assert.AreEqual(storedNodes.Count, loadedNodes.Count);
            Assert.AreEqual(storedBranches.Count, loadedBranches.Count);

            // loop over the nodes and assert each item
            for (int i = 0; i < storedNodes.Count; ++i)
            {
                // test node names
                string storedNodeName = storedNodes[i].Name.Trim().Replace(" ", "_");
                string loadedNodeName = loadedNodes[i].Name.Trim();
                Assert.AreEqual(storedNodeName, loadedNodeName);

                // test x coordinate
                double storedNodeCoordinateX = storedNodes[i].Geometry.Coordinates[0].X;
                double loadedNodeCoordinateX = loadedNodes[i].Geometry.Coordinates[0].X;
                Assert.AreEqual(storedNodeCoordinateX, loadedNodeCoordinateX);

                // test y coordinate
                double storedNodeCoordinateY = storedNodes[i].Geometry.Coordinates[0].Y;
                double loadedNodeCoordinateY = loadedNodes[i].Geometry.Coordinates[0].Y;
                Assert.AreEqual(storedNodeCoordinateY, loadedNodeCoordinateY);

                // test node description
                string storedNodeDescription = storedNodes[i].Description != null
                    ? storedNodes[i].Description.Trim().Replace(" ", "_")
                    : "";
                string loadedNodeDescription = loadedNodes[i].Description.Trim();
                Assert.AreEqual(storedNodeDescription, loadedNodeDescription);
            }

            // loop over the branches and assert each item
            for (int i = 0; i < storedBranches.Count; ++i)
            {
                var storedBranch = storedBranches[i];
                var loadedBranch = loadedBranches[i];
                // test source nodes
                INode storedBranchSourceNode = storedBranch.Source;
                storedBranchSourceNode.Name = storedBranchSourceNode.Name.Replace(" ", "_");
                INode loadedBranchSourceNode = loadedBranch.Source;
                Assert.AreEqual(storedBranchSourceNode, loadedBranchSourceNode);

                // test target nodes
                INode storedBranchTargetNode = storedBranch.Target;
                storedBranchTargetNode.Name = storedBranchTargetNode.Name.Replace(" ", "_");
                INode loadedBranchTargetNode = loadedBranch.Target;
                Assert.AreEqual(storedBranchTargetNode, loadedBranchTargetNode);

                // test branch lengths
                var storedBranchLength = storedBranch.Length;
                var loadedBranchLength = loadedBranch.Length;
                Assert.AreEqual(storedBranchLength, loadedBranchLength);

                // test number of geometry points per branch
                var storedBranchGeometryPointsCount = storedBranch.Geometry.Coordinates.Length;
                var loadedBranchGeometryPointsCount = loadedBranch.Geometry.Coordinates.Length;
                Assert.AreEqual(storedBranchGeometryPointsCount, loadedBranchGeometryPointsCount);

                // test branch names
                var storedBranchName = storedBranch.Name.Trim().Replace(" ", "_");
                var loadedBranchName = loadedBranch.Name.Trim();
                Assert.AreEqual(storedBranchName, loadedBranchName);

                // test branch descriptions
                var storedBranchDescription = storedBranch.Description != null
                    ? storedBranch.Description.Trim().Replace(" ", "_")
                    : "";
                var loadedBranchDescription = loadedBranch.Description.Trim();
                Assert.AreEqual(storedBranchDescription, loadedBranchDescription);
            }
            
            var storedGeometryPoints = storedNetwork.Branches.SelectMany(b => b.Geometry.Coordinates).ToList();
            var loadedGeometryPoints = loadedNetwork.Branches.SelectMany(b => b.Geometry.Coordinates).ToList();

            Assert.AreEqual(storedGeometryPoints.Count, loadedGeometryPoints.Count);
            // loop over geometrypoints and assert each item
            for (int i = 0; i < storedGeometryPoints.Count; ++i)
            {
                Assert.AreEqual(storedGeometryPoints[i].X, loadedGeometryPoints[i].X);
                Assert.AreEqual(storedGeometryPoints[i].Y, loadedGeometryPoints[i].Y);
            }
        }
    }
}
