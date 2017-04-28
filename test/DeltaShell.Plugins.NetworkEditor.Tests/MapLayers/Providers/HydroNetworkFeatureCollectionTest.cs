using System;
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
            var channelFeatureCollection = new HydroNetworkFeatureCollection
                                              {
                                                  
                                                  FeatureType = typeof(int)
                                              };
            
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
            

            var channelFeatureCollection = new HydroNetworkFeatureCollection
                                              {
                                                  Network = network,
                                                  FeatureType = typeof(Channel),
                                              };
            var hydroNodeFeatureCollection = new HydroNetworkFeatureCollection
                                            {
                                                Network = network,
                                                FeatureType = typeof(HydroNode)
                                            };
            var pumpFeatureCollection = new HydroNetworkFeatureCollection
                                                     {
                                                         Network = network,
                                                         FeatureType = typeof(Pump)
                                                     };
            var weirFeatureCollection = new HydroNetworkFeatureCollection
                                                   {
                                                       Network = network,
                                                       FeatureType = typeof(Weir)
                                                   };
            var bridgeFeatureCollection = new HydroNetworkFeatureCollection
            {
                Network = network,
                FeatureType = typeof(Bridge)
            };
            
            Assert.AreEqual(1, channelFeatureCollection.Features.Count);
            Assert.AreEqual(1, hydroNodeFeatureCollection.Features.Count);

            Assert.AreEqual(1, pumpFeatureCollection.Features.Count);
            Assert.AreEqual(1, weirFeatureCollection.Features.Count);
            Assert.AreEqual(1, bridgeFeatureCollection.Features.Count);
        }
    }
}