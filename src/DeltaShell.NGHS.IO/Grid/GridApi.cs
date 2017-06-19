using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DelftTools.Utils.Interop;
using DelftTools.Utils.NetCdf;
using DeltaShell.Dimr;
using log4net;
using ProtoBufRemote;

namespace DeltaShell.NGHS.IO.Grid
{
    public abstract class GridApi : IGridApi
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(GridApi));
        protected int ioncid;
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
        /// <returns>The convention in the grid nc file (or other)</returns>
        public GridApiDataSet.DataSetConventions GetConvention(string file)
        {
            if (string.IsNullOrEmpty(file)) return GridApiDataSet.DataSetConventions.IONC_CONV_OTHER;

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
                             " to determine what the convention in the nc file was. " + ex_dsfw.Message + " " +
                             ex_ionc.Message);
                    return GridApiDataSet.DataSetConventions.IONC_CONV_OTHER;
                }
            }
            try
            {
                Close();
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
                return iconvtype;
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
        public virtual GridApiDataSet.DataSetConventions GetConventionViaDSFramework(string file)
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

        #endregion


        #region Implementation of IGridApi

        public bool adherestoConventions(GridApiDataSet.DataSetConventions convtype)
        {
            if (!Initialized) return convtype == GridApiDataSet.DataSetConventions.IONC_CONV_NULL;
            var iconvtypeApi = (int)convtype;
            return wrapper.ionc_adheresto_conventions(ref ioncid, ref iconvtypeApi);
        }

        public virtual void Open(string c_path, GridApiDataSet.NetcdfOpenMode mode)
        {
            if (Initialized)
                Close();
            if (c_path == null)
                c_path = string.Empty;
            var imode = (int)mode;
            var iconvtypeApi = 0;
            var ierr = wrapper.ionc_open(c_path, ref imode, ref ioncid, ref iconvtypeApi, ref convversion);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't open grid nc file : " + c_path + " because of err nr : " + ierr);

            iconvtype = typeof(GridApiDataSet.DataSetConventions).IsEnumDefined(iconvtypeApi)
                ? (GridApiDataSet.DataSetConventions)iconvtypeApi
                : GridApiDataSet.DataSetConventions.IONC_CONV_OTHER;
            iconvtype = iconvtype == GridApiDataSet.DataSetConventions.IONC_CONV_UGRID && convversion < 1.0d
                ? GridApiDataSet.DataSetConventions.IONC_CONV_OTHER
                : iconvtype;
        }

        public virtual bool Initialized
        {
            get { return ioncid > 0; }
        }

        public virtual void Close()
        {
            if (!Initialized) return;
            var ierr = wrapper.ionc_close(ref ioncid);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't close grid nc file because of err nr : " + ierr);
            ioncid = 0;
        }

        public int GetMeshCount()
        {
            if (!Initialized) return 0;
            var nmesh = 0;
            var ierr = wrapper.ionc_get_mesh_count(ref ioncid, ref nmesh);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get number of meshes because of err nr : " + ierr); // TODO: Kunnen excepties hier wel gegooid worden?
            return nmesh;
        }

        public int GetNumberOfNetworks(out int numberOfNetworks)
        {
            numberOfNetworks = 0;

            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            try
            {
                int nNetworks = 0;
                var ierr = wrapper.ionc_get_number_of_networks(ref ioncid, ref nNetworks);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }
                numberOfNetworks = nNetworks;
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
        }

        public Dictionary<UGridMeshType, int> GetNumberOfMeshes()
        {
            Dictionary<UGridMeshType, int> meshTypeCountDict = new Dictionary<UGridMeshType, int>();

            var meshTypes = Enum.GetValues(typeof(UGridMeshType));

            foreach (UGridMeshType meshType in meshTypes)
            {
                int nMesh;
                var ierr = GetNumberOfMeshByType(meshType, out nMesh);

                nMesh = ierr == GridApiDataSet.GridConstants.IONC_NOERR ? nMesh : 0;

                meshTypeCountDict.Add(meshType, nMesh);
            }

            return meshTypeCountDict;
        }

        private int GetNumberOfMeshByType(UGridMeshType meshType, out int numberOfMesh)
        {
            numberOfMesh = 0;

            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            try
            {
                var type = (int)meshType;
                int nMesh = 0;
                var ierr = wrapper.ionc_get_number_of_meshes(ref ioncid, ref type, ref nMesh);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }
                numberOfMesh = nMesh;
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
        }

        public int GetNetworkIds(out int[] networkIds)
        {
            networkIds = new int[0];
            int numberOfNetworks;

            var ierr = GetNumberOfNetworks(out numberOfNetworks);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR) return ierr;

            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            IntPtr networkIdsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfNetworks);
            try
            {
                ierr = wrapper.ionc_get_network_ids(ref ioncid, ref networkIdsPtr, ref numberOfNetworks);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }

                networkIds = new int[numberOfNetworks];
                Marshal.Copy(networkIdsPtr, networkIds, 0, numberOfNetworks);
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            finally
            {
                if (networkIdsPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(networkIdsPtr);
                networkIdsPtr = IntPtr.Zero;
            }
        }

        public Dictionary<UGridMeshType, int[]> GetMeshIds()
        {
            Dictionary<UGridMeshType, int[]> meshTypeIdsDict = new Dictionary<UGridMeshType, int[]>();

            int[] meshIds = new int[0];
            var meshTypes = Enum.GetValues(typeof(UGridMeshType));

            foreach (UGridMeshType meshType in meshTypes)
            {
                int nMesh;
                var ierr = GetNumberOfMeshByType(meshType, out nMesh);

                if (ierr == GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    ierr = GetMeshIdsByType(meshType, nMesh, out meshIds);
                }
                
                meshIds = ierr == GridApiDataSet.GridConstants.IONC_NOERR ? meshIds : new int[0];

                meshTypeIdsDict.Add(meshType, meshIds);
            }
            return meshTypeIdsDict;
        }

        public int GetMeshIdsByType(UGridMeshType meshType, int numberOfMeshes, out int[] meshIds)
        {
            meshIds = new int[0];
            IntPtr meshIdsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfMeshes);
            try
            {
                var type = (int) meshType;
                var ierr = wrapper.ionc_get_mesh_ids(ref ioncid, ref type, ref meshIdsPtr, ref numberOfMeshes);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }

                meshIds = new int[numberOfMeshes];
                Marshal.Copy(meshIdsPtr, meshIds, 0, numberOfMeshes);
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            finally
            {
                if (meshIdsPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(meshIdsPtr);
                meshIdsPtr = IntPtr.Zero;
            }
        }


        public int GetCoordinateSystemCode()
        {
            if (!Initialized) return 0;
            var epsg_code = 0;
            var ierr = wrapper.ionc_get_coordinate_system(ref ioncid, ref epsg_code);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                throw new Exception("Couldn't get coordinate system code because of err nr : " + ierr);
            return epsg_code;
        }

        public GridApiDataSet.DataSetConventions GetConvention()
        {
            return !Initialized ? GridApiDataSet.DataSetConventions.IONC_CONV_NULL : iconvtype;
        }

        public double GetVersion()
        {
            return !Initialized ? double.NaN : convversion;
        }

        public void CreateFile(string filePath, UGridGlobalMetaData globalMetaData, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_write)
        {
            var imode = (int)mode;
            var ierr = wrapper.ionc_create(filePath, ref imode, ref ioncid);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(
                    string.Format("Couldn't create new NetCDF file at location {0} because of error number {1}", filePath, ierr));
            }

            CreateAndWriteDefaultNetCdfMetaData(filePath, globalMetaData);
        }

        private void CreateAndWriteDefaultNetCdfMetaData(string filePath, UGridGlobalMetaData globalMetaData)
        {
            GridWrapper.interop_metadata metadata;
            metadata.institution = ToDataSizeCharArray("Deltares");
            metadata.source = ToDataSizeCharArray(globalMetaData.Source);
            metadata.references = ToDataSizeCharArray("https://github.com/ugrid-conventions/ugrid-conventions");
            metadata.version = ToDataSizeCharArray(globalMetaData.Version);
            metadata.modelname = ToDataSizeCharArray(globalMetaData.Modelname);

            var ierr = wrapper.ionc_add_global_attributes(ref ioncid, metadata);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(
                    string.Format("Couldn't write global metadata to NetCDF file at location {0} because of error number {1}"
                    , filePath, ierr));
            }
        }

        private static char[] ToDataSizeCharArray(string value)
        {
            return value.PadRight(GridWrapper.metadatasize, ' ').ToCharArray(0, GridWrapper.metadatasize);
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
            var ierr = wrapper.ionc_initialize(message_callback, progress_callback);
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



    }
}