using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Converters.WellKnownText;
using SharpMap.Editors.FallOff;
using SharpMap.Editors.Interactors.Network;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Editors
{
    // TODO: extract ChannelInteractorTest and move to SharpMap
    [TestFixture]
    public class ChannelInteractorTest
    {
        private static INetwork network;
        private static IChannel branch1;
        private static IHydroNode node1;
        private static IHydroNode node2;

        [SetUp]
        public void NetworkSetup()
        {
            network = new HydroNetwork();

            node1 = new HydroNode { Geometry = new Point(0, 0) };
            node2 = new HydroNode { Geometry = new Point(50, 0) };

            branch1 = new Channel
                          {
                              Geometry =
                                  new LineString(new[]
                                                     {
                                                         new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(20, 0),
                                                         new Coordinate(30, 0), new Coordinate(40, 0), new Coordinate(50, 0)
                                                     }),
                              Source = node1,
                              Target = node2,
                          };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            network.Branches.Add(branch1);
        }

        [Test]
        public void PropertyTest()
        {
            var interactor = new ChannelInteractor(null, branch1, null, null);
            Assert.AreEqual(true, interactor.AllowDeletion());
            Assert.AreEqual(true, interactor.AllowMove());
        }

        [Test]
        public void MoveBranchTest()
        {
            var interactor = new ChannelInteractor(null, branch1, null, null);
            interactor.Network = network;
            const double deltaX = 0;
            const double deltaY = 5;
            //nodeEditor.Move(node1, deltaX, deltaY);
            // start
            interactor.Start();
            interactor.FallOffPolicy = new NoFallOffPolicy();
            interactor.SetTrackerSelection(interactor.Trackers.First(), true);
            interactor.MoveTracker(interactor.Trackers.First(), deltaX, deltaY);
            // select tracker 2; 0 and 2 are now selected.
            interactor.SetTrackerSelection(interactor.Trackers[2], true);
            interactor.MoveTracker(interactor.Trackers[2], deltaX, deltaY);
            interactor.Stop();

            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].X);
            Assert.AreEqual(10, branch1.Geometry.Coordinates[0].Y);
            Assert.AreEqual(10, branch1.Geometry.Coordinates[1].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[1].Y);
            Assert.AreEqual(20, branch1.Geometry.Coordinates[2].X);
            Assert.AreEqual(5, branch1.Geometry.Coordinates[2].Y);
            Assert.AreEqual(30, branch1.Geometry.Coordinates[3].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[3].Y);
            Assert.AreEqual(40, branch1.Geometry.Coordinates[4].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[4].Y);
            Assert.AreEqual(50, branch1.Geometry.Coordinates[5].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[5].Y);
        }

        private ICrossSection AddCrossSection(out ChannelInteractor interactor)
        {
            var crossSection = new CrossSectionDefinitionYZ();
            crossSection.YZDataTable.AddCrossSectionYZRow(0, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(2,-2);
            crossSection.YZDataTable.AddCrossSectionYZRow(4, 0);

            //CrossSectionHelper.CalculateYZProfileFromGeometry(crossSection.Profile, crossSection.Geometry);
            var crossSectionBranchFeature = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, crossSection, 25.0);
            
            interactor = new ChannelInteractor(null, branch1, null, null);
            
            return crossSectionBranchFeature;
        }

        /// <summary>
        /// Create branch of length 50 and add cross section at position 25
        /// 
        /// O------x------x--CS--x------x------x
        /// 0     10     20  25 30     40     50
        /// 
        /// if tracker at 10 is moved expect CS not to move
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void MoveBranchWithCrossSectionTestNoFallOffPolicyAt10()
        {
            ChannelInteractor interactor;
            var crossSection = AddCrossSection(out interactor);
            interactor.Network = network;
            interactor.Start();
            interactor.SetTrackerSelection(interactor.Trackers[1], true);
            interactor.MoveTracker(interactor.Trackers[1], 0, 10);
            interactor.Stop();

            Assert.AreEqual(0, crossSection.Geometry.Coordinates[0].Y);
            Assert.AreEqual(25, crossSection.Geometry.Coordinates[0].X);
        }

        /// <summary>
        /// Create branch of length 50 and add cross section at position 25
        /// 
        /// O------x------x--CS--x------x------x
        /// 0     10     20  25 30     40     50
        /// 
        /// if tracker at 20 is moved expect CS to move within subsection (20-30) -> X constant, y changes
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void MoveBranchWithCrossSectionTestNoFallOffPolicyAt20()
        {
            ChannelInteractor interactor;
            var crossSection = AddCrossSection(out interactor);
            interactor.Network = network;
            interactor.Start();
            interactor.SetTrackerSelection(interactor.Trackers[2], true);
            interactor.MoveTracker(interactor.Trackers[2], 0, 10);
            interactor.Stop();

            Assert.AreNotEqual(0, crossSection.Geometry.Coordinates[0].Y);
            Assert.AreEqual(25, crossSection.Geometry.Coordinates[0].X);
        }

        /// <summary>
        /// Create branch of length 50 and add cross section at position 25
        /// 
        /// O------x------x--CS--x------x------x
        /// 0     10     20  25 30     40     50
        /// 
        /// if tracker at 30 is moved expect CS to move within subsection (20-30) -> X constant, y changes
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void MoveBranchWithCrossSectionTestNoFallOffPolicyAt30()
        {
            ChannelInteractor interactor;
            var crossSection = AddCrossSection(out interactor);
            interactor.Network = network;
            interactor.Start();
            interactor.SetTrackerSelection(interactor.Trackers[3], true);
            interactor.MoveTracker(interactor.Trackers[3], 0, 10);
            interactor.Stop();

            Assert.AreNotEqual(0, crossSection.Geometry.Coordinates[0].Y);
            Assert.AreEqual(25, crossSection.Geometry.Coordinates[0].X);
        }


        /// <summary>
        /// Create branch of length 50 and add cross section at position 25
        /// 
        /// O------x------x--CS--x------x------x
        /// 0     10     20  25 30     40     50
        /// 
        /// if tracker at 40 is moved expect CS not to move
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void MoveBranchWithCrossSectionTestNoFallOffPolicyAt40()
        {
            ChannelInteractor interactor;
            var crossSection = AddCrossSection(out interactor);
            interactor.Network = network;
            interactor.Start();
            interactor.SetTrackerSelection(interactor.Trackers[4], true);
            interactor.MoveTracker(interactor.Trackers[4], 0, 10);
            interactor.Stop();

            Assert.AreEqual(0, crossSection.Geometry.Coordinates[0].Y);
            Assert.AreEqual(25, crossSection.Geometry.Coordinates[0].X);
        }


        [Test]
        [Category(TestCategory.Integration)]
        public void MoveBranchWithStructureFeatureTest()
        {
            var compositeStructure = new CompositeBranchStructure
            {
                Geometry = new Point(new Coordinate(10, 0))
            };

            branch1.BranchFeatures.Add(compositeStructure);
            // NB structureFeature.Branch will be automatically set
            compositeStructure.Chainage = 10;
            var interactor = new ChannelInteractor(null, branch1, null, null);
            interactor.Network = network;
            const double deltaX = 40;
            const double deltaY = 0;
            //nodeEditor.Move(node1, deltaX, deltaY);
            // start
            interactor.FallOffPolicy = new NoFallOffPolicy();
            interactor.Start();
            interactor.SetTrackerSelection(interactor.Trackers[4], true);
            interactor.MoveTracker(interactor.Trackers[4], deltaX, deltaY);

            interactor.Stop();

            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].Y);
            Assert.AreEqual(80, branch1.Geometry.Coordinates[4].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[4].Y);

            // no falloff policy cross section should not move
            Assert.AreEqual(0, compositeStructure.Geometry.Coordinates[0].Y);
            Assert.AreEqual(10, compositeStructure.Geometry.Coordinates[0].X);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void MoveBranchWithStructureFeatureTestLinearFallOffPolicy()
        {
            var compositeStructure = new CompositeBranchStructure
            {
                Geometry = new Point(new Coordinate(10, 0))
            };


            branch1.BranchFeatures.Add(compositeStructure);
            // NB structureFeature.Branch will be automatically set
            compositeStructure.Chainage = 10;
            var interactor = new ChannelInteractor(null, branch1, null, null) {Network = network};

            const double deltaX = 50;
            const double deltaY = 0;
            //nodeEditor.Move(node1, deltaX, deltaY);
            // start
            interactor.FallOffPolicy = new LinearFallOffPolicy();
            interactor.Start();
            interactor.SetTrackerSelection(interactor.Trackers[5], true);
            interactor.MoveTracker(interactor.Trackers[5], deltaX, deltaY);

            interactor.Stop();

            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].Y);
            Assert.AreEqual(100, branch1.Geometry.Coordinates[5].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[5].Y);

            Assert.AreEqual(0, compositeStructure.Geometry.Coordinates[0].Y);
            Assert.AreEqual(20, compositeStructure.Geometry.Coordinates[0].X);
        }


        //[Test]
        //[Category(TestCategory.Integration)]
        //public void MoveBranchWithDiscretisationNoFallOfPolicy()
        //{
        //}

        [Test]
        [Category(TestCategory.Integration)]
        public void DeleteBranchIncludingNodes()
        {
            var interactor = new ChannelInteractor(null, branch1, null, null);
            interactor.Network = network;

            Assert.AreEqual(1, network.Branches.Count);
            Assert.AreEqual(2, network.Nodes.Count);
            
            interactor.Start();
            interactor.SetTrackerSelection(interactor.Trackers[0], true);
            interactor.Delete();
            interactor.Stop();

            Assert.AreEqual(0, network.Branches.Count);
            Assert.AreEqual(0, network.Nodes.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void DeleteBranchWithoutDeletingNodes()
        {
            var interactor = new ChannelInteractor(null, branch1, null, null);
            interactor.Network = network;
            interactor.BranchNodeTopology = new BranchNodeTopology(){ AllowRemoveUnusedNodes = false };

            Assert.AreEqual(1, network.Branches.Count);
            Assert.AreEqual(2, network.Nodes.Count);

            interactor.Start();
            interactor.SetTrackerSelection(interactor.Trackers[0], true);
            interactor.Delete();
            interactor.Stop();

            Assert.AreEqual(0, network.Branches.Count);
            Assert.AreEqual(2, network.Nodes.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddBranchByDrawingWithoutAllowReUseNodes()
        {
            INetwork hydroNetwork = new HydroNetwork();

            IHydroNode hydroNode1 = new HydroNode { Name = "Node1", Geometry = new Point(0, 0) };
            IHydroNode hydroNode2 = new HydroNode { Name = "Node2", Geometry = new Point(50, 0) };
            hydroNetwork.Nodes.Add(hydroNode1);
            hydroNetwork.Nodes.Add(hydroNode2);

            IChannel channel = new Channel
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(50, 0) }),
            };

            var interactor = new ChannelInteractor(null, channel, null, null);
            interactor.Network = hydroNetwork;
            interactor.BranchNodeTopology = new BranchNodeTopology() { AllowReUseNodes = false };
            interactor.Add(channel);

            Assert.AreEqual(2, hydroNetwork.Nodes.Count);
            Assert.AreEqual(1, hydroNetwork.Branches.Count);
            Assert.AreEqual(hydroNode1, hydroNetwork.Branches[0].Source);
            Assert.AreEqual(hydroNode2, hydroNetwork.Branches[0].Target);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddBranchForFlow1DNetwork()
        {
            // AllowReUseNodes = true, AllowRemoveUnusedNodes = true
            INetwork hydroNetwork = new HydroNetwork();

            IHydroNode hydroNode1 = new HydroNode { Name = "Node1", Geometry = new Point(0, 0) };
            IHydroNode hydroNode2 = new HydroNode { Name = "Node1", Geometry = new Point(50, 0) };
            hydroNetwork.Nodes.Add(hydroNode1);
            hydroNetwork.Nodes.Add(hydroNode2);

            IChannel channel1 = new Channel
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(50, 0) }),
                Source = hydroNode1,
                Target = hydroNode2,
            };
            hydroNetwork.Branches.Add(channel1);

            Assert.AreEqual(2, hydroNetwork.Nodes.Count);
            Assert.AreEqual(1, hydroNetwork.Branches.Count);
            Assert.AreEqual(hydroNode1, hydroNetwork.Branches[0].Source);
            Assert.AreEqual(hydroNode2, hydroNetwork.Branches[0].Target);

            IHydroNode hydroNode3 = new HydroNode { Name = "Node3", Geometry = new Point(50, 50) };
            IHydroNode hydroNode2b = new HydroNode { Name = "Node2b", Geometry = new Point(50, 0) };
            hydroNetwork.Nodes.Add(hydroNode3);
            hydroNetwork.Nodes.Add(hydroNode2b);
            IChannel channel2 = new Channel
            {
                Geometry = new LineString(new[] { new Coordinate(50, 50), new Coordinate(50, 0) }),
                Source = hydroNode3,
                Target = hydroNode2b,
            };

            var interactor = new ChannelInteractor(null, channel2, null, null);
            interactor.Network = hydroNetwork;
            interactor.BranchNodeTopology = new BranchNodeTopology() { AllowReUseNodes = true, AllowRemoveUnusedNodes = true };
            interactor.Add(channel2);

            // hydroNode2 is reused eventhough hydroNode2b was set as Target before adding the branch
            // hydroNode2b becomes unused and is therefore removed
            Assert.AreEqual(3, hydroNetwork.Nodes.Count);
            Assert.AreEqual(2, hydroNetwork.Branches.Count);
            Assert.AreEqual(hydroNode3, hydroNetwork.Branches[1].Source);
            Assert.AreEqual(hydroNode2, hydroNetwork.Branches[1].Target);
        }

        [Test]
        public void AddBranchWithCoordinateSystem()
        {
            var mocks = new MockRepository();
            var network = mocks.StrictMock<IHydroNetwork>();
            var channel = new Channel() { Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 50 0)"), GeodeticLength = 150 };
            network.Expect(n => n.CoordinateSystem).Return(new OgrCoordinateSystemFactory().CreateFromEPSG(28992)).Repeat.Twice(); // Amersfoort / RD New
            network.Expect(n => n.Branches).PropertyBehavior();
            network.Branches = new EventedList<IBranch>();
            network.Expect(n => n.Nodes).Return(new EventedList<INode> {node1, node2}).Repeat.Times(4);
            var interactor = new ChannelInteractor(null, channel, null, null);
            interactor.Network = network;
            channel.Network = network;
            mocks.ReplayAll();
            if (Map.CoordinateSystemFactory == null)
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            Assert.That(channel.GeodeticLength, Is.EqualTo(150).Within(0.1));
            interactor.Add(channel);
            Assert.That(channel.GeodeticLength, Is.EqualTo(channel.Length).Within(0.1));
            Assert.That(channel.GeodeticLength, Is.Not.EqualTo(150).Within(0.1));
            mocks.VerifyAll();
        }
        [Test]
        public void AddBranchWithoutCoordinateSystem()
        {
            var mocks = new MockRepository();
            var network = mocks.StrictMock<IHydroNetwork>();
            var channel = new Channel() { Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 50 0)") };
            network.Expect(n => n.CoordinateSystem).Return(null).Repeat.Once();
            network.Expect(n => n.Branches).PropertyBehavior();
            network.Branches = new EventedList<IBranch>();
            network.Expect(n => n.Nodes).Return(new EventedList<INode> {node1, node2}).Repeat.Times(4);
            var interactor = new ChannelInteractor(null, channel, null, null);
            interactor.Network = network;
            channel.Network = network;
            mocks.ReplayAll();
            if (Map.CoordinateSystemFactory == null)
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            Assert.That(channel.GeodeticLength, Is.NaN);
            Assert.That(channel.Length, Is.EqualTo(50).Within(0.1));
            interactor.Add(channel);
            Assert.That(channel.GeodeticLength, Is.NaN);
            Assert.That(channel.Length, Is.EqualTo(50).Within(0.1));
            mocks.VerifyAll();
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddBranchForWFDExplorerNetwork()
        {
            // AllowReUseNodes = false, AllowRemoveUnusedNodes = false
            INetwork hydroNetwork = new HydroNetwork();

            IHydroNode hydroNode1 = new HydroNode { Name = "Node1", Geometry = new Point(0, 0) };
            IHydroNode hydroNode2 = new HydroNode { Name = "Node1", Geometry = new Point(50, 0) };
            hydroNetwork.Nodes.Add(hydroNode1);
            hydroNetwork.Nodes.Add(hydroNode2);

            IChannel channel1 = new Channel
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(50, 0) }),
                Source = hydroNode1,
                Target = hydroNode2,
            };
            hydroNetwork.Branches.Add(channel1);

            Assert.AreEqual(2, hydroNetwork.Nodes.Count);
            Assert.AreEqual(1, hydroNetwork.Branches.Count);
            Assert.AreEqual(hydroNode1, hydroNetwork.Branches[0].Source);
            Assert.AreEqual(hydroNode2, hydroNetwork.Branches[0].Target);

            IHydroNode hydroNode3 = new HydroNode { Name = "Node3", Geometry = new Point(50, 50) };
            IHydroNode hydroNode2b = new HydroNode { Name = "Node2b", Geometry = new Point(50, 0) };
            hydroNetwork.Nodes.Add(hydroNode3);
            hydroNetwork.Nodes.Add(hydroNode2b);
            IChannel channel2 = new Channel
            {
                Geometry = new LineString(new[] { new Coordinate(50, 50), new Coordinate(50, 0) }),
                Source = hydroNode3,
                Target = hydroNode2b,
            };
            IHydroNode hydroNode4 = new HydroNode { Name = "Node4", Geometry = new Point(150, 0) };
            hydroNetwork.Nodes.Add(hydroNode4);

            var interactor = new ChannelInteractor(null, channel2, null, null);
            interactor.Network = hydroNetwork;
            interactor.BranchNodeTopology = new BranchNodeTopology() { AllowReUseNodes = true, AllowRemoveUnusedNodes = false };
            interactor.Add(channel2);

            // hydroNode2 is reused
            // hydroNode4 is unused but not removed
            Assert.AreEqual(5, hydroNetwork.Nodes.Count);
            Assert.AreEqual(2, hydroNetwork.Branches.Count);
            Assert.AreEqual(hydroNode3, hydroNetwork.Branches[1].Source);
            Assert.AreEqual(hydroNode2, hydroNetwork.Branches[1].Target);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddChannel_ToExistingSewerConnection_ManholeIsReplacedWithNewChannelHydroNode()
        {
            // Setup
            var coordinate1 = new Coordinate(1.23, 2.34);
            var coordinate2 = new Coordinate(3.45, 4.56);
            var coordinate3 = new Coordinate(5.67, 6.78);

            var sewerConnection = GetBranch<SewerConnection>(coordinate1, coordinate2);
            var channel = GetBranch<Channel>(coordinate2, coordinate3);

            var hydroNetwork = new HydroNetwork();

            SewerConnectionInteractor sewerConnectionInteractor = GetSewerConnectionInteractor(hydroNetwork, sewerConnection);
            ChannelInteractor channelInteractor = GetChannelInteractor(hydroNetwork, channel);

            // Add a sewer connection to the network
            sewerConnectionInteractor.Add(sewerConnection);
            AssertAddedToNetwork(hydroNetwork, sewerConnection);

            INode manholeToReplace = sewerConnection.Target;

            // Call
            channelInteractor.Add(channel);

            // Assert
            AssertAddedToNetwork(hydroNetwork, channel);
            Assert.That(channel.Source, Is.SameAs(sewerConnection.Target));
            Assert.That(hydroNetwork.Nodes, Does.Not.Contain(manholeToReplace));
            Assert.That(hydroNetwork.Manholes, Does.Not.Contain(manholeToReplace));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RemoveChannel_FromExistingSewerConnection_HydroNodeIsReplacedWithNewManhole()
        {
            // Setup
            var coordinate1 = new Coordinate(1.23, 2.34);
            var coordinate2 = new Coordinate(3.45, 4.56);
            var coordinate3 = new Coordinate(5.67, 6.78);

            var sewerConnection = GetBranch<SewerConnection>(coordinate1, coordinate2);
            var channel = GetBranch<Channel>(coordinate2, coordinate3);

            var hydroNetwork = new HydroNetwork();

            SewerConnectionInteractor sewerConnectionInteractor = GetSewerConnectionInteractor(hydroNetwork, sewerConnection);
            ChannelInteractor channelInteractor = GetChannelInteractor(hydroNetwork, channel);

            // Add a sewer connection to the network
            sewerConnectionInteractor.Add(sewerConnection);
            AssertAddedToNetwork(hydroNetwork, sewerConnection);

            // Add a channel to the network
            channelInteractor.Add(channel);
            AssertAddedToNetwork(hydroNetwork, channel);

            Assert.That(sewerConnection.Target, Is.SameAs(channel.Source));

            // Call
            channelInteractor.Delete();

            // Assert
            AssertRemovedFromNetwork(hydroNetwork, channel);

            Assert.That(sewerConnection.Target, Is.Not.SameAs(channel.Source));
            Assert.That(hydroNetwork.Nodes, Does.Contain(sewerConnection.Target));
            Assert.That(hydroNetwork.Manholes, Does.Contain(sewerConnection.Target));
        }

        private static void AssertAddedToNetwork(IHydroNetwork hydroNetwork, SewerConnection sewerConnection)
        {
            Assert.That(hydroNetwork.Branches, Does.Contain(sewerConnection));
            Assert.That(hydroNetwork.SewerConnections, Does.Contain(sewerConnection));
            Assert.That(hydroNetwork.Nodes, Does.Contain(sewerConnection.Source));
            Assert.That(hydroNetwork.Nodes, Does.Contain(sewerConnection.Target));
            Assert.That(hydroNetwork.Manholes, Does.Contain(sewerConnection.Source));
            Assert.That(hydroNetwork.Manholes, Does.Contain(sewerConnection.Target));
        }

        private static void AssertAddedToNetwork(IHydroNetwork hydroNetwork, Channel channel)
        {
            Assert.That(hydroNetwork.Branches, Does.Contain(channel));
            Assert.That(hydroNetwork.Channels, Does.Contain(channel));
            Assert.That(hydroNetwork.Nodes, Does.Contain(channel.Source));
            Assert.That(hydroNetwork.Nodes, Does.Contain(channel.Target));
            Assert.That(hydroNetwork.HydroNodes, Does.Contain(channel.Source));
            Assert.That(hydroNetwork.HydroNodes, Does.Contain(channel.Target));
        }

        private static void AssertRemovedFromNetwork(IHydroNetwork hydroNetwork, Channel channel)
        {
            Assert.That(hydroNetwork.Branches, Does.Not.Contain(channel));
            Assert.That(hydroNetwork.Channels, Does.Not.Contain(channel));
            Assert.That(hydroNetwork.Nodes, Does.Not.Contain(channel.Source));
            Assert.That(hydroNetwork.Nodes, Does.Not.Contain(channel.Target));
            Assert.That(hydroNetwork.HydroNodes, Does.Not.Contain(channel.Source));
            Assert.That(hydroNetwork.HydroNodes, Does.Not.Contain(channel.Target));
        }

        private static SewerConnectionInteractor GetSewerConnectionInteractor(IHydroNetwork hydroNetwork, ISewerConnection sewerConnection)
        {
            return new SewerConnectionInteractor(null,
                                                 sewerConnection,
                                                 null,
                                                 null) { Network = hydroNetwork };
        }

        private static ChannelInteractor GetChannelInteractor(IHydroNetwork hydroNetwork, IChannel channel)
        {
            return new ChannelInteractor(null,
                                         channel,
                                         null,
                                         null) { Network = hydroNetwork };
        }

        private static TBranch GetBranch<TBranch>(Coordinate start, Coordinate end) where TBranch : IBranch, new()
        {
            var geometry = new LineString(new[] { start, end });
            var branch = new TBranch { Geometry = geometry };

            return branch;
        }
    }
}
