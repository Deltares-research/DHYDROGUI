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

    public static class GridWizardMapToolHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GridWizardMapToolHelper));

        public static IList<IPolygon> ComputePolygons(IDiscretization discretization, IList<Feature2D> embankments,
                                                      IPolygon userPolygon, double supportPointDistance, double minimumSupportPointDistance)
        {
            // Protect against a folded userPolygon.
            if (!userPolygon.IsValid)
            {
                log.Error("Invalid or folded user polygon.");
                return null;
            }

            // Check for the presence of embankments.
            IList<LineString> embankmentLineStrings;
            try
            {
                embankmentLineStrings = embankments.Select(embankment => new LineString(embankment.Geometry.Coordinates)).ToList();
            }
            catch (Exception)
            {
                return null;
            }

            if (!embankmentLineStrings.Any())
            {
                log.Error("No embankments cross the user polygon.");
                return null;
            }

            // Demand that each embankment intersects the userPolygon an even number of times.
            ILineString userLineString = userPolygon.ExteriorRing;
            List<Feature2D> oddEmbankments = embankments.Where(e => e.Geometry.Intersection(userLineString).NumPoints % 2 != 0).ToList();
            if (oddEmbankments.Any())
            {
                // Show the names of 5 problematic embankments. 
                log.ErrorFormat("The following {0} embankments crosses the user polygon an odd number of times (max 5): {1}", oddEmbankments.Count, string.Join(", ", oddEmbankments.Take(5).Select(e => e.Name)));
                return null;
            }

            // Identify the branches intersecting the userPolygon.
            List<IBranch> branches = discretization.Network.Branches
                                                   .Where(branch => branch.Geometry.Intersects(userPolygon))
                                                   .ToList();

            if (!branches.Any())
            {
                log.Error("No branches cross the user polygon.");
                return null;
            }

            // Construct velocity LineStrings by cutting the branches with the userPolygon.
            List<ILineString> velocityLineStrings = GetVelocityLineStrings(discretization, branches, userPolygon);
            if (!velocityLineStrings.Any())
            {
                log.Error("No branch velocity points inside the user polygon.");
                return null;
            }

            // Construct embankment LineStrings by cutting the embankments with the userPolygon.
            List<ILineString> cutEmbankmentLineStrings = GetEmbankmentLineStrings(embankmentLineStrings, userPolygon).ToList();

            // Construct LineStrings from velocityLineStrings projected on embankmentLineStrings.
            //    Identify the embankmentLineString - velocityLineStrings associations (1->N).
            Dictionary<ILineString, IList<ILineString>> associations = GetAssociations(cutEmbankmentLineStrings, velocityLineStrings);
            List<ILineString> projectedLineStrings = ProjectLineStrings(associations, minimumSupportPointDistance, supportPointDistance).ToList();
            if (!projectedLineStrings.Any())
            {
                log.Error("Less than two Locations in the Spatial data of a Branch are inside the user polygon.");
                return null;
            }

            // Densify unassociated embankmentLineStrings.
            IEnumerable<LineString> densifiedEmbankmentLineStrings = DensifiedEmbankmentLineStrings(supportPointDistance, minimumSupportPointDistance, cutEmbankmentLineStrings, associations);

            // Construct boundary LineStrings structured as: Embankment intersection, userPolygon Coordinates, Embankment intersection.
            List<IGeometry> branchGeometries = branches.Select(branch => branch.Geometry).ToList();
            IEnumerable<LineString> boundaryLineStrings = MakeBoundaryLineStrings(embankmentLineStrings, userPolygon, branchGeometries);
            if (boundaryLineStrings == null)
            {
                return null;
            }

            List<LineString> denseBoundaryLineStrings = boundaryLineStrings.Select(c => Densify(c, supportPointDistance)).ToList();

            // Cluster all LineStrings.
            IEnumerable<List<ILineString>> toMergeLists = ClusterLineStrings(projectedLineStrings, densifiedEmbankmentLineStrings, denseBoundaryLineStrings);
            if (toMergeLists == null)
            {
                return null;
            }

            // Merge the clustered LineStrings.
            IEnumerable<ILineString> mergedLineStrings = MergeLineStrings(toMergeLists);

            // Construct polygons from merged LineStrings.
            IList<IPolygon> polygons = MakePolygons(mergedLineStrings);

            return polygons;
        }

        private static IEnumerable<LineString> DensifiedEmbankmentLineStrings(double supportPointDistance, double minimumSupportPointDistance,
                                                                              IEnumerable<ILineString> embankmentLineStrings, IDictionary<ILineString, IList<ILineString>> associations)
        {
            ILineString[] unassociatedEmbankmentLineStrings = embankmentLineStrings.Where(embankment => !associations.ContainsKey(embankment)).ToArray();
            var filteredUnassociated = new List<ILineString>();
            foreach (ILineString unassociatedEmbankmentLineString in unassociatedEmbankmentLineStrings)
            {
                Coordinate[] coordinates = unassociatedEmbankmentLineString.Coordinates;
                var newCoordinates = new List<Coordinate> {coordinates[0]};
                Coordinate previous = newCoordinates[0];
                for (var i = 1; i < coordinates.Length - 1; i++)
                {
                    Coordinate current = coordinates[i];
                    if (previous.Distance(current) < minimumSupportPointDistance)
                    {
                        continue;
                    }

                    newCoordinates.Add(current);
                    previous = current;
                }

                newCoordinates.Add(coordinates[coordinates.Length - 1]);
                filteredUnassociated.Add(new LineString(newCoordinates.ToArray()));
            }

            List<LineString> densifiedLineStrings = filteredUnassociated.Select(uaEmbankment => Densify(uaEmbankment, supportPointDistance)).ToList();
            int numberOfUnassociatedEmbankmentLineStrings = densifiedLineStrings.Count;
            if (numberOfUnassociatedEmbankmentLineStrings > 0)
            {
                log.Warn("Found " + numberOfUnassociatedEmbankmentLineStrings + " embankment(s) not belonging to any branch.");
            }

            return densifiedLineStrings;
        }

        private static IEnumerable<List<ILineString>> ClusterLineStrings(IList<ILineString> projectedLineStrings, IEnumerable<ILineString> densifiedLineStrings,
                                                                         IEnumerable<ILineString> denseBoundaryLineStrings)
        {
            // Self-intersecting LineStrings lead to folded polygons.
            if (projectedLineStrings.Any(l => !l.IsSimple))
            {
                log.Warn("Found a self-intersecting embankment.");
                return null;
            }

            var allLineStrings = new List<ILineString>();
            allLineStrings.AddRange(projectedLineStrings);
            allLineStrings.AddRange(densifiedLineStrings);
            allLineStrings.AddRange(denseBoundaryLineStrings);
            foreach (ILineString aLineString in allLineStrings)
            {
                foreach (Coordinate aCoordinate in aLineString.Coordinates)
                {
                    aCoordinate.Z = 0.0;
                }
            }

            var toMergeLists = new List<List<ILineString>>();
            while (allLineStrings.Count > 1)
            {
                var newList = new List<ILineString>();
                ILineString seed = allLineStrings[0];
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

            foreach (List<ILineString> mergeList in toMergeLists)
            {
                while (mergeList.Count > 1)
                {
                    ILineString seed = mergeList[0];
                    mergeList.RemoveAt(0);
                    for (var i = 0; i < mergeList.Count; i++)
                    {
                        Coordinate firstCoordinateMergeLine = mergeList[i].Coordinates[0];
                        Coordinate lastCoordinateMergeLine = mergeList[i].Coordinates[mergeList[i].Coordinates.Length - 1];

                        // Try growing from the end of the seed.
                        Coordinate lastSeedCoordinate = seed.Coordinates[seed.Coordinates.Length - 1];
                        if (lastSeedCoordinate.Distance(firstCoordinateMergeLine) <= distanceEpsilon)
                        {
                            mergeList.Add(new LineString(seed.Coordinates.Concat(mergeList[i].Coordinates.Skip(1)).ToArray()));
                            mergeList.RemoveAt(i);
                            break;
                        }

                        if (lastSeedCoordinate.Distance(lastCoordinateMergeLine) <= distanceEpsilon)
                        {
                            mergeList.Add(new LineString(seed.Coordinates.Concat(mergeList[i].Coordinates.Reverse().Skip(1)).ToArray()));
                            mergeList.RemoveAt(i);
                            break;
                        }

                        // Try growing from the other end of the seed by reversing it.
                        seed = new LineString(seed.Coordinates.Reverse().ToArray());

                        lastSeedCoordinate = seed.Coordinates[seed.Coordinates.Length - 1];
                        if (lastSeedCoordinate.Distance(firstCoordinateMergeLine) <= distanceEpsilon)
                        {
                            mergeList.Add(new LineString(seed.Coordinates.Concat(mergeList[i].Coordinates.Skip(1)).ToArray()));
                            mergeList.RemoveAt(i);
                            break;
                        }

                        if (lastSeedCoordinate.Distance(lastCoordinateMergeLine) <= distanceEpsilon)
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
            CoordinateList[] coordinateLists = mergedLineStrings.Select(lineString => new CoordinateList(lineString.Coordinates.ToArray())).ToArray();
            IList<IPolygon> polygons = new List<IPolygon>();

            foreach (CoordinateList coordinateList in coordinateLists)
            {
                // Determine closedness in 2D, not 3D.
                if (coordinateList[0].Distance(coordinateList[coordinateList.Count - 1]) <= 10.0)
                {
                    coordinateList.RemoveAt(0);
                }

                coordinateList.CloseRing();
                if (coordinateList.Count < 4)
                {
                    continue;
                }

                polygons.Add(new Polygon(new LinearRing(coordinateList.ToCoordinateArray())));
            }

            // Protect against a folded polygons.
            if (polygons.Any(polygon => !polygon.IsValid))
            {
                log.Warn("Generated invalid or folded polygon, aborting.");
                return null;
            }

            // Test for overlap, indicating misconstructed polygons.
            if (polygons.Count > 1 && polygons.Any(p => polygons.Any(p2 => p != p2 && p.Intersects(p2))))
            {
                log.Warn("Constructed overlapping polygons, aborting.");
                return null;
            }

            return polygons;
        }

        private static IEnumerable<ILineString> ProjectLineStrings(Dictionary<ILineString, IList<ILineString>> associations, double minimumDistance, double optimalDistance)
        {
            foreach (KeyValuePair<ILineString, IList<ILineString>> kvp in associations)
            {
                var lengthIndexedLine = new LengthIndexedLine(kvp.Key);

                // Keep the start and end chainages, so that either the projectedLineString's endpoints lie on the userPolygon.
                var chainages = new List<double>
                {
                    lengthIndexedLine.StartIndex,
                    lengthIndexedLine.EndIndex
                };

                // Project the velocity points of each velocityLineString onto the embankmentLineString and determine the maximumProjectedLength for use in densification.
                double maximumProjectedLength = double.NegativeInfinity;
                foreach (ILineString vl in kvp.Value)
                {
                    IOrderedEnumerable<double> chainagesSection = vl.Coordinates.Select(lengthIndexedLine.Project).OrderBy(item => item);
                    chainages.AddRange(chainagesSection);
                    double localMaximum = chainagesSection.Skip(1).Zip(chainagesSection, (second, first) => Math.Abs(second - first)).Max();
                    maximumProjectedLength = Math.Max(maximumProjectedLength, localMaximum);
                }

                // Remove chainages when too close together, preserving start and end chainages.
                double[] orderedChainages = chainages.OrderBy(chainage => chainage).ToArray();
                var newChainages = new List<double>
                {
                    orderedChainages[0],
                    orderedChainages[1]
                };
                double previous = newChainages[0];
                for (var i = 2; i < orderedChainages.Count() - 2; i++)
                {
                    double current = orderedChainages[i];
                    if (Math.Abs(previous - current) < minimumDistance)
                    {
                        continue;
                    }

                    newChainages.Add(current);
                    previous = current;
                }

                newChainages.Add(orderedChainages[orderedChainages.Length - 2]);
                newChainages.Add(orderedChainages[orderedChainages.Length - 1]);

                // Convert chainages to coordinates, accounting for multiple velocity points being projected on the endpoints of the embankmentLineString.
                var sparseLineString = new LineString(newChainages.Distinct().Select(c => lengthIndexedLine.ExtractPoint(c)).ToArray());

                // Densify chainages where a part of the embankments is not associated with a branch.
                LineString denseLineString = Densify(sparseLineString, optimalDistance, maximumProjectedLength);

                // Construct the projectedLineString.
                yield return denseLineString;
            }
        }

        private static Dictionary<ILineString, IList<ILineString>> GetAssociations(IEnumerable<ILineString> embankmentLineStrings, IList<ILineString> velocityLineStrings)
        {
            var associations = new Dictionary<ILineString, IList<ILineString>>();
            foreach (ILineString embankmentLineString in embankmentLineStrings)
            {
                // Store the velocityLineStrings indices closest to each embankmentLineString segment.
                var segmentAssociations = new List<int>();

                // Determine the velocityLineStrings indices.
                var locationIndexedLine = new LocationIndexedLine(embankmentLineString);
                for (int index = locationIndexedLine.StartIndex.SegmentIndex;
                     index <= locationIndexedLine.EndIndex.SegmentIndex;
                     index++)
                {
                    var midPoint = new Point(locationIndexedLine.ExtractPoint(new LinearLocation(index, 0.5)));
                    double closest = double.PositiveInfinity;
                    int velocityIndex = -1;
                    for (var j = 0; j < velocityLineStrings.Count; j++)
                    {
                        double distance = velocityLineStrings[j].Distance(midPoint);
                        // Corner case: A Embankment whose Branch(es) lie outside the userPolygon will be associated to the nearest Branch inside the userPolygon.
                        // Approximate solution: demand distance < 500 m;
                        if (distance >= closest)
                        {
                            continue;
                        }

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
                List<ILineString> associatedEmbankments = Enumerable.Range(0, velocityLineStrings.Count).Where(index => segmentAssociations.Count(sa => sa == index) >= 2).Select(i => velocityLineStrings[i]).ToList();
                if (associatedEmbankments.Any())
                {
                    associations.Add(embankmentLineString, associatedEmbankments);
                }
            }

            return associations;
        }

        private static IEnumerable<ILineString> GetEmbankmentLineStrings(IEnumerable<LineString> embankments, IPolygon userPolygon)
        {
            ILineString userLineString = userPolygon.ExteriorRing;
            foreach (LineString embankment in embankments)
            {
                // Find all intersection coordinates.
                var lineString = new LineString(embankment.Coordinates);
                List<Coordinate> intersectionCoordinates = lineString.Intersection(userLineString).Coordinates.ToList();

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
                double[] intersectionChainages = intersectionCoordinates.Select(lengthIndexedLine.IndexOf).OrderBy(c => c).ToArray();

                // Construct the embankment LineStrings.
                for (var i = 0; i < intersectionChainages.Length; i += 2)
                {
                    var newLine = (LineString) lengthIndexedLine.ExtractLine(intersectionChainages[i], intersectionChainages[i + 1]);
                    yield return newLine;
                }
            }
        }

        private static List<ILineString> GetVelocityLineStrings(INetworkCoverage discretization, IEnumerable<IBranch> branches, IPolygon userPolygon)
        {
            ILineString userLineString = userPolygon.ExteriorRing;
            var velocityLineStrings = new List<ILineString>();

            foreach (IBranch branch in branches)
            {
                // Find all intersection coordinates.
                var lineString = new LineString(branch.Geometry.Coordinates);
                List<Coordinate> intersectionCoordinates = lineString.Intersection(userLineString).Coordinates.ToList();

                Coordinate firstCoordinate = lineString.Coordinates.First();
                Coordinate lastCoordinate = lineString.Coordinates.Last();

                // Supplement intersections when a branch ends inside the userPolygon.
                if (intersectionCoordinates.Count % 2 == 1)
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
                double[] intersectionChainages = intersectionCoordinates.Select(lengthIndexedLine.IndexOf).OrderBy(c => c).ToArray();

                // Find the sorted chainages of the velocity points.
                IList<INetworkLocation> locations = discretization.GetLocationsForBranch(branch);
                double[] velocityChainages = locations.Skip(1).Zip(locations, (second, first) => 0.5 * (first.Chainage + second.Chainage)).OrderBy(c => c).ToArray();

                // Group the velocity chainages by the intersection coordinates.
                double[][] chainageGroups = intersectionChainages.Skip(1)
                                                                 .Zip(intersectionChainages, (second, first) =>
                                                                          velocityChainages.Where(chainage => first <= chainage && chainage <= second).ToArray())
                                                                 .Where(cg => cg.Count() > 1).ToArray();

                // Translate velocity chainages into coordinates.
                IEnumerable<Coordinate[]> coordinateGroups = chainageGroups.Select(group => @group.Select(g => lengthIndexedLine.ExtractPoint(g)).ToArray());

                // Construct the velocity LineStrings.
                velocityLineStrings.AddRange(coordinateGroups.Select(group => new LineString(@group)));
            }

            return velocityLineStrings;
        }

        private static IEnumerable<LineString> MakeBoundaryLineStrings(IList<LineString> embankments, IGeometry userPolygon, IList<IGeometry> branchGeometries)
        {
            // Construct typed Coordinates using userPolygon Coordinates and the Embankment and Branch intersections.
            var typedCoordinates = new List<FlavouredCoordinate>();
            Coordinate[] userCoordinates = userPolygon.Coordinates;

            for (var i = 0; i <= userCoordinates.Length - 2; i++)
            {
                var segment = new LineString(new[]
                {
                    userCoordinates[i],
                    userCoordinates[i + 1]
                });

                IEnumerable<FlavouredCoordinate> embankmentIntersections = embankments.SelectMany(b => b.Intersection(segment).Coordinates.Select(c => new FlavouredCoordinate(c, Flavour.Embankment)));
                IEnumerable<FlavouredCoordinate> branchIntersections = branchGeometries.SelectMany(bg => bg.Intersection(segment).Coordinates.Select(c => new FlavouredCoordinate(c, Flavour.Branch)));
                List<FlavouredCoordinate> intersections = embankmentIntersections.Concat(branchIntersections).ToList();

                // Sort the intersections based on the distance from userCoordinates[i].
                var comparer = new CoordinateDistanceComparer(userCoordinates[i]);
                intersections.Sort(comparer);

                // Add current userLineString Coordinate and subsequent crossings.
                typedCoordinates.Add(new FlavouredCoordinate(userCoordinates[i], Flavour.UserPolygon));
                typedCoordinates.AddRange(intersections);
            }
            // No need to add the last userPolygon Coordinate.

            // Sanity check.
            if (typedCoordinates.Count(c => c.Flavour == Flavour.Branch) < 2)
            {
                log.Warn("Fewer than two crossings between branches and the user polygon found.");
                return null;
            }

            // Shift the typedCoordinates until they start with a Branch.
            int lastIndex = typedCoordinates.Count - 1;
            while (typedCoordinates[0].Flavour != Flavour.Branch)
            {
                typedCoordinates.Move(0, lastIndex);
            }

            // Copy the starting Branch to the end of the typedCoordinates.
            typedCoordinates.Add(typedCoordinates[0]);

            // Identify Embankment * Embankment sections not containing Branch. (* ==  wildcard)
            var typedCoordinateSections = new List<List<int>>();
            int startIndex = -1;
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
                if (typedCoordinates[i].Flavour != Flavour.Embankment)
                {
                    continue;
                }

                if (startIndex > -1)
                {
                    sectionsAfterBranch += 1;
                    // Even number of sections after a Branch are superseded by the densified unassociated embankmentLineStrings.
                    if (sectionsAfterBranch % 2 == 1)
                    {
                        typedCoordinateSections.Add(Enumerable.Range(startIndex, (i - startIndex) + 1).ToList());
                    }
                }

                startIndex = i;
            }

            // Construct the resulting LineStrings.
            IEnumerable<Coordinate[]> coordinateSections = typedCoordinateSections.Select(section => section.Select(c => typedCoordinates[c].Coordinate).ToArray());
            return coordinateSections.Select(section => new LineString(section)).ToList();
        }

        private static LineString Densify(IGeometry lineString, double optimum, double minimumLength = double.NegativeInfinity)
        {
            IPrecisionModel precisionModel = lineString.PrecisionModel;
            Coordinate[] coordinates = lineString.Coordinates.ToArray();

            var denseCoordinates = new List<Coordinate>();

            for (var i = 0; i < coordinates.Length - 1; i++)
            {
                denseCoordinates.Add(coordinates[i]);
                var segment = new LineSegment(coordinates[i], coordinates[i + 1]);

                double distance = segment.Length;
                if (distance < 2 * optimum)
                {
                    continue;
                }

                if (distance < minimumLength)
                {
                    continue;
                }

                double numberOfParts = Math.Floor(distance / optimum);
                if (numberOfParts <= 1)
                {
                    continue;
                }

                for (var j = 1; j < numberOfParts; j++)
                {
                    Coordinate coordinate = segment.PointAlong(j / numberOfParts);
                    precisionModel.MakePrecise(coordinate);
                    denseCoordinates.Add(coordinate);
                }
            }

            denseCoordinates.Add(coordinates.Last());

            return new LineString(denseCoordinates.ToArray());
        }
    }
}