using System.Linq;
using DelftTools.Hydro.Structures;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class ManholeExtensionsTest
    {
        [Test]
        public void NetworkWithManholeAndConnections_ReturnOnlyInternalConnections()
        {
            var manhole = new Manhole("manhole 1");
            var manhole2 = new Manhole("manhole 2");
            var network = new HydroNetwork();

            manhole.Network = network;
            manhole2.Network = network;
            network.Nodes.Add(manhole);
            network.Nodes.Add(manhole2);

            const string compartment1Name = "cmp1_1";
            const string compartment2Name = "cmp1_2";
            const string compartment3Name = "cmp2_1";
            var pipe = new Pipe
            {
                Source = manhole,
                Target = manhole2,
                TargetCompartment = manhole2.GetCompartmentByName(compartment3Name),
                SourceCompartment = manhole.GetCompartmentByName(compartment2Name)
            };

            network.Branches.Add(pipe);
            
            SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole, compartment1Name);
            SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole, compartment2Name);
            SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole2, compartment3Name);
            Assert.AreEqual(2, manhole.Compartments.Count);
            Assert.AreEqual(1, manhole2.Compartments.Count);

            var internalConnection = SewerFactory.CreateNewInternalConnection(manhole);
            internalConnection.SourceCompartment = manhole.GetCompartmentByName(compartment1Name);
            internalConnection.TargetCompartment = manhole.GetCompartmentByName(compartment1Name);

            network.Branches.Add(internalConnection);

            var connectionsInManhole = manhole.InternalConnections().ToList();
            Assert.AreEqual(1, connectionsInManhole.Count);
            Assert.AreEqual(internalConnection, connectionsInManhole[0]);
        }

        [Test]
        public void ManholeWithInternalStructures_GetStructuresWithoutComposite()
        {
            var manhole = new Manhole("manhole 1");
            var network = new HydroNetwork();
            manhole.Network = network;
            var internalConnection = SewerFactory.CreateConnectionWithStructure<Pump>(manhole);
            network.Branches.Add(internalConnection);

            var internalConnections = manhole.InternalConnections().ToList();
            Assert.AreEqual(1, internalConnections.Count);
            Assert.AreEqual(2, internalConnections[0].BranchFeatures.Count);

            var internalStructures = manhole.InternalStructures().ToList();
            Assert.AreEqual(1, internalStructures.Count);
        }
    }
}