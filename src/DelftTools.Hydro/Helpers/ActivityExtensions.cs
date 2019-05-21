using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow;

namespace DelftTools.Hydro.Helpers
{
    public static class ActivityExtensions
    {
        public static IEnumerable<T> GetActivitiesOfType<T>(this IActivity activity)
        {
            if (activity is ActivityWrapper)
            {
                activity = ((ActivityWrapper) activity).Activity;
            }

            if (activity is T)
            {
                yield return (T) activity;
            }

            var compActivity = activity as ICompositeActivity;
            if (compActivity == null)
            {
                yield break;
            }

            foreach (IActivity subActivity in compActivity.Activities)
            {
                foreach (T typedSubActivity in GetActivitiesOfType<T>(subActivity))
                {
                    yield return typedSubActivity;
                }
            }
        }
    }
}