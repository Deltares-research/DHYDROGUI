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
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Actions;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Helpers
{
    [TestFixture]
    public class HydroNetworkHelperTest
    {
        [Test]
        public void TestAddStructureToExistingCompositeStructureOrToANewOne_GeneratesUniqueNamesForCompositeBranchStructures()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var branch = network.Branches.First();
            Assert.NotNull(branch);

            var weir1 = new Weir("weir1") { Chainage = branch.Length / 3 };
            var weir2 = new Weir("weir2") { Chainage = branch.Length / 3 };
            var weir3 = new Weir("weir3") { Chainage = branch.Length * 2 / 3 };

            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir1, branch);
            Assert.AreEqual(1, network.CompositeBranchStructures.Count());

            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir2, branch);
            Assert.AreEqual(1, network.CompositeBranchStructures.Count());

            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir3, branch);
            Assert.AreEqual(2, network.CompositeBranchStructures.Count());

            Assert.IsTrue(network.CompositeBranchStructures.Select(cbs => cbs.Name).HasUniqueValues());
        }

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
        [Category("Quarantine")]
        public void GenerateCalculationPointsShouldWorkWellWithFixedPointsTools8709()
        {
            var network = CreateTestNetwork();

            var computationalGrid = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };
            HydroNetworkHelper.GenerateDiscretization(computationalGrid, false, false, 1.0, false, 1.0, true, false, false, 10.0);
            
            var sampleLocation = computationalGrid.Locations.Values[1];
            var countBefore = computationalGrid.Locations.Values.Count;
            
            // set one point to fixed
            computationalGrid[sampleLocation] = 1.0;

            // regenerate grid
            HydroNetworkHelper.GenerateDiscretization(computationalGrid, true, false, 1.0, false, 1.0, true, false, false, 10.0);
            var countAfter = computationalGrid.Locations.Values.Count;
            
            Assert.AreEqual(countBefore, countAfter);
            Assert.AreEqual(1.0, computationalGrid[sampleLocation]);
        }

        [Test]
        [Category("Quarantine")]
        public void GenerateCalculationPointsShouldWorkWellWithFixedPointsAtBeginOfBranchTools8709()
        {
            var network = CreateTestNetwork();

            var computationalGrid = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
            };
            HydroNetworkHelper.GenerateDiscretization(computationalGrid, false, false, 1.0, false, 1.0, true, false, false, 10.0);

            var sampleLocation = computationalGrid.Locations.Values[0];
            var countBefore = computationalGrid.Locations.Values.Count;

            // set one point to fixed
            computationalGrid[sampleLocation] = 1.0;

            // regenerate grid
            HydroNetworkHelper.GenerateDiscretization(computationalGrid, true, false, 1.0, false, 1.0, true, false, false, 10.0);
            var countAfter = computationalGrid.Locations.Values.Count;

            Assert.AreEqual(countBefore, countAfter);
            Assert.AreEqual(1.0, computationalGrid[sampleLocation]);
        }
        [Test]
        public void GenerateCalculationPointsOnCrossSectionsSkipsIfAlsoStructurePresent()
        {
            var network = CreateTestNetwork();

            var cs1 = network.CrossSections.First();

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
                        new NetworkLocation(branch, 0), new NetworkLocation(branch, 115),
                        new NetworkLocation(branch, branch.Length)
                    }, computationalGrid.Locations.Values);
        }

        /// <summary>
        /// Creates a simple test network of 1 branch and 2 nodes. The branch has '3' parts, in the center of
        /// the first and last is a cross section.
        ///                 n
        ///                /
        ///               /
        ///              cs
        ///             /
        ///     -------/
        ///    /
        ///   cs
        ///  /
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
                                                                    new Coordinate(0, 0), new Coordinate(30, 40),
                                                                    new Coordinate(70, 40), new Coordinate(100, 100)
                                                                })
                              };

            var node1 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(0, 0)) };
            var node2 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(100, 100)) };

            network.Branches.Add(branch1);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var crossSection1 = new CrossSectionDefinitionXYZ { Geometry = new LineString(new[] { new Coordinate(15, 20), new Coordinate(16, 20) }) };
            double offset1 = Math.Sqrt(15 * 15 + 20 * 20);
            var crossSectionBranchFeature1 = new CrossSection(crossSection1) {Chainage = offset1};

            var crossSection2 = new CrossSectionDefinitionXYZ { Geometry = new LineString(new[] { new Coordinate(85, 70), new Coordinate(86, 70) }) };
            double offset2 = Math.Sqrt(30 * 30 + 40 * 40) + 40 + Math.Sqrt(15 * 15 + 20 * 20);
            var crossSectionBranchFeature2 = new CrossSection(crossSection2) { Chainage = offset2 };
            
            branch1.Source = node1;
            branch1.Target = node2;
            NetworkHelper.AddBranchFeatureToBranch(crossSectionBranchFeature1, branch1, crossSectionBranchFeature1.Chainage);
            NetworkHelper.AddBranchFeatureToBranch(crossSectionBranchFeature2, branch1, crossSectionBranchFeature2.Chainage);

            return network;
        }

        /// <summary>
        /// Creates the testnetwork, inserts a node and test if it added correctly.
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void SplitBranchIn2()
        {
            IHydroNetwork network = CreateTestNetwork();
            var branch1 = network.Channels.First();
            branch1.Name = "branch1";
            branch1.LongName = "maas";
            double length = branch1.Geometry.Length;

            int nodesCount = network.Nodes.Count;
            IHydroNode hydroNode = HydroNetworkHelper.SplitChannelAtNode(branch1, length / 2);

            Assert.IsNull(network.CurrentEditAction);
            Assert.AreEqual(nodesCount + 1, network.Nodes.Count);
            Assert.AreNotEqual(-1, network.Nodes.IndexOf(hydroNode));
            
            Assert.AreEqual("branch1_A",branch1.Name);
            Assert.AreEqual("maas_A", branch1.LongName);
            
            var branch2 = network.Channels.ElementAt(1);
            Assert.AreEqual("branch1_B", branch2.Name);
            Assert.AreEqual("maas_B", branch2.LongName);

            Assert.AreEqual(0, hydroNode.Geometry.Coordinate.Z);
        }

        [Test]
        public void SplitBranchAndRemoveNode()
        {
            var network = CreateTestNetwork();
            var leftBranch = network.Channels.First();
            var startNode = leftBranch.Source;
            var endNode = leftBranch.Target;

            Assert.IsFalse(startNode.IsConnectedToMultipleBranches);
            Assert.IsFalse(endNode.IsConnectedToMultipleBranches);

            var insertedNode = HydroNetworkHelper.SplitChannelAtNode(leftBranch, leftBranch.Geometry.Length / 2);

            Assert.IsFalse(startNode.IsConnectedToMultipleBranches);
            Assert.IsTrue(insertedNode.IsConnectedToMultipleBranches);
            Assert.IsFalse(endNode.IsConnectedToMultipleBranches);

            NetworkHelper.MergeNodeBranches(insertedNode, network);

            Assert.IsNull(network.CurrentEditAction);
            Assert.IsFalse(startNode.IsConnectedToMultipleBranches);
            Assert.IsFalse(endNode.IsConnectedToMultipleBranches);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SplitBranchDoesNotCreateANaNInBranchGeometry()
        {
            //relates to issue 2477
            IHydroNetwork network = CreateTestNetwork();
            var branch1 = network.Channels.First();
            double length = branch1.Geometry.Length;

            int nodesCount = network.Nodes.Count;
            IHydroNode hydroNode = HydroNetworkHelper.SplitChannelAtNode(branch1, length / 2);

            Assert.IsNull(network.CurrentEditAction);
            Assert.AreEqual(nodesCount + 1, network.Nodes.Count);
            Assert.AreNotEqual(-1, network.Nodes.IndexOf(hydroNode));

            //the network should not contain branches with coordinates as NaN (messes up wkbwriter )
            Assert.IsFalse(network.Branches.Any(b => b.Geometry.Coordinates.Any(c => double.IsNaN(c.Z))));
        }

        /// <summary>
        /// Creates the testnetwork, adds a route and split the branch.
        /// TOOLS-1199 collection changed events caused recreating routing network triggered
        /// by changing of geometry of branch (removeunused nodes) and temporarily invalid network.
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void SplitBranchWithRouteIn2()
        {
            IHydroNetwork network = CreateTestNetwork();
            var branch1 = network.Channels.First();
            double length = branch1.Geometry.Length;

            NetworkCoverage route = new Route
                                        {
                                            Network = network,
                                            SegmentGenerationMethod = SegmentGenerationMethod.RouteBetweenLocations
                                        };
            route.Locations.Values.Add(new NetworkLocation(branch1, length / 12));
            route.Locations.Values.Add(new NetworkLocation(branch1, length / 8));

            int nodesCount = network.Nodes.Count;
            IHydroNode hydroNode = HydroNetworkHelper.SplitChannelAtNode(branch1, length / 2);
            Assert.IsNull(network.CurrentEditAction);

            Assert.AreEqual(nodesCount + 1, network.Nodes.Count);
            Assert.AreNotEqual(-1, network.Nodes.IndexOf(hydroNode));

            Assert.AreEqual(2, route.Locations.Values.Count);
            Assert.AreEqual(1, route.Segments.Values.Count);
        }
        
        [Test]
        public void GivenRoute_WhenOnExecute_ThenRemoveRoute()
        {
            // Arrange
            IHydroNetwork hydroNetwork = CreateTestNetwork();
            Route route = AddNewRouteToHydroNetwork(hydroNetwork);
            Assert.That(hydroNetwork.Routes.Contains(route), Is.True);

            // Act
            HydroNetworkHelper.RemoveRoute(route);

            //Assert
            Assert.That(hydroNetwork.Routes.Contains(route), Is.False);
        }

        [Test]
        public void GivenTwoRoutes_WhenOnExecute_ThenRemoveOneRoute()
        {
            // Arrange
            IHydroNetwork hydroNetwork = CreateTestNetwork();
            Route route = AddNewRouteToHydroNetwork(hydroNetwork);
            Route route2 = AddNewRouteToHydroNetwork(hydroNetwork);
            Assert.That(hydroNetwork.Routes.Contains(route), Is.True);
            Assert.That(hydroNetwork.Routes.Contains(route2), Is.True);

            // Act
            HydroNetworkHelper.RemoveRoute(route);

            //Assert
            Assert.That(hydroNetwork.Routes.Contains(route), Is.False);
            Assert.That(hydroNetwork.Routes.Contains(route2), Is.True);
        }
        
        private static Route AddNewRouteToHydroNetwork(IHydroNetwork hydroNetwork)
        {
            var route = new Route() { Network = hydroNetwork };
            hydroNetwork.Routes.Add(route);
            return route;
        }

        /// <summary>
        /// Split the test network in the center.
        /// </summary>
        [Test]
        public void SplitCustomLengthBranchWithCrossSections()
        {
            IHydroNetwork network = CreateTestNetwork();

            var branch1 = network.Channels.First();
            branch1.IsLengthCustom = true;
            double length = branch1.Geometry.Length;

            HydroNetworkHelper.SplitChannelAtNode(branch1, new Coordinate(50, 40));

            Assert.IsNull(network.CurrentEditAction);

            double offset1 = Math.Sqrt(15 * 15 + 20 * 20);
            double offset2 = Math.Sqrt(30 * 30 + 40 * 40) + 40 + Math.Sqrt(15 * 15 + 20 * 20);
            double length1 = Math.Sqrt(30 * 30 + 40 * 40) + 20;
            double length2 = 20 + Math.Sqrt(30 * 30 + 60 * 60);

            Assert.AreEqual(length, length1 + length2);

            Assert.AreEqual(2, network.Branches.Count);
            var branch2 = network.Channels.Skip(1).First();
            Assert.AreEqual(3, network.Nodes.Count);
            Assert.AreEqual(1, network.Nodes[0].OutgoingBranches.Count);
            Assert.AreEqual(1, network.Nodes[1].IncomingBranches.Count);
            Assert.AreEqual(1, network.Nodes[2].IncomingBranches.Count);
            Assert.AreEqual(1, network.Nodes[2].OutgoingBranches.Count);

            Assert.AreEqual(2, network.CrossSections.Count());
            Assert.AreEqual(1, branch1.CrossSections.Count());
            Assert.AreEqual(1, branch2.CrossSections.Count());
            Assert.AreEqual(offset1, branch1.CrossSections.First().Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length1, branch1.Geometry.Length);
            Assert.AreEqual(offset2 - length1, branch2.CrossSections.First().Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length2, branch2.Geometry.Length);
            Assert.AreEqual(branch1, branch1.CrossSections.First().Branch);
            Assert.AreEqual(branch2, branch2.CrossSections.First().Branch);
        }

        /// <summary>
        /// Split the test network in the center.
        /// </summary>
        [Test]
        public void SplitBranchWithCrossSections()
        {
            IHydroNetwork network = CreateTestNetwork();

            var branch1 = network.Channels.First();
            double length = branch1.Geometry.Length;

            HydroNetworkHelper.SplitChannelAtNode(branch1, new Coordinate(50, 40));

            Assert.IsNull(network.CurrentEditAction);

            double offset1 = Math.Sqrt(15 * 15 + 20 * 20);
            double offset2 = Math.Sqrt(30 * 30 + 40 * 40) + 40 + Math.Sqrt(15 * 15 + 20 * 20);
            double length1 = Math.Sqrt(30 * 30 + 40 * 40) + 20;
            double length2 = 20 + Math.Sqrt(30 * 30 + 60 * 60);

            Assert.AreEqual(length, length1 + length2);

            Assert.AreEqual(2, network.Branches.Count);
            var branch2 = network.Channels.Skip(1).First();
            Assert.AreEqual(3, network.Nodes.Count);
            Assert.AreEqual(1, network.Nodes[0].OutgoingBranches.Count);
            Assert.AreEqual(1, network.Nodes[1].IncomingBranches.Count);
            Assert.AreEqual(1, network.Nodes[2].IncomingBranches.Count);
            Assert.AreEqual(1, network.Nodes[2].OutgoingBranches.Count);

            Assert.AreEqual(2, network.CrossSections.Count());
            Assert.AreEqual(1, branch1.CrossSections.Count());
            Assert.AreEqual(1, branch2.CrossSections.Count());
            Assert.AreEqual(offset1, branch1.CrossSections.First().Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length1, branch1.Geometry.Length);
            Assert.AreEqual(offset2 - length1, branch2.CrossSections.First().Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length2, branch2.Geometry.Length);
            Assert.AreEqual(branch1, branch1.CrossSections.First().Branch);
            Assert.AreEqual(branch2, branch2.CrossSections.First().Branch);
        }

        /// <summary>
        /// split at begin or end of branch should not work
        /// split on chainage = branch.length or 0 returns null
        /// </summary>
        [Test]
        public void SplitBranchOnExistingNodeShouldNotWork()
        {
            IHydroNetwork network = CreateTestNetwork();
            var numberOfChannels = network.Channels.Count();
            var branch1 = network.Channels.First();
            double length = branch1.Geometry.Length;

            var result = HydroNetworkHelper.SplitChannelAtNode(branch1, length);
            Assert.IsNull(result);
            Assert.IsNull(network.CurrentEditAction);
            Assert.AreEqual(numberOfChannels, network.Channels.Count());
        }

        [Test]
        public void SplitBranchWithCustomLengthTools9057()
        {
            var network = new HydroNetwork();
            var node1 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(0, 0)) };
            var node2 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(100, 0)) };
            var branch = new Channel
                {
                    Source = node1,
                    Target = node2,
                    Geometry = new LineString(new[]
                        {
                            new Coordinate(0, 0), new Coordinate(100, 0)
                        })
                };
            
            network.Branches.Add(branch);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            branch.IsLengthCustom = true;
            branch.Length = 200.0;

            // split branch
            HydroNetworkHelper.SplitChannelAtNode(branch, new Coordinate(50, 0));

            // split branch closer to origin
            HydroNetworkHelper.SplitChannelAtNode(branch, new Coordinate(10, 0));

            // split again at same location
            HydroNetworkHelper.SplitChannelAtNode(branch, new Coordinate(10, 0));
        }

        /// <summary>
        /// Split branch and check order is equal for the resulting new branches
        /// </summary>
        [Test]
        public void AfterSplitTheOrderIsEqualForResultingChannels()
        {
            IHydroNetwork network = CreateTestNetwork();
            var channel = network.Channels.First();
            var name = channel.Name;
            channel.OrderNumber = 7;
            double length = channel.Geometry.Length / 2.0;

            var result = HydroNetworkHelper.SplitChannelAtNode(channel, length);
            Assert.IsNotNull(result);
            Assert.IsNull(network.CurrentEditAction);

            var newChannelA = network.Channels.First(c => c.Name == name + "_A");
            var newChannelB = network.Channels.First(c => c.Name == name + "_B");
            Assert.AreEqual(newChannelA.OrderNumber, newChannelB.OrderNumber);
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
            var branch1 = network.Channels.First();
            var length = branch1.Geometry.Length;
            HydroNetworkHelper.GenerateDiscretization(networkCoverage, branch1, new[] { 0.0, length / 3, 2 * length / 3, length });

            Assert.AreEqual(4, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(3, networkCoverage.Segments.Values.Count);

            Assert.AreEqual(0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length / 3, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(2 * length / 3, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length, networkCoverage.Locations.Values[3].Chainage, BranchFeature.Epsilon);

            Assert.AreEqual(0, networkCoverage.Segments.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length / 3, networkCoverage.Segments.Values[0].EndChainage, BranchFeature.Epsilon);
            Assert.AreEqual(length / 3, networkCoverage.Segments.Values[0].Length, BranchFeature.Epsilon);

            Assert.AreEqual(length / 3, networkCoverage.Segments.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(2 * length / 3, networkCoverage.Segments.Values[1].EndChainage, BranchFeature.Epsilon);

            Assert.AreEqual(2 * length / 3, networkCoverage.Segments.Values[2].Chainage, BranchFeature.Epsilon);
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
                Name = "Channel1", Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(100, 0) })
            };
            var channel2 = new Channel
            {
                Name = "Channel2", Geometry = new LineString(new[] { new Coordinate(100, 0), new Coordinate(200, 0) })
            };
            var node1 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(0, 0)) };
            var node2 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(100, 0)) };
            var node3 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(200, 0)) };

            network.Branches.Add(channel1);
            network.Branches.Add(channel2);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            channel1.Source = node1;
            channel1.Target = node2;
            channel2.Source = node2;
            channel2.Target = node3;


            var discretization = new Discretization
            {
                Network = network
            };

            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false, true, 20.0, null);
            // 6 + 6 - 1 double point 
            Assert.AreEqual(11, discretization.Locations.Values.Count);
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false, 
                                                      true, 10.0, new List<IChannel> { channel2 });
            // 11 + 6 - 1 double point 
            Assert.AreEqual(16, discretization.Locations.Values.Count);
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false, 
                                                      true, 10.0, new List<IChannel> { channel1 });
            // 11 + 11 - 1 double point 
            Assert.AreEqual(21, discretization.Locations.Values.Count);
        }

        /// <summary>
        /// Creates the testnetwork, adds 3 branch segments and splits the branch in 2.
        /// </summary>
        [Test]
        public void SplitBranchWithBranchSegments()
        {
            IHydroNetwork network = CreateTestNetwork();
            var branch1 = network.Channels.First();
            double length = branch1.Geometry.Length;
            // see also test GenerateDiscretization
            INetworkCoverage networkCoverage = new NetworkCoverage
                                                   {
                                                       Network = network,
                                                       SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                                                   };
            HydroNetworkHelper.GenerateDiscretization(networkCoverage, branch1, new[] { 0.0, length / 3, 2 * length / 3, length });

            HydroNetworkHelper.SplitChannelAtNode(branch1, length / 2);

            Assert.IsNull(network.CurrentEditAction);
            var branch2 = network.Channels.Skip(1).First();

            //4 segments are created...2 on branch 1 and 2 on branch 2
            Assert.AreEqual(4, networkCoverage.Segments.Values.Count);
            Assert.AreEqual(2, networkCoverage.Segments.Values.Where(s => s.Branch == branch1).Count());
            Assert.AreEqual(2, networkCoverage.Segments.Values.Where(s => s.Branch == branch2).Count());
        }

        /// <summary>
        /// Creates the testnetwork, splits the branch in 2 and merges them again.
        /// </summary>
        [Test]
        public void MergeBranchWithCrossSections()
        {
            var network = CreateTestNetwork();
            var branch1 = network.Channels.First();

            var offset1 = Math.Sqrt(15 * 15 + 20 * 20);
            var offset2 = Math.Sqrt(30 * 30 + 40 * 40) + 40 + Math.Sqrt(15 * 15 + 20 * 20);
            var length1 = Math.Sqrt(30 * 30 + 40 * 40) + 20;
            var length2 = 20 + Math.Sqrt(30 * 30 + 60 * 60);

            var node = HydroNetworkHelper.SplitChannelAtNode(branch1, new Coordinate(50, 40));

            // remove the newly added node
            NetworkHelper.MergeNodeBranches(node, network);

            Assert.IsNull(network.CurrentEditAction);
            Assert.AreEqual(1, network.Branches.Count);
            Assert.AreEqual(2, network.Nodes.Count);
            Assert.AreEqual(2, network.CrossSections.Count());
            Assert.AreEqual(2, branch1.CrossSections.Count());
            Assert.AreEqual(offset1, branch1.CrossSections.First().Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(offset2, branch1.CrossSections.Skip(1).First().Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length1 + length2, branch1.Geometry.Length);

            Assert.AreEqual(branch1, branch1.CrossSections.First().Branch);
            Assert.AreEqual(branch1, branch1.CrossSections.Skip(1).First().Branch);
        }

        [Test]
        public void AddingToExistingChannelCopiesOrder()
        {
            var network = CreateTestNetwork();
            var channel1 = network.Channels.First();
            channel1.OrderNumber = 7;
            var node2 = channel1.Target; // at coordinate (100,100), see method CreateTestNetwork
                
            var node3 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(200, 200)) };
            network.Nodes.Add(node3);

            var channel2 = new Channel
                               {
                                   Geometry = new LineString(new[]
                                                                 {
                                                                     new Coordinate(100, 100), new Coordinate(200, 200)
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
            var n1 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(50, 50)), Name = "n1" };
            var n2 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(50, 100)), Name = "n2" };
            var n3 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(100, 50)), Name = "n3" };
            var n4 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(50, 0)), Name = "n4" };
            var channel1 = new Channel
                            {
                                Geometry = new LineString(new[] { new Coordinate(50, 50), new Coordinate(50, 100) }),
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
                                Geometry = new LineString(new[] { new Coordinate(50, 50), new Coordinate(100, 50) }),
                                Source = n1,
                                Target = n3,
                                Name = "channel2"
                            };

            network.Nodes.Add(n3);
            NetworkHelper.AddChannelToHydroNetwork(network, channel2);

            var channel3 = new Channel
                            {
                                Geometry = new LineString(new[] { new Coordinate(50, 50), new Coordinate(50, 0) }),
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

            var n5 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(0, 50)) };
            var channel4 = new Channel
            {
                Geometry = new LineString(new[] { new Coordinate(50, 50), new Coordinate(0, 50) }),
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
            var n1 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(50, 50)), Name = "n1" };
            var n2 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(50, 100)), Name = "n2" };
            var n3 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(100, 50)), Name = "n3" };
            var n4 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(50, 0)), Name = "n4" };
            var channel1 = new Channel
            {
                Geometry = new LineString(new[] { new Coordinate(50, 50), new Coordinate(50, 100) }),
                Source = n1,
                Target = n2,
                Name = "channel1" // we don't specify OrderNumber so it will use default value
            };

            network.Nodes.Add(n1);
            network.Nodes.Add(n2);
            NetworkHelper.AddChannelToHydroNetwork(network, channel1);

            var channel2 = new Channel
            {
                Geometry = new LineString(new[] { new Coordinate(50, 50), new Coordinate(100, 50) }),
                Source = n1,
                Target = n3,
                Name = "channel2"
            };

            network.Nodes.Add(n3);
            NetworkHelper.AddChannelToHydroNetwork(network, channel2);

            var channel3 = new Channel
            {
                Geometry = new LineString(new[] { new Coordinate(50, 50), new Coordinate(50, 0) }),
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

            var n5 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(0, 50)) };
            var channel4 = new Channel
            {
                Geometry = new LineString(new[] { new Coordinate(50, 50), new Coordinate(0, 50) }),
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
            var n1 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(50, 0)), Name = "n1" };
            var n2 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(50, 50)), Name = "n2" };
            var n3 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(100, 0)), Name = "n3" };
            var n4 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(100, 50)), Name = "n4" };
            var channel1 = new Channel
            {
                Geometry = new LineString(new[] { new Coordinate(50, 0), new Coordinate(50, 50) }),
                Source = n1,
                Target = n2,
                Name = "channel1",
                OrderNumber =  1
            };

            network.Nodes.Add(n1);
            network.Nodes.Add(n2);
            NetworkHelper.AddChannelToHydroNetwork(network, channel1);

            var channel2 = new Channel
            {
                Geometry = new LineString(new[] { new Coordinate(100, 0), new Coordinate(100, 50) }),
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
                Geometry = new LineString(new[] { new Coordinate(50, 50), new Coordinate(100, 50) }),
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
        public void ReverseBranchWithCrossSections()
        {
            var network = CreateTestNetwork();
            var branch1 = (IChannel)network.Branches[0];

            var nodeFrom = branch1.Source;
            var nodeTo = branch1.Target;

            double offsetCrossSection1 = branch1.CrossSections.First().Chainage;
            double offsetCrossSection2 = branch1.CrossSections.Skip(1).First().Chainage;
            double length = branch1.Geometry.Length;

            HydroNetworkHelper.ReverseBranch(branch1);
            Assert.IsNull(network.CurrentEditAction);
            Assert.AreEqual(nodeFrom, branch1.Target);
            Assert.AreEqual(nodeTo, branch1.Source);
            Assert.AreEqual(length - offsetCrossSection2, branch1.CrossSections.First().Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(length - offsetCrossSection1, branch1.CrossSections.Skip(1).First().Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void ReverseBranchWithCrossSectionsForCustomLength()
        {
            var network = CreateTestNetwork();
            var branch1 = (IChannel)network.Branches[0];

            var nodeFrom = branch1.Source;
            var nodeTo = branch1.Target;

            double offsetCrossSection1 = branch1.CrossSections.First().Chainage;
            double offsetCrossSection2 = branch1.CrossSections.Skip(1).First().Chainage;
            double customLength = branch1.Geometry.Length * 4;
            branch1.IsLengthCustom = true;
            branch1.Length = customLength;
            Assert.AreEqual(offsetCrossSection1 * 4, branch1.CrossSections.First().Chainage);
            Assert.AreEqual(offsetCrossSection2 * 4, branch1.CrossSections.Skip(1).First().Chainage);

            HydroNetworkHelper.ReverseBranch(branch1);
            Assert.IsNull(network.CurrentEditAction);
            Assert.AreEqual(nodeFrom, branch1.Target);
            Assert.AreEqual(nodeTo, branch1.Source);
            Assert.AreEqual(customLength - offsetCrossSection2 * 4, branch1.CrossSections.First().Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(customLength - offsetCrossSection1 * 4, branch1.CrossSections.Skip(1).First().Chainage, BranchFeature.Epsilon);
        }

        /// <summary>
        /// Creates a simple test network of 1 branch and 2 nodes. The branch has '3' parts, in the center of
        /// the first and last is a cross section.
        ///                 n
        ///                /
        ///               /
        ///              cs
        ///             /
        ///     -------/
        ///    /
        ///   cs
        ///  /
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
                                                                    new Coordinate(0, 0), new Coordinate(0, 100),
                                                                })
                              };

            var node1 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(0, 0)) };
            var node2 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(100, 0)) };

            network.Branches.Add(branch1);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            branch1.Source = node1;
            branch1.Target = node2;

            return network;
        }

        private static void AddTestStructureAt(IHydroNetwork network, IChannel branch, double offset)
        {
            IWeir weir = new Weir { Chainage = offset };
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
                                          Geometry =  new LineString(new[]
                                                                 {
                                                                     new Coordinate(offset - 1, 0),
                                                                     new Coordinate(offset + 1, 0)
                                                                 })
                                      };
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, crossSectionXyz, offset);
        }

        [Test]
        public void CreateSegments1Structure()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            var branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 10);

            var networkCoverage = new Discretization
                                      {
                                          Network = network,
                                          SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                      };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,  // branch
                                                      0, // minimumDistance
                                                      true,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, // gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength
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
            var branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 20);
            AddTestStructureAt(network, branch1, 40);
            AddTestStructureAt(network, branch1, 60);

            var networkCoverage = new Discretization
                                      {
                                          Network = network,
                                          SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                      };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1,  // branch
                                                      0, // minimumDistance
                                                      true,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, // gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength
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
            var branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 0.4);

            var networkCoverage = new Discretization
                                      {
                                          Network = network,
                                          SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                      };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1, // branch
                                                      0.5, // minimumDistance
                                                      true,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, // gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

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
            var branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 0.8);

            var networkCoverage = new Discretization
                                      {
                                          Network = network,
                                          SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                      };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1, // branch
                                                      0.5, // minimumDistance
                                                      true,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, // gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

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
            var branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 0.8);
            AddTestStructureAt(network, branch1, 1.2);

            var networkCoverage = new Discretization
                                      {
                                          Network = network,
                                          SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                      };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1, // branch
                                                      0.001, // minimumDistance
                                                      true,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, // gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

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
                                                      branch1, // branch
                                                      0.5, // minimumDistance
                                                      true,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, // gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength
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
            var branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 99.6);

            var networkCoverage = new Discretization
                                      {
                                          Network = network,
                                          SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                      };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1, // branch
                                                      0.5, // minimumDistance
                                                      true,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, // gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

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
            var branch1 = network.Channels.First();

            AddTestStructureAt(network, branch1, 99.2);
            AddTestStructureAt(network, branch1, 98.8);

            var networkCoverage = new Discretization
                                      {
                                          Network = network,
                                          SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                      };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1, // branch
                                                      0.5, // minimumDistance
                                                      true,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, // gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

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
            var branch1 = network.Channels.First();

            AddTestCrossSectionAt(branch1, 50.0);

            var networkCoverage = new Discretization
                                      {
                                          Network = network,
                                          SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                      };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1, // branch
                                                      0.5, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      true, // gridAtCrossSection
                                                      false, // gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength


            Assert.AreEqual(3, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(50.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsFixedLocations()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            var branch1 = network.Channels.First();
            var discretization = new Discretization
                                        {
                                            Network = network,
                                            SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                        };

            HydroNetworkHelper.GenerateDiscretization(discretization, // networkCoverage
                                                      branch1, // branch
                                                      0.5, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      true, // gridAtFixedLength
                                                      10); // fixedLength
            Assert.AreEqual(11, discretization.Locations.Values.Count);
            Assert.AreEqual(0.0, discretization.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(50.0, discretization.Locations.Values[5].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, discretization.Locations.Values[10].Chainage, BranchFeature.Epsilon);

            INetworkLocation networkLocation = discretization.Locations.Values[7];
            Assert.AreEqual(70.0, networkLocation.Chainage, BranchFeature.Epsilon);
            discretization.ToggleFixedPoint(networkLocation);
            //DiscretizationHelper.SetUserDefinedGridPoint(networkLocation, true);

            HydroNetworkHelper.GenerateDiscretization(discretization, // networkCoverage
                                                      branch1, // branch
                                                      0.5, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      true, // gridAtFixedLength
                                                      40); // fixedLength
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
            var network = CreateTestNetwork();
            var firstBranch = (IChannel)network.Branches[0];

            var networkCoverage = new Discretization
                                        {
                                            Network = network,
                                            SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                        };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      firstBranch, // branch
                                                      5.0, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

            Assert.AreEqual(2, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(1, networkCoverage.Segments.Values.Count);
            firstBranch.Length = firstBranch.Length * 2;
            firstBranch.IsLengthCustom = true;
            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      firstBranch, // branch
                                                      5.0, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

            Assert.AreEqual(2, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(1, networkCoverage.Segments.Values.Count);
        }

        [Test]
        public void CreateSegmentsCrossSectionAndMinimumDistance()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            var branch1 = network.Channels.First();

            AddTestCrossSectionAt(branch1, 1.0);

            var networkCoverage = new Discretization
                                      {
                                          Network = network,
                                          SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                      };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1, // branch
                                                      5.0, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      true, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

            Assert.AreEqual(2, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsCrossSectionAndMinimumDistanceNearEnd()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            var branch1 = network.Channels.First();

            AddTestCrossSectionAt(branch1, 99.0);

            var networkCoverage = new Discretization
                                      {
                                          Network = network,
                                          SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                      };

            HydroNetworkHelper.GenerateDiscretization(networkCoverage, // networkCoverage
                                                      branch1, // branch
                                                      5.0, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      true, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

            Assert.AreEqual(2, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsMultipleCrossSection()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            var branch1 = network.Channels.First();

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
                                                      branch1, // branch
                                                      5.0, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      true, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

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
            var branch1 = network.Channels.First();

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
                                                      branch1, // branch
                                                      5.0, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      true, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

            Assert.AreEqual(3, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(5.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void CreateSegmentsMultipleLateralsAndMinimumDistance()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            var branch1 = network.Channels.First();

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
                                                      branch1, // branch
                                                      5.0, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      true, //gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

            Assert.AreEqual(3, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(5.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
        }
        
        [Test]
        public void CreateSegmentsMultipleLaterals()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            var branch1 = network.Channels.First();

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
                                                      branch1, // branch
                                                      1.0, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      true, //gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

            Assert.AreEqual(8, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(2.0, networkCoverage.Locations.Values[2].Chainage, BranchFeature.Epsilon);
        }

        [Test]
        public void DoNotCreateSegmentsMultipleLateralsAndMinimumDistance()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            var branch1 = network.Channels.First();

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
                                                      branch1, // branch
                                                      5.0, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

            Assert.AreEqual(2, networkCoverage.Locations.Values.Count);
            Assert.AreEqual(0.0, networkCoverage.Locations.Values[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(100.0, networkCoverage.Locations.Values[1].Chainage, BranchFeature.Epsilon);
        }

        private static void AddLateralAt(IChannel branch, double offset)
        {
            NetworkHelper.AddBranchFeatureToBranch(new LateralSource(), branch, offset);
        }

        [Test]
        public void CreateSegmentsMultipleCrossSectionsAndFixedPoint()
        {
            IHydroNetwork network = CreateSegmentTestNetwork();
            var branch1 = network.Channels.First();

            var discretization = new Discretization
                                     {
                                          Network = network,
                                          SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                      };

            HydroNetworkHelper.GenerateDiscretization(discretization, // networkCoverage
                                                      branch1, // branch
                                                      5.0, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      true, // gridAtFixedLength
                                                      2); // fixedLength
            Assert.AreEqual(51, discretization.Locations.Values.Count);


            INetworkLocation networkLocation = discretization.Locations.Values.Where(nl => nl.Chainage == 8).First();
            discretization.ToggleFixedPoint(networkLocation);
            networkLocation = discretization.Locations.Values.Where(nl => nl.Chainage == 32).First();
            discretization.ToggleFixedPoint(networkLocation);

            AddTestCrossSectionAt(branch1, 10.0);
            AddTestCrossSectionAt(branch1, 20.0);
            AddTestCrossSectionAt(branch1, 30.0);

            HydroNetworkHelper.GenerateDiscretization(discretization, // networkCoverage
                                                      branch1, // branch
                                                      5.0, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      true, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength
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
            var branch1 = network.Channels.First();

            var discretization = new Discretization
                                     {
                                          Network = network,
                                          SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocations
                                      };

            HydroNetworkHelper.GenerateDiscretization(discretization, // networkCoverage
                                                      branch1, // branch
                                                      5.0, // minimumDistance
                                                      false,  // gridAtStructure
                                                      0.5, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      true, // gridAtFixedLength
                                                      2); // fixedLength
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
                                                      branch1, // branch
                                                      6.0, // minimumDistance
                                                      true,  // gridAtStructure
                                                      4.0, // structureDistance
                                                      false, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength
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
                                        new Coordinate(0, 0), new Coordinate(1262.0, 0),
                                    })
            };
            var node1 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(0, 0)) };
            var node2 = new HydroNode { Network = network, Geometry = new Point(new Coordinate(1262.0, 0)) };
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
                                                      channel, // branch
                                                      0.5, // minimumDistance
                                                      true,  // gridAtStructure
                                                      1.0, // structureDistance
                                                      true, // gridAtCrossSection
                                                      false, //gridAtLateralSource
                                                      false, // gridAtFixedLength
                                                      -1); // fixedLength

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

            var gridPoints = discretization.Locations.Values;
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

        private static void AddCrossSection(Channel branch, double chainage)
        {
            var crossSection = new CrossSectionDefinitionXYZ
                                   {
                                       Geometry = new LineString(new[] { new Coordinate(chainage, 0), new Coordinate(chainage + 1, 0) }),
                                   };
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, crossSection, chainage);
        }

        [Test]
        public void SendCustomActionForSplitBranch()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(0, 100));
            int callCount = 0;
            IChannel channelToSplit = network.Channels.First();
            ((INotifyPropertyChange)network).PropertyChanged += (s, e) =>
                                                                     {
                                                                         //finished editing
                                                                         if ((e.PropertyName == "IsEditing") &&
                                                                             (!network.IsEditing))
                                                                         {
                                                                             callCount++;
                                                                             var editAction =
                                                                                 (BranchSplitAction)
                                                                                 network.CurrentEditAction;
                                                                             Assert.AreEqual(channelToSplit,
                                                                                             editAction.SplittedBranch);
                                                                             Assert.AreEqual(50,
                                                                                             editAction.SplittedBranch.Length);
                                                                             Assert.AreEqual(
                                                                                 network.Channels.ElementAt(1),
                                                                                 editAction.NewBranch);
                                                                         }
                                                                     };

            HydroNetworkHelper.SplitChannelAtNode(channelToSplit, 50);
            Assert.AreEqual(1, callCount);
            Assert.IsNull(network.CurrentEditAction);
        }

        [Test]
        public void ReverseBranchUsesEditAction()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(0, 100));

            IChannel channelToReverse = network.Channels.First();
            channelToReverse.Name = "testChannel";
            var testCoverage = new NetworkCoverage { Network = network };
            testCoverage[new NetworkLocation(channelToReverse, 10)] = 1.1; // This value would map to offset 90, and should not generate issues (dictionary key duplication for example)
            testCoverage[new NetworkLocation(channelToReverse, 30)] = 2.2;
            testCoverage[new NetworkLocation(channelToReverse, 90)] = 3.3; 

            int callCount = 0;
            ((INotifyPropertyChange)network).PropertyChanged += (s, e) =>
                                                                    {
                                                                        //finished editing
                                                                        if ((e.PropertyName == "IsEditing") &&
                                                                            (!network.IsEditing))
                                                                        {
                                                                            callCount++;
                                                                            Assert.IsTrue(network.CurrentEditAction is BranchReverseAction);
                                                                            var editAction = (BranchReverseAction) network.CurrentEditAction;
                                                                            Assert.AreEqual(channelToReverse, editAction.ReversedBranch);
                                                                        }
                                                                    };

            HydroNetworkHelper.ReverseBranch(channelToReverse);
            Assert.AreEqual(1, callCount);
            Assert.IsNull(testCoverage.CurrentEditAction);
            Assert.IsNull(network.CurrentEditAction);

            var locations = testCoverage.GetLocationsForBranch(channelToReverse);
            Assert.AreEqual(10.0, locations[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(3.3, testCoverage[locations[0]]);
            Assert.AreEqual(70.0, locations[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(2.2, testCoverage[locations[1]]);
            Assert.AreEqual(90.0, locations[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.1, testCoverage[locations[2]]);
        }

        [Test]
        public void ReverseBranchWithTimeDependentCoverage()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(0, 100));
            var testCoverage = new NetworkCoverage { Network = network, IsTimeDependent = true};
            IChannel channelToReverse = network.Channels.First();

            // Setup locations
            var offsets = new[] {10, 30, 90};
            var locations = from offset in offsets
                            select new NetworkLocation(channelToReverse, offset);
            testCoverage.Locations.FixedSize = 3;
            testCoverage.Locations.SetValues(locations.OrderBy(loc => loc));
            
            // steup values
            var values = new[] {1.1, 2.2, 3.3};

            // setup time arguments
            var startTime = new DateTime(2000, 1, 1);
            var times = from i in Enumerable.Range(1, 3)
                        select startTime.AddDays(i);

            // Set function values
            int outputTimeStepIndex = 0;
            foreach (var dateTime in times)
            {
                var locationIndexFilter = new VariableIndexRangeFilter(testCoverage.Locations,
                                                                       0, testCoverage.Locations.FixedSize - 1);
                var timeIndexFilter = new VariableIndexRangeFilter(testCoverage.Time, outputTimeStepIndex);

                testCoverage.Time.AddValues(new[] {dateTime});
                testCoverage.SetValues(values, new[]{locationIndexFilter,timeIndexFilter});

                for (int i = 0; i < values.Length; i++)
                {
                    values[i]++;
                }
                outputTimeStepIndex++;
            }

            // Set up event listener
            int callCount = 0;
            ((INotifyPropertyChange)network).PropertyChanged += (s, e) =>
            {
                //finished editing
                if ((e.PropertyName == "IsEditing") &&
                    (!network.IsEditing))
                {
                    callCount++;
                    Assert.IsTrue(network.CurrentEditAction is BranchReverseAction);
                    var editAction = (BranchReverseAction)network.CurrentEditAction;
                    Assert.AreEqual(channelToReverse, editAction.ReversedBranch);
                }
            };

            // Function at this time is:
            //                    T1         T2         T3
            // Argument[0] = { 1/2/2000 , 1/3/2000, 1/4/2000 }
            //                   L1            L2             L3
            // Argument[1] = { NL(br1, 10), NL(br1, 30), NL(br1, 90) }
            //  where NL stands for 'NetworkLocation', and 'br1' for channelToReverse
            //
            //                    L1    L2   L3
            // Component[ ] = { { 1.1, 2.2, 3.3 },       T1
            //                  { 2.1, 3.2, 4.3 },       T2
            //                  { 3.1, 4.2, 5.3 } }      T3

            HydroNetworkHelper.ReverseBranch(channelToReverse);

            // Function at this time should be:
            //                    T1         T2         T3
            // Argument[0] = { 1/2/2000 , 1/3/2000, 1/4/2000 }
            //                   L3            L2             L1
            // Argument[1] = { NL(br1, 10), NL(br1, 70), NL(br1, 90) }
            //  Notice that L1 and L3 have changed positions in the arguments array, due to sorting!
            //  As such, component needs to be adapted too
            //
            //                    L3    L2   L1
            // Component[ ] = { { 3.3, 2.2, 1.1 },       T1
            //                  { 4.3, 3.2, 2.1 },       T2
            //                  { 5.3, 4.2, 3.1 } }      T3

            Assert.AreEqual(1, callCount);
            Assert.IsNull(testCoverage.CurrentEditAction);
            Assert.IsNull(network.CurrentEditAction);

            var postReversalLocations = testCoverage.GetLocationsForBranch(channelToReverse);
            // Assert correctness of L3 (now first in Locations)
            Assert.AreEqual(10.0, postReversalLocations[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(3.3, testCoverage.Evaluate(times.ElementAt(0), postReversalLocations[0]));
            Assert.AreEqual(4.3, testCoverage.Evaluate(times.ElementAt(1), postReversalLocations[0]));
            Assert.AreEqual(5.3, testCoverage.Evaluate(times.ElementAt(2), postReversalLocations[0]));
            // Assert correctness of L2
            Assert.AreEqual(70.0, postReversalLocations[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(2.2, testCoverage.Evaluate(times.ElementAt(0), postReversalLocations[1]));
            Assert.AreEqual(3.2, testCoverage.Evaluate(times.ElementAt(1), postReversalLocations[1]));
            Assert.AreEqual(4.2, testCoverage.Evaluate(times.ElementAt(2), postReversalLocations[1]));
            // Assert correctness of L1 (now last in Locations)
            Assert.AreEqual(90.0, postReversalLocations[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.1, testCoverage.Evaluate(times.ElementAt(0), postReversalLocations[2]));
            Assert.AreEqual(2.1, testCoverage.Evaluate(times.ElementAt(1), postReversalLocations[2]));
            Assert.AreEqual(3.1, testCoverage.Evaluate(times.ElementAt(2), postReversalLocations[2]));
        }

        [Test]
        public void ReverseBranchWithMultipleArguments()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(0, 100));
            var testCoverage = new NetworkCoverage { Network = network };
            IChannel channelToReverse = network.Channels.First();

            var argument1 = new Variable<double>();
            argument1.SetValues(new[] { 1.0, 3.0 });
            var variable2 = new Variable<int>();
            variable2.SetValues(new[] {5, 9});

            testCoverage.Arguments.Add(argument1);

            // Setup locations
            var offsets = new[] { 10, 30, 90 };
            var locations = from offset in offsets
                            select new NetworkLocation(channelToReverse, offset);
            testCoverage.Locations.FixedSize = 3;
            testCoverage.Locations.SetValues(locations.OrderBy(loc => loc));

            testCoverage.Arguments.Add(variable2);

            int callCount = 0;
            ((INotifyPropertyChange)network).PropertyChanged += (s, e) =>
            {
                //finished editing
                if ((e.PropertyName == "IsEditing") &&
                    (!network.IsEditing))
                {
                    callCount++;
                    Assert.IsTrue(network.CurrentEditAction is BranchReverseAction);
                    var editAction = (BranchReverseAction)network.CurrentEditAction;
                    Assert.AreEqual(channelToReverse, editAction.ReversedBranch);
                }
            };

            // Assign values
            testCoverage.SetValues(new[] { 11.1, 11.2, 11.3 }, new IVariableValueFilter[] { new VariableValueFilter<double>(argument1, 1.0), new VariableValueFilter<int>(variable2, 5) });
            testCoverage.SetValues(new[] { 12.1, 12.2, 12.3 }, new IVariableValueFilter[] { new VariableValueFilter<double>(argument1, 1.0), new VariableValueFilter<int>(variable2, 9) });
            testCoverage.SetValues(new[] { 21.1, 21.2, 21.3 }, new IVariableValueFilter[] { new VariableValueFilter<double>(argument1, 3.0), new VariableValueFilter<int>(variable2, 5) });
            testCoverage.SetValues(new[] { 22.1, 22.2, 22.3 }, new IVariableValueFilter[] { new VariableValueFilter<double>(argument1, 3.0), new VariableValueFilter<int>(variable2, 9) });

            // Function at this time is:
            //                   L1            L2             L3
            // Argument[0] = { NL(br1, 10), NL(br1, 30), NL(br1, 90) }
            //                  V1   V2
            // Argument[1] = { 1.0, 3.0 }
            //                  X1    X2
            // Argument[2] = {  5  ,  9  }
            //  where NL stands for 'NetworkLocation', and 'br1' for channelToReverse
            //
            //                   |  X1 |  X2  |  | X1  |  X2  |
            // Component[ ] ={ { { 11.1, 12.1 }, { 21.1, 22.1 } },       L1
            //                 { { 11.2, 12.2 }, { 21.2, 22.2 } },       L2
            //                 { { 11.3, 12.3 }, { 21.3, 22.3 } } }      L3
            //                   |     V1     |  |     V2     |

            HydroNetworkHelper.ReverseBranch(channelToReverse);

            // Function should be:
            //                   L3            L2             L1
            // Argument[0] = { NL(br1, 10), NL(br1, 70), NL(br1, 90) }
            //                  V1   V2
            // Argument[1] = { 1.0, 3.0 }
            //                  X1    X2
            // Argument[2] = {  5  ,  9  }
            //  Notice that L1 and L3 have changed positions in the arguments array, due to sorting!
            //  As such, component needs to be adapted too
            //
            //                   |  X1 |  X2  |  | X1  |  X2  |
            // Component[ ] ={ { { 11.3, 12.3 }, { 21.3, 22.3 } },       L3
            //                 { { 11.2, 12.2 }, { 21.2, 22.2 } },       L2
            //                 { { 11.1, 12.1 }, { 21.1, 22.1 } } }      L1
            //                   |     V1     |  |     V2     |
            Assert.AreEqual(1, callCount);
            Assert.IsNull(testCoverage.CurrentEditAction);
            Assert.IsNull(network.CurrentEditAction);

            var postReversallocations = testCoverage.GetLocationsForBranch(channelToReverse);
            // For L3
            Assert.AreEqual(10.0, postReversallocations[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(11.3, testCoverage[postReversallocations[0], 1.0, 5]);
            Assert.AreEqual(12.3, testCoverage[postReversallocations[0], 1.0, 9]);
            Assert.AreEqual(21.3, testCoverage[postReversallocations[0], 3.0, 5]);
            Assert.AreEqual(22.3, testCoverage[postReversallocations[0], 3.0, 9]);
            // For L2
            Assert.AreEqual(70.0, postReversallocations[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(11.2, testCoverage[postReversallocations[1], 1.0, 5]);
            Assert.AreEqual(12.2, testCoverage[postReversallocations[1], 1.0, 9]);
            Assert.AreEqual(21.2, testCoverage[postReversallocations[1], 3.0, 5]);
            Assert.AreEqual(22.2, testCoverage[postReversallocations[1], 3.0, 9]);
            // FOr L1
            Assert.AreEqual(90.0, postReversallocations[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(11.1, testCoverage[postReversallocations[2], 1.0, 5]);
            Assert.AreEqual(12.1, testCoverage[postReversallocations[2], 1.0, 9]);
            Assert.AreEqual(21.1, testCoverage[postReversallocations[2], 3.0, 5]);
            Assert.AreEqual(22.1, testCoverage[postReversallocations[2], 3.0, 9]);
        }

        [Test]
        public void ReverseBranchUsesEditActionAndWorksProperlyForCustomLength()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(0, 100));
            IChannel channelToReverse = network.Channels.First();
            channelToReverse.IsLengthCustom = true;
            channelToReverse.Length = 200;
            var testCoverage = new NetworkCoverage { Network = network };
            testCoverage[new NetworkLocation(channelToReverse, 10)] = 1.1;
            testCoverage[new NetworkLocation(channelToReverse, 30)] = 2.2;

            int callCount = 0;
            ((INotifyPropertyChange)network).PropertyChanged += (s, e) =>
            {
                //finished editing
                if ((e.PropertyName == "IsEditing") &&
                    (!network.IsEditing))
                {
                    callCount++;
                    Assert.IsTrue(network.CurrentEditAction is BranchReverseAction);
                    var editAction = (BranchReverseAction)network.CurrentEditAction;
                    Assert.AreEqual(channelToReverse, editAction.ReversedBranch);
                }
            };

            HydroNetworkHelper.ReverseBranch(channelToReverse);
            Assert.AreEqual(1, callCount);
            Assert.IsNull(testCoverage.CurrentEditAction);
            Assert.IsNull(network.CurrentEditAction);

            var locations = testCoverage.GetLocationsForBranch(channelToReverse);
            Assert.AreEqual(170.0, locations[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(2.2, testCoverage[locations[0]]);
            Assert.AreEqual(190.0, locations[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.1, testCoverage[locations[1]]);
        }

        [Test]
        [Category(TestCategory.Jira)]
        public void MultipleBranchReversalsShouldNotMoveNetworkCoverageLocationsDueToRoundingTools6878()
        {
            var from = new Node();
            var to = new Node();
            var branch = new Branch { Source = from, Target = to, Geometry = new LineString(new Coordinate[] { new Coordinate(0, 0), new Coordinate(0, 100.987654321) }) };
            var network = new Network { Nodes = { from, to }, Branches = { branch } };

            var testCoverage = new NetworkCoverage { Network = network };
            testCoverage[new NetworkLocation(branch, 10)] = 1.1;
            testCoverage[new NetworkLocation(branch, 30)] = 2.2;

            // reverse number of times
            for (var i = 0; i < 30; i++ )
            {
                HydroNetworkHelper.ReverseBranch(branch);
            }

            // check if locations do not move due to rounding errors
            var locations = testCoverage.Locations.Values;

            Assert.AreEqual(10.0, locations[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.1, testCoverage[locations[0]]);
            Assert.AreEqual(30.0, locations[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(2.2, testCoverage[locations[1]]);
        }

        [Test]
        public void BranchReverseFollowedByResizeShouldWorkAsExpected()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(0, 100));

            IChannel channelToReverse = network.Channels.First();
            channelToReverse.IsLengthCustom = true;
            channelToReverse.Length = 100;

            channelToReverse.Name = "testChannel";
            var testCoverage = new NetworkCoverage { Network = network };
            testCoverage[new NetworkLocation(channelToReverse, 10)] = 1.1; // This value would map to offset 90, and should not generate issues (dictionary key duplication for example)
            testCoverage[new NetworkLocation(channelToReverse, 30)] = 2.2;
            testCoverage[new NetworkLocation(channelToReverse, 90)] = 3.3;

            int callCount = 0;
            ((INotifyPropertyChange)network).PropertyChanged += (s, e) =>
            {
                //finished editing
                if ((e.PropertyName == "IsEditing") &&
                    (!network.IsEditing))
                {
                    callCount++;
                    Assert.IsTrue(network.CurrentEditAction is BranchReverseAction);
                    var editAction = (BranchReverseAction)network.CurrentEditAction;
                    Assert.AreEqual(channelToReverse, editAction.ReversedBranch);
                }
            };

            HydroNetworkHelper.ReverseBranch(channelToReverse);
            Assert.AreEqual(1, callCount);
            Assert.IsNull(testCoverage.CurrentEditAction);
            Assert.IsNull(network.CurrentEditAction);

            var locations = testCoverage.GetLocationsForBranch(channelToReverse);
            Assert.AreEqual(10.0, locations[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(3.3, testCoverage[locations[0]]);
            Assert.AreEqual(70.0, locations[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(2.2, testCoverage[locations[1]]);
            Assert.AreEqual(90.0, locations[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.1, testCoverage[locations[2]]);

            // Change in length of the branch, should cause an automatic update in the location Offsets.
            channelToReverse.Length = 50;

            locations = testCoverage.GetLocationsForBranch(channelToReverse);
            Assert.AreEqual(5.0, locations[0].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(3.3, testCoverage[locations[0]]);
            Assert.AreEqual(35.0, locations[1].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(2.2, testCoverage[locations[1]]);
            Assert.AreEqual(45.0, locations[2].Chainage, BranchFeature.Epsilon);
            Assert.AreEqual(1.1, testCoverage[locations[2]]);
        }

        [Test]
        public void CreateUniqueHydroObjectNameInSubregion()
        {
            var sourceObject = new Catchment() {Name = "source"};
            var targetObject = new LateralSource() { Name = "target" };
            var link = new HydroLink(sourceObject, targetObject) {Name = "HydroLink1"};

            var region = new HydroRegion();
            region.Links.Add(link);

            var name = HydroNetworkHelper.GetUniqueFeatureName(region, new HydroLink(Substitute.For<IHydroObject>(), Substitute.For<IHydroObject>()));

            Assert.AreNotEqual(link.Name, name);
        }

        [Test]
        public void KeepHydroObjectNameIfUniqueHydroObjectNameDoesntExitsInNetworkObject()
        {
            const string uniquehydrolinkName1 = "UniqueHydroLink1";
            const string uniquehydrolinkName2 = "UniqueHydroLink2"; 
            var sourceObject = new Catchment() { Name = "source" };
            var targetObject = new LateralSource() { Name = "target" };
            var link = new HydroLink(sourceObject, targetObject) { Name = uniquehydrolinkName1 };

            var region = new HydroRegion();
            region.Links.Add(link);


            var name = HydroNetworkHelper.GetUniqueFeatureName(region, new HydroLink(Substitute.For<IHydroObject>(), Substitute.For<IHydroObject>()) { Name = uniquehydrolinkName2 }, true);
            Assert.That(name, Is.EqualTo(uniquehydrolinkName2));
        }

        [Test]
        public void DontKeepHydroObjectNameIfUniqueHydroObjectNameExitsInNetworkObjectAndCheckIfNewNameIsNeeded()
        {
            const string uniquehydrolinkName = "UniqueHydroLink";

            var sourceObject = new HydroNode("source");
            var targetObject = new LateralSource() { Name = "target" };
            var link = new HydroLink(sourceObject, targetObject) { Name = uniquehydrolinkName };

            var region = new HydroRegion();
            region.Links.Add(link);
            
            var nameAlreadyExistCreateNew = HydroNetworkHelper.GetUniqueFeatureName(region, new HydroLink(Substitute.For<IHydroObject>(), Substitute.For<IHydroObject>()) { Name = uniquehydrolinkName }, true);
            Assert.That(nameAlreadyExistCreateNew, Is.Not.EqualTo(uniquehydrolinkName));
        }

        [Test]
        public void CreateHydroObjectNameIfNewUniqueHydroObjectNameIsNullInNetworkObjectAndCheckIfNewNameIsNeeded()
        {
            const string uniquehydrolinkName = "UniqueHydroLink";

            var sourceObject = new HydroNode("source");
            var targetObject = new LateralSource() { Name = "target" };
            var link = new HydroLink(sourceObject, targetObject) { Name = uniquehydrolinkName };

            var region = new HydroRegion();
            region.Links.Add(link);

            var nameAlreadyExistCreateNew = HydroNetworkHelper.GetUniqueFeatureName(region, new HydroLink(Substitute.For<IHydroObject>(), Substitute.For<IHydroObject>()) { Name = null }, true);
            Assert.That(nameAlreadyExistCreateNew, Is.Not.Null);
            Assert.That(nameAlreadyExistCreateNew, Is.Not.EqualTo(link.Name));
        }

        [Test]
        [Category(TestCategory.Jira)]
        public void RemoveStructureOnDisconnectedStructureShouldNotCrashTools9784()
        {
            var weir = new Weir();
            Assert.DoesNotThrow(() => HydroNetworkHelper.RemoveStructure(weir));
        }
    }
}