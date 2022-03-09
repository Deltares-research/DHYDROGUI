using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridApi : GridApi, IUGridApi
    {
        private double zCoordinateFillValue;

        public UGridApi()
        {
            zCoordinateFillValue = 0.0d;
        }

        public double ZCoordinateFillValue
        {
            get => zCoordinateFillValue;
            set => zCoordinateFillValue = value;
        }

        public int WriteXYCoordinateValues(int meshId, double[] xValues, double[] yValues)
        {
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            int numberOfNodes;
            int ierr = GetNumberOfNodes(meshId, out numberOfNodes);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            try
            {
                Marshal.Copy(xValues, 0, xPtr, numberOfNodes);
                Marshal.Copy(yValues, 0, yPtr, numberOfNodes);
                ierr = wrapper.PutNodeCoordinates(ioncId, meshId, xPtr, yPtr,
                                                  numberOfNodes);

                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (xPtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(xPtr);
                }

                xPtr = IntPtr.Zero;
                if (yPtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(yPtr);
                }

                yPtr = IntPtr.Zero;
            }
        }

        public int WriteZCoordinateValues(int meshId, GridApiDataSet.LocationType locationType, string varName, string longName, double[] zValues)
        {
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            int nVal = zValues.Length;
            IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nVal);

            try
            {
                var varId = 0;
                wrapper.InqueryVariableIdByStandardName(ioncId, meshId, locationType, GridApiDataSet.UGridApiConstants.Altitude, ref varId);

                // Testing...
                wrapper.InqueryVariableId(ioncId, meshId, varName, ref varId);

                if (varId == -1) // does not exist
                {
                    wrapper.DefineVariable(ioncId, meshId, varId, GridApiDataSet.GridConstants.NF90_DOUBLE, locationType, varName,
                                           GridApiDataSet.UGridApiConstants.Altitude, longName, GridApiDataSet.UGridApiConstants.M, GridApiDataSet.GridConstants.DEFAULT_FILL_VALUE);
                }

                Marshal.Copy(zValues, 0, zPtr, nVal);

                // Eventually the idea is to change PutVariable to use varId rather than varName
                int ierr = wrapper.PutVariable(ioncId, meshId, locationType, varName, zPtr, nVal);
                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            finally
            {
                if (zPtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zPtr);
                }

                zPtr = IntPtr.Zero;
            }
        }

        public int ReadZCoordinateValues(int meshId, GridApiDataSet.LocationType locationType, string varName, out double[] zValues)
        {
            zValues = new double[0];

            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            int ierr = GridApiDataSet.GridConstants.NOERR;
            var nVal = 0;
            try
            {
                var varId = 0;
                wrapper.InqueryVariableIdByStandardName(ioncId, meshId, locationType, GridApiDataSet.UGridApiConstants.Altitude, ref varId);
                if (varId == -1)
                {
                    return GridApiDataSet.GridConstants.NOERR;
                }

                switch (locationType)
                {
                    case GridApiDataSet.LocationType.UG_LOC_NODE:
                        ierr = wrapper.GetNodeCount(ioncId, meshId, ref nVal);
                        break;
                    case GridApiDataSet.LocationType.UG_LOC_FACE:
                        ierr = wrapper.GetFaceCount(ioncId, meshId, ref nVal);
                        break;
                }

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch (Exception)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            if (nVal == 0)
            {
                return GridApiDataSet.GridConstants.NOERR;
            }

            IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nVal);

            try
            {
                ierr = wrapper.GetVariable(ioncId, meshId, (int) locationType, varName, ref zPtr, nVal, ref zCoordinateFillValue);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                zValues = new double[nVal];
                Marshal.Copy(zPtr, zValues, 0, nVal);
            }
            catch (Exception)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (zPtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zPtr);
                }

                zPtr = IntPtr.Zero;
            }

            return ierr;
        }

        public int GetMeshName(int meshId, out string meshName)
        {
            meshName = string.Empty;
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            var meshNameBuilder = new StringBuilder(GridApiDataSet.GridConstants.MAXSTRLEN);
            try
            {
                int ierr = wrapper.GetMeshName(ioncId, meshId, meshNameBuilder);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                meshName = meshNameBuilder.ToString();
                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Close();
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
                int ierr = wrapper.GetNodeCoordinates(ioncId, meshId, ref xPtr, ref yPtr,
                                                      numberOfNodes);
                if (ierr != GridApiDataSet.GridConstants.NOERR || xPtr == IntPtr.Zero || yPtr == IntPtr.Zero)
                {
                    return ierr;
                }

                Marshal.Copy(xPtr, xCoordinates, 0, numberOfNodes);
                Marshal.Copy(yPtr, yCoordinates, 0, numberOfNodes);
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (xPtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(xPtr);
                }

                xPtr = IntPtr.Zero;
                if (yPtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(yPtr);
                }

                yPtr = IntPtr.Zero;
            }

            return GridApiDataSet.GridConstants.NOERR;
        }

        #region functions needed for testing

        public int write_geom_ugrid(string filename)
        {
            return wrapper.WriteGeomUgrid(filename);
        }

        public int write_map_ugrid(string filename)
        {
            return wrapper.WriteMapUgrid(filename);
        }

        #endregion

        #region Implementation of IUGridApi

        public virtual int GetNumberOfNodes(int meshId, out int numberOfNodes)
        {
            int ierr;
            numberOfNodes = -1;
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            try
            {
                ierr = wrapper.GetNodeCount(ioncId, meshId, ref numberOfNodes);
            }
            catch
            {
                ierr = GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            return ierr;
        }

        public virtual int GetNumberOfEdges(int meshId, out int numberOfEdges)
        {
            int ierr;
            numberOfEdges = -1;
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            try
            {
                ierr = wrapper.GetEdgeCount(ioncId, meshId, ref numberOfEdges);
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            return ierr;
        }

        public virtual int GetNumberOfFaces(int meshId, out int numberOfFaces)
        {
            numberOfFaces = -1;
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            try
            {
                return wrapper.GetFaceCount(ioncId, meshId, ref numberOfFaces);
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public virtual int GetMaxFaceNodes(int meshId, out int maxFaceNodes)
        {
            maxFaceNodes = -1;
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            try
            {
                return wrapper.GetMaxFaceNodes(ioncId, meshId, ref maxFaceNodes);
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int GetNodeXCoordinates(int meshId, out double[] xCoordinates)
        {
            xCoordinates = new double[0];

            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            try
            {
                int numberOfNodes;
                double[] yCoordinates;
                int ierr = GetNumberOfNodes(meshId, out numberOfNodes);
                ThrowIfError(ierr);
                return GetNodeXYCoordinates(meshId, numberOfNodes, out xCoordinates, out yCoordinates);
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int GetNodeYCoordinates(int meshId, out double[] yCoordinates)
        {
            yCoordinates = new double[0];
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            try
            {
                int numberOfNodes;
                double[] xCoordinates;
                int ierr = GetNumberOfNodes(meshId, out numberOfNodes);
                ThrowIfError(ierr);
                return GetNodeXYCoordinates(meshId, numberOfNodes, out xCoordinates, out yCoordinates);
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int GetNodeZCoordinates(int meshId, out double[] zCoordinates)
        {
            int numberOfNodes;
            zCoordinates = new double[0];

            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            int ierr = GetNumberOfNodes(meshId, out numberOfNodes);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            var locationId = (int) GridApiDataSet.LocationType.UG_LOC_NODE;
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            try
            {
                ierr = wrapper.GetVariable(ioncId, meshId, locationId, GridApiDataSet.UGridApiConstants.NodeZ, ref ptr,
                                           numberOfNodes, ref zCoordinateFillValue);
                if (ierr != GridApiDataSet.GridConstants.NOERR || ptr == IntPtr.Zero)
                {
                    ierr = wrapper.GetVariable(ioncId, meshId, locationId, GridApiDataSet.UGridApiConstants.NetNodeZ, ref ptr,
                                               numberOfNodes, ref zCoordinateFillValue);
                    if (ierr != GridApiDataSet.GridConstants.NOERR)
                    {
                        return ierr;
                    }

                    if (ptr == IntPtr.Zero)
                    {
                        return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
                    }
                }

                zCoordinates = new double[numberOfNodes];
                Marshal.Copy(ptr, zCoordinates, 0, numberOfNodes);
                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr);
                }

                ptr = IntPtr.Zero;
            }
        }

        public int GetEdgeNodesForMesh(int meshId, out int[,] edgeNodes)
        {
            edgeNodes = new int[0, 0];
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            int numberOfEdges;
            int ierr = GetNumberOfEdges(meshId, out numberOfEdges);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfEdges * GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_AN_EDGE);

            try
            {
                ierr = wrapper.GetEdgeNodes(ioncId, meshId, ref ptr, numberOfEdges);
                if (ptr == IntPtr.Zero)
                {
                    return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
                }

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                // ptr now points to unmanaged 2D array.             
                edgeNodes = MarshalDataTo2DArray(ptr, numberOfEdges, GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_AN_EDGE);
                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr);
                }

                ptr = IntPtr.Zero;
            }
        }

        public int GetFaceNodesForMesh(int meshId, out int[,] faceNodes)
        {
            int numberOfFaces;
            faceNodes = new int[0, 0];

            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            int ierr = GetNumberOfFaces(meshId, out numberOfFaces);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            int numberOfMaxFaceNodes;
            ierr = GetMaxFaceNodes(meshId, out numberOfMaxFaceNodes);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfFaces * numberOfMaxFaceNodes);
            var nfillValue = 0;
            try
            {
                ierr = wrapper.GetFaceNodes(ioncId, meshId, ref ptr, numberOfFaces,
                                            numberOfMaxFaceNodes, ref nfillValue);
                if (ptr == IntPtr.Zero)
                {
                    return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
                }

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                // ptr now points to unmanaged 2D array.
                faceNodes = MarshalDataTo2DArray(ptr, numberOfFaces, numberOfMaxFaceNodes);
                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr);
                }
            }
        }

        public int GetVarCount(int meshId, GridApiDataSet.LocationType locationType, out int nCount)
        {
            nCount = 0;
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            try
            {
                int ierr = wrapper.GetVariablesCount(ioncId, meshId, locationType, ref nCount);
                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int GetVarNames(int meshId, GridApiDataSet.LocationType locationType, out int[] varIds)
        {
            varIds = new int[0];
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            int nVar;
            int ierr = GetVarCount(meshId, locationType, out nVar);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nVar);
            try
            {
                ierr = wrapper.InqueryVariableIds(ioncId, meshId, locationType, ref ptr, nVar);
                if (ptr == IntPtr.Zero)
                {
                    return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
                }

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                varIds = new int[nVar];
                Marshal.Copy(ptr, varIds, 0, nVar);
                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr);
                }

                ptr = IntPtr.Zero;
            }
        }

        #endregion
    }
}