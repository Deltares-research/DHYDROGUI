using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridApi : GridApi, IUGridApi
    {
        private double fillValue;
        private int nNodes;
        private int nEdges;
        private int nFaces;
        private int nMaxFaceNodes;

        public UGridApi()
        {
            fillValue = 0.0d;
            nNodes = -1;
            nEdges = -1;
            nFaces = -1;
            nMaxFaceNodes = -1;
        }


        public double zCoordinateFillValue
        {
            get { return fillValue; }
            set { fillValue = value; }
        }

        public void WriteXYCoordinateValues(int meshid, double[] xValues, double[] yValues)
        {
            if (!Initialized) return;
            int numberOfNodes = GetNumberOfNodes(meshid);
            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);

            try
            {
                Marshal.Copy(xValues, 0, xPtr, numberOfNodes);
                Marshal.Copy(yValues, 0, yPtr, numberOfNodes);
                var ierr = wrapper.ionc_put_node_coordinates(ref ioncid, ref meshid, ref xPtr, ref yPtr, ref numberOfNodes);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new Exception("Couldn't save x and y coordinates because of err nr: " + ierr);
                }
            }
            finally
            {
                if (xPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(xPtr);
                xPtr = IntPtr.Zero;
                if (yPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(yPtr);
                yPtr = IntPtr.Zero;
            }
        }

        public void WriteZCoordinateValues(int meshid, double[] zValues)
        {
            if (!Initialized) return;
            int nVal = GetNumberOfNodes(meshid);
            const string varname = "node_z";
            int locationId = (int)GridApiDataSet.Locations.UG_LOC_NODE;

            IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nVal);

            try
            {
                Marshal.Copy(zValues, 0, zPtr, nVal);
                var ierr = wrapper.ionc_put_var(ref ioncid, ref meshid, ref locationId, varname, ref zPtr, ref nVal);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new Exception("Couldn't save z coordinates because of err nr: " + ierr);
                }
            }
            finally
            {
                if (zPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(zPtr);
                zPtr = IntPtr.Zero;
            }
        }

        public string GetMeshName(int mesh)
        {
            if (!Initialized) return string.Empty;
            var name = new StringBuilder(GridApiDataSet.GridConstants.MAXSTRLEN);
            var ierr = wrapper.ionc_get_mesh_name(ref ioncid, ref mesh, name);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get meshname because of err nr: " + ierr);
            return name.ToString();
        }


        #region functions needed for testing

        public int ionc_write_geom_ugrid(string filename)
        {
            return wrapper.ionc_write_geom_ugrid(filename);
        }

        public int ionc_write_map_ugrid(string filename)
        {
            return wrapper.ionc_write_map_ugrid(filename);
        }
        #endregion

        #region Implementation of IUGridApi


        public virtual int GetNumberOfNodes(int meshid)
        {
            if (Initialized && meshid > 0 && nNodes > 0)
            {
                return nNodes;
            }
            int rnNodes = -1;
            try
            {
                var ierr = wrapper.ionc_get_node_count(ref ioncid, ref meshid, ref rnNodes);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                    Log.ErrorFormat("Couldn't get number of nodes because of io netcdf error nr : {0}", ierr);
            }
            catch
            {
                // on exception don't crash...
                Log.ErrorFormat("Couldn't get number of nodes");
            }
            nNodes = rnNodes;
            return nNodes;
        }

        public virtual int GetNumberOfEdges(int meshid)
        {
            if (Initialized && meshid > 0 && nEdges > 0)
            {
                return nEdges;
            }

            int rnEdges = -1;

            try
            {
                var ierr = wrapper.ionc_get_edge_count(ref ioncid, ref meshid, ref rnEdges);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                    Log.ErrorFormat("Couldn't get number of edges because of io netcdf error nr : {0}", ierr);
            }
            catch
            {
                // on exception don't crash...
                Log.ErrorFormat("Couldn't get number of edges");
            }
            nEdges = rnEdges;
            return nEdges;
        }

        public virtual int GetNumberOfFaces(int meshid)
        {
            if (Initialized && meshid > 0 && nFaces > 0)
            {
                return nFaces;
            }

            int rnFaces = -1;

            try
            {
                var ierr = wrapper.ionc_get_face_count(ref ioncid, ref meshid, ref rnFaces);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                    Log.ErrorFormat("Couldn't get number of edges because of io netcdf error nr : {0}", ierr);
            }
            catch
            {
                // on exception don't crash...
                Log.ErrorFormat("Couldn't get number of edges");
            }
            nFaces = rnFaces;
            return nFaces;
        }

        public virtual int GetMaxFaceNodes(int meshid)
        {
            if (Initialized && meshid > 0 && nMaxFaceNodes > 0)
            {
                return nMaxFaceNodes;
            }

            int rnMaxFaceNodes = -1;

            try
            {
                var ierr = wrapper.ionc_get_max_face_nodes(ref ioncid, ref meshid, ref rnMaxFaceNodes);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                    Log.ErrorFormat("Couldn't get max face nodes count because of err nr : {0}", ierr);
            }
            catch
            {
                // on exception don't crash...
                Log.ErrorFormat("Couldn't get max face nodes count");
            }
            nMaxFaceNodes = rnMaxFaceNodes;
            return nMaxFaceNodes;
        }
        
        public double[] GetNodeXCoordinates(int meshId)
        {
            if (!Initialized) return new double[0];
            double[] xCoordinates, yCoordinates;
            var ierr = GetNodeXYCoordinates(meshId, GetNumberOfNodes(meshId), out xCoordinates, out yCoordinates);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get x and y node coordinates because of err nr : " + ierr);
            return xCoordinates;
        }


        public double[] GetNodeYCoordinates(int meshId)
        {
            if (!Initialized) return new double[0];
            double[] xCoordinates, yCoordinates;
            var ierr = GetNodeXYCoordinates(meshId, GetNumberOfNodes(meshId), out xCoordinates, out yCoordinates);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get x and y node coordinates because of err nr : " + ierr);
            return yCoordinates;
        }

        public double[] GetNodeZCoordinates(int meshId)
        {
            int numberOfNodes = GetNumberOfNodes(meshId);
            int locationId = (int)GridApiDataSet.Locations.UG_LOC_NODE;
            string varname = "node_z";
            IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            try
            {

                var ierr = wrapper.ionc_get_var(ref ioncid, ref meshId, ref locationId, varname, ref zPtr, ref numberOfNodes, ref fillValue);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || zPtr == IntPtr.Zero)
                {
                    varname = "NetNode_z";
                    ierr = wrapper.ionc_get_var(ref ioncid, ref meshId, ref locationId, varname, ref zPtr, ref numberOfNodes, ref fillValue);
                    if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || zPtr == IntPtr.Zero)
                    {
                        throw new Exception("Couldn't get z node coordinates because of err nr : " + ierr);
                    }
                }
                var zCoordinates = new double[numberOfNodes];
                Marshal.Copy(zPtr, zCoordinates, 0, numberOfNodes);
                return zCoordinates;
            }
            finally
            {
                if (zPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(zPtr);
                zPtr = IntPtr.Zero;
            }
        }

        public int[,] GetEdgeNodesForMesh(int meshId)
        {
            if (!Initialized) return new int[0, 0];
            var numberOfEdges = GetNumberOfEdges(meshId);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfEdges * GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_A_EDGE);

            try
            {
                var ierr = wrapper.ionc_get_edge_nodes(ref ioncid, ref meshId, ref ptr, ref numberOfEdges);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || ptr == IntPtr.Zero)
                {
                    throw new Exception("Couldn't get edge nodes list");
                }

                // ptr now points to unmanaged 2D array.             
                return MarshalDataTo2DArray(ptr, numberOfEdges, GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_A_EDGE);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptr);
            }
        }
        public int[,] GetFaceNodesForMesh(int meshId)
        {
            int numberOfFaces = GetNumberOfFaces(meshId);
            int numberOfMaxFaceNodes = GetMaxFaceNodes(meshId);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfFaces * numberOfMaxFaceNodes);
            try
            {
                var ierr = wrapper.ionc_get_face_nodes(ref ioncid, ref meshId, ref ptr, ref numberOfFaces, ref numberOfMaxFaceNodes);

                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || ptr == IntPtr.Zero)
                {
                    throw new Exception("Couldn't get face nodes list");
                }

                // ptr now points to unmanaged 2D array.             
                return MarshalDataTo2DArray(ptr, numberOfFaces, numberOfMaxFaceNodes);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptr);
            }
        }
        public int GetVarCount(int meshId, int locationId)
        {
            var nCount = 0;
            if (!Initialized) return nCount;
            var ierr = wrapper.ionc_get_var_count(ref ioncid, ref meshId, ref locationId, ref nCount);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get the nr of number of names at location because of err nr : " + ierr);
            return nCount;
        }

        public int[] GetVarNames(int meshId, int locationId)
        {
            if (!Initialized) return new int[0];
            int nVar = GetVarCount(meshId, locationId);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nVar);
            try
            {
                var ierr = wrapper.ionc_inq_varids(ref ioncid, ref meshId, ref locationId, ref ptr, ref nVar);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || ptr == IntPtr.Zero)
                {
                    throw new Exception("Couldn't get the names at location because of err nr : " + ierr);
                }
                var varIds = new int[nVar];
                Marshal.Copy(ptr, varIds, 0, nVar);
                return varIds;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptr);
                ptr = IntPtr.Zero;
            }
        }

        #endregion

        private int GetNodeXYCoordinates(int meshId, int numberOfNodes, out double[] xCoordinates, out double[] yCoordinates)
        {
            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            xCoordinates = new double[numberOfNodes];
            yCoordinates = new double[numberOfNodes];
            try
            {
                var ierr = wrapper.ionc_get_node_coordinates(ref ioncid, ref meshId, ref xPtr, ref yPtr, ref numberOfNodes);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || xPtr == IntPtr.Zero || yPtr == IntPtr.Zero)
                {
                    return ierr;
                }
                Marshal.Copy(xPtr, xCoordinates, 0, numberOfNodes);
                Marshal.Copy(yPtr, yCoordinates, 0, numberOfNodes);
            }
            finally
            {
                if (xPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(xPtr);
                xPtr = IntPtr.Zero;
                if (yPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(yPtr);
                yPtr = IntPtr.Zero;
            }
            return GridApiDataSet.GridConstants.IONC_NOERR;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}