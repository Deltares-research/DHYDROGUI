using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridMesh2DApi : GridApi, IUGridMesh2DApi
    {
        private int mesh2DIdForWriting;

        public UGridMesh2DApi()
        {
            mesh2DIdForWriting = -1;

        }

        #region Implementation of IUGridMesh2DApi

        #region Write Mesh2D

        public int CreateMesh2D(GridWrapper.meshgeomdim dimensions, GridWrapper.meshgeom data)
        {
            
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            try
            {
             //   var ierr = wrapper.Create2DMesh(ioncId, ref mesh2DIdForWriting, GridApiDataSet.DataSetNames.Mesh2D, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints);
                int networkid = -1;
                int start_index = 1;
                var ierr = wrapper.Create2DMesh(ioncId, ref mesh2DIdForWriting, ref networkid, ref data, ref dimensions, GridApiDataSet.DataSetNames.Mesh2D,"", ref start_index);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                return GridApiDataSet.GridConstants.NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }
        #endregion
        

        #endregion
        
        

        #region Implementation of IDisposable

        public void Dispose()
        {
            Close();
        }

        #endregion


        public virtual bool Mesh2DReadyForWriting
        {
            get { return mesh2DIdForWriting > 0; }
        }
    }
    public static class MeshgeomMemoryManager
    {

        public static void allocate(ref GridWrapper.meshgeomdim meshdim, ref GridWrapper.meshgeom mesh)
        {
            if (meshdim.numnode > 0)
                mesh.nodex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.numnode);
            if (meshdim.numnode > 0)
                mesh.nodey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.numnode);
            if (meshdim.numnode > 0)
                mesh.nodez = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.numnode);
            if (meshdim.numedge > 0)
                mesh.edge_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * meshdim.numedge * 2);
            if (meshdim.numface > 0)
                mesh.face_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * meshdim.maxnumfacenodes * meshdim.numface);
            if (meshdim.numedge > 0)
                mesh.edge_faces = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * meshdim.numedge * 2);
            if (meshdim.numface > 0)
                mesh.face_edges = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * meshdim.maxnumfacenodes * meshdim.numface);
            if (meshdim.numface > 0)
                mesh.face_links = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * meshdim.maxnumfacenodes * meshdim.numface);
            if (meshdim.nnodes > 0)
                mesh.nodex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.nnodes);
            if (meshdim.nnodes > 0)
                mesh.nodey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.nnodes);
            if (meshdim.nnodes > 0)
                mesh.nodez = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.nnodes);
            if (meshdim.numedge > 0)
                mesh.edgex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.numedge);
            if (meshdim.numedge > 0)
                mesh.edgey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.numedge);
            if (meshdim.numface > 0)
                mesh.facex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.numface);
            if (meshdim.numface > 0)
                mesh.facey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.numface);
            //network part
            if (meshdim.nnodes > 0)
                mesh.nnodex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.nnodes);
            if (meshdim.nnodes > 0)
                mesh.nnodey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.nnodes);
            if (meshdim.nnodes > 0)
                mesh.branchidx = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * meshdim.numnode);
            if (meshdim.nnodes > 0)
                mesh.branchoffsets = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.numnode);
            if (meshdim.nbranches > 0)
                mesh.nbranchlengths = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.nbranches);
            if (meshdim.nbranches > 0)
                mesh.nbranchgeometrynodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * meshdim.nbranches);
            if (meshdim.ngeometry > 0)
                mesh.ngeopointx = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.ngeometry);
            if (meshdim.ngeometry > 0)
                mesh.ngeopointy = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshdim.ngeometry);

            if (meshdim.nbranches > 0)
                mesh.nedge_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * meshdim.nbranches);
            if (meshdim.nbranches > 0)
                mesh.nbranchorder = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * meshdim.nbranches);
        }

        public static void deallocate(ref GridWrapper.meshgeom mesh)
        {
            foreach (var field in typeof(GridWrapper.meshgeom).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var ptr = (IntPtr)field.GetValue(mesh);
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr);
                }
            }
        }
    }
}