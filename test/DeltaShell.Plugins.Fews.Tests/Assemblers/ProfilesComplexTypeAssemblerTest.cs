using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.Fews.Assemblers;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using nl.wldelft.util.timeseries;
using NUnit.Framework;
using SharpMap.Converters.WellKnownText;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.Fews.Tests.Assemblers
{
    // DTO = Data transfer object (see Martin Fowler: Design Patterns for Enterprise Architectures)
    [TestFixture]
    public class ProfilesComplexTypeAssemblerTest
    {
        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void Assemble_WithoutNetworkCoverage_Throws()
        {
            var assembler = new ProfilesComplexTypeAssembler();
            assembler.AssembleProfileTimeSeries(null, null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Assemble_NetworkCoverageWithoutNetwork_Throws()
        {
            var assembler = new ProfilesComplexTypeAssembler();
            assembler.AssembleProfileTimeSeries(null, null);
        }

        [Test]
        [Category(TestCategory.WorkInProgress)] // Test data is not correct (nog netw. cov. points in route).
        public void Assemble_UsingRouteContainsRightNumberOfProfiles()
        {
            // setup
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(false, new Point(0, 0), new Point(100, 0),
                                                               new Point(100, 100));

            NetworkCoverage networkCoverage = new NetworkCoverage
                                                  {
                                                      Network = network,
                                                      IsTimeDependent = true
                                                  };
            Route route = new Route();
            List<INetworkLocation> locations = new List<INetworkLocation>
                                                   {
                                                       new NetworkLocation(network.Branches[0], 20),
                                                       new NetworkLocation(network.Branches[0], 40)
                                                   };
            route.SetLocations(locations);
            var assembler = new ProfilesComplexTypeAssembler
            {
                NetworkCoverage = networkCoverage,
                Route = route
            };

            TimeSeriesArray timeSeriesArray = new TimeSeriesArray(new DefaultTimeSeriesHeader());

            // call
            assembler.AssembleProfileTimeSeries(timeSeriesArray, networkCoverage);

            // checks
            Assert.AreEqual(2, timeSeriesArray.getValueCount(), "Unexpected #profiles");
        }


        [Test]
        [Category(TestCategory.Integration)]
        public void Assemble_UsingRouteProfileHasCorrectTimeStep()
        {
            // setup                        
            var node1 = new Node { Geometry = new Point(0, 0) };
            var node2 = new Node { Geometry = new Point(100, 0) };

            const string branchName = "Branch1";
            var branch1 = new Branch
            {
                Name = branchName,
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"),
                Source = node1,
                Target = node2
            };
            var network = new Network();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(branch1);
            NetworkCoverage networkCoverage = new NetworkCoverage
                                                  {
                                                      Network = network,
                                                      IsTimeDependent = true
                                                  };

            var assembler = new ProfilesComplexTypeAssembler
            {
                NetworkCoverage = networkCoverage,
                Route = new Route { Name = "route_1" }
            };

            TimeSeriesArray timeSeriesArray = new TimeSeriesArray(new DefaultTimeSeriesHeader());
            Assert.IsTrue(timeSeriesArray.getValueCount() == 0);

            // call
            assembler.AssembleProfileTimeSeries(timeSeriesArray, networkCoverage);

            // checks
            TimeStepType timeStepType = timeSeriesArray.getHeader().getTimeStep().getType();

            Assert.AreEqual(TimeStepType.IRREGULAR, timeStepType);
        }

        [Test]
        public void Assemble_UsingRouteHasCorrectTimeEventSequence()
        {
            // setup
            var network = RouteTestHelper.GetSnakeNetwork(false, new Point(0, 0), new Point(100, 0), new Point(200, 100));
                //; var network = GetNetwork();
            var branch1 = network.Branches[0];

            var networkLocation1= new NetworkLocation(branch1, 0);
            var networkLocation2 = new NetworkLocation(branch1, 10);
            var networkLocation3 = new NetworkLocation(branch1, 50);

            var route = RouteHelper.CreateRoute(
                new[] { networkLocation1, networkLocation2, networkLocation3 });

            route.Name = "route_1";

            var networkCoverage = new NetworkCoverage
            {
                Network = network,
                IsTimeDependent = true
            };

            var time0 = new DateTime(1, 1, 1); // take fixed date so we can compare output with expected file
            var time1 = time0.AddDays(1);

            networkCoverage[time0, networkLocation1] = 1.1;
            networkCoverage[time0, networkLocation2] = 1.2;
            networkCoverage[time0, networkLocation3] = 1.3;
            networkCoverage[time1, networkLocation1] = 2.1;
            networkCoverage[time1, networkLocation2] = 2.2;
            networkCoverage[time1, networkLocation3] = 2.3;

            var assembler = new ProfilesComplexTypeAssembler
            {
                NetworkCoverage = networkCoverage,
                Route = route
            };

            TimeSeriesArray timeSeriesArray = new TimeSeriesArray(TimeSeriesArray.Type.COVERAGE, new DefaultTimeSeriesHeader());

            // call
            assembler.AssembleProfileTimeSeries(timeSeriesArray, networkCoverage);

            // checks
            // TODO: perform check when profile-serializer is available
        }
    }
}