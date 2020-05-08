using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Editors
{
    // TODO: make it independent of HydroNetwork and move to SharpMap.Tests.Editors.Interactors
    [TestFixture]
    public class BranchNodeTopologyTest
    {
        [Test]
        public void RecalculateBranchOrdersForNetworksThatAreToBeMergedAndThatHaveNoOrderingDefined()
        {
            HydroNetwork[] networks = CreateTestNetworks();
            HydroNetwork leftNetwork = networks[0];
            HydroNetwork rightNetwork = networks[1];

            BranchOrderHelper.RecalculateBranchOrdersForNetworksThatAreToBeMerged(
                leftNetwork.Nodes.First(n => n.Name == "n1"),
                rightNetwork.Nodes.First(n => n.Name == "n5"));

            foreach (IBranch b in leftNetwork.Branches)
            {
                Assert.AreEqual(-1, b.OrderNumber);
            }

            foreach (IBranch b in rightNetwork.Branches)
            {
                Assert.AreEqual(-1, b.OrderNumber);
            }
        }

        [Test]
        public void RecalculateBranchOrdersForNetworksThatAreToBeMergedAndThatHaveOrderingDefined()
        {
            HydroNetwork[] networks = CreateTestNetworks();
            HydroNetwork leftNetwork = networks[0];
            HydroNetwork rightNetwork = networks[1];

            leftNetwork.Branches.First(b => b.Name == "c1").OrderNumber = 1;
            leftNetwork.Branches.First(b => b.Name == "c2").OrderNumber = 2;
            leftNetwork.Branches.First(b => b.Name == "c3").OrderNumber = 3;
            rightNetwork.Branches.First(b => b.Name == "c4").OrderNumber = 2;
            rightNetwork.Branches.First(b => b.Name == "c5").OrderNumber = 1;
            rightNetwork.Branches.First(b => b.Name == "c6").OrderNumber = 3;

            // in this case no changes to the ordernumbers should be made

            BranchOrderHelper.RecalculateBranchOrdersForNetworksThatAreToBeMerged(
                leftNetwork.Nodes.First(n => n.Name == "n1"),
                rightNetwork.Nodes.First(n => n.Name == "n5"));

            Assert.AreEqual(1, leftNetwork.Branches.First(b => b.Name == "c1").OrderNumber);
            Assert.AreEqual(2, leftNetwork.Branches.First(b => b.Name == "c2").OrderNumber);
            Assert.AreEqual(3, leftNetwork.Branches.First(b => b.Name == "c3").OrderNumber);
            Assert.AreEqual(2, rightNetwork.Branches.First(b => b.Name == "c4").OrderNumber);
            Assert.AreEqual(1, rightNetwork.Branches.First(b => b.Name == "c5").OrderNumber);
            Assert.AreEqual(3, rightNetwork.Branches.First(b => b.Name == "c6").OrderNumber);
        }

        [Test]
        public void RecalculateBranchOrdersForNetworksThatAreToBeMergedAndThatHaveConflictingOrderingDefined()
        {
            HydroNetwork[] networks = CreateTestNetworks();
            HydroNetwork leftNetwork = networks[0];
            HydroNetwork rightNetwork = networks[1];

            leftNetwork.Branches.First(b => b.Name == "c1").OrderNumber = 1;
            leftNetwork.Branches.First(b => b.Name == "c2").OrderNumber = 2;
            leftNetwork.Branches.First(b => b.Name == "c3").OrderNumber = 3;
            rightNetwork.Branches.First(b => b.Name == "c4").OrderNumber = 2;
            rightNetwork.Branches.First(b => b.Name == "c5").OrderNumber = 3;
            rightNetwork.Branches.First(b => b.Name == "c6").OrderNumber = 2;

            // in this case changes to the ordernumbers should be made

            BranchOrderHelper.RecalculateBranchOrdersForNetworksThatAreToBeMerged(
                leftNetwork.Nodes.First(n => n.Name == "n1"),
                rightNetwork.Nodes.First(n => n.Name == "n5"));

            Assert.AreEqual(4, leftNetwork.Branches.First(b => b.Name == "c1").OrderNumber);
            Assert.AreEqual(5, leftNetwork.Branches.First(b => b.Name == "c2").OrderNumber);
            Assert.AreEqual(6, leftNetwork.Branches.First(b => b.Name == "c3").OrderNumber);
            Assert.AreEqual(2, rightNetwork.Branches.First(b => b.Name == "c4").OrderNumber);
            Assert.AreEqual(3, rightNetwork.Branches.First(b => b.Name == "c5").OrderNumber);
            Assert.AreEqual(2, rightNetwork.Branches.First(b => b.Name == "c6").OrderNumber);
        }

        private HydroNetwork[] CreateTestNetworks()
        {
            // Creates two simple test networks that look like this:
            //
            //  n2--(c1)--\          /--(c4)--n6
            //             \        /
            //  n3--(c2)----n1    n5----(c5)--n7
            //             /        \
            //  n4--(c3)--/          \--(c6)--n8
            //

            var leftNetwork = new HydroNetwork();
            var n1 = new HydroNode
            {
                Network = leftNetwork,
                Geometry = new Point(new Coordinate(50, 50)),
                Name = "n1"
            };
            var n2 = new HydroNode
            {
                Network = leftNetwork,
                Geometry = new Point(new Coordinate(0, 100)),
                Name = "n2"
            };
            var n3 = new HydroNode
            {
                Network = leftNetwork,
                Geometry = new Point(new Coordinate(0, 50)),
                Name = "n3"
            };
            var n4 = new HydroNode
            {
                Network = leftNetwork,
                Geometry = new Point(new Coordinate(0, 0)),
                Name = "n4"
            };
            var channel1 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 100),
                    new Coordinate(50, 50)
                }),
                Source = n2,
                Target = n1,
                Name = "c1"
            };
            var channel2 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 50),
                    new Coordinate(50, 50)
                }),
                Source = n3,
                Target = n1,
                Name = "c2"
            };
            var channel3 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(50, 50),
                    new Coordinate(0, 0)
                }),
                Source = n1,
                Target = n4,
                Name = "c3"
            };
            leftNetwork.Nodes.Add(n1);
            leftNetwork.Nodes.Add(n2);
            leftNetwork.Nodes.Add(n3);
            leftNetwork.Nodes.Add(n4);
            NetworkHelper.AddChannelToHydroNetwork(leftNetwork, channel1);
            NetworkHelper.AddChannelToHydroNetwork(leftNetwork, channel2);
            NetworkHelper.AddChannelToHydroNetwork(leftNetwork, channel3);

            var rightNetwork = new HydroNetwork();
            var n5 = new HydroNode
            {
                Network = rightNetwork,
                Geometry = new Point(new Coordinate(150, 50)),
                Name = "n5"
            };
            var n6 = new HydroNode
            {
                Network = rightNetwork,
                Geometry = new Point(new Coordinate(200, 100)),
                Name = "n6"
            };
            var n7 = new HydroNode
            {
                Network = rightNetwork,
                Geometry = new Point(new Coordinate(200, 50)),
                Name = "n7"
            };
            var n8 = new HydroNode
            {
                Network = rightNetwork,
                Geometry = new Point(new Coordinate(200, 0)),
                Name = "n8"
            };
            var channel4 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(150, 50),
                    new Coordinate(200, 100)
                }),
                Source = n5,
                Target = n6,
                Name = "c4"
            };
            var channel5 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(150, 50),
                    new Coordinate(200, 50)
                }),
                Source = n5,
                Target = n7,
                Name = "c5"
            };
            var channel6 = new Channel
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(200, 0),
                    new Coordinate(150, 50)
                }),
                Source = n8,
                Target = n5,
                Name = "c6"
            };
            rightNetwork.Nodes.Add(n5);
            rightNetwork.Nodes.Add(n6);
            rightNetwork.Nodes.Add(n7);
            rightNetwork.Nodes.Add(n8);
            NetworkHelper.AddChannelToHydroNetwork(rightNetwork, channel4);
            NetworkHelper.AddChannelToHydroNetwork(rightNetwork, channel5);
            NetworkHelper.AddChannelToHydroNetwork(rightNetwork, channel6);

            return new[]
            {
                leftNetwork,
                rightNetwork
            };
        }
    }
}