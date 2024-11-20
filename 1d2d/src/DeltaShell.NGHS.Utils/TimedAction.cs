using System;
using System.Diagnostics;

namespace DeltaShell.NGHS.Utils
{
    public class TimedAction : DisposableObjectWrapper<Stopwatch>
    {
        public TimedAction(Action<TimeSpan> afterAction): base(()=> new Stopwatch())
        {
            WrapperObject.Start();

            disposeAction = s =>
            {
                s.Stop();
                afterAction?.Invoke(s.Elapsed);
            };
        }
    }
}