using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Converters.WellKnownText;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CompositeStructureView
{
    [TestFixture]
    public class SideViewDataBuilderTest
    {
        [Test]
        public void GetNetworkSideViewDataForStructure()
        {
            var network = new HydroNetwork();

            var node1 = new HydroNode("node1") { Geometry = new Point(0, 0) };
            var node2 = new HydroNode("node2") { Geometry = new Point(100, 0) };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            
            var branch = new Channel("branch1", node1, node2) { Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)") };

            network.Branches.Add(branch);
            var structure = new CompositeBranchStructure();
            structure.Branch = branch;
            structure.Chainage = 10;
            var data = CompositeStructureViewDataBuilder.GetCompositeStructureViewDataForStructure(structure);
            var route = data.NetworkRoute;
            //route is one from left and right of structure without length
            Assert.AreEqual(new NetworkLocation(branch, 9), route.Locations.Values[0]);
            Assert.AreEqual(new NetworkLocation(branch, 11), route.Locations.Values[1]);
        }
    }
}
