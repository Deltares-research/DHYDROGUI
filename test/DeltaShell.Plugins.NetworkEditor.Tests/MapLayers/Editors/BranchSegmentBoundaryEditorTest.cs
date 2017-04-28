using DelftShell.Plugins.NetworkEditor.Editors;
using DelftTools.DataObjects.Helpers;
using DelftTools.DataObjects.HydroNetwork;
using GeoAPI.Extensions.Networks;
using GisSharpBlog.NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.UI.Editors;
using SharpMap.UI.FallOff;
using SharpMap.UI.Forms;
using System.Drawing;
using Point = GisSharpBlog.NetTopologySuite.Geometries.Point;

namespace DelftShell.Plugins.NetworkEditor.Tests.Editors
{
    [TestFixture]
    public class BranchSegmentBoundaryEditorTest
    {
        private static IHydroNetwork network;
        private static IChannel branch1;
        private static INode node1;
        private static INode node2;
        private static IMapControl mapControl;
        private static ICoordinateConverter coordinateConverter;

        [SetUp]
        public void NetworkSetup()
        {
            mapControl = new MapControl { Map = { Size = new Size(1000, 1000) } }; // enable coordinate conversions, default size is 100x100
            coordinateConverter = new CoordinateConverter(mapControl); 
            network = new HydroNetwork();

            branch1 = new Channel
            {
                Geometry =
                    new LineString(new[]
                                     {
                                         new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(20, 0), 
                                         new Coordinate(30, 0)
                                     })
            };
            node1 = new HydroNode { Geometry = new Point(0, 0) };
            node2 = new HydroNode { Geometry = new Point(30, 0) };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            branch1.Source = node1;
            branch1.Target = node2;
            network.Branches.Add(branch1);

            double length = branch1.Geometry.Length;
            HydroNetworkHelper.CreateSegments(branch1, new[] { 0.0, length / 3, 2 * length / 3, length });
        }

        [Test]
        public void PropertyTest()
        {
            var segmentBoundaryEditor = new BranchSegmentBoundaryEditor(null, null, branch1.BranchSegments[0].BranchSegmentBoundaryStart, null);
            Assert.AreEqual(false, segmentBoundaryEditor.AllowDeletion());
            Assert.AreEqual(false, segmentBoundaryEditor.AllowMove());
            Assert.AreEqual(false, segmentBoundaryEditor.AllowSingleClickAndMove());

            segmentBoundaryEditor = new BranchSegmentBoundaryEditor(null, null, branch1.BranchSegments[0].BranchSegmentBoundaryEnd, null);
            Assert.AreEqual(false, segmentBoundaryEditor.AllowDeletion());
            Assert.AreEqual(true, segmentBoundaryEditor.AllowMove());
            Assert.AreEqual(true, segmentBoundaryEditor.AllowSingleClickAndMove());
        }

        [Test]
        public void SnapTargetTest()
        {
            var segmentBoundaryEditor = new BranchSegmentBoundaryEditor(null, null, branch1.BranchSegments[0].BranchSegmentBoundaryStart, null);
            Assert.AreEqual(1, segmentBoundaryEditor.GetSnapTargets().Count);
            Assert.AreEqual(branch1, segmentBoundaryEditor.GetSnapTargets()[0]);
        }

        [Test]
        public void MoveSegmentBoundary()
        {
            // moves a segment boundary
            Assert.AreEqual(3, branch1.BranchSegments.Count);
            Assert.AreEqual(10, branch1.BranchSegments[1].Length);

            // Create an editor for the boundary at the end of the first segment. This is the boundary at position 10, 0
            var segmentBoundaryEditor = new BranchSegmentBoundaryEditor(coordinateConverter, null, branch1.BranchSegments[0].BranchSegmentBoundaryEnd, null);
            Assert.AreEqual(0, branch1.BranchSegments[0].BranchSegmentBoundaryEnd.Geometry.Coordinates[0].Y);
            Assert.AreEqual(10, branch1.BranchSegments[0].BranchSegmentBoundaryEnd.Geometry.Coordinates[0].X);
            
            const double deltaX = 5;
            const double deltaY = 0;

            segmentBoundaryEditor.Start();
            segmentBoundaryEditor.FallOffPolicy = new NoFallOffPolicy();
            segmentBoundaryEditor.Select(segmentBoundaryEditor.GetTrackerByIndex(0), true);
            segmentBoundaryEditor.MoveTracker(segmentBoundaryEditor.GetTrackerByIndex(0), deltaX, deltaY, null);
            segmentBoundaryEditor.Stop();

            Assert.AreEqual(0, branch1.BranchSegments[0].BranchSegmentBoundaryEnd.Geometry.Coordinates[0].Y);
            Assert.AreEqual(15, branch1.BranchSegments[0].BranchSegmentBoundaryEnd.Geometry.Coordinates[0].X);
            Assert.AreEqual(15, branch1.BranchSegments[0].BranchSegmentBoundaryEnd.Offset);
            Assert.AreEqual(7.5, branch1.BranchSegments[0].BranchSegmentCenter.Geometry.Coordinates[0].X);
            Assert.AreEqual(7.5, branch1.BranchSegments[0].BranchSegmentCenter.Offset);
            Assert.AreEqual(0, branch1.BranchSegments[0].BranchSegmentBoundaryStart.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, branch1.BranchSegments[0].BranchSegmentBoundaryStart.Offset);
            Assert.AreEqual(15, branch1.BranchSegments[0].Geometry.Length);
            Assert.AreEqual(15, branch1.BranchSegments[0].Length);

        }
    }
}
