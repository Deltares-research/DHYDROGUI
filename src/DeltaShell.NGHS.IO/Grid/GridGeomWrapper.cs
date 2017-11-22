using System;
using System.Runtime.InteropServices;

namespace DeltaShell.NGHS.IO.Grid
{
    public class GridGeomWrapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="meshgeom"></param>
        /// <param name="meshgeomdim"></param>
        /// <returns></returns>
        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_convert", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_convert_dll([In, Out] ref GridWrapper.meshgeom meshgeom, [In] ref GridWrapper.meshgeomdim meshgeomdim);

        /// <summary>
        /// Makes the 1d/2d links (results are stored in memory)
        /// </summary>
        /// <returns></returns>
        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_make1D2Dinternalnetlinks", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_make1D2Dinternalnetlinks_dll();

        /// <summary>
        /// Use 1d array to fill kn matrix
        /// </summary>
        /// <param name="c_meshXCoords">The x coordinates of the mesh points</param>
        /// <param name="c_meshYCoords">The y coordinates of the mesh points</param>
        /// <param name="c_branchids">The branch ids</param>
        /// <param name="nbranches">The number of branches</param>
        /// <param name="nmeshnodes">The number of mesh nodes</param>
        /// <returns></returns>
        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_convert_1d_arrays", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_convert_1d_arrays_dll([In] ref IntPtr c_meshXCoords, [In] ref IntPtr c_meshYCoords, [In] ref IntPtr c_branchoffset, [In] ref IntPtr c_branchlength, [In] ref IntPtr c_branchids, [In] ref IntPtr c_sourcenodeid, [In] ref IntPtr c_targetnodeid, [In] ref int nbranches, [In] ref int nmeshnodes, [In] ref int startIndex);


        /// <summary>
        /// The number of links
        /// </summary>
        /// <param name="nlinks"></param>
        /// <returns></returns>
        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_get_links_count", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_get_links_count_dll([In, Out] ref int nlinks);

        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_get_links", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_get_links_dll([In, Out] ref IntPtr arrayfrom, [In, Out] ref IntPtr arrayto, [In] ref int nlinks);

        /// <summary>
        /// Clears the memory to allow new links being generated.
        /// </summary>
        /// <returns></returns>
        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_deallocate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_deallocate();

        /// <summary>
        /// Algorithm to create the edge_nodes from the branchid
        /// </summary>
        /// <param name="c_branchids"></param>
        /// <param name="c_edgenodes"></param>
        /// <param name="nBranches"></param>
        /// <param name="nNodes"></param>
        /// <param name="nEdgeNodes"></param>
        /// <returns></returns>
        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_create_edge_nodes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_create_edge_nodes_dll([In] ref IntPtr c_branchoffset, [In] ref IntPtr c_branchlength, [In] ref IntPtr c_branchids, [In] ref IntPtr c_sourceNodeId, [In] ref IntPtr c_targetNodeId, [In, Out] ref IntPtr c_edgenodes, [In] ref int nBranches, [In] ref int nNodes, [In] ref int nEdgeNodes, [In] ref int startIndex);

        public int CreateEdgeNodes(ref IntPtr c_branchoffset, ref IntPtr c_branchlength, ref IntPtr c_branchids, ref IntPtr c_sourceNodeId, ref IntPtr c_targetNodeId, ref IntPtr c_edgenodes, ref int nBranches, ref int nNodes, ref int nEdgeNodes, ref int startIndex)
        {
            int ierr = ggeo_create_edge_nodes_dll(ref c_branchoffset, ref c_branchlength, ref c_branchids, ref c_sourceNodeId, ref c_targetNodeId, ref c_edgenodes, ref nBranches, ref nNodes, ref nEdgeNodes, ref startIndex);
            return ierr;
        }

        public int DeallocateMemory()
        {
            int ierr = ggeo_deallocate();
            return ierr;
        }


        public int Convert(ref GridWrapper.meshgeom c_meshgeom, ref GridWrapper.meshgeomdim c_meshgeomdim)
        {
            int ierr = ggeo_convert_dll(ref c_meshgeom, ref c_meshgeomdim);
            return ierr;
        }

        public int Make1d2dInternalnetlinks()
        {
            int ierr = ggeo_make1D2Dinternalnetlinks_dll();
            return ierr;
        }

        public int Convert1dArray(ref IntPtr c_meshXCoords, ref IntPtr c_meshYCoords, ref IntPtr c_branchoffset, ref IntPtr c_branchlength,
            ref IntPtr c_branchids, ref IntPtr c_sourcenodeid, ref IntPtr c_targetnodeid, ref int nbranches, ref int nmeshnodes, ref int start_index)
        {
            int ierr = ggeo_convert_1d_arrays_dll(ref c_meshXCoords, ref c_meshYCoords, ref c_branchoffset, ref c_branchlength, ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nbranches, ref nmeshnodes, ref start_index);
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