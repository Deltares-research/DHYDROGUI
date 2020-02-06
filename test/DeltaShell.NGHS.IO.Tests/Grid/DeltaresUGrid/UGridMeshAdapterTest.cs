using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.NGHS.IO.Tests.Grid.DeltaresUGrid
{
    [TestFixture]
    public class UGridMeshAdapterTest
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

            var grid = disposable2DMeshGeometry.CreateUnstructuredGrid();

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
        public void GivenUGridMeshAdapter_DoingCreateNetwork_ShouldCreateValidNetwork()
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

            var compartmentProperties = new List<NodeFile.CompartmentProperties>
            {
                new NodeFile.CompartmentProperties{ CompartmentId = "compartment1", ManholeId = "manhole1"},
                new NodeFile.CompartmentProperties{ CompartmentId = "compartment2", ManholeId = "manhole2"},
                new NodeFile.CompartmentProperties{ CompartmentId = "compartment3", ManholeId = "manhole2"},
                new NodeFile.CompartmentProperties{ CompartmentId = "compartment4", ManholeId = "manhole3"},
            };

            var branchProperties = new List<BranchFile.BranchProperties>
            {
                new BranchFile.BranchProperties{Name = "pipe1", BranchType = BranchFile.BranchType.Pipe },
                new BranchFile.BranchProperties{Name = "pipe2", BranchType = BranchFile.BranchType.Pipe },
            };

            // Act
            var network = networkGeometry.CreateNetwork(branchProperties,compartmentProperties);
            
            // Assert
            // check dimensions
            Assert.AreEqual(7, network.Nodes.Count);
            Assert.AreEqual(4, network.HydroNodes.Count());
            Assert.AreEqual(3, network.Manholes.Count());
            
            Assert.AreEqual(5, network.Branches.Count);
            Assert.AreEqual(3, network.Channels.Count());
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

            //Arrange
            var node1 = new HydroNode("node1") {LongName = "node1 long", Geometry = new Point(0, 100)};
            var node2 = new HydroNode("node2") {LongName = "node2 long", Geometry = new Point(100, 100)};
            var node3 = new HydroNode("node3") {LongName = "node3 long", Geometry = new Point(50, 50)};
            var node4 = new HydroNode("node4") {LongName = "node4 long", Geometry = new Point(100, 0)};
            
            var compartment1 = new Compartment("compartment1");
            var compartment2 = new Compartment("compartment2");
            var compartment3 = new Compartment("compartment3");
            var compartment4 = new Compartment("compartment4");

            var manhole1 = new Manhole("manhole1")
            {
                Geometry = new Point(200, 100),  
                Compartments = new EventedList<ICompartment> { compartment1 }
            };

            var manhole2 = new Manhole("manhole2") 
            {
                Geometry = new Point(200, 50), 
                Compartments = new EventedList<ICompartment> { compartment2, compartment3 }
            };

            var manhole3 = new Manhole("manhole3")
            {
                Geometry = new Point(300, 50),
                Compartments = new EventedList<ICompartment> { compartment4 }
            };

            var nodes = new INode[]
            {
                node1, node2, node3, node4,
                manhole1, manhole2,manhole3
            };

            var network = new HydroNetwork
            {
                Nodes = new EventedList<INode>(nodes),
                Branches = new EventedList<IBranch>
                {
                    new Branch(node1,node3)
                    {
                        Name = "branch1", 
                        Description = "branch1 long",
                        OrderNumber = 1,
                        Geometry = new LineString(new[] {node1.Geometry.Coordinate, node3.Geometry.Coordinate})
                    },
                    new Branch(node2,node3)
                    {
                        Name = "branch2",
                        Description = "branch2 long",
                        OrderNumber = 2,
                        Geometry = new LineString(new[] {node2.Geometry.Coordinate, node3.Geometry.Coordinate})
                    },
                    new Branch(node3,node4)
                    {
                        Name = "branch3",
                        Description = "branch3 long",
                        OrderNumber = 3,
                        Geometry = new LineString(new[] {node3.Geometry.Coordinate, node4.Geometry.Coordinate})
                    },

                    new Pipe
                    {
                        Name = "pipe1",
                        SourceCompartment = compartment1,
                        TargetCompartment = compartment2,
                        Geometry = new LineString(new[] {manhole1.Geometry.Coordinate, manhole2.Geometry.Coordinate})
                    },
                    new Pipe
                    {
                        Name = "pipe2",
                        SourceCompartment = compartment3,
                        TargetCompartment = compartment4,
                        Geometry = new LineString(new[] { manhole2.Geometry.Coordinate, manhole3.Geometry.Coordinate})
                    }
                }
            };

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
            Assert.AreEqual(manhole2.LongName, networkGeometry.NodeLongNames[compartment1Index]);

            var branch = network.Branches[0];
            var branchIdList = networkGeometry.BranchIds.ToList();

            var branch1Index = branchIdList.IndexOf(branch.Name);
            Assert.AreEqual(branch.Length, networkGeometry.BranchLengths[branch1Index]);
            Assert.AreEqual(branch.OrderNumber, networkGeometry.BranchOrder[branch1Index]);
            Assert.AreEqual(branch.Description, networkGeometry.BranchLongNames[branch1Index]);
            Assert.AreEqual(2, networkGeometry.BranchGeometryNodesCount[branch1Index]);

            var pipe = network.Pipes.First();
            var pipe1Index = branchIdList.IndexOf(pipe.Name);

            Assert.AreEqual(pipe.Length, networkGeometry.BranchLengths[pipe1Index]);
            Assert.AreEqual(2, networkGeometry.BranchGeometryNodesCount[pipe1Index]);
            Assert.AreEqual(pipe.LongName, networkGeometry.BranchLongNames[pipe1Index]);

            var innerConnection1Index = branchIdList.IndexOf("innerConnection1");
            Assert.AreEqual(1, networkGeometry.BranchLengths[innerConnection1Index]);
            Assert.AreEqual(2, networkGeometry.BranchGeometryNodesCount[innerConnection1Index]);
            Assert.AreEqual(1, networkGeometry.NodesFrom[innerConnection1Index]);
            Assert.AreEqual(2, networkGeometry.NodesTo[innerConnection1Index]);
        }
    }
}