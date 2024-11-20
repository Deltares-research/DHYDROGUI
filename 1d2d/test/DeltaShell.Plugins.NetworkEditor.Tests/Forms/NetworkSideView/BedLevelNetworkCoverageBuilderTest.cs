using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap.Converters.WellKnownText;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class BedLevelNetworkCoverageBuilderTest
    {
        private INetworkCoverage route;
        private NetworkLocation location1, location2;
        private ICrossSection cs1, cs2;
        [OneTimeSetUp]
        public void FixtureSetup()
        {
            // create network
            var network = new HydroNetwork();

            var node1 = new HydroNode("node1");
            var node2 = new HydroNode("node2");
            var node3 = new HydroNode("node3");
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node2) { Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)") };
            var branch2 = new Channel("branch2", node2, node3) { Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 300 0)") };
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            
            var yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 18.0),
                                        new Coordinate(100.0, 18.0),
                                        new Coordinate(150.0, 10.0),
                                        new Coordinate(300.0, 10.0),
                                        new Coordinate(350.0, 18.0),
                                        new Coordinate(500.0, 20.0)
                                    };

            cs1 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch1, 20.0, yzCoordinates);

            yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 16.0),
                                        new Coordinate(100.0, 14.0),
                                        new Coordinate(150.0, 8.0),
                                        new Coordinate(300.0, 8.0),
                                        new Coordinate(350.0, 14.0),
                                        new Coordinate(500.0, 14.0)
                                    }; 
            
            cs2 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch2, 50.0, yzCoordinates);

            // create route
            route = new NetworkCoverage { Network = network, SegmentGenerationMethod = SegmentGenerationMethod.RouteBetweenLocations };
            location1 = new NetworkLocation(network.Branches[0], 10.0);
            location2 = new NetworkLocation(network.Branches[1], 90.0);
            route[location1] = 1.0;
            route[location2] = 3.0;
        }

        [Test]
        public void BuilderThrowsExceptionWhenRouteIsNull()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Route route = null;
                BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(route);
            });
        }

        [Test]
        public void BuilderThrowsExceptionWhenNetworkIsEmpty()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(new NetworkCoverage());
            });
        }

        [Test]
        public void BuilderReturnsNetworkCoverageFromValidRoute()
        {
            Assert.IsTrue(BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(route) is NetworkCoverage);
        }

        [Test]
        public void BuildAddsPointsForStartAndEndOfRoute()
        {
            Assert.IsTrue(BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(route) is NetworkCoverage);
        }

        [Test]
        public void CanExtractCorrectBedLevelValuesFromNetworkCoverage()
        {
            //location 1 and 2 and the start and end of the route.
            //the network contains 2 crosssections so we expect 4 points in total.
            var coverage = BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(route);
            Assert.AreEqual(4, coverage.Locations.Values.Count);
            Assert.AreEqual(10, coverage[location1]);
            Assert.AreEqual(10, coverage[new NetworkLocation(cs1.Branch, cs1.Chainage)]);
            Assert.AreEqual(8, coverage[new NetworkLocation(cs2.Branch, cs2.Chainage)]);
            Assert.AreEqual(8, coverage[location2]);
        }

        [Test]
        public void CanExtractCorrectLeftEmbankmentValuesFromNetwork()
        {
            var coverage = BedLevelNetworkCoverageBuilder.BuildLeftEmbankmentCoverage(route);
            Assert.AreEqual(4, coverage.Locations.Values.Count);
            Assert.AreEqual(18, coverage[location1]);
            Assert.AreEqual(18, coverage[new NetworkLocation(cs1.Branch, cs1.Chainage)]);
            Assert.AreEqual(16, coverage[new NetworkLocation(cs2.Branch, cs2.Chainage)]);
            Assert.AreEqual(16, coverage[location2]);
        }

        [Test]
        public void CanExtractCorrectRightEmbankmentValuesFromNetwork()
        {
            var coverage = BedLevelNetworkCoverageBuilder.BuildRightEmbankmentCoverage(route);
            Assert.AreEqual(4, coverage.Locations.Values.Count);
            Assert.AreEqual(20, coverage[location1]);
            Assert.AreEqual(20, coverage[new NetworkLocation(cs1.Branch, cs1.Chainage)]);
            Assert.AreEqual(14, coverage[new NetworkLocation(cs2.Branch, cs2.Chainage)]);
            Assert.AreEqual(14, coverage[location2]);
        }

        [Test]
        public void CanExtractCorrectLowestEmbankmentValuesFromNetwork()
        {
            var coverage = BedLevelNetworkCoverageBuilder.BuildLowestEmbankmentCoverage(route);
            Assert.AreEqual(4, coverage.Locations.Values.Count);
            Assert.AreEqual(18, coverage[location1]);
            Assert.AreEqual(18, coverage[new NetworkLocation(cs1.Branch, cs1.Chainage)]);
            Assert.AreEqual(14, coverage[new NetworkLocation(cs2.Branch, cs2.Chainage)]);
            Assert.AreEqual(14, coverage[location2]);
        }

        [Test]
        public void CanExtractCorrectHighestEmbankmentValuesFromNetwork()
        {
            var coverage = BedLevelNetworkCoverageBuilder.BuildHighestEmbankmentCoverage(route);
            Assert.AreEqual(4, coverage.Locations.Values.Count);
            Assert.AreEqual(20, coverage[location1]);
            Assert.AreEqual(20, coverage[new NetworkLocation(cs1.Branch, cs1.Chainage)]);
            Assert.AreEqual(16, coverage[new NetworkLocation(cs2.Branch, cs2.Chainage)]);
            Assert.AreEqual(16, coverage[location2]);
        }

        [Test]
        public void CreateCoverageBasedOnNetwork()
        {
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var firstChannel = hydroNetwork.Channels.First();

            //two crossections on the branch should result in two points in the BLC..
            CrossSectionHelper.AddCrossSection(firstChannel, 10, -15);
            CrossSectionHelper.AddCrossSection(firstChannel, 50, -10);

            var bedLevelCoverage = BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(hydroNetwork);
            
            //check the values match the cross-sections
            Assert.AreEqual(2, bedLevelCoverage.Locations.Values.Count);
            Assert.AreEqual(-15, bedLevelCoverage[new NetworkLocation(firstChannel, 10)]);
            Assert.AreEqual(-10, bedLevelCoverage[new NetworkLocation(firstChannel, 50)]);
        }

        /// <summary>
        /// N = { b1 }
        /// 
        /// r = r(nl), nl = (b, l)
        /// yb = yb(nl), nl = (b, l)
        /// </summary>
        [Test]
        public void ChangingBranchGeometryShouldUpdateCovarageLocationsCorrectlyTools7439()
        {
            // create network
            var node1 = new HydroNode("node1");
            var node2 = new HydroNode("node2");
            var branch1 = new Channel("branch1", node1, node2) { Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 30 0)") };
            var network = new HydroNetwork { Nodes = { node1, node2 }, Branches = { branch1 } };
            
            // create network coverage with route segmentation
            var routeCoverage = new NetworkCoverage { Network = network, SegmentGenerationMethod = SegmentGenerationMethod.RouteBetweenLocations };
            routeCoverage[new NetworkLocation(network.Branches[0], 10.0)] = 0.0;
            routeCoverage[new NetworkLocation(network.Branches[0], 20.0)] = 0.0;
            
            // build bed level coverage based on route
            var bedLevelCoverage = BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(routeCoverage);

            // update branch geometry
            branch1.IsLengthCustom = false;
            branch1.Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 15 0)");
            
            // asserts
            Assert.AreEqual(5, routeCoverage.Locations.Values[0].Chainage, 1e-10);
            Assert.AreEqual(10, routeCoverage.Locations.Values[1].Chainage, 1e-10);

            Assert.AreEqual(5, bedLevelCoverage.Locations.Values[0].Chainage, 1e-10);
            Assert.AreEqual(10, bedLevelCoverage.Locations.Values[1].Chainage, 1e-10);            
        }
    }
}