using System;
using System.Collections.Generic;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Converters.WellKnownText;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    public class NetworkSideViewDataTestHelper
    {
        public static void AssertMinMaxIsUpdatedForStructure(int expectedZMin, int expectedZMax, IStructure1D weir)
        {
            var data = CreateDefaultViewData();
            var network = data.Network;

            //add a composite structure
            var compositeBranchStructure = new CompositeBranchStructure();
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);

            //action! add a weir
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);

            //notice the max is crestlevel. a margin is added on the table axis.
            Assert.AreEqual((object) expectedZMax, data.ZMaxValue);
            Assert.AreEqual((object) expectedZMin, data.ZMinValue);
        }

        public static NetworkSideViewDataController CreateDefaultViewData()
        {
            return CreateDefaultViewData(true);
        }

        public static NetworkSideViewDataController CreateDefaultViewData(bool createCrossSections)
        {
            // create network
            var network = new HydroNetwork();

            var node1 = new HydroNode("node1") {Geometry = new Point(0, 0)};
            var node2 = new HydroNode("node2") { Geometry = new Point(0, 0) };
            var node3 = new HydroNode("node3") { Geometry = new Point(0, 0) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node2) { Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"), Id = 1 };
            var branch2 = new Channel("branch2", node2, node3) { Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 300 0)"), Id = 2 };

            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            if (createCrossSections)
            {
                AddCrossSections(branch1, branch2);    
            }

            // create route
            var route = new Route { Network = network, SegmentGenerationMethod = SegmentGenerationMethod.RouteBetweenLocations };
            route[new NetworkLocation(network.Branches[0], 10.0)] = 1.0;
            route[new NetworkLocation(network.Branches[1], 90.0)] = 3.0;

            // create water level network coverage
            var waterLevel = new NetworkCoverage { Network = network };
            waterLevel[new NetworkLocation(network.Branches[0], 0.0)] = 18.0;
            waterLevel[new NetworkLocation(network.Branches[0], 20.0)] = 16.0;
            waterLevel[new NetworkLocation(network.Branches[1], 50.0)] = 11.0;
            waterLevel.Arguments[0].Name = "x";
            waterLevel.Components[0].Name = "y";
            waterLevel.Name = NetworkSideViewDataController.WaterLevelCoverageNameInMapFile;

            return new NetworkSideViewDataController(route, new NetworkSideViewCoverageManager(route, null, new[] { waterLevel }));
        }

        public static void AddCrossSections(Channel branch1, Channel branch2)
        {
            var yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 18.0),
                                        new Coordinate(100.0, 18.0),
                                        new Coordinate(150.0, 10.0),
                                        new Coordinate(300.0, 10.0),
                                        new Coordinate(350.0, 18.0),
                                        new Coordinate(500.0, 18.0)
                                    };

            var cs1 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch1, 20.0, yzCoordinates);

            yzCoordinates = new List<Coordinate>
                                {
                                    new Coordinate(0.0, 14.0),
                                    new Coordinate(100.0, 14.0),
                                    new Coordinate(150.0, 8.0),
                                    new Coordinate(300.0, 8.0),
                                    new Coordinate(350.0, 14.0),
                                    new Coordinate(500.0, 14.0)
                                };

            var cs2 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch2, 50.0,yzCoordinates);
        }

        public static INetworkCoverage CreateTimeDependentWaterLevelCoverageInNetCdf(INetwork network, int timeSteps, string ncName=null)
        {
            List<DateTime> times = GetTimes(timeSteps);

            var waterLevel = new NetworkCoverage { Network = network, IsTimeDependent = true };
            var store = new NetCdfFunctionStore();
            string path = ncName ?? TestHelper.GetCurrentMethodName() + ".nc";
            store.CreateNew(path);
            var networkLocations = new[]
                                       {
                                           new NetworkLocation(network.Branches[0], 0),
                                           new NetworkLocation(network.Branches[0], 20),
                                           new NetworkLocation(network.Branches[1], 50)
                                       };
            waterLevel.Locations.FixedSize = 3;
            waterLevel.SetLocations(networkLocations);

            store.Functions.Add(waterLevel);
            var startTime = new DateTime(2000, 1, 1);
            for (int i = 0; i < times.Count; i++)
            {
                //write using indexes is a lot faster than using variablevaluefilters..find a nice way to get it in the coverage interface.
                var time = startTime.AddMinutes(i);
                waterLevel.Time.AddValues(new[] { time });
                var locationsFilter = new VariableIndexRangeFilter(
                    waterLevel.Locations, 0,2);
                var timeFilter = new VariableIndexRangeFilter(
                    waterLevel.Time, i, i);

                waterLevel.SetValues(new[] { 18.0, 16.0, 11.0 }, new []{ locationsFilter, timeFilter });
            }

            return waterLevel;
        }

        public static List<DateTime> GetTimes(int timeSteps)
        {
            var times = new List<DateTime>();
            var time = DateTime.Now;
            times.Add(time);
            for (int i = 1; i < timeSteps; i++)
            {
                times.Add(time.AddSeconds(i));
            }
            return times;
        }
    }
}