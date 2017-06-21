using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGrid1DIntegrationTests
    {
        private const string UGRID_TEST_FILE = @"ugrid\Custom_Ugrid.nc";
        
        public static IDiscretization CreateSimpleNetworkDiscretization()
        {
            var network = new HydroNetwork() { Name = "my Simple Network" };
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
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 0));
            // add calculation points
            var location1 = new NetworkLocation(branch1, 1);
            networkDiscretisation.Locations.Values.Add(location1);
            var location2 = new NetworkLocation(branch1, 2.5);
            networkDiscretisation.Locations.Values.Add(location2);
            var location3 = new NetworkLocation(branch1, 4);
            networkDiscretisation.Locations.Values.Add(location3);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 5));

            const int expectedNumberOfNetworkNodes = 2;
            const int expectedNumberOfNetworkBranches = 1;
            const int expectedNumberOfNetworkGeometryPoints = 3;
            const int expectedNumberOfDiscretisationPoints = 5;
            const int expectedNumberOfMeshEdges = 4;

            return networkDiscretisation;

            //WriteRead1DNetworkAndTest(
            //    network, expectedNumberOfNetworkNodes, expectedNumberOfNetworkBranches, expectedNumberOfNetworkGeometryPoints,
            //    networkDiscretisation, expectedNumberOfDiscretisationPoints, expectedNumberOfMeshEdges);

            //WriteRead1DMesh(networkDiscretisation, expectedNumberOfDiscretisationPoints, expectedNumberOfMeshEdges, network.Nodes.Count, network.Branches.Count);
        }

        public static IDiscretization CreateNetworkDiscretization()
        {
            var network = new HydroNetwork() { Name = "My Network" };
            var hydroNode1 = new HydroNode() { Name = "my Node 1", Description = "node 1 description", Geometry = new Point(-187.96667, 720.81667), Network = network };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode() { Name = "my Node 2", Description = "node 2 description", Geometry = new Point(2195.7333, 708.71667), Network = network };
            network.Nodes.Add(hydroNode2);
            var hydroNode3 = new HydroNode() { Name = "my Node 3", Description = "node 3 description", Geometry = new Point(4071.4928, 690.94861), Network = network };
            network.Nodes.Add(hydroNode3);
            var hydroNode4 = new HydroNode() { Name = "my Node  4", Description = "node 4 description", Geometry = new Point(3445.4246, 1540.1838), Network = network };
            network.Nodes.Add(hydroNode4);

            var branch1 = new Branch()
            {
                Name = "my Branch 1",
                Description = "branch 1 description",
                Network = network,
                Source = hydroNode1,
                Target = hydroNode2,
                Geometry = new LineString(new[]
                    {
                    new Coordinate(-187.96667, 720.81667),
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
                Description = "branch 2 description",
                Network = network,
                Source = hydroNode2,
                Target = hydroNode3,
                Geometry = new LineString(new[]
                {
                    new Coordinate(2195.7333, 708.71667),
                    new Coordinate(2577.8276, 567.00618),
                    new Coordinate(3235.6759, 576.54021),
                    new Coordinate(4071.4928, 690.94861)
                })
            };
            network.Branches.Add(branch2);

            var branch3 = new Branch()
            {
                Name = "my Branch 3",
                Description = "branch 3 description",
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
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 0));
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

            return networkDiscretisation;
        }

        [Test]
        [Ignore("This test will be disabled (and moved to UGridToNetworkAdapterTest)")]
        [Category(TestCategory.DataAccess)]
        private static void WriteRead1DNetworkAndTest()
        {

            var testFilePath =
                TestHelper.GetTestFilePath(@"ugrid\WriteRead1DNetworkAndTest.nc");
            var testFolderPath = Path.GetDirectoryName(testFilePath);
            FileUtils.CreateDirectoryIfNotExists(testFolderPath);
            FileUtils.DeleteIfExists(testFilePath);

            var networkDiscretization = CreateNetworkDiscretization();
            var network = networkDiscretization.Network;

            const int expNrNwNodes = 4;
            const int expNrNwBranches = 3;
            const int expNrNwGeoPoints = 15;
            //const int expNrDiscrPoints = 13;
            //const int expNrMeshEdges = 12;

            try
            {
                using (var ugrid1D = new UGrid1D(testFilePath))
                {
                    ugrid1D.CreateFile();

                    var totalNumberOfGeometryPoints = network.Branches.Sum(b => b.Geometry.Coordinates.Length);
                    Assert.AreEqual(expNrNwGeoPoints, totalNumberOfGeometryPoints);

                    #region Write 1D network

                    int networkId;
                    // create 1D grid
                    ugrid1D.Create1DNetworkInFile(
                        network.Name,
                        network.Nodes.Count,
                        network.Branches.Count,
                        totalNumberOfGeometryPoints,
                        out networkId);

                    network.Attributes = new DictionaryFeatureAttributeCollection
                    {
                        {"IoNetCdfNetworkId", networkId}
                    };

                    // write 1D network nodes
                    ugrid1D.Write1DNetworkNodes(
                        network.Nodes.Select(n => n.Geometry.Coordinates[0].X).ToArray(),
                        network.Nodes.Select(n => n.Geometry.Coordinates[0].Y).ToArray(),
                        network.Nodes.Select(n => n.Name).ToArray(), network.Nodes.Select(n => n.Description).ToArray());

                    var numberOfNetworkNodes = ugrid1D.GetNumberOfNetworkNodes(networkId);
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

                    var numberOfNetworkBranches = ugrid1D.GetNumberOfNetworkBranches(networkId);
                    Assert.AreEqual(expNrNwBranches, numberOfNetworkBranches);

                    // write 1D network geometry
                    ugrid1D.Write1DNetworkGeometry(
                        network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.X).ToArray()).ToArray(),
                        network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.Y).ToArray()).ToArray());
                    var numberOfNetworkGeometryPoints = ugrid1D.GetNumberOfNetworkGeometryPoints(networkId);
                    Assert.AreEqual(expNrNwGeoPoints, numberOfNetworkGeometryPoints);

                    #endregion

                    #region Read 1D network

                    // read 1D network nodes
                    double[] nodesX;
                    double[] nodesY;
                    string[] nodesIds;
                    string[] nodesLongnames;

                    ugrid1D.Read1DNetworkNodes(networkId, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
                    //Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierrNodes);
                    Assert.AreEqual(network.Nodes.Count, nodesX.Length);
                    Assert.AreEqual(network.Nodes.Count, nodesY.Length);
                    Assert.AreEqual(network.Nodes.Count, nodesIds.Length);

                    // loop over the items and assert each item
                    for (int i = 0; i < network.Nodes.Count; ++i)
                    {
                        // test node names
                        string inNodeName = network.Nodes[i].Name.Trim().Replace(" ", "_");
                        string outNodeName = nodesIds[i].Trim();
                        Assert.AreEqual(inNodeName, outNodeName);

                        // test x coordinate
                        double inCoordinateX = network.Nodes[i].Geometry.Coordinates[0].X;
                        double outCoordinateX = nodesX[i];
                        Assert.AreEqual(inCoordinateX, outCoordinateX);

                        // test y coordinate
                        double inCoordinateY = network.Nodes[i].Geometry.Coordinates[0].Y;
                        double outCoordinateY = nodesY[i];
                        Assert.AreEqual(inCoordinateY, outCoordinateY);

                        // test node description
                        var description = network.Nodes[i].Description;
                        string inNodeDescription = description != null ? description.Trim().Replace(" ", "_") : "";
                        string outNodeDescription = nodesLongnames[i].Trim();
                        Assert.AreEqual(inNodeDescription, outNodeDescription);
                    }

                    // read 1D branches
                    int[] sourceNodes;
                    int[] targetNodes;
                    double[] branchLengths;
                    int[] branchGeoPoints;
                    string[] branchIds;
                    string[] branchLongnames;

                    ugrid1D.Read1DNetworkBranches(networkId, out sourceNodes, out targetNodes,
                        out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);

                    //Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierrBranches);
                    Assert.AreEqual(network.Branches.Count, sourceNodes.Length);
                    Assert.AreEqual(network.Branches.Count, targetNodes.Length);
                    Assert.AreEqual(network.Branches.Count, branchLengths.Length);
                    Assert.AreEqual(network.Branches.Count, branchGeoPoints.Length);
                    Assert.AreEqual(network.Branches.Count, branchIds.Length);
                    Assert.AreEqual(network.Branches.Count, branchLongnames.Length);

                    // loop over the items and assert each item
                    for (int i = 0; i < network.Branches.Count; ++i)
                    {
                        var branch = network.Branches[i];
                        // test source nodes
                        INode inBranchSourceNode = branch.Source;
                        INode outBranchSourceNode = network.Nodes[sourceNodes[i]];
                        Assert.AreEqual(inBranchSourceNode, outBranchSourceNode);

                        // test target nodes
                        INode inBranchTargetNode = branch.Target;
                        INode outBranchTargetNode = network.Nodes[targetNodes[i]];
                        Assert.AreEqual(inBranchTargetNode, outBranchTargetNode);

                        // test branch lengths
                        var inBranchLength = branch.Length;
                        var outBranchLength = branchLengths[i];
                        Assert.AreEqual(inBranchLength, outBranchLength);

                        // test number of geometry points per branch
                        var inBranchGeoPointsCount = branch.Geometry.Coordinates.Length;
                        var outBranchGeoPointsCount = branchGeoPoints[i];
                        Assert.AreEqual(inBranchGeoPointsCount, outBranchGeoPointsCount);

                        // test branch names
                        var inBranchName = branch.Name.Trim().Replace(" ", "_");
                        var outBranchName = branchIds[i].Trim();
                        Assert.AreEqual(inBranchName, outBranchName);

                        // test branch descriptions
                        var description = branch.Description;
                        var inBranchDescription = description != null ? description.Trim().Replace(" ", "_") : "";
                        var outBranchDescription = branchLongnames[i].Trim();
                        Assert.AreEqual(inBranchDescription, outBranchDescription);

                    }

                    // read 1D network geometry points
                    double[] geopointsX;
                    double[] geopointsY;

                    ugrid1D.Read1DNetworkGeometry(networkId, out geopointsX, out geopointsY);

                    //Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierrGeometry);
                    Assert.AreEqual(network.Branches.Sum(b => b.Geometry.Coordinates.Length), geopointsX.Length);
                    Assert.AreEqual(network.Branches.Sum(b => b.Geometry.Coordinates.Length), geopointsY.Length);

                    // test the x and y coordinates for each item
                    int index = 0;
                    foreach (var geoCoordinate in network.Branches.SelectMany(b => b.Geometry.Coordinates).ToList())
                    {
                        Assert.AreEqual(geoCoordinate.X, geopointsX[index]);
                        Assert.AreEqual(geoCoordinate.Y, geopointsY[index]);
                        index++;
                    }
                    #endregion
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(testFilePath);
            }
        }

        private static void WriteRead1DMesh(IDiscretization networkDiscretization, int expNrDiscrPoints, int expNrMeshEdges, int numberOfNetworkNodes, int numberOfNetworkBranches)
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);

            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            try
            {
                using (var uGrid1DMesh = new UGrid1DDiscretisation(localCopyOfTestFile))
                {
                    #region Write 1D network discretisation

                    // get the discretisation points from the network discretisation
                    var discretisationPoints = networkDiscretization.Locations.Values.ToArray();
                    Assert.AreEqual(expNrDiscrPoints, discretisationPoints.Length);

                    // calculate the number of mesh edges -> #meshEdges = #discretisationPoints - #connectionNodes + #branches
                    var numberOfMeshEdges = discretisationPoints.Length - networkDiscretization.Network.Nodes.Count + networkDiscretization.Network.Branches.Count;// numberOfNetworkNodes + numberOfNetworkBranches;
                    Assert.AreEqual(expNrMeshEdges, numberOfMeshEdges);

                    if (!networkDiscretization.Network.Attributes.ContainsKey("IoNetCdfNetworkId"))
                    {

                    };
                    int networkId = (int)networkDiscretization.Network.Attributes["IoNetCdfNetworkId"];

                    // create 1D mesh
                    uGrid1DMesh.Create1DMeshInFile(
                        networkDiscretization.Name,
                        discretisationPoints.Length,
                        numberOfMeshEdges,
                        networkId
                    );
                    Assert.AreEqual(expNrDiscrPoints, uGrid1DMesh.GetNumberOf1DMeshDiscretisationPoints());

                    // write 1D discretisation points
                    int[] branchIdx = discretisationPoints.Select(l => l.Branch)
                        .ToArray()
                        .Select(b => networkDiscretization.Network.Branches.IndexOf(b))
                        .ToArray();
                    Assert.AreEqual(expNrDiscrPoints, branchIdx.Length);

                    double[] offset = discretisationPoints.Select(l => l.Chainage).ToArray();

                    uGrid1DMesh.Write1DMeshDiscretizationPoints(
                        branchIdx,
                        offset
                    );
                    #endregion

                    #region Read 1D network discretisation

                    int[] loadedBranchIdx;
                    double[] loadedOffset;
                    var ierr = uGrid1DMesh.Read1DMeshDiscretisationPoints(out loadedBranchIdx, out loadedOffset);

                    Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
                    Assert.AreEqual(discretisationPoints.Length, loadedBranchIdx.Length);
                    Assert.AreEqual(discretisationPoints.Length, loadedOffset.Length);

                    for (int i = 0; i < discretisationPoints.Length; ++i)
                    {
                        Assert.AreEqual(branchIdx[i], loadedBranchIdx[i]);
                        Assert.AreEqual(offset[i], loadedOffset[i]);
                    }

                    #endregion
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(localCopyOfTestFile);
            }
        }
    }
}