using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Editors.Interactors
{
    [TestFixture]
    public class BranchNodeDisconnectorTest
    {
        [Test]
        [TestCaseSource(nameof(DisconnectNodes_ArgNullCases))]
        public void DisconnectNodes_ArgNull_ThrowsArgumentNullException(INode node, IHydroNetwork hydroNetwork)
        {
            // Setup
            var disconnector = new BranchNodeDisconnector();

            // Call
            void Call()
            {
                disconnector.DisconnectNodes(node, hydroNetwork);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void DisconnectNodes_NodeConnected_ToNothing_NothingHappens()
        {
            // Setup
            var disconnector = new BranchNodeDisconnector();
            var node = Substitute.For<INode>();
            var hydroNetwork = Substitute.For<IHydroNetwork>();

            hydroNetwork.Nodes.Returns(new EventedList<INode> { node });

            node.IncomingBranches.Returns(new EventedList<IBranch>());
            node.OutgoingBranches.Returns(new EventedList<IBranch>());

            // Call
            disconnector.DisconnectNodes(node, hydroNetwork);

            // Assert
            Assert.That(hydroNetwork.Nodes, Does.Contain(node));
            Assert.That(node.IncomingBranches, Is.Empty);
            Assert.That(node.OutgoingBranches, Is.Empty);
        }

        [Test]
        public void DisconnectNodes_NodeConnected_ToOtherChannels_NothingHappens()
        {
            // Setup
            var disconnector = new BranchNodeDisconnector();
            var node = Substitute.For<INode>();
            var hydroNetwork = Substitute.For<IHydroNetwork>();

            hydroNetwork.Nodes.Returns(new EventedList<INode> { node });

            var incomingChannel = Substitute.For<IChannel>();
            incomingChannel.Target = node;
            node.IncomingBranches.Returns(new EventedList<IBranch> { incomingChannel });

            var outgoingChannel = Substitute.For<IChannel>();
            outgoingChannel.Source = node;
            node.OutgoingBranches.Returns(new EventedList<IBranch> { outgoingChannel });

            // Call
            disconnector.DisconnectNodes(node, hydroNetwork);

            // Assert
            Assert.That(hydroNetwork.Nodes, Does.Contain(node));
            Assert.That(incomingChannel.Target, Is.SameAs(node));
            Assert.That(outgoingChannel.Source, Is.SameAs(node));
        }

        [Test]
        public void DisconnectNodes_NodeConnected_ToOtherChannels_AndSewerConnections_NothingHappens()
        {
            // Setup
            var disconnector = new BranchNodeDisconnector();
            var node = Substitute.For<INode>();
            var hydroNetwork = Substitute.For<IHydroNetwork>();

            hydroNetwork.Nodes.Returns(new EventedList<INode> { node });

            var incomingChannel = Substitute.For<IChannel>();
            incomingChannel.Target = node;
            var incomingSewerConnection = Substitute.For<ISewerConnection>();
            incomingSewerConnection.Target = node;
            node.IncomingBranches.Returns(new EventedList<IBranch>
            {
                incomingChannel,
                incomingSewerConnection
            });

            var outgoingChannel = Substitute.For<IChannel>();
            outgoingChannel.Source = node;
            var outgoingSewerConnection = Substitute.For<ISewerConnection>();
            outgoingSewerConnection.Source = node;
            node.OutgoingBranches.Returns(new EventedList<IBranch>
            {
                outgoingChannel,
                outgoingSewerConnection
            });

            // Call
            disconnector.DisconnectNodes(node, hydroNetwork);

            // Assert
            Assert.That(hydroNetwork.Nodes, Does.Contain(node));
            Assert.That(incomingChannel.Target, Is.SameAs(node));
            Assert.That(incomingSewerConnection.Target, Is.SameAs(node));
            Assert.That(outgoingChannel.Source, Is.SameAs(node));
            Assert.That(outgoingSewerConnection.Source, Is.SameAs(node));
        }

        [Test]
        public void DisconnectNodes_NodeConnected_ToSewerConnections_NodeIsReplacedWithAManhole()
        {
            // Setup
            var disconnector = new BranchNodeDisconnector();
            var node = Substitute.For<INode>();
            var hydroNetwork = Substitute.For<IHydroNetwork>();

            hydroNetwork.Nodes.Returns(new EventedList<INode> { node });

            var incomingSewerConnection = Substitute.For<ISewerConnection>();
            incomingSewerConnection.Target = node;
            node.IncomingBranches.Returns(new EventedList<IBranch> { incomingSewerConnection });

            var outgoingSewerConnection = Substitute.For<ISewerConnection>();
            outgoingSewerConnection.Source = node;
            node.OutgoingBranches.Returns(new EventedList<IBranch> { outgoingSewerConnection });

            // Call
            disconnector.DisconnectNodes(node, hydroNetwork);

            // Assert
            Assert.That(hydroNetwork.Nodes, Does.Not.Contain(node));
            Assert.That(incomingSewerConnection.Target, Is.Not.SameAs(node));
            Assert.That(incomingSewerConnection.Target, Is.AssignableTo<IManhole>());
            Assert.That(outgoingSewerConnection.Source, Is.Not.SameAs(node));
            Assert.That(outgoingSewerConnection.Source, Is.AssignableTo<IManhole>());
            Assert.That(incomingSewerConnection.Target, Is.SameAs(outgoingSewerConnection.Source));
        }

        private static IEnumerable<TestCaseData> DisconnectNodes_ArgNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IHydroNetwork>());
            yield return new TestCaseData(Substitute.For<INode>(), null);
        }
    }
}