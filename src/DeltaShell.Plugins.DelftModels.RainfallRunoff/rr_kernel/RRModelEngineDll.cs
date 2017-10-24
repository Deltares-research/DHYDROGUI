using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.rr_kernel
{
    public class RRModelEngineDll : IRRModelEngineDll
    {
        private const string RR_FOLDER_NAME = "drr";
        private const string RR_BINFOLDER_NAME = "bin";
        public const string RR_DLL_NAME = "rr_dll.dll";

        public static string DllPath
        {
            get { return Path.Combine(DimrApiDataSet.DllDirectory, "x64", RR_FOLDER_NAME, RR_BINFOLDER_NAME); }
        }

        static RRModelEngineDll()
        {
            DimrApiDataSet.SetSharedPath();
            NativeLibrary.LoadNativeDll(RR_DLL_NAME, DllPath);
        }

        #region PInvoke

        [DllImport(RR_DLL_NAME, EntryPoint = "initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int initialize([In] string file);

        [DllImport(RR_DLL_NAME, EntryPoint = "finalize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int finalize();

        [DllImport(RR_DLL_NAME, EntryPoint = "update", CallingConvention = CallingConvention.Cdecl)]
        public static extern int update([In] double dt);

        [DllImport(RR_DLL_NAME, EntryPoint = "SE_GETVALUESBYINTID", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SE_GetValuesByIntId(
            [In] string componentId,
            [In] string schemId,
            [In] ref int valueId,
            [In] ref int elementsetId,
            [In] ref int ivalues,
            [In, Out] double[] values,
            int a, int b);

        [DllImport(RR_DLL_NAME, EntryPoint = "GETELEMENTINSETCOUNT", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetElementInSetCount(
            [In] string componentId,
            [In] string schemId,
            [In] string elementSetName,
            int a, int b, int c);

        [DllImport(RR_DLL_NAME, EntryPoint = "OES_SETVALUES", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int OES_SetValues(
            [In] string componentID,
            [In] string schemID,
            [In] string quantityID,
            [In] string elementsetID,
            [In] ref int elementCount,
            [In] double[] values,
            int a, int b, int c, int d);

        [DllImport(RR_DLL_NAME, EntryPoint = "GETERROR", CharSet= CharSet.Ansi)]
        public static extern int GetError_(
            [In] ref int errorId,
            [In, Out] StringBuilder errorDescription,
            [In] int errorDescriptionLength);

        [DllImport(RR_DLL_NAME, EntryPoint = "CREATE_OES_WITH_LOGGING", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateOesWithLogging([In] string logfile, int a);

        [DllImport(RR_DLL_NAME, EntryPoint = "MODELFINDORCREATE_OES", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int ModelFindOrCreate_OES(
            [In] string componentId,
            [In] string schemId,
            int a, int b);

        [DllImport(RR_DLL_NAME, EntryPoint = "DEFINEWRAPPERELMSET_OES", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int DefineWrapperElmset_OES(
            [In] string componentId,
            [In] string schemId,
            [In] string quantityID,
            [In] ref int role,
            [In] string elementsetID,
            int a, int b, int c, int d);

        [DllImport(RR_DLL_NAME, EntryPoint = "ANALYZERELATIONS_OES", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int AnalyzeRelations_OES();
        #endregion

        public int ModelInitialize(string componentId, string schemId)
        {
            var retVal = initialize(schemId);

            InitializeOESWrapper(componentId, schemId);

            return retVal;
        }

        private void InitializeOESWrapper(string componentId, string schemId)
        {
            ModelFindOrCreate_OES(componentId, schemId, componentId.Length, schemId.Length);
            var qt = "Boundary levels";
            var es = "RR-Boundaries";
            int role = 2;
            DefineWrapperElmset_OES(componentId, schemId, qt, ref role, es, componentId.Length, schemId.Length, qt.Length,
                                    es.Length);
            AnalyzeRelations_OES();
        }

        public int ModelFinalize(string componentId, string schemId)
        {
            return finalize();
        }

        public int ModelPerformTimeStep(string componentId, string schemId)
        {
            return update(-1);
        }

        public int GetValuesByIntId(string componentId, string schemId, ref int valueId, ref int elementsetId, ref int ivalues, [Out] double[] values)
        {
            return SE_GetValuesByIntId(componentId, schemId, ref valueId, ref elementsetId,
                                                            ref ivalues, values, componentId.Length, schemId.Length);
        }

        public int GetSize(string componentId, string schemId, string elementSetName)
        {
            return GetElementInSetCount(componentId, schemId, elementSetName, componentId.Length, schemId.Length,
                                        elementSetName.Length);
        }

        public int SetValues(string componentID, string schemID, string quantityID, string elementsetID, int elementCount, double[] values)
        {
            return OES_SetValues(componentID, schemID, quantityID, elementsetID, ref elementCount, values,
                                 componentID.Length, schemID.Length, quantityID.Length, elementsetID.Length);
        }

        public int GetError(ref int errorId, StringBuilder errorDescription)
        {
            return GetError_(ref errorId, errorDescription, errorDescription.Length);
        }
    }
}