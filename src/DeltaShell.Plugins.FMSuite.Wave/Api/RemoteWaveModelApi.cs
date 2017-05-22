using System;
using System.Threading;
using DelftTools.Utils.Remoting;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.FMSuite.Wave.Api
{
    public class RemoteWaveModelApi : IWaveModelApi
    {
        private bool disposed;
        private IWaveModelApi remoteInstanceApi;

        public RemoteWaveModelApi(bool showConsole)
        {
            remoteInstanceApi =
                RemoteInstanceContainer.CreateInstance<IWaveModelApi, WaveModelApi>(Environment.Is64BitOperatingSystem, null, showConsole,  typeof(DimrApi).Assembly);}

        public void Initialize(string mdwFilePath)
        {
            remoteInstanceApi.Initialize(mdwFilePath);
        }

        public void Update(double timestep)
        {
            remoteInstanceApi.Update(timestep);
        }

        public void Finish()
        {
            remoteInstanceApi.Finish();
        }

        public void SetVar(string variable, string value)
        {
            remoteInstanceApi.SetVar(variable, value);
        }

        public DateTime CurrentTime
        {
            get { return remoteInstanceApi.CurrentTime; }
        }

        public DateTime ReferenceDateTime
        {
            get { return remoteInstanceApi.ReferenceDateTime; }
            set { remoteInstanceApi.ReferenceDateTime = value; }
        }

        
        ~RemoteWaveModelApi()
        {
            // in case someone forgets to dispose..
            DisposeInternal();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            GC.SuppressFinalize(this);
            DisposeInternal();
        }

        private void DisposeInternal()
        {
            if (remoteInstanceApi != null)
                RemoteInstanceContainer.RemoveInstance(remoteInstanceApi);
            remoteInstanceApi = null;
            disposed = true;
            Thread.Sleep(100); // wait for process to truly exit
        }
    }
}