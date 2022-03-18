using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Grid.MeshKernel;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid.MeshKernel
{
    [TestFixture]
    public class MeshKernelExtensionsTest
    {
        [Test]
        public void GivenMeshKernelExtensions_Doing_Should()
        {
            //Arrange
            var delta = 25e-9;
            var network = new HydroNetwork();
            var startNode = new HydroNode("Node 1") { Geometry = new Point(0, delta) };
            var endNode = new HydroNode("Node 2") { Geometry = new Point(10 + delta, delta) };
            var branch = new Channel("Channel 1", startNode, endNode)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, delta),
                    new Coordinate(10 + delta, delta)
                })
            };

            network.Nodes.AddRange(new[] { startNode, endNode });
            network.Branches.Add(branch);

            var discretization = new Discretization { Network = network };

            var locations = new[]
            {
                new NetworkLocation(branch, 0.0),
                new NetworkLocation(branch, 5 + delta),
                new NetworkLocation(branch, 10 + delta)
            };

            discretization.Locations.AddValues(locations);

            // Act

            var mesh = discretization.CreateDisposableMesh1D();

            // Assert
            Assert.AreEqual(3, mesh.NodeX.Length);
            Assert.AreEqual(0, mesh.NodeX[0]);
            Assert.AreEqual(5 + delta, mesh.NodeX[1]);
            Assert.AreEqual(10 + delta, mesh.NodeX[2]);

            Assert.AreEqual(3, mesh.NodeY.Length);
            Assert.AreEqual(delta, mesh.NodeY[0]);
            Assert.AreEqual(delta, mesh.NodeY[1]);
            Assert.AreEqual(delta, mesh.NodeY[2]);
        }
    }
}