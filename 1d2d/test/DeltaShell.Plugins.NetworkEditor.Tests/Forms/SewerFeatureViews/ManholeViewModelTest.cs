using System.Linq;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture]
    public class ManholeViewModelTest
    {
        [Test]
        public void TestTestManhole()
        {
            var testManhole = GetManholeWithTwoCompartments();
            Assert.AreEqual(2, testManhole.Compartments.Count);
            Assert.AreNotEqual(testManhole.Compartments[0].Name, testManhole.Compartments[1].Name);
        }

        [Test]
        public void AddPumpSewerConnection_TwoCompartmentsManhole()
        {
            var testManhole = GetManholeWithTwoCompartments();
            var nBranches = testManhole.Network.Branches.Count;
            var nPumps = testManhole.Network.BranchFeatures.OfType<Pump>().Count();
            
            var manholeViewModel = new ManholeViewModel
            {
                Manhole = testManhole
            };
            manholeViewModel.AddShape(ShapeType.Pump);

            // manhole
            var internalConnections = testManhole.InternalConnections().ToList();
            Assert.AreEqual(1, internalConnections.Count);
            Assert.AreEqual(testManhole, internalConnections[0].Target);
            Assert.AreEqual(testManhole, internalConnections[0].Source);

            Assert.AreEqual(testManhole.Compartments[0], internalConnections[0].SourceCompartment);
            Assert.AreEqual(testManhole.Compartments[1], internalConnections[0].TargetCompartment);
            
            //network
            var network = manholeViewModel.Manhole.Network;
            Assert.AreEqual(nBranches + 1, network.Branches.Count);
            Assert.AreEqual(nPumps + 1, network.BranchFeatures.OfType<Pump>().Count());

            //pump
            var pump = testManhole.InternalStructures().FirstOrDefault();
            Assert.NotNull(pump);
            Assert.AreSame(pump.Branch, internalConnections[0]);
        }

        [Test]
        public void AddPumpSewerConnection_FourCompartmentsManhole()
        {
            var testManhole = GetManholeFourCompartmentsOnePump();
            var manholeViewModel = new ManholeViewModel
            {
                Manhole = testManhole
            };

            manholeViewModel.AddShape(ShapeType.Pump);

            AssertManholeAndInternalStructure<Pump>(manholeViewModel.Manhole, 1, 1);
        }

        [Test]
        public void AddWeirSewerConnection_FourCompartmentsManhole()
        {
            var testManhole = GetManholeFourCompartmentsOnePump();
            var manholeViewModel = new ManholeViewModel
            {
                Manhole = testManhole
            };

            manholeViewModel.AddShape(ShapeType.Weir);

            AssertManholeAndInternalStructure<Weir>(manholeViewModel.Manhole, 1, 0);

        }

        [Test]
        public void AddOrificeSewerConnection_FourCompartmentsManhole()
        {
            var testManhole = GetManholeFourCompartmentsOnePump();
            var manholeViewModel = new ManholeViewModel
            {
                Manhole = testManhole
            };

            manholeViewModel.AddShape(ShapeType.Orifice);

            // Test is failing because orifice is at the moment not a structure on a branch. This might become a structure
            AssertManholeAndInternalStructure<Orifice>(manholeViewModel.Manhole, 1, 0);
        }

        [Test]
        public void AddCompartmentToManhole()
        {
            var testManhole = GetManholeFourCompartmentsOnePump();
            var vm = new ManholeViewModel
            {
                Manhole = testManhole,
            };

            var numberOfCompartments = testManhole.Compartments.Count;
            Assert.AreEqual(numberOfCompartments, testManhole.Compartments.Count);

            vm.AddShape(ShapeType.Compartment);

            Assert.AreEqual(numberOfCompartments + 1, testManhole.Compartments.Count);
        }

        [Test]
        public void GivenTwoPipesAndAddedNewCompartmentToConnectingManhole_WhenDeleteNewCompartment_ThenNoExceptionAndOriginalCompartmentIsSetToPipes()
        {
            // Arrange
            var hydroNetwork = new HydroNetwork();

            var compartment1 = new Compartment("compartment1");
            var compartment2a = new Compartment("compartment2a");
            var compartment2b = new Compartment("compartment2b");
            var compartment3 = new Compartment("compartment3");

            var manhole = new Manhole("manhole");
            manhole.Compartments.Add(compartment1);

            var manhole2 = new Manhole("manhole2");
            manhole2.Compartments.Add(compartment2a);

            var manhole3 = new Manhole("manhole3");
            manhole.Compartments.Add(compartment3);

            hydroNetwork.Nodes.Add(manhole);
            hydroNetwork.Nodes.Add(manhole2);
            hydroNetwork.Nodes.Add(manhole3);

            var pipe1 = new Pipe()
            {
                SourceCompartment = compartment1,
                TargetCompartment = compartment2a,
            };

            var pipe2 = new Pipe()
            {
                SourceCompartment = compartment2a,
                TargetCompartment = compartment3,
            };
            hydroNetwork.Branches.Add(pipe1);
            hydroNetwork.Branches.Add(pipe2);


            var vm = new ManholeViewModel
            {
                Manhole = manhole2,
            };

            manhole2.Compartments.Add(compartment2b);
            pipe1.TargetCompartment = compartment2b;
            pipe2.SourceCompartment = compartment2b;

            vm.SelectedItem = compartment2b;

            // Act
            void Call() => vm.DeleteCommand.Execute(null);

            // Asserts
            Assert.That(Call, Throws.Nothing);
            Assert.That(pipe1.TargetCompartment, Is.EqualTo(compartment2a));
            Assert.That(pipe2.SourceCompartment, Is.EqualTo(compartment2a));
        }

        private static void AssertManholeAndInternalStructure<T>(Manhole manhole, int initialBranchesCount, int initialStructuresCountForType)
        {
            // manhole
            var internalConnections = manhole.InternalConnections().ToList();
            Assert.AreEqual(2, internalConnections.Count);
            Assert.AreEqual(manhole, internalConnections[1].Target);
            Assert.AreEqual(manhole, internalConnections[1].Source);

            Assert.AreEqual(manhole.Compartments[0], internalConnections[1].SourceCompartment);
            Assert.AreEqual(manhole.Compartments[3], internalConnections[1].TargetCompartment);

            //network
            var nBranches = manhole.Network.Branches.Count;
            Assert.AreEqual(initialBranchesCount + 1, nBranches);

            var nStructures = manhole.Network.BranchFeatures.OfType<T>().Count();
            Assert.AreEqual(initialStructuresCountForType + 1, nStructures);

            //structure
            var structure = manhole.InternalStructures().LastOrDefault();
            Assert.NotNull(structure);
            Assert.AreSame(structure.Branch, internalConnections[1]);
        }

        private Manhole GetManholeWithTwoCompartments()
        {
            var manhole = new Manhole("manhole 1");
            var network = new HydroNetwork();

            manhole.Network = network;
            network.Nodes.Add(manhole);

            SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole);
            SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole);
            
            return manhole;
        }

        private Manhole GetManholeFourCompartmentsOnePump()
        {
            var manhole = new Manhole("manhole 1");
            var network = new HydroNetwork();

            manhole.Network = network;
            network.Nodes.Add(manhole);
            
            var compartment1 = SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole);
            var compartment2 = SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole);
            SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole);
            SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole);

            var pumpConnection = SewerFactory.CreatePumpConnection(manhole);
            
            pumpConnection.SourceCompartment = compartment1;
            pumpConnection.TargetCompartment = compartment2;
            pumpConnection.Name = "default connection";
            network.Branches.Add(pumpConnection);

            return manhole;
        }

    }
}