using System;
using System.Threading;
using BasicModelInterface;
using DelftTools.Utils.Remoting;
using log4net;
using ProtoBufRemote;

namespace DeltaShell.Dimr
{
    public class RemoteDimrApi : IDimrApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RemoteDimrApi));
        private bool disposed;
        private IDimrApi api;

        public RemoteDimrApi()
        {
            // DeltaShell is 32bit, however we still want to take advantage of the 64bit dimr.dll if the system can use it, 
            // so we need to start the 64bit worker. This works as long as the data send over the Api border 
            // is not bit dependent, eg IntPtr and the like.
            RemotingTypeConverters.RegisterTypeConverter(new LoggerToProtoConverter());
            api = RemoteInstanceContainer.CreateInstance<IDimrApi, DimrApi>();
            api.SetLoggingLevel(DimrLogging.FeedbackLevelKey, DimrLogging.FeedbackLevel);
            api.SetLoggingLevel(DimrLogging.LogFileLevelKey, DimrLogging.LogFileLevel);
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
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (api != null)
                {
                    // Issue: SOBEK3-1523
                    // This try/catch was introduced to handle exceptions that might occur
                    // when disposing the API, this should be replaced by a check if the api
                    // thread is still valid, once the framework supports this.
                    try
                    {
                        api.Dispose();
                    }
                    catch (InvalidOperationException e)
                    {
                        Log.Debug(e.Message);
                    }
                    finally
                    {
                        RemoteInstanceContainer.RemoveInstance(api);
                        Thread.Sleep(100); // wait for process to truly exit
                    }
                }

                api = null;
            }

            disposed = true;
        }

        #endregion

        #region Implementation of IDimrApi

        public string KernelDirs
        {
            get => api.KernelDirs;
            set => api.KernelDirs = value;
        }

        public DateTime DimrRefDate
        {
            get => api.DimrRefDate;
            set => api.DimrRefDate = value;
        }

        public void set_feedback_logger()
        {
            api?.set_feedback_logger();
        }

        /// <summary>
        /// Initializes this <see cref="RemoteDimrApi"/>.
        /// </summary>
        /// <param name="path">Path to the DIMR XML file.</param>
        /// <returns>The exit code of initializing this <see cref="RemoteDimrApi"/>.</returns>
        public int Initialize(string path)
        {
            return api?.Initialize(path) ?? 1;
        }

        /// <summary>
        /// Updates this <see cref="RemoteDimrApi"/> with the specified time step <paramref name="dt"/>.
        /// </summary>
        /// <param name="dt">Optional; the time step dt. Default value is -1D.</param>
        /// <returns>The exit code of the Update call.</returns>
        public int Update(double dt = -1D)
        {
            return api?.Update(dt) ?? 1;
        }

        public int Finish()
        {
            api?.Finish();
            return 0;
        }

        public int[] GetShape(string variable)
        {
            return new int[]
                {};
        }

        public Array GetValues(string variable)
        {
            return api.GetValues(variable);
        }

        public Array GetValues(string variable, int[] index)
        {
            return Array.Empty<double>();
        }

        public Array GetValues(string variable, int[] start, int[] count)
        {
            return Array.Empty<double>();
        }

        public void SetValues(string variable, Array values)
        {
            switch (values)
            {
                case double[] doubles:
                    SetValuesDouble(variable, doubles);
                    return;
                case int[] ints:
                    SetValuesInt(variable, ints);
                    return;
                default:
                    api?.SetValues(variable, values);
                    break;
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

        public DateTime StartTime => api.StartTime;

        public DateTime StopTime => api.StopTime;

        public DateTime CurrentTime => api.CurrentTime;

        public TimeSpan TimeStep => api.TimeStep;

        public string[] VariableNames => api.VariableNames;

        public Logger Logger
        {
            get => api.Logger;
            set => api.Logger = value;
        }

        public string[] Messages
        {
            get
            {
                return api != null
                           ? api.Messages
                           : new[]
                           {
                               string.Empty
                           };
            }
        }

        public void ProcessMessages()
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

        public void SetValuesDouble(string variable, double[] values)
        {
            api?.SetValuesDouble(variable, values);
        }

        public void SetValuesInt(string variable, int[] values)
        {
            api?.SetValuesInt(variable, values);
        }

        public void SetLoggingLevel(string logType, Level level)
        {
            api?.SetLoggingLevel(logType, level);
        }

        #endregion
    }
}