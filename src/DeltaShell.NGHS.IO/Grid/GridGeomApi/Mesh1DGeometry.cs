using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using ProtoBuf;

namespace DeltaShell.NGHS.IO.Grid.GridGeomApi
{
    [ProtoContract(AsReferenceDefault = true)]
    public class Mesh1DGeometry
    {
        private readonly List<GCHandle> objectGarbageCollectHandles = new List<GCHandle>();

        public Mesh1DGeometry(IDiscretization discretization)
        {
            var discretisationPoints = discretization.Locations.AllValues.ToList();
            var branches = discretization.Network.Branches;

            var branchesIndexLookup = branches.ToIndexDictionary();
            var nodesIndexLookup = discretization.Network.Nodes.ToIndexDictionary();

            nBranches = branches.Count;
            branchLength = new double[nBranches];
            sourcenodeid = new int[nBranches];
            targetnodeid = new int[nBranches];

            for (int i = 0; i < nBranches; i++)
            {
                var branch = branches[i];
                branchLength[i] = branch.Length;
                sourcenodeid[i] = nodesIndexLookup[branch.Source];
                targetnodeid[i] = nodesIndexLookup[branch.Target];
            }

            nMeshPoints = discretisationPoints.Count;

            branchIds = new int[nMeshPoints];
            meshXCoords = new double[nMeshPoints];
            meshYCoords = new double[nMeshPoints];
            branchOffset = new double[nMeshPoints];

            for (int i = 0; i < nMeshPoints; i++)
            {
                var point = discretisationPoints[i];
                branchIds[i] = branchesIndexLookup[point.Branch];
                meshXCoords[i] = point.Geometry.Coordinate.X;
                meshYCoords[i] = point.Geometry.Coordinate.Y;
                branchOffset[i] = point.Chainage;
            }
        }

        [ProtoMember(1)] public double[] meshXCoords;

        [ProtoMember(2)] public double[] meshYCoords;

        [ProtoMember(3)] public double[] branchOffset;

        [ProtoMember(4)] public double[] branchLength;

        [ProtoMember(5)] public int[] branchIds;

        [ProtoMember(6)] public int[] sourcenodeid;

        [ProtoMember(7)] public int[] targetnodeid;

        [ProtoMember(8)] public int nBranches;

        [ProtoMember(9)] public int nMeshPoints;

        public Mesh1DGeometryNative GetNative()
        {
            if (!IsMemoryPinned)
            {
                PinMemory();
            }

            var dictionary = objectGarbageCollectHandles.ToDictionary(h => h.Target, h => h);
            return new Mesh1DGeometryNative
            {
                meshXCoords = dictionary[meshXCoords].AddrOfPinnedObject(),
                meshYCoords = dictionary[meshYCoords].AddrOfPinnedObject(),
                branchOffset = dictionary[branchOffset].AddrOfPinnedObject(),
                branchLength = dictionary[branchLength].AddrOfPinnedObject(),
                branchIds = dictionary[branchIds].AddrOfPinnedObject(),
                sourcenodeid = dictionary[sourcenodeid].AddrOfPinnedObject(),
                targetnodeid = dictionary[targetnodeid].AddrOfPinnedObject(),
                nBranches = nBranches,
                nMeshPoints = nMeshPoints
            };
        }

        public bool IsMemoryPinned
        {
            get { return objectGarbageCollectHandles.Count > 0; }
        }

        private void PinMemory()
        {
            meshXCoords = GetArray(meshXCoords);
            meshYCoords = GetArray(meshYCoords);
            branchOffset = GetArray(branchOffset);
            branchLength = GetArray(branchLength);
            branchIds = GetArray(branchIds);
            sourcenodeid = GetArray(sourcenodeid);
            targetnodeid = GetArray(targetnodeid);
            
            objectGarbageCollectHandles.Add(GCHandle.Alloc(meshXCoords, GCHandleType.Pinned));
            objectGarbageCollectHandles.Add(GCHandle.Alloc(meshYCoords, GCHandleType.Pinned));
            objectGarbageCollectHandles.Add(GCHandle.Alloc(branchOffset, GCHandleType.Pinned));
            objectGarbageCollectHandles.Add(GCHandle.Alloc(branchLength, GCHandleType.Pinned));
            objectGarbageCollectHandles.Add(GCHandle.Alloc(branchIds, GCHandleType.Pinned));
            objectGarbageCollectHandles.Add(GCHandle.Alloc(sourcenodeid, GCHandleType.Pinned));
            objectGarbageCollectHandles.Add(GCHandle.Alloc(targetnodeid, GCHandleType.Pinned));
        }

        public void UnPinMemory()
        {
            foreach (var garbageCollectHandle in objectGarbageCollectHandles)
            {
                garbageCollectHandle.Free();
            }
            
            objectGarbageCollectHandles.Clear();
        }

        private static T[] GetArray<T>(T[] array)
        {
            return array ?? new T[0];
        }
    }
}