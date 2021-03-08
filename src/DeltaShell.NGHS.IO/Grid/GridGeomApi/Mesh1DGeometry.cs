using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using ProtoBuf;

namespace DeltaShell.NGHS.IO.Grid.GridGeomApi
{
    [ProtoContract(AsReferenceDefault = true)]
    public sealed class Mesh1DGeometry : DisposableMeshObject
    {
        public Mesh1DGeometry(IDiscretization discretization)
        {
            var discretisationPoints = discretization.Locations.AllValues.ToList();
            var branches = discretization.Network.Branches;

            var branchesIndexLookup = branches.ToIndexDictionary();

            var compartments = discretization.Network.Nodes.OfType<Manhole>().SelectMany(m => m.Compartments).OfType<object>();
            var networkNodes = discretization.Network.Nodes.OfType<HydroNode>();
            var nodesIndexLookup = networkNodes.Concat(compartments).ToList().ToIndexDictionary();

            nBranches = branches.Count;
            branchLength = new double[nBranches];
            sourcenodeid = new int[nBranches];
            targetnodeid = new int[nBranches];

            for (int i = 0; i < nBranches; i++)
            {
                var branch = branches[i];
                branchLength[i] = branch.Length;

                object source = branch.Source;
                object target = branch.Target;

                if (branch is SewerConnection connection)
                {
                    source = (object) connection.SourceCompartment ?? connection.Source;
                    target = (object) connection.TargetCompartment ?? connection.Target;
                }

                sourcenodeid[i] = nodesIndexLookup[source];
                targetnodeid[i] = nodesIndexLookup[target];
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
                branchOffset[i] = point.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(point.Chainage); 
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
            return new Mesh1DGeometryNative
            {
                meshXCoords = GetPinnedObjectPointer(meshXCoords),
                meshYCoords = GetPinnedObjectPointer(meshYCoords),
                branchOffset = GetPinnedObjectPointer(branchOffset),
                branchLength = GetPinnedObjectPointer(branchLength),
                branchIds = GetPinnedObjectPointer(branchIds),
                sourcenodeid = GetPinnedObjectPointer(sourcenodeid),
                targetnodeid = GetPinnedObjectPointer(targetnodeid),
                nBranches = nBranches,
                nMeshPoints = nMeshPoints
            };
        }
    }
}