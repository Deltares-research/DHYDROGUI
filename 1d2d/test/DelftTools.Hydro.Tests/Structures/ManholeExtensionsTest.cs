using System.Linq;
using DelftTools.Hydro.SewerFeatures;
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
            var sourceManhole = new Manhole("manhole 1");
            var targetManhole = new Manhole("manhole 2");
            const string compartment1Name = "cmp1_1";
            const string compartment2Name = "cmp1_2";
            const string compartment3Name = "cmp2_1";

            var network = new HydroNetwork();
            network.Nodes.Add(sourceManhole);
            network.Nodes.Add(targetManhole);

            SewerFactory.CreateNewCompartmentAndAddToManhole(network, sourceManhole, compartment1Name);
            SewerFactory.CreateNewCompartmentAndAddToManhole(network, sourceManhole, compartment2Name);
            SewerFactory.CreateNewCompartmentAndAddToManhole(network, targetManhole, compartment3Name);
            var pipe = new Pipe
            {
                Source = sourceManhole,
                Target = targetManhole
            };

            network.Branches.Add(pipe);

            Assert.AreEqual(2, sourceManhole.Compartments.Count);
            Assert.AreEqual(1, targetManhole.Compartments.Count);

            var internalConnection = SewerFactory.CreateNewInternalConnection(sourceManhole);
            internalConnection.SourceCompartment = sourceManhole.GetCompartmentByName(compartment1Name);
            internalConnection.TargetCompartment = sourceManhole.GetCompartmentByName(compartment1Name);

            network.Branches.Add(internalConnection);

            var connectionsInManhole = sourceManhole.InternalConnections().ToList();
            Assert.AreEqual(1, connectionsInManhole.Count);
            Assert.AreEqual(internalConnection, connectionsInManhole[0]);
        }

        [Test]
        public void ManholeWithInternalStructures_GetStructuresWithoutComposite()
        {
            var manhole = new Manhole("myManhole");
            manhole.Compartments.Add(new Compartment());
            var network = new HydroNetwork();
            manhole.Network = network;
            var internalConnection = SewerFactory.CreatePumpConnection(manhole);
            network.Branches.Add(internalConnection);

            var internalConnections = manhole.InternalConnections().ToList();
            Assert.AreEqual(1, internalConnections.Count);
            Assert.AreEqual(2, internalConnections[0].BranchFeatures.Count);

            var internalStructures = manhole.InternalStructures().ToList();
            Assert.AreEqual(1, internalStructures.Count);
        }
    }
}