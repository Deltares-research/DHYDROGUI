using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
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

            var networkDiscretisation = new Discretization
            {
                Name = "my Discretisation",
                Network = network
            };

            // add source node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1,0));
            // add calculation points
            var location1 = new NetworkLocation(branch1, 1);
            networkDiscretisation.Locations.Values.Add(location1);
            var location2 = new NetworkLocation(branch1, 2.5);
            networkDiscretisation.Locations.Values.Add(location2);
            var location3 = new NetworkLocation(branch1, 4);
            networkDiscretisation.Locations.Values.Add(location3);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 5));

            var expectedNumberOfNetworkNodes = 2;
            var expectedNumberOfNetworkBranches = 1;
            var expectedNumberOfNetworkGeometryPoints = 3;
            var expectedNumberOfDiscretisationPoints = 5;
            var expectedNumberOfMeshEdges = 4;
            Write1DNetworkAndTest(
                network, expectedNumberOfNetworkNodes, expectedNumberOfNetworkBranches, expectedNumberOfNetworkGeometryPoints, 
                networkDiscretisation, expectedNumberOfDiscretisationPoints, expectedNumberOfMeshEdges);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write1DNetworkTest()
        {
            var network = new HydroNetwork() { Name = "my Network" };
            var hydroNode1 = new HydroNode() { Name = "my Node 1", Geometry = new Point(-187.96667, 720.81667), Network = network };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode() { Name = "my Node 2", Geometry = new Point(2195.7333, 708.71667), Network = network };
            network.Nodes.Add(hydroNode2);
            var hydroNode3 = new HydroNode() { Name = "my Node 3", Geometry = new Point(4071.4928, 690.94861), Network = network };
            network.Nodes.Add(hydroNode3);
            var hydroNode4 = new HydroNode() { Name = "my Node  4", Geometry = new Point(3445.4246, 1540.1838), Network = network };
            network.Nodes.Add(hydroNode4);

            var branch1 = new Branch()
            {
                Name = "my Branch 1",
                Network = network,
                Source = hydroNode1,
                Target = hydroNode2,
                Geometry = new LineString(new[]
                    {
                    new Coordinate(-187.96667, 20.81667),
                    new Coordinate(187.13333, 1039.45),
                    new Coordinate(828.43333, 861.98333),
                    new Coordinate(1219.6667, 406.21667),
                    new Coordinate(1712.2164, 273.32123),
                    new Coordinate(2094.9, 547.38333),
                    new Coordinate(2195.7333, 708.71667)
                })
            };
            network.Branches.Add(branch1);

            var branch2 = new Branch()
            {
                Name = "my Branch 2",
                Network = network,
                Source = hydroNode2,
                Target = hydroNode3,
                Geometry = new LineString(new[]
                {
                    new Coordinate(2195.7333, 708.71667),
                    new Coordinate(2577.8276, 567.00618),
                    new Coordinate(3235.6759, 576.54021),
                    new Coordinate(4071.4928, 690.94861),
                    
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
                    new Coordinate(2195.7333, 708.71667),
                    new Coordinate(2739.9061, 938.83347),
                    new Coordinate(3226.4949, 1367.1587),
                    new Coordinate(3445.4246, 1540.1838)
                })
            };
            network.Branches.Add(branch3);
            
            var networkDiscretisation = new Discretization
            {
                Name = "my Discretisation",
                Network = network
            };

            // Branch 1
            // add source node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 0)); // TODO: Can i add a node directly?
            // add calculation points
            var location1 = new NetworkLocation(branch1, 500);
            networkDiscretisation.Locations.Values.Add(location1);
            var location2 = new NetworkLocation(branch1, 1000);
            networkDiscretisation.Locations.Values.Add(location2);
            var location3 = new NetworkLocation(branch1, 1500);
            networkDiscretisation.Locations.Values.Add(location3);
            var location4 = new NetworkLocation(branch1, 2000);
            networkDiscretisation.Locations.Values.Add(location4);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 2500));

            // Branch3
            // add calculation points
            var location5 = new NetworkLocation(branch3, 700);
            networkDiscretisation.Locations.Values.Add(location5);
            var location6 = new NetworkLocation(branch3, 1400);
            networkDiscretisation.Locations.Values.Add(location6);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch3, 2100));

            // Branch 2
            // add calculation points
            var location7 = new NetworkLocation(branch2, 400);
            networkDiscretisation.Locations.Values.Add(location7);
            var location8 = new NetworkLocation(branch2, 800);
            networkDiscretisation.Locations.Values.Add(location8);
            var location9 = new NetworkLocation(branch2, 1200);
            networkDiscretisation.Locations.Values.Add(location9);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch2, 1600));

            var expectedNumberOfNetworkNodes = 4;
            var expectedNumberOfNetworkBranches = 3;
            var expectedNumberOfNetworkGeometryPoints = 15;
            var expectedNumberOfDiscretisationPoints = 13;
            var expectedNumberOfMeshEdges = 12;
            Write1DNetworkAndTest(
                network, expectedNumberOfNetworkNodes, expectedNumberOfNetworkBranches, expectedNumberOfNetworkGeometryPoints,
                networkDiscretisation, expectedNumberOfDiscretisationPoints, expectedNumberOfMeshEdges);

        }

        private static void Write1DNetworkAndTest(HydroNetwork network, int expNrNwNodes, int expNrNwBranches, int expNrNwGeoPoints, IDiscretization networkDiscretization, int expNrDiscrPoints, int expNrMeshEdges)
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);

            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            try
            {
                using (var ugrid1D = new UGrid1D(localCopyOfTestFile))
                {
                    var totalNumberOfGeometryPoints = network.Branches.Sum(b => b.Geometry.Coordinates.Length);
                    Assert.AreEqual(expNrNwGeoPoints, totalNumberOfGeometryPoints);

                    // create 1D grid
                    ugrid1D.Create1DGridInFile(
                        network.Name,
                        network.Nodes.Count,
                        network.Branches.Count,
                        totalNumberOfGeometryPoints);

                    // write 1D network nodes
                    ugrid1D.Write1DNetworkNodes(
                        network.Nodes.Select(n => n.Geometry.Coordinates[0].X).ToArray(),
                        network.Nodes.Select(n => n.Geometry.Coordinates[0].Y).ToArray(),
                        network.Nodes.Select(n => n.Name).ToArray(), network.Nodes.Select(n => n.Description).ToArray());

                    var numberOfNetworkNodes = ugrid1D.GetNumberOfNetworkNodes();
                    Assert.AreEqual(expNrNwNodes, numberOfNetworkNodes);

                    // write 1D network branches
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
                    Assert.AreEqual(expNrNwBranches, numberOfNetworkBranches);

                    // write 1D network geometry
                    ugrid1D.Write1DNetworkGeometry(
                        network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.X).ToArray()).ToArray(),
                        network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.Y).ToArray()).ToArray());
                    var numberOfNetworkGeometryPoints = ugrid1D.GetNumberOfNetworkGeometryPoints();
                    Assert.AreEqual(expNrNwGeoPoints, numberOfNetworkGeometryPoints);

                    // get the discretisation points from the network discretisation
                    var discretisationPoints = networkDiscretization.Locations.Values.ToArray();
                    Assert.AreEqual(expNrDiscrPoints,discretisationPoints.Length);

                    // calculate the number of mesh edges -> #meshEdges = #discretisationPoints - #connectionNodes + #branches
                    var numberOfMeshEdges = 0;
                    numberOfMeshEdges = discretisationPoints.Length - numberOfNetworkNodes + numberOfNetworkBranches;
                    Assert.AreEqual(expNrMeshEdges, numberOfMeshEdges);
                    
                    // create 1D mesh
                    ugrid1D.Create1DMeshInFile(
                        networkDiscretization.Name,
                        discretisationPoints.Length,    
                        numberOfMeshEdges               
                    );
                    Assert.AreEqual(expNrDiscrPoints, ugrid1D.GetNumberOfMeshDiscretisationPoints());

                    // write 1D discretisation points
                    int[] branchIdx = discretisationPoints.Select(l => l.Branch) // TODO: Test this function
                        .ToArray()
                        .Select(b => networkDiscretization.Network.Branches.IndexOf(b))
                        .ToArray(); 
                    Assert.AreEqual(expNrDiscrPoints, branchIdx.Length);

                    double[] offset = discretisationPoints.Select(l => l.Chainage).ToArray(); // TODO: Test + is the order correct?

                    ugrid1D.Write1DMeshDiscretizationPoints(
                        branchIdx,
                        offset
                    );


                }
            }
            finally
            {
                FileUtils.DeleteIfExists(localCopyOfTestFile);
            }
        }
    }
}