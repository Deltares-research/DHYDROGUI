using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class DisposableMeshGeometryGridGeom : IDisposable
    {
        private readonly List<GCHandle> objectGarbageCollectHandles = new List<GCHandle>();

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

        public DisposableMeshGeometryGridGeom()
        {

        }

        public DisposableMeshGeometryGridGeom(UnstructuredGrid grid)
        {
            CreateNodeArrays(grid.Vertices);
            CreateEdgeArrays(grid.Edges);
            CreateCellArrays(grid.Cells);
        }

        public bool IsMemoryPinned
        {
            get { return objectGarbageCollectHandles.Count > 0; }
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
            if (!IsMemoryPinned)
            {
                PinMemory();
            }

            var lookup = objectGarbageCollectHandles.ToDictionary(h => h.Target, h => h);

            return new meshgeom
            {
                nodex = lookup[xNodes].AddrOfPinnedObject(),
                nodey = lookup[yNodes].AddrOfPinnedObject(),
                nodez = lookup[zNodes].AddrOfPinnedObject(),

                edge_nodes = lookup[edgeNodes].AddrOfPinnedObject(),

                facex = lookup[faceX].AddrOfPinnedObject(),
                facey = lookup[faceY].AddrOfPinnedObject(),
                face_nodes = lookup[faceNodes].AddrOfPinnedObject(),
            };
        }

        public void UnPinMemory()
        {
            foreach (var handle in objectGarbageCollectHandles)
            {
                handle.Free();
            }

            objectGarbageCollectHandles.Clear();
        }

        public void Dispose()
        {
            UnPinMemory();
        }

        private void PinMemory()
        {
            // compensate for null arrays
            xNodes = GetArray(xNodes);
            yNodes = GetArray(yNodes);
            zNodes = GetArray(zNodes);

            edgeNodes = GetArray(edgeNodes);

            faceNodes = GetArray(faceNodes);
            faceX = GetArray(faceX);
            faceY = GetArray(faceY);

            objectGarbageCollectHandles.Add(GCHandle.Alloc(xNodes, GCHandleType.Pinned));
            objectGarbageCollectHandles.Add(GCHandle.Alloc(yNodes, GCHandleType.Pinned));
            objectGarbageCollectHandles.Add(GCHandle.Alloc(zNodes, GCHandleType.Pinned));

            objectGarbageCollectHandles.Add(GCHandle.Alloc(edgeNodes, GCHandleType.Pinned));

            objectGarbageCollectHandles.Add(GCHandle.Alloc(faceNodes, GCHandleType.Pinned));
            objectGarbageCollectHandles.Add(GCHandle.Alloc(faceX, GCHandleType.Pinned));
            objectGarbageCollectHandles.Add(GCHandle.Alloc(faceY, GCHandleType.Pinned));
        }

        private static T[] GetArray<T>(T[] array)
        {
            return array ?? new T[0];
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