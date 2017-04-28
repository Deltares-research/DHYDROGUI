using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    public class SignalProvider
    {
        public static IEnumerable<Type> GetAllSignals()
        {
            yield return typeof(LookupSignal);
        }

        public static string GetTitle(Type signalType)
        {
            if (signalType == typeof(LookupSignal))
            {
                return "Lookup Table";
            }
            throw new ArgumentException(@"Unsupported type", "signalType");
        }
    }
}
