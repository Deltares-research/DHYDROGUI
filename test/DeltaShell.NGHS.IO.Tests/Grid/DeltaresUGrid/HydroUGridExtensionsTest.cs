using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.TestUtils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using SharpMapTestUtils;

namespace DeltaShell.NGHS.IO.Tests.Grid.DeltaresUGrid
{
    [TestFixture]
    public class HydroUGridExtensionsTest
    {
        [Test]
        public void GivenUGridMeshAdapter_DoingCreateUnstructuredGrid_ShouldCreateValidGrid()
        {
            //
            //          7
            //    6.----.----. 8
            //     |    |    | 
            //     |    |    | 
            //    3.----.----. 5
            //     |   4|    | 
            //     |    |    | 
            //     .----.----.
            //     0    1    2
            //
            // 

            //Arrange
            var disposable2DMeshGeometry = new Disposable2DMeshGeometry
            {
                Name = "Mesh2d",
                NodesX = new double[] { 1, 2, 3, 1, 2, 3, 1, 2, 3 },
                NodesY = new double[] { 1, 1, 1, 2, 2, 2, 3, 3, 3 },
                EdgeNodes = new[] { 0, 1, 1, 2, 0, 3, 1, 4, 2, 5, 3, 4, 4, 5, 3, 6, 4, 7, 5, 8, 6, 7, 7, 8 },
                FaceNodes = new[] { 0, 1, 3, 4, 1, 2, 4, 5, 3, 4, 6, 7, 4, 5, 7, 8 },
                FaceX = new double[] { 1.5, 1.5, 2.5, 2.5 },
                FaceY = new double[] { 1.5, 2.5, 1.5, 2.5 },
                MaxNumberOfFaceNodes = 4
            };

            // Act

            var grid = disposable2DMeshGeometry.CreateUnstructuredGrid(false);

            // Assert
            Assert.AreEqual(9, grid.Vertices.Count);

            Assert.AreEqual(1, grid.Vertices[0].X);
            Assert.AreEqual(1, grid.Vertices[0].Y);

            Assert.AreEqual(3, grid.Vertices[8].X);
            Assert.AreEqual(3, grid.Vertices[8].Y);

            Assert.AreEqual(12, grid.Edges.Count);

            Assert.AreEqual(0, grid.Edges[0].VertexFromIndex);
            Assert.AreEqual(1, grid.Edges[0].VertexToIndex);

            Assert.AreEqual(7, grid.Edges[11].VertexFromIndex);
            Assert.AreEqual(8, grid.Edges[11].VertexToIndex);

            Assert.AreEqual(4, grid.Cells.Count);

            Assert.AreEqual(new[] { 0, 1, 3, 4}, grid.Cells[0].VertexIndices);
            Assert.AreEqual(new[] { 4, 5, 7, 8 }, grid.Cells[3].VertexIndices);

        }

        [Test]
        public void GivenUGridMeshAdapter_DoingCreateDisposable2DMeshGeometry_ShouldGiveValidCreateDisposable2DMeshGeometry()
        {
            //Arrange
            var numbOfCellsHorizontal = 4;
            var numbOfCellsVertical = 6;

            var yOffset = 20;
            var xOffset = 10;
            var cellHeight = 50;
            var cellWidth = 20;

            var grid = UnstructuredGridTestHelper.GenerateRegularGrid(numbOfCellsHorizontal, numbOfCellsVertical, cellWidth, cellHeight, xOffset, yOffset);

            // Act
            var meshGeometry = grid.CreateDisposable2DMeshGeometry();

            // Assert
            Assert.Null(meshGeometry.Name);
            Assert.AreEqual(grid.Vertices.Count, meshGeometry.NodesX.Length);
            Assert.AreEqual(grid.Vertices.Count, meshGeometry.NodesY.Length);

            Assert.AreEqual(grid.Edges.Count *2, meshGeometry.EdgeNodes.Length);
            
            Assert.AreEqual(grid.Cells.Count, meshGeometry.FaceX.Length);
            Assert.AreEqual(grid.Cells.Count, meshGeometry.FaceY.Length);
            Assert.AreEqual(grid.Cells.Count * 4, meshGeometry.FaceNodes.Length);
            Assert.AreEqual(4, meshGeometry.MaxNumberOfFaceNodes);
        }

        [Test]
        [TestCaseSource(nameof(SetNetworkGeometryArgumentNullCases))]
        public void SetNetworkGeometry_ArgumentNull_ThrowsArgumentNullException(
            IHydroNetwork network,
            DisposableNetworkGeometry networkGeometry,
            IEnumerable<BranchProperties> branchProperties,
            IEnumerable<CompartmentProperties> compartmentProperties,
            string parameterName)
        {
            UGridFileHelperNetworkGeometry uGridFileHelperNetworkGeometry = new UGridFileHelperNetworkGeometry();

            // Call
            void Action() => uGridFileHelperNetworkGeometry.SetNetworkGeometry(network, networkGeometry, branchProperties, compartmentProperties);
            

            // Assert
            Assert.That(Action, Throws.ArgumentNullException
                                      .With.Property(nameof(ArgumentNullException.ParamName)).EqualTo(parameterName));
        }

        private static IEnumerable<TestCaseData> SetNetworkGeometryArgumentNullCases()
        {
            var network = Substitute.For<IHydroNetwork>();
            var networkGeometry = new DisposableNetworkGeometry();
            var branchProperties = Substitute.For<IEnumerable<BranchProperties>>();
            var compartmentProperties = Substitute.For<IEnumerable<CompartmentProperties>>();

            yield return new TestCaseData(null, networkGeometry, branchProperties, compartmentProperties, "network")
                .SetName("Network null");
            yield return new TestCaseData(network, null, branchProperties, compartmentProperties, "networkGeometry")
                .SetName("NetworkGeometry null");
            yield return new TestCaseData(network, networkGeometry, null, compartmentProperties, "branchProperties")
                .SetName("BranchProperties null");
            yield return new TestCaseData(network, networkGeometry, branchProperties, null, "compartmentProperties")
                .SetName("CompartmentProperties null");
        }

        [Test]
        public void GivenUGridMeshAdapter_DoingSetNetworkGeometry_ShouldCreateValidNetwork()
        {                                      
            //          Rural network             Urban network
            // ^                                   
            // |  100       o node1      o  node2       o manhole1
            // y             \          /               |                           (manhole2 has 2 compartments)
            //         branch1 \      / branch2         | pipe1
            //                   \  /                   |
            //     50             o node3              o--------o pipe2
            //                     \              manhole2    manhole3
            //                       \ branch3
            //                         \
            //    0                     o node4
            //                   
            //             0     50    100            200       300 
            //                                          x -->

            //Arrange
            var networkGeometry = new DisposableNetworkGeometry
            {
                NetworkName = "TestNetwork",
                NodesX = new double[] { 0, 100, 50, 100, 200, 199.5, 200.5, 300 },
                NodesY = new double[] { 100, 100, 50, 0, 100, 50, 50, 50 },
                NodeIds = new [] { "node1", "node2", "node3", "node4", "compartment1", "compartment2", "compartment3", "compartment4" },
                NodeLongNames = new[] { "node1 long", "node2 long", "node3 long", "node4 long", "manhole1 long", "manhole2 long", "manhole2 long", "manhole3 long" },
                
                BranchIds = new[] { "branch1", "branch2", "branch3", "pipe1", "connection", "pipe2" },
                BranchLongNames = new[] { "branch1 long", "branch2 long", "branch3 long", "pipe1 long", "connection long", "pipe2 long" },
                BranchLengths = new double[] {100,100,100,50,1,100},
                BranchOrder = new[] {1,2,3,4,0,5},
                BranchTypes = new[] {1,1,1,2,2,2},
                
                BranchGeometryNodesCount = new[] {2,2,2,2,2,2,2},

                //                               branch1   branch2   branch3   pipe1      connection    pipe2
                BranchGeometryX = new double[] {   0, 50,  100, 50,  50,   0,  200, 200,  200, 200.5,   200.5, 300 },
                BranchGeometryY = new double[] { 100, 50,  100, 50,  50, 100,  100,  50,   50,    50,      50,  50 },
                
                NodesFrom = new int[] { 0, 1, 2, 4, 5, 6},
                NodesTo = new int[]   { 2, 2, 3, 5, 6, 7}
            };

            var compartmentProperties = new List<CompartmentProperties>
            {
                new CompartmentProperties{ CompartmentId = "compartment1", ManholeId = "manhole1"},
                new CompartmentProperties{ CompartmentId = "compartment2", ManholeId = "manhole2"},
                new CompartmentProperties{ CompartmentId = "compartment3", ManholeId = "manhole2"},
                new CompartmentProperties{ CompartmentId = "compartment4", ManholeId = "manhole3"},
            };

            var branchProperties = new List<BranchProperties>
            {
                new BranchProperties{Name = "pipe1", BranchType = BranchFile.BranchType.Pipe },
                new BranchProperties{Name = "pipe2", BranchType = BranchFile.BranchType.Pipe },
                new BranchProperties{Name = "connection", BranchType = BranchFile.BranchType.SewerConnection },
            };

            if (Map.CoordinateSystemFactory == null)
            {
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            }
            var rdCoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(28992);
            var network = new HydroNetwork{CoordinateSystem = rdCoordinateSystem };
            UGridFileHelperNetworkGeometry uGridFileHelperNetworkGeometry = new UGridFileHelperNetworkGeometry();

            // Act
            uGridFileHelperNetworkGeometry.SetNetworkGeometry(network, networkGeometry, branchProperties, compartmentProperties);
            
            // Assert
            // check dimensions
            Assert.AreEqual(7, network.Nodes.Count);
            Assert.AreEqual(4, network.HydroNodes.Count());
            Assert.AreEqual(3, network.Manholes.Count());
            
            Assert.AreEqual(6, network.Branches.Count);
            Assert.AreEqual(3, network.Channels.Count());
            Assert.AreEqual(3, network.SewerConnections.Count());
            Assert.AreEqual(2, network.Pipes.Count());

            // check nodes
            var node2 = network.HydroNodes.First(n => n.Name == "node2");
            var manhole2 = network.Manholes.First(n => n.Name == "manhole2");

            Assert.AreEqual("node2 long", node2.LongName);
            Assert.AreEqual(100, node2.Geometry.Coordinate.X);
            Assert.AreEqual(100, node2.Geometry.Coordinate.Y);

            Assert.AreEqual("manhole2 long", manhole2.LongName);
            Assert.AreEqual(200, manhole2.Geometry.Coordinate.X);
            Assert.AreEqual(50, manhole2.Geometry.Coordinate.Y);
            Assert.AreEqual(2, manhole2.Compartments.Count);

            var branch1 = network.Channels.First(c => c.Name == "branch1");
            var pipe1 = network.Pipes.First(c => c.Name == "pipe1");

            Assert.AreEqual("branch1 long", branch1.LongName);
            Assert.AreEqual(2, branch1.Geometry.Coordinates.Length);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].X);
            Assert.AreEqual(100, branch1.Geometry.Coordinates[0].Y);
            Assert.AreEqual(50, branch1.Geometry.Coordinates[1].X);
            Assert.AreEqual(50, branch1.Geometry.Coordinates[1].Y);

            Assert.AreEqual("pipe1 long",pipe1.LongName);
            Assert.AreEqual(2, pipe1.Geometry.Coordinates.Length);
            Assert.AreEqual(200, pipe1.Geometry.Coordinates[0].X);
            Assert.AreEqual(100, pipe1.Geometry.Coordinates[0].Y);
            Assert.AreEqual(200, pipe1.Geometry.Coordinates[1].X);
            Assert.AreEqual(50, pipe1.Geometry.Coordinates[1].Y);
        }

        [Test]
        public void SetNetworkGeometry_NetworkGeometryWithDuplicateCompartmentIds_ThrowsArgumentException()
        {
            // Setup
            var networkGeometry = new DisposableNetworkGeometry
            {
                NetworkName = "TestNetwork",
                NodesX = new double[] { 0, 100, 50, 100, 200, 199.5, 200.5, 300 },
                NodesY = new double[] { 100, 100, 50, 0, 100, 50, 50, 50 },
                NodeIds = new [] { "node1", "node2", "node3", "node4", "compartment1", "compartment2", "compartment3", "compartment4" },
                NodeLongNames = new[] { "node1 long", "node2 long", "node1 (duplicate) long", "node2 (duplicate) long", "manhole1 long", "manhole2 long", "manhole2 long", "manhole3 long" },
                
                BranchIds = new[] { "branch1", "branch2", "branch3", "pipe1", "connection", "pipe2" },
                BranchLongNames = new[] { "branch1 long", "branch2 long", "branch3 long", "pipe1 long", "connection long", "pipe2 long" },
                BranchLengths = new double[] {100,100,100,50,1,100},
                BranchOrder = new[] {1,2,3,4,0,5},
                BranchTypes = new[] {1,1,1,2,2,2},
                
                BranchGeometryNodesCount = new[] {2,2,2,2,2,2,2},

                //                               branch1   branch2   branch3   pipe1      connection    pipe2
                BranchGeometryX = new double[] {   0, 50,  100, 50,  50,   0,  200, 200,  200, 200.5,   200.5, 300 },
                BranchGeometryY = new double[] { 100, 50,  100, 50,  50, 100,  100,  50,   50,    50,      50,  50 },
                
                NodesFrom = new int[] { 0, 1, 2, 4, 5, 6},
                NodesTo = new int[]   { 2, 2, 3, 5, 6, 7}
            };

            // add some compartment properties with duplicate compartmentIds
            var compartmentProperties = new List<CompartmentProperties>
            {
                new CompartmentProperties{ CompartmentId = "compartment1", ManholeId = "manhole1"},
                new CompartmentProperties{ CompartmentId = "compartment2", ManholeId = "manhole2"},
                new CompartmentProperties{ CompartmentId = "compartment1", ManholeId = "manhole2"},
                new CompartmentProperties{ CompartmentId = "compartment1", ManholeId = "manhole3"},
            };

            var branchProperties = new List<BranchProperties>
            {
                new BranchProperties{Name = "pipe1", BranchType = BranchFile.BranchType.Pipe },
                new BranchProperties{Name = "pipe2", BranchType = BranchFile.BranchType.Pipe },
                new BranchProperties{Name = "connection", BranchType = BranchFile.BranchType.SewerConnection },
            };

            if (Map.CoordinateSystemFactory == null)
            {
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            }
            var rdCoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(28992);
            var network = new HydroNetwork{CoordinateSystem = rdCoordinateSystem };
            UGridFileHelperNetworkGeometry uGridFileHelperNetworkGeometry = new UGridFileHelperNetworkGeometry();

            // Call
            void Call() => uGridFileHelperNetworkGeometry.SetNetworkGeometry(network, networkGeometry, branchProperties, compartmentProperties);
            
            // Assert
            var expectedMessage = $"The following entries were not unique in compartment ids: {Environment.NewLine}compartment1 at indices (0, 2, 3)";
            Assert.That(Call, Throws.ArgumentException.With.Message.EqualTo(expectedMessage));
        }
        
        [Test]
        public void SetNetworkGeometry_NetworkGeometryWithDuplicateNodeIds_ThrowsArgumentException()
        {
            // Setup
            var networkGeometry = new DisposableNetworkGeometry
            {
                NetworkName = "TestNetwork",
                NodesX = new double[] { 0, 100, 50, 100, 200, 199.5, 200.5, 300 },
                NodesY = new double[] { 100, 100, 50, 0, 100, 50, 50, 50 },
                // add duplicate nodeIds
                NodeIds = new [] { "node1", "node2", "node2", "node2", "compartment1", "compartment2", "compartment3", "compartment4" },
                NodeLongNames = new[] { "node1 long", "node2 long", "node1 (duplicate) long", "node2 (duplicate) long", "manhole1 long", "manhole2 long", "manhole2 long", "manhole3 long" },
                
                BranchIds = new[] { "branch1", "branch2", "branch3", "pipe1", "connection", "pipe2" },
                BranchLongNames = new[] { "branch1 long", "branch2 long", "branch3 long", "pipe1 long", "connection long", "pipe2 long" },
                BranchLengths = new double[] {100,100,100,50,1,100},
                BranchOrder = new[] {1,2,3,4,0,5},
                BranchTypes = new[] {1,1,1,2,2,2},
                
                BranchGeometryNodesCount = new[] {2,2,2,2,2,2,2},

                //                               branch1   branch2   branch3   pipe1      connection    pipe2
                BranchGeometryX = new double[] {   0, 50,  100, 50,  50,   0,  200, 200,  200, 200.5,   200.5, 300 },
                BranchGeometryY = new double[] { 100, 50,  100, 50,  50, 100,  100,  50,   50,    50,      50,  50 },
                
                NodesFrom = new int[] { 0, 1, 2, 4, 5, 6},
                NodesTo = new int[]   { 2, 2, 3, 5, 6, 7}
            };
            
            var compartmentProperties = new List<CompartmentProperties>
            {
                new CompartmentProperties{ CompartmentId = "compartment1", ManholeId = "manhole1"},
                new CompartmentProperties{ CompartmentId = "compartment2", ManholeId = "manhole2"},
                new CompartmentProperties{ CompartmentId = "compartment3", ManholeId = "manhole2"},
                new CompartmentProperties{ CompartmentId = "compartment4", ManholeId = "manhole3"},
            };

            var branchProperties = new List<BranchProperties>
            {
                new BranchProperties{Name = "pipe1", BranchType = BranchFile.BranchType.Pipe },
                new BranchProperties{Name = "pipe2", BranchType = BranchFile.BranchType.Pipe },
                new BranchProperties{Name = "connection", BranchType = BranchFile.BranchType.SewerConnection },
            };

            if (Map.CoordinateSystemFactory == null)
            {
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            }
            var rdCoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(28992);
            var network = new HydroNetwork{CoordinateSystem = rdCoordinateSystem };
            UGridFileHelperNetworkGeometry uGridFileHelperNetworkGeometry = new UGridFileHelperNetworkGeometry();

            // Call
            void Call() => uGridFileHelperNetworkGeometry.SetNetworkGeometry(network, networkGeometry, branchProperties, compartmentProperties);
            
            // Assert
            var expectedMessage = $"The following entries were not unique in network nodes: {Environment.NewLine}node2 at indices (1, 2, 3)";
            Assert.That(Call, Throws.ArgumentException.With.Message.EqualTo(expectedMessage));
        }

        [Test]
        public void GivenUGridMeshAdapter_DoingCreateDisposableNetworkGeometry_ShouldGiveValidDisposableNetworkGeometry()
        {
            //          Rural network             Urban network
            // ^                                   
            // |  100       o node1      o  node2       o manhole1
            // y             \          /               |                           (manhole2 has 2 compartments)
            //         branch1 \      / branch2         | pipe1
            //                   \  /                   |
            //     50             o node3              o--------o pipe2
            //                     \              manhole2    manhole3
            //                       \ branch3
            //                         \
            //    0                     o node4
            //                   
            //             0     50    100            200       300 
            //                                          x -->

            var network = CreateTestNetwork();
            var manhole2 = network.Manholes.First(m => m.Name == "manhole2");
            var node1 = network.HydroNodes.First();
            var compartment2 = manhole2.Compartments[0];

            // Act
            var networkGeometry = network.CreateDisposableNetworkGeometry();

            // Assert
            Assert.AreEqual(8,networkGeometry.NodeIds.Length);
            Assert.AreEqual(6, networkGeometry.BranchIds.Length);

            // check node1
            var node1Index = networkGeometry.NodeIds.ToList().IndexOf(node1.Name);
            Assert.AreEqual(  0, networkGeometry.NodesX[node1Index]);
            Assert.AreEqual(100, networkGeometry.NodesY[node1Index]);
            Assert.AreEqual("node1 long", networkGeometry.NodeLongNames[node1Index]);

            var compartment1Index = networkGeometry.NodeIds.ToList().IndexOf(compartment2.Name);
            Assert.AreEqual(manhole2.Geometry.Coordinate.X - 0.5, networkGeometry.NodesX[compartment1Index]);
            Assert.AreEqual(manhole2.Geometry.Coordinate.Y, networkGeometry.NodesY[compartment1Index]);
            // null gets converted to empty string
            Assert.IsNull(manhole2.LongName);
            Assert.AreEqual("", networkGeometry.NodeLongNames[compartment1Index]);

            var branch = network.Branches[0];
            var branchIdList = networkGeometry.BranchIds.ToList();

            var branch1Index = branchIdList.IndexOf(branch.Name);
            Assert.AreEqual(branch.Length, networkGeometry.BranchLengths[branch1Index], 1e-6);
            Assert.AreEqual(branch.OrderNumber, networkGeometry.BranchOrder[branch1Index]);
            Assert.AreEqual(branch.Description, networkGeometry.BranchLongNames[branch1Index]);
            Assert.AreEqual(2, networkGeometry.BranchGeometryNodesCount[branch1Index]);

            var pipe = network.Pipes.First();
            var pipe1Index = branchIdList.IndexOf(pipe.Name);

            Assert.AreEqual(pipe.Length, networkGeometry.BranchLengths[pipe1Index]);
            Assert.AreEqual(2, networkGeometry.BranchGeometryNodesCount[pipe1Index]);
            // null gets converted to empty string
            Assert.IsNull(pipe.LongName);
            Assert.AreEqual("", networkGeometry.BranchLongNames[pipe1Index]);

            var innerConnection1Index = branchIdList.IndexOf("innerConnection1");
            Assert.AreEqual(1, networkGeometry.BranchLengths[innerConnection1Index]);
            Assert.AreEqual(2, networkGeometry.BranchGeometryNodesCount[innerConnection1Index]);
            Assert.AreEqual(5, networkGeometry.NodesFrom[innerConnection1Index]);
            Assert.AreEqual(6, networkGeometry.NodesTo[innerConnection1Index]);
        }

        [Test]
        public void GivenUGridMeshAdapter_DoingCreateDiscretization_ShouldGiveValidDiscretization()
        {
            //          Rural network             Urban network
            // ^                                   
            // |  100       o node1      o  node2       o manhole1
            // y             \          /               |                           (manhole2 has 2 compartments)
            //         branch1 \      / branch2         | pipe1
            //                   \  /                   |
            //     50             o node3              o--------o pipe2
            //                     \              manhole2    manhole3
            //                       \ branch3
            //                         \
            //    0                     o node4
            //                   
            //             0     50    100            200       300 
            //                                          x -->

            //Arrange

            var network = CreateTestNetwork();

            var points = network.Branches.Where(b => b is IPipe || b is IChannel).SelectMany((b, i) =>
            {
                return Enumerable.Range(0, 10).Select(j => new
                {
                    branch = b,
                    branchId = i,
                    chainage = j * (b.Length / 10.0),
                    name = $"{b.Name}_node{j}",
                    longname = $"{b.Name}_node{j}_long",
                });
            }).ToArray();
            
            /*
             * we are now calculating the locations via branch / chainage,
             * this should not matter. what we write should come back
             */
            var nodesX = network.Branches
                .Select(b=> points.Where(p => p.branch.Equals(b))
                    .Select(p1 => GeometryHelper.LineStringCoordinate((ILineString)b.Geometry, p1.chainage))
                    .Select(g => g.X)).SelectMany(d => d).ToArray();

            var nodesY = network.Branches
                .Select(b => points.Where(p => p.branch.Equals(b))
                    .Select(p1 => GeometryHelper.LineStringCoordinate((ILineString)b.Geometry, p1.chainage))
                    .Select(g => g.Y)).SelectMany(d => d).ToArray();

            var disposable1DMeshGeometry = new Disposable1DMeshGeometry
            {
                Name = "Discretization",
                BranchIDs = points.Select(p => p.branchId).ToArray(),
                BranchOffsets = points.Select(p => p.chainage).ToArray(),
                NodeIds = points.Select(p => p.name).ToArray(),
                NodeLongNames = points.Select(p => p.longname).ToArray(),
                NodesX = nodesX,
                NodesY = nodesY,
            };

            // Act

            var discretization = disposable1DMeshGeometry.CreateDiscretization(network);

            // Assert

            Assert.AreEqual(50, discretization.Locations.Values.Count);
            Assert.AreEqual(network, discretization.Network);
            
            var networkLocation2 = discretization.Locations.Values.Skip(1).First();
            Assert.AreEqual(network.Branches[0], networkLocation2.Branch);
            Assert.AreEqual(7, networkLocation2.Chainage, 0.1);
            Assert.AreEqual($"{network.Branches[0].Name}_node1", networkLocation2.Name);
            Assert.AreEqual($"{network.Branches[0].Name}_node1_long", networkLocation2.LongName);
            Assert.AreEqual(nodesX.Skip(1).First(), networkLocation2.Geometry.Coordinate.X, 0.1);
            Assert.AreEqual(nodesY.Skip(1).First(), networkLocation2.Geometry.Coordinate.Y, 0.1);
        }

        [Test]
        public void GivenUGridMeshAdapter_DoingCreateDisposable1DMeshGeometry_ShouldCreateAValidDisposable1DMeshGeometry()
        {
            //          Rural network             Urban network
            // ^                                   
            // |  100       o node1      o  node2      o manhole1
            // y             \          /              |                           (manhole2 has 2 compartments)
            //         branch1 \      / branch2        | pipe1
            //                   \  /                  |
            //     50             o node3              o--------o pipe2
            //                     \              manhole2    manhole3
            //                       \ branch3
            //                         \
            //    0                     o node4
            //                   
            //             0     50    100            200       300 
            //                                          x -->

            //Arrange
            var network = CreateTestNetwork();
            var points = network.Branches.Where(b => b is IPipe || b is IChannel).SelectMany((b, i) =>
            {
                return Enumerable.Range(0, 9).Select(j => new
                {
                    branch = b,
                    chainage = j * (b.Length / 10.0),
                    name = $"{b.Name}_node{j}",
                    longname = $"{b.Name}_node{j}_long",
                }).Plus(new
                {
                    branch = b,
                    chainage = b.Length,
                    name = $"{b.Name}_node{10}",
                    longname = $"{b.Name}_node{10}_long",
                });
            }).ToArray();

            var networkLocations = points.Select(p => new NetworkLocation(p.branch, p.chainage)
            {
                Name = p.name,
                LongName = p.longname,
                Network = network,
            });

            var discretization = new Discretization
                {
                    Name = "TestDiscretization",
                    Network = network
                };

            discretization.Locations.SetValues(networkLocations);

            // Act
            var mesh1d = discretization.CreateDisposable1DMeshGeometry();

            // Assert
            Assert.AreEqual(50, mesh1d.NodeIds.Length);
            Assert.AreEqual(45, mesh1d.EdgeBranchIds.Length);

            var branch1Node3Index = mesh1d.NodeIds.ToList().IndexOf("branch1_node3");
            Assert.AreEqual("branch1_node3_long", mesh1d.NodeLongNames[branch1Node3Index]);
            Assert.AreEqual(0, mesh1d.BranchIDs[branch1Node3Index]);
            Assert.AreEqual(21.2, mesh1d.BranchOffsets[branch1Node3Index], 0.1);

            var pipe1Node2Index = mesh1d.NodeIds.ToList().IndexOf("pipe1_node2");
            Assert.AreEqual("pipe1_node2_long", mesh1d.NodeLongNames[pipe1Node2Index]);
            Assert.AreEqual(3, mesh1d.BranchIDs[pipe1Node2Index]);
            Assert.AreEqual(10, mesh1d.BranchOffsets[pipe1Node2Index]);

            var expectedEdgeNodes = new[]
            {
                0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 10, 11, 11, 12, 12, 13, 13, 14, 14, 15, 15, 16,
                16, 17, 17, 18, 18, 19, 20, 21, 21, 22, 22, 23, 23, 24, 24, 25, 25, 26, 26, 27, 27, 28, 28, 29, 30, 31,
                31, 32, 32, 33, 33, 34, 34, 35, 35, 36, 36, 37, 37, 38, 38, 39, 40, 41, 41, 42, 42, 43, 43, 44, 44, 45,
                45, 46, 46, 47, 47, 48, 48, 49
            };
            Assert.AreEqual(expectedEdgeNodes, mesh1d.EdgeNodes);

            var expectedEdgeIds = new[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 
                1, 1, 1, 1, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 2, 2, 2, 2,
                3, 3, 3, 3, 3, 3, 3, 3, 3,
                4, 4, 4, 4, 4, 4, 4, 4, 4
            };
            Assert.AreEqual(expectedEdgeIds, mesh1d.EdgeBranchIds);
        }
        
        [Test]
        public void GivenUGridMeshAdapterTest_DoingCreateDisposableLinksGeometry_ShouldCreateValidDisposableLinksGeometry()
        {
            //Arrange
            var links = Enumerable.Range(0, 5)
                .Select(i => new Link1D2D(i, 5 - i, $"link{i}")
                {
                    LongName = $"link{i}_long",
                    TypeOfLink = LinkStorageType.Embedded
                })
                .Cast<ILink1D2D>()
                .ToList();

            // Act
            var disposableLinksGeometry = links.CreateDisposableLinksGeometry();

            // Assert
            Assert.AreEqual(5, disposableLinksGeometry.LinkId.Length);
            Assert.AreEqual("link2", disposableLinksGeometry.LinkId[2]);
            Assert.AreEqual("link2_long", disposableLinksGeometry.LinkLongName[2]);
            Assert.AreEqual(2, disposableLinksGeometry.Mesh1DFrom[2]);
            Assert.AreEqual(3, disposableLinksGeometry.Mesh2DTo[2]);
            Assert.AreEqual((int)LinkStorageType.Embedded, disposableLinksGeometry.LinkType[2]);
        }

        [Test, Category(TestCategory.Integration)]
        public void GivenHydroUGridExtensions_CreateMesh1dWithUrbanGrid_ShouldGiveCorrect1dMesh()
        {
            //Arrange
            var network = new HydroNetwork();
            network.AddSimpleUrbanNetwork();
            var discretization = new Discretization { Network = network };

            // add default grid
            discretization.Locations.AddValues(discretization.GenerateSewerConnectionNetworkLocations());
            discretization.UpdateNetworkLocations(discretization.Locations.Values, false);

            // Act
            var mesh = discretization.CreateDisposable1DMeshGeometry();

            // Assert
            Assert.AreEqual(5, mesh.NodeIds.Length);
        }

        [Test]
        public void GivenHydroUGridExtensions_CreateDisposableNetworkGeometry_ShouldGiveCorrectCoordinates()
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

            // Act
            var networkGeometry = network.CreateDisposableNetworkGeometry();

            // Assert
            Assert.AreEqual(2, networkGeometry.NodesX.Length);
            Assert.AreEqual(0, networkGeometry.NodesX[0]);
            Assert.AreEqual(10 + delta, networkGeometry.NodesX[1]);

            Assert.AreEqual(2, networkGeometry.NodesY.Length);
            Assert.AreEqual(delta, networkGeometry.NodesY[0]);
            Assert.AreEqual(delta, networkGeometry.NodesY[1]);

            Assert.AreEqual(10 + delta, networkGeometry.BranchLengths[0]);
            
            Assert.AreEqual(2, networkGeometry.BranchGeometryX.Length);
            Assert.AreEqual(0, networkGeometry.BranchGeometryX[0]);
            Assert.AreEqual(10 + delta, networkGeometry.BranchGeometryX[1]);

            Assert.AreEqual(2, networkGeometry.BranchGeometryY.Length);
            Assert.AreEqual(delta, networkGeometry.BranchGeometryY[0]);
            Assert.AreEqual(delta, networkGeometry.BranchGeometryY[1]);
        }


        [Test]
        public void GivenHydroUGridExtensions_CreateDisposable1DMeshGeometry_ShouldGiveCorrect1dMeshCoordinates()
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
            
            network.Nodes.AddRange(new []{startNode, endNode});
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
            var mesh = discretization.CreateDisposable1DMeshGeometry();

            // Assert
            Assert.AreEqual(3, mesh.NodesX.Length);
            Assert.AreEqual(0, mesh.NodesX[0]);
            Assert.AreEqual(5 + delta, mesh.NodesX[1]);
            Assert.AreEqual(10 + delta, mesh.NodesX[2]);

            Assert.AreEqual(3, mesh.NodesY.Length);
            Assert.AreEqual(delta, mesh.NodesY[0]);
            Assert.AreEqual(delta, mesh.NodesY[1]);
            Assert.AreEqual(delta, mesh.NodesY[2]);
        }

        [Test, Category(TestCategory.Integration)]
        public void GivenHydroUGridExtensions_CreateMesh1dWithUrbanGridAndMultipleCompartments_ShouldGiveCorrect1dMesh()
        {
            //Arrange
            var network = new HydroNetwork();

            var manhole1 = new Manhole { Name = "manhole1" };
            var manhole2 = new Manhole { Name = "manhole2" };
            var manhole3 = new Manhole { Name = "manhole3" };
            var manhole4 = new Manhole { Name = "manhole4" };
            var manhole5 = new Manhole { Name = "manhole4" };

            var compartment1 = new Compartment { Name = "compartment1" };
            var compartment2 = new Compartment { Name = "compartment2" };
            var compartment3 = new Compartment { Name = "compartment3" };
            var compartment4 = new Compartment { Name = "compartment4" };
            var compartment5 = new Compartment { Name = "compartment5" };
            var compartment6 = new Compartment { Name = "compartment6" };
            var compartment7 = new Compartment { Name = "compartment7" };

            manhole1.Compartments.Add(compartment1);
            manhole1.Compartments.Add(compartment2);
            manhole1.Compartments.Add(compartment3);
            manhole2.Compartments.Add(compartment4);
            manhole3.Compartments.Add(compartment5);
            manhole4.Compartments.Add(compartment6);
            manhole5.Compartments.Add(compartment7);

            var connection1 = new SewerConnection { Name = "Con1", SourceCompartment = compartment4, TargetCompartment = compartment2, Length = 11 };
            var connection2 = new SewerConnection { Name = "Con2", SourceCompartment = compartment5, TargetCompartment = compartment2, Length = 12 };
            var connection3 = new SewerConnection { Name = "Con3", SourceCompartment = compartment6, TargetCompartment = compartment3, Length = 13 };
            var connection4 = new SewerConnection { Name = "Con4", SourceCompartment = compartment1, TargetCompartment = compartment7, Length = 14 };
            var connection5 = new SewerConnection { Name = "Con5", SourceCompartment = compartment3, TargetCompartment = compartment2, Length = 1 };
            var connection6 = new SewerConnection { Name = "Con6", SourceCompartment = compartment2, TargetCompartment = compartment1, Length = 1 };

            network.Nodes.AddRange(new[] { manhole1, manhole2, manhole3, manhole4, manhole5 });
            network.Branches.AddRange(new[] { connection1, connection2, connection3, connection4, connection5, connection6 });

            var discretization = new Discretization { Network = network, SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered};

            // add default grid
            discretization.Locations.AddValues(discretization.GenerateSewerConnectionNetworkLocations());
            discretization.UpdateNetworkLocations(discretization.Locations.Values, false);

            // Act
            var mesh = discretization.CreateDisposable1DMeshGeometry();

            // Assert
            Assert.AreEqual(7, mesh.NodeIds.Length);
            Assert.AreEqual(6, mesh.EdgeBranchIds.Length);

            int[] nodes = mesh.EdgeNodes;

            // Con1
            Assert.AreEqual(0,nodes[0]);
            Assert.AreEqual(1,nodes[1]);

            // con2
            Assert.AreEqual(2,nodes[2]);
            Assert.AreEqual(1,nodes[3]);

            // con3
            Assert.AreEqual(3,nodes[4]);
            Assert.AreEqual(4,nodes[5]);

            // con4
            Assert.AreEqual(6,nodes[6]);
            Assert.AreEqual(5,nodes[7]);

            // con5
            Assert.AreEqual(4,nodes[8]);
            Assert.AreEqual(1,nodes[9]);

            // con6
            Assert.AreEqual(1,nodes[10]);
            Assert.AreEqual(6,nodes[11]);
        }

        private static IHydroNetwork CreateTestNetwork()
        {
            //          Rural network             Urban network
            // ^                                   
            // |  100       o node1      o  node2       o manhole1
            // y             \          /               |                           (manhole2 has 2 compartments)
            //         branch1 \      / branch2         | pipe1
            //                   \  /                   |
            //     50             o node3               o--------o pipe2
            //                     \              manhole2    manhole3
            //                       \ branch3
            //                         \
            //    0                     o node4
            //                   
            //             0     50    100            200       300 
            //                                          x -->

            //Arrange

            var network = new HydroNetwork();

            var node1 = new HydroNode("node1") { Network = network, LongName = "node1 long", Geometry = new Point(0, 100) };
            var node2 = new HydroNode("node2") { Network = network, LongName = "node2 long", Geometry = new Point(100, 100) };
            var node3 = new HydroNode("node3") { Network = network, LongName = "node3 long", Geometry = new Point(50, 50) };
            var node4 = new HydroNode("node4") { Network = network, LongName = "node4 long", Geometry = new Point(100, 0) };

            var compartment1 = new Compartment("compartment1");
            var compartment2 = new Compartment("compartment2");
            var compartment3 = new Compartment("compartment3");
            var compartment4 = new Compartment("compartment4");

            var manhole1 = new Manhole("manhole1")
            {
                Geometry = new Point(200, 100),
                Network = network,
                Compartments = new EventedList<ICompartment> { compartment1 }
            };

            var manhole2 = new Manhole("manhole2")
            {
                Geometry = new Point(200, 50),
                Network = network,
                Compartments = new EventedList<ICompartment> { compartment2, compartment3 }
            };

            var manhole3 = new Manhole("manhole3")
            {
                Geometry = new Point(300, 50),
                Network = network,
                Compartments = new EventedList<ICompartment> { compartment4 }
            };

            network.Nodes = new EventedList<INode>(new INode[]
            {
                node1, node2, node3, node4,
                manhole1, manhole2,manhole3
            });

            network.Branches = new EventedList<IBranch>
            {
                new Channel(node1,node3)
                {
                    Name = "branch1",
                    Description = "branch1 long",
                    OrderNumber = 1,
                    Network = network,
                    Geometry = new LineString(new[] {node1.Geometry.Coordinate, node3.Geometry.Coordinate})
                },
                new Channel(node2,node3)
                {
                    Name = "branch2",
                    Description = "branch2 long",
                    OrderNumber = 2,
                    Network = network,
                    Geometry = new LineString(new[] {node2.Geometry.Coordinate, node3.Geometry.Coordinate})
                },
                new Channel(node3,node4)
                {
                    Name = "branch3",
                    Description = "branch3 long",
                    OrderNumber = 3,
                    Network = network,
                    Geometry = new LineString(new[] {node3.Geometry.Coordinate, node4.Geometry.Coordinate})
                },

                new Pipe
                {
                    Name = "pipe1",
                    SourceCompartment = compartment1,
                    TargetCompartment = compartment2,
                    Network = network,
                    Geometry = new LineString(new[] {manhole1.Geometry.Coordinate, manhole2.Geometry.Coordinate})
                },
                new Pipe
                {
                    Name = "pipe2",
                    SourceCompartment = compartment3,
                    TargetCompartment = compartment4,
                    Network = network,
                    Geometry = new LineString(new[] { manhole2.Geometry.Coordinate, manhole3.Geometry.Coordinate})
                },
                new SewerConnection
                {
                    Name = "innerConnection1",
                    SourceCompartment = compartment2,
                    TargetCompartment = compartment3,
                    Network = network,
                    Geometry = new LineString(new[] { manhole2.Geometry.Coordinate, manhole2.Geometry.Coordinate})
                }
            };

            return network;
        }
    }
}