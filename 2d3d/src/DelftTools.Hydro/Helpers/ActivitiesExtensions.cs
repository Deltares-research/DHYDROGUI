using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;

namespace DelftTools.Hydro.Helpers
{
    public static class ActivitiesExtensions
    {
        public static IEnumerable<T> GetActivitiesOfType<T>(this IEnumerable<IActivity> activities)
        {
            return activities.SelectMany(a => a.GetActivitiesOfType<T>());
        }
    }
}