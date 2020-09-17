using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Helpers
{
    /// <summary>
    /// Utility class to work with networks
    /// </summary>
    [Obsolete("D3DFMIQ-2083 Remove obsolete 1D functionality")]
    public class HydroNetworkHelper
    {
        /// <summary>
        /// Sets the default name of a specific feature.
        /// </summary>
        /// <param name="region"> </param>
        /// <param name="feature"> </param>
        public static string GetUniqueFeatureName(IHydroRegion region, IFeature feature,
                                                  bool checkIfNewNameIsNeeded = false)
        {
            string featureName = feature.GetEntityType().Name;
            IHydroRegion fullRegion = region.Parent as IHydroRegion ?? region;
            IEnumerable<string> hydroObjectNames = fullRegion
                                                   .AllHydroObjects.Where(f => f.GetEntityType().Name == featureName)
                                                   .Select(f => f.Name);
            IEnumerable<string> allLinkNames =
                fullRegion.AllRegions.OfType<IHydroRegion>().SelectMany(r => r.Links).Select(l => l.Name);
            IEnumerable<string> allNames = hydroObjectNames.Concat(allLinkNames);
            var names = new HashSet<string>(allNames);

            if (checkIfNewNameIsNeeded)
            {
                PropertyInfo nameProperty = feature.GetType().GetProperty("Name");
                if (nameProperty != null)
                {
                    object currentName = nameProperty.GetValue(feature, null);

                    if (!string.IsNullOrWhiteSpace(currentName as string) && !names.Contains(currentName.ToString()))
                    {
                        return currentName.ToString();
                    }
                }
            }

            var i = 1;
            string uniqueName = featureName + i;
            while (names.Contains(uniqueName))
            {
                i++;
                uniqueName = featureName + i;
            }

            return uniqueName;
        }
    }
}
