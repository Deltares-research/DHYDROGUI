using System;
using System.Runtime.InteropServices;

namespace DeltaShell.NGHS.IO.Grid
{
    public class GridGeomWrapper: IGridGeomWrapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="meshId"></param>
        /// <param name="meshgeom"></param>
        /// <param name="includeArrays"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_meshgeom",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_meshgeom_dll(ref int ioncid, ref int meshid, ref meshgeom meshgeom, ref bool includeArrays);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="meshId"></param>
        /// <param name="meshgeomdim"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_meshgeom_dim", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_meshgeom_dim_dll(ref int ioncid, ref int meshId, [In] ref meshgeomdim meshgeomdim);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="meshgeom"></param>
        /// <param name="meshgeomdim"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.LIB_DLL_NAME, EntryPoint = "ggeo_convert", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_convert_dll([In, Out] ref meshgeom meshgeom, [In] ref meshgeomdim meshgeomdim);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [DllImport(GridApiDataSet.LIB_DLL_NAME, EntryPoint = "ggeo_make1D2Dinternalnetlinks", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_make1D2Dinternalnetlinks_dll();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [DllImport(GridApiDataSet.LIB_DLL_NAME, EntryPoint = "ggeo_convert_1d_arrays", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_convert_1d_arrays_dll([In] ref IntPtr c_meshXCoords, [In] ref IntPtr c_meshYCoords, [In] ref IntPtr c_branchids, [In] ref int nbranches, [In] ref int nmeshnodes);

        /// <summary>
        /// The number of links
        /// </summary>
        /// <param name="nlinks"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.LIB_DLL_NAME, EntryPoint = "ggeo_get_links_count", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_get_links_count_dll([In, Out] ref int nlinks);

        [DllImport(GridApiDataSet.LIB_DLL_NAME, EntryPoint = "ggeo_get_links", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_get_links_dll([In, Out] ref IntPtr arrayfrom, [In, Out] ref IntPtr arrayto, [In] ref int nlinks);

        [StructLayout(LayoutKind.Sequential)]
        public struct meshgeom
        {
            public IntPtr edge_nodes;
            public IntPtr face_nodes;
            public IntPtr edge_faces;
            public IntPtr face_edges;
            public IntPtr face_links;

            public IntPtr branchids;
            public IntPtr nbranchgeometrynodes;

            public IntPtr nodex;
            public IntPtr nodey;
            public IntPtr nodez;
            public IntPtr edgex;
            public IntPtr edgey;
            public IntPtr edgez;
            public IntPtr facex;
            public IntPtr facey;
            public IntPtr facez;

            public IntPtr branchoffsets;
            public IntPtr geopointsX;
            public IntPtr geopointsY;
            public IntPtr branchlengths;

            public IntPtr layer_zs;
            public IntPtr interface_zs;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct meshgeomdim
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] meshname;
            public int dim;
            public int numnode;
            public int numedge;
            public int numface;
            public int maxnumfacenodes;
            public int numlayer;
            public int layertype;
            public int nt_nbranches;
            public int nt_ngeometry;
        }

        public int get_meshgeom(int ioncid, int meshId, ref meshgeom meshgeom, bool includeArrays)
        {
            return ionc_get_meshgeom_dll(ref ioncid, ref meshId, ref meshgeom, ref includeArrays);
        }

        public int get_meshgeom_dim(int ioncid, int meshId, ref meshgeomdim meshgeomdim)
        {
            return ionc_get_meshgeom_dim_dll(ref ioncid, ref meshId, ref meshgeomdim);
        }

        public int Convert(ref meshgeom c_meshgeom, ref meshgeomdim c_meshgeomdim)
        {
            int ierr = ggeo_convert_dll(ref c_meshgeom, ref c_meshgeomdim);
            return ierr;
        }

        public int Make1d2dInternalnetlinks()
        {
            int ierr = ggeo_make1D2Dinternalnetlinks_dll();
            return ierr;
        }

        public int Convert1dArray(ref IntPtr c_meshXCoords, ref IntPtr c_meshYCoords, ref IntPtr c_branchids,
            ref IntPtr c_sourcenodeid, ref IntPtr c_targetnodeid, ref int nbranches, ref int nmeshnodes)
        {
            int ierr = ggeo_convert_1d_arrays_dll(ref c_meshXCoords, ref c_meshYCoords, ref c_branchids, ref nbranches, ref nmeshnodes);
            return ierr;
        }

        public int GetLinkCount(ref int nbranches)
        {
            int ierr = ggeo_get_links_count_dll(ref nbranches);
            return ierr;
        }

        public int Get1d2dLinks(ref IntPtr arrayfrom, ref IntPtr arrayto, ref int nlinks)
        {
            int ierr = ggeo_get_links_dll(ref arrayfrom, ref arrayto, ref nlinks);
            return ierr;
        }

    }
}