using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools
{
    internal enum Flavour
    {
        UserPolygon,
        Embankment,
        Branch
    }

    internal class FlavouredCoordinate
    {
        public FlavouredCoordinate(Coordinate coordinate, Flavour flavour)
        {
            Coordinate = coordinate;
            Flavour = flavour;
        }

        public Coordinate Coordinate { get; private set; }

        public Flavour Flavour { get; private set; }
    }

    internal class CoordinateDistanceComparer : IComparer<FlavouredCoordinate>
    {
        private readonly Coordinate startCoordinate;
        
        public CoordinateDistanceComparer(Coordinate startCoordinate)
        {
            this.startCoordinate = startCoordinate;
        }

        public int Compare(FlavouredCoordinate x, FlavouredCoordinate y)
        {
            return startCoordinate.Distance(x.Coordinate) < startCoordinate.Distance(y.Coordinate) ? -1 : 1; 
        }
    }

    public class GridWizardMapToolHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (GridWizardMapToolHelper));

        public static IList<IPolygon> ComputePolygons(IDiscretization discretization, IList<Feature2D> embankments,
            IPolygon userPolygon, double supportPointDistance, double minimumSupportPointDistance)
        {
            // Protect against a folded userPolygon.
            if (!(userPolygon.IsValid))
            {
                Log.Error("Invalid or folded user polygon.");
                return null;
            }

            // Check for the presence of embankments.
            var embankmentLineStrings = embankments.Select(embankment => new LineString(embankment.Geometry.Coordinates)).ToList();
            if (!embankmentLineStrings.Any())
            {
                Log.Error("No embankments cross the user polygon.");
                return null;
            }

            // Demand that each embankment intersects the userPolygon an even number of times.
            var userLineString = userPolygon.ExteriorRing;
            var oddEmbankments = embankments.Where(e => e.Geometry.Intersection(userLineString).NumPoints % 2 != 0).ToList();
            if (oddEmbankments.Any())
            {
                // Show the names of 5 problematic embankments. 
                Log.ErrorFormat("The following {0} embankments crosses the user polygon an odd number of times (max 5): {1}", oddEmbankments.Count, String.Join(", ", oddEmbankments.Take(5).Select(e => e.Name)));
                return null;
            }

            // Identify the branches intersecting the userPolygon.
            var branches = discretization.Network.Branches
                .Where(branch => branch.Geometry.Intersects(userPolygon))
                .ToList();

            if (!branches.Any())
            {
                Log.Error("No branches cross the user polygon.");
                return null;
            }

            // Construct velocity LineStrings by cutting the branches with the userPolygon.
            var velocityLineStrings = GetVelocityLineStrings(discretization, branches, userPolygon);
            if (!velocityLineStrings.Any())
            {
                Log.Error("No branch velocity points inside the user polygon.");
                return null;
            }

            // Construct embankment LineStrings by cutting the embankments with the userPolygon.
            var cutEmbanksmentLineStrings = GetEmbankmentLineStrings(embankmentLineStrings, userPolygon).ToList();
            
            // Construct LineStrings from velocityLineStrings projected on embankmentLineStrings.
            //    Identify the embankmentLineString - velocityLineStrings associations (1->N).
            var associations = GetAssociations(cutEmbanksmentLineStrings, velocityLineStrings);
            var projectedLineStrings = ProjectLineStrings(associations, minimumSupportPointDistance, supportPointDistance).ToList();
            if (!(projectedLineStrings.Any()))
            {
                Log.Error("Less than two Locations in the Spatial data of a Branch are inside the user polygon.");
                return null;
            }
            
            // Densify unassociated embankmentLineStrings.
            var densifiedEmbankmentLineStrings = DensifiedEmbankmentLineStrings(supportPointDistance, minimumSupportPointDistance, cutEmbanksmentLineStrings, associations);

            // Construct boundary LineStrings structured as: Embankment intersection, userPolygon Coordinates, Embankment intersection.
            var branchGeometries = branches.Select(branch => branch.Geometry).ToList();
            var boundaryLineStrings = MakeBoundaryLineStrings(embankmentLineStrings, userPolygon, branchGeometries);
            if (boundaryLineStrings == null) return null;
            var denseBoundaryLineStrings = boundaryLineStrings.Select(c => Densify(c, supportPointDistance)).ToList();

            // Cluster all LineStrings.
            var toMergeLists = ClusterLineStrings(projectedLineStrings, densifiedEmbankmentLineStrings, denseBoundaryLineStrings);
            if (toMergeLists == null) return null;

            // Merge the clustered LineStrings.
            var mergedLineStrings = MergeLineStrings(toMergeLists);

            // Construct polygons from merged LineStrings.
            var polygons = MakePolygons(mergedLineStrings);

            return polygons;
        }

        private static IEnumerable<LineString> DensifiedEmbankmentLineStrings(double supportPointDistance, double minimumSupportPointDistance,
            IEnumerable<ILineString> embankmentLineStrings, IDictionary<ILineString, IList<ILineString>> associations)
        {
            var unassociatedEmbankmentLineStrings = embankmentLineStrings.Where(embankment => !associations.ContainsKey(embankment)).ToArray();
            var filteredUnassociated = new List<ILineString>();
            foreach (var unassociatedEmbankmentLineString in unassociatedEmbankmentLineStrings)
            {
                var coordinates = unassociatedEmbankmentLineString.Coordinates;
                var newCoordinates = new List<Coordinate> {coordinates[0]};
                var previous = newCoordinates[0];
                for (var i = 1; i < coordinates.Count() - 1; i++)
                {
                    var current = coordinates[i];
                    if (!(previous.Distance(current) >= minimumSupportPointDistance)) continue;
                    newCoordinates.Add(current);
                    previous = current;
                }
                newCoordinates.Add(coordinates[coordinates.Count() - 1]);
                filteredUnassociated.Add(new LineString(newCoordinates.ToArray()));
            }
            var densifiedLineStrings = filteredUnassociated.Select(uaEmbankment => Densify(uaEmbankment, supportPointDistance)).ToList();
            var numberOfUnassociatedEmbankmentLineStrings = densifiedLineStrings.Count;
            if (numberOfUnassociatedEmbankmentLineStrings > 0)
            {
                Log.Warn("Found " + numberOfUnassociatedEmbankmentLineStrings + " embankment(s) not belonging to any branch.");
            }
            return densifiedLineStrings;
        }

        private static IEnumerable<List<ILineString>> ClusterLineStrings(IList<ILineString> projectedLineStrings, IEnumerable<ILineString> densifiedLineStrings,
            IEnumerable<ILineString> denseBoundaryLineStrings)
        {
            // Self-intersecting LineStrings lead to folded polygons.
            if (projectedLineStrings.Any(l => !(l.IsSimple)))
            {
                Log.Warn("Found a self-intersecting embankment.");
                return null;
            }

            var allLineStrings = new List<ILineString>();
            allLineStrings.AddRange(projectedLineStrings);
            allLineStrings.AddRange(densifiedLineStrings);
            allLineStrings.AddRange(denseBoundaryLineStrings);
            foreach (var aLineString in allLineStrings)
            {
                foreach (var aCoordinate in aLineString.Coordinates)
                {
                    aCoordinate.Z = 0.0;
                }
            }
            var toMergeLists = new List<List<ILineString>>();
            while (allLineStrings.Count > 1)
            {
                var newList = new List<ILineString>();
                var seed = allLineStrings[0];
                allLineStrings.RemoveAt(0);
                newList.Add(seed);
                var busy = true;
                while (busy)
                {
                    if (allLineStrings.Count < 1)
                    {
                        break;
                    }
                    busy = false;
                    for (var i = 0; i < allLineStrings.Count; i++)
                    {
                        if (seed.Distance(allLineStrings[i]) < 10.0)
                        {
                            seed = allLineStrings[i];
                            allLineStrings.RemoveAt(i);
                            newList.Add(seed);
                            busy = true;
                            break;
                        }
                    }
                }
                toMergeLists.Add(newList);
            }
            // check remainder
            if (allLineStrings.Any())
            {
                toMergeLists.Add(allLineStrings);
            }
            return toMergeLists;
        }

        private static IEnumerable<ILineString> MergeLineStrings(IEnumerable<List<ILineString>> toMergeLists)
        {
            const double distanceEpsilon = 1.0;
            var mergedLineStrings = new List<ILineString>();
            
            foreach (var mergeList in toMergeLists)
            {
                while (mergeList.Count > 1)
                {
                    var seed = mergeList[0];
                    mergeList.RemoveAt(0);
                    for (var i = 0; i < mergeList.Count; i++)
                    {
                        var firstcoordinateMergeLine = mergeList[i].Coordinates[0];
                        var lastcoordinateMergeLine = mergeList[i].Coordinates[mergeList[i].Coordinates.Count() - 1];

                        // Try growing from the end of the seed.
                        var lastSeedCoordinate = seed.Coordinates[seed.Coordinates.Count() - 1];
                        if (lastSeedCoordinate.Distance(firstcoordinateMergeLine) <= distanceEpsilon)
                        {
                            mergeList.Add(new LineString(seed.Coordinates.Concat(mergeList[i].Coordinates.Skip(1)).ToArray()));
                            mergeList.RemoveAt(i);
                            break;
                        }
                        if (lastSeedCoordinate.Distance(lastcoordinateMergeLine) <= distanceEpsilon)
                        {
                            mergeList.Add(new LineString(seed.Coordinates.Concat(mergeList[i].Coordinates.Reverse().Skip(1)).ToArray()));
                            mergeList.RemoveAt(i);
                            break;
                        }

                        // Try growing from the other end of the seed by reversing it.
                        seed = new LineString(seed.Coordinates.Reverse().ToArray());

                        lastSeedCoordinate = seed.Coordinates[seed.Coordinates.Count() - 1];
                        if (lastSeedCoordinate.Distance(firstcoordinateMergeLine) <= distanceEpsilon)
                        {
                            mergeList.Add(new LineString(seed.Coordinates.Concat(mergeList[i].Coordinates.Skip(1)).ToArray()));
                            mergeList.RemoveAt(i);
                            break;
                        }
                        if (lastSeedCoordinate.Distance(lastcoordinateMergeLine) <= distanceEpsilon)
                        {
                            mergeList.Add(new LineString(seed.Coordinates.Concat(mergeList[i].Coordinates.Reverse().Skip(1)).ToArray()));
                            mergeList.RemoveAt(i);
                            break;
                        }
                    }
                }
                mergedLineStrings.Add(mergeList[0]);
            }
            return mergedLineStrings;
        }

        private static IList<IPolygon> MakePolygons(IEnumerable<ILineString> mergedLineStrings)
        {
            var coordinateLists = mergedLineStrings.Select(lineString => new CoordinateList(lineString.Coordinates.ToArray())).ToArray();
            IList<IPolygon> polygons = new List<IPolygon>();

            foreach (var coordinateList in coordinateLists)
            {
                // Determine closedness in 2D, not 3D.
                if (coordinateList[0].Distance(coordinateList[coordinateList.Count - 1]) <= 10.0)
                {
                    coordinateList.RemoveAt(0);
                }
                coordinateList.CloseRing();
                if (coordinateList.Count < 4) continue;
                polygons.Add(new Polygon(new LinearRing(coordinateList.ToCoordinateArray())));
            }

            // Protect against a folded polygons.
            if (polygons.Any(polygon => !(polygon.IsValid)))
            {
                Log.Warn("Generated invalid or folded polygon, aborting.");
                return null;
            }

            // Test for overlap, indicating misconstructed polygons.
            if (polygons.Count > 1 && polygons.Any(p => polygons.Any(p2 => p != p2 && p.Intersects(p2))))
            {
                Log.Warn("Constructed overlapping polygons, aborting.");
                return null;                
            }

            return polygons;
        }

        private static IEnumerable<ILineString> ProjectLineStrings(Dictionary<ILineString, IList<ILineString>> associations, double minimumDistance, double optimalDistance)
        {
            foreach (var kvp in associations)
            {
                var lengthIndexedLine = new LengthIndexedLine(kvp.Key);

                // Keep the start and end chainages, so that either the projectedLineString's endpoints lie on the userPolygon.
                var chainages = new List<double> { lengthIndexedLine.StartIndex, lengthIndexedLine.EndIndex };

                // Project the velocity points of each velocityLineString onto the embankmentLineString and determine the maximumProjectedLength for use in densification.
                var maximumProjectedLength = Double.NegativeInfinity;
                foreach (var vl in kvp.Value)
                {
                    var chainagesSection = vl.Coordinates.Select(lengthIndexedLine.Project).OrderBy(item => item);
                    chainages.AddRange(chainagesSection);
                    var localMaximum = chainagesSection.Skip(1).Zip(chainagesSection, (second, first) => Math.Abs(second - first)).Max();
                    maximumProjectedLength = Math.Max(maximumProjectedLength, localMaximum);
                }

                // Remove chainages when too close together, preserving start and end chainages.
                var orderedChainages = chainages.OrderBy(chainage => chainage).ToArray();
                var newChainages = new List<double> { orderedChainages[0], orderedChainages[1] };
                var previous = newChainages[0];
                for (var i = 2; i < orderedChainages.Count() - 2; i++)
                {
                    var current = orderedChainages[i];
                    if (!(Math.Abs(previous - current) >= minimumDistance)) continue;
                    newChainages.Add(current);
                    previous = current;
                }
                newChainages.Add(orderedChainages[orderedChainages.Count() - 2]);
                newChainages.Add(orderedChainages[orderedChainages.Count() - 1]);

                // Convert chainages to coordinates, accounting for multiple velocity points being projected on the endpoints of the embankmentLineString.
                var sparseLineString = new LineString(newChainages.Distinct().Select(c => lengthIndexedLine.ExtractPoint(c)).ToArray());

                // Densify chainages where a part of the embankments is not associated with a branch.
                var denseLineString = Densify(sparseLineString, optimalDistance, maximumProjectedLength);

                // Construct the projectedLineString.
                yield return denseLineString;
            }
        }

        private static Dictionary<ILineString, IList<ILineString>> GetAssociations(IEnumerable<ILineString> embankmentLineStrings, IList<ILineString> velocityLineStrings)
        {
            var associations = new Dictionary<ILineString, IList<ILineString>>();
            foreach (var embankmentLineString in embankmentLineStrings)
            {
                // Store the velocityLineStrings indices closest to each embankmentLineString segment.
                var segmentAssociations = new List<int>();

                // Determine the velocityLineStrings indices.
                var locationIndexedLine = new LocationIndexedLine(embankmentLineString);
                for (var index = locationIndexedLine.StartIndex.SegmentIndex;
                    index <= locationIndexedLine.EndIndex.SegmentIndex; index++)
                {
                    var midPoint = new Point(locationIndexedLine.ExtractPoint(new LinearLocation(index, 0.5)));
                    var closest = Double.PositiveInfinity;
                    var velocityIndex = -1;
                    for (var j = 0; j < velocityLineStrings.Count; j++)
                    {
                        var distance = velocityLineStrings[j].Distance(midPoint);
                        // Cornercase: A Embankment whose Branch(es) lie outside the userPolygon will be associated to the nearest Branch inside the userPolygon.
                        // Approximate solution: demand distance < 500 m;
                        if (!(distance < closest)) continue; 
                        
                        velocityIndex = j;
                        closest = distance;
                    }

                    if (velocityIndex > -1)
                    {
                        segmentAssociations.Add(velocityIndex);                        
                    }
                }

                // Associate a velocityLineString to a embankmentLineString if it is associated to three or more segments.
                //Using 2 in stead of: Math.Min(velocityLineStrings.Count, locationIndexedLine.EndIndex.SegmentIndex - locationIndexedLine.StartIndex.SegmentIndex + 1);
                var associatedEmbankments = Enumerable.Range(0, velocityLineStrings.Count).Where(index => segmentAssociations.Count(sa => sa == index) >= 2).Select(i => velocityLineStrings[i]).ToList();
                if (associatedEmbankments.Any())
                {
                    associations.Add(embankmentLineString, associatedEmbankments);
                }
            }
            return associations;
        }

        private static IEnumerable<ILineString> GetEmbankmentLineStrings(IEnumerable<LineString> embankments, IPolygon userPolygon)        {
            var userLineString = userPolygon.ExteriorRing;
            foreach (var embankment in embankments)            {
                // Find all intersection coordinates.
                var lineString = new LineString(embankment.Coordinates);
                var intersectionCoordinates = lineString.Intersection(userLineString).Coordinates.ToList();

                // Handle embankments which lie inside the userPolygon.
                if (intersectionCoordinates.Count == 0)
                {
                    yield return embankment;
                }

                // Add the start and end points of a bank when those are inside.
                if (new Point(lineString.Coordinates.First()).Within(userPolygon))
                {
                    intersectionCoordinates.Add(lineString.Coordinates.First());
                }
                if (new Point(lineString.Coordinates.Last()).Within(userPolygon))
                {
                    intersectionCoordinates.Add(lineString.Coordinates.Last());
                }

                // Find the sorted intersection chainages.
                var lengthIndexedLine = new LengthIndexedLine(lineString);
                var intersectionChainages = intersectionCoordinates.Select(lengthIndexedLine.IndexOf).OrderBy(c => c).ToArray();

                // Construct the embankment LineStrings.
                for (var i = 0; i < intersectionChainages.Count(); i += 2)
                {
                    var newLine = (LineString)lengthIndexedLine.ExtractLine(intersectionChainages[i], intersectionChainages[i+1]);
                    yield return newLine;
                }
            }
        }

        private static List<ILineString> GetVelocityLineStrings(INetworkCoverage discretization, IEnumerable<IBranch> branches, IPolygon userPolygon)
        {
            var userLineString = userPolygon.ExteriorRing;
            var velocityLineStrings = new List<ILineString>();

            foreach (var branch in branches)
            {
                // Find all intersection coordinates.
                var lineString = new LineString(branch.Geometry.Coordinates);
                var intersectionCoordinates = lineString.Intersection(userLineString).Coordinates.ToList();

                var firstCoordinate = lineString.Coordinates.First();
                var lastCoordinate = lineString.Coordinates.Last();

                // Supplement intersections when a branch ends inside the userPolygon.
                if (intersectionCoordinates.Count%2 == 1)
                {
                    intersectionCoordinates.Add(new Coordinate(
                        new Point(firstCoordinate).Within(userPolygon)
                            ? firstCoordinate
                            : lastCoordinate));
                }

                // Handle branches fully inside the userPolygon.
                if (intersectionCoordinates.Count == 0)
                {
                    intersectionCoordinates.Add(new Coordinate(firstCoordinate));
                    intersectionCoordinates.Add(new Coordinate(lastCoordinate));
                }

                // Find the sorted branch chainages, corresponding to the intersection coordinates.
                var lengthIndexedLine = new LengthIndexedLine(lineString);
                var intersectionChainages = intersectionCoordinates.Select(lengthIndexedLine.IndexOf).OrderBy(c => c) .ToArray();

                // Find the sorted chainages of the velocity points.
                var locations = discretization.GetLocationsForBranch(branch);
                var velocityChainages = locations.Skip(1).Zip(locations, (second, first) => 0.5*(first.Chainage + second.Chainage)).OrderBy(c => c).ToArray();

                // Group the velocity chainages by the intersection coordinates.
                var chainageGroups = intersectionChainages.Skip(1)
                        .Zip(intersectionChainages, (second, first) =>
                                velocityChainages.Where(chainage => first <= chainage && chainage <= second).ToArray())
                        .Where(cg => cg.Count() > 1).ToArray();

                // Translate velocity chainages into coordinates.
                var coordinateGroups = chainageGroups.Select(group => @group.Select(g => lengthIndexedLine.ExtractPoint(g)).ToArray());

                // Construct the velocity LineStrings.
                velocityLineStrings.AddRange(coordinateGroups.Select(group => new LineString(@group)));
            }
            return velocityLineStrings;
        }

        private static IEnumerable<LineString> MakeBoundaryLineStrings(List<LineString> embankments, IGeometry userPolygon, IList<IGeometry> branchGeometries)
        {
            // Construct typed Coordinates using userPolygon Coordinates and the Embankment and Branch intersections.
            var typedCoordinates = new List<FlavouredCoordinate>();
            var userCoordinates = userPolygon.Coordinates;

            for (var i = 0; i <= userCoordinates.Count() - 2; i++)
            {
                var segment = new LineString(new[] {userCoordinates[i], userCoordinates[i + 1]});

                var embankmentIntersections = embankments.SelectMany(b => b.Intersection(segment).Coordinates.Select(c => new FlavouredCoordinate(c, Flavour.Embankment)));
                var branchIntersections = branchGeometries.SelectMany(bg => bg.Intersection(segment).Coordinates.Select(c => new FlavouredCoordinate(c, Flavour.Branch)));
                var intersections = embankmentIntersections.Concat(branchIntersections).ToList();

                // Sort the intersections based on the distance from userCoordinates[i].
                var comparer = new CoordinateDistanceComparer(userCoordinates[i]);
                intersections.Sort(comparer);

                // Add current userLineString Coordinate and subsequent crossings.
                typedCoordinates.Add(new FlavouredCoordinate(userCoordinates[i], Flavour.UserPolygon));
                typedCoordinates.AddRange(intersections);
            }
            // No need to add the last userPolygon Coordinate.

            // Sanity check.
            if (typedCoordinates.Count(coord => coord.Flavour == Flavour.Branch) < 2)
            {
                Log.Warn("Fewer than two crossings between branches and the user polygon found.");
                return null; 
            }

            // Shift the typedCoordinates untill they start with a Branch.
            var lastIndex = typedCoordinates.Count - 1;
            while (typedCoordinates[0].Flavour != Flavour.Branch)
            {
                typedCoordinates.Move(0, lastIndex);
            }

            // Copy the starting Branch to the end of the typedCoordinates.
            typedCoordinates.Add(typedCoordinates[0]);

            // Identify Embankment * Embankment sections not containing Branch. (* ==  wildcard)
            var typedCoordinateSections = new List<List<int>>();
            var startIndex = -1;
            var sectionsAfterBranch = 0;
            for (var i = 0; i < typedCoordinates.Count; i++)
            {
                // Reset search upon encountering a Branch.
                if (typedCoordinates[i].Flavour == Flavour.Branch)
                {
                    startIndex = -1;
                    sectionsAfterBranch = 0;
                }
                // Start or finish segment upon encountering a Embankment.
                if (typedCoordinates[i].Flavour != Flavour.Embankment) continue;
                if (startIndex > -1)
                {
                    sectionsAfterBranch += 1;
                    // Even number of sections after a Branch are superceded by the densified unassociated embankmentLineStrings.
                    if (sectionsAfterBranch % 2 == 1)
                    {
                        typedCoordinateSections.Add(Enumerable.Range(startIndex, i - startIndex + 1).ToList());
                    }
                }
                startIndex = i;
            }
            
            // Construct the resulting LineStrings.
            var coordinateSections = typedCoordinateSections.Select(section => section.Select(c => typedCoordinates[c].Coordinate).ToArray());
            return coordinateSections.Select(section => new LineString(section)).ToList();
        }

        private static LineString Densify(IGeometry lineString, double optimum, double minimumLength=Double.NegativeInfinity)
        {
            var precisionModel = lineString.PrecisionModel;
            var coordinates = lineString.Coordinates.ToArray();

            var denseCoordinates = new List<Coordinate>();

            for (var i = 0; i < coordinates.Length - 1; i++)
            {
                denseCoordinates.Add(coordinates[i]);
                var segment = new LineSegment(coordinates[i], coordinates[i + 1]);
                
                var distance = segment.Length;
                if (distance < 2 * optimum) continue;
                if (distance < minimumLength) continue;

                var numberOfParts = Math.Floor(distance/optimum);
                if (!(numberOfParts > 1)) continue;
                
                for (var j = 1; j < numberOfParts; j++)
                {
                    var coordinate = segment.PointAlong(j/numberOfParts);
                    precisionModel.MakePrecise(coordinate);
                    denseCoordinates.Add(coordinate);
                }
            }

            denseCoordinates.Add(coordinates.Last());

            return new LineString(denseCoordinates.ToArray());
        }
    }
}
