using System;
using System.Collections.Generic;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DeltaShell.NGHS.TestUtils;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroNetworkTest
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Info);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneHydroNetworkWithData()
        {
            IHydroNetwork network = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            ReflectionTestHelper.FillObjectListPropertiesWithRandomInstances(network);

            var clone = (HydroNetwork) network.Clone();

            TestReferenceHelper.AssertStringRepresentationOfGraphIsEqual(network, clone);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneHydroNetworkWithRoutes()
        {
            IHydroNetwork network = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            var route = new Route();
            var routeName = "RouteToBeCloned";
            route.Name = routeName;
            network.Routes.Add(route);

            var clone = (HydroNetwork) network.Clone();

            List<string> lingeringReferences = TestReferenceHelper.SearchObjectInObjectGraph(route.Network, clone);
            lingeringReferences.ForEach(Console.WriteLine);
            Assert.AreEqual(0, lingeringReferences.Count);

            Assert.AreEqual(1, clone.Routes.Count);

            Route clonedRoute = clone.Routes[0];
            Assert.AreEqual(clone, clonedRoute.Network);
            Assert.AreEqual(routeName, clonedRoute.Name);
        }

        [Test]
        [Category(NghsTestCategory.DoNotRunForCodeCoverage)] // Garbage collection is not performed directly during coverage run
        public void ClonedNetworkIsCollected()
        {
            GC.Collect();

            //issue 5410 openda memory problems
            var weakReference = new WeakReference(null);
            HydroNetwork network = GetNetwork();
            for (var i = 0; i < 10; i++)
            {
                weakReference.Target = network.Clone(); //create clones that get out of scope
            }

            GC.Collect();

            //test it was collected
            Assert.IsNull(weakReference.Target);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneHydroNetworkAndAddBranch()
        {
            var network = new HydroNetwork();
            var from = new HydroNode();
            var to = new HydroNode();
            network.Nodes.Add(from);
            network.Nodes.Add(to);
            var channel = new Channel
            {
                Source = from,
                Target = to
            };
            network.Branches.Add(channel);

            var clonedNetwork = (IHydroNetwork) network.Clone();

            var from2 = new HydroNode("from2");
            var to2 = new HydroNode("to2");
            clonedNetwork.Nodes.Add(from2);
            clonedNetwork.Nodes.Add(to2);
            var channel2 = new Channel
            {
                Name = "channel2",
                Source = from2,
                Target = to2
            };
            clonedNetwork.Branches.Add(channel2);

            Assert.AreEqual(1, network.Branches.Count);
            Assert.AreEqual(2, clonedNetwork.Branches.Count);
        }

       private HydroNetwork GetNetwork()
        {
            var network = new HydroNetwork();
            var from = new HydroNode();
            var to = new HydroNode();
            network.Nodes.Add(from);
            network.Nodes.Add(to);
            var channel = new Channel
            {
                Source = from,
                Target = to
            };
            network.Branches.Add(channel);
            
            return network;
        }
    }
}