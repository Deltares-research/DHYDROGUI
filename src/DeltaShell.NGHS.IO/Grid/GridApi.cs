using System;
using System.Runtime.InteropServices;
using System.Text;
using DelftTools.Utils.Interop;
using DelftTools.Utils.NetCdf;
using DeltaShell.Dimr;
using log4net;

namespace DeltaShell.NGHS.IO.Grid
{
    public class GridApi : IGridApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GridApi));
        private int ioncid;
        private double convversion;
        private GridApiDataSet.DataSetConventions iconvtype;
        private double fillValue;
        private bool disposed;

        static GridApi()
        {
            NativeLibrary.LoadNativeDll(GridApiDataSet.GRIDDLL_NAME, DimrApiDataSet.SharedDllPath);
        }

        public GridApi()
        {
            fillValue = 0.0d;
        }

        #region Backwards compatibility

        /// <summary>
        /// Read the convention from the grid nc file via the io_netcdf.dll
        /// </summary>
        /// <param name="file">The grid nc file</param>
        /// <returns>The convention in the grid nc file (or other)</returns>
        public GridApiDataSet.DataSetConventions GetConvention(string file)
        {
            if (file == null) return GridApiDataSet.DataSetConventions.IONC_CONV_OTHER;

            GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite;
            string filename = file;

            try
            {
                Open(filename, mode);
            }
            catch (Exception ex_ionc)
            {
                try
                {
                    filename = file;
                    return GetConventionViaDSFramework(filename);
                }
                catch (Exception ex_dsfw)
                {
                    Log.Warn("Couldn't open nc grid file : " + filename +
                             " to determine what the convention in the nc file was. " + ex_dsfw.Message +
                             ex_ionc.Message);
                }
            }
            try
            {
                Close();
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
            }
            
            if (iconvtype == GridApiDataSet.DataSetConventions.IONC_CONV_NULL)
                return GetConventionViaDSFramework(file);
            if (iconvtype == GridApiDataSet.DataSetConventions.IONC_CONV_UGRID && convversion < 1.0d)
            {
                return GridApiDataSet.DataSetConventions.IONC_CONV_OTHER;
            }
            return iconvtype;
        }

        /// <summary>
        /// Read the convention from the grid nc file via the NetCdf file library in DeltaShell Framework
        /// (Fallback!)
        /// </summary>
        /// <param name="file">The grid nc file</param>
        /// <returns>The convention in the grid nc file (or other)</returns>
        private GridApiDataSet.DataSetConventions GetConventionViaDSFramework(string file)
        {
            try
            {
                var netCdfFile = NetCdfFile.OpenExisting(file);
                var conventions = netCdfFile.GetGlobalAttribute("Conventions");
                return conventions != null &&
                       conventions.Value.ToString().Contains(GridApiDataSet.GridConstants.UG_CONV_UGRID)
                    ? GridApiDataSet.DataSetConventions.IONC_CONV_UGRID
                    : GridApiDataSet.DataSetConventions.IONC_CONV_OTHER;
            }
            catch
            {
                return GridApiDataSet.DataSetConventions.IONC_CONV_OTHER;
            }
        }

        public double zCoordinateFillValue
        {
            get { return fillValue; }
            set { fillValue = value; }
        }

        public void WriteXYCoordinateValues(int meshid, double[] xValues, double[] yValues)
        {
            if(!Initialized()) return;
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
            if(!Initialized()) return;
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
        #endregion
        
                                
           


        
               

        public int ionc_write_geom_ugrid(string filename)
        {
            return GridWrapper.ionc_write_geom_ugrid(filename);
        }
        
        public int ionc_write_map_ugrid(string filename)
        {
            return GridWrapper.ionc_write_map_ugrid(filename);
        }
        
        public int Initialize()
        {
            GridWrapper.IO_NetCDF_Message_Callback message_callback = (int level, string message) =>
            {
                Console.WriteLine("Level: {0}. message = {1}", level, message);
            };
            GridWrapper.IO_NetCDF_Progress_Callback progress_callback = (string message, ref double progress) =>
            {
                Console.WriteLine("Progress: {0:P2}. message = {1}", progress, message);
            };
            var ierr = GridWrapper.ionc_initialize(message_callback, progress_callback);
            return ierr;
        }
        
        public void Dispose()
        {
            if (disposed) return;
            try
            {
                if(Initialized()) Close();
                disposed = true;
            }
            finally
            {
                // Must always ensure this happens to prevent GC deadlock on project close!
                GC.SuppressFinalize(this);
            }
        }

        #region Implementation of IGridApi
        
        public bool adherestoConventions(GridApiDataSet.DataSetConventions convtype)
        {
            if (!Initialized()) return convtype == GridApiDataSet.DataSetConventions.IONC_CONV_NULL;
            var iconvtypeApi = (int) convtype;
            return GridWrapper.ionc_adheresto_conventions(ref ioncid, ref iconvtypeApi);
        }

        public void Open(string c_path, GridApiDataSet.NetcdfOpenMode mode)
        {
            if (c_path == null)
                c_path = string.Empty;
            var imode = (int)mode;
            var iconvtypeApi = 0;
            var ierr = GridWrapper.ionc_open(c_path, ref imode, ref ioncid, ref iconvtypeApi, ref convversion);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't open grid nc file : " + c_path + " because of err nr : " + ierr);

            iconvtype = typeof(GridApiDataSet.DataSetConventions).IsEnumDefined(iconvtypeApi)
                ? (GridApiDataSet.DataSetConventions)iconvtypeApi
                : GridApiDataSet.DataSetConventions.IONC_CONV_OTHER;
        }

        public bool Initialized()
        {
            return ioncid > 0;
        }

        public void Close()
        {
            if (!Initialized()) return;
            var ierr = GridWrapper.ionc_close(ref ioncid);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't close grid nc file because of err nr : " + ierr);
            ioncid = 0;
        }

        public int GetMeshCount()
        {
            var nmesh = 0;
            var ierr = GridWrapper.ionc_get_mesh_count(ref ioncid, ref nmesh);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get number of meshes because of err nr : " + ierr);
            return nmesh;
        }

        public int GetCoordinateSystemCode()
        {
            var epsg_code = 0;
            var ierr = GridWrapper.ionc_get_coordinate_system(ref ioncid, ref epsg_code);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't close grid nc file because of err nr : " + ierr);
            return epsg_code;
        }

        public GridApiDataSet.DataSetConventions GetConvention()
        {
            return !Initialized() ? GridApiDataSet.DataSetConventions.IONC_CONV_NULL : iconvtype;
        }

        public double GetVersion()
        {
            return !Initialized() ? double.NaN : convversion;
        }

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
            if(!Initialized()) return new double[0];
            double[] xCoordinates, yCoordinates;
            var ierr = GetNodeXYCoordinates(meshId, GetNumberOfNodes(meshId), out xCoordinates, out yCoordinates);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get x and y node coordinates because of err nr : " + ierr);
            return xCoordinates;
        }
        

        public double[] GetNodeYCoordinates(int meshId)
        {
            if(!Initialized()) return new double[0];
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
            if (!Initialized()) return new int[0,0];
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
            int nfillValue = 0;
            try
            {
                var ierr = GridWrapper.ionc_get_face_nodes(ref ioncid, ref meshId, ref ptr, ref nFaces, ref nMaxFaceNodes, ref nfillValue);

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
            if (!Initialized()) return nCount;
            var ierr = GridWrapper.ionc_get_var_count(ref ioncid, ref meshId, ref locationId, ref nCount);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get the nr of number of names at location because of err nr : " + ierr);
            return nCount;
        }

        public int[] GetVarNames(int meshId, int locationId)
        {
            if(!Initialized()) return new int[0];
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

        private static int[,] MarshalDataTo2DArray(IntPtr ptr, int numElements, int numValuesPerElement)
        {
            var elements = new int[numElements, numValuesPerElement];
            for (var i = 0; i < numElements; i++)
            {
                // Navigate through ptr to obtain the pointer                  
                // To the first element of every new dimension of the array.                 
                IntPtr pIntArray = (IntPtr)((int)ptr + (Marshal.SizeOf(typeof(int)) * (numValuesPerElement * i)));

                // OneDArrayOfInt will hold values ​​of one dimension of the 2D array,                  
                int[] OneDArrayOfInt = new int[numValuesPerElement];

                // Copy the values ​​of this dimension.                 
                Marshal.Copy(pIntArray, OneDArrayOfInt, 0, numValuesPerElement);
                for (var j = 0; j < numValuesPerElement; j++)
                {
                    elements[i, j] = OneDArrayOfInt[j];
                }
            }
            return elements;
        }
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

        
    }
}