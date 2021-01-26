using System;
using System.Globalization;
using GeoAPI.Extensions.Coverages;

namespace DelftTools.ModelExchange
{
    public class StaggeredGridPointHelper
    {
        public const char GridPointDelimeter = '-';  // TODO: AVOID SPLITTING ON THIS CHARACTER

        public static string GetLocationId(INetworkLocation location)
        {
            return string.Concat(location.Branch.Name, GridPointDelimeter,
                                                                     location.Chainage.ToString(
                                                                         CultureInfo.InvariantCulture));
        }

        public static Utils.Tuple<string,double> ParseLocation(string locationId)
        {
            int indexOfDash = locationId.LastIndexOf(GridPointDelimeter);
            if (indexOfDash > -1)
            {
                string branchName = locationId.Substring(0, indexOfDash);

                double offset;
                if (!double.TryParse(locationId.Substring(indexOfDash+1), NumberStyles.Float, CultureInfo.InvariantCulture, out offset))
                {
                    offset = double.NaN;
                }

                return new Utils.Tuple<string, double>(branchName, offset);
            }

            throw new InvalidOperationException("There was an error parsing the location id");

        }
    }
}