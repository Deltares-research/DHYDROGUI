using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using MeshKernelNETCore.Api;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.NGHS.IO.Grid.MeshKernel
{
    /// <summary>
    /// Functions to translate our objects to MeshKernelNet objects 
    /// </summary>
    public static class MeshKernelExtensions
    {
        private const double missingValue = -999.0;

        /// <summary>
        /// Translates a <see cref="IGeometry"/> object to a <see cref="DisposableGeometryList"/>
        /// </summary>
        /// <param name="geometry">Geometry to translate</param>
        /// <returns><see cref="DisposableGeometryList"/> containing the <paramref name="geometry"/> data</returns>
        public static DisposableGeometryList CreateDisposableGeometryList(this IGeometry geometry)
        {
            return new[] { geometry }.CreateDisposableGeometryList();
        }

        /// <summary>
        /// Translates a list of <see cref="IGeometry"/> objects to a <see cref="DisposableGeometryList"/>
        /// </summary>
        /// <param name="geometries">Geometries to translate</param>
        /// <returns><see cref="DisposableGeometryList"/> containing the <paramref name="geometries"/> data</returns>
        public static DisposableGeometryList CreateDisposableGeometryList(this IList<IGeometry> geometries)
        {
            var geometryList = new DisposableGeometryList();
            var numberOfPoints = geometries.Count == 0 ? 0 : geometries.Sum(g => g.Coordinates.Length) + geometries.Count;

            geometryList.XCoordinates = new double[numberOfPoints];
            geometryList.YCoordinates = new double[numberOfPoints];
            geometryList.Values = new double[numberOfPoints];

            var index = 0;
            foreach (var geometry in geometries)
            {
                for (int i = 0; i < geometry.Coordinates.Length; i++)
                {
                    geometryList.XCoordinates[index] = geometry.Coordinates[i].X;
                    geometryList.YCoordinates[index] = geometry.Coordinates[i].Y;
                    geometryList.Values[index] = geometry.Coordinates[i].Z;

                    index++;
                }

                geometryList.XCoordinates[index] = missingValue;
                geometryList.YCoordinates[index] = missingValue;
                geometryList.Values[index] = missingValue;
                index++;
            }

            return geometryList;
        }

        /// <summary>
        /// Translates a <see cref="IDiscretization"/> object to a <see cref="DisposableMesh1D"/>
        /// </summary>
        /// <param name="discretization">Discretization to translate</param>
        /// <returns><see cref="DisposableMesh1D"/> containing the <paramref name="discretization"/> data</returns>
        public static DisposableMesh1D CreateDisposableMesh1D(this IDiscretization discretization)
        {
            var locations = discretization.Locations.Values.ToArray();
            var segments = discretization.Segments.Values.ToList();

            var mesh1D = new DisposableMesh1D(locations.Length, segments.Count);
            
            for (int i = 0; i < mesh1D.NumNodes; i++)
            {
                var location = locations[i];

                mesh1D.NodeX[i] = location.Geometry?.Coordinate.X.TruncateByDigits() ?? 0;
                mesh1D.NodeY[i] = location.Geometry?.Coordinate.Y.TruncateByDigits() ?? 0;
            }

            var locationIdLookup = locations.ToIndexDictionary();
            var locationIdxBySegment = new Dictionary<INetworkSegment, int[]>();
            
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];

                locationIdxBySegment[segment] = HydroUGridExtensions.GetLocationIndices(discretization, segment, locationIdLookup, out var doNotWriteTheseSegments);
            }

            var edgeNodeIndex = 0;
            for (int i = 0; i < mesh1D.NumEdges; i++)
            {
                var segment = segments[i];
                
                mesh1D.EdgeNodes[edgeNodeIndex++] = locationIdxBySegment[segment][0];
                mesh1D.EdgeNodes[edgeNodeIndex++] = locationIdxBySegment[segment][1];
            }

            return mesh1D;
        }

        /// <summary>
        /// Translates a <see cref="UnstructuredGrid"/> object to a <see cref="DisposableMesh2D"/>
        /// </summary>
        /// <param name="grid">Grid to translate</param>
        /// <returns><see cref="DisposableMesh2D"/> containing the <paramref name="grid"/> data</returns>
        public static DisposableMesh2D CreateDisposableMesh2D(this UnstructuredGrid grid)
        {
            var mesh2d = new DisposableMesh2D();

            mesh2d.FillWithGrid(grid);

            return mesh2d;
        }

        private static void FillWithGrid(this DisposableMesh2D mesh2d, UnstructuredGrid grid)
        {
            mesh2d.SetNodeArrays(grid.Vertices);
            mesh2d.SetEdgeArrays(grid.Edges);
            mesh2d.SetCellArrays(grid.Cells);
        }

        private static void SetCellArrays(this DisposableMesh2D mesh2d, IList<Cell> gridCells)
        {
            mesh2d.NumFaces = gridCells.Count;
            mesh2d.NumFaceNodes = gridCells.Count > 0 ? gridCells.Max(c => c.VertexIndices.Length) : 0;

            mesh2d.FaceNodes = new int[mesh2d.NumFaceNodes * gridCells.Count];
            mesh2d.FaceX = new double[gridCells.Count];
            mesh2d.FaceY = new double[gridCells.Count];
            var max = 0;

            for (var i = 0; i < gridCells.Count; i++)
            {
                var offset = i * mesh2d.NumFaceNodes;

                var cell = gridCells[i];
                for (int j = 0; j < cell.VertexIndices.Length; j++)
                {
                    mesh2d.FaceNodes[offset + j] = cell.VertexIndices[j];
                }

                mesh2d.FaceX[i] = cell.CenterX;
                mesh2d.FaceY[i] = cell.CenterY;
            }
        }

        private static void SetEdgeArrays(this DisposableMesh2D mesh2d, IList<Edge> gridEdges)
        {
            mesh2d.NumEdges = gridEdges.Count;
            mesh2d.EdgeNodes = gridEdges.SelectMany(e => new[] { e.VertexFromIndex, e.VertexToIndex }).ToArray();
            //mesh2d.EdgeX, mesh2d.EdgeY
        }

        private static void SetNodeArrays(this DisposableMesh2D mesh2d, IList<Coordinate> coordinates)
        {
            mesh2d.NumNodes = coordinates.Count;

            mesh2d.NodeX = new double[coordinates.Count];
            mesh2d.NodeY = new double[coordinates.Count];

            for (int i = 0; i < coordinates.Count; i++)
            {
                var vertex = coordinates[i];
                mesh2d.NodeX[i] = vertex.X;
                mesh2d.NodeY[i] = vertex.Y;
            }
        }
    }
}