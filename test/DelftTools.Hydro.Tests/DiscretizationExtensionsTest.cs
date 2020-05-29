using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class DiscretizationExtensionsTest
    {
        [Test]
        public void GivenDiscretization_GetDuplicatePointsOnManholes_ShouldReturnTheCorrectDuplicates()
        {

            // c1 (m1)
            //  |
            //  |
            //  |
            //  V con 1
            //  |
            //  |
            //  |
            // c2 (m2) -------->-------- c4 (m3)
            //  |           con2
            //  V con 3
            //  |
            // c3 (m2)
            //  |
            //  |
            //  |
            //  V con 4
            //  |
            //  |
            //  |
            // c5 (m4)
            //
            // c = compartment
            // m = manhole
            // con = sewer connection

            //Arrange
            var manhole1 = new Manhole();
            var manhole2 = new Manhole();
            var manhole3 = new Manhole(); 
            var manhole4 = new Manhole();

            var compartment1 = new Compartment();
            var compartment2 = new Compartment();
            var compartment3 = new Compartment();
            var compartment4 = new Compartment();
            var compartment5 = new Compartment();

            manhole1.Compartments.Add(compartment1);
            manhole2.Compartments.Add(compartment2);
            manhole2.Compartments.Add(compartment3);
            manhole3.Compartments.Add(compartment4);
            manhole4.Compartments.Add(compartment5);

            var connection1 = new SewerConnection {Name = "Con1", SourceCompartment = compartment1, TargetCompartment = compartment2, Length = 10};
            var connection2 = new SewerConnection {Name = "Con2", SourceCompartment = compartment2, TargetCompartment = compartment4, Length = 12};
            var connection3 = new SewerConnection {Name = "Con3", SourceCompartment = compartment2, TargetCompartment = compartment3, Length = 1 };
            var connection4 = new SewerConnection {Name = "Con4", SourceCompartment = compartment3, TargetCompartment = compartment5, Length = 14};

            var network = new HydroNetwork();
            network.Nodes.AddRange(new[] {manhole1, manhole2, manhole3, manhole4});
            network.Branches.AddRange(new[] { connection1, connection2, connection3, connection4});

            var discretization = new Discretization{Network = network};

            foreach (var branch in network.Branches)
            {
                discretization.Locations.Values.Add(new NetworkLocation(branch, 0));
                discretization.Locations.Values.Add(new NetworkLocation(branch, branch.Length));
            }

            // Act
            var duplicates = discretization.GetDuplicatePointsOnManholes().ToList();

            // Assert
            Assert.AreEqual(3, duplicates.Count);
            Assert.AreEqual(new NetworkLocation(connection2, 0), duplicates[0]);
            Assert.AreEqual(new NetworkLocation(connection3, 0), duplicates[1]);
            Assert.AreEqual(new NetworkLocation(connection4, 0), duplicates[2]);
        }
    }
}