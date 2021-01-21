using System;
using System.Linq;
using System.Threading;
using BasicModelInterface;
using DelftTools.Utils.Remoting;
using log4net;
using ProtoBufRemote;

namespace DeltaShell.Dimr
{
    public class RemoteDimrApi : IDimrApi
    {
        private bool disposed;
        private IDimrApi api;
        private static readonly ILog Log = LogManager.GetLogger(typeof(RemoteDimrApi));

        public RemoteDimrApi()
        {
            // DeltaShell is 32bit, however we still want to take advantage of the 64bit dimr.dll if the system can use it, 
            // so we need to start the 64bit worker. This works as long as the data send over the Api border 
            // is not bit dependent, eg IntPtr and the like.
            if (!RemotingTypeConverters.RegisteredConverters.OfType<LoggerToProtoConverter>().Any())
            {
                lock (RemotingTypeConverters.RegisteredConverters)
                {
                    RemotingTypeConverters.RegisterTypeConverter(new LoggerToProtoConverter());
                }
            }
            api = RemoteInstanceContainer.CreateInstance<IDimrApi, DimrApi>(true);
            SetLoggingLevel("feedbackLevel", DimrApiDataSet.FeedbackLevel);
            SetLoggingLevel("debugLevel", DimrApiDataSet.LogFileLevel);
            set_feedback_logger();
        }

        #region Implementation of IDisposable

        ~RemoteDimrApi()
        {
            // in case someone forgets to dispose..
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (api != null)
                    {
                        if (RemoteInstanceContainer.IsProcessAlive(api))
                        {
                            api.Dispose(); 
                        }

                        // connection will be broken because remote process is killed => results in InvalidOperationException
                        RemoteInstanceContainer.RemoveInstance(api);
                        
                        Thread.Sleep(100); // wait for process to truly exit
                    }
                    api = null;
                }
                disposed = true;
            }
        }

        #endregion

        #region Implementation of IDimrApi

        public string KernelDirs { get { return api.KernelDirs; } set { api.KernelDirs = value; } }
        public DateTime DimrRefDate { get { return api.DimrRefDate; } set { api.DimrRefDate = value; } }

        public void set_feedback_logger()
        {
            if (api != null) api.set_feedback_logger();
        }

        
        public int Initialize(string xmlFile)
        {
            if (api != null)
            {
                var state = api.Initialize(xmlFile);
                if (state != 0)
                {
                    ProcessMessages();
                }
                return state;
            }
            return 0;
        }

        public int Update(double step)
        {
            if (api != null)
            {
                var state = api.Update(step);
                if (state != 0)
                {
                    ProcessMessages();
                }
                return state;
            }

            return 0;
        }

        public int Finish()
        {
            if (api != null)
            {
                var state = api.Finish();
                if (state != 0)
                {
                    ProcessMessages();
                }
                return state;
            }

            return 0;
        }

        public int[] GetShape(string variable)
        {
            return new int[] {};
        }

        public Array GetValues(string variable)
        {
            return api.GetValues(variable);
        }

        public Array GetValues(string variable, int[] index)
        {
            return null;
        }

        public Array GetValues(string variable, int[] start, int[] count)
        {
            return null;
        }

        public void SetValues(string variable, Array values)
        {
            var doubles = values as double[];
            if (doubles != null)
            {
                SetValuesDouble(variable, doubles);
                return;
            }
            var ints = values as int[];
            if (ints != null)
            {
                SetValuesInt(variable, ints);
                return;
            }
            if (api != null) api.SetValues(variable, values);
        }

        public void SetValues(string variable, int[] start, int[] count, Array values)
        {
        }

        public void SetValues(string variable, int[] index, Array values)
        {
        }

        public DateTime StartTime { get { return api.StartTime; } }
        public DateTime StopTime { get { return api.StopTime; } }
        public DateTime CurrentTime { get { return api.CurrentTime; } }
        public TimeSpan TimeStep { get { return api.TimeStep; } }
        public string[] VariableNames { get { return api.VariableNames; } }
        public Logger Logger { get { return api.Logger; } set { api.Logger = value; } }
        
        public string[] Messages { get { return api != null ? api.Messages :  new []{string.Empty} ; } }
        public void ProcessMessages()
        {
            var infoMsgs = Messages;
            if (infoMsgs.Length > 0 && !(infoMsgs.Length == 1 && infoMsgs[0] == string.Empty))
            {
                foreach (var infoMsg in infoMsgs)
                {
                    var containsLogLevel  = new System.Text.RegularExpressions.Regex(@"(?<prefix>Dimr \[.+\][\ ])+(?<level>[a-zA-Z0-9]+)").Match(infoMsg);
                    if (containsLogLevel.Success)
                    {
                        var stringLevel = containsLogLevel.Groups["level"].Value;
                        if (Enum.TryParse(stringLevel, true, out Level level))
                        {
                            if (level >= Level.Error)
                            {
                                Log.ErrorFormat(infoMsg);
                                break;
                            }

                            if (level >= Level.Warning)
                            {
                                Log.WarnFormat(infoMsg);
                                break;
                            }
                            if (level >= Level.Info)
                            {
                                Log.InfoFormat(infoMsg);
                                break;
                            }
                            if (level >= Level.Debug)
                            {
                                Log.DebugFormat(infoMsg);
                                break;
                            }
                        }
                    }
                    // just an info message
                    Log.Info(infoMsg);
                }
            }
        }

        public void SetValuesDouble(string variable, double[] values)
        {
            if (api != null) api.SetValuesDouble(variable, values);
        }

        public void SetValuesInt(string variable, int[] values)
        {
            if (api != null) api.SetValuesInt(variable, values);
        }

        public void SetLoggingLevel(string logType, Level level)
        {
            if (api != null) api.SetLoggingLevel(logType, level);
        }

        #endregion

    }
}