using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BasicModelInterface;
using DelftTools.Utils.Interop;
using log4net;

namespace DeltaShell.Dimr
{
    public class DimrApi : IDimrApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DimrApi));
        private readonly bool useMessagesBuffering; 
        private double tStart;
        private double tEnd;
        private double tStep;
        private double tCurrent;
        private List<string> messages;
        private bool reduceLogging = false;
        public string KernelDirs { get; set; }
        private DimrApiWrapper.Message_Callback cMessageCallback; // keep the callback so it doesn't get garbage collected!
        
        static DimrApi()
        {
            DimrApiDataSet.SetSharedPath();
            NativeLibrary.LoadNativeDll(DimrApiDataSet.DIMR_DLL_NAME, DimrApiDataSet.DllPath);
        }
        public DimrApi():this(true){}

        public DimrApi(bool useMessagesBuffering)
        {
            tStart = tEnd = tStep = tCurrent = 0;
            this.useMessagesBuffering = useMessagesBuffering;
            messages = new List<string>();
            SetLoggingLevel(DimrApiDataSet.FEEDBACKLEVELKEY, Level.Debug);
            SetLoggingLevel(DimrApiDataSet.LOGFILELEVELKEY, Level.Debug);
            cMessageCallback = FeedbackLog;
            set_feedback_logger();
            Logger = BMI_Logger_function;
        }
        [ExcludeFromCodeCoverage]
        private void BMI_Logger_function(Level level, string message)
        {
            var msg = message != null ? string.Copy(message) : string.Empty;
            
            msg = string.Format("Dimr [{0}] {1} >> {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), Enum.GetName(typeof(Level), level), msg);
            if (useMessagesBuffering)
            {
                messages.Add(msg);
            }
            else
            {
                Console.WriteLine(msg);
                Log.DebugFormat(msg);
            }
        }
        public void set_logger()
        {
            DimrApiWrapper.set_logger(Logger);
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

        public void set_feedback_logger()
        {
            DimrApiWrapper.set_logger_callback(cMessageCallback);
        }

        [ExcludeFromCodeCoverage]
        private void FeedbackLog(string time, string message, uint level)
        {
            var msg = message != null ? string.Copy(message) : string.Empty;
            string dateTimeString = string.Empty;
            try
            {
                var dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddSeconds((long)(double.Parse(time.Split('.')[0], System.Globalization.CultureInfo.InvariantCulture)))
                    .AddMilliseconds((long)(double.Parse(time.Split('.')[1], System.Globalization.CultureInfo.InvariantCulture)))
                    .AddDays(-1)
                    .ToLocalTime();
                dateTimeString = dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            catch
            {
                dateTimeString = time;
            }
            Level debugLevel = Level.Info;
            if (Enum.IsDefined(typeof(Level), (int)level))
            {
                debugLevel = (Level) level;
            }
            
            msg = string.Format("Dimr [{0}] {1} >> {2}", dateTimeString , Enum.GetName(typeof(Level), debugLevel), msg);
            if (useMessagesBuffering)
            {
                messages.Add(msg);
            }
            else
            {
                Console.WriteLine(msg);
                Log.DebugFormat(msg);
            }
        }

        public int Initialize(string xmlFile)
        {
            
            var previousDir = Environment.CurrentDirectory;
            reduceLogging = false;

            try
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(xmlFile);
                LogMsg(string.Format("Running dimr in : {0}", Environment.CurrentDirectory));
                
                var path = Environment.GetEnvironmentVariable("PATH");

                path = KernelDirs + ";" +
                       DimrApiDataSet.DllPath + ";" +
                       path;
                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
                
                LogMsg(string.Format("Path used: {0}", Environment.GetEnvironmentVariable("PATH")));

                
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

        public void SetLoggingLevel(string debugLevelType, Level level)
        {
            // Allocating memory for long
            IntPtr intPointer = Marshal.AllocHGlobal(sizeof(int));

            Marshal.WriteInt32(intPointer, (int)level);

            // sending intPointer to unmanaged code here

            DimrApiWrapper.set_var(debugLevelType, intPointer);
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
            return 0;
        }

        public int Finish()
        {
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
            GCHandle handle = GCHandle.Alloc(value.Length * Marshal.SizeOf(typeof(double)), GCHandleType.Pinned);
            IntPtr dPtr = handle.AddrOfPinnedObject();
            try
            {
                DimrApiWrapper.get_var(variable, ref dPtr);
                Marshal.Copy(dPtr, value, 0, value.Length);
            }
            finally
            {
                if (dPtr != IntPtr.Zero)
                    handle.Free();
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
            if (useMessagesBuffering)
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
            DimrApiWrapper.set_logger_callback(null);
        }

        #endregion
    }
}