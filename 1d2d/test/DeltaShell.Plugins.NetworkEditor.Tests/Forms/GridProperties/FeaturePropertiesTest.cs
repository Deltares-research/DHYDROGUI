using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.GridProperties
{
    [TestFixture]
    public class FeaturePropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowPropertiesOfBridge()
        {
            var bridge = new Bridge();
            bridge.Attributes.Add("test","11");

            WindowsFormsTestHelper.ShowModal(new PropertyGrid { SelectedObject = new BridgeProperties { Data = bridge } });
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void MoveBranchNodeShouldMoveBranchStructures()
        {
            var network = new HydroNetwork();
            var node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(0.0, 0.0) };
            var node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(100.0, 0.0) };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            var channel = new Channel("branch", node1, node2)
                {
                    Geometry = new LineString(new[]
                        {
                            new Coordinate(0, 0),
                            new Coordinate(100, 0)
                        })
                };
            network.Branches.Add(channel);
            var lateralSource = new LateralSource { Name = "L1", Network = network, Branch = channel, Chainage = 50.0, Geometry = new Point(50.0, 0.0) };
            channel.BranchFeatures.Add(lateralSource);
            var region = new HydroRegion();
            region.SubRegions.Add(network);
            var nodePropertyGrid = new HydroNodeProperties { Data = node1, Y = 100.0 };
            Assert.AreEqual(50.0, lateralSource.Geometry.Centroid.Y, 1e-06);
        }
    }

}
