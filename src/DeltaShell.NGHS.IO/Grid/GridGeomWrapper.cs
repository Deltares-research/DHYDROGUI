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
        public static extern int ggeo_convert_dll([In, Out] ref GridWrapper.meshgeom meshgeom, [In] ref GridWrapper.meshgeomdim meshgeomdim, ref int startIndex);

        /// <summary>
        /// Makes embedded 1-1 the 1d2d links 
        /// </summary>
        /// <returns></returns>
        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_make1D2Dinternalnetlinks", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_make1D2Dinternalnetlinks_dll(ref int c_npl, [In] ref IntPtr c_xpl, [In] ref IntPtr c_ypl, [In] ref IntPtr c_zpl, [In] ref int c_nOneDMask, [In] ref IntPtr c_oneDmask, ref int c_jsferic, ref int c_jasfer3D, ref int c_jglobe);

        /// <summary>
        /// Makes embedded 1-n 1d2d links. 1d-2d internal connections.With this function multiple 2d cells can be connected to 1d mesh points. all the cell crossing the 1d links will be connected to the closest 1d point.
        /// <summary>
        /// Please note that the gridgeom library has to be initialized before this function can be called.
        /// c_jsferic:: 2d sferic flag (1 = spheric / 0 = cartesian)
        ///c_jasfer3D  :: 3d sferic flag (1 = advanced spheric algorithm, 0 = default spheric algorithm )
        /// c_nOneDMask::size of the 1d mask for mesh 1d
        ///c_oneDmask::mask for 1d mesh points(1 = potential connection, 0 = do not connect)
        /// <returns></returns>
        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_make1D2Dembeddedlinks", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_make1D2Dembeddedlinks_dll(ref int c_jsferic, ref int c_jasfer3D, [In] ref int c_nOneDMask, [In] ref IntPtr c_oneDmask);

        /// <summary>
        /// Makes lateral 1d2d links. 1d-2d river connections connections.With this function multiple 2d boundary cells can be connected to 1d mesh points. 
        ///Please note that the gridgeom library has to be initialized before this function can be called.
        /// </summeray>
        ///c_jsferic:: 2d sferic flag (1 = spheric / 0 = cartesian)
        ///c_jasfer3D     :: 3d sferic flag (1 = advanced spheric algorithm, 0 = default spheric algorithm )
        ///c_searchRadius::the search radius for making links
        ///c_nOneDMask::size of the 1d mask for mesh points
        ///c_oneDmask::mask for 1d mesh points(1 = potential connection, 0 = do not connect)
        /// <returns></returns>
        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_make1D2DRiverLinks", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_make1D2DRiverLinks_dll(ref int c_jsferic, ref int c_jasfer3D, ref int c_searchRadius, [In] ref int c_nOneDMask, [In] ref IntPtr c_oneDmask);

        /// <summary>
        /// 1d2d links gullies - 1d.
        /// </summary>
        /// <param name="cNin">The c nin.</param>
        /// <param name="cXpl">The c XPL.</param>
        /// <param name="cYpl">The c ypl.</param>
        /// <param name="c_nOneDMask">The c n one d mask.</param>
        /// <param name="cMesh1DIndexesMask">The c mesh1 d indexes mask.</param>
        /// <param name="c_jsferic">The c jsferic.</param>
        /// <param name="c_jasfer3D">The c jasfer3 d.</param>
        /// <param name="c_jglobe">The c jglobe.</param>
        /// <returns></returns>
        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_make1D2Dstreetinletpipes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_make1D2Dstreetinletpipes_dll(ref int c_npl, [In] ref IntPtr c_xpl, [In] ref IntPtr c_ypl, [In] ref int c_nOneDMask, [In] ref IntPtr c_oneDmask, ref int c_jsferic, ref int c_jasfer3D, ref int c_jglobe);


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
        public static extern int ggeo_get_links_count_dll([In, Out] ref int nlinks, [In, Out] ref int linktype);

        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_get_links", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_get_links_dll([In, Out] ref IntPtr arrayfrom, [In, Out] ref IntPtr arrayto, [In] ref int nlinks, [In, Out] ref int linktype, ref int startindex);

        /// <summary>
        /// Clears the memory to allow new links being generated.
        /// </summary>
        /// <returns></returns>
        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_deallocate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_deallocate();

        /// <summary>
        /// Algorithm to create the edge_nodes from the branchid
        /// </summary>
        /// <param name="c_branchoffset">The c branchoffset.</param>
        /// <param name="c_branchlength">The c branchlength.</param>
        /// <param name="c_branchids">The c branchids.</param>
        /// <param name="c_nedgenodes">The nedgenodes = edges * 2.</param>
        /// <param name="c_edgenodes">The c edgenodes.</param>
        /// <param name="nBranches">The n branches.</param>
        /// <param name="nNodes">The n nodes.</param>
        /// <param name="nEdgeNodes">The network edge nodes.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns></returns>
        [DllImport(GridGeomApi.LIB_DLL_NAME, EntryPoint = "ggeo_create_edge_nodes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ggeo_create_edge_nodes_dll([In] ref IntPtr c_branchoffset, [In] ref IntPtr c_branchlength, [In] ref IntPtr c_branchids, [In] ref IntPtr c_nedge_nodes, [In, Out] ref IntPtr c_edgenodes, [In] ref int nBranches, [In] ref int nNodes, [In] ref int nEdgeNodes, [In] ref int startIndex);

        public int CreateEdgeNodes(ref IntPtr c_branchoffset, ref IntPtr c_branchlength, ref IntPtr c_branchids, ref IntPtr c_network_edgenodes, ref IntPtr c_mesh1D_edgenodes, ref int nBranches, ref int n_mesh1D_Edges, ref int nEdges, ref int startIndex)
        {
            int ierr = ggeo_create_edge_nodes_dll(ref c_branchoffset, ref c_branchlength, ref c_branchids, ref c_network_edgenodes, ref c_mesh1D_edgenodes, ref nBranches, ref n_mesh1D_Edges, ref nEdges, ref startIndex);
            return ierr;
        }

        public int DeallocateMemory()
        {
            int ierr = ggeo_deallocate();
            return ierr;
        }

        public int Convert(ref GridWrapper.meshgeom c_meshgeom, ref GridWrapper.meshgeomdim c_meshgeomdim, ref int startIndex)
        {
            int ierr = ggeo_convert_dll(ref c_meshgeom, ref c_meshgeomdim, ref startIndex);
            return ierr;
        }

        public int Make1D2DEmbeddedOneToOneLinks(ref int c_nin, ref IntPtr c_xpl, ref IntPtr c_ypl, ref IntPtr c_zpl, ref int intnFilterMesh1DPoints, ref IntPtr intPtrfilterMesh1DPoints)
        {
            int c_jsferic = 0;
            int c_jasfer3D = 0;
            int c_jglobe = 0;
            int ierr = ggeo_make1D2Dinternalnetlinks_dll(ref c_nin, ref c_xpl, ref c_ypl, ref c_zpl, ref intnFilterMesh1DPoints, ref intPtrfilterMesh1DPoints, ref c_jsferic, ref c_jasfer3D, ref c_jglobe);
            return ierr;
        }

        public int Make1D2DEmbeddedOneToManyLinks(ref int c_nin, ref IntPtr c_xpl, ref IntPtr c_ypl, ref IntPtr c_zpl, ref int intnFilterMesh1DPoints, ref IntPtr intPtrfilterMesh1DPoints)
        {
            int c_jsferic = 0;
            int c_jasfer3D = 0;
            int c_jglobe = 0;
            int ierr = ggeo_make1D2Dembeddedlinks_dll(ref c_jsferic, ref c_jasfer3D, ref intnFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
            return ierr;
        }

        public int Make1D2DLateralLinks(ref int c_nin, ref IntPtr c_xpl, ref IntPtr c_ypl, ref IntPtr c_zpl, ref int intnFilterMesh1DPoints, ref IntPtr intPtrfilterMesh1DPoints)
        {
            int c_jsferic = 0;
            int c_jasfer3D = 0;
            int c_searchRadius = 100;
            int ierr = ggeo_make1D2DRiverLinks_dll(ref c_jsferic, ref c_jasfer3D, ref c_searchRadius, ref intnFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
            return ierr;
        }

        public int Make1D2DGullyLinks(ref int nCoordinatesGullies, ref IntPtr intPtrXValuesGullies, ref IntPtr intPtrYValuesGullies, ref int intnFlterMesh1DPoints, ref IntPtr intPtrfilterMesh1DPoints)
        {
            int c_jsferic = 0;
            int c_jasfer3D = 0;
            int c_jglobe = 0;
            int ierr = ggeo_make1D2Dstreetinletpipes_dll(ref nCoordinatesGullies, ref intPtrXValuesGullies, ref intPtrYValuesGullies, ref intnFlterMesh1DPoints, ref intPtrfilterMesh1DPoints, ref c_jsferic, ref c_jasfer3D, ref c_jglobe);
            return ierr;
        }

        public int Convert1dArray(ref IntPtr c_meshXCoords, ref IntPtr c_meshYCoords, ref IntPtr c_branchoffset, ref IntPtr c_branchlength,
            ref IntPtr c_branchids, ref IntPtr c_sourcenodeid, ref IntPtr c_targetnodeid, ref int nbranches, ref int nmeshnodes, ref int start_index)
        {
            int ierr = ggeo_convert_1d_arrays_dll(ref c_meshXCoords, ref c_meshYCoords, ref c_branchoffset, ref c_branchlength, ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nbranches, ref nmeshnodes, ref start_index);
            return ierr;
        }

        public int GetLinkCount(ref int nbranches, ref int linkType)
        {
            int ierr = ggeo_get_links_count_dll(ref nbranches, ref linkType);
            return ierr;
        }

        public int Get1d2dLinks(ref IntPtr arrayfrom, ref IntPtr arrayto, ref int nlinks, ref int linkType, ref int startindex)
        {
            int ierr = ggeo_get_links_dll(ref arrayfrom, ref arrayto, ref nlinks, ref linkType, ref startindex);
            return ierr;
        }
    }
}