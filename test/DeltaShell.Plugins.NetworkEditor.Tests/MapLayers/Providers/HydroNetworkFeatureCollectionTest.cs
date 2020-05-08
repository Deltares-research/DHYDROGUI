using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Providers
{
    [TestFixture]
    public class HydroNetworkFeatureCollectionTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Type 'System.Int32' is not a IFeature.")]
        public void ThrowExceptionWithImpossibleFeatureType()
        {
            var channelFeatureCollection = new HydroNetworkFeatureCollection {FeatureType = typeof(int)};
        }

        /// <summary>
        /// Test the featurecollections for all network types. featurecollections are used as DataSource for layers
        /// in the maptool.
        /// </summary>
        [Test]
        public void CreateForAllNetworkTypes()
        {
            //create a network containing one of everything
            var network = new HydroNetwork();
            network.Nodes.Add(new HydroNode());
            network.Branches.Add(new Channel());
            network.Branches[0].BranchFeatures.Add(new Pump());
            network.Branches[0].BranchFeatures.Add(new Weir());
            network.Branches[0].BranchFeatures.Add(new Bridge());
            network.Branches[0].BranchFeatures.Add(new Gate());

            HydroNetworkFeatureCollection channelFeatureCollection = GetHydroNetworkFeatureCollection<Channel>(network);
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<HydroNode>(network);
            HydroNetworkFeatureCollection pumpFeatureCollection = GetHydroNetworkFeatureCollection<Pump>(network);
            HydroNetworkFeatureCollection weirFeatureCollection = GetHydroNetworkFeatureCollection<Weir>(network);
            HydroNetworkFeatureCollection bridgeFeatureCollection = GetHydroNetworkFeatureCollection<Bridge>(network);
            HydroNetworkFeatureCollection gateFeatureCollection = GetHydroNetworkFeatureCollection<Gate>(network);

            Assert.AreEqual(1, channelFeatureCollection.Features.Count);
            Assert.AreEqual(1, hydroNodeFeatureCollection.Features.Count);

            Assert.AreEqual(1, pumpFeatureCollection.Features.Count);
            Assert.AreEqual(1, weirFeatureCollection.Features.Count);
            Assert.AreEqual(1, bridgeFeatureCollection.Features.Count);
            Assert.AreEqual(1, gateFeatureCollection.Features.Count);
        }

        private HydroNetworkFeatureCollection GetHydroNetworkFeatureCollection<T>(IHydroNetwork network)
        {
            return new HydroNetworkFeatureCollection
            {
                Network = network,
                FeatureType = typeof(T)
            };
        }

        #region Add to HydroNetworkFeatureCollection

        [Test]
        public void GivenHydroNodeHydroNetworkFeatureCollection_WhenAddingHydroNodeToFeatureCollection_ThenTheHydroNodeIsAddedToTheNetwork()
        {
            var network = new HydroNetwork();
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<HydroNode>(network);

            Assert.That(network.Nodes.Count, Is.EqualTo(0));
            hydroNodeFeatureCollection.Features.Add(new HydroNode());
            Assert.That(network.Nodes.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenChannelHydroNetworkFeatureCollection_WhenAddingChannelToFeatureCollection_ThenTheChannelIsAddedToTheNetwork()
        {
            var network = new HydroNetwork();
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<Channel>(network);

            Assert.That(network.Branches.Count, Is.EqualTo(0));
            hydroNodeFeatureCollection.Features.Add(new Channel());
            Assert.That(network.Branches.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenPumpHydroNetworkFeatureCollection_WhenAddingPumpToFeatureCollection_ThenThePumpIsAddedToTheNetwork()
        {
            var network = new HydroNetwork();
            network.Branches.Add(new Channel());
            var pump = new Pump {Branch = network.Branches[0]};
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<Pump>(network);

            Assert.That(network.Pumps.Count(), Is.EqualTo(0));
            hydroNodeFeatureCollection.Features.Add(pump);
            Assert.That(network.Pumps.Count(), Is.EqualTo(1));
        }

        #endregion

        #region Remove from HydroNetworkFeatureCollection

        [Test]
        public void GivenHydroNodeHydroNetworkFeatureCollection_WhenRemovingNodeFromFeatureCollection_ThenTheOriginalNetworkNodeIsAlsoRemovedFromNetwork()
        {
            var network = new HydroNetwork();
            network.Nodes.Add(new HydroNode());
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<HydroNode>(network);

            Assert.That(network.Nodes.Count, Is.EqualTo(1));
            hydroNodeFeatureCollection.Features.RemoveAt(0);
            Assert.That(network.Nodes.Count, Is.EqualTo(0));
        }

        [Test]
        public void GivenChannelHydroNetworkFeatureCollection_WhenRemovingChannelFromFeatureCollection_ThenTheOriginalChannelIsAlsoRemovedFromNetwork()
        {
            var network = new HydroNetwork();
            network.Branches.Add(new Channel());
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<Channel>(network);

            Assert.That(network.Branches.Count, Is.EqualTo(1));
            hydroNodeFeatureCollection.Features.RemoveAt(0);
            Assert.That(network.Branches.Count, Is.EqualTo(0));
        }

        [Test]
        public void GivenPumpHydroNetworkFeatureCollection_WhenRemovingPumpFromFeatureCollection_ThenTheOriginalPumpIsAlsoRemovedFromNetwork()
        {
            var network = new HydroNetwork();
            network.Branches.Add(new Channel());
            network.Branches[0].BranchFeatures.Add(new Pump());

            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<Pump>(network);

            Assert.That(network.Pumps.Count(), Is.EqualTo(1));
            hydroNodeFeatureCollection.Features.RemoveAt(0);
            Assert.That(network.Pumps.Count(), Is.EqualTo(0));
        }

        #endregion

        #region Insert in HydroNetworkFeatureCollection

        [Test]
        public void GivenHydroNodeHydroNetworkFeatureCollection_WhenInsertingHydroNodeInFeatureCollection_ThenTheHydroNodeIsInsertedInTheNetwork()
        {
            var network = new HydroNetwork();
            network.Nodes.Add(new HydroNode("OriginalNode"));
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<HydroNode>(network);

            Assert.That(network.Nodes.Count, Is.EqualTo(1));
            hydroNodeFeatureCollection.Features.Insert(0, new HydroNode("InsertedNode"));
            Assert.That(network.Nodes.Count, Is.EqualTo(2));
            Assert.That(network.Nodes[0].Name, Is.EqualTo("InsertedNode"));
            Assert.That(network.Nodes[1].Name, Is.EqualTo("OriginalNode"));
        }

        [Test]
        public void GivenChannelHydroNetworkFeatureCollection_WhenInsertingChannelInFeatureCollection_ThenTheChannelIsInsertedInTheNetwork()
        {
            var network = new HydroNetwork();
            network.Branches.Add(new Channel("OriginalChannel", new HydroNode(), new HydroNode(), 2));
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<Channel>(network);

            Assert.That(network.Branches.Count, Is.EqualTo(1));
            hydroNodeFeatureCollection.Features.Insert(0, new Channel("InsertedChannel", new HydroNode(), new HydroNode(), 2));
            Assert.That(network.Branches.Count, Is.EqualTo(2));
            Assert.That(network.Branches[0].Name, Is.EqualTo("InsertedChannel"));
            Assert.That(network.Branches[1].Name, Is.EqualTo("OriginalChannel"));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GivenPumpHydroNetworkFeatureCollection_WhenInsertingPumpInFeatureCollection_ThenNotSupportedExceptionIsThrown()
        {
            var network = new HydroNetwork();
            network.Branches.Add(new Channel());
            network.Branches[0].BranchFeatures.Add(new Pump());
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<Pump>(network);

            Assert.That(network.Pumps.Count, Is.EqualTo(1));
            hydroNodeFeatureCollection.Features.Insert(0, new Pump());
        }

        #endregion

        #region Replace in HydroNetworkFeatureCollection

        [Test]
        public void GivenHydroNodeHydroNetworkFeatureCollection_WhenReplacingHydroNodeInFeatureCollection_ThenTheHydroNodeIsReplacedInTheNetwork()
        {
            var network = new HydroNetwork();
            network.Nodes.Add(new HydroNode("OriginalNode"));
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<HydroNode>(network);

            Assert.That(network.Nodes.Count, Is.EqualTo(1));
            hydroNodeFeatureCollection.Features[0] = new HydroNode("ReplacingNode");
            Assert.That(network.Nodes.Count, Is.EqualTo(1));
            Assert.That(network.Nodes[0].Name, Is.EqualTo("ReplacingNode"));
        }

        [Test]
        public void GivenChannelHydroNetworkFeatureCollection_WhenReplacingChannelInFeatureCollection_ThenTheChannelIsReplacedInTheNetwork()
        {
            var network = new HydroNetwork();
            network.Branches.Add(new Channel("OriginalChannel", new HydroNode(), new HydroNode()));
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<Channel>(network);

            Assert.That(network.Branches.Count, Is.EqualTo(1));
            hydroNodeFeatureCollection.Features[0] = new Channel("ReplacingChannel", new HydroNode(), new HydroNode());
            Assert.That(network.Branches.Count, Is.EqualTo(1));
            Assert.That(network.Branches[0].Name, Is.EqualTo("ReplacingChannel"));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void GivenPumpHydroNetworkFeatureCollection_WhenReplacingPumpInFeatureCollection_ThenNotSupportedExceptionIsThrown()
        {
            var network = new HydroNetwork();
            network.Branches.Add(new Channel());
            network.Branches[0].BranchFeatures.Add(new Pump("OriginalPump"));
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<Pump>(network);

            Assert.That(network.Pumps.Count, Is.EqualTo(1));
            hydroNodeFeatureCollection.Features[0] = new Pump("ReplacingPump");
        }

        #endregion

        #region Clear HydroNetworkFeatureCollection

        [Test]
        public void GivenHydroNodeHydroNetworkFeatureCollection_WhenClearingHydroNodesFromFeatureCollection_ThenHydroNodesAreClearedFromTheNetwork()
        {
            var network = new HydroNetwork();
            network.Nodes.Add(new HydroNode("Node1"));
            network.Nodes.Add(new HydroNode("Node2"));
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<HydroNode>(network);

            Assert.That(network.Nodes.Count, Is.EqualTo(2));
            hydroNodeFeatureCollection.Features.Clear();
            Assert.That(network.Nodes.Count, Is.EqualTo(0));
        }

        [Test]
        public void GivenChannelHydroNetworkFeatureCollection_WhenClearingChannelsFromFeatureCollection_ThenChannelsAreClearedFromTheNetwork()
        {
            var network = new HydroNetwork();
            network.Branches.Add(new Channel());
            network.Branches.Add(new Channel());
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<Channel>(network);

            Assert.That(network.Branches.Count, Is.EqualTo(2));
            hydroNodeFeatureCollection.Features.Clear();
            Assert.That(network.Branches.Count, Is.EqualTo(0));
        }

        [Test]
        public void GivenPumpHydroNetworkFeatureCollection_WhenClearingPumpsFromFeatureCollection_ThenPumpsAreClearedFromTheNetwork()
        {
            var network = new HydroNetwork();
            network.Branches.Add(new Channel());
            network.Branches[0].BranchFeatures.Add(new Pump());
            network.Branches[0].BranchFeatures.Add(new Pump());
            HydroNetworkFeatureCollection hydroNodeFeatureCollection = GetHydroNetworkFeatureCollection<Pump>(network);

            Assert.That(network.Pumps.Count, Is.EqualTo(2));
            hydroNodeFeatureCollection.Features.Clear();
            Assert.That(network.Pumps.Count, Is.EqualTo(0));
        }

        #endregion
    }
}