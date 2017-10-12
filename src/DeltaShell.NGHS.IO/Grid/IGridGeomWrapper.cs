using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IGridGeomWrapper
    {
        int Convert(ref GridWrapper.meshgeom c_meshgeom, ref GridWrapper.meshgeomdim c_meshgeomdim);
        int Make1d2dInternalnetlinks();
        int Convert1dArray(ref IntPtr c_meshXCoords, ref IntPtr c_meshYCoords, ref IntPtr c_branchoffset, ref IntPtr c_branchlength, ref IntPtr c_branchids, ref IntPtr c_sourcenodeid, ref IntPtr c_targetnodeid, ref int nbranches, ref int nmeshnodes, ref int start_index);
        int GetLinkCount(ref int nbranches);
        int Get1d2dLinks(ref IntPtr arrayfrom, ref IntPtr arrayto, ref int nlinks);
        int DeallocateMemory();

        int CreateEdgeNodes(ref IntPtr c_branchoffset, ref IntPtr c_branchlength, ref IntPtr c_branchids,
            ref IntPtr c_sourceNodeId, ref IntPtr c_targetNodeId, ref IntPtr c_edgenodes, ref int nBranches,
            ref int nNodes, ref int nEdgeNodes);
    }
}