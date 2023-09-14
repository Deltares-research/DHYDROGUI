using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DataObjects;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Category = NUnit.Framework.CategoryAttribute;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture] 
    public class WaterFlowFMModelNetworkTest
    {
        [Test]
        public void CreateNewModelCheckNetworkStuff()
        {
            var model = new WaterFlowFMModel(); // empty model
            Assert.That(model.Network.HydroNodes.Count(), Is.EqualTo(0));
            Assert.That(model.Network.Branches.Count, Is.EqualTo(0));
            Assert.That(model.Network.Name, Is.EqualTo("network"));
            
            //check eventing
            var colChangedCount = 0;
            var propChangedCount = 0;
            var network = new HydroNetwork {Name = "OtherNetwork"};
            
            //enable eventing
            ((INotifyCollectionChange) network).CollectionChanged += (s, a) => colChangedCount++;
            ((INotifyPropertyChanged) network).PropertyChanged += (s, a) => propChangedCount++;
            model.Network = network;
            Assert.That(propChangedCount, Is.EqualTo(2));
            var node = new HydroNode();
            model.Network.Nodes.Add(node);
            Assert.That(colChangedCount, Is.EqualTo(2));
            node.Name = "myNodeName";
            model.Network.Name = "myNetworkName";
            Assert.That(propChangedCount, Is.EqualTo(4));

            //create new network so old eventing should be removed
            TypeUtils.SetPrivatePropertyValue(model,"OutputIsEmpty", false);
            Assert.IsFalse(model.OutputIsEmpty);
            model.Network = new HydroNetwork();
            Assert.IsTrue(model.OutputIsEmpty);
            model.Network.Nodes.Add(new HydroNode());
            Assert.That(colChangedCount, Is.EqualTo(2)); // old event handler should not be fired!
            model.Network.Name = "new";
            Assert.That(propChangedCount, Is.EqualTo(5)); // old event handler should not be fired!
        }

        [Test]
        public void CreateNewModelCheckNetworkDiscretization()
        {
            var model = new WaterFlowFMModel(); // empty model
            Assert.IsNotNull(model.NetworkDiscretization);
            Assert.That(model.NetworkDiscretization.Network, Is.EqualTo(model.Network));
            Assert.That(model.NetworkDiscretization.Locations.Values.Count, Is.EqualTo(0));
            Assert.That(model.NetworkDiscretization.Name, Is.EqualTo(WaterFlowFMModel.DiscretizationObjectName));
        }

        [Test]
        public void NetworkIsContainedInDirectChildren()
        {
            var model = new WaterFlowFMModel();
            var directChildren = model.GetDirectChildren();
            Assert.IsNotNull(directChildren.FirstOrDefault(c => c == model.Network));
        }

        [Test]
        public void AddingSewerConnectionToNetwork_ShouldGenerateTwoDiscretizationPoints()
        {
            var fmModel = new WaterFlowFMModel();
            var network = fmModel.Network;
            var discretization = fmModel.NetworkDiscretization;
            var manhole1 = new Manhole {Compartments = {new Compartment()}};
            var manhole2 = new Manhole {Compartments = {new Compartment()}};

            var sewerConnection = new SewerConnection
            {
                Length = 100,
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 100)
                }),
                Source = manhole1,
                Target = manhole2
            };

            network.Branches.Add(sewerConnection);

            var discretizationLocations = discretization.Locations.Values;
            Assert.That(discretizationLocations.Count(l => l.Branch.Equals(sewerConnection)), Is.EqualTo(2));
        }

        [Test]
        public void RemovingSewerConnectionFromNetwork_ShouldRemoveItsDiscretizationPoints()
        {
            var fmModel = new WaterFlowFMModel();
            var network = fmModel.Network;
            var discretization = fmModel.NetworkDiscretization;

            var manhole1 = new Manhole { Compartments = { new Compartment() } };
            var manhole2 = new Manhole { Compartments = { new Compartment() } };
            var sewerConnection = new SewerConnection
            {
                Length = 100,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) }),
                Source = manhole1,
                Target = manhole2
            };

            network.Branches.Add(sewerConnection);
            network.Branches.Remove(sewerConnection);

            var discretizationLocations = discretization.Locations.Values;
            Assert.That(discretizationLocations.Count(l => l.Branch.Equals(sewerConnection)), Is.EqualTo(0));
        }
        
        [Test]
        public void Reconnecting_SewerConnectionFromNetwork_To_A_New_Compartment_Should_Give_Correct_DiscretizationPoints()
        {
            //model: compartment 1 - sewerconnection 1 - compartment 2 - sewerconnection 2 - compartment 3
            IDiscretization discretization = CreateFmModelWith3ManholesAnd2SewerConnections(out Manhole manhole2, out SewerConnection sewerConnection1, out SewerConnection sewerConnection2);

            //Action reconnect sewer connection from compartment 2 to the new compartment 4
            var compartment4 = new Compartment() { Name = "Compartment4" };
            manhole2.Compartments.Add(compartment4);
            sewerConnection2.SourceCompartment = compartment4;
            
            //check calculation points
            var locationsSewerConnection1 = discretization.Locations.Values.Where(l => l.Branch.Equals(sewerConnection1)).ToList();
            var firstLocation = locationsSewerConnection1.First();
            var lastLocation = locationsSewerConnection1.Last();
            Assert.AreEqual(0.0,firstLocation.Chainage);
            Assert.AreEqual(sewerConnection1.Length,lastLocation.Chainage);
            
            var locationsSewerConnection2 = discretization.Locations.Values.Where(l => l.Branch.Equals(sewerConnection2)).ToList();
            Assert.AreEqual(2,locationsSewerConnection2.Count());
            firstLocation = locationsSewerConnection2.First();
            lastLocation = locationsSewerConnection2.Last();
            Assert.AreEqual(0.0,firstLocation.Chainage);
            Assert.AreEqual(sewerConnection2.Length,lastLocation.Chainage);
        }

        [Test]
        public void Reconnecting_SewerConnectionFromNetwork_To_An_Existing_Compartment_Should_Give_Correct_DiscretizationPoints()
        {
            //model: compartment 1 - sewerconnection 1 - compartment 2 - compartment 3 - sewerconnection 2 - compartment 4
            //                                                |--- manhole 2 ---|
            IDiscretization discretization = CreateFmModelWith3Manholes2SewerConnectionsConnectedToDifferentCompartmentsInSecondManhole(out Compartment compartment2, out SewerConnection sewerConnection1, out SewerConnection sewerConnection2);

            //Action reconnect sewer connection 2 from compartment 3 to compartment 2
            sewerConnection2.SourceCompartment = compartment2;
            
            //check calculation points
            var locationsSewerConnection1 = discretization.Locations.Values.Where(l => l.Branch.Equals(sewerConnection1)).ToList();
            var firstLocation = locationsSewerConnection1.First();
            var lastLocation = locationsSewerConnection1.Last();
            Assert.AreEqual(0.0,firstLocation.Chainage);
            Assert.AreEqual(sewerConnection1.Length,lastLocation.Chainage);
            
            var locationsSewerConnection2 = discretization.Locations.Values.Where(l => l.Branch.Equals(sewerConnection2)).ToList();
            Assert.AreEqual(1,locationsSewerConnection2.Count());
            var location = locationsSewerConnection2.First();
            Assert.AreEqual(sewerConnection2.Length,location.Chainage);

        }
        
        [Test]
        public void AddingLateralSourceToManholeDownstreamOfSewerConnection_ShouldCreateLateralSourcesDataWithCorrectManholeSet()
        {
            // Setup
            using (var fmModel = new WaterFlowFMModel())
            {
                var upstreamManhole = new Manhole("upstream manhole");
                upstreamManhole.Compartments.Add(new Compartment("upstream compartment"));

                var downstreamManhole = new Manhole("downstream manhole");
                var downStreamCompartment = new Compartment("downstream compartment");
                downstreamManhole.Compartments.Add(downStreamCompartment);

                var sewerConnection = new SewerConnection("sewerconnection");
                sewerConnection.Source = upstreamManhole;
                sewerConnection.Target = downstreamManhole;

                fmModel.Network.Nodes.Add(upstreamManhole);
                fmModel.Network.Nodes.Add(downstreamManhole);
                fmModel.Network.Branches.Add(sewerConnection);

                var lateralSource = new LateralSource { Name = "lateralSource1" };

                // Call
                NetworkHelper.AddBranchFeatureToBranch(lateralSource, sewerConnection, sewerConnection.Length);

                // Assert
                Assert.That(fmModel.LateralSourcesData.Count, Is.EqualTo(1));

                Model1DLateralSourceData lateralSourcesData = fmModel.LateralSourcesData.First();
                Assert.That(lateralSourcesData.Compartment, Is.EqualTo(downStreamCompartment));
                Assert.That(lateralSourcesData.Feature, Is.EqualTo(lateralSource));
            }
        }
        #region ModelSetups
        
        
        private IDiscretization CreateFmModelWith3ManholesAnd2SewerConnections(out Manhole manhole2, out SewerConnection sewerConnection1, out SewerConnection sewerConnection2)
        {
            //initialize test
            var fmModel = new WaterFlowFMModel();
            var network = fmModel.Network;
            var discretization = fmModel.NetworkDiscretization;

            var manhole1 = new Manhole
            {
                Name = "Manhole1",
                Compartments = { new Compartment() { Name = "Compartment1" } }
            };
            manhole2 = new Manhole
            {
                Name = "Manhole2",
                Compartments = { new Compartment() { Name = "Compartment2" } }
            };
            var manhole3 = new Manhole
            {
                Name = "Manhole3",
                Compartments = { new Compartment() { Name = "Compartment3" } }
            };
            sewerConnection1 = new SewerConnection
            {
                Name = "SewerConnection1",
                Length = 100,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) }),
                Source = manhole1,
                Target = manhole2
            };

            sewerConnection2 = new SewerConnection
            {
                Name = "SewerConnection2",
                Length = 200,
                Geometry = new LineString(new[] { new Coordinate(0, 100), new Coordinate(0, 300) }),
                Source = manhole2,
                Target = manhole3
            };

            network.Branches.Add(sewerConnection1);
            network.Branches.Add(sewerConnection2);

            //check assumption
            SewerConnection connection = sewerConnection1;
            var locationsSewerConnection1 = discretization.Locations.Values.Where(l => l.Branch.Equals(connection)).ToList();
            var firstLocation = locationsSewerConnection1.First();
            var lastLocation = locationsSewerConnection1.Last();
            Assert.AreEqual(0.0, firstLocation.Chainage);
            Assert.AreEqual(sewerConnection1.Length, lastLocation.Chainage);

            SewerConnection connection2 = sewerConnection2;
            var locationsSewerConnection2 = discretization.Locations.Values.Where(l => l.Branch.Equals(connection2)).ToList();
            Assert.AreEqual(1, locationsSewerConnection2.Count());
            var location = locationsSewerConnection2.First();
            Assert.AreEqual(sewerConnection2.Length, location.Chainage);
            return discretization;
        }
        
        private IDiscretization CreateFmModelWith3Manholes2SewerConnectionsConnectedToDifferentCompartmentsInSecondManhole(out Compartment compartment2, out SewerConnection sewerConnection1, out SewerConnection sewerConnection2)
        {
            INetworkLocation firstLocation;
            INetworkLocation lastLocation;
            INetworkLocation location;

            var nameSecondSewerConnection = "SewerConnection2";
            var fmModel = new WaterFlowFMModel();
            var network = fmModel.Network;
            var discretization = fmModel.NetworkDiscretization;

            compartment2 = new Compartment() { Name = "Compartment2" };
            var compartment3 = new Compartment() { Name = "Compartment3" };

            var manhole1 = new Manhole
            {
                Name = "Manhole1",
                Compartments = { new Compartment() { Name = "Compartment1" } }
            };
            var manhole2 = new Manhole
            {
                Name = "Manhole2",
                Compartments =
                {
                    compartment2,
                    compartment3
                }
            };
            var manhole3 = new Manhole
            {
                Name = "Manhole3",
                Compartments = { new Compartment() { Name = "Compartment4" } }
            };
            sewerConnection1 = new SewerConnection
            {
                Name = "SewerConnection1",
                Length = 100,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) }),
                Source = manhole1,
                Target = manhole2,
                TargetCompartment = compartment2
            };

            sewerConnection2 = new SewerConnection
            {
                Name = nameSecondSewerConnection,
                Length = 200,
                Geometry = new LineString(new[] { new Coordinate(0, 100), new Coordinate(0, 300) }),
                Source = manhole2,
                SourceCompartment = compartment3,
                Target = manhole3
            };

            network.Branches.Add(sewerConnection1);
            network.Branches.Add(sewerConnection2);

            //check assumption
            SewerConnection connection = sewerConnection1;
            var locationsSewerConnection1 = discretization.Locations.Values.Where(l => l.Branch.Equals(connection)).ToList();
            firstLocation = locationsSewerConnection1.First();
            lastLocation = locationsSewerConnection1.Last();
            Assert.AreEqual(0.0, firstLocation.Chainage);
            Assert.AreEqual(sewerConnection1.Length, lastLocation.Chainage);

            SewerConnection connection2 = sewerConnection2;
            var locationsSewerConnection2 = discretization.Locations.Values.Where(l => l.Branch.Equals(connection2)).ToList();

            Assert.AreEqual(2, locationsSewerConnection2.Count());
            firstLocation = locationsSewerConnection2.First();
            lastLocation = locationsSewerConnection2.Last();
            Assert.AreEqual(0.0, firstLocation.Chainage);
            Assert.AreEqual(sewerConnection2.Length, lastLocation.Chainage);
            return discretization;
        }
              
        #endregion ModelSetups
    }
}