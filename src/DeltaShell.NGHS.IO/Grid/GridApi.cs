using System;
using System.Runtime.InteropServices;
using DelftTools.Utils.Interop;
using DelftTools.Utils.NetCdf;
using log4net;

namespace DeltaShell.NGHS.IO.Grid
{
    public abstract class GridApi : IGridApi
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(GridApi));
        protected int ioncid;
        protected double convversion;
        protected GridApiDataSet.DataSetConventions iconvtype;
        
        static GridApi()
        {
            NativeLibrary.LoadNativeDllForCurrentPlatform(GridApiDataSet.GRIDDLL_NAME, GridApiDataSet.DllDirectory);
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
                             " to determine what the convention in the nc file was. " + ex_dsfw.Message +
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
            return GridWrapper.ionc_adheresto_conventions(ref ioncid, ref iconvtypeApi);
        }

        public virtual void Open(string c_path, GridApiDataSet.NetcdfOpenMode mode)
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

        public bool Initialized
        {
            get { return ioncid > 0; }
        }

        public void Close()
        {
            if (!Initialized) return;
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
            return !Initialized ? GridApiDataSet.DataSetConventions.IONC_CONV_NULL : iconvtype;
        }

        public double GetVersion()
        {
            return !Initialized ? double.NaN : convversion;
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