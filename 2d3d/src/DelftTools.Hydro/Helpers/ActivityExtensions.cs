using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow;

namespace DelftTools.Hydro.Helpers
{
    public static class ActivityExtensions
    {
        public static IEnumerable<T> GetActivitiesOfType<T>(this IActivity activity)
        {
            if (activity is ActivityWrapper wrapper)
            {
                activity = wrapper.Activity;
            }

            if (activity is T castedActivity)
            {
                yield return castedActivity;
            }

            if (!(activity is ICompositeActivity compActivity))
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