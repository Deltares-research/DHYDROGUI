using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using SharpMap.Api.GridGeom;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class UGridFileHelperMesh2D
    {
        /// <summary>
        /// Sets the <paramref name="meshGeometry"/> to the 2D <paramref name="grid"/>
        /// </summary>
        /// <param name="grid">Grid to reset</param>
        /// <param name="meshGeometry">Mesh geometry to use</param>
        /// <param name="recreateCells">If needed we need to recreate the cells because the cell index administration of grid geom (or kernel output) is different.</param>
        public static void SetMesh2DGeometry(UnstructuredGrid grid, Disposable2DMeshGeometry meshGeometry, bool recreateCells)
        {
            UnstructuredGrid tmpGrid = new UnstructuredGrid();
            tmpGrid.Vertices = CreateVertices(meshGeometry);
            tmpGrid.Edges = CreateEdges(meshGeometry);
            if (recreateCells)
            {
                using (var api = new RemoteGridGeomApi())
                using (var mesh = new DisposableMeshGeometry(tmpGrid))
                {
                    DisposableMeshGeometry resultMesh = api.CreateCells(mesh);
                    tmpGrid.Cells = resultMesh.CreateCells();
                }
            }
            else
            {
                tmpGrid.Cells = CreateCells(meshGeometry);
            }
            
            if (tmpGrid.IsEmpty)
            {
                return;
            }

            if (!grid.IsEmpty)
            {
                grid.Clear();
            }
            
            grid.ResetState(tmpGrid.Vertices, tmpGrid.Edges, tmpGrid.Cells, tmpGrid.FlowLinks);
        }
        private static IList<Coordinate> CreateVertices(Disposable2DMeshGeometry mesh)
        {
            bool canNodeArraysBeUsedForCoordinateList = mesh?.NodesX == null
                                                        || mesh.NodesX.Length == 0
                                                        || mesh.NodesY == null
                                                        || mesh.NodesY.Length == 0
                                                        || mesh.NodesX.Length != mesh.NodesY.Length;

            return canNodeArraysBeUsedForCoordinateList
                       ? new List<Coordinate>()
                       : mesh.NodesX.Select((t, i) => new Coordinate(t, mesh.NodesY[i])).ToList();
        }

        private static IList<Edge> CreateEdges(Disposable2DMeshGeometry mesh)
        {
            var edgeList = new ConcurrentDictionary<int, Edge>();
            if (mesh.EdgeNodes == null)
            {
                return edgeList.Values.ToList();
            }

            var numberOfEdges = (int)(mesh.EdgeNodes.Length / 2.0);
            Parallel.For(0, numberOfEdges, blockIndex =>
            {
                int[] blockFromArray = HydroUGridExtensions.GetBlockFromArray(mesh.EdgeNodes, 2, blockIndex);
                edgeList.AddOrUpdate(blockIndex, new Edge(blockFromArray[0], blockFromArray[1]), (i, edge) => edge);
            });
            return edgeList.Values.ToList();
        }
        private static IList<Cell> CreateCells(Disposable2DMeshGeometry mesh, int fillValueMesh2DFaceNodes = (int)UGridFile.DEFAULT_NO_DATA_VALUE)
        {
            var cellList = new ConcurrentDictionary<int, Cell>();
            if (mesh?.FaceNodes == null ||
                mesh.FaceX == null ||
                mesh.FaceY == null ||
                mesh.MaxNumberOfFaceNodes == 0)
            {
                return cellList.Values.ToList();
            }

            var numberOfFaces = mesh.FaceX.Length;
            Parallel.For(0, numberOfFaces, blockIndex =>
            {
                int[] blockFromArray = HydroUGridExtensions.GetBlockFromArray(mesh.FaceNodes, mesh.MaxNumberOfFaceNodes, blockIndex);
                cellList.AddOrUpdate(blockIndex, new Cell(blockFromArray.Where(j => j != fillValueMesh2DFaceNodes).ToArray())
                {
                    CenterX = mesh.FaceX[blockIndex],
                    CenterY = mesh.FaceY[blockIndex]
                }, (i, cell) => cell);
            });
            return cellList.Values.ToList();
        }
    }
}