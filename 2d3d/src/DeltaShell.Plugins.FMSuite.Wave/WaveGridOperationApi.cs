using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;
using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    public class WaveGridOperationApi : IGridOperationApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaveGridOperationApi));

        private readonly IDiscreteGridPointCoverage grid;

        public WaveGridOperationApi(IDiscreteGridPointCoverage grid)
        {
            this.grid = grid;
        }

        public bool SnapsToGrid(IGeometry geometry)
        {
            return true;
        }

        public IGeometry GetGridSnappedGeometry(string featureType, IGeometry geometry)
        {
            return GetGridSnappedGeometry(geometry);
        }

        public IEnumerable<IGeometry> GetGridSnappedGeometry(string featureType, ICollection<IGeometry> geometries)
        {
            return geometries.Select(GetGridSnappedGeometry).ToArray();
        }

        public int[] GetLinkedCells()
        {
            throw new NotImplementedException();
        }

        private IGeometry GetGridSnappedGeometry(IGeometry geometry)
        {
            if (geometry == null)
            {
                return null;
            }

            if (!grid.X.Values.Any())
            {
                // no snapping on empty grid..
                return geometry;
            }

            IList<GridCoordinate> gridEnvelope = CreateGridEnvelope();
            List<GridCoordinate> snappedCoords =
                geometry.Coordinates.Select(c => FindNearestCoordinate(c, gridEnvelope)).ToList();

            if (snappedCoords.Count < 2)
            {
                return null;
            }

            var finalCoordinates = new List<GridCoordinate> { snappedCoords[0] };
            for (var i = 1; i < snappedCoords.Count; ++i)
            {
                // check if same segment:
                GridCoordinate next = snappedCoords[i];
                GridCoordinate start = snappedCoords[0];

                if (!OnSameSide(start, next))
                {
                    // no cornering
                    Log.ErrorFormat("It is not allowed to have boundaries go around a corner.");
                    return null;
                }

                if (finalCoordinates.Any(c => c.Equals(next)))
                {
                    // no duplicates
                    Log.ErrorFormat(
                        "It is not allowed to have multiple support points snapped to the same grid point.");
                    return null;
                }

                finalCoordinates.Add(snappedCoords[i]);
            }

            finalCoordinates = finalCoordinates.Distinct().ToList();

            if (finalCoordinates.Count < 2)
            {
                return null;
            }

            // we know they're on the same side, now check for dry points
            if (ContainsDryPoints(finalCoordinates))
            {
                return null;
            }

            // ordered?
            bool mProperlyOrdered = finalCoordinates.Select(c => c.M).IsMonotonousAscending() ||
                                    finalCoordinates.Select(c => c.M).IsMonotonousDescending();
            bool nProperlyOrdered = finalCoordinates.Select(c => c.N).IsMonotonousAscending() ||
                                    finalCoordinates.Select(c => c.N).IsMonotonousDescending();
            if (!(nProperlyOrdered && mProperlyOrdered))
            {
                Log.ErrorFormat(
                    "Snapped boundary points should be properly ordered in terms of grid coordinates (i.e. not self-intersecting)");
                return null;
            }

            return new LineString(finalCoordinates.Select(GetCoordinate).ToArray());
        }

        private bool OnSameSide(GridCoordinate c1, GridCoordinate c2)
        {
            return c2.N == c1.N || c2.M == c1.M;
        }

        private GridCoordinate FindNearestCoordinate(Coordinate coord, IList<GridCoordinate> envelope)
        {
            GridCoordinate nearestCoordinate = null;
            var closestApproach = double.MaxValue;

            foreach (GridCoordinate c in envelope)
            {
                double d = GetCoordinate(c).Distance(coord);
                if (d < closestApproach)
                {
                    closestApproach = d;
                    nearestCoordinate = c;
                }
            }

            if (nearestCoordinate == null)
            {
                throw new ArgumentException("Snapping failed, this can happen only when there is no boundary ...");
            }

            return nearestCoordinate;
        }

        private Coordinate GetCoordinate(GridCoordinate c)
        {
            double x = grid.X.Values[c.N, c.M];
            double y = grid.Y.Values[c.N, c.M];
            return new Coordinate(x, y);
        }

        private IList<GridCoordinate> CreateGridEnvelope()
        {
            int sizeN = grid.Size1;
            int sizeM = grid.Size2;
            var coordinates = new List<GridCoordinate>();

            int nMax = sizeN - 1;
            int mMax = sizeM - 1;
            coordinates.AddRange(Enumerable.Range(0, sizeN)
                                           .Select(n => new GridCoordinate(nMax - n, mMax))); // bottom right, ccw
            coordinates.AddRange(Enumerable.Range(0, sizeM - 1).Select(m => new GridCoordinate(0, mMax - m)));
            coordinates.AddRange(Enumerable.Range(0, sizeN - 1).Select(n => new GridCoordinate(n, 0)));
            coordinates.AddRange(Enumerable.Range(0, sizeM - 1).Select(m => new GridCoordinate(nMax, m)));

            return RemoveDryPoints(coordinates);
        }

        private bool ContainsDryPoints(List<GridCoordinate> finalCoordinates)
        {
            GridCoordinate firstCoord = finalCoordinates[0];
            GridCoordinate lastCoord = finalCoordinates[finalCoordinates.Count - 1];

            int deltaN = lastCoord.N - firstCoord.N;
            int deltaM = lastCoord.M - firstCoord.M;
            int count = Math.Max(Math.Abs(deltaN), Math.Abs(deltaM));
            deltaN = deltaN > 0 ? 1 : deltaN < 0 ? -1 : 0;
            deltaM = deltaM > 0 ? 1 : deltaM < 0 ? -1 : 0;

            for (var i = 0; i < count; ++i)
            {
                var nextToCheck = new GridCoordinate(firstCoord.N + (i * deltaN), firstCoord.M + (i * deltaM));
                if (IsDryPoint(nextToCheck))
                {
                    return true;
                }
            }

            return false;
        }

        // creates boundary segments of the grid by excluding dry points (== null)
        private IList<GridCoordinate> RemoveDryPoints(IEnumerable<GridCoordinate> coordinates)
        {
            return coordinates.Where(c => !IsDryPoint(c)).ToList();
        }

        private bool IsDryPoint(GridCoordinate gridCoordinate)
        {
            double x = grid.X.Values[gridCoordinate.N, gridCoordinate.M];
            double y = grid.Y.Values[gridCoordinate.N, gridCoordinate.M];

            return WaveDomainHelper.IsDryPoint(x, y);
        }

        private class GridCoordinate
        {
            public GridCoordinate(int n, int m)
            {
                N = n;
                M = m;
            }

            public int N { get; private set; }
            public int M { get; private set; }
        }
    }
}