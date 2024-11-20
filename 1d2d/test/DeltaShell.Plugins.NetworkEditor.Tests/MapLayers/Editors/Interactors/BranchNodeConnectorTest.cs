using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Editors.Interactors
{
    [TestFixture]
    public class BranchNodeConnectorTest
    {
        [Test]
        [TestCaseSource(nameof(ConnectNodes_ArgNullCases))]
        public void ConnectNodes_ArgNull_ThrowsArgumentNullException(IBranch branch, INetwork network)
        {
            // Setup
            var connector = new BranchNodeConnector();

            // Call
            void Call()
            {
                connector.ConnectNodes(branch, network);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void ConnectNodes_ExistingHydroNodeAtSourceLocation_SetsNodesOnBranch()
        {
            // Setup
            var connector = new BranchNodeConnector();

            var startNodeCoordinate = new Coordinate(1.23, 2.34);
            var endNodeCoordinate = new Coordinate(3.45, 4.56);

            IBranch branch = GetBranch(startNodeCoordinate, endNodeCoordinate);
            var existingHydroNode = GetNode<IHydroNode>(startNodeCoordinate);
            var newNode = Substitute.For<INode>();
            INetwork network = GetNetworkWithNode(existingHydroNode, newNode);

            // Call
            connector.ConnectNodes(branch, network);

            // Assert
            Assert.That(branch.Source, Is.SameAs(existingHydroNode));
            Assert.That(branch.Target, Is.SameAs(newNode));
            Assert.That(network.Nodes, Does.Contain(newNode));
        }

        [Test]
        public void ConnectNodes_ExistingHydroNodeAtTargetLocation_SetsNodesOnBranch()
        {
            // Setup
            var connector = new BranchNodeConnector();

            var startNodeCoordinate = new Coordinate(1.23, 2.34);
            var endNodeCoordinate = new Coordinate(3.45, 4.56);

            IBranch branch = GetBranch(startNodeCoordinate, endNodeCoordinate);
            var existingHydroNode = GetNode<IHydroNode>(endNodeCoordinate);
            var newNode = Substitute.For<INode>();
            INetwork network = GetNetworkWithNode(existingHydroNode, newNode);

            // Call
            connector.ConnectNodes(branch, network);

            // Assert
            Assert.That(branch.Source, Is.SameAs(newNode));
            Assert.That(branch.Target, Is.SameAs(existingHydroNode));
            Assert.That(network.Nodes, Does.Contain(newNode));
        }

        [Test]
        public void ConnectNodes_ExistingManholeAtSourceLocation_ExistingManholeIsReplacedWithANewNode()
        {
            // Setup
            var connector = new BranchNodeConnector();

            var startNodeCoordinate = new Coordinate(1.23, 2.34);
            var endNodeCoordinate = new Coordinate(3.45, 4.56);

            IBranch branch = GetBranch(startNodeCoordinate, endNodeCoordinate);
            var existingManhole = GetNode<IManhole>(startNodeCoordinate);
            var newNode = Substitute.For<INode>();
            INetwork network = GetNetworkWithNode(existingManhole, newNode);

            // Call
            connector.ConnectNodes(branch, network);

            // Assert
            Assert.That(branch.Source, Is.SameAs(newNode));
            Assert.That(branch.Target, Is.SameAs(newNode));
            foreach (IBranch incomingBranch in existingManhole.IncomingBranches)
            {
                Assert.That(incomingBranch.Target, Is.EqualTo(newNode));
            }

            foreach (IBranch outgoingBranch in existingManhole.OutgoingBranches)
            {
                Assert.That(outgoingBranch.Source, Is.EqualTo(newNode));
            }

            Assert.That(network.Nodes, Does.Contain(newNode));
            Assert.That(network.Nodes, Does.Not.Contain(existingManhole));
        }

        [Test]
        public void ConnectNodes_ExistingManholeAtTargetLocation_ExistingManholeIsReplacedWithANewNode()
        {
            // Setup
            var connector = new BranchNodeConnector();

            var startNodeCoordinate = new Coordinate(1.23, 2.34);
            var endNodeCoordinate = new Coordinate(3.45, 4.56);

            IBranch branch = GetBranch(startNodeCoordinate, endNodeCoordinate);
            var existingManhole = GetNode<IManhole>(endNodeCoordinate);
            var newNode = Substitute.For<INode>();
            INetwork network = GetNetworkWithNode(existingManhole, newNode);

            // Call
            connector.ConnectNodes(branch, network);

            // Assert
            Assert.That(branch.Source, Is.SameAs(newNode));
            Assert.That(branch.Target, Is.SameAs(newNode));
            foreach (IBranch incomingBranch in existingManhole.IncomingBranches)
            {
                Assert.That(incomingBranch.Target, Is.EqualTo(newNode));
            }

            foreach (IBranch outgoingBranch in existingManhole.OutgoingBranches)
            {
                Assert.That(outgoingBranch.Source, Is.EqualTo(newNode));
            }

            Assert.That(network.Nodes, Does.Contain(newNode));
            Assert.That(network.Nodes, Does.Not.Contain(existingManhole));
        }

        private static IEnumerable<TestCaseData> ConnectNodes_ArgNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<INetwork>());
            yield return new TestCaseData(Substitute.For<IBranch>(), null);
        }

        private static IBranch GetBranch(Coordinate start, Coordinate end)
        {
            var geometry = Substitute.For<IGeometry>();
            geometry.Coordinates.Returns(new[] { start, end });

            var branch = Substitute.For<IBranch>();
            branch.Geometry = geometry;

            return branch;
        }

        private static TNode GetNode<TNode>(Coordinate coordinate) where TNode : class, INode
        {
            var geometry = Substitute.For<IGeometry>();
            geometry.Coordinate.Returns(coordinate);

            var node = Substitute.For<TNode>();
            node.Geometry = geometry;

            node.IncomingBranches.Returns(new EventedList<IBranch>
            {
                Substitute.For<IBranch>(),
                Substitute.For<IBranch>()
            });
            node.OutgoingBranches.Returns(new EventedList<IBranch>
            {
                Substitute.For<IBranch>(),
                Substitute.For<IBranch>()
            });

            return node;
        }

        private static INetwork GetNetworkWithNode(INode existingNode, INode newNode)
        {
            var network = Substitute.For<INetwork>();
            network.Nodes.Returns(new EventedList<INode> { existingNode });
            network.NewNode().Returns(newNode);

            return network;
        }
    }
}