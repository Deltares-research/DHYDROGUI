using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.TestUtils;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMModelSynchronizeTest
    {
        [Test]
        public void GivenWaterFlowFMModel_ChangingConnectionLength_ShouldMoveComputationPoint()
        {
            //Arrange
            var manhole1 = new Manhole();
            var manhole2 = new Manhole();

            var compartment1 = new Compartment();
            var compartment2 = new Compartment();

            manhole1.Compartments.Add(compartment1);
            manhole2.Compartments.Add(compartment2);

            var connection1 = new SewerConnection { Name = "Con1", SourceCompartment = compartment1, TargetCompartment = compartment2, Length = 10 };
            
            var model = new WaterFlowFMModel();
            var network = model.Network;

            network.Nodes.AddRange(new[] { manhole1, manhole2 });
            network.Branches.AddRange(new[] { connection1 });

            // Act && Assert
            var discretization = model.NetworkDiscretization;
            Assert.AreEqual(2, discretization.Locations.Values.Count);
            Assert.AreEqual(0, discretization.Locations.Values[0].Chainage);
            Assert.AreEqual(10, discretization.Locations.Values[1].Chainage);

            connection1.Length = 20;
            Assert.AreEqual(2, discretization.Locations.Values.Count);
            Assert.AreEqual(0, discretization.Locations.Values[0].Chainage);
            Assert.AreEqual(20, discretization.Locations.Values[1].Chainage);
        }

        [Test]
        public void GivenWaterFlowFMModel_AddingOrRemovingConnections_ShouldAddOrRemoveMissingComputationPoints()
        {
            //Arrange
            var model = new WaterFlowFMModel();
            var network = model.Network;
            var discretization = model.NetworkDiscretization;
            var branches = network.Branches;
            
            // Act
            network.AddSimpleUrbanNetwork();

            var connection1 = branches[0];
            var connection2 = branches[1];
            var connection3 = branches[2];
            var connection4 = branches[3];

            Assert.AreEqual(5, discretization.Locations.Values.Count, "One computation node for each connected compartment");

            Assert.AreEqual(new NetworkLocation(connection1, 0), discretization.Locations.Values[0]);
            Assert.AreEqual(new NetworkLocation(connection1, connection1.Length), discretization.Locations.Values[1]);
            Assert.AreEqual(new NetworkLocation(connection2, connection2.Length), discretization.Locations.Values[2]);
            Assert.AreEqual(new NetworkLocation(connection3, connection3.Length), discretization.Locations.Values[3]);
            Assert.AreEqual(new NetworkLocation(connection4, connection4.Length), discretization.Locations.Values[4]);
            
            network.Branches.Remove(connection1);

            // Assert
            Assert.AreEqual(4, discretization.Locations.Values.Count, "One computation node for each connected compartment");
            Assert.AreEqual(new NetworkLocation(connection2, 0), discretization.Locations.Values[0]);
            Assert.AreEqual(new NetworkLocation(connection2, connection2.Length), discretization.Locations.Values[1]);
            Assert.AreEqual(new NetworkLocation(connection3, connection3.Length), discretization.Locations.Values[2]);
            Assert.AreEqual(new NetworkLocation(connection4, connection4.Length), discretization.Locations.Values[3]);
        }
    }
}