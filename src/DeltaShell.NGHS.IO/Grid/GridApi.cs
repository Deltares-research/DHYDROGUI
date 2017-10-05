using System;
using System.Runtime.InteropServices;
using DelftTools.Utils.Interop;
using DelftTools.Utils.NetCdf;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.Properties;
using ProtoBufRemote;

namespace DeltaShell.NGHS.IO.Grid
{
    public abstract class GridApi : IGridApi
    {
        protected int ioncId;
        protected double convversion;
        protected GridApiDataSet.DataSetConventions iconvtype;
        protected IGridWrapper wrapper;

        static GridApi()
        {
            RemotingTypeConverters.RegisterTypeConverter(new UgridGlobalMetaDataToProtoConverter());
            NativeLibrary.LoadNativeDll(GridApiDataSet.GRIDDLL_NAME, DimrApiDataSet.SharedDllPath);
        }

        public GridApi()
        {
            wrapper = new GridWrapper();
        }

        #region Backwards compatibility

        /// <summary>
        /// Read the convention from the grid nc file via the io_netcdf.dll
        /// </summary>
        /// <param name="file">The grid nc file</param>
        /// <param name="convention">The convention in the grid nc file (or other) (out)</param>
        /// <returns>Error code</returns>
        public int GetConvention(string file, out GridApiDataSet.DataSetConventions convention)
        {
            if (string.IsNullOrEmpty(file))
            {
                convention = GridApiDataSet.DataSetConventions.CONV_OTHER;
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite;
            string filename = file;

            var ierr = Open(filename, mode);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                try
                {
                    filename = file;
                    convention = GetConventionViaDSFramework(filename);
                    return GridApiDataSet.GridConstants.NOERR;
                }
                catch
                {
                    convention = GridApiDataSet.DataSetConventions.CONV_OTHER;
                    return GridApiDataSet.GridConstants.NOERR;
                }
            }

            var ierrClose = Close();
            if (ierrClose != GridApiDataSet.GridConstants.NOERR)
            {
                convention = iconvtype;
                return GridApiDataSet.GridConstants.NOERR;
            }
            
            if (iconvtype == GridApiDataSet.DataSetConventions.CONV_NULL)
            {
                convention = GetConventionViaDSFramework(file);
                return GridApiDataSet.GridConstants.NOERR;
            }
            if (iconvtype == GridApiDataSet.DataSetConventions.CONV_UGRID && convversion < 1.0d)
            {
                convention = GridApiDataSet.DataSetConventions.CONV_OTHER;
                return GridApiDataSet.GridConstants.NOERR;
            }
            convention = iconvtype;
            return GridApiDataSet.GridConstants.NOERR;
        }

        /// <summary>
        /// Read the convention from the grid nc file via the NetCdf file library in DeltaShell Framework
        /// (Fallback!)
        /// </summary>
        /// <param name="file">The grid nc file</param>
        /// <returns>The convention in the grid nc file (or other)</returns>
        public virtual GridApiDataSet.DataSetConventions GetConventionViaDSFramework(string file)
        {
            try
            {
                var netCdfFile = NetCdfFile.OpenExisting(file);
                var conventions = netCdfFile.GetGlobalAttribute("Conventions");
                return conventions != null &&
                       conventions.Value.ToString().Contains(GridApiDataSet.GridConstants.UG_CONV_UGRID)
                    ? GridApiDataSet.DataSetConventions.CONV_UGRID
                    : GridApiDataSet.DataSetConventions.CONV_OTHER;
            }
            catch
            {
                return GridApiDataSet.DataSetConventions.CONV_OTHER;
            }
        }

        #endregion


        #region Implementation of IGridApi

        public bool adherestoConventions(GridApiDataSet.DataSetConventions convtype)
        {
            if (!Initialized) return convtype == GridApiDataSet.DataSetConventions.CONV_NULL;
            var iconvtypeApi = (int)convtype;
            return wrapper.AdherestoConventions(ioncId, iconvtypeApi);
        }

        public virtual int Open(string filePath, GridApiDataSet.NetcdfOpenMode mode)
        {
            if (Initialized)
                Close();
            if (filePath == null)
                filePath = string.Empty;
            var imode = (int)mode;
            var iconvtypeApi = 0;
            var ierr = wrapper.Open(filePath, imode, ref ioncId, ref iconvtypeApi, ref convversion);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            iconvtype = typeof(GridApiDataSet.DataSetConventions).IsEnumDefined(iconvtypeApi)
                ? (GridApiDataSet.DataSetConventions)iconvtypeApi
                : GridApiDataSet.DataSetConventions.CONV_OTHER;
            iconvtype = iconvtype == GridApiDataSet.DataSetConventions.CONV_UGRID && convversion < 1.0d
                ? GridApiDataSet.DataSetConventions.CONV_OTHER
                : iconvtype;

            return GridApiDataSet.GridConstants.NOERR;
        }

        public virtual bool Initialized
        {
            get { return ioncId > 0; }
        }

        public virtual int Close()
        {
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            try
            {
                var ierr = wrapper.Close(ioncId);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
                ioncId = 0;
                return GridApiDataSet.GridConstants.NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int GetMeshCount(out int numberOfMeshes)
        {
            numberOfMeshes = 0;
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            try
            {
                var ierr = wrapper.GetMeshCount(ioncId, ref numberOfMeshes);
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

        public int GetNumberOfNetworks(out int numberOfNetworks)
        {
            numberOfNetworks = 0;

            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            try
            {
                int nNetworks = 0;
                var ierr = wrapper.GetNumberOfNetworks(ioncId, ref nNetworks);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
                numberOfNetworks = nNetworks;
                return GridApiDataSet.GridConstants.NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int GetNumberOfMeshByType(UGridMeshType meshType, out int numberOfMesh)
        {
            numberOfMesh = 0;

            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            int ierr;
            try
            {
                var type = (int)meshType;
                int nMesh = 0;
                ierr = wrapper.GetNumberOfMeshes(ioncId, type, ref nMesh);
                if(ierr == GridApiDataSet.GridConstants.NOERR) numberOfMesh = nMesh;
            }
            catch
            {
                ierr = GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            return ierr;
        }

        public int GetNetworkIds(out int[] networkIds)
        {
            networkIds = new int[0];
            int numberOfNetworks = 0;
            IntPtr networkIdsPtr = IntPtr.Zero;
            int ierr;

            try
            {
                ierr = GetNumberOfNetworks(out numberOfNetworks);
            }
            catch
            {
                ierr = GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            if (ierr != GridApiDataSet.GridConstants.NOERR) return ierr;
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            if (numberOfNetworks != 0) networkIdsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfNetworks);

            try
            {
                ierr = wrapper.GetNetworkIds(ioncId, ref networkIdsPtr, numberOfNetworks);
                if (ierr == GridApiDataSet.GridConstants.NOERR)
                {
                    networkIds = new int[numberOfNetworks];
                    Marshal.Copy(networkIdsPtr, networkIds, 0, numberOfNetworks);
                    return GridApiDataSet.GridConstants.NOERR;
                }
            }
            catch
            {
                ierr = GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (networkIdsPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(networkIdsPtr);
                networkIdsPtr = IntPtr.Zero;
            }
            return ierr;
        }

        public int GetMeshIdsByMeshType(UGridMeshType meshType, int numberOfMeshes, out int[] meshIds)
        {
            meshIds = new int[0];
            IntPtr meshIdsPtr = IntPtr.Zero;
            int ierr;
            try
            {
                meshIdsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfMeshes);
                ierr = wrapper.GetMeshIds(ioncId, meshType, ref meshIdsPtr, numberOfMeshes);
                if (ierr == GridApiDataSet.GridConstants.NOERR)
                {
                    meshIds = new int[numberOfMeshes];
                    Marshal.Copy(meshIdsPtr, meshIds, 0, numberOfMeshes);
                }
            }
            catch
            {
                ierr = GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (meshIdsPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(meshIdsPtr);
                meshIdsPtr = IntPtr.Zero;
            }
            return ierr;
        }
        
        public int GetCoordinateSystemCode(out int epsg_code)
        {
            epsg_code = 0;
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            try
            {
                var ierr = wrapper.GetCoordinateSystem(ioncId, ref epsg_code);
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

        public GridApiDataSet.DataSetConventions GetConvention()
        {
            return !Initialized ? GridApiDataSet.DataSetConventions.CONV_NULL : iconvtype;
        }

        public double GetVersion()
        {
            return !Initialized ? double.NaN : convversion;
        }

        public int CreateFile(string filePath, UGridGlobalMetaData globalMetaData, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_write)
        {
            var imode = (int)mode;
            int netcdfId = 0;
            var ierr = wrapper.create(filePath, imode, ref netcdfId);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            ierr = CreateAndWriteDefaultNetCdfMetaData(globalMetaData, netcdfId);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            // close the file after creation
            ierr = wrapper.Close(netcdfId);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }
            return GridApiDataSet.GridConstants.NOERR;
        }

        private int CreateAndWriteDefaultNetCdfMetaData(UGridGlobalMetaData globalMetaData, int netcdfId)
        {
            GridWrapper.interop_metadata metadata;
            metadata.institution = ToDataSizeCharArray("Deltares");
            metadata.source = ToDataSizeCharArray(globalMetaData.Source);
            metadata.references = ToDataSizeCharArray("https://github.com/ugrid-conventions/ugrid-conventions");
            metadata.version = ToDataSizeCharArray(globalMetaData.Version);
            metadata.modelname = ToDataSizeCharArray(globalMetaData.Modelname);

            var ierr = wrapper.AddGlobalAttributes(netcdfId, metadata);
            return ierr;
        }

        private static char[] ToDataSizeCharArray(string value)
        {
            return value.PadRight(GridWrapper.metadatasize, ' ').ToCharArray(0, GridWrapper.metadatasize);
        }

        public int Initialize()
        {
            GridWrapper.IO_NetCDF_Message_Callback message_callback = (int level, string message) =>
            {
                Console.WriteLine(Resources.GridApi_Initialize_Level_0__Message_1_, level, message);
            };
            GridWrapper.IO_NetCDF_Progress_Callback progress_callback = (string message, ref double progress) =>
            {
                Console.WriteLine(Resources.GridApi_Initialize_Progress_0_Message_1_, progress, message);
            };
            var ierr = wrapper.initialize(message_callback, progress_callback);
            return ierr;
        }
        #endregion

        protected static int[,] MarshalDataTo2DArray(IntPtr ptr, int numElements, int numValuesPerElement)
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

        protected void ThrowIfError(int ierr)
        {
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                throw new Exception();
            }
        }

        public int GetMeshGeom(ref int ioncid, ref int meshId, ref GridWrapper.meshgeom mesh, int nodes2D, bool includeArrays, ref double[] rc_twodnodex, ref double[] rc_twodnodey, ref double[] rc_twodnodez)
        {
            mesh.nodex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nodes2D);
            mesh.nodey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nodes2D);
            mesh.nodez = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nodes2D);
            mesh.edge_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nodes2D * 2);

            try
            {
                var ierr = wrapper.get_meshgeom(ref ioncid, ref meshId, ref mesh, includeArrays);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            Marshal.Copy(mesh.nodex, rc_twodnodex, 0, nodes2D);
            Marshal.Copy(mesh.nodey, rc_twodnodey, 0, nodes2D);
            Marshal.Copy(mesh.nodez, rc_twodnodez, 0, nodes2D);

            //Free memory
//            Marshal.FreeCoTaskMem(mesh.nodex);
//            Marshal.FreeCoTaskMem(mesh.nodey);
//            Marshal.FreeCoTaskMem(mesh.nodez);
//            Marshal.FreeCoTaskMem(mesh.edge_nodes);
//
//            mesh.nodex = IntPtr.Zero;
//            mesh.nodey = IntPtr.Zero;
//            mesh.nodez = IntPtr.Zero;
//            mesh.edge_nodes = IntPtr.Zero;

            return GridApiDataSet.GridConstants.NOERR;
        }

        public int GetMeshGeomDim(ref int ioncid, ref int meshId, ref GridWrapper.meshgeomdim meshgeomdim)
        {
            try
            {
                var ierr = wrapper.get_meshgeom_dim(ref ioncid, ref meshId, ref meshgeomdim);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            return GridApiDataSet.GridConstants.NOERR;
        }
    }
}