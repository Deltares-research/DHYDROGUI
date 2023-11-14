using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.Grid.Validation;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
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
    }
}