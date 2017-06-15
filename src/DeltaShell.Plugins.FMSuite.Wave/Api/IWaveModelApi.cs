using System;

namespace DeltaShell.Plugins.FMSuite.Wave.Api
{
    public interface IWaveModelApi : IDisposable
    {
        void Initialize(string mdwFilePath);
        void Update(double timestep = -1.0);
        void Finish();

        void SetVar(string variable, string value);

        DateTime StartTime { get; }
        DateTime CurrentTime { get; }
        DateTime ReferenceDateTime { get; set; }
    }
}