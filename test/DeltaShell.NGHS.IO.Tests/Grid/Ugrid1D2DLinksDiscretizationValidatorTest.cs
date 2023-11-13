using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.Grid.Validation;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class Ugrid1D2DLinksDiscretizationValidatorTest
    {
        private IHydroModel model;
        private Ugrid1D2DLinksDiscretizationValidator validator;

        [SetUp]
        public void Setup()
        {
            model = Substitute.For<IHydroModel>();
            validator = new Ugrid1D2DLinksDiscretizationValidator(model);
        }

        [Test]
        public void GivenNothingWhenValidateThenExpectedReportTitle()
        {
            // act
            var report = validator.Validate(Enumerable.Empty<ILink1D2D>());

            // asserts
            Assert.IsNotNull(report);
            Assert.That(report.Category, Is.EqualTo(Properties.Resources.Ugrid1D2DLinksDiscretizationValidator_Validate__1D2D_link_mesh1D_source_discretization_locations_validation));

        }

        [Test]
        public void GivenEmpty1D2DLinksAndNoDiscretizationSetWhenValidateThenNoDiscretizationMessage()
        {
            // act
            var report = validator.Validate(Enumerable.Empty<ILink1D2D>());

            // asserts
            Assert.IsNotNull(report);
            Assert.That(report.Issues.Count(), Is.EqualTo(1));
            Assert.That(report.Issues.First().Message, Is.EqualTo(Properties.Resources.Ugrid1D2DLinksDiscretizationValidator_Validate_Discretization_for_1D_network_is_not_set));
        }
        
        [Test]
        public void GivenEmpty1D2DLinksAndEmptyDiscretizationSetWhenValidateThenNoMessages()
        {
            // arrange
            IDiscretization discretization = Substitute.For<IDiscretization>();

            // act
            var report = validator.Validate(Enumerable.Empty<ILink1D2D>(), discretization);

            // asserts
            Assert.IsNotNull(report);
            Assert.That(report.Issues.Count(), Is.EqualTo(0));
        }
        
        [Test]
        public void Given1D2DLinksAndConnectedCorrectlyToDiscretizationSetWhenValidateThenNoMessage()
        {
            // arrange
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var discretization = new Discretization() { Network = network };
            var channel = network.Channels.First();
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                                                      true, 10.0, new List<IChannel> { channel });

            ILink1D2D link1D2D = Substitute.For<ILink1D2D>();
            link1D2D.DiscretisationPointIndex.Returns(0);
            // act
            var report = validator.Validate(Enumerable.Repeat(link1D2D, 1), discretization);

            // asserts
            Assert.IsNotNull(report);
            Assert.That(report.Issues.Count(), Is.EqualTo(0));
        }
        
        [Test]
        public void Given1D2DLinksAndDiscretizationSourceLocationGeometryExistMoreThanOnceWhenValidateThenErrorMessage()
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

            // act
            var report = validator.Validate(link1D2Ds, discretization);

            // asserts
            Assert.IsNotNull(report);
            Assert.That(report.Issues.Count(), Is.EqualTo(1));
            ValidationIssue validationIssue = report.Issues.First();
            Assert.That(validationIssue.Subject, Is.EqualTo(link1D2Ds));
            Assert.That(validationIssue.Severity, Is.EqualTo(ValidationSeverity.Error));
            var otherDiscretizationPointNames = new[] { discretization.Locations.Values[3].Name };
            var message = string.Format(Properties.Resources.UGridFileHelper_ValidateMesh1DSourceLocationsOnlyExistOnce_ErrorMessage_part1, link1D2D.Name, discretization.Locations.Values[0].Name, link1D2D.FaceIndex) +
                          Environment.NewLine +
                          string.Format(Properties.Resources.UGridFileHelper_ValidateMesh1DSourceLocationsOnlyExistOnce_ErrorMessage_part2, string.Join(", ", otherDiscretizationPointNames));
            Assert.That(validationIssue.Message, Is.EqualTo(message));
        }

        [Test]
        public void GivenDiscretizationOnNetworkWithALocationAfterBranchLength_WhenCreateMeshForKernelAndCheckForDoubleCalculationPoint_ThenReturnMessage()
        {
            // arrange
            IHydroNetwork network = CreateHydroNetworkWithSimpleBranch();
            IDiscretization discretization = CreateSimpleDiscretization(network);

            var branchIdLookup = network.Branches.ToIndexDictionary();
            IBranch branch = network.Branches[0];
            INetworkLocation lastPossibleNetworkLocation = discretization.Locations.Values[discretization.Locations.Values.Count - 2];
            INetworkLocation lastNetworkLocation = discretization.Locations.Values.Last();

            // act
            double lastChainageOfLastCalculationPointInMesh = 100;
            var report = validator.Validate(Enumerable.Empty<ILink1D2D>(), discretization);

            // asserts
            Assert.IsNotNull(report);
            Assert.That(report.Issues.Count(), Is.EqualTo(1));
            ValidationIssue validationIssue = report.Issues.First();
            Assert.That(validationIssue.Subject, Is.EqualTo(discretization));
            Assert.That(validationIssue.Severity, Is.EqualTo(ValidationSeverity.Warning));
            var message = string.Format(
                Properties.Resources.HydroUGridExtensions_CheckForDoubleCalculationPoint_In_Mesh1D_With_Existing_NetworkLocations_Of_Model,
                branchIdLookup[branch],
                branch.Name,
                branch.Length,
                lastChainageOfLastCalculationPointInMesh,
                lastPossibleNetworkLocation.Chainage,
                lastPossibleNetworkLocation.Name,
                lastNetworkLocation.Name).Replace("\r\n", " ").Replace("\t\t", " ");
            Assert.That(validationIssue.Message, Is.EqualTo(message));
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
    }
}