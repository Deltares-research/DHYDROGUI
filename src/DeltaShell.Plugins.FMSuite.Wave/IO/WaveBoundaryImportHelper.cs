using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public static class WaveBoundaryImportHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaveBoundaryImportHelper));

        public static IGeometry CreateBoundaryGeometry(Coordinate startCoordinate, Coordinate endCoordinate,
                                                       IList<double> condSpecAtDists)
        {
            IGeometry geometry;
            if (condSpecAtDists.Any())
            {
                Coordinate delta = Min(endCoordinate, startCoordinate);
                double length = Math.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y));
                var coordinates = new List<Coordinate> {startCoordinate};

                const double eps = 1e-5;
                foreach (double condSpecAtDist in condSpecAtDists)
                {
                    if (condSpecAtDist > (1 - eps) * length) //skipping spurious end points
                    {
                        Log.WarnFormat(
                            "CondSpecAtDist {0} exceeds boundary length {1}: skipping boundary points from here",
                            condSpecAtDist, length);
                        break;
                    }

                    if (condSpecAtDist > eps * length && condSpecAtDist < length)
                    {
                        coordinates.Add(Plus(startCoordinate, Times(delta, condSpecAtDist / length)));
                    }
                }

                coordinates.Add(endCoordinate);
                geometry = new LineString(coordinates.ToArray());
            }
            else
            {
                geometry = new LineString(new[]
                {
                    startCoordinate,
                    endCoordinate
                });
            }

            return geometry;
        }

        public static void CreateDummyFeature(IFeature feature, IEnumerable<double> condSpecAtDists)
        {
            Coordinate[] coordinates = Enumerable.Range(0, 2 + condSpecAtDists.Count())
                                                 .Select(i => new Coordinate(i, 0))
                                                 .OfType<Coordinate>()
                                                 .ToArray();
            feature.Geometry = new LineString(coordinates);
            if (feature.Attributes == null)
            {
                feature.Attributes = new DictionaryFeatureAttributeCollection();
            }

            feature.Attributes["condSpecAtDists"] = condSpecAtDists.ToList();
        }

        public static IEnumerable<WaveBoundaryCondition> ConvertToCoordinateBased(
            IEnumerable<WaveBoundaryCondition> orientedBoundaryConditions, IDiscreteGridPointCoverage grid)
        {
            if (grid == null || grid.X.Values.Count == 0)
            {
                if (orientedBoundaryConditions.Any())
                {
                    Log.ErrorFormat("Import of boundaries failed: no valid grid to convert oriented boundaries");
                }

                yield break;
            }

            List<Tuple<double, List<Coordinate>>> boundaryDirections = CalculateSideOrientations(grid);

            foreach (WaveBoundaryCondition bc in orientedBoundaryConditions)
            {
                Feature2D dummyFeature = bc.Feature;
                var condSpecAtDists = (IList<double>) dummyFeature.Attributes["condSpecAtDists"];
                double direction = GetDirection((string) dummyFeature.Attributes["orientation"]);

                // match to side:
                Tuple<double, List<Coordinate>> matchingSide =
                    boundaryDirections.OrderBy(t => Math.Cos(direction - t.Item1)).Last();

                Coordinate start = matchingSide.Item2.First(c => !double.IsNaN(c.X) && !double.IsNaN(c.Y));
                Coordinate end = matchingSide.Item2.Last(c => !double.IsNaN(c.X) && !double.IsNaN(c.Y));

                // create feature
                bc.Feature = new Feature2D
                {
                    Name = bc.Feature.Name,
                    Geometry = CreateBoundaryGeometry(start, end, condSpecAtDists)
                };

                Log.WarnFormat("Converting oriented boundary \'{0}\' to xy-coordinates, please check its location",
                               bc.Name);

                yield return bc;
            }
        }

        public static List<Tuple<double, List<Coordinate>>> CalculateSideOrientations(IDiscreteGridPointCoverage grid)
        {
            int nMax = grid.Size1 - 1;
            int mMax = grid.Size2 - 1;
            IMultiDimensionalArray<double> x = grid.X.Values;
            IMultiDimensionalArray<double> y = grid.Y.Values;

            var sides = new List<List<Coordinate>>();
            sides.Add(Enumerable.Range(0, grid.Size1).Select(n => new Coordinate(x[nMax - n, mMax], y[nMax - n, mMax]))
                                .ToList());
            sides.Add(Enumerable.Range(0, grid.Size2).Select(m => new Coordinate(x[0, mMax - m], y[0, mMax - m]))
                                .ToList());
            sides.Add(Enumerable.Range(0, grid.Size1).Select(n => new Coordinate(x[n, 0], y[n, 0])).ToList());
            sides.Add(Enumerable.Range(0, grid.Size2).Select(m => new Coordinate(x[nMax, m], y[nMax, m])).ToList());

            return sides.Select(s => new Tuple<double, List<Coordinate>>(CalculateOrientation(s), s)).ToList();
        }

        private static Coordinate Min(Coordinate coord1, Coordinate coord2)
        {
            return new Coordinate(coord1.X - coord2.X, coord1.Y - coord2.Y, coord1.Z - coord2.Z);
        }

        private static Coordinate Plus(Coordinate coord1, Coordinate coord2)
        {
            return new Coordinate(coord1.X + coord2.X, coord1.Y + coord2.Y, coord1.Z + coord2.Z);
        }

        private static Coordinate Times(Coordinate coord1, double d)
        {
            return new Coordinate(coord1.X * d, coord1.Y * d, coord1.Z * d);
        }

        /// <summary>
        /// Calculates side orientation: between -pi and pi
        /// </summary>
        /// <param name="coordinates"> </param>
        /// <returns> </returns>
        private static double CalculateOrientation(List<Coordinate> coordinates)
        {
            if (coordinates.Count < 2)
            {
                throw new ArgumentException("Grid boundary should have more than 1 coordinate");
            }

            var sumX = 0.0;
            var sumY = 0.0;

            for (var i = 1; i < coordinates.Count; ++i)
            {
                // from SWAN source 4091:
                // if both grid points at ends of a step are valid, then
                // take DX and DY into account when determining direction
                double x1 = coordinates[i - 1].X;
                double y1 = coordinates[i - 1].Y;
                double x2 = coordinates[i].X;
                double y2 = coordinates[i].Y;

                if (double.IsNaN(x1) ||
                    double.IsNaN(y1) ||
                    double.IsNaN(x2) ||
                    double.IsNaN(y2))
                {
                    continue;
                }

                sumX += x2 - x1;
                sumY += y2 - y1;
            }

            return Math.Atan2(sumY, sumX); // x-axis points eastward, north equals 0 degrees
        }

        private static double GetDirection(string orientation)
        {
            if (orientation == "north")
            {
                return 0.0;
            }

            if (orientation == "northwest")
            {
                return Math.PI / 4.0;
            }

            if (orientation == "west")
            {
                return Math.PI / 2.0;
            }

            if (orientation == "southwest")
            {
                return (3.0 * Math.PI) / 4.0;
            }

            if (orientation == "south")
            {
                return Math.PI;
            }

            if (orientation == "southeast")
            {
                return (-3.0 * Math.PI) / 4.0;
            }

            if (orientation == "east")
            {
                return -Math.PI / 2.0;
            }

            if (orientation == "northeast")
            {
                return -Math.PI / 4.0;
            }

            return 0.0;
        }
    }
}