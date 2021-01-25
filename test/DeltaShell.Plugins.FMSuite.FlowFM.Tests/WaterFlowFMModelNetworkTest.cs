using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using GeoAPI.Geometries;
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
            var sewerConnection = new SewerConnection {Length = 100, Geometry = new LineString(new[]{ new Coordinate(0, 0), new Coordinate(0, 100) })};

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
            var sewerConnection = new SewerConnection { Length = 100, Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) }) };

            network.Branches.Add(sewerConnection);
            network.Branches.Remove(sewerConnection);

            var discretizationLocations = discretization.Locations.Values;
            Assert.That(discretizationLocations.Count(l => l.Branch.Equals(sewerConnection)), Is.EqualTo(0));
        }
    }
}