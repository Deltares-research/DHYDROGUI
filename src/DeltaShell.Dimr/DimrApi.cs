using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BasicModelInterface;
using DelftTools.Utils.Interop;
using log4net;

namespace DeltaShell.Dimr
{
    public class DimrApiDataSet
    {
        public const string DIMRDLL_NAME = "dimr_dll.dll";
        public const string DIMREXE_NAME = "dimr.exe";

        public static string DllDirectory
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(typeof(DimrApi).Assembly.Location), "kernels");
            }
        }

        public static string DllPath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", DIMRDLL_NAME); }
        }

        public static string ExePath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", DIMREXE_NAME); }
        }

        [Flags]
        public enum DebugLevel
        {
            SILENT            = 1,
            ALWAYS            = 2,
            WARN              = 4,
            MAJOR             = 8,
            MINOR             = 16,
            DETAIL            = 16,
            CONFIG_MAJOR      = 32,
            ITER_MAJOR        = 64,
            DDMAPPER_MAJOR    = 128,
            RESERVED_9        = 256,
            RESERVED_10       = 512,
            RESERVED_11       = 1024,
            RESERVED_12       = 2048,
            RESERVED_13       = 4096,
            RESERVED_14       = 8192,
            RESERVED_15       = 16384,
            
            CONFIG_MINOR      = 32768,
            ITER_MINOR        = 65536,
            DDMAPPER_MINOR    = 131072,
            DD_SENDRECV       = 262144,
            DD_SEMAPHORE      = 524288,
            RESERVED_21       = 1048576,
            RESERVED_22       = 2097152,
            RESERVED_23       = 4194304,
            RESERVED_24       = 8388608,
            RESERVED_25       = 16777216,
            RESERVED_26       = 33554432,
            RESERVED_27       = 67108864,
            RESERVED_28       = 134217728,
            RESERVED_29       = 268435456,
            RESERVED_30       = 536870912,
            LOG_DETAIL        = 1073741824,
        }

    }
    public class DimrApi : IDimrApi
    {
        private const DimrApiDataSet.DebugLevel DebugLevel = DimrApiDataSet.DebugLevel.ALWAYS | DimrApiDataSet.DebugLevel.WARN |
                                              DimrApiDataSet.DebugLevel.MAJOR | DimrApiDataSet.DebugLevel.MINOR | DimrApiDataSet.DebugLevel.DETAIL;

        private const DimrApiDataSet.DebugLevel ExecuteLevel = DimrApiDataSet.DebugLevel.ALWAYS | DimrApiDataSet.DebugLevel.WARN;

        private static readonly ILog Log = LogManager.GetLogger(typeof(DimrApi));
        private readonly bool useMessagesBuffering; 
        private double tStart;
        private double tEnd;
        private double tStep;
        private double tCurrent;
        private List<string> messages;
        private DimrApiWrapper.Message_Callback cMessageCallback; // keep the callback so it doesn't get garbage collected!
        private bool reduceLogging = false;
        public string KernelDirs { get; set; }

        static DimrApi()
        {
            NativeLibrary.LoadNativeDllForCurrentPlatform(DimrApiDataSet.DIMRDLL_NAME, DimrApiDataSet.DllDirectory);
        }
        public DimrApi():this(true){}

        public DimrApi(bool useMessagesBuffering)
        {
            tStart = tEnd = tStep = tCurrent = 0;
            this.useMessagesBuffering = useMessagesBuffering;
            messages = new List<string>();
            set_logger();
        }

        #region Implementation of IDimrApi

        public virtual DateTime DimrRefDate { get; set; }
        public void SetValues(string variable, int[] index, Array values)
        {
        }

        public DateTime StartTime 
        {
            get { return DimrRefDate.AddSeconds(tStart); }
        }

        public DateTime StopTime
        {
            get { return DimrRefDate.AddSeconds(tEnd); }
        }

        public TimeSpan TimeStep
        {
            get { return new TimeSpan((long)(TimeSpan.TicksPerSecond * tStep)); }
        }

        public string[] VariableNames { get; private set; }
        public Logger Logger { get; set; }

        public DateTime CurrentTime
        {
            get { return DimrRefDate.AddSeconds(tCurrent); }
        }

        public void set_logger()
        {
            cMessageCallback = message =>
            {
                var msg = message != null ? string.Copy(message) : string.Empty;

                if (useMessagesBuffering)
                {
                    messages.Add(msg);
                }
                else
                {
                    Console.WriteLine("message = {0}", msg);
                    Log.Debug(msg);
                }
            };
            DimrApiWrapper.set_logger_callback(cMessageCallback);
        }

        public int Initialize(string xmlFile)
        {
            
            var previousDir = Environment.CurrentDirectory;
            reduceLogging = false;

            try
            {
                //Debugger.Launch(); 
                //Debugger.Break(); 
                Environment.CurrentDirectory = Path.GetDirectoryName(xmlFile);
                LogMsg(string.Format("Running dimr in : {0}", Environment.CurrentDirectory));


                var path = Environment.GetEnvironmentVariable("PATH");

                path = KernelDirs + ";" +
                       Path.GetDirectoryName(DimrApiDataSet.DllPath) + ";" +
                       path;
                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);

                LogMsg(string.Format("Path used: {0}", Environment.GetEnvironmentVariable("PATH")));

                SetLoggingLevel((int) DebugLevel);

                byte useMpi = 0;
                
                
                // Allocating memory for int
                IntPtr intPointer = Marshal.AllocHGlobal(sizeof(byte));

                Marshal.WriteByte(intPointer, useMpi);

                // sending intPointer to unmanaged code here

                DimrApiWrapper.set_var("useMPI", intPointer);

                // Free memory
                Marshal.FreeHGlobal(intPointer);
                int numranks = 1;

                // Allocating memory for int
                intPointer = Marshal.AllocHGlobal(sizeof(int));

                Marshal.WriteInt32(intPointer, numranks);

                // sending intPointer to unmanaged code here

                DimrApiWrapper.set_var("numRanks", intPointer);

                // Free memory
                Marshal.FreeHGlobal(intPointer);

                int my_rank = 0;

                // Allocating memory for int
                intPointer = Marshal.AllocHGlobal(sizeof(int));

                Marshal.WriteInt32(intPointer, my_rank);

                // sending intPointer to unmanaged code here

                DimrApiWrapper.set_var("myRank", intPointer);

                // Free memory
                Marshal.FreeHGlobal(intPointer);

                var result = DimrApiWrapper.initialize(xmlFile);
                if (result != 0)
                {
                    //throw new Exception("dimr returned error code " + result);
                    return result;
                }
                DimrApiWrapper.get_start_time(ref tStart);
                DimrApiWrapper.get_end_time(ref tEnd);
                DimrApiWrapper.get_time_step(ref tStep);
                DimrApiWrapper.get_current_time(ref tCurrent);
            }
            catch (Exception exception)
            {
                LogMsg(exception.Message);
                return 1;
            }
            finally
            {
                Environment.CurrentDirectory = previousDir;
            }
            return 0;
        }

        private static void SetLoggingLevel(int debugLevel)
        {
            // Allocating memory for int
            IntPtr intPointer = Marshal.AllocHGlobal(sizeof(int));

            Marshal.WriteInt32(intPointer, debugLevel);

            // sending intPointer to unmanaged code here

            DimrApiWrapper.set_var("debugLevel", intPointer);
            // Free memory
            Marshal.FreeHGlobal(intPointer);
        }

        private void LogMsg(string message)
        {
            var msg = message != null ? string.Copy(message) : string.Empty;
            if (useMessagesBuffering)
            {
                messages.Add(msg);
            }
            {
                Log.Info(msg);
            }
        }
        public int Update(double step)
        {
            DimrApiWrapper.update(step);
            DimrApiWrapper.get_current_time(ref tCurrent);
            if (!reduceLogging)
            {
                LogMsg("Will reduce logging from Dimr during updates to speed up process");
                reduceLogging = true;
                SetLoggingLevel((int)ExecuteLevel);
            }
            return 0;
        }

        public int Finish()
        {
            SetLoggingLevel((int)DebugLevel);
            DimrApiWrapper.finalize();
            return 0;
        }

        public int[] GetShape(string variable)
        {
            return new int[] {};
        }

        public Array GetValues(string variable)
        {
            double[] value = { default(double) };
            IntPtr dPtr = Marshal.AllocCoTaskMem(value.Length * Marshal.SizeOf(typeof(double)));
            try
            {
                DimrApiWrapper.get_var(variable, ref dPtr);
                Marshal.Copy(dPtr, value, 0, value.Length);
            }
            finally
            {
                if (dPtr != IntPtr.Zero)
                {
                    try
                    {
                        //Marshal.FreeCoTaskMem(dPtr);
                    }
                    catch { }
                }

                dPtr = IntPtr.Zero;
            }

            return value;
        }

        public Array GetValues(string variable, int[] index)
        {
            return null;
        }

        public Array GetValues(string variable, int[] start, int[] count)
        {
            return null;
        }

        public void SetValuesInt(string variable, int[] values)
        {
            if (values == null) return;
            // Allocating memory for double[]
            IntPtr intArrayPointer = Marshal.AllocHGlobal(sizeof(int) * values.Length);

            // place int array values into pointer
            Marshal.Copy(values, 0, intArrayPointer, values.Length);

            // sending doubleArrayPointer to unmanaged code here
            DimrApiWrapper.set_var(variable, intArrayPointer);

            // Free memory
            Marshal.FreeHGlobal(intArrayPointer);
        }
        public void SetValuesDouble(string variable, double[] values)
        {
            if(values == null) return;
            // Allocating memory for double[]
            IntPtr doubleArrayPointer = Marshal.AllocHGlobal(sizeof(double) * values.Length);

            // place double array values into pointer
            Marshal.Copy(values, 0, doubleArrayPointer, values.Length);

            // sending doubleArrayPointer to unmanaged code here
            DimrApiWrapper.set_var(variable, doubleArrayPointer);

            // Free memory
            Marshal.FreeHGlobal(doubleArrayPointer);
        }

        public void SetValues(string variable, Array values)
        {
            Debugger.Launch();
            var valuesDouble = values as double[];
            if (valuesDouble != null)
            {
                SetValuesDouble(variable, valuesDouble);

            }
        }

        public void SetValues(string variable, int[] start, int[] count, Array values)
        {
        }

        public string[] Messages
        {
            get
            {
                if (messages == null)
                {
                    messages = new List<string>();
                }
                if (messages.Any())
                {
                    var messagesFromDimr = messages.ToArray();
                    messages.Clear();
                    return messagesFromDimr;
                }
                return new [] { string.Empty };
            }
        }

        public void ProcessMessages()
        {
            if (!useMessagesBuffering)
            {
                var infoMsgs = Messages;
                if (infoMsgs.Length > 0 && !(infoMsgs.Length == 1 && infoMsgs[0] == string.Empty))
                {
                    foreach (var infoMsg in infoMsgs)
                    {
                        Log.Info(infoMsg);
                    }
                }
            }
        }

        
        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            //
        }

        #endregion
    }
}