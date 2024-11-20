using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow;

namespace DelftTools.Hydro.Helpers
{
    public static class ActivityExtensions
    {
        public static IEnumerable<T> GetActivitiesOfType<T>(this IActivity activity)
        {
            if (activity is ActivityWrapper activityWrapper)
            {
                activity = activityWrapper.Activity;
            }
            if (activity is T typeT)
            {
                yield return typeT;
            }

            if (!(activity is ICompositeActivity compActivity)) 
                yield break;

            foreach (var subActivity in compActivity.Activities)
            {
                foreach (var typedSubActivity in GetActivitiesOfType<T>(subActivity))
                {
                    yield return typedSubActivity;
                }
            }
        }
    }
}