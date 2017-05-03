using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridApi : GridApi, IUGridApi
    {
        private double fillValue;

        public UGridApi()
        {
            fillValue = 0.0d;
        }


        public double zCoordinateFillValue
        {
            get { return fillValue; }
            set { fillValue = value; }
        }

        public void WriteXYCoordinateValues(int meshid, double[] xValues, double[] yValues)
        {
            if (!Initialized) return;
            int nNode = GetNumberOfNodes(meshid);
            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNode);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNode);

            try
            {
                Marshal.Copy(xValues, 0, xPtr, nNode);
                Marshal.Copy(yValues, 0, yPtr, nNode);
                var ierr = GridWrapper.ionc_put_node_coordinates(ref ioncid, ref meshid, ref xPtr, ref yPtr, ref nNode);
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
                var ierr = GridWrapper.ionc_put_var(ref ioncid, ref meshid, ref locationId, varname, ref zPtr, ref nVal);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new Exception("Couldn't save x and y coordinates because of err nr: " + ierr);
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
            var name = new StringBuilder(GridApiDataSet.GridConstants.MAXSTRLEN);
            var ierr = GridWrapper.ionc_get_mesh_name(ref ioncid, ref mesh, name);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get meshname because of err nr: " + ierr);
            return name.ToString();
        }


        public int ionc_write_geom_ugrid(string filename)
        {
            return GridWrapper.ionc_write_geom_ugrid(filename);
        }

        public int ionc_write_map_ugrid(string filename)
        {
            return GridWrapper.ionc_write_map_ugrid(filename);
        }

        #region Implementation of IUGridApi


        public int GetNumberOfNodes(int meshid)
        {
            var nodes = 0;
            var ierr = GridWrapper.ionc_get_node_count(ref ioncid, ref meshid, ref nodes);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get nodes count because of err nr : " + ierr);
            return nodes;
        }

        public int GetNumberOfEdges(int meshid)
        {
            var edges = 0;
            var ierr = GridWrapper.ionc_get_edge_count(ref ioncid, ref meshid, ref edges);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get edges count because of err nr : " + ierr);
            return edges;
        }

        public int GetNumberOfFaces(int meshid)
        {
            var faces = 0;
            var ierr = GridWrapper.ionc_get_face_count(ref ioncid, ref meshid, ref faces);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get faces count because of err nr : " + ierr);
            return faces;
        }

        public int GetMaxFaceNodes(int meshid)
        {
            var maxFaceNodes = 0;
            var ierr = GridWrapper.ionc_get_max_face_nodes(ref ioncid, ref meshid, ref maxFaceNodes);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get max face nodes count because of err nr : " + ierr);
            return maxFaceNodes;
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
            int nNode = GetNumberOfNodes(meshId);
            int locationId = (int)GridApiDataSet.Locations.UG_LOC_NODE;
            string varname = "node_z";
            IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNode);
            try
            {

                var ierr = GridWrapper.ionc_get_var(ref ioncid, ref meshId, ref locationId, varname, ref zPtr, ref nNode, ref fillValue);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || zPtr == IntPtr.Zero)
                {
                    varname = "NetNode_z";
                    ierr = GridWrapper.ionc_get_var(ref ioncid, ref meshId, ref locationId, varname, ref zPtr, ref nNode, ref fillValue);
                    if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || zPtr == IntPtr.Zero)
                    {
                        throw new Exception("Couldn't get z node coordinates because of err nr : " + ierr);
                    }
                }
                var zCoordinates = new double[nNode];
                Marshal.Copy(zPtr, zCoordinates, 0, nNode);
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
            var nEdges = GetNumberOfEdges(meshId);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nEdges * GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_A_EDGE);

            try
            {
                var ierr = GridWrapper.ionc_get_edge_nodes(ref ioncid, ref meshId, ref ptr, ref nEdges);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || ptr == IntPtr.Zero)
                {
                    throw new Exception("Couldn't get edge nodes list");
                }

                // ptr now points to unmanaged 2D array.             
                return MarshalDataTo2DArray(ptr, nEdges, GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_A_EDGE);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptr);
            }
        }
        public int[,] GetFaceNodesForMesh(int meshId)
        {
            int nFaces = GetNumberOfFaces(meshId);
            int nMaxFaceNodes = GetMaxFaceNodes(meshId);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFaces * nMaxFaceNodes);
            try
            {
                var ierr = GridWrapper.ionc_get_face_nodes(ref ioncid, ref meshId, ref ptr, ref nFaces, ref nMaxFaceNodes);

                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || ptr == IntPtr.Zero)
                {
                    throw new Exception("Couldn't get face nodes list");
                }

                // ptr now points to unmanaged 2D array.             
                return MarshalDataTo2DArray(ptr, nFaces, nMaxFaceNodes);
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
            var ierr = GridWrapper.ionc_get_var_count(ref ioncid, ref meshId, ref locationId, ref nCount);
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
                var ierr = GridWrapper.ionc_inq_varids(ref ioncid, ref meshId, ref locationId, ref ptr, ref nVar);
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

        private int GetNodeXYCoordinates(int meshId, int nNode, out double[] xCoordinates, out double[] yCoordinates)
        {
            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNode);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNode);
            xCoordinates = new double[nNode];
            yCoordinates = new double[nNode];
            try
            {
                var ierr = GridWrapper.ionc_get_node_coordinates(ref ioncid, ref meshId, ref xPtr, ref yPtr, ref nNode);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || xPtr == IntPtr.Zero || yPtr == IntPtr.Zero)
                {
                    return ierr;
                }
                Marshal.Copy(xPtr, xCoordinates, 0, nNode);
                Marshal.Copy(yPtr, yCoordinates, 0, nNode);
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