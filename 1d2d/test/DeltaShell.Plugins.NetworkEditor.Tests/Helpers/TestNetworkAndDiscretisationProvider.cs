using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Helpers
{
    public static class TestNetworkAndDiscretisationProvider
    {
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
