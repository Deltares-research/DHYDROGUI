using System;
using BasicModelInterface;

namespace DeltaShell.Dimr
{
    public interface IDimrApi : IDisposable, IBasicModelInterface
    {
        string KernelDirs { get; set; }
        DateTime DimrRefDate { get; set; }
        string[] Messages { get; }
        void set_feedback_logger();
        void ProcessMessages();
        void SetValuesDouble(string variable, double[] values);
        void SetValuesInt(string variable, int[] values);
        void SetLoggingLevel(string logType, Level level);
    }
}