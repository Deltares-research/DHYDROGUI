using System;
using System.Collections.Generic;
using DelftTools.Functions;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.ImportExport.Sobek.HisData
{
    /// <summary>
    /// In his-file only location names are available
    /// NetworkLocationTypeConvertor converts a location name into a networklocation
    /// </summary>
    public class NetworkLocationTypeConvertor : TypeConverterBase<INetworkLocation>
    {
        private Dictionary<string, INetworkLocation> fromStore = new Dictionary<string, INetworkLocation>();
        private Dictionary<INetworkLocation,string> toStore = new Dictionary<INetworkLocation,string>();

        public override Type[] StoreTypes
        {
            get { return new[]{typeof(string)}; }
        }

        public override string[] VariableNames
        {
            get { return new[] {"location"}; }
        }

        public override string[] VariableStandardNames
        {
            get { return new[] { "" }; }
        }

        public override string[] VariableUnits
        {
            get { return new[] { "" }; }
        }

        public void AddItem(string locationName, INetworkLocation networkLocation) 
        {
            //his file has only 20 char for a name, NETWORK.GR much more
            //trunk name to 20 char for compare NETWORK.GR name with his file name
            var pos = (locationName.Length > 20) ? 20 : locationName.Length;
            var locationName20 = locationName.Substring(0, pos).Trim();

            if (!fromStore.ContainsKey(locationName20))
            {
                fromStore.Add(locationName20, networkLocation);
            }
            toStore.Add(networkLocation,locationName20);
        }

        public override INetworkLocation ConvertFromStore(object source)
        {
            var key = source.ToString();
            if(fromStore.ContainsKey(key))
            {
                return fromStore[key];
            }

            throw new IndexOutOfRangeException(String.Format("Feature name '{0}' has not been found.",key));
        }

        public override object[] ConvertToStore(INetworkLocation source)
        {
            var key = source;
            if (toStore.ContainsKey(key))
            {
                return new[]{toStore[key]};
            }

            throw new IndexOutOfRangeException(String.Format("Feature '{0}-{1}' has not been found.", key.Branch.Id,key.Chainage));
        }

        public List<string> LocationMames()
        {
           return new List<string>(fromStore.Keys);
        }

        public List<INetworkLocation> NetworkLocations()
        {
            return new List<INetworkLocation>(fromStore.Values);
        }
    }
}
