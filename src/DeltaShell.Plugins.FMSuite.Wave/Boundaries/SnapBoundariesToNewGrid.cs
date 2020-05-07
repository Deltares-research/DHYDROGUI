using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    internal static class SnapBoundariesToNewGrid
    {
        /// <summary>
        /// <see cref="ILog"/> used to log messages.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(SnapBoundariesToNewGrid));

        internal static IEnumerable<IWaveBoundary> RestoreBoundariesIfPossible(IEnumerable<CachedBoundary> cachedBoundaries, IBoundarySnappingCalculator snappingCalculator)
        {
            Ensure.NotNull(cachedBoundaries, nameof(cachedBoundaries));
            if (!cachedBoundaries.Any())
            {
                return Enumerable.Empty<IWaveBoundary>();
            }
            if (snappingCalculator == null)
            {
                return new List<IWaveBoundary>();
            }

            var waveBoundaries = new List<IWaveBoundary>();
            foreach (CachedBoundary cachedBoundary in cachedBoundaries)
            {
                IWaveBoundary res = HandleBoundary(cachedBoundary, snappingCalculator);
                if (res != null)
                {
                    waveBoundaries.Add(res);
                }
            }

            return waveBoundaries;
        }

        internal static IEnumerable<CachedBoundary> CreateCachedBoundaries(IEnumerable<IWaveBoundary> boundaries, IGridBoundary gridBoundary)
        {
            Ensure.NotNull(boundaries, nameof(boundaries));
            if (!boundaries.Any())
            {
                return Enumerable.Empty<CachedBoundary>();
            }
            if (gridBoundary == null)
            {
                return Enumerable.Empty<CachedBoundary>();
            }

            var caching = new List<CachedBoundary>();
            foreach (IWaveBoundary waveBoundary in boundaries)
            {
                caching.Add(CreateCachedBoundary(waveBoundary, gridBoundary));
            }

            return caching;
        }

        private static IWaveBoundary HandleBoundary(CachedBoundary boundary, IBoundarySnappingCalculator snappingCalculator)
        {
            var endpoints = new List<Coordinate>()
            {
                boundary.StartingPointWorldCoordinate,
                boundary.EndingPointWorldCoordinate
            };

            List<GridBoundaryCoordinate> reSappedBoundaryCoordinates = WaveBoundaryGeometricDefinitionFactoryHelper.GetSnappedEndPoints(snappingCalculator, endpoints).ToList();

            if (reSappedBoundaryCoordinates.Count < 2)
            {
                log.Warn($"Boundary {boundary.WaveBoundary.Name} could not snap to the new grid (begin and or end point problematic). Please inspect your boundaries.");
                return null;
            }

            //Create a new boundary and add this to the boundaryContainer
            IWaveBoundaryGeometricDefinition newGeometricDefinition = WaveBoundaryGeometricDefinitionFactoryHelper.GetGeometricDefinition(reSappedBoundaryCoordinates, snappingCalculator);
            if (newGeometricDefinition == null)
            {
                log.Warn($"Boundary {boundary.WaveBoundary.Name} could not snap to the new grid. Please inspect your boundaries.");
                return null;
            }

            var waveBoundary = new WaveBoundary(boundary.WaveBoundary.Name, newGeometricDefinition, boundary.WaveBoundary.ConditionDefinition);

            // skip the first and last points, these are always the begin and endpoint and will be added during construction of the WaveBoundary
            IEventedList<SupportPoint> oldSupportPoints = boundary.WaveBoundary.GeometricDefinition.SupportPoints;
            var dict = new Dictionary<SupportPoint, SupportPoint>();

            SupportPoint firstPoint = FirstPoint(oldSupportPoints);
            SupportPoint lastPoint = LastPoint(oldSupportPoints);

            dict.Add(firstPoint, FirstPoint(newGeometricDefinition.SupportPoints));
            dict.Add(lastPoint, LastPoint(newGeometricDefinition.SupportPoints));
            
            foreach (SupportPoint supportPoint in oldSupportPoints)
            {
                if (supportPoint == firstPoint || supportPoint == lastPoint)
                {
                    continue;
                }

                if (supportPoint.Distance <= newGeometricDefinition.Length)
                {
                    var newSup = new SupportPoint(supportPoint.Distance, newGeometricDefinition);
                    dict.Add(supportPoint, newSup);
                    waveBoundary.GeometricDefinition.SupportPoints.Add(newSup);
                }
                else
                {
                    log.Warn($"Support point at distance {supportPoint.Distance} does no longer fit on the snapped Boundary {boundary.WaveBoundary.Name}; Removed. Please inspect your support points");
                    dict.Add(supportPoint, null);
                }
            }

            UpdateSupportPointsInDataComponent(waveBoundary.ConditionDefinition.DataComponent, dict);

            return waveBoundary;
        }

        private static SupportPoint FirstPoint(IEnumerable<SupportPoint> supportPoints)
        {
            SupportPoint firstSupportPoint = null;
            var lowestSupportPointDistance = double.MaxValue;
            foreach (SupportPoint supportPoint in supportPoints)
            {
                if (supportPoint.Distance < lowestSupportPointDistance)
                {
                    firstSupportPoint = supportPoint;
                    lowestSupportPointDistance = supportPoint.Distance;
                }
            }

            return firstSupportPoint;
        }

        private static SupportPoint LastPoint(IEnumerable<SupportPoint> supportPoints)
        {
            SupportPoint lastSupportPoint = null;
            var highestSupportPointDistance = double.MinValue;
            foreach (SupportPoint supportPoint in supportPoints)
            {
                if (supportPoint.Distance > highestSupportPointDistance)
                {
                    lastSupportPoint = supportPoint;
                    highestSupportPointDistance = supportPoint.Distance;
                }
            }

            return lastSupportPoint;
        }

        private static void UpdateSupportPointsInDataComponent(ISpatiallyDefinedDataComponent component, IDictionary<SupportPoint, SupportPoint> toUpdate)
        {
            var visitor = new UpdateSupportPointVisitor(toUpdate);
            component.AcceptVisitor(visitor);
        }

        private static CachedBoundary CreateCachedBoundary(IWaveBoundary boundary, IGridBoundary gridBoundary)
        {
            var startingLocalCoordinate = new GridBoundaryCoordinate(boundary.GeometricDefinition.GridSide, boundary.GeometricDefinition.StartingIndex);
            var endingLocalCoordinate = new GridBoundaryCoordinate(boundary.GeometricDefinition.GridSide, boundary.GeometricDefinition.EndingIndex);
            Coordinate startingPointWorldCoordinate = gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(startingLocalCoordinate);
            Coordinate endingPointWordCoordinate = gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(endingLocalCoordinate);

            return new CachedBoundary(startingPointWorldCoordinate, endingPointWordCoordinate, boundary);
        }

        internal class UpdateSupportPointVisitor : ISpatiallyDefinedDataComponentVisitor
        {
            private readonly IDictionary<SupportPoint, SupportPoint> toUpdate;

            public UpdateSupportPointVisitor(IDictionary<SupportPoint, SupportPoint> toUpdate)
            {
                Ensure.NotNull(toUpdate, nameof(toUpdate));
                this.toUpdate = toUpdate;
            }

            public void Visit<T>(UniformDataComponent<T> uniformDataComponent) where T : IForcingTypeDefinedParameters
            {
                // Nothing to update
            }

            public void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent) where T : IForcingTypeDefinedParameters
            {
                foreach (KeyValuePair<SupportPoint, SupportPoint> elem in toUpdate)
                {
                    if (!spatiallyVaryingDataComponent.Data.ContainsKey(elem.Key))
                    {
                        continue;
                    }

                    if (elem.Value != null)
                    {
                        spatiallyVaryingDataComponent.ReplaceSupportPoint(elem.Key, elem.Value);
                    }
                    else
                    {
                        spatiallyVaryingDataComponent.RemoveSupportPoint(elem.Key);
                    }
                }
            }
        }
    }
}