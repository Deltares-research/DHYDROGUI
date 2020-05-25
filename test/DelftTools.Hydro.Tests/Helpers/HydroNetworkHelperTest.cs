using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Actions;
using NetTopologySuite.Extensions.Coverages;
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
        public void GenerateCalculationPointsShouldWorkWellWithFixedPointsTools8709()
        {
            IHydroNetwork network = CreateTestNetwork();

            var computationalGrid = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };
            HydroNetworkHelper.GenerateDiscretization(computationalGrid, false, false, 1.0, false, 1.0, true, false, false, 10.0);

            INetworkLocation sampleLocation = computationalGrid.Locations.Values[1];
            int countBefore = computationalGrid.Locations.Values.Count;

            // set one point to fixed
            computationalGrid[sampleLocation] = 1.0;

            // regenerate grid
            HydroNetworkHelper.GenerateDiscretization(computationalGrid, true, false, 1.0, false, 1.0, true, false, false, 10.0);
            int countAfter = computationalGrid.Locations.Values.Count;

            Assert.AreEqual(countBefore, countAfter);
            Assert.AreEqual(1.0, computationalGrid[sampleLocation]);
        }

        [Test]
        public void GenerateCalculationPointsShouldWorkWellWithFixedPointsAtBeginOfBranchTools8709()
        {
            IHydroNetwork network = CreateTestNetwork();

            var computationalGrid = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };
            HydroNetworkHelper.GenerateDiscretization(computationalGrid, false, false, 1.0, false, 1.0, true, false, false, 10.0);

            INetworkLocation sampleLocation = computationalGrid.Locations.Values[0];
            int countBefore = computationalGrid.Locations.Values.Count;

            // set one point to fixed
            computationalGrid[sampleLocation] = 1.0;

            // regenerate grid
            HydroNetworkHelper.GenerateDiscretization(computationalGrid, true, false, 1.0, false, 1.0, true, false, false, 10.0);
            int countAfter = computationalGrid.Locations.Values.Count;

            Assert.AreEqual(countBefore, countAfter);
            Assert.AreEqual(1.0, computationalGrid[sampleLocation]);
        }

        [Test]
        public void GenerateCalculationPointsOnCrossSectionsSkipsIfAlsoStructurePresent()
        {
            IHydroNetwork network = CreateTestNetwork();

            ICrossSection cs1 = network.CrossSections.First();

            var branch = cs1.Branch as IChannel;

            var weir = new Weir();
            NetworkHelper.AddBranchFeatureToBranch(weir, branch, cs1.Chainage);

            IDiscretization computationalGrid = new Discretization
            {
                Network = network,
                SegmentGenerationMethod =
                    SegmentGenerationMethod.SegmentBetweenLocations
            };
            HydroNetworkHelper.GenerateDiscretization(computationalGrid, false, false, 1.0, false, 1.0, true, false, false, 0.0);

            Assert.AreEqual(
                new INetworkLocation[]
                {
                    new NetworkLocation(branch, 0),
                    new NetworkLocation(branch, 115),
                    new NetworkLocation(branch, branch.Length)
                }, computationalGrid.Locations.Values);
        }
        
        [Test]
        public void CreateNetworkCoverageSegments()
        {
            IHydroNetwork network = CreateTestNetwork();

            INetworkCoverage networkCoverage = new NetworkCoverage
            {
                Network = network,
                SegmentGenerationMethod =
                    SegmentGenerationMethod.SegmentBetweenLocations
            };
            IChannel branch1 = network.Channels.First();
            double length = branch1.Geometry.Length;
            HydroNetworkHelper.GenerateDiscretization(networkCoverage, branch1, new[]
            {
                0.0,
                length / 3,
                (2 * length) / 3,
                length
            });

            Assert.AreEqual(4, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(3, networkCoverage.Segments.Values.Count);

            Assert.AreEqual(0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length / 3, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual((2 * length) / 3, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length, networkCoverage.Locations.Values[3].Chainage, BranchFeature.Epsilon);

            Assert.AreEqual(0, networkCoverage.Segments.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length / 3, networkCoverage.Segments.Values[0].EndChainage, BranchFeature.Epsilon);
            Assert.AreEqual(length / 3, networkCoverage.Segments.Values[0].Length, BranchFeature.Epsilon);

            Assert.AreEqual(length / 3, networkCoverage.Segments.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual((2 * length) / 3, networkCoverage.Segments.Values[1].EndChainage, BranchFeature.Epsilon);

            Assert.AreEqual((2 * length) / 3, networkCoverage.Segments.Values[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length, networkCoverage.Segments.Values[2].EndChainage, BranchFeature.Epsilon);

            Assert.AreEqual(length / 3, networkCoverage.Segments.Values[0].Length, BranchFeature.Epsilon);
            Assert.AreEqual(length / 3, networkCoverage.Segments.Values[1].Length, BranchFeature.Epsilon);
            Assert.AreEqual(length / 3, networkCoverage.Segments.Values[2].Length, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegementsAndIgnoreFor1Channel()
        {
            var network = new HydroNetwork();
            var channel1 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(100, 0)
                })
            };
            var channel2 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(100, 0),
                    new Coordinate(200, 0)
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
            var node3 = new HydroNode
            {
                Network = network,
                Geometry = new Point(new Coordinate(200, 0))
            };

            network.Branches.Add(channel1);
            network.Branches.Add(channel2);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            channel1.Source = node1;
            channel1.Target = node2;
            channel2.Source = node2;
            channel2.Target = node3;

            var discretization = new Discretization {Network = network};

            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false, true, 20.0, null);
            // 6 + 6
            Assert.AreEqual(12, discretization.Locations.Values.Count);
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                                                      true, 10.0, new List<IChannel> {channel2});
            // 11 + 6
            Assert.AreEqual(17, discretization.Locations.Values.Count);
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                                                      true, 10.0, new List<IChannel> {channel1});
            // 11 + 11
            Assert.AreEqual(22, discretization.Locations.Values.Count);
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
        public void CreateSegments1Structure()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 10);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      0,               // minimumDistance
                                                      true,            // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      false,           // gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength
            Assert.AreEqual(4, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(9.5, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(10.5, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[3].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsMultipleStructures()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 20);
            AddTestStructureAt(network, branch1, 40);
            AddTestStructureAt(network, branch1, 60);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      0,               // minimumDistance
                                                      true,            // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      false,           // gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength
            Assert.AreEqual(8, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(19.5, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(20.5, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(39.5, networkCoverage.Locations.Values[3].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(40.5, networkCoverage.Locations.Values[4].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(59.5, networkCoverage.Locations.Values[5].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(60.5, networkCoverage.Locations.Values[6].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[7].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegments1StructureAtMinimumBeginBranch()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 0.4);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      0.5,             // minimumDistance
                                                      true,            // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      false,           // gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            // structure at less than minimumdistance; expect 1 point left out
            // [----------------------
            //        0.4
            // x                   x ----------------------------- x
            // 0                  0.9                             100
            Assert.AreEqual(3, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(0.9, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegments1StructureAtNearMinimumBeginBranch()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 0.8);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      0.5,             // minimumDistance
                                                      true,            // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      false,           // gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            // structure at near minimumdistance; expect point centered at 0.8 - 0.5 = 0.3 not created
            // [----------------------
            //                0.8
            // x       x             x ----------------------------- x
            // 0    (0.3)           1.3                             100
            //        ^

            Assert.AreEqual(3, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.3, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegments2StructureAtNearMinimumBeginBranch()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 0.8);
            AddTestStructureAt(network, branch1, 1.2);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      0.001,           // minimumDistance
                                                      true,            // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      false,           // gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            // structure at near minimumdistance; expect 1 point centered at first segment
            // [----------------------
            //                0.8   1.2
            // x       x          x             x ------------------ x
            // 0      0.3        1.0           1.7                  100
            //         ^

            Assert.AreEqual(5, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(0.3, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.0, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.7, networkCoverage.Locations.Values[3].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[4].Chainage, BranchFeature.Epsilon);

            // repeat with minimumDistance set to 0.5
            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      0.5,             // minimumDistance
                                                      true,            // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      false,           // gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength
            // expect gridpoints at 0.3 eliminated
            Assert.AreEqual(4, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.7, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[3].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegments1StructureAtMinimumEndBranch()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 99.6);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      0.5,             // minimumDistance
                                                      true,            // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      false,           // gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            // structure at less than minimumdistance; expect 1 point left out
            // [-----------------------------------------------------]
            //                                               99.6
            // x-------------------------------------------x-----(x)--- x
            // 0                                          99.1  (99.8) 100
            Assert.AreEqual(3, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(99.1, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegments2StructureAtNearMinimumEndBranch()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 99.2);
            AddTestStructureAt(network, branch1, 98.8);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      0.5,             // minimumDistance
                                                      true,            // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      false,           // gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            // structure at near minimumdistance; expect 1 point centered at first segment
            // structure at less than minimumdistance; expect 1 point left out
            // [-----------------------------------------------------------]
            //                                             98.8   99.2
            // x----------------------------------------x-------x------x---x
            // 0                                      98.3     99   (99.6) 100

            Assert.AreEqual(4, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(98.3, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(99.0, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
            //Assert.AreEqual(99.6, networkCoverage.Locations.Values[3].Offset, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[3].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsCrossSection()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            AddTestCrossSectionAt(branch1, 50.0);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      0.5,             // minimumDistance
                                                      false,           // gridAtStructure
                                                      0.5,             // structureDistance
                                                      true,            // gridAtCrossSection
                                                      false,           // gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            Assert.AreEqual(3, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(50.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsFixedLocations()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();
            var discretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(discretization, // networkCoverage
                                                      branch1,        // branch
                                                      0.5,            // minimumDistance
                                                      false,          // gridAtStructure
                                                      0.5,            // structureDistance
                                                      false,          // gridAtCrossSection
                                                      false,          //gridAtLateralSource
                                                      true,           // gridAtFixedLength
                                                      10);            // fixedLength
            Assert.AreEqual(11, discretization.Locations.Values.Count);
            Assert.AreEqual(0.0, discretization.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(50.0, discretization.Locations.Values[5].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, discretization.Locations.Values[10].Chainage, BranchFeature.Epsilon);

            INetworkLocation networkLocation = discretization.Locations.Values[7];
            Assert.AreEqual(70.0, networkLocation.Chainage, BranchFeature.Epsilon);
            discretization.ToggleFixedPoint(networkLocation);
            //DiscretizationHelper.SetUserDefinedGridPoint(networkLocation, true);

            HydroNetworkHelper.GenerateDiscretization(discretization, // networkCoverage
                                                      branch1,        // branch
                                                      0.5,            // minimumDistance
                                                      false,          // gridAtStructure
                                                      0.5,            // structureDistance
                                                      false,          // gridAtCrossSection
                                                      false,          //gridAtLateralSource
                                                      true,           // gridAtFixedLength
                                                      40);            // fixedLength
            // expect values at 
            // - 0 and 100 start and end
            // - 70 for fixed location
            // - none between 70 and 100
            // - (0 - 70) > 40, divide in equal parts -> 35
            Assert.AreEqual(4, discretization.Locations.Values.Count);
            Assert.AreEqual(0.0, discretization.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(35.0, discretization.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(70.0, discretization.Locations.Values[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, discretization.Locations.Values[3].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsForChannelWithCustomLength()
        {
            IHydroNetwork network = CreateTestNetwork();
            var firstBranch = (IChannel) network.Branches[0];

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      firstBranch,     // branch
                                                      5.0,             // minimumDistance
                                                      false,           // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      false,           //gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            Assert.AreEqual(2, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(1, networkCoverage.Segments.Values.Count);
            firstBranch.Length = firstBranch.Length * 2;
            firstBranch.IsLengthCustom = true;
            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      firstBranch,     // branch
                                                      5.0,             // minimumDistance
                                                      false,           // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      false,           //gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            Assert.AreEqual(2, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(1, networkCoverage.Segments.Values.Count);
        }

        [Test]
        public void CreateSegmentsCrossSectionAndMinimumDistance()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            AddTestCrossSectionAt(branch1, 1.0);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      5.0,             // minimumDistance
                                                      false,           // gridAtStructure
                                                      0.5,             // structureDistance
                                                      true,            // gridAtCrossSection
                                                      false,           //gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            Assert.AreEqual(2, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsCrossSectionAndMinimumDistanceNearEnd()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            AddTestCrossSectionAt(branch1, 99.0);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      5.0,             // minimumDistance
                                                      false,           // gridAtStructure
                                                      0.5,             // structureDistance
                                                      true,            // gridAtCrossSection
                                                      false,           //gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            Assert.AreEqual(2, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsMultipleCrossSection()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            // add multiple cross sections and generate calculation points at the cross section locations
            // Grid cells too smal should not be generated.
            AddTestCrossSectionAt(branch1, 10.0);
            AddTestCrossSectionAt(branch1, 20.0);
            AddTestCrossSectionAt(branch1, 30.0);
            AddTestCrossSectionAt(branch1, 40.0);
            AddTestCrossSectionAt(branch1, 50.0);
            AddTestCrossSectionAt(branch1, 60.0);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      5.0,             // minimumDistance
                                                      false,           // gridAtStructure
                                                      0.5,             // structureDistance
                                                      true,            // gridAtCrossSection
                                                      false,           //gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            Assert.AreEqual(8, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(10.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(20.0, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(30.0, networkCoverage.Locations.Values[3].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(40.0, networkCoverage.Locations.Values[4].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(50.0, networkCoverage.Locations.Values[5].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(60.0, networkCoverage.Locations.Values[6].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[7].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsMultipleCrossSectionAndMinimumDistance()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            // add multiple cross sections and generate calculation points at the cross section locations
            // Grid cells too smal should not be generated.
            AddTestCrossSectionAt(branch1, 1.0);
            AddTestCrossSectionAt(branch1, 2.0);
            AddTestCrossSectionAt(branch1, 3.0);
            AddTestCrossSectionAt(branch1, 4.0);
            AddTestCrossSectionAt(branch1, 5.0);
            AddTestCrossSectionAt(branch1, 6.0);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      5.0,             // minimumDistance
                                                      false,           // gridAtStructure
                                                      0.5,             // structureDistance
                                                      true,            // gridAtCrossSection
                                                      false,           //gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            Assert.AreEqual(3, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(5.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsMultipleLateralsAndMinimumDistance()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            // add multiple cross sections and generate calculation points at the cross section locations
            // Grid cells too smal should not be generated.
            AddLateralAt(branch1, 1.0);
            AddLateralAt(branch1, 2.0);
            AddLateralAt(branch1, 3.0);
            AddLateralAt(branch1, 4.0);
            AddLateralAt(branch1, 5.0);
            AddLateralAt(branch1, 6.0);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      5.0,             // minimumDistance
                                                      false,           // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      true,            //gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            Assert.AreEqual(3, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(5.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsMultipleLaterals()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            // add multiple cross sections and generate calculation points at the cross section locations
            // Grid cells too smal should not be generated.
            AddLateralAt(branch1, 1.0);
            AddLateralAt(branch1, 2.0);
            AddLateralAt(branch1, 3.0);
            AddLateralAt(branch1, 4.0);
            AddLateralAt(branch1, 5.0);
            AddLateralAt(branch1, 6.0);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      1.0,             // minimumDistance
                                                      false,           // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      true,            //gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            Assert.AreEqual(8, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(2.0, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void DoNotCreateSegmentsMultipleLateralsAndMinimumDistance()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            // add multiple cross sections and generate calculation points at the cross section locations
            // Grid cells too smal should not be generated.
            AddLateralAt(branch1, 1.0);
            AddLateralAt(branch1, 2.0);
            AddLateralAt(branch1, 3.0);
            AddLateralAt(branch1, 4.0);
            AddLateralAt(branch1, 5.0);
            AddLateralAt(branch1, 6.0);

            var networkCoverage = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,         // branch
                                                      5.0,             // minimumDistance
                                                      false,           // gridAtStructure
                                                      0.5,             // structureDistance
                                                      false,           // gridAtCrossSection
                                                      false,           //gridAtLateralSource
                                                      false,           // gridAtFixedLength
                                                      -1);             // fixedLength

            Assert.AreEqual(2, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsMultipleCrossSectionsAndFixedPoint()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            var discretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(discretization, // networkCoverage
                                                      branch1,        // branch
                                                      5.0,            // minimumDistance
                                                      false,          // gridAtStructure
                                                      0.5,            // structureDistance
                                                      false,          // gridAtCrossSection
                                                      false,          //gridAtLateralSource
                                                      true,           // gridAtFixedLength
                                                      2);             // fixedLength
            Assert.AreEqual(51, discretization.Locations.Values.Count);

            INetworkLocation networkLocation = discretization.Locations.Values.Where(nl => nl.Chainage == 8).First();
            discretization.ToggleFixedPoint(networkLocation);
            networkLocation = discretization.Locations.Values.Where(nl => nl.Chainage == 32).First();
            discretization.ToggleFixedPoint(networkLocation);

            AddTestCrossSectionAt(branch1, 10.0);
            AddTestCrossSectionAt(branch1, 20.0);
            AddTestCrossSectionAt(branch1, 30.0);

            HydroNetworkHelper.GenerateDiscretization(discretization, // networkCoverage
                                                      branch1,        // branch
                                                      5.0,            // minimumDistance
                                                      false,          // gridAtStructure
                                                      0.5,            // structureDistance
                                                      true,           // gridAtCrossSection
                                                      false,          //gridAtLateralSource
                                                      false,          // gridAtFixedLength
                                                      -1);            // fixedLength
            // expect gridpoints at:
            // begin and end 0 and 100
            // fixed locations 8 and 32.
            // 20 for the cross section, 10 and 30 should not be generated due to existing 
            // fixed points and minimium distance 0f 5.
            Assert.AreEqual(5, discretization.Locations.Values.Count);
            Assert.AreEqual(0.0, discretization.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(8.0, discretization.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(20.0, discretization.Locations.Values[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(32.0, discretization.Locations.Values[3].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, discretization.Locations.Values[4].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsMultipleStructuresAndFixedPoint()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            IChannel branch1 = network.Channels.First();

            var discretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(discretization, // networkCoverage
                                                      branch1,        // branch
                                                      5.0,            // minimumDistance
                                                      false,          // gridAtStructure
                                                      0.5,            // structureDistance
                                                      false,          // gridAtCrossSection
                                                      false,          //gridAtLateralSource
                                                      true,           // gridAtFixedLength
                                                      2);             // fixedLength
            Assert.AreEqual(51, discretization.Locations.Values.Count);

            INetworkLocation networkLocation = discretization.Locations.Values.Where(nl => nl.Chainage == 8).First();
            //DiscretizationHelper.SetUserDefinedGridPoint(networkLocation, true);
            discretization.ToggleFixedPoint(networkLocation);
            networkLocation = discretization.Locations.Values.Where(nl => nl.Chainage == 32).First();
            discretization.ToggleFixedPoint(networkLocation);
            //DiscretizationHelper.SetUserDefinedGridPoint(networkLocation, true);

            AddTestStructureAt(network, branch1, 10.0);
            AddTestStructureAt(network, branch1, 20.0);
            AddTestStructureAt(network, branch1, 30.0);

            HydroNetworkHelper.GenerateDiscretization(discretization, // networkCoverage
                                                      branch1,        // branch
                                                      6.0,            // minimumDistance
                                                      true,           // gridAtStructure
                                                      4.0,            // structureDistance
                                                      false,          // gridAtCrossSection
                                                      false,          //gridAtLateralSource
                                                      false,          // gridAtFixedLength
                                                      -1);            // fixedLength
            // expect gridpoints with no minimumDistance
            // 0  8 (6 14) (16 24) (26 34) 32 100 
            // 0  6 8 14 16 24 26 32 34 100
            //        10   20   30                 // structure locations
            // 0    8   14    24    32      100    // result 

            // fixed locations 8 and 32.
            // first structure (6) and 14
            // second structure 16 and 24; 16 will be merged into 14 -> 15
            // third structure 26 and (34); 26 will be merged into 24 -> 25
            // fixed points and minimium distance 0f 5.

            Assert.AreEqual(6, discretization.Locations.Values.Count);
            Assert.AreEqual(0.0, discretization.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(8.0, discretization.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(15.0, discretization.Locations.Values[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(25.0, discretization.Locations.Values[3].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(32.0, discretization.Locations.Values[4].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, discretization.Locations.Values[5].Chainage, BranchFeature.Epsilon);
        }

        /// <summary>
        /// Test for Jira Issue 2213. Grid points at channel Ovk98 are to close connected.
        /// grid points generated at:
        /// (OVK98, 0)
        /// (OVK98, 0.9999)
        /// (OVK98, 227.6)
        /// (OVK98, 229.6)
        /// (OVK98, 241.4)
        /// (OVK98, 241.5)
        /// (OVK98, 243.4)
        /// (OVK98, 243.4)
        /// (OVK98, 595)
        /// (OVK98, 597)
        /// (OVK98, 730.2)
        /// (OVK98, 732.2)
        /// (OVK98, 1253)
        /// (OVK98, 1255)
        /// (OVK98, 1260.51113164371)
        /// (OVK98, 1261.51114183887)
        /// settings at structure and crosssection
        /// 1m before and after structure
        /// minimum 0.5 m.
        /// thus point at 241.4, 241.5 and 243.4, 243.4 should either be merged or eliminated.
        /// </summary>
        [Test]
        public void JiraTools2213Ovk98()
        {
            var network = new HydroNetwork();
            var channel = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1262.0, 0)
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
                Geometry = new Point(new Coordinate(1262.0, 0))
            };
            network.Branches.Add(channel);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            channel.Source = node1;
            channel.Target = node2;

            AddCrossSection(channel, 1.0);
            AddCrossSection(channel, 241.47);
            AddCrossSection(channel, 243.44);
            AddCrossSection(channel, 1260.51);
            AddTestStructureAt(network, channel, 228.61);
            AddTestStructureAt(network, channel, 242.42);
            AddTestStructureAt(network, channel, 596.01);
            AddTestStructureAt(network, channel, 731.25);
            AddTestStructureAt(network, channel, 1253.95);

            var discretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };

            HydroNetworkHelper.GenerateDiscretization(discretization, // networkCoverage
                                                      channel,        // branch
                                                      0.5,            // minimumDistance
                                                      true,           // gridAtStructure
                                                      1.0,            // structureDistance
                                                      true,           // gridAtCrossSection
                                                      false,          //gridAtLateralSource
                                                      false,          // gridAtFixedLength
                                                      -1);            // fixedLength

            // expected at:
            //  0: 0 = start channel
            //  1: 1 = cross section
            //  2: 227.61 = 1 m before struct
            //  3: 229.61 = 1 m after struct
            //  4: 241.42 = 1 m before struct
            //  5: 243.42 = 1 m after struct
            //  6: 595.01 = 1 m before struct
            //  7: 597.01 = 1 m after struct
            //  8: 730.25 = 1 m before struct
            //  9: 732.25 = 1 m after struct
            // 10: 1252.95 = 1 m before struct
            // 11: 1254.95 = 1 m after struct
            // 12: 1260.51 = cross section
            // 13: 1262 = length channel
            // = skipped cross sections at 241.47 and 243.44

            IMultiDimensionalArray<INetworkLocation> gridPoints = discretization.Locations.Values;
            Assert.AreEqual(14, gridPoints.Count);
            Assert.AreEqual(0.0, gridPoints[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.0, gridPoints[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(227.61, gridPoints[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(229.61, gridPoints[3].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(241.42, gridPoints[4].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(243.42, gridPoints[5].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(595.01, gridPoints[6].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(597.01, gridPoints[7].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(730.25, gridPoints[8].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(732.25, gridPoints[9].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1252.95, gridPoints[10].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1254.95, gridPoints[11].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1260.51, gridPoints[12].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1262, gridPoints[13].Chainage, BranchFeature.Epsilon);
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

        [Test]
        public void RemoveStructureOnDisconnectedStructureShouldNotCrashTools9784()
        {
            var weir = new Weir();
            Assert.DoesNotThrow(() => HydroNetworkHelper.RemoveStructure(weir));
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

            var crossSection1 = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(15, 20),
                    new Coordinate(16, 20)
                })
            };
            double offset1 = Math.Sqrt((15 * 15) + (20 * 20));
            var crossSectionBranchFeature1 = new CrossSection(crossSection1) {Chainage = offset1};

            var crossSection2 = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(85, 70),
                    new Coordinate(86, 70)
                })
            };
            double offset2 = Math.Sqrt((30 * 30) + (40 * 40)) + 40 + Math.Sqrt((15 * 15) + (20 * 20));
            var crossSectionBranchFeature2 = new CrossSection(crossSection2) {Chainage = offset2};

            branch1.Source = node1;
            branch1.Target = node2;
            NetworkHelper.AddBranchFeatureToBranch(crossSectionBranchFeature1, branch1, crossSectionBranchFeature1.Chainage);
            NetworkHelper.AddBranchFeatureToBranch(crossSectionBranchFeature2, branch1, crossSectionBranchFeature2.Chainage);

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

        private static void AddTestStructureAt(IHydroNetwork network, IChannel branch, double offset)
        {
            IWeir weir = new Weir {Chainage = offset};
            var compositeBranchStructure = new CompositeBranchStructure
            {
                Network = network,
                Geometry = new Point(offset, 0),
                Chainage = offset
            };
            compositeBranchStructure.Structures.Add(weir);
            branch.BranchFeatures.Add(compositeBranchStructure);
        }

        private static void AddTestCrossSectionAt(IChannel branch, double offset)
        {
            var crossSectionXyz = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(offset - 1, 0),
                    new Coordinate(offset + 1, 0)
                })
            };
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, crossSectionXyz, offset);
        }

        private static void AddLateralAt(IChannel branch, double offset)
        {
            NetworkHelper.AddBranchFeatureToBranch(new LateralSource(), branch, offset);
        }

        private static void AddCrossSection(Channel branch, double chainage)
        {
            var crossSection = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(chainage, 0),
                    new Coordinate(chainage + 1, 0)
                })
            };
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, crossSection, chainage);
        }
    }
}