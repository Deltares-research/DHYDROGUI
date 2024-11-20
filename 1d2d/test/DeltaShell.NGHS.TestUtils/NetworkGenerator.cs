using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.NGHS.TestUtils
{
    public static class NetworkGenerator
    {
        /// <summary>
        /// Adds the following simple urban network
        /// c1 (m1)
        ///  |
        ///  |
        ///  |
        ///  V con 1
        ///  |
        ///  |
        ///  |
        /// c2 (m2) -------->-------- c4 (m3)
        ///  |           con2
        ///  V con 3
        ///  |
        /// c3 (m2)
        ///  |
        ///  |
        ///  |
        ///  V con 4
        ///  |
        ///  |
        ///  |
        /// c5 (m4)
        ///
        /// c = compartment
        /// m = manhole
        /// con = sewer connection
        /// </summary>
        public static void AddSimpleUrbanNetwork(this IHydroNetwork hydroNetwork)
        {
            var manhole1 = new Manhole { Name = "manhole1" };
            var manhole2 = new Manhole { Name = "manhole2" };
            var manhole3 = new Manhole { Name = "manhole3" };
            var manhole4 = new Manhole { Name = "manhole4" };

            var compartment1 = new Compartment { Name = "compartment1" };
            var compartment2 = new Compartment { Name = "compartment2" };
            var compartment3 = new Compartment { Name = "compartment3" };
            var compartment4 = new Compartment { Name = "compartment4" };
            var compartment5 = new Compartment { Name = "compartment5" };

            manhole1.Compartments.Add(compartment1);
            manhole2.Compartments.Add(compartment2);
            manhole2.Compartments.Add(compartment3);
            manhole3.Compartments.Add(compartment4);
            manhole4.Compartments.Add(compartment5);

            var connection1 = new SewerConnection { Name = "Con1", SourceCompartment = compartment1, TargetCompartment = compartment2, Length = 10 };
            var connection2 = new SewerConnection { Name = "Con2", SourceCompartment = compartment2, TargetCompartment = compartment4, Length = 12 };
            var connection3 = new SewerConnection { Name = "Con3", SourceCompartment = compartment2, TargetCompartment = compartment3, Length = 1 };
            var connection4 = new SewerConnection { Name = "Con4", SourceCompartment = compartment3, TargetCompartment = compartment5, Length = 14 };

            hydroNetwork.Nodes.AddRange(new[] { manhole1, manhole2, manhole3, manhole4 });
            hydroNetwork.Branches.AddRange(new[] { connection1, connection2, connection3, connection4 });
        }

        public static IDiscretization DiscretizationForSimpleUrbanNetwork()
        {
            var network = new HydroNetwork();
            network.AddSimpleUrbanNetwork();

            var discretization = new Discretization {Network = network};

            foreach (var branch in network.Branches)
            {
                discretization.Locations.Values.Add(new NetworkLocation(branch, 0));
                discretization.Locations.Values.Add(new NetworkLocation(branch, branch.Length));
            }

            discretization.UpdateNetworkLocations(discretization.Locations.Values, false);

            return discretization;
        }
    }
}