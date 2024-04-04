using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BasicModelInterface;
using DelftTools.Utils.Interop;
using DeltaShell.NGHS.Common;
using log4net;

namespace DeltaShell.Dimr
{
    public class DimrApi : IDimrApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DimrApi));

        static DimrApi()
        {
            DimrApiDataSet.AddKernelDirToPath();
            NativeLibrary.LoadNativeDll(DimrApiDataSet.DimrDllName, DimrApiDataSet.DimrDllDirectory);
        }

        private readonly bool useMessagesBuffering;
        private double tStart;
        private double tEnd;
        private double tStep;
        private double tCurrent;
        private List<string> messages;
        private readonly DimrDll.Message_Callback cMessageCallback; // keep the callback so it doesn't get garbage collected!
        private DateTime currentTime;
        private DateTime dimrRefDate;
        private double relativeStartTime;
        public DimrApi() : this(true) {}

        public DimrApi(bool useMessagesBuffering)
        {
            tStart = tEnd = tStep = tCurrent = 0;
            dimrRefDate = DateTime.MinValue;
            this.useMessagesBuffering = useMessagesBuffering;
            messages = new List<string>();
            SetLoggingLevel(DimrLogging.FeedbackLevelKey, DimrLogging.FeedbackLevel);
            SetLoggingLevel(DimrLogging.LogFileLevelKey, DimrLogging.LogFileLevel);
            cMessageCallback = FeedbackLog;
            set_feedback_logger();
            Logger = BMI_Logger_function;
        }

        public string KernelDirs { get; set; }

        public void set_logger()
        {
            DimrDll.set_logger(Logger);
        }

        [ExcludeFromCodeCoverage]
        private void BMI_Logger_function(Level level, string message)
        {
            string msg = message != null ? string.Copy(message) : string.Empty;

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

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DimrDll.set_logger_callback(null);
            }
        }

        #endregion

        #region Implementation of IDimrApi

        public virtual DateTime DimrRefDate
        {
            get
            {
                return dimrRefDate;
            }
            set
            {
                dimrRefDate = value;
                currentTime = dimrRefDate.AddSeconds(tCurrent);
            }
        }

        public DateTime StartTime
        {
            get
            {
                return DimrRefDate.AddSeconds(tStart - relativeStartTime);
            }
        }

        public DateTime StopTime
        {
            get
            {
                return DimrRefDate.AddSeconds(tEnd - relativeStartTime);
            }
        }

        public TimeSpan TimeStep
        {
            get
            {
                return new TimeSpan((long) (TimeSpan.TicksPerSecond * tStep));
            }
        }

        public string[] VariableNames { get; private set; }
        public Logger Logger { get; set; }

        public DateTime CurrentTime
        {
            get
            {
                return currentTime;
            }
        }

        public void set_feedback_logger()
        {
            DimrDll.set_logger_callback(cMessageCallback);
        }

        [ExcludeFromCodeCoverage]
        private void FeedbackLog(string time, string message, uint level)
        {
            string msg = message != null ? string.Copy(message) : string.Empty;
            var dateTimeString = string.Empty;
            try
            {
                DateTime dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddSeconds((long) double.Parse(time.Split('.')[0], CultureInfo.InvariantCulture))
                                                                                          .AddMilliseconds((long) double.Parse(time.Split('.')[1], CultureInfo.InvariantCulture))
                                                                                          .AddDays(-1)
                                                                                          .ToLocalTime();
                dateTimeString = dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            catch
            {
                dateTimeString = time;
            }

            var debugLevel = Level.Info;
            if (Enum.IsDefined(typeof(Level), (int) level))
            {
                debugLevel = (Level) level;
            }

            msg = string.Format("Dimr [{0}] {1} >> {2}", dateTimeString, Enum.GetName(typeof(Level), debugLevel), msg);
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

        /// <summary>
        /// Initializes this <see cref="DimrApi"/>.
        /// </summary>
        /// <param name="path">Path to the DIMR XML file.</param>
        /// <returns>The exit code of initializing this <see cref="DimrApi"/>.</returns>
        public int Initialize(string path)
        {
            string previousDir = Environment.CurrentDirectory;

            try
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(path);
                LogMsg(string.Format("Running dimr in : {0}", Environment.CurrentDirectory));

                string environmentPathVariable = Environment.GetEnvironmentVariable(EnvironmentConstants.PathKey);

                environmentPathVariable = KernelDirs + ";" +
                                          DimrApiDataSet.DimrDllDirectory + ";" +
                                          environmentPathVariable;
                Environment.SetEnvironmentVariable(EnvironmentConstants.PathKey, environmentPathVariable, EnvironmentVariableTarget.Process);

                LogMsg(string.Format("Path used: {0}", Environment.GetEnvironmentVariable(EnvironmentConstants.PathKey)));

                byte useMpi = 0;

                // Allocating memory for int
                IntPtr intPointer = Marshal.AllocHGlobal(sizeof(byte));

                Marshal.WriteByte(intPointer, useMpi);

                // sending intPointer to unmanaged code here

                DimrDll.set_var("useMPI", intPointer);

                // Free memory
                Marshal.FreeHGlobal(intPointer);
                var numranks = 1;

                // Allocating memory for int
                intPointer = Marshal.AllocHGlobal(sizeof(int));

                Marshal.WriteInt32(intPointer, numranks);

                // sending intPointer to unmanaged code here

                DimrDll.set_var("numRanks", intPointer);

                // Free memory
                Marshal.FreeHGlobal(intPointer);

                var my_rank = 0;

                // Allocating memory for int
                intPointer = Marshal.AllocHGlobal(sizeof(int));

                Marshal.WriteInt32(intPointer, my_rank);

                // sending intPointer to unmanaged code here

                DimrDll.set_var("myRank", intPointer);

                // Free memory
                Marshal.FreeHGlobal(intPointer);

                int returnCode = DimrDll.initialize(path);
                if (returnCode != 0)
                {
                    return returnCode;
                }

                DimrDll.get_start_time(ref tStart);
                DimrDll.get_end_time(ref tEnd);
                DimrDll.get_time_step(ref tStep);
                DimrDll.get_current_time(ref tCurrent);
                relativeStartTime = tCurrent;
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

        /// <summary>
        /// Sets the logging level.
        /// </summary>
        /// <param name="logType">Type of the debug level.</param>
        /// <param name="level">The level.</param>
        public void SetLoggingLevel(string logType, Level level)
        {
            // Allocating memory for long
            IntPtr intPointer = Marshal.AllocHGlobal(sizeof(int));

            Marshal.WriteInt32(intPointer, (int) level);

            // sending intPointer to unmanaged code here

            DimrDll.set_var(logType, intPointer);
            // Free memory
            Marshal.FreeHGlobal(intPointer);
        }

        private void LogMsg(string message)
        {
            string msg = message != null ? string.Copy(message) : string.Empty;
            if (useMessagesBuffering)
            {
                messages.Add(msg);
            }

            Log.Info(msg);
        }

        /// <summary>
        /// Updates this <see cref="DimrApi"/> with the specified time step <paramref name="dt"/>.
        /// </summary>
        /// <param name="dt">The time step dt.</param>
        /// <returns>The exit code of the Update call.</returns>
        public int Update(double dt = -1.0)
        {
            int returnCode = DimrDll.update(dt);

            if (returnCode != 0)
            {
                return returnCode;
            }

            DimrDll.get_current_time(ref tCurrent);
            currentTime = DimrRefDate.AddSeconds(tCurrent - relativeStartTime);
            return 0;
        }

        public int Finish()
        {
            DimrDll.finalize();
            return 0;
        }

        public int[] GetShape(string variable)
        {
            return new int[]
                {};
        }

        public Array GetValues(string variable)
        {
            double[] value =
            {
                default(double)
            };
            GCHandle handle = GCHandle.Alloc(value.Length * Marshal.SizeOf(typeof(double)), GCHandleType.Pinned);
            IntPtr dPtr = handle.AddrOfPinnedObject();
            try
            {
                DimrDll.get_var(variable, ref dPtr);
                Marshal.Copy(dPtr, value, 0, value.Length);
            }
            finally
            {
                if (dPtr != IntPtr.Zero)
                {
                    handle.Free();
                }

                dPtr = IntPtr.Zero;
            }

            return value;
        }

        public Array GetValues(string variable, int[] index)
        {
            return Array.Empty<double>();
        }

        public Array GetValues(string variable, int[] start, int[] count)
        {
            return Array.Empty<double>();
        }

        public void SetValuesInt(string variable, int[] values)
        {
            if (values == null)
            {
                return;
            }

            // Allocating memory for double[]
            IntPtr intArrayPointer = Marshal.AllocHGlobal(sizeof(int) * values.Length);

            // place int array values into pointer
            Marshal.Copy(values, 0, intArrayPointer, values.Length);

            // sending doubleArrayPointer to unmanaged code here
            DimrDll.set_var(variable, intArrayPointer);

            // Free memory
            Marshal.FreeHGlobal(intArrayPointer);
        }

        public void SetValuesDouble(string variable, double[] values)
        {
            if (values == null)
            {
                return;
            }

            // Allocating memory for double[]
            IntPtr doubleArrayPointer = Marshal.AllocHGlobal(sizeof(double) * values.Length);

            // place double array values into pointer
            Marshal.Copy(values, 0, doubleArrayPointer, values.Length);

            // sending doubleArrayPointer to unmanaged code here
            DimrDll.set_var(variable, doubleArrayPointer);

            // Free memory
            Marshal.FreeHGlobal(doubleArrayPointer);
        }

        public void SetValues(string variable, Array values)
        {
            if (values is double[] valuesDouble)
            {
                SetValuesDouble(variable, valuesDouble);
            }
        }

        public void SetValues(string variable, int[] start, int[] count, Array values)
        {
            // Not needed.
        }
        
        public void SetValues(string variable, int[] index, Array values)
        {
            // Not needed.
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
                    string[] messagesFromDimr = messages.ToArray();
                    messages.Clear();
                    return messagesFromDimr;
                }

                return new[]
                {
                    string.Empty
                };
            }
        }

        public void ProcessMessages()
        {
            if (useMessagesBuffering)
            {
                string[] infoMsgs = Messages;
                if (infoMsgs.Length > 0 && !(infoMsgs.Length == 1 && infoMsgs[0] == string.Empty))
                {
                    foreach (string infoMsg in infoMsgs)
                    {
                        Log.Info(infoMsg);
                    }
                }
            }
        }

        #endregion
    }
}