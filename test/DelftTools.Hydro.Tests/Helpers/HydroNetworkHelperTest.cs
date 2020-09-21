using System.Linq;
using DelftTools.Hydro.Helpers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Helpers
{
    [TestFixture]
    public class HydroNetworkHelperTest
    {
        [Test]
        public void DetectAndUpdateBranchBoundaries()
        {
            var network = new HydroNetwork();
            var branch1 = new Channel();
            var branch2 = new Channel();
            var node1 = new HydroNode();
            var node2 = new HydroNode();
            var node3 = new HydroNode();

            branch1.Source = node1;
            branch1.Target = node2;
            network.Branches.Add(branch1);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            Assert.IsFalse(node1.IsConnectedToMultipleBranches);
            Assert.IsFalse(node2.IsConnectedToMultipleBranches);
            Assert.IsFalse(node3.IsConnectedToMultipleBranches);

            branch2.Source = node2;
            branch2.Target = node3;
            network.Branches.Add(branch2);
            network.Nodes.Add(node3);

            Assert.IsFalse(node1.IsConnectedToMultipleBranches);
            Assert.IsTrue(node2.IsConnectedToMultipleBranches);
            Assert.IsFalse(node3.IsConnectedToMultipleBranches);
        }

        [Test]
        public void AddingToExistingChannelCopiesOrder()
        {
            IHydroNetwork network = CreateTestNetwork();
            IChannel channel1 = network.Channels.First();
            channel1.OrderNumber = 7;
            INode node2 = channel1.Target; // at coordinate (100,100), see method CreateTestNetwork

            var node3 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(200, 200))
            };
            network.Nodes.Add(node3);

            var channel2 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(100, 100),
                    new Coordinate(200, 200)
                }),
                Source = node2,
                Target = node3
            };

            NetworkHelper.AddChannelToHydroNetwork(network, channel2);
            Assert.AreEqual(channel1.OrderNumber, channel2.OrderNumber);
        }

        [Test]
        public void AddingToExistingJunctionAssignsOrderMinusOne()
        {
            // Creates a simple test network of 3 channels and 4 nodes, which looks like this:
            //
            //        (50,100) n2
            //                 |
            //        channel1 |
            //                 |       channel2
            //         (50,50) n1-------------------n3 (100,50)
            //                 |
            //        channel3 |
            //                 |
            //          (50,0) n4

            var network = new HydroNetwork();
            var n1 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(50, 50)),
                Name = "n1"
            };
            var n2 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(50, 100)),
                Name = "n2"
            };
            var n3 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(100, 50)),
                Name = "n3"
            };
            var n4 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(50, 0)),
                Name = "n4"
            };
            var channel1 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(50, 50),
                    new Coordinate(50, 100)
                }),
                Source = n1,
                Target = n2,
                Name = "channel1",
                OrderNumber = 7 // the order of the remaining channels will be computed when adding
                // them to the network
            };

            network.Nodes.Add(n1);
            network.Nodes.Add(n2);
            NetworkHelper.AddChannelToHydroNetwork(network, channel1);

            var channel2 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(50, 50),
                    new Coordinate(100, 50)
                }),
                Source = n1,
                Target = n3,
                Name = "channel2"
            };

            network.Nodes.Add(n3);
            NetworkHelper.AddChannelToHydroNetwork(network, channel2);

            var channel3 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(50, 50),
                    new Coordinate(50, 0)
                }),
                Source = n1,
                Target = n4,
                Name = "channel3"
            };

            network.Nodes.Add(n4);
            NetworkHelper.AddChannelToHydroNetwork(network, channel3);

            Assert.AreEqual(7, channel2.OrderNumber);
            Assert.AreEqual(-1, channel3.OrderNumber);

            // Now add a new channel from n1 to a new node (n5) at location (0,50). The order assigned to this
            // channel should be equal to: ((the order of the existing channel with the highest order) + 1)

            var n5 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(0, 50))
            };
            var channel4 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(50, 50),
                    new Coordinate(0, 50)
                }),
                Source = n1,
                Target = n5
            };
            network.Nodes.Add(n5);
            NetworkHelper.AddChannelToHydroNetwork(network, channel4);

            Assert.AreEqual(-1, channel4.OrderNumber);
        }

        [Test]
        public void AddingToExistingJunctionAssignsDefaultValueIfAllOrdersAreUndefined()
        {
            // Creates a simple test network of 3 channels and 4 nodes, which looks like this:
            //
            //        (50,100) n2
            //                 |
            //        channel1 |
            //                 |       channel2
            //         (50,50) n1-------------------n3 (100,50)
            //                 |
            //        channel3 |
            //                 |
            //          (50,0) n4

            var network = new HydroNetwork();
            var n1 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(50, 50)),
                Name = "n1"
            };
            var n2 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(50, 100)),
                Name = "n2"
            };
            var n3 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(100, 50)),
                Name = "n3"
            };
            var n4 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(50, 0)),
                Name = "n4"
            };
            var channel1 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(50, 50),
                    new Coordinate(50, 100)
                }),
                Source = n1,
                Target = n2,
                Name = "channel1" // we don't specify OrderNumber so it will use default value
            };

            network.Nodes.Add(n1);
            network.Nodes.Add(n2);
            NetworkHelper.AddChannelToHydroNetwork(network, channel1);

            var channel2 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(50, 50),
                    new Coordinate(100, 50)
                }),
                Source = n1,
                Target = n3,
                Name = "channel2"
            };

            network.Nodes.Add(n3);
            NetworkHelper.AddChannelToHydroNetwork(network, channel2);

            var channel3 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(50, 50),
                    new Coordinate(50, 0)
                }),
                Source = n1,
                Target = n4,
                Name = "channel3"
            };

            network.Nodes.Add(n4);
            NetworkHelper.AddChannelToHydroNetwork(network, channel3);

            Assert.AreEqual(-1, channel2.OrderNumber);
            Assert.AreEqual(-1, channel3.OrderNumber);

            // Now add a new channel from n1 to a new node (n5) at location (0,50). The order assigned to this
            // channel should be equal to the default value (-1) since all channels have this default value as well

            var n5 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(0, 50))
            };
            var channel4 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(50, 50),
                    new Coordinate(0, 50)
                }),
                Source = n1,
                Target = n5
            };
            network.Nodes.Add(n5);
            NetworkHelper.AddChannelToHydroNetwork(network, channel4);

            Assert.AreEqual(-1, channel4.OrderNumber);
        }

        [Test]
        public void MergingTwoNetworksByAConnectingBranchAssignsDefaultValueToThatBranch()
        {
            // Creates a simple test network of 2 channels and 4 nodes, which looks like this:
            //
            //         (50,50) n2                   n4 (100,50)
            //                 |                    |
            //        channel1 |                    | channel2
            //                 |                    |
            //          (50,0) n1                   n3 (100,0)
            //
            // Both channels have an order number defined. Next, nodes n2 and n4 are connected
            // by a new channel. This channel should be assigned a default value (-1) to its
            // order number.

            var network = new HydroNetwork();
            var n1 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(50, 0)),
                Name = "n1"
            };
            var n2 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(50, 50)),
                Name = "n2"
            };
            var n3 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(100, 0)),
                Name = "n3"
            };
            var n4 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(100, 50)),
                Name = "n4"
            };
            var channel1 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(50, 0),
                    new Coordinate(50, 50)
                }),
                Source = n1,
                Target = n2,
                Name = "channel1",
                OrderNumber = 1
            };

            network.Nodes.Add(n1);
            network.Nodes.Add(n2);
            NetworkHelper.AddChannelToHydroNetwork(network, channel1);

            var channel2 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(100, 0),
                    new Coordinate(100, 50)
                }),
                Source = n3,
                Target = n4,
                Name = "channel2",
                OrderNumber = 2
            };

            network.Nodes.Add(n3);
            network.Nodes.Add(n4);
            NetworkHelper.AddChannelToHydroNetwork(network, channel2);

            var channel3 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(50, 50),
                    new Coordinate(100, 50)
                }),
                Source = n2,
                Target = n4,
                Name = "channel3"
            };

            // Now add a new channel from n2 to n4. The order assigned to this
            // channel should be equal to the default value (-1)

            NetworkHelper.AddChannelToHydroNetwork(network, channel3);

            Assert.AreEqual(-1, channel3.OrderNumber);
        }

        [Test]
        public void CreateUniqueHydroObjectNameInSubregion()
        {
            var sourceObject = new HydroNode("source");
            var targetObject = new HydroNode("target");
            var link = new HydroLink(sourceObject, targetObject) {Name = "HydroLink1"};

            var region = new HydroRegion();
            region.Links.Add(link);
            var subRegion = new HydroRegion() {Parent = region};

            string name = HydroNetworkHelper.GetUniqueFeatureName(region, new HydroLink());

            Assert.AreNotEqual(link.Name, name);
        }

        [Test]
        public void KeepHydroObjectNameIfUniqueHydroObjectNameDoesntExitsInNetworkObject()
        {
            const string uniquehydrolinkName1 = "UniqueHydroLink1";
            const string uniquehydrolinkName2 = "UniqueHydroLink2";
            var sourceObject = new HydroNode("source");
            var targetObject = new HydroNode("target");
            var link = new HydroLink(sourceObject, targetObject) {Name = uniquehydrolinkName1};

            var region = new HydroRegion();
            region.Links.Add(link);

            string name = HydroNetworkHelper.GetUniqueFeatureName(region, new HydroLink() {Name = uniquehydrolinkName2}, true);
            Assert.That(name, Is.EqualTo(uniquehydrolinkName2));
        }

        [Test]
        public void DontKeepHydroObjectNameIfUniqueHydroObjectNameExitsInNetworkObjectAndCheckIfNewNameIsNeeded()
        {
            const string uniquehydrolinkName = "UniqueHydroLink";

            var sourceObject = new HydroNode("source");
            var targetObject = new HydroNode("target");
            var link = new HydroLink(sourceObject, targetObject) {Name = uniquehydrolinkName};

            var region = new HydroRegion();
            region.Links.Add(link);

            string nameAlreadyExistCreateNew = HydroNetworkHelper.GetUniqueFeatureName(region, new HydroLink() {Name = uniquehydrolinkName}, true);
            Assert.That(nameAlreadyExistCreateNew, Is.Not.EqualTo(uniquehydrolinkName));
        }

        [Test]
        public void CreateHydroObjectNameIfNewUniqueHydroObjectNameIsNullInNetworkObjectAndCheckIfNewNameIsNeeded()
        {
            const string uniquehydrolinkName = "UniqueHydroLink";

            var sourceObject = new HydroNode("source");
            var targetObject = new HydroNode("target");
            var link = new HydroLink(sourceObject, targetObject) {Name = uniquehydrolinkName};

            var region = new HydroRegion();
            region.Links.Add(link);

            string nameAlreadyExistCreateNew = HydroNetworkHelper.GetUniqueFeatureName(region, new HydroLink() {Name = null}, true);
            Assert.That(nameAlreadyExistCreateNew, Is.Not.Null);
            Assert.That(nameAlreadyExistCreateNew, Is.Not.EqualTo(link.Name));
        }

        /// <summary>
        /// Creates a simple test network of 1 branch and 2 nodes. The branch has '3' parts, in the center of
        /// the first and last is a cross section.
        /// n
        /// /
        /// /
        /// cs
        /// /
        /// -------/
        /// /
        /// cs
        /// /
        /// n
        /// </summary>
        /// <returns></returns>
        private static IHydroNetwork CreateTestNetwork()
        {
            var network = new HydroNetwork();
            var branch1 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(30, 40),
                    new Coordinate(70, 40),
                    new Coordinate(100, 100)
                })
            };

            var node1 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(0, 0))
            };
            var node2 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(100, 100))
            };

            network.Branches.Add(branch1);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            branch1.Source = node1;
            branch1.Target = node2;

            return network;
        }

        /// <summary>
        /// Creates a simple test network of 1 branch and 2 nodes. The branch has '3' parts, in the center of
        /// the first and last is a cross section.
        /// n
        /// /
        /// /
        /// cs
        /// /
        /// -------/
        /// /
        /// cs
        /// /
        /// n
        /// </summary>
        /// <returns></returns>
        private static IHydroNetwork CreateSegmentTestNetwork()
        {
            var network = new HydroNetwork();
            var branch1 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 100)
                })
            };

            var node1 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(0, 0))
            };
            var node2 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(100, 0))
            };

            network.Branches.Add(branch1);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            branch1.Source = node1;
            branch1.Target = node2;

            return network;
        }
    }
}