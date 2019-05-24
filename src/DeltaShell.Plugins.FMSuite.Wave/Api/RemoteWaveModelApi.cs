using System;
using System.Threading;
using BasicModelInterface;
using DelftTools.Utils.Remoting;
using DeltaShell.Dimr;
using ProtoBufRemote;

namespace DeltaShell.Plugins.FMSuite.Wave.Api
{
    public class RemoteWaveModelApi : IWaveModelApi
    {
        private bool disposed;
        private IWaveModelApi remoteInstanceApi;

        public RemoteWaveModelApi(bool showConsole)
        {
            RemotingTypeConverters.RegisterTypeConverter(new LoggerToProtoConverter());
            remoteInstanceApi =
                RemoteInstanceContainer.CreateInstance<IWaveModelApi, WaveModelApi>(
                    Environment.Is64BitOperatingSystem, null, showConsole, typeof(DimrApi).Assembly);
        }

        public int Initialize(string mdwFilePath)
        {
            return remoteInstanceApi.Initialize(mdwFilePath);
        }

        public int Update(double timestep)
        {
            return remoteInstanceApi.Update(timestep);
        }

        public int Finish()
        {
            return remoteInstanceApi.Finish();
        }

        public int[] GetShape(string variable)
        {
            return remoteInstanceApi.GetShape(variable);
        }

        public Array GetValues(string variable)
        {
            return remoteInstanceApi.GetValues(variable);
        }

        public Array GetValues(string variable, int[] index)
        {
            return remoteInstanceApi.GetValues(variable, index);
        }

        public Array GetValues(string variable, int[] start, int[] count)
        {
            return remoteInstanceApi.GetValues(variable, start, count);
        }

        public void SetValues(string variable, Array values)
        {
            remoteInstanceApi.SetValues(variable, values);
        }

        public void SetValues(string variable, int[] start, int[] count, Array values)
        {
            remoteInstanceApi.SetValues(variable, start, count, values);
        }

        public void SetValues(string variable, int[] index, Array values)
        {
            remoteInstanceApi.SetValues(variable, index, values);
        }

        public DateTime StartTime => remoteInstanceApi.StartTime;
        public DateTime StopTime => remoteInstanceApi.StopTime;

        public DateTime CurrentTime => remoteInstanceApi.CurrentTime;

        public TimeSpan TimeStep => remoteInstanceApi.TimeStep;
        public string[] VariableNames => remoteInstanceApi.VariableNames;

        public Logger Logger
        {
            get => remoteInstanceApi.Logger;
            set => remoteInstanceApi.Logger = value;
        }

        public DateTime ReferenceDateTime
        {
            get => remoteInstanceApi.ReferenceDateTime;
            set => remoteInstanceApi.ReferenceDateTime = value;
        }

        ~RemoteWaveModelApi()
        {
            // in case someone forgets to dispose..
            DisposeInternal();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            GC.SuppressFinalize(this);
            DisposeInternal();
        }

        private void DisposeInternal()
        {
            if (remoteInstanceApi != null)
            {
                RemoteInstanceContainer.RemoveInstance(remoteInstanceApi);
            }

            remoteInstanceApi = null;
            disposed = true;
            Thread.Sleep(100); // wait for process to truly exit
        }
    }
}