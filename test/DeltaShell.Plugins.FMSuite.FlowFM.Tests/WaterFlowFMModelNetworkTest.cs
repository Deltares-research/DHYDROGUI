using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

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
            Assert.That(model.Network.Branches.Count(), Is.EqualTo(0));
            Assert.That(model.Network.Name, Is.EqualTo("Network"));
            
            //check eventing
            var colChangedCount = 0;
            var propChangedCount = 0;
            var network = new HydroNetwork {Name = "OtherNetwork"};
            
            //enable eventing
            ((INotifyCollectionChange) network).CollectionChanged += (s, a) => colChangedCount++;
            ((INotifyPropertyChanged) network).PropertyChanged += (s, a) => propChangedCount++;
            model.Network = network;
            var node = new HydroNode();
            model.Network.Nodes.Add(node);
            Assert.That(colChangedCount, Is.EqualTo(1));
            node.Name = "myNodeName";
            model.Network.Name = "myNetworkName";
            Assert.That(propChangedCount, Is.EqualTo(2));

            //create new network so old eventing should be removed
            TypeUtils.SetPrivatePropertyValue(model,"OutputIsEmpty", false);
            Assert.IsFalse(model.OutputIsEmpty);
            model.Network = new HydroNetwork();
            Assert.IsTrue(model.OutputIsEmpty);
            model.Network.Nodes.Add(new HydroNode());
            Assert.That(colChangedCount, Is.EqualTo(1)); // old event handler should not be fired!
            model.Network.Name = "new";
            Assert.That(propChangedCount, Is.EqualTo(2)); // old event handler should not be fired!
        }
    }
}