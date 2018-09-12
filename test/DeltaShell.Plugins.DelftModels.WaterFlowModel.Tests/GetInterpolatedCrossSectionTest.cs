using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Converters.Geometries;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class GetInterpolatedCrossSectionTest
    {
        private HydroNetwork network;
        private IChannel branch1, branch2, branch3;

        [SetUp]
        public void SetUp()
        {
            network = new HydroNetwork();
            var crossSectionType = new CrossSectionSectionType { Name = "CrossSectionRoughnessSectionType" };
            network.CrossSectionSectionTypes.Add(crossSectionType);
            // add nodes and branches
            IHydroNode node1 = new HydroNode { Name = "node1", Network = network };
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network };
            IHydroNode node3 = new HydroNode { Name = "node3", Network = network };
            IHydroNode node4 = new HydroNode { Name = "node4", Network = network };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);
            network.Nodes.Add(node4);

            branch1 = new Channel("branch1", node1, node2)
                {
                    OrderNumber = 0
                };
            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(0, 0),
                                   new Coordinate(100, 0)
                               };
            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            branch2 = new Channel("branch2", node2, node3)
                {
                    OrderNumber = 0
                };
            vertices = new List<Coordinate>
                           {
                               new Coordinate(100, 0),
                               new Coordinate(200, 0)
                           };
            branch2.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            branch3 = new Channel("branch3", node3, node4)
                {
                    OrderNumber = 0
                };
            vertices = new List<Coordinate>
                           {
                               new Coordinate(200, 0),
                               new Coordinate(300, 0)
                           };
            branch3.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            network.Branches.Add(branch3);
        }

        public void AddCrossSections(CrossSectionType crossSectionType)
        {
            var isZw = crossSectionType == CrossSectionType.ZW || crossSectionType == CrossSectionType.Standard;
            // add cross-sections
            var offset = 33.3;

            var cs1 = CrossSection.CreateDefault(crossSectionType, branch1, offset);

            if (isZw)
            {
                foreach (var r in ((CrossSectionDefinitionZW) cs1.Definition).ZWDataTable.Rows)
                {
                    r.Width /= 2d;
                }
            }

            branch1.BranchFeatures.Add(cs1);

            var cs2 = CrossSection.CreateDefault(crossSectionType, branch3, offset);
            cs2.Definition.ShiftLevel(+10d);
            branch3.BranchFeatures.Add(cs2);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void FindNearestCrossSectionTest()
        {
            AddCrossSections(CrossSectionType.ZW);

            // for network:
            //
            //      branch 1          branch2         branch 3
            // n1 --cs1-------> n2 ------------> n3 --cs2-------> n4
            //
            // branches all have length 100, cs1 and cs2 are at distance 33.3 from nodes n1 and n3 respectively.
            //
            var result = GetInterpolatedCrossSection.FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber((Channel)network.Channels.First(c => c.Name == "branch2"), 50.0, true, new List<Channel>());
            Assert.AreEqual((50d + 66.7), result.First, 1e-5, "Distance to left crosssection (1st network) incorrect");

            result = GetInterpolatedCrossSection.FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber((Channel)network.Channels.First(c => c.Name == "branch2"), 50.0, false, new List<Channel>());
            Assert.AreEqual((50d + 33.3), result.First, 1e-5, "Distance to right crosssection (1st network) incorrect");

            // now reverse direction of 1st branch and see if distance to left crosssection is still calculated correctly
            //
            //      branch 1          branch2         branch 3
            // n1 <-cs1-------- n2 ------------> n3 --cs2-------> n4
            //
            HydroNetworkHelper.ReverseBranch(network.Channels.First(c => c.Name == "branch1"));

            result = GetInterpolatedCrossSection.FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber((Channel)network.Channels.First(c => c.Name == "branch2"), 50.0, true, new List<Channel>());
            Assert.AreEqual((50d + 66.7), result.First, 1e-5, "Distance to left crosssection (2nd network) incorrect");

            // now reverse direction of 3rd branch and see if distance to right crosssection is still calculated correctly
            //
            //      branch 1          branch2         branch 3
            // n1 <-cs1-------- n2 ------------> n3 <-cs2-------- n4
            //
            HydroNetworkHelper.ReverseBranch(network.Channels.First(c => c.Name == "branch3"));

            result = GetInterpolatedCrossSection.FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber((Channel)network.Channels.First(c => c.Name == "branch2"), 50.0, false, new List<Channel>());
            Assert.AreEqual((50d + 66.7), result.First, 1e-5, "Distance to left crosssection (3rd network) incorrect");

            // finally reverse direction of 2nd branch and see if distance to left and right crosssections are still calculated correctly
            //
            //      branch 1          branch2         branch 3
            // n1 <-cs1-------- n2 <------------ n3 <-cs2-------- n4
            //
            HydroNetworkHelper.ReverseBranch(network.Channels.First(c => c.Name == "branch2"));

            result = GetInterpolatedCrossSection.FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber((Channel)network.Channels.First(c => c.Name == "branch2"), 50.0, false, new List<Channel>());
            Assert.AreEqual((50d + 66.7), result.First, 1e-5, "Distance to left crosssection (4th network) incorrect");

            result = GetInterpolatedCrossSection.FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber((Channel)network.Channels.First(c => c.Name == "branch2"), 50.0, true, new List<Channel>());
            Assert.AreEqual((50d + 33.3), result.First, 1e-5, "Distance to right crosssection (4th network) incorrect");

            // really finally :) get nearest crosssection from points that are not exactly at center of branch
            result = GetInterpolatedCrossSection.FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber((Channel)network.Channels.First(c => c.Name == "branch2"), 60.0, false, new List<Channel>());
            Assert.AreEqual((40d + 66.7), result.First, 1e-5, "Distance to left crosssection (4th network, different chainage) incorrect");

            result = GetInterpolatedCrossSection.FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber((Channel)network.Channels.First(c => c.Name == "branch2"), 60.0, true, new List<Channel>());
            Assert.AreEqual((60d + 33.3), result.First, 1e-5, "Distance to left crosssection (4th network, different chainage) incorrect");
        }

        [Test]
        public void GetDistancesToNearestCrossSections()
        {
            //local setup for this test
            network = new HydroNetwork();
            var crossSectionType = new CrossSectionSectionType { Name = "CrossSectionRoughnessSectionType" };
            network.CrossSectionSectionTypes.Add(crossSectionType);
            // add nodes and branches
            IHydroNode node1 = new HydroNode { Name = "node1", Network = network };
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            branch1 = new Channel("branch1", node1, node2)
            {
                OrderNumber = 0
            };
            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(0, 0),
                                   new Coordinate(100, 0)
                               };
            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            network.Branches.Add(branch1);

            // add cross-sections
            var offset = 0d;

            var cs1 = CrossSection.CreateDefault(CrossSectionType.ZW, branch1, offset);
            branch1.BranchFeatures.Add(cs1);

            offset = branch1.Length;
            var cs2 = CrossSection.CreateDefault(CrossSectionType.ZW, branch1, offset);
            branch1.BranchFeatures.Add(cs2);

            // test
            var channel = branch1 as Channel;

            var foundcs1 = GetInterpolatedCrossSection.FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber(channel, 50d, true);
            var foundcs2 = GetInterpolatedCrossSection.FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber(channel, 50d, false);
            var distanceBetweenCrossSections = foundcs1.First + foundcs2.First;
            var distanceToCrossSectionNr1 = foundcs1.First;

            Assert.AreEqual(50d, foundcs1.First);
            Assert.AreEqual(50d, foundcs2.First);
        }
    }
}