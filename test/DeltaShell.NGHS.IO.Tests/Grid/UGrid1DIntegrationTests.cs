using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGrid1DIntegrationTests
    {
        private const string UGRID_TEST_FILE = @"ugrid\Custom_Ugrid.nc";

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write1DSimpleNetworkTest()
        {
            var network = new HydroNetwork() { Name = "my Network" };
            var hydroNode1 = new HydroNode() { Name = "my Node1", Geometry = new Point(1, 4), Network = network };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode() { Name = "myNode2", Geometry = new Point(5, 1), Network = network };
            network.Nodes.Add(hydroNode2);
            var branch1 = new Branch()
            {
                Name = "my Branch 1",
                Network = network,
                Source = hydroNode1,
                Target = hydroNode2,
                Geometry = new LineString(new[]
                {
                    new Coordinate(1, 4),
                    new Coordinate(6, 12),
                    new Coordinate(5, 1)
                })
            };
            network.Branches.Add(branch1);

            Write1DNetworkAndTest(network, 2, 1, 3);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write1DNetworkTest()
        {
            var network = new HydroNetwork() { Name = "my Network" };
            var hydroNode1 = new HydroNode() { Name = "my Node 1", Geometry = new Point(1, 2), Network = network };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode() { Name = "my Node 2", Geometry = new Point(5, 3), Network = network };
            network.Nodes.Add(hydroNode2);
            var hydroNode3 = new HydroNode() { Name = "myNode3", Geometry = new Point(9, 6), Network = network };
            network.Nodes.Add(hydroNode3);
            var hydroNode4 = new HydroNode() { Name = "my Node  4", Geometry = new Point(11, 2), Network = network };
            network.Nodes.Add(hydroNode4);
            var hydroNode5 = new HydroNode() { Name = "my Node 5", Geometry = new Point(15, 1), Network = network };
            network.Nodes.Add(hydroNode5);
            var branch1 = new Branch()
            {
                Name = "my Branch 1",
                Network = network,
                Source = hydroNode1,
                Target = hydroNode2,
                Geometry = new LineString(new[]
                    {
                    new Coordinate(1, 2),
                    new Coordinate(1.5, 2.5),
                    new Coordinate(3, 0),
                    new Coordinate(5, 3)})
            };
            network.Branches.Add(branch1);
            var branch2 = new Branch()
            {
                Name = "my Branch 2",
                Network = network,
                Source = hydroNode3,
                Target = hydroNode2,
                Geometry = new LineString(new[]
                {
                    new Coordinate(9, 6),
                    new Coordinate(7, 5),
                    new Coordinate(9, 4),
                    new Coordinate(4, 6),
                    new Coordinate(5, 3)
                })
            };
            network.Branches.Add(branch2);
            var branch3 = new Branch()
            {
                Name = "my Branch 3",
                Network = network,
                Source = hydroNode2,
                Target = hydroNode4,
                Geometry = new LineString(new[]
                {
                    new Coordinate(5, 3),
                    new Coordinate(10, 3),
                    new Coordinate(10.5, 0),
                    new Coordinate(11, 2.5)
                })
            };
            network.Branches.Add(branch3);
            var branch4 = new Branch()
            {
                Name = "myBranch4",
                Network = network,
                Source = hydroNode4,
                Target = hydroNode5,
                Geometry = new LineString(new[]
                {
                    new Coordinate(11, 2.5),
                    new Coordinate(13, 6),
                    new Coordinate(10, -1),
                    new Coordinate(17, 5),
                    new Coordinate(15, 1)
                })
            };
            network.Branches.Add(branch4);

            Write1DNetworkAndTest(network, 5, 4, 18);

        }

        private static void Write1DNetworkAndTest(HydroNetwork network, int expectedNumberOfNetworkNodes, int expectedNumberOfNetworkBranches, int expectedNumberOfNetworkGeometryPoints)
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);

            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            try
            {
                using (var ugrid1D = new UGrid1D(localCopyOfTestFile))
                {
                    var totalNumberOfGeometryPoints = network.Branches.Sum(b => b.Geometry.Coordinates.Length);

                    ugrid1D.Create1DGridInFile(
                        network.Name,
                        network.Nodes.Count,
                        network.Branches.Count,
                        totalNumberOfGeometryPoints);

                    ugrid1D.Write1DNetworkNodes(
                        network.Nodes.Select(n => n.Geometry.Coordinates[0].X).ToArray(),
                        network.Nodes.Select(n => n.Geometry.Coordinates[0].Y).ToArray(),
                        network.Nodes.Select(n => n.Name).ToArray(), network.Nodes.Select(n => n.Description).ToArray());

                    var numberOfNetworkNodes = ugrid1D.GetNumberOfNetworkNodes();
                    Assert.AreEqual(expectedNumberOfNetworkNodes, numberOfNetworkNodes);

                    ugrid1D.Write1DNetworkBranches(
                        network.Branches.Select(b => b.Source).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray(),
                        network.Branches.Select(b => b.Target).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray(),
                        network.Branches.Select(b => b.Length).ToArray(),
                        network.Branches.Select(b =>
                            {
                                if (b.Geometry != null && b.Geometry.Coordinates != null)
                                    return b.Geometry.Coordinates.Length;
                                return 0;
                            })
                            .ToArray(),
                        network.Branches.Select(b => b.Name).ToArray(),
                        network.Branches.Select(b => b.Description).ToArray()
                    );

                    var numberOfNetworkBranches = ugrid1D.GetNumberOfNetworkBranches();
                    Assert.AreEqual(expectedNumberOfNetworkBranches, numberOfNetworkBranches);

                    ugrid1D.Write1DNetworkGeometry(
                        network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.X).ToArray()).ToArray(),
                        network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.Y).ToArray()).ToArray());
                    var numberOfNetworkGeometryPoints = ugrid1D.GetNumberOfNetworkGeometryPoints();
                    Assert.AreEqual(expectedNumberOfNetworkGeometryPoints, numberOfNetworkGeometryPoints);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(localCopyOfTestFile);
            }
        }
    }
}