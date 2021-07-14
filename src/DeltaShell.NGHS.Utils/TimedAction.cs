using System;
using System.Diagnostics;

namespace DeltaShell.NGHS.Utils
{
    public class TimedAction : IDisposable
    {
        private readonly Action<TimeSpan> afterAction;
        private readonly Stopwatch stopWatch = new Stopwatch();

        public TimedAction(Action<TimeSpan> afterAction)
        {
            this.afterAction = afterAction;
            stopWatch.Start();
        }

        public void Dispose()
        {
            stopWatch.Stop();
            afterAction?.Invoke(stopWatch.Elapsed);
        }
    }
}