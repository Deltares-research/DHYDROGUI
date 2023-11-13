using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid.DeltaresUGrid
{
    [TestFixture]
    public class Disposable1DMeshGeometryExtensionsTests
    {

        [Test]
        public void GivenDiscretizationOnNetworkWithALocationAfterBranchLength_WhenCreateMeshForKernelAndCheckForDoubleCalculationPoint_ThenReturnMessage()
        {
            // arrange
            IHydroNetwork network = CreateHydroNetworkWithSimpleBranch();
            IDiscretization discretization = CreateSimpleDiscretization(network);

            string[] messages;
            var branchIdLookup = network.Branches.ToIndexDictionary();
            IBranch branch = network.Branches[0];
            INetworkLocation lastPossibleNetworkLocation = discretization.Locations.Values[discretization.Locations.Values.Count - 2];
            INetworkLocation lastNetworkLocation = discretization.Locations.Values.Last();

            // act
            double lastChainageOfLastCalculationPointInMesh;
            using (var disposable1DMeshGeometry = discretization.CreateDisposable1DMeshGeometry())
            {
                messages = disposable1DMeshGeometry.ValidateAgainstDiscretization(discretization).ToArray();
                lastChainageOfLastCalculationPointInMesh = disposable1DMeshGeometry.BranchOffsets.Last();
            }
            // asserts
            Assert.That(messages.Length, Is.EqualTo(1));
            Assert.That(lastNetworkLocation.Chainage, Is.Not.EqualTo(lastChainageOfLastCalculationPointInMesh));
            Assert.That(messages[0], Is.EqualTo(string.Format(
                                                    Properties.Resources.HydroUGridExtensions_CheckForDoubleCalculationPoint_In_Mesh1D_With_Existing_NetworkLocations_Of_Model,
                                                    branchIdLookup[branch],
                                                    branch.Name,
                                                    branch.Length,
                                                    lastChainageOfLastCalculationPointInMesh,
                                                    lastPossibleNetworkLocation.Chainage,
                                                    lastPossibleNetworkLocation.Name,
                                                    lastNetworkLocation.Name)));

        }

        [Test]
        public void GivenDiscretizationOnNetworkWithASegmentWithoutAEndNetworkLocation_WhenCreateMeshForKernelAndValidateAgainstDiscretization_ThenReturnMessage()
        {
            // arrange
            IHydroNetwork network = CreateHydroNetworkWithSimpleBranch();
            IDiscretization discretization = CreateWrongSegmentEndPointDiscretization(network);

            string[] messages;
            INetworkSegment segment = discretization.Segments.Values.Last();

            // act
            using (var disposable1DMeshGeometry = discretization.CreateDisposable1DMeshGeometry())
            {
                messages = disposable1DMeshGeometry.ValidateAgainstDiscretization(discretization).ToArray();
            }
            // asserts
            Assert.That(messages.Length, Is.EqualTo(1));
            Assert.That(messages[0], Is.EqualTo(string.Format(Properties.Resources.HydroUGridExtensions_Cannot_find_end_edge_node_of_section,
                                                              segment.SegmentNumber,
                                                              segment.Branch.Name,
                                                              segment.EndChainage,
                                                              segment.Branch.Name)));

        }

        [Test]
        public void GivenDiscretizationOnNetworkWithASegmentWithoutABeginNetworkLocation_WhenCreateMeshForKernelAndValidateAgainstDiscretization_ThenReturnMessage()
        {
            // arrange
            IHydroNetwork network = CreateHydroNetworkWithSimpleBranch();
            IDiscretization discretization = CreateWrongSegmentBeginPointDiscretization(network);

            string[] messages;
            INetworkSegment segment = discretization.Segments.Values.First();

            // act
            using (var disposable1DMeshGeometry = discretization.CreateDisposable1DMeshGeometry())
            {
                messages = disposable1DMeshGeometry.ValidateAgainstDiscretization(discretization).ToArray();
            }
            // asserts
            Assert.That(messages.Length, Is.EqualTo(1));
            Assert.That(messages[0], Is.EqualTo(string.Format(Properties.Resources.HydroUGridExtensions_Cannot_find_start_edge_node_of_section,
                                                              segment.SegmentNumber,
                                                              segment.Branch.Name,
                                                              segment.Chainage,
                                                              segment.Branch.Name)));

        }

        [Test]
        public void Given1D2DLinksAndDiscretizationSourceLocationGeometryExistMoreThanOnceWhenCreateMeshForKernelAndValidateAgainsTheLinksThenErrorMessage()
        {
            // arrange
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var discretization = new Discretization() { Network = network };
            var channel = network.Channels.First();
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                                                      true, 10.0, new List<IChannel> { channel });
            discretization.Locations.Values[3].Geometry.Coordinate.X = discretization.Locations.Values[0].Geometry.Coordinate.X;
            discretization.Locations.Values[3].Geometry.Coordinate.Y = discretization.Locations.Values[0].Geometry.Coordinate.Y;
            ILink1D2D link1D2D = Substitute.For<ILink1D2D>();
            link1D2D.DiscretisationPointIndex.Returns(0);
            link1D2D.FaceIndex.Returns(10);
            const string mylinkName = "MyLink";
            link1D2D.Name.Returns(mylinkName);
            IEnumerable<ILink1D2D> link1D2Ds = Enumerable.Repeat(link1D2D, 1);
            string[] messages;
            
            // act
            using (var mesh = discretization.CreateDisposable1DMeshGeometry())
            {
                messages = mesh.ValidateAgainstLinks(link1D2Ds).ToArray();
            }

            // asserts
            Assert.That(messages.Length, Is.EqualTo(1));
            var otherDiscretizationPointNames = new[] { discretization.Locations.Values[3].Name };
            var message = string.Format(Properties.Resources.UGridFileHelper_ValidateMesh1DSourceLocationsOnlyExistOnce_ErrorMessage_part1, link1D2D.Name, discretization.Locations.Values[0].Name, link1D2D.FaceIndex) +
                          Environment.NewLine +
                          string.Format(Properties.Resources.UGridFileHelper_ValidateMesh1DSourceLocationsOnlyExistOnce_ErrorMessage_part2, string.Join(", ", otherDiscretizationPointNames));
            Assert.That(messages[0], Is.EqualTo(message));
        }

        private static IDiscretization CreateSimpleDiscretization(IHydroNetwork network)
        {
            IDiscretization discretization = Substitute.For<IDiscretization>();
            discretization.Network.Returns(network);
            IBranch branch = network.Branches[0];
            int expectedLocations = 11;
            double segmentLength = branch.Length / (expectedLocations - 1);

            var points = network.Branches.SelectMany((b, i) =>
            {
                return Enumerable.Range(0, expectedLocations).Select(j => new
                {
                    branch = b,
                    chainage = j * (b.Length / segmentLength),
                    name = $"{b.Name}_node{j}"
                });
            }).ToArray();
            var networkLocations = points.Select(p => (INetworkLocation)new NetworkLocation(p.branch, p.chainage) { Name = p.name }).ToList();

            // now create an already 'existing' networklocation which will be in  the mesh1d, on the branch but longer than the branch
            var networkLocation = (INetworkLocation)networkLocations.Last().Clone();
            networkLocation.Name = "special";
            networkLocation.Chainage = branch.Length + 1;
            networkLocations.Add(networkLocation);

            discretization.Locations.Values.Returns(new MultiDimensionalArray<INetworkLocation>(networkLocations));

            INetworkSegment[] networkSegments = points.TakeWhile(p => p.chainage < branch.Length)
                                                      .Select((p, i) =>
                                                                  new NetworkSegment
                                                                  {
                                                                      Geometry = new LineString(new[]
                                                                      {
                                                                          new Coordinate(0, i * segmentLength),
                                                                          new Coordinate(0, (i * segmentLength) + segmentLength)
                                                                      }),
                                                                      Branch = branch,
                                                                      Chainage = p.chainage,
                                                                      Length = segmentLength,
                                                                      SegmentNumber = i
                                                                  }).ToArray();
            discretization.Segments.Values.Returns(new MultiDimensionalArray<INetworkSegment>(networkSegments));
            discretization.GetLocationsForBranch(branch).Returns(networkLocations);
            return discretization;
        }

        private static IDiscretization CreateWrongSegmentEndPointDiscretization(IHydroNetwork network)
        {
            IDiscretization discretization = Substitute.For<IDiscretization>();
            discretization.Network.Returns(network);
            IBranch branch = network.Branches[0];
            int expectedLocations = 11;
            double segmentLength = branch.Length / (expectedLocations - 1);

            // create 1 less network location at the end of the branch (so not 11 but 10)
            var points = network.Branches.SelectMany((b, i) =>
            {
                return Enumerable.Range(0, expectedLocations - 1).Select(j => new
                {
                    branch = b,
                    chainage = j * (b.Length / segmentLength),
                    name = $"{b.Name}_node{j}"
                });
            }).ToArray();

            var networkLocations = points.Select(p => (INetworkLocation)new NetworkLocation(p.branch, p.chainage) { Name = p.name }).ToList();
            discretization.Locations.Values.Returns(new MultiDimensionalArray<INetworkLocation>(networkLocations));


            INetworkSegment[] networkSegments = points
                                                .TakeWhile(p => p.chainage < branch.Length)
                                                .Select((p, i) =>
                                                            new NetworkSegment
                                                            {
                                                                Geometry = new LineString(new[]
                                                                {
                                                                    new Coordinate(0, i * segmentLength),
                                                                    new Coordinate(0, (i * segmentLength) + segmentLength)
                                                                }),
                                                                Branch = branch,
                                                                Chainage = p.chainage,
                                                                Length = segmentLength,
                                                                SegmentNumber = i
                                                            }).ToArray();

            discretization.Segments.Values.Returns(new MultiDimensionalArray<INetworkSegment>(networkSegments));
            discretization.GetLocationsForBranch(branch).Returns(networkLocations);
            return discretization;
        }

        private static IDiscretization CreateWrongSegmentBeginPointDiscretization(IHydroNetwork network)
        {
            IDiscretization discretization = Substitute.For<IDiscretization>();
            discretization.Network.Returns(network);
            IBranch branch = network.Branches[0];
            int expectedLocations = 11;
            double segmentLength = branch.Length / (expectedLocations - 1);


            // create 1 less network location at the begin of the branch (so not 11 but 10), so start at 1
            var points = network.Branches.SelectMany((b, i) =>
            {
                return Enumerable.Range(1, expectedLocations - 1).Select(j =>
                {
                    return new
                    {
                        branch = b,
                        chainage = j * (b.Length / segmentLength),
                        name = $"{b.Name}_node{j}"
                    };
                });
            }).ToArray();

            var networkLocations = points.Select(p => (INetworkLocation)new NetworkLocation(p.branch, p.chainage) { Name = p.name }).ToList();
            discretization.Locations.Values.Returns(new MultiDimensionalArray<INetworkLocation>(networkLocations));


            INetworkSegment[] networkSegments = points
                                                .TakeWhile(p => p.chainage < branch.Length)
                                                .Select((p, i) =>
                                                            new NetworkSegment
                                                            {
                                                                Geometry = new LineString(new[]
                                                                {
                                                                    new Coordinate(0, i * segmentLength),
                                                                    new Coordinate(0, (i * segmentLength) + segmentLength)
                                                                }),
                                                                Branch = branch,
                                                                Chainage = p.chainage - segmentLength, //correct chainage because we started location chainage from 1 (1*segmentLength)
                                                                Length = segmentLength,
                                                                SegmentNumber = i
                                                            }).ToArray();

            discretization.Segments.Values.Returns(new MultiDimensionalArray<INetworkSegment>(networkSegments));
            discretization.GetLocationsForBranch(branch).Returns(networkLocations);
            return discretization;
        }

        private static IHydroNetwork CreateHydroNetworkWithSimpleBranch()
        {
            IHydroNetwork network = Substitute.For<IHydroNetwork>();
            IBranch branch = Substitute.For<IBranch>();
            ILineString line = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) });
            branch.Geometry.Returns(line);
            branch.Length.Returns(100);
            branch.Name.Returns("myName");
            IEventedList<IBranch> branches = new EventedList<IBranch>() { branch };
            network.Branches.Returns(branches);
            return network;
        }
    }
}