using System;

namespace DeltaShell.NGHS.IO.Grid.GridGeomApi
{
    public class Mesh1DGeometryNative
    {
        public IntPtr meshXCoords;

        public IntPtr meshYCoords;

        public IntPtr branchOffset;

        public IntPtr branchLength;

        public IntPtr branchIds;

        public IntPtr sourcenodeid;

        public IntPtr targetnodeid;

        public int nBranches;

        public int nMeshPoints;
    }
}