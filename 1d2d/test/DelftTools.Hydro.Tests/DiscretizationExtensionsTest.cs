using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class DiscretizationExtensionsTest
    {
        #region Update

        [Test]
        public void GivenDiscretizationExtensions_AddingPointsToExistingRuralNetworkUsingUpdateNetworkLocations_ShouldGiveUniquePoints()
        {
            //Arrange
            var network = new HydroNetwork();
            var node1 = new HydroNode("Node1");
            var node2 = new HydroNode("Node2");
            var channel1 = new Channel("Channel1", node1, node2, length:100);
            
            network.Nodes.AddRange(new []{node1, node2});
            network.Branches.Add(channel1);

            var discretization = new Discretization {Network = network};
            discretization.Locations.AddValues(new []
            {
                new NetworkLocation(channel1, 20),
                new NetworkLocation(channel1, 40),
                new NetworkLocation(channel1, 60),
                new NetworkLocation(channel1, 80)
            });

            // Act
            var newLocations = new[]
            {
                new NetworkLocation(channel1, 10),
                new NetworkLocation(channel1, 30),
                new NetworkLocation(channel1, 40),
                new NetworkLocation(channel1, 50),
                new NetworkLocation(channel1, 60),
                new NetworkLocation(channel1, 70),
                new NetworkLocation(channel1, 90),
                new NetworkLocation(channel1, 110) // outside length of branch 
            };

            TestHelper.AssertIsFasterThan(100, () => discretization.UpdateNetworkLocations(newLocations));

            // Assert
            Assert.AreEqual(9, discretization.Locations.AllValues.Count);

            var chainages = discretization.Locations.Values.Select(l => l.Chainage).OrderBy(c => c).ToArray();
            Assert.AreEqual(new double[] {10,20,30,40,50,60,70,80,90},chainages);
        }

        [Test]
        public void GivenDiscretizationExtensions_AddingPointsToExistingRuralNetworkWithConnectedBranchesUsingUpdateNetworkLocations_ShouldGiveUniquePoints()
        {
            //Arrange
            var network = new HydroNetwork();
            var node1 = new HydroNode("Node1");
            var node2 = new HydroNode("Node2");
            var node3 = new HydroNode("Node2");

            var channel1 = new Channel("Channel1", node1, node2, length: 100);
            var channel2 = new Channel("Channel2", node3, node2, length: 100);

            network.Nodes.AddRange(new[] { node1, node2 });
            network.Branches.AddRange(new[] { channel1, channel2 });

            var discretization = new Discretization { Network = network };
            discretization.Locations.AddValues(new[]
            {
                new NetworkLocation(channel1, 0),
                new NetworkLocation(channel1, 20),
                new NetworkLocation(channel1, 40),
                new NetworkLocation(channel1, 60),
                new NetworkLocation(channel1, 80),
                new NetworkLocation(channel1, 100)
            });

            // Act
            var newLocations = new[]
            {
                new NetworkLocation(channel2, 0),
                new NetworkLocation(channel2, 20),
                new NetworkLocation(channel2, 40),
                new NetworkLocation(channel2, 60),
                new NetworkLocation(channel2, 80),
                new NetworkLocation(channel2, 100)
            };

            TestHelper.AssertIsFasterThan(100, () => discretization.UpdateNetworkLocations(newLocations));

            // Assert
            Assert.AreEqual(11, discretization.Locations.AllValues.Count, "Overlapping location should be removed");

            var channel1Chaimages = discretization.GetLocationsForBranch(channel1).Select(l => l.Chainage).OrderBy(c => c).ToArray();
            var channel2Chaimages = discretization.GetLocationsForBranch(channel2).Select(l => l.Chainage).OrderBy(c => c).ToArray();
            Assert.AreEqual(new double[] { 0, 20, 40, 60, 80, 100 }, channel1Chaimages, "Channel 1 should have all locations");
            Assert.AreEqual(new double[] { 0, 20, 40, 60, 80 }, channel2Chaimages, "Channel 2 should have all locations except the one at chainage 100");
        }

        [Test]
        public void GivenDiscretization_UpdateNetworkLocationsForUrbanNetwork_ShouldReturnComputationPointForEachCompartment()
        {
            var network = new HydroNetwork();
            network.AddSimpleUrbanNetwork();
            var discretization = new Discretization{Network = network};
            
            var branches = network.Branches;
            var connection1 = branches[0];
            var connection2 = branches[1];
            var connection3 = branches[2];
            var connection4 = branches[3];

            discretization.Locations.AddValues(discretization.GenerateSewerConnectionNetworkLocations());

            // Act
            discretization.UpdateNetworkLocations(discretization.Locations.Values, false);
            
            // Assert
            Assert.AreEqual(5, discretization.Locations.Values.Count, "One computation node for each compartment");

            Assert.AreEqual(new NetworkLocation(connection1, 0), discretization.Locations.Values[0]);
            Assert.AreEqual(new NetworkLocation(connection1, connection1.Length), discretization.Locations.Values[1]);
            Assert.AreEqual(new NetworkLocation(connection2, connection2.Length), discretization.Locations.Values[2]);
            Assert.AreEqual(new NetworkLocation(connection3, connection3.Length), discretization.Locations.Values[3]);
            Assert.AreEqual(new NetworkLocation(connection4, connection4.Length), discretization.Locations.Values[4]);
        }


        [Test]
        public void GivenDiscretizationExtensions_UpdateNetworkLocations_ShouldGiveOrderedLocationsList()
        {
            //Arrange
            var network = new HydroNetwork();
            var node1 = new HydroNode("Node1");
            var node2 = new HydroNode("Node2");
            var node3 = new HydroNode("Node2");

            var channel1 = new Channel("Channel1", node1, node2, length: 100);
            var channel2 = new Channel("Channel2", node3, node2, length: 100);

            network.Nodes.AddRange(new[] { node1, node2 });
            network.Branches.AddRange(new[] { channel1, channel2 });

            var discretization = new Discretization { Network = network };
            var currentLocations = new[]
            {
                new NetworkLocation(channel1, 0),
                new NetworkLocation(channel1, 100),
                new NetworkLocation(channel1, 50)
            };

            discretization.Locations.AddValues(currentLocations);

            // Act
            var newLocations = new[]
            {
                new NetworkLocation(channel2, 100),
                new NetworkLocation(channel2, 0),
                new NetworkLocation(channel2, 50)
            };

            discretization.UpdateNetworkLocations(newLocations);

            // Assert
            Assert.Multiple(() => {
                var locations = discretization.Locations.AllValues.ToList();
                Assert.AreEqual(currentLocations[0], locations[0]);
                Assert.AreEqual(currentLocations[2], locations[1]);
                Assert.AreEqual(currentLocations[1], locations[2]);

                Assert.AreEqual(newLocations[1], locations[3]);
                Assert.AreEqual(newLocations[2], locations[4]);
                Assert.AreEqual(newLocations[0], locations[5]);
            });
        }

        #endregion

        [Test]
        public void GivenDiscretizationExtensions_ReplacePointsForRemovedBranch_ShouldCreateNewPointsOnOtherBranchesForRemovedChannels()
        {
            //Arrange
            var network = new HydroNetwork();
            var node1 = new HydroNode("Node1");
            var node2 = new HydroNode("Node2");
            var node3 = new HydroNode("Node3");

            var channel1 = new Channel("Channel1", node1, node2, length: 100);
            var channel2 = new Channel("Channel2", node2, node3, length: 100);

            network.Nodes.AddRange(new[] { node1, node2, node3 });
            network.Branches.AddRange(new []{ channel1, channel2 });

            var discretization = new Discretization { Network = network };
            discretization.Locations.AddValues(new[]
            {
                new NetworkLocation(channel1, 20),
                new NetworkLocation(channel1, 40),
                new NetworkLocation(channel1, 60),
                new NetworkLocation(channel1, 80),
                new NetworkLocation(channel2, 0),
                new NetworkLocation(channel2, 30),
                new NetworkLocation(channel2, 60),
                new NetworkLocation(channel2, 100)
            });

            // Act
            discretization.ReplacePointsForRemovedBranch(channel2);
            network.Branches.Remove(channel2);

            // Assert
            Assert.AreEqual(5, discretization.Locations.Values.Count);
        }

        [Test]
        public void GivenDiscretizationExtensions_ReplacePointsForRemovedBranch_ShouldCreateMissingPoints()
        {
            var discretization = NetworkGenerator.DiscretizationForSimpleUrbanNetwork();
            var network = discretization.Network;
            var connection1 = (ISewerConnection) network.Branches[0];
            var connection2 = (ISewerConnection) network.Branches[1];
            var connection3 = (ISewerConnection) network.Branches[2];
            var connection4 = (ISewerConnection) network.Branches[3];

            // Act
            network.Branches.Remove(connection1);
            Assert.AreEqual(3, discretization.Locations.Values.Count);

            discretization.ReplacePointsForRemovedBranch(connection1);

            // Assert
            Assert.AreEqual(4, discretization.Locations.Values.Count, "One computation node for each compartment");

            Assert.AreEqual(new NetworkLocation(connection2, 0), discretization.Locations.Values[0]);
            Assert.AreEqual(new NetworkLocation(connection2, connection2.Length), discretization.Locations.Values[1]);
            Assert.AreEqual(new NetworkLocation(connection3, connection3.Length), discretization.Locations.Values[2]);
            Assert.AreEqual(new NetworkLocation(connection4, connection4.Length), discretization.Locations.Values[3]);
        }

        [Test]
        public void GivenDiscretizationExtensions_AddMissingLocationsForSewerConnections_ShouldOnlyCreateNewLocationsWereMissing()
        {
            //Arrange
            var discretization = NetworkGenerator.DiscretizationForSimpleUrbanNetwork();
            var network = (IHydroNetwork)discretization.Network;
            var manHole1 = network.Nodes[0];
            var manHole3 = network.Nodes[2];
            var manHole5 = new Manhole("Manhole 5");
            var compartment6 = new Compartment {Name = "Compartment 6"};
            manHole5.Compartments.Add(compartment6);
            
            // connection between existing manholes (nodes)
            var newConnection1 = new SewerConnection("new connection 1")
            {
                Source = manHole1,
                Target = manHole3
            };

            // connection between an existing manhole and a new manhole
            var newConnection2 = new SewerConnection("new connection 2")
            {
                Source = manHole1,
                Target = manHole5
            };

            // Act & Assert
            Assert.AreEqual(5, discretization.Locations.Values.Count, "One computation node for each compartment");

            network.Branches.Add(newConnection1);

            discretization.AddMissingLocationsForSewerConnections(newConnection1);

            Assert.AreEqual(5, discretization.Locations.Values.Count, "No new locations should be added");
            
            network.Nodes.Add(manHole5);
            network.Branches.Add(newConnection2);

            discretization.AddMissingLocationsForSewerConnections(newConnection2);

            Assert.AreEqual(6, discretization.Locations.Values.Count, "One computation node should be added for (new) manhole 5");
        }

        
        [Test]
        public void GivenDiscretizationExtensions_ChangingCompartmentOfConnection_ShouldCreateAValidDiscretization()
        {
            //Arrange
            var discretization = NetworkGenerator.DiscretizationForSimpleUrbanNetwork();
            var network = (IHydroNetwork)discretization.Network;

            var connection1 = network.Branches.OfType<ISewerConnection>().FirstOrDefault(n => n.Name == "Con1");
            var connection2 = network.Branches.OfType<ISewerConnection>().FirstOrDefault(n => n.Name == "Con2");
            var connection3 = network.Branches.OfType<ISewerConnection>().FirstOrDefault(n => n.Name == "Con3");
            var connection4 = network.Branches.OfType<ISewerConnection>().FirstOrDefault(n => n.Name == "Con4");

            var manhole2 = network.Nodes.OfType<IManhole>().FirstOrDefault(n => n.Name == "manhole2");
            var compartment2 = manhole2?.Compartments.FirstOrDefault(c => c.Name == "compartment2");
            var compartment3 = manhole2?.Compartments.FirstOrDefault(c => c.Name == "compartment3");

            // Act
            Assert.NotNull(connection1);
            Assert.NotNull(connection2);
            Assert.NotNull(connection3);
            Assert.NotNull(connection4);
            Assert.NotNull(compartment3);

            Assert.AreEqual(5, discretization.Locations.Values.Count, "One computation node for each compartment");

            Assert.AreEqual(new NetworkLocation(connection1, 0), discretization.Locations.Values[0]);
            Assert.AreEqual(new NetworkLocation(connection1, connection1.Length), discretization.Locations.Values[1]);
            Assert.AreEqual(new NetworkLocation(connection2, connection2.Length), discretization.Locations.Values[2]);
            Assert.AreEqual(new NetworkLocation(connection3, connection3.Length), discretization.Locations.Values[3]);
            Assert.AreEqual(new NetworkLocation(connection4, connection4.Length), discretization.Locations.Values[4]);

            connection1.TargetCompartment = compartment3;
            discretization.HandleCompartmentSwitch(compartment2, compartment3);

            // Assert
            Assert.NotNull(connection1);
            Assert.NotNull(connection2);
            Assert.NotNull(connection3);
            Assert.NotNull(connection4);
            Assert.NotNull(compartment3);

            Assert.AreEqual(5, discretization.Locations.Values.Count, "One computation node for each compartment");

            Assert.AreEqual(new NetworkLocation(connection1, 0), discretization.Locations.Values[0]);
            Assert.AreEqual(new NetworkLocation(connection2, 0), discretization.Locations.Values[1]);
            Assert.AreEqual(new NetworkLocation(connection2, connection2.Length), discretization.Locations.Values[2]);
            Assert.AreEqual(new NetworkLocation(connection3, connection3.Length), discretization.Locations.Values[3]);
            Assert.AreEqual(new NetworkLocation(connection4, connection4.Length), discretization.Locations.Values[4]);
        }
    }
}