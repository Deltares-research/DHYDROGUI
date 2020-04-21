using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Helpers
{
    public static class TestNetworkAndDiscretisationProvider
    {
        public static IDiscretization CreateNetworkAndDiscretisation()
        {
            var network = new HydroNetwork { Name = "network" };
            var hydroNode1 = new HydroNode { Name = "my Node 1", LongName = "node 1 description", Description = "node 1 description", Geometry = new Point(-187.96667, 720.81667), Network = network };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode { Name = "my Node 2", LongName = "node 2 description", Description = "node 2 description", Geometry = new Point(2195.7333, 708.71667), Network = network };
            network.Nodes.Add(hydroNode2);
            var hydroNode3 = new HydroNode { Name = "my Node 3", LongName = "node 3 description", Description = "node 3 description", Geometry = new Point(4071.4928, 690.94861), Network = network };
            network.Nodes.Add(hydroNode3);
            var hydroNode4 = new HydroNode { Name = "my Node  4", LongName = "node 4 description", Description = "node 4 description", Geometry = new Point(3445.4246, 1540.1838), Network = network };
            network.Nodes.Add(hydroNode4);

            var branch1 = new Branch
            {
                Name = "my Branch 1",
                Description = "branch 1 description",
                Network = network,
                Source = hydroNode1,
                Target = hydroNode2,
                Geometry = new LineString(new[]
                {
                    new Coordinate(-187.96667, 720.81667),
                    new Coordinate(187.13333, 1039.45),
                    new Coordinate(828.43333, 861.98333),
                    new Coordinate(1219.6667, 406.21667),
                    new Coordinate(1712.2164, 273.32123),
                    new Coordinate(2094.9, 547.38333),
                    new Coordinate(2195.7333, 708.71667)
                })
            };
            network.Branches.Add(branch1);

            var branch2 = new Branch
            {
                Name = "my Branch 2",
                Description = "branch 2 description",
                Network = network,
                Source = hydroNode2,
                Target = hydroNode3,
                Geometry = new LineString(new[]
                {
                    new Coordinate(2195.7333, 708.71667),
                    new Coordinate(2577.8276, 567.00618),
                    new Coordinate(3235.6759, 576.54021),
                    new Coordinate(4071.4928, 690.94861)
                })
            };
            network.Branches.Add(branch2);

            var branch3 = new Branch
            {
                Name = "my Branch 3",
                Description = "branch 3 description",
                Network = network,
                Source = hydroNode2,
                Target = hydroNode4,
                Geometry = new LineString(new[]
                {
                    new Coordinate(2195.7333, 708.71667),
                    new Coordinate(2739.9061, 938.83347),
                    new Coordinate(3226.4949, 1367.1587),
                    new Coordinate(3445.4246, 1540.1838)
                })
            };
            network.Branches.Add(branch3);

            var networkDiscretisation = new Discretization
            {
                Name = "mesh1d",
                Network = network
            };

            // Branch 1
            // add source node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 0) {Name = "point_1"});
            // add calculation points
            var location1 = new NetworkLocation(branch1, 500) { Name = "point_2" };
            networkDiscretisation.Locations.Values.Add(location1);
            var location2 = new NetworkLocation(branch1, 1000) { Name = "point_3" };
            networkDiscretisation.Locations.Values.Add(location2);
            var location3 = new NetworkLocation(branch1, 1500) { Name = "point_4" };
            networkDiscretisation.Locations.Values.Add(location3);
            var location4 = new NetworkLocation(branch1, 2000) { Name = "point_5" };
            networkDiscretisation.Locations.Values.Add(location4);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 2500) { Name = "point_6" });

            // Branch3
            // add calculation points
            var location5 = new NetworkLocation(branch3, 700) { Name = "point_7" };
            networkDiscretisation.Locations.Values.Add(location5);
            var location6 = new NetworkLocation(branch3, 1400) { Name = "point_8" };
            networkDiscretisation.Locations.Values.Add(location6);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch3, 2100) { Name = "point_9" });

            // Branch 2
            // add calculation points
            var location7 = new NetworkLocation(branch2, 400) { Name = "point_10" };
            networkDiscretisation.Locations.Values.Add(location7);
            var location8 = new NetworkLocation(branch2, 800) { Name = "point_11" };
            networkDiscretisation.Locations.Values.Add(location8);
            var location9 = new NetworkLocation(branch2, 1200) { Name = "point_12" };
            networkDiscretisation.Locations.Values.Add(location9);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch2, 1600) { Name = "point_13" });

            return networkDiscretisation;
        }

        public static IDiscretization CreateSimpleNetworkAndDiscretisation()
        {
            var network = new HydroNetwork { Name = "network" };
            var hydroNode1 = new HydroNode { Name = "my Node1", Geometry = new Point(1, 4), Network = network, LongName = "my node 1 description" , Description = "my node 1 description" };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode { Name = "myNode2", Geometry = new Point(5, 1), Network = network, LongName = "my node 2 description" , Description = "my node 2 description" };
            network.Nodes.Add(hydroNode2);
            var branch1 = new Branch
            {
                Name = "my Branch 1",
                Description = "my branch description",
                Network = network,
                Source = hydroNode1,
                Target = hydroNode2,
                Geometry = new LineString(new[]
                {
                    new Coordinate(1, 4),
                    new Coordinate(6, 12),
                    new Coordinate(5, 1)
                })
            };
            network.Branches.Add(branch1);

            var networkDiscretisation = new Discretization
            {
                Name = "mesh1d",
                Network = network
            };

            // add source node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 0) {Name = "point_1", LongName = "point 1"});
            // add calculation points
            var location1 = new NetworkLocation(branch1, 1) {Name = "point_2", LongName = "point 2" };
            networkDiscretisation.Locations.Values.Add(location1);
            var location2 = new NetworkLocation(branch1, 2.5) {Name = "point_3", LongName = "point 3" };
            networkDiscretisation.Locations.Values.Add(location2);
            var location3 = new NetworkLocation(branch1, 4) { Name = "point_4", LongName = "point 4" };
            networkDiscretisation.Locations.Values.Add(location3);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 5) { Name = "point_5", LongName = "point 5" });

            return networkDiscretisation;
        }

        public static HydroNetwork CreateSimpleSewerNetwork(string pipeName)
        {
            const string sourceCompartmentName = "cmp1";
            const string targetCompartmentName = "cmp2";

            var network = new HydroNetwork();

            var manhole1 = new Manhole("manhole1") { Geometry = new Point(0, 0), Network = network };
            var manhole2 = new Manhole("manhole2") { Geometry = new Point(0, 100), Network = network };
            manhole1.Compartments.Add(new Compartment(sourceCompartmentName));
            manhole2.Compartments.Add(new Compartment(targetCompartmentName));
            network.Nodes.Add(manhole1);
            network.Nodes.Add(manhole2);

            var pipe1 = new Pipe
            {
                Name = pipeName,
                Network = network,
                SourceCompartment = manhole1.GetCompartmentByName(sourceCompartmentName),
                TargetCompartment = manhole2.GetCompartmentByName(targetCompartmentName),
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 100)
                }),
                WaterType = SewerConnectionWaterType.DryWater
            };

            network.Branches.Add(pipe1);
            return network;
        }
    }
}
