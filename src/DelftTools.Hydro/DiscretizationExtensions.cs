using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.Hydro
{
    public static class DiscretizationExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DiscretizationExtensions));

        public static void AddNetworkDiscretizationCalculationLocationIfNotAlreadyCreated(this IDiscretization discretization, NetworkLocation toLocation)
        {
            var locations = new HashSet<Coordinate>(discretization.Locations.Values.Select(l => l.Geometry?.Coordinate));
            var locationGeometry = toLocation.Geometry;
            if (locationGeometry == null) Log.Warn($"No geometry set for {toLocation.Name}");
            if (!locations.Contains(locationGeometry?.Coordinate))
            {
                discretization.Locations.AddValues(new[] { toLocation });
            }
        }
    }
}