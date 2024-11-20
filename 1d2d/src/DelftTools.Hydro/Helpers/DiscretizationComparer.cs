using System.Collections.Generic;
using GeoAPI.Extensions.Coverages;

namespace DelftTools.Hydro.Helpers
{
    public class DiscretizationComparer : IEqualityComparer<IDiscretization>
    {
        public bool Equals(IDiscretization x, IDiscretization y)
        {
            if(x == null && y != null 
               || x != null && y == null) return false;

            if(!Equals(x?.Name, y?.Name) ||
             (!Equals(x?.Locations.Values.Count, y?.Locations.Values.Count)))
                return false;
            
            var comparer = new HydroNetworkComparer();

            if (x?.Network is IHydroNetwork primaryDiscretisationNetwork &&
                y?.Network is IHydroNetwork secondaryDiscretisationNetwork && 
                !comparer.Equals(primaryDiscretisationNetwork, secondaryDiscretisationNetwork))
                return false;

            for (int i = 0; i < x?.Locations.Values.Count; i++)
            {
                var primaryLocation = x.Locations.Values[i];
                var secondaryLocation = y.Locations.Values[i];

                if(!Equals(primaryLocation.Chainage, secondaryLocation.Chainage) ||
                   !Equals(primaryLocation.Name, secondaryLocation.Name) ||
                   !Equals(primaryLocation.Description, secondaryLocation.Description)) 
                    return false;
            }

            return true;
        }

        public int GetHashCode(IDiscretization obj)
        {
            return obj.GetType().GetHashCode();
        }
    }
}