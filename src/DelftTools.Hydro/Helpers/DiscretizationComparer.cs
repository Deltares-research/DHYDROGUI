using System.Collections.Generic;
using GeoAPI.Extensions.Coverages;

namespace DelftTools.Hydro.Helpers
{
    public class DiscretizationComparer : IEqualityComparer<IDiscretization>
    {
        public bool Equals(IDiscretization primaryDiscretisation, IDiscretization secondaryDiscretisation)
        {
            if(primaryDiscretisation == null && secondaryDiscretisation != null 
               || primaryDiscretisation != null && secondaryDiscretisation == null) return false;

            if(!Equals(primaryDiscretisation?.Name, secondaryDiscretisation?.Name))return false;
            if(!Equals(primaryDiscretisation?.Locations.Values.Count, secondaryDiscretisation?.Locations.Values.Count)) return false;
            var comperator = new HydroNetworkComparer();
            if (primaryDiscretisation?.Network is IHydroNetwork primaryDiscretisationNetwork &&
                secondaryDiscretisation?.Network is IHydroNetwork secondaryDiscretisationNetwork)
            {
                if(!comperator.Equals(primaryDiscretisationNetwork, secondaryDiscretisationNetwork)) return false;
            }
            else
            {
                //need to think...for a network comparer instead of a hydronetwork comparer
            }

            for (int i = 0; i < primaryDiscretisation?.Locations.Values.Count; i++)
            {
                var primaryLocation = primaryDiscretisation.Locations.Values[i];
                var secondaryLocation = secondaryDiscretisation.Locations.Values[i];

                if(!Equals(primaryLocation.Chainage, secondaryLocation.Chainage))return false;
                if(!Equals(primaryLocation.Name, secondaryLocation.Name)) return false;
                if(!Equals(primaryLocation.Description, secondaryLocation.Description)) return false;
            }

            return true;
        }

        public int GetHashCode(IDiscretization obj)
        {
            return obj.GetType().GetHashCode();
        }
    }
}