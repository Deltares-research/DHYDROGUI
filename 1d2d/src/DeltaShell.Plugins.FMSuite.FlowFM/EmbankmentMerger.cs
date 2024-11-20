using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public static class EmbankmentMerger
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (EmbankmentMerger));

        public static Embankment MergeSelectedEmbankments(IList<Embankment> embankmentDefinitions, Embankment embankment1,
            Embankment embankment2)
        {

            var intersections = embankment1.Geometry.Intersection(embankment2.Geometry);
            Embankment mergedEmbankment;
            if (intersections.NumPoints == 1)
            {
                mergedEmbankment = MergeSelectedEmbankmentsWithOneIntersection(embankmentDefinitions, embankment1,
                    embankment2);
            }
            else if (intersections.NumPoints == 0)
            {
                mergedEmbankment = MergeSelectedEmbankmentsWithNoIntersection(embankmentDefinitions, embankment1, embankment2);
            }
            else
            {
                Log.Warn("Embankments with more than one intersection cannot be merged. Merge cancelled.");
                return null; 
            }

                         
            return mergedEmbankment;
        }

        private static Embankment MergeSelectedEmbankmentsWithNoIntersection(IList<Embankment> embankmentDefinitions, Embankment embankment1,
            Embankment embankment2)
        {

            // Step 1: Make sure that order of coordinates in the embankments' geometries are such, that only
            // ascending for loops are necessary. That is: the _end_ of Embankment1 will be connected to the _beginning_ of Embankment2.
            var embankment1FirstCoord = new Point(embankment1.Geometry.Coordinates.First());
            var embankment1LastCoord = new Point(embankment1.Geometry.Coordinates.Last());
            var embankment2FirstCoord = new Point(embankment2.Geometry.Coordinates.First());
            var embankment2LastCoord = new Point(embankment2.Geometry.Coordinates.Last());

            var distances = new List<double>
            {
                embankment1FirstCoord.Distance(embankment2FirstCoord),
                embankment1FirstCoord.Distance(embankment2LastCoord),
                embankment1LastCoord.Distance(embankment2FirstCoord),
                embankment1LastCoord.Distance(embankment2LastCoord)
            };

            var indexMinDistance = distances.IndexOf(distances.Min());
            IEnumerable<Coordinate> embankment1Coordinates = (indexMinDistance == 0 || indexMinDistance == 1)
                ? embankment1.Geometry.Coordinates.Reverse()
                : embankment1.Geometry.Coordinates;

            IEnumerable<Coordinate> embankment2Coordinates = (indexMinDistance == 1 || indexMinDistance == 3)
                ? embankment2.Geometry.Coordinates.Reverse()
                : embankment2.Geometry.Coordinates;

            // Step 2: merge embankments. 
            var mergedEmbankment = new Embankment
            {
                Name = NetworkHelper.GetUniqueName("MergedEmbankment{0:D2}", embankmentDefinitions, "Embankment"), 
                Geometry = new LineString(embankment1Coordinates.Concat(embankment2Coordinates).ToArray())
            };

            return mergedEmbankment; 
        }

        private static Embankment MergeSelectedEmbankmentsWithOneIntersection(IList<Embankment> embankmentDefinitions, Embankment embankment1,
            Embankment embankment2)
        {

            // Step 1: Make sure that order of coordinates in the embankments' geometries are such, that only
            // ascending for loops are necessary. That is: the _end_ of Embankment1 will be connected to the _beginning_ of Embankment2.
            var embankment1FirstCoord = embankment1.Geometry.Coordinates.First();
            var embankment1LastCoord = embankment1.Geometry.Coordinates.Last();
            var embankment2FirstCoord = embankment2.Geometry.Coordinates.First();
            var embankment2LastCoord = embankment2.Geometry.Coordinates.Last();

            var intersections = embankment1.Geometry.Intersection(embankment2.Geometry);
            var intersectionCoordinate = intersections.GetGeometryN(0).Coordinate;
            Coordinate[] embankment1Coordinates = (intersectionCoordinate.Distance(embankment1FirstCoord) < intersectionCoordinate.Distance(embankment1LastCoord))
                ? embankment1.Geometry.Coordinates.Reverse().ToArray()
                : embankment1.Geometry.Coordinates.ToArray();

            Coordinate[] embankment2Coordinates = (intersectionCoordinate.Distance(embankment2FirstCoord) > intersectionCoordinate.Distance(embankment2LastCoord))
                ? embankment2.Geometry.Coordinates.Reverse().ToArray()
                : embankment2.Geometry.Coordinates.ToArray();

            // Step 2: merge embankments. 
            var mergedEmbankment = new Embankment
            {
                Name = NetworkHelper.GetUniqueName("MergedEmbankment{0:D2}", embankmentDefinitions, "Embankment")
            };
            var pointList = new List<Coordinate>();

            // One intersection: connect with Intersection Point
            // Step 2a: Add points from embankment1 start until intersection.
            // Step 2b: Add the intersection point. 
            // Step 2c: Add points from intersection until embankment2 end. 
            intersectionCoordinate.Z = 0.0;

            // Step 2a
            pointList.Add(embankment1Coordinates[0]);
            for (var i = 1; i < embankment1Coordinates.Length; i++)
            {
                // Funny way to test for intersection, but a LineString never intersects with a Point, apparently. 
                if (Math.Abs(embankment1Coordinates[i - 1].Distance(embankment1Coordinates[i]) -
                             intersectionCoordinate.Distance(embankment1Coordinates[i - 1]) -
                             intersectionCoordinate.Distance(embankment1Coordinates[i])) < 1.0)
                {
                    // Reached the intersection, so stop adding points to pointList. 
                    break;
                }
                pointList.Add(embankment1Coordinates[i]);
            }

            // Step 2b
            pointList.Add(intersectionCoordinate);

            // Step 2c
            for (var i = 0; i < embankment2Coordinates.Length - 1; i++)
            {
                if (Math.Abs(embankment2Coordinates[i + 1].Distance(embankment2Coordinates[i]) -
                             intersectionCoordinate.Distance(embankment2Coordinates[i + 1]) -
                             intersectionCoordinate.Distance(embankment2Coordinates[i])) < 1.0)
                {
                    // coordinate i should not be included (wrong side of intersection); coordinate i+1 should. 
                    pointList.AddRange(embankment2Coordinates.Skip(i + 1));
                    break;
                }
            }

            mergedEmbankment.Geometry = new LineString(pointList.ToArray());
            return mergedEmbankment;
        }

    }
}