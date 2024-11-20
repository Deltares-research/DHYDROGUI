using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    public static class ConditionProvider
    {
        public static IEnumerable<Type> GetAllConditions()
        {
            yield return typeof(StandardCondition);
            yield return typeof(TimeCondition);
            yield return typeof(DirectionalCondition);
        }

        public static string GetTitle(Type conditionType)
        {
            if (conditionType == typeof(StandardCondition))
            {
                return "Standard Condition";
            }

            if (conditionType == typeof(TimeCondition))
            {
                return "Time Condition";
            }

            if (conditionType == typeof(DirectionalCondition))
            {
                return "Standard Differential Condition";
            }

            if (conditionType.Name.Contains("Proxy")) //for tests... :-(
            {
                return "Proxy Rule";
            }

            throw new ArgumentException(@"Unsupported type", conditionType.FullName);
        }
    }
}