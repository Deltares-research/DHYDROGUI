using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    public static class RuleProvider
    {
        public static IEnumerable<Type> GetAllRules()
        {
            yield return typeof(TimeRule);
            yield return typeof(IntervalRule);
            yield return typeof(HydraulicRule);
            yield return typeof(PIDRule);
            yield return typeof(RelativeTimeRule);
            yield return typeof(FactorRule);
        }

        public static string GetTitle(Type ruleType)
        {
            if (ruleType == typeof(TimeRule))
            {
                return "Time Rule";
            }

            if (ruleType == typeof(IntervalRule))
            {
                return "Interval Rule";
            }

            if (ruleType == typeof(HydraulicRule))
            {
                return "Lookup table";
            }

            if (ruleType == typeof(PIDRule))
            {
                return "PID Rule";
            }

            if (ruleType == typeof(RelativeTimeRule))
            {
                return "Relative Time Rule";
            }

            if (ruleType == typeof(FactorRule))
            {
                return "Invertor Rule";
            }

            if (ruleType.Name.Contains("Proxy")) //for tests... :-(
            {
                return "Proxy Rule";
            }

            throw new ArgumentException(@"Unsupported type", nameof(ruleType));
        }
    }
}