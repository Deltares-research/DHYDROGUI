using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGrid1D2DLinksApi: GridApi, IUGrid1D2DLinksApi
    {
        public int Create1D2DLinks(int fileIdx, int linkmeshIdx, string linkmeshname, int numberOf1D2DLinks, int mesh1Idx, int mesh2Idx,
            int locationType1Id, int locationType2Id)
        {
            throw new NotImplementedException();
        }

        public int Write1D2DLinks(int fileIdx, int mesh2DIdx, int[] mesh1DPointIdx, int[] mesh2DFaceIdx, int[] linkType,
            string[] linkName, int numberOf1D2DLinks)
        {
            throw new NotImplementedException();
        }

        public int GetNumberOf1D2DLinks(int fileIdx, int linkmeshIdx, out int numberOf1D2DLinks)
        {
            throw new NotImplementedException();
        }

        public int Read1D2DLinks(int fileIdx, int linkmeshIdx, out int[] mesh1DPointIdx, out int[] mesh2DFaceIdx, out int[] linkTYpe,
            out string[] linkName, int numberOf1D2DLinks)
        {
            throw new NotImplementedException();
        }
    }
}
