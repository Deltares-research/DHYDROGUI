using System;
using BasicModelInterface;

namespace DeltaShell.Plugins.FMSuite.Wave.Api
{
    public interface IWaveModelApi : IBasicModelInterface, IDisposable
    {
        DateTime ReferenceDateTime { get; set; }
    }
}