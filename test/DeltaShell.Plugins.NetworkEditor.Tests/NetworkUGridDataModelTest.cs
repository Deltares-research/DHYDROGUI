using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Tests.Helpers;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class NetworkUGridDataModelTest
    {
        private IHydroNetwork TestNetwork()
        {
            var network = new HydroNetwork() { Name = "my Network" };
            var hydroNode1 = new HydroNode() { Name = "my Node1", Description = "Node 1 Description", Geometry = new Point(0, 0), Network = network };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode() { Name = "my Node2", Description = "Node 2 Description", Geometry = new Point(3, 4), Network = network };
            network.Nodes.Add(hydroNode2);
            var branch1 = new Branch()
            {
                Name = "my Branch 1",
                Description = "Branch 1 Description",
                Network = network,
                Source = hydroNode1,
                Target = hydroNode2,
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(3, 4)
                })
            };
            network.Branches.Add(branch1);

            return network;
        }
        
        [Test]
        public void ConstructNetworkDataModelTest()
        {
            var network = TestNetwork();

            var networkDataModel = new NetworkUGridDataModel(network);

            // Test the nodes
            Assert.AreEqual("my Network", networkDataModel.Name);
            Assert.AreEqual(2, networkDataModel.NumberOfNodes);
            Assert.AreEqual(new[] {0, 3}, networkDataModel.NodesX);
            Assert.AreEqual(new[] {0, 4}, networkDataModel.NodesY);
            Assert.AreEqual(new[] {"my Node1", "my Node2"}, networkDataModel.NodesNames);
            Assert.AreEqual(new[] {"Node 1 Description", "Node 2 Description"}, networkDataModel.NodesDescriptions);

            // Test the branches
            Assert.AreEqual(1, networkDataModel.NumberOfBranches);
            Assert.AreEqual(new[] {0}, networkDataModel.SourceNodeIds);
            Assert.AreEqual(new[] {1}, networkDataModel.TargedNodesIds);
            Assert.AreEqual(new[] {5}, networkDataModel.BranchLengths);

            Assert.AreEqual(2, networkDataModel.NumberOfGeometryPoints);
            Assert.AreEqual(new[] {2}, networkDataModel.NumberOfBranchGeometryPoints);

            Assert.AreEqual(new[] { "my Branch 1" }, networkDataModel.BranchNames);
            Assert.AreEqual(new[] { "Branch 1 Description" }, networkDataModel.BranchDescriptions);
            
            // Test the geometry points
            Assert.AreEqual(new[] {0, 3}, networkDataModel.GeopointsX);
            Assert.AreEqual(new[] {0, 4}, networkDataModel.GeopointsY);
        }

        [Test]
        public void ReconstructHydroNetworkTest()
        {
            var network = TestNetwork();

            var networkDataModel = new NetworkUGridDataModel(network);

            var reconstructedNetwork = NetworkUGridDataModel.ReconstructHydroNetwork(networkDataModel);
            HydroNetworkTestHelper.CompareAndAssertNetworks(network, reconstructedNetwork);
            Assert.AreEqual(network.Name, reconstructedNetwork.Name);
            Assert.AreEqual(network.CoordinateSystem, reconstructedNetwork.CoordinateSystem);

            // Test the nodes
            Assert.AreEqual(network.Nodes.Count, reconstructedNetwork.Nodes.Count);

            /* When new nodes are constructed, the following parameters are required:
             * ---------
             * Network
             * Name
             * Description
             * Geometry
             * ---------
             * Test these individual components */

            for (int i = 0; i < network.Nodes.Count; i++)
            {
                var node = network.Nodes[i];
                var reconstructedNode = reconstructedNetwork.Nodes[i];

                HydroNetworkTestHelper.CompareAndAssertNodes(node, reconstructedNode);

                Assert.AreEqual(node.Network, network);
                Assert.AreEqual(reconstructedNode.Network, reconstructedNetwork);
            }

            // Test the branches
            Assert.AreEqual(network.Branches.Count, reconstructedNetwork.Branches.Count);

            /* When new branches are constructed, the following parameters are required:
             * ---------
             * Network
             * Name
             * Description
             * Source node
             * Target node
             * Geometry
             * ---------
             * Test these individual components */

            for (int i = 0; i < network.Branches.Count; i++)
            {
                var branch = network.Branches[i];
                var reconstructedBranch = reconstructedNetwork.Branches[i];

                HydroNetworkTestHelper.CompareAndAssertBranches(branch, reconstructedBranch);

                Assert.AreEqual(branch.Source.Network, network);
                Assert.AreEqual(branch.Target.Network, network);

                Assert.AreEqual(reconstructedBranch.Source.Network, reconstructedNetwork);
                Assert.AreEqual(reconstructedBranch.Target.Network, reconstructedNetwork);
            }
        }
    }
}
