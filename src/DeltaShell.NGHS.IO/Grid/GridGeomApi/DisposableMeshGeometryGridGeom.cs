using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.UGrid.Api;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using ProtoBuf;

namespace DeltaShell.NGHS.IO.Grid.GridGeomApi
{
    /// <summary>
    /// Copy of official <see cref="SharpMap.Api.DisposableMeshGeometry"/> using custom
    /// <see cref="GridWrapper.meshgeomdim"/> and <see cref="GridWrapper.meshgeom"/> structures
    /// </summary>
    [ProtoContract(AsReferenceDefault = true)]
    public class DisposableMeshGeometryGridGeom : DisposableMeshObject
    {
        [ProtoMember(1)]
        public double[] xNodes;

        [ProtoMember(2)]
        public double[] yNodes;

        [ProtoMember(3)]
        public double[] zNodes;

        [ProtoMember(4)]
        public int[] edgeNodes;

        [ProtoMember(5)]
        public int[] faceNodes;

        [ProtoMember(6)]
        public double[] faceX;

        [ProtoMember(7)]
        public double[] faceY;

        [ProtoMember(8)]
        public int maxNumberOfFaceNodes;

        [ProtoMember(9)]
        public int numberOfFaces;

        [ProtoMember(10)]
        public int numberOfNodes;

        [ProtoMember(11)]
        public int numberOfEdges;

        public DisposableMeshGeometryGridGeom(UnstructuredGrid grid)
        {
            CreateNodeArrays(grid.Vertices);
            CreateEdgeArrays(grid.Edges);
            CreateCellArrays(grid.Cells);
        }

        public meshgeomdim CreateMeshDimensions()
        {
            return new meshgeomdim()
            {
                dim = 2, //-> Type of grid 1d (=1)/2d (=2)
                numnode = numberOfNodes,
                numedge = numberOfEdges,
                numface = numberOfFaces,
                maxnumfacenodes = maxNumberOfFaceNodes,
                numlayer = 1,
                layertype = 1,
                nnodes = 0,
                nbranches = 0,
                ngeometry = 0
            };
        }

        public meshgeom CreateMeshGeometry()
        {
            return new meshgeom
            {
                nodex = GetPinnedObjectPointer(xNodes),
                nodey = GetPinnedObjectPointer(yNodes),
                nodez = GetPinnedObjectPointer(zNodes),

                edge_nodes = GetPinnedObjectPointer(edgeNodes),

                facex = GetPinnedObjectPointer(faceX),
                facey = GetPinnedObjectPointer(faceY),
                face_nodes = GetPinnedObjectPointer(faceNodes),
            };
        }

        private void CreateCellArrays(IList<Cell> gridCells)
        {
            numberOfFaces = gridCells.Count;
            maxNumberOfFaceNodes = gridCells.Count > 0 ? gridCells.Max(c => c.VertexIndices.Length) : 0;

            faceNodes = new int[maxNumberOfFaceNodes * gridCells.Count];
            faceX = new double[gridCells.Count];
            faceY = new double[gridCells.Count];

            for (var i = 0; i < gridCells.Count; i++)
            {
                var offset = i * maxNumberOfFaceNodes;

                var cell = gridCells[i];
                for (int j = 0; j < cell.VertexIndices.Length; j++)
                {
                    faceNodes[offset + j] = cell.VertexIndices[j];
                }

                faceX[i] = cell.CenterX;
                faceY[i] = cell.CenterY;
            }
        }

        private void CreateEdgeArrays(IList<Edge> gridEdges)
        {
            numberOfEdges = gridEdges.Count;
            edgeNodes = gridEdges.SelectMany(e => new[] { e.VertexFromIndex, e.VertexToIndex }).ToArray();
        }

        private void CreateNodeArrays(IList<Coordinate> coordinates)
        {
            numberOfNodes = coordinates.Count;

            xNodes = new double[coordinates.Count];
            yNodes = new double[coordinates.Count];
            zNodes = new double[coordinates.Count];

            for (int i = 0; i < coordinates.Count; i++)
            {
                var vertex = coordinates[i];
                xNodes[i] = vertex.X;
                yNodes[i] = vertex.Y;
            }
        }
    }
}