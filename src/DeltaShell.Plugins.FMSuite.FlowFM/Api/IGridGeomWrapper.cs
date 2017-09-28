using System;
using DeltaShell.NGHS.IO.Grid;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    public interface IGridGeomWrapper
    {
        int Convert(ref GridWrapper.meshgeom c_meshgeom, ref GridWrapper.meshgeomdim c_meshgeomdim);
        int Make1d2dInternalnetlinks();
        //(ref c_meshXCoords, ref c_meshYCoords, ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nBranches, ref numberOfNodes);
        int Convert1dArray(ref IntPtr c_meshXCoords, ref IntPtr c_meshYCoords, ref IntPtr c_branchids, ref IntPtr c_sourcenodeid, ref IntPtr c_targetnodeid, ref int nbranches, ref int nmeshnodes);
        int GetLinkCount(ref int nbranches);
        int Get1d2dLinks(ref IntPtr arrayfrom, ref IntPtr arrayto, ref int nlinks);
    }
}