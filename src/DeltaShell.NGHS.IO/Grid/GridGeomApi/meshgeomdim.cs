using System.Runtime.InteropServices;

namespace DeltaShell.NGHS.IO.Grid.GridGeomApi
{
    [StructLayout(LayoutKind.Sequential)]
    public struct meshgeomdim
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
        public char[] name;
        public int dim;
        public int numnode;
        public int numedge;
        public int numface;
        public int maxnumfacenodes;
        public int numlayer;
        public int layertype;
        public int nnodes;
        public int nbranches;
        public int ngeometry;
        public int epgs;
        public int numlinks;
    }
}