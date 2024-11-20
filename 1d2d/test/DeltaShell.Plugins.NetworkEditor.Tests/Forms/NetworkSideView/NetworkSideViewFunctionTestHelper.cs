using DelftTools.Hydro;
using DelftTools.Units;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Converters.WellKnownText;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    public static class NetworkSideViewFunctionTestHelper
    {
        /// <summary>
        /// Asserts the min and max in source for route is min and max.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="route"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        public static void AssertMinMax(INetworkCoverage source, Route route, double max, double min)
        {
            var function = new NetworkSideViewDataController(route, null).CreateRouteFunctionFromNetworkCoverage(source, new Unit()); 

            Assert.AreEqual(max, function.Components[0].Values.MaxValue);
            Assert.AreEqual(min, function.Components[0].Values.MinValue);
        }

        public static void SetNetworkLocationAt10And20(INetworkCoverage source)
        {
            double delta = 0.0;
            foreach (IBranch branch in source.Network.Branches)
            {
                double offset = 0;
                while (offset < (branch.Length + 1.0e-3))
                {
                    source[new NetworkLocation(branch, offset)] = offset;
                    offset += 10.0 + delta;
                }
                delta += 10;
            }
        }

        public static void AssertRouteIsCorrect(INetworkCoverage source, Route route, int[] expectedOffsets, int[] expectedValues)
        {
            var function = new NetworkSideViewDataController(route, null).CreateRouteFunctionFromNetworkCoverage(source, new Unit()); 

            //using a loop like sideview does since a lot is not implemented here. Implement more
            //if needed in production code..not for test.
            //assert the function is what we expected
            Assert.AreEqual(expectedOffsets.Length, function.Components[0].Values.Count);

            for (int i = 0; i < expectedOffsets.Length; i++)
            {
                Assert.AreEqual(expectedOffsets[i], function.Arguments[0].Values[i]);
                Assert.AreEqual(expectedValues[i], function.Components[0].Values[i]);
            }
        }

        public static HydroNetwork GetNetwork2Branches()
        {
            var network = new HydroNetwork();

            var node1 = new HydroNode("node1") {Geometry = new Point(0, 0)};
            var node2 = new HydroNode("node2") { Geometry = new Point(100, 0) };
            var node3 = new HydroNode("node3") { Geometry = new Point(300, 0) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node2) { Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)") };
            var branch2 = new Channel("branch2", node2, node3) { Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 300 0)") };
            
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            return network;
        }

        public static IHydroNetwork CreateNetwork()
        {
            var network = new HydroNetwork();

            var node1 = new HydroNode("node1") { Geometry = new Point(new Coordinate(0, 0)) };
            var node2 = new HydroNode("node2") { Geometry = new Point(new Coordinate(100, 0)) };
            var node3 = new HydroNode("node3") { Geometry = new Point(new Coordinate(300, 0)) };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node2) { Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)") };
            var branch2 = new Channel("branch2", node2, node3) { Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 300 0)") };
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            return network;
        }

        public static Network GetNetwork()
        {
            var network = new HydroNetwork();
            var node1 = new Node("node1");
            var node2 = new Node("node2");
            var node3 = new Node("node3");


            var geometry1 = new LineString(new[]
                                               {
                                                   new Coordinate(0, 0),
                                                   new Coordinate(0, 100)
                                               });
            var geometry2 = new LineString(new[]
                                               {
                                                   new Coordinate(0, 100),
                                                   new Coordinate(0, 200)
                                               });
            IBranch branch1 = new Branch(node1, node2, 100) { Geometry = geometry1, Name = "branch1" };
            IBranch branch2 = new Branch(node2, node3, 100) { Geometry = geometry2, Name = "branch2" };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            return network;
        }
    }
}