using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DelftTools.Utils.Interop;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public enum LocationTypes
    {
        LocTypeAllSegments = -1,
        LocTypeSegment = 1,
        LocTypeBoundary = 2,
        LocTypeDischarge = 3
    }

    public enum OperationType
    {
        OperSet = 1,         // Simply replace the original value by the new value
        OperAdd = 2,         // Add the given value to the original value
        OperMultiply = 3,    // Multiply the original value by the given value
    }

    public enum DelwaqItem
    {
        DLWQ_CONSTANT          = 1,  // Constant in the processes library (one value)
        DLWQ_PARAMETER         = 2,  // Process parameter varying per cell (noseg values)
        DLWQ_CONCENTRATION     = 3,  // Current concentration of a substance (noseg values)
        DLWQ_BOUNDARY_VALUE    = 4,  // Concentration at a single boundary (one value)
        DLWQ_DISCHARGE         = 5,  // Concentration at a single discharge location (one value)
        DLWQ_MONITOR_POINT     = 6,  // Concentration at a single monitoring location (one value)
        DLWQ_ACTIVE_SUBSTANCES = 7,  // Retrieve number of transportable (active) substances
        DLWQ_ALL_SUBSTANCES    = 8   // Retrieve number of all substances
    }

    enum errorLevels
    {
        debug = 1,
        info,
        warning,
        error,
        fatal
    }

    public class WaqModelApi : MarshalByRefObject, IWaqModelApi
    {
        static WaqModelApi()
        {
            var waqKernelFolder = DelwaqFileStructureHelper.GetDelwaqKernelMainFolderPath();
            NativeLibrary.LoadNativeDllForCurrentPlatform(DelwaqFileStructureHelper.DELWAQ2LIB_DLL, waqKernelFolder);
        }

        //logging
        private static readonly ILog log = LogManager.GetLogger(typeof(WaqModelApi));
        bool loggingEnabled = true;

        public bool LoggingEnabled
        {
            get { return loggingEnabled; }
            set { loggingEnabled = value; }
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SETPROCESSDEFINITION", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetProcessDefinition([In] String mode, [In] String processDefinitionFile, int modeLength, int pdfLength);

        public bool SetWQProcessDefinition(string mode, string processDefinitionFile)
        {
            int modeLength = mode.Length;
            int pdfLength = processDefinitionFile.Length;
            bool res = SetProcessDefinition(mode, processDefinitionFile, modeLength, pdfLength);
            LogMessages();
            return res;
        }
        
        //Note: time comprises both date and time as seconds since a reference date/time
        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SETSIMULATIONTIMES", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetSimulationTimes([In] ref int startTime, [In] ref int endTime, [In] ref int timeStep);

        public bool SetWQSimulationTimes(int startTime, int endTime, int timeStep)
        {
            bool res = SetSimulationTimes(ref startTime, ref endTime, ref timeStep);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SETREFERENCEDATE", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetReferenceDateTime([In] ref int year_in, [In] ref int month_in, [In] ref int day_in,
                                                [In] ref int hour_in, [In] ref int minute_in, [In] ref int second_in);

        public bool SetWQReferenceDateTime(int year_in, int month_in, int day_in,
                                         int hour_in, int minute_in, int second_in)
        {
            bool res = SetReferenceDateTime(ref year_in, ref month_in, ref day_in, ref hour_in, ref minute_in, ref second_in);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SETOUTPUTTIMERS", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetOutputTimers([In] ref int type, [In] ref int startTime, [In] ref int stopTime, [In] ref int timeStep);

        public bool SetWQOutputTimers(int type, int startTime, int stopTime, int timeStep)
        {
            bool res = SetOutputTimers(ref type, ref startTime, ref stopTime, ref timeStep);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SETCURRENTVALUESCALARINIT", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetCurrentValueScalarInit([In] String name, [In] ref float value, [In] int nameStringLength);

        public bool SetWQCurrentValueScalarInit(string name, double value)
        {
            float valueFloat = (float) value;
            int stringLength = name.Length;
            bool res = SetCurrentValueScalarInit(name, ref valueFloat, stringLength);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SETCURRENTVALUESCALARRUN", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetCurrentValueScalarRun([In] String name, [In] ref float value, [In] int nameStringLength);

        public bool SetWQCurrentValueScalarRun(string name, double value)
        {
            float valueFloat = (float)value;
            int stringLength = name.Length;
            bool res = SetCurrentValueScalarRun(name, ref valueFloat, stringLength);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "GETITEMINDEX", CallingConvention = CallingConvention.Cdecl)]
        static extern int GetItemIndex([In] ref int type, [In] String name, [In] int nameStringLength);

        public int GetWQItemIndex(int type, string name)
        {
            int stringLength = name.Length;
            int res = GetItemIndex(ref type, name, stringLength);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "GETLOCATIONCOUNT", CallingConvention = CallingConvention.Cdecl)]
        static extern int GetLocationCount([In] ref int type);

        public int GetWQLocationCount(int type)
        {
            int res = GetLocationCount(ref type);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "GETCURRENTVALUE", CallingConvention = CallingConvention.Cdecl)]
        static extern bool GetCurrentValue([In] String name, float[] value, int nameStringLength);

        public double[] GetWQCurrentValue(string name, int numberOfElements)
        {
            var valueFloat = new float[numberOfElements];
            GetCurrentValue(name, valueFloat, name.Length);
            
            return valueFloat.Select(Convert.ToDouble).ToArray();
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SETINTEGRATIONOPTIONS", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetIntegrationOptions([In] ref int method, [In] ref bool dispZeroFlow, [In] ref bool dispBound, [In] ref bool firstOrder,
                                                 [In] ref bool forester, [In] ref bool anticreep);

        public bool SetWQIntegrationOptions( int method, bool dispZeroFlow, bool dispBound, bool firstOrder,
                                             bool forester, bool anticreep)
        {
            bool res = SetIntegrationOptions( ref method, ref dispZeroFlow, ref dispBound, ref firstOrder,
                                              ref forester, ref anticreep);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SETBALANCEOUTPUTOPTIONS", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetBalanceOutputOptions([In] ref int type, [In] ref bool lumpProcesses, [In] ref bool lumpLoads, [In] ref bool lumpTransport,
                                                   [In] ref bool suppressSpace, [In] ref bool suppressTime, [In] ref int unitType);

        public bool SetWQBalanceOutputOptions(int type, bool lumpProcesses, bool lumpLoads, bool lumpTransport,
                                              bool suppressSpace, bool suppressTime, int unitType)
        {
            bool res = SetBalanceOutputOptions(ref type, ref lumpProcesses, ref lumpLoads, ref lumpTransport,
                                               ref suppressSpace, ref suppressTime, ref unitType);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "DEFINEWQSCHEMATISATION", CallingConvention = CallingConvention.Cdecl)]
        static extern bool DefineWQSchematisation([In] ref int numberSegments, [In] int[] pointerTable, [In] int[] numberExchanges);

        public bool DefineWQSchematisation(int numberSegments, int[] pointerTable, int[] numberExchanges)
        {
            bool res = DefineWQSchematisation(ref numberSegments, pointerTable, numberExchanges);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "DEFINEWQDISPERSION", CallingConvention = CallingConvention.Cdecl)]
        static extern bool DefineWQDispersion([In] float[] dispc, [In] float[] length);

        public bool DefineWQDispersion(double[] dispc, double[] length)
        {
            float[] dispcFloat  = dispc.Select(d => (float) d).ToArray();
            float[] lengthFloat = length.Select(d => (float) d).ToArray();

            bool res = DefineWQDispersion(dispcFloat, lengthFloat);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "DEFINEWQPROCESSES", CallingConvention = CallingConvention.Cdecl)]
        static extern bool DefineWQProcesses([In] StringBuilder substance, [In] ref int numberSubstances, [In] ref int numberTransported,
                                             [In] StringBuilder processParameter, [In] ref int numberParameters, [In] StringBuilder process,
                                             [In] ref int numberProcesses, [In] int substanceStringLength, [In] int parameterSringLength,
                                             [In] int processStringLength);

        public bool DefineWQProcesses(string[] substance, int numberSubstances, int numberTransported,
                                      string[] processParameter, int numberParameters, string[] process,
                                      int numberProcesses)
        {
            var substanceNames = new StringBuilder(Str2Builder(substance, 20));
            var parameterNames = new StringBuilder(Str2Builder(processParameter, 20));
            var processNames   = new StringBuilder(Str2Builder(process, 20));

            bool res = DefineWQProcesses(substanceNames, ref numberSubstances, ref numberTransported,
                                         parameterNames, ref numberParameters, processNames,
                                         ref numberProcesses, 20, 20, 20);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "DEFINEDISCHARGELOCATIONS", CallingConvention = CallingConvention.Cdecl)]
        static extern bool DefineDischargeLocations([In] int[] cell, [In] ref int numberLocations);

        public bool DefineWQDischargeLocations([In] int[] cell, [In] int numberLocations)
        {
            bool res = DefineDischargeLocations(cell, ref numberLocations);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "DEFINEMONITORINGLOCATIONS", CallingConvention = CallingConvention.Cdecl)]
        static extern bool DefineMonitoringLocations([In] int[] cell, [In] StringBuilder name, [In] ref int numberLocations, int nameStringLength);

        public bool DefineWQMonitoringLocations(int[] cell, string[] name, int numberLocations)
        {
            var names = new StringBuilder(Str2Builder(name, 20));
            bool res = DefineMonitoringLocations(cell, names, ref numberLocations, 20);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SETINITIALVOLUME", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetInitialVolume([In] float[] volume);

        public bool SetWQInitialVolume(double[] volume)
        {
            float[] volumeFloat = volume.Select(d => (float) d).ToArray();
            bool res = SetInitialVolume(volumeFloat);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SETFLOWDATA", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetFlowData([In] float[] volume, [In] float[] area, [In] float[] flow);

        public bool SetWQFlowData(double[] volume, double[] area, double[] flow)
        {
            float[] volumeFloat = volume.Select(d => (float) d).ToArray();
            float[] areaFloat   = area.Select(d => (float) d).ToArray();
            float[] flowFloat   = flow.Select(d => (float) d).ToArray();
            bool res = SetFlowData(volumeFloat, areaFloat, flowFloat);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "CORRECTVOLUMESURFACE", CallingConvention = CallingConvention.Cdecl)]
        static extern bool CorrectVolumeSurface([In] float[] volume, [In] float[] surf, [In] ref int mass);

        public bool CorrectWQVolumeSurface(double[] volume, double[] surf)
        {
            float[] volumeFloat = volume.Select(d => (float) d).ToArray();
            float[] surfFloat = surf.Select(d => (float) d).ToArray();
            var mass = 1;
            bool res = CorrectVolumeSurface(volumeFloat, surfFloat, ref mass);

            LogMessages();
            return res;
        }

        /// <summary>
        /// Set the current value of a substance or process parameter:
        /// This function provides a general interface to the state variables and computational parameters.
        /// </summary>  
        /// <param name="dlwqtype">Type of parameter/location to be set</param>
        /// <param name="parid">Index of the parameter</param>
        /// <param name="locid">Index of the location</param>
        /// <param name="operation">Operation to apply</param>
        /// <param name="number">Number of values</param>
        /// <param name="values">Value(s) to be used in the operation</param>
        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SET_VALUES_GENERAL", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetValuesGeneral([In] ref int dlwqtype, [In] ref int parid, [In] ref int locid, [In] ref int operation, [In] ref int number, [In] float[] values);
        
        public bool SetWQValuesGeneral(string parameterName, DelwaqItem parameterType, double[] value)
        {
            // Apply a set operation to all segments for the parameter type
            var operationId = (int) OperationType.OperSet;
            var locationId = (int) LocationTypes.LocTypeAllSegments;
            var dlwqtype = (int) parameterType;
            
            // Get the index of the parameter
            var parameterId = GetWQItemIndex(dlwqtype, parameterName.PadRight(20));

            // Convert the values to floats and obtain the length of the array
            var values = value.Select(d => (float) d).ToArray();
            var number = values.Length;

            return SetValuesGeneral(ref dlwqtype, ref parameterId, ref locationId, ref operationId, ref number, values);
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SETWASTELOADVALUES", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetWasteLoadValues([In] ref int idx, [In] float[] value);

        public bool SetWQWasteLoadValues(int idx, double[] value)
        {
            float[] valueFloat = value.Select(d => (float) d).ToArray();
            bool res = SetWasteLoadValues(ref idx, valueFloat);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "SETBOUNDARYCONDITIONS", CallingConvention = CallingConvention.Cdecl)]
        static extern bool SetBoundaryConditions([In] ref int idx, [In] float[] value);

        public bool SetWQBoundaryConditions(int idx, double[] value)
        {
            float[] valueFloat = value.Select(d => (float) d).ToArray();
            bool res = SetBoundaryConditions(ref idx, valueFloat);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "WRITERESTARTFILEDEFAULTNAME", CallingConvention = CallingConvention.Cdecl)]
        static extern int WriteRestartFileDefaultName();

        public void WriteRestart()
        {
            WriteRestartFileDefaultName();
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "MODELPERFORMTIMESTEP", CallingConvention = CallingConvention.Cdecl)]
        static extern bool ModelPerformTimeStep();
        public bool WaqPerformTimeStep()
        {
            bool res;
            try
            {
                res = ModelPerformTimeStep();
            }
            catch (Exception)
            {
                res = false;
            }
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "MODELINITIALIZE", CallingConvention = CallingConvention.Cdecl)]
        static extern bool ModelInitialize();

        public bool WaqInitialize()
        {
            try
            {
                 bool res = ModelInitialize();
                LogMessages();
                return res;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "MODELINITIALIZE_BY_ID", CallingConvention = CallingConvention.Cdecl)]
        static extern bool ModelInitialize_By_Id([In] String runid, int runidStringLength);

        public bool WaqInitialize_By_Id(string name)
        {
            bool res = ModelInitialize_By_Id(name, name.Length);
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "MODELFINALIZE", CallingConvention = CallingConvention.Cdecl)]
        static extern bool ModelFinalize();

        public bool WaqFinalize()
        {
            bool res = ModelFinalize();
            LogMessages();
            return res;
        }

        [DllImport(DelwaqFileStructureHelper.DELWAQ2LIB, EntryPoint = "GETWQCURRENTTIME", CallingConvention = CallingConvention.Cdecl)]
        static extern int GetWQCurrentTime(ref double time);

        public double WaqGetCurrentTime()
        {
            double time = 0.0;
            int res = GetWQCurrentTime(ref time);
            return time;
        }

        public void LogMessages()
        {
            //TODO
        }

        private static string Str2Builder(IEnumerable<string> names, int length)
        {
            return names.Aggregate("", (c, n) => c + n.PadRight(length));
        }
    }
}
