using System.Collections;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Grid;
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
            var network = new HydroNetwork { Name = "my Network" };
            var hydroNode1 = new HydroNode { Name = "my Node1", LongName= "Node 1 Description", Description = "Node 1 Description", Geometry = new Point(0, 0), Network = network };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode { Name = "my Node2", LongName = "Node 2 Description", Description = "Node 2 Description", Geometry = new Point(3, 4), Network = network };
            network.Nodes.Add(hydroNode2);
            var branch1 = new Branch
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

            //      * (3, 4)
            //     /
            //    /
            //   /
            //  /
            // * (0, 0)

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
            Assert.AreEqual(new[] {2}, networkDataModel.NumberOfGeometryPointsPerBranch);

            Assert.AreEqual(new[] { "my Branch 1" }, networkDataModel.BranchNames);
            Assert.AreEqual(new[] { "Branch 1 Description" }, networkDataModel.BranchDescriptions);
            
            // Test the geometry points
            Assert.AreEqual(new[] {0, 3}, networkDataModel.GeopointsX);
            Assert.AreEqual(new[] {0, 4}, networkDataModel.GeopointsY);
        }

        [Test]
        public void GivenNetworkWithOneCompartmentInOneManhole_WhenInstantiatingNetworkUGridDataModel_ThenCompartmentsAreTreatedAsIndividualNodes()
        {
            // Check network node properties
            var networkDataModel = GetNetworkUGridDataModel(1);
            Assert.That(networkDataModel.NumberOfNodes, Is.EqualTo(4));
            Assert.That(networkDataModel.NodesX, Is.EqualTo(new[] { 10.0, 11.5, 12.5, 8.0 }));
            Assert.That(networkDataModel.NodesY, Is.EqualTo(new[] { 10.0, 15.0, 15.0, 8.0 }));
            Assert.That(networkDataModel.NodesNames, Is.EqualTo(new[] { "cmp1", "cmp11", "cmp12", "cmp21" }));
            Assert.That(networkDataModel.NodesDescriptions, Is.EqualTo(new[] { string.Empty, string.Empty, string.Empty, string.Empty }));

            CheckNetworkBranchProperties(networkDataModel, new[] { 10.0, 11.5, 8.0, 10.0 });
        }

        [Test]
        public void GivenNetworkWithTwoCompartmentsInOneManhole_WhenInstantiatingNetworkUGridDataModel_ThenCompartmentsAreTreatedAsIndividualNodes()
        {
            // Check network node properties
            var networkDataModel = GetNetworkUGridDataModel(2);
            Assert.That(networkDataModel.NumberOfNodes, Is.EqualTo(5));
            Assert.That(networkDataModel.NodesX, Is.EqualTo(new[] { 9.5, 10.5, 11.5, 12.5, 8.0 }));
            Assert.That(networkDataModel.NodesY, Is.EqualTo(new[] { 10.0, 10.0, 15.0, 15.0, 8.0 }));
            Assert.That(networkDataModel.NodesNames, Is.EqualTo(new[] { "cmp1", "cmp2", "cmp11", "cmp12", "cmp21" }));
            Assert.That(networkDataModel.NodesDescriptions, Is.EqualTo(new[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty }));

            CheckNetworkBranchProperties(networkDataModel, new[] { 9.5, 11.5, 8.0, 9.5 });
        }

        [Test]
        public void GivenNetworkWithThreeCompartmentsInOneManhole_WhenInstantiatingNetworkUGridDataModel_ThenCompartmentsAreTreatedAsIndividualNodes()
        {
            // Check network node properties
            var networkDataModel = GetNetworkUGridDataModel(3);
            Assert.That(networkDataModel.NumberOfNodes, Is.EqualTo(6));
            Assert.That(networkDataModel.NodesX, Is.EqualTo(new[] { 9.0, 10.0, 11.0, 11.5, 12.5, 8.0 }));
            Assert.That(networkDataModel.NodesY, Is.EqualTo(new[] { 10.0, 10.0, 10.0, 15.0, 15.0, 8.0 }));
            Assert.That(networkDataModel.NodesNames, Is.EqualTo(new[] { "cmp1", "cmp2", "cmp3", "cmp11", "cmp12", "cmp21" }));
            Assert.That(networkDataModel.NodesDescriptions, Is.EqualTo(new[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty }));

            CheckNetworkBranchProperties(networkDataModel, new[] { 9.0, 11.5, 8.0, 9.0 });
        }

        [Test]
        public void ReconstructHydroNetworkTest()
        {
            var network = TestNetwork();

            var networkDataModel = new NetworkUGridDataModel(network);

            var reconstructedNetwork = NetworkDiscretisationFactory.CreateHydroNetwork(networkDataModel);
            HydroNetworkTestHelper.CompareNetworks(network, reconstructedNetwork);
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

                HydroNetworkTestHelper.CompareNodes(node, reconstructedNode);

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

                HydroNetworkTestHelper.CompareBranches(branch, reconstructedBranch);

                Assert.AreEqual(branch.Source.Network, network);
                Assert.AreEqual(branch.Target.Network, network);

                Assert.AreEqual(reconstructedBranch.Source.Network, reconstructedNetwork);
                Assert.AreEqual(reconstructedBranch.Target.Network, reconstructedNetwork);
            }
        }

        #region Test helpers

        private static NetworkUGridDataModel GetNetworkUGridDataModel(int numberOfCompartments)
        {
            var network = new HydroNetwork {Name = "my Network"};

            var manhole1 = new Manhole("myManhole1") { Geometry = new Point(10, 10) };
            for (var i = 0; i < numberOfCompartments; i++)
            {
                manhole1.Compartments.Add(new Compartment("cmp" + (i+1)));
            }
            network.Nodes.Add(manhole1);

            var manhole2 = new Manhole("myManhole2")
            {
                Geometry = new Point(12, 15),
                Compartments = new EventedList<Compartment> { new Compartment("cmp11"), new Compartment("cmp12") }
            };
            network.Nodes.Add(manhole2);

            var manhole3 = new Manhole("myManhole3")
            {
                Geometry = new Point(8, 8),
                Compartments = new EventedList<Compartment> { new Compartment("cmp21") }
            };
            network.Nodes.Add(manhole3);

            var source1 = manhole1.Compartments.FirstOrDefault(c => c.Name == "cmp1");
            var target1 = manhole2.Compartments.FirstOrDefault(c => c.Name == "cmp11");
            var sewerConnection1 = new SewerConnection
            {
                Name = "mySewerConnection1",
                SourceCompartment = source1,
                TargetCompartment = target1,
                Geometry = new LineString(new [] { source1?.ParentManhole.Geometry.Coordinate, target1?.ParentManhole.Geometry.Coordinate })
            };
            network.Branches.Add(sewerConnection1);

            var source2 = manhole3.Compartments.FirstOrDefault(c => c.Name == "cmp21");
            var target2 = manhole1.Compartments.FirstOrDefault(c => c.Name == "cmp1");
            var sewerConnection2 = new SewerConnection
            {
                Name = "mySewerConnection2",
                SourceCompartment = source2,
                TargetCompartment = target2,
                Geometry = new LineString(new[] { source2?.ParentManhole.Geometry.Coordinate, target2?.ParentManhole.Geometry.Coordinate })
            };
            network.Branches.Add(sewerConnection2);

            return new NetworkUGridDataModel(network);
        }

        private static void CheckNetworkBranchProperties(NetworkUGridDataModel networkDataModel, IEnumerable geopointsX)
        {
            Assert.That(networkDataModel.NumberOfBranches, Is.EqualTo(2));
            Assert.That(networkDataModel.SourceNodeIds.Length, Is.EqualTo(2));
            Assert.That(networkDataModel.TargedNodesIds.Length, Is.EqualTo(2));
            Assert.That(networkDataModel.BranchLengths, Is.EqualTo(new[] { 0, 0 }));
            Assert.That(networkDataModel.NumberOfGeometryPoints, Is.EqualTo(4));
            Assert.That(networkDataModel.NumberOfGeometryPointsPerBranch, Is.EqualTo(new[] { 2, 2 }));
            Assert.That(networkDataModel.BranchNames, Is.EqualTo(new[] { "mySewerConnection1", "mySewerConnection2" }));
            Assert.That(networkDataModel.GeopointsX, Is.EqualTo(geopointsX));
            Assert.That(networkDataModel.GeopointsY, Is.EqualTo(new[] { 10.0, 15.0, 8.0, 10.0 }));
        }

        #endregion
    }
}
