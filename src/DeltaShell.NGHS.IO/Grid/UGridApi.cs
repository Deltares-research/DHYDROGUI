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

        public int WriteXYCoordinateValues(int meshid, double[] xValues, double[] yValues)
        {
            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            int numberOfNodes;
            if(GetNumberOfNodes(meshid, out numberOfNodes) != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);

            try
            {
                Marshal.Copy(xValues, 0, xPtr, numberOfNodes);
                Marshal.Copy(yValues, 0, yPtr, numberOfNodes);
                var ierr = wrapper.ionc_put_node_coordinates(ref ioncid, ref meshid, ref xPtr, ref yPtr,
                    ref numberOfNodes);

                return ierr;

            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
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
        
        public int WriteZCoordinateValues(int meshId, int locationId, string varName, string longName, double[] zValues)
        {
            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            var nVal = zValues.Length;
            IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nVal);

            try
            {
                const string StandardName = "altitude";
                int varId = 0;

                wrapper.ionc_inq_varid_by_standard_name(ref ioncid, ref meshId, ref locationId, StandardName,
                    ref varId);

                // Testing...
                wrapper.ionc_inq_varid(ref ioncid, ref meshId, varName, ref varId);

                if (varId == -1) // does not exist
                {
                    const string Unit = "m";
                    int NF90_DOUBLE = 6;
                    double fillValue = -999.9;

                    wrapper.ionc_def_var(ref ioncid, ref meshId, ref varId, ref NF90_DOUBLE, ref locationId, varName,
                        StandardName, longName, Unit, ref fillValue);
                }

                Marshal.Copy(zValues, 0, zPtr, nVal);

                // Eventually then idea is to change put_var to use varId rather than varName
                var ierr = wrapper.ionc_put_var(ref ioncid, ref meshId, ref locationId, varName, ref zPtr, ref nVal);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            finally
            {
                if (zPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(zPtr);
                zPtr = IntPtr.Zero;
            }
        }

        public int GetMeshName(int mesh, out string name)
        {
            name = string.Empty;
            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            var meshName = new StringBuilder(GridApiDataSet.GridConstants.MAXSTRLEN);
            try
            {
                var ierr = wrapper.ionc_get_mesh_name(ref ioncid, ref mesh, meshName);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR) return ierr;

                name = meshName.ToString();
                return ierr;
            }
            catch 
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
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


        public virtual int GetNumberOfNodes(int meshid, out int numberOfNodes)
        {
            numberOfNodes = -1;
            if (Initialized && meshid > 0 && nNodes > 0)
            {
                numberOfNodes = nNodes;
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            int rnNodes = -1;
            try
            {
                var ierr = wrapper.ionc_get_node_count(ref ioncid, ref meshid, ref rnNodes);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }

                numberOfNodes = rnNodes;
                nNodes = rnNodes;
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
        }

        private bool TryGetNumberOfNodes(int identifier, out int numberOfNodes)
        {
            var ierr = GetNumberOfNodes(identifier, out numberOfNodes);
            return ierr == GridApiDataSet.GridConstants.IONC_NOERR;
        }

        public virtual int GetNumberOfEdges(int meshid, out int numberOfMeshEdges)
        {
            numberOfMeshEdges = -1;
            if (Initialized && meshid > 0 && nEdges > 0)
            {
                numberOfMeshEdges = nEdges;
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }

            int rnEdges = -1;

            try
            {
                var ierr = wrapper.ionc_get_edge_count(ref ioncid, ref meshid, ref rnEdges);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }

                numberOfMeshEdges = rnEdges;
                nEdges = numberOfMeshEdges;
                return ierr;
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
        }

        public virtual int GetNumberOfFaces(int meshid, out int numberOfFaces)
        {
            numberOfFaces = -1;
            if (Initialized && meshid > 0 && nFaces > 0)
            {
                numberOfFaces = nFaces;
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }

            int rnFaces = -1;

            try
            {
                var ierr = wrapper.ionc_get_face_count(ref ioncid, ref meshid, ref rnFaces);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }

                numberOfFaces = rnFaces;
                nFaces = rnFaces;
                return GridApiDataSet.GridConstants.IONC_NOERR;

            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
        }

        public virtual int GetMaxFaceNodes(int meshid, out int maxFaceNodes)
        {
            maxFaceNodes = -1;
            if (Initialized && meshid > 0 && nMaxFaceNodes > 0)
            {
                maxFaceNodes = nMaxFaceNodes;
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }

            int rnMaxFaceNodes = -1;

            try
            {
                var ierr = wrapper.ionc_get_max_face_nodes(ref ioncid, ref meshid, ref rnMaxFaceNodes);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }

                maxFaceNodes = rnMaxFaceNodes;
                nMaxFaceNodes = rnMaxFaceNodes;
                return ierr;

            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
        }
        
        public int GetNodeXCoordinates(int meshId, out double[] xCoordinates)
        {
            xCoordinates = new double[0];

            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            double[] yCoordinates;
            int numberOfNodes;
            if (!TryGetNumberOfNodes(meshId, out numberOfNodes))
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            return GetNodeXYCoordinates(meshId, numberOfNodes, out xCoordinates, out yCoordinates);
        }


        public int GetNodeYCoordinates(int meshId, out double[] yCoordinates)
        {
            yCoordinates = new double[0];
            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            double[] xCoordinates;

            int numberOfNodes;
            if (!TryGetNumberOfNodes(meshId, out numberOfNodes))
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            return GetNodeXYCoordinates(meshId, numberOfNodes, out xCoordinates, out yCoordinates);
        }
        
        public int GetNodeZCoordinates(int meshId, out double[] zCoordinates)
        {
            zCoordinates = new double[0];
            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            int numberOfNodes;
            if (!TryGetNumberOfNodes(meshId, out numberOfNodes))
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
           
            int locationId = (int)GridApiDataSet.Locations.UG_LOC_NODE;
            string varname = "node_z";
            IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            try
            {
                var ierr = wrapper.ionc_get_var(ref ioncid, ref meshId, ref locationId, varname, ref zPtr,
                    ref numberOfNodes, ref fillValue);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || zPtr == IntPtr.Zero)
                {
                    varname = "NetNode_z";
                    ierr = wrapper.ionc_get_var(ref ioncid, ref meshId, ref locationId, varname, ref zPtr,
                        ref numberOfNodes, ref fillValue);
                    if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || zPtr == IntPtr.Zero)
                    {
                        return ierr != GridApiDataSet.GridConstants.IONC_NOERR ? ierr : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
                    }
                }
                zCoordinates = new double[numberOfNodes];
                Marshal.Copy(zPtr, zCoordinates, 0, numberOfNodes);
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            finally
            {
                if (zPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(zPtr);
                zPtr = IntPtr.Zero;
            }
        }

        public int GetEdgeNodesForMesh(int meshId, out int[,] edgeNodes)
        {
            edgeNodes = new int[0, 0];
            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            int numberOfEdges;
            var ierr = GetNumberOfEdges(meshId, out numberOfEdges);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfEdges * GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_A_EDGE);

            try
            {
                ierr = wrapper.ionc_get_edge_nodes(ref ioncid, ref meshId, ref ptr, ref numberOfEdges);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || ptr == IntPtr.Zero)
                {
                    return ierr != GridApiDataSet.GridConstants.IONC_NOERR
                        ? ierr
                        : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
                }

                // ptr now points to unmanaged 2D array.             
                edgeNodes = MarshalDataTo2DArray(ptr, numberOfEdges,
                    GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_A_EDGE);
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptr);
                ptr = IntPtr.Zero;
            }
        }
        public int GetFaceNodesForMesh(int meshId, out int[,] faceNodes)
        {
            int numberOfFaces;
            faceNodes = new int[0,0];
            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            var ierr = GetNumberOfFaces(meshId, out numberOfFaces);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            int numberOfMaxFaceNodes;
            ierr = GetMaxFaceNodes(meshId, out numberOfMaxFaceNodes);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfFaces * numberOfMaxFaceNodes);
            int nfillValue = 0;
            try
            {
                ierr = wrapper.ionc_get_face_nodes(ref ioncid, ref meshId, ref ptr, ref numberOfFaces,
                    ref numberOfMaxFaceNodes, ref nfillValue);

                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || ptr == IntPtr.Zero)
                {
                    return ierr != GridApiDataSet.GridConstants.IONC_NOERR ? ierr : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
                }

                // ptr now points to unmanaged 2D array.     
                faceNodes = MarshalDataTo2DArray(ptr, numberOfFaces, numberOfMaxFaceNodes);
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptr);
            }
        }

        public int GetVarCount(int meshId, int locationId, out int nCount)
        {
            nCount = 0;
            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            try
            {
                var ierr = wrapper.ionc_get_var_count(ref ioncid, ref meshId, ref locationId, ref nCount);

                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
        }

        public int GetVarNames(int meshId, int locationId, out int[] varIds)
        {
            varIds = new int[0];
            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            int nVar;
            var ierr = GetVarCount(meshId, locationId, out nVar);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nVar);
            try
            {
                ierr = wrapper.ionc_inq_varids(ref ioncid, ref meshId, ref locationId, ref ptr, ref nVar);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || ptr == IntPtr.Zero)
                {
                    return ierr != GridApiDataSet.GridConstants.IONC_NOERR ? ierr : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
                }
                varIds = new int[nVar];
                Marshal.Copy(ptr, varIds, 0, nVar);
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
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
                var ierr = wrapper.ionc_get_node_coordinates(ref ioncid, ref meshId, ref xPtr, ref yPtr,
                    ref numberOfNodes);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR || xPtr == IntPtr.Zero || yPtr == IntPtr.Zero)
                {
                    return ierr;
                }
                Marshal.Copy(xPtr, xCoordinates, 0, numberOfNodes);
                Marshal.Copy(yPtr, yCoordinates, 0, numberOfNodes);
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
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