using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.ModelExchange.Queries;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.Fews.Assemblers
{
    public static class NodeFeatureCollectionAssembler
    {
        public static IList<DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>>> Assemble(IEnumerable<AggregationResult> context, bool exportStaggeredGridPoints = false)
        {
            // holds lists of elements already added to the shapefile feature collection
            var featuresAdded = new HashSet<string>();

            // the feature collections to write to the shapefiles
            var nodeFeatureCollection = new List<DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>>>();

            foreach (var queryResult in context)
            {
                // Check for filters in results (shape file)
                // for example filter out staggered points...
                if (!exportStaggeredGridPoints)
                {
                    if (queryResult.LocationType == FunctionAttributes.StandardFeatureNames.ReachSegment)
                        continue;
                }

                if (queryResult.Geometry == null)
                    continue;

                string locationId = queryResult.LocationId;
                string locationType = string.IsNullOrEmpty(queryResult.LocationType) ? "unknown" : queryResult.LocationType;

                if (string.IsNullOrEmpty(locationId) || locationId.Trim() == string.Empty)
                    continue;

                if (queryResult.Geometry.GeometryType == "Point")
                {
                    if (featuresAdded.Contains(locationId))
                    {
                        // Id already added. It is not allowed to add more features with 
                        // the same ID, but QBoundaries and HBoundaries have precedence
                        if (locationType == FunctionAttributes.QBoundary || locationType == FunctionAttributes.HBoundary)
                        {
                            var tuple =
                                nodeFeatureCollection.FirstOrDefault(t => t.Second["ID"].ToString() == locationId);
                            if (tuple != null)
                            {
                                tuple.Second["TYPE"] = locationType;
                                tuple.Second["NAME"] = queryResult.Name ?? "";
                            }
                        }
                        continue;
                    }

                    featuresAdded.Add(locationId);

                    var attributes = new Dictionary<string, object>
                                    {
                                        {"ID", locationId}, 
                                        {"NAME", queryResult.Name ?? string.Empty }, 
                                        {"TYPE", locationType},
                                        {"X", queryResult.X}, 
                                        {"Y", queryResult.Y}, 
                                        {"Z", double.IsNaN(queryResult.Z) ? 0.0 : queryResult.Z }
                                    };

                    var feature = new DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>>(queryResult.Geometry, attributes);
                    nodeFeatureCollection.Add(feature);
                }
            }
            return nodeFeatureCollection;
        }
    }
}