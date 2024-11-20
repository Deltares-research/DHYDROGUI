using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="SnapBoundariesToNewGrid"/> is responsible for caching and restoring
    /// wave boundaries when the grid is changed.
    /// </summary>
    internal static class SnapBoundariesToNewGrid
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SnapBoundariesToNewGrid));

        /// <summary>
        /// Restores the boundaries if possible.
        /// </summary>
        /// <param name="cachedBoundaries">The cached boundaries.</param>
        /// <param name="geometricDefinitionFactory">The geometric definition factory.</param>
        /// <returns>
        /// The <see cref="IEnumerable{IWaveBoundary}"/> containing the successfully
        /// restored <see cref="IWaveBoundary"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="cachedBoundaries"/> or <paramref name="geometricDefinitionFactory"/> is <c>null</c>.
        /// </exception>
        internal static IEnumerable<IWaveBoundary> RestoreBoundariesIfPossible(IEnumerable<CachedBoundary> cachedBoundaries,
                                                                               IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory)
        {
            Ensure.NotNull(cachedBoundaries, nameof(cachedBoundaries));
            Ensure.NotNull(geometricDefinitionFactory, nameof(geometricDefinitionFactory));

            foreach (CachedBoundary cachedBoundary in cachedBoundaries)
            {
                IWaveBoundary updatedBoundary = HandleBoundary(cachedBoundary,
                                                               geometricDefinitionFactory);
                if (updatedBoundary != null)
                {
                    yield return updatedBoundary;
                }
            }
        }

        /// <summary>
        /// Creates the cached boundaries.
        /// </summary>
        /// <param name="boundaries">The boundaries.</param>
        /// <param name="gridBoundary">The grid boundary.</param>
        /// <returns>
        /// The <see cref="IEnumerable{CachedBoundary}"/> containing the
        /// <see cref="CachedBoundary"/> created from the provided <paramref name="boundaries"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaries"/> is <c>null</c>.
        /// </exception>
        internal static IEnumerable<CachedBoundary> CreateCachedBoundaries(IEnumerable<IWaveBoundary> boundaries, IGridBoundary gridBoundary)
        {
            Ensure.NotNull(boundaries, nameof(boundaries));

            if (gridBoundary == null)
            {
                yield break;
            }

            foreach (IWaveBoundary waveBoundary in boundaries)
            {
                yield return CreateCachedBoundary(waveBoundary, gridBoundary);
            }
        }

        private static IWaveBoundary HandleBoundary(CachedBoundary boundary,
                                                    IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory)
        {
            IWaveBoundaryGeometricDefinition newGeometricDefinition =
                geometricDefinitionFactory.ConstructWaveBoundaryGeometricDefinition(boundary.StartingPointWorldCoordinate,
                                                                                    boundary.EndingPointWorldCoordinate);

            if (newGeometricDefinition == null)
            {
                log.WarnFormat(Resources.SnapBoundariesToNewGrid_HandleBoundary_Boundary__0__could_not_snap_to_the_new_grid__Please_inspect_your_boundaries_,
                               boundary.WaveBoundary.Name);
                return null;
            }

            var waveBoundary = new WaveBoundary(boundary.WaveBoundary.Name,
                                                newGeometricDefinition,
                                                boundary.WaveBoundary.ConditionDefinition);

            // skip the first and last points, these are always the begin and endpoint and will be added during construction of the WaveBoundary
            IEventedList<SupportPoint> oldSupportPoints = boundary.WaveBoundary.GeometricDefinition.SupportPoints;
            var toUpdateDict = new Dictionary<SupportPoint, SupportPoint>();

            SupportPoint firstPoint = FirstPoint(oldSupportPoints);
            SupportPoint lastPoint = LastPoint(oldSupportPoints);

            toUpdateDict.Add(firstPoint, FirstPoint(newGeometricDefinition.SupportPoints));
            toUpdateDict.Add(lastPoint, LastPoint(newGeometricDefinition.SupportPoints));

            foreach (SupportPoint supportPoint in oldSupportPoints)
            {
                if (supportPoint == firstPoint || supportPoint == lastPoint)
                {
                    continue;
                }

                if (supportPoint.Distance <= newGeometricDefinition.Length)
                {
                    var newSup = new SupportPoint(supportPoint.Distance, newGeometricDefinition);
                    toUpdateDict.Add(supportPoint, newSup);
                    waveBoundary.GeometricDefinition.SupportPoints.Add(newSup);
                }
                else
                {
                    log.WarnFormat(Resources.SnapBoundariesToNewGrid_HandleBoundary_Support_point_at_distance__0__does_no_longer_fit_on_the_snapped_Boundary__1___Removed__Please_inspect_your_support_points,
                                   supportPoint.Distance, boundary.WaveBoundary.Name);
                    toUpdateDict.Add(supportPoint, null);
                }
            }

            UpdateSupportPointsInDataComponent(waveBoundary.ConditionDefinition.DataComponent, toUpdateDict);

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

        /// <summary>
        /// <see cref="UpdateSupportPointVisitor"/> is responsible for replacing the support points in a
        /// <see cref="ISpatiallyDefinedDataComponent"/> by newly created support points.
        /// </summary>
        /// <seealso cref="ISpatiallyDefinedDataComponentVisitor"/>
        internal class UpdateSupportPointVisitor : ISpatiallyDefinedDataComponentVisitor
        {
            private readonly IDictionary<SupportPoint, SupportPoint> toUpdate;

            /// <summary>
            /// Creates a new <see cref="UpdateSupportPointVisitor"/>.
            /// </summary>
            /// <param name="toUpdate">The dictionary containing the mapping of old to new support points to update.</param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="toUpdate"/> is <c>null</c>.
            /// </exception>
            public UpdateSupportPointVisitor(IDictionary<SupportPoint, SupportPoint> toUpdate)
            {
                Ensure.NotNull(toUpdate, nameof(toUpdate));
                this.toUpdate = toUpdate;
            }

            public void Visit<T>(UniformDataComponent<T> uniformDataComponent) where T : IForcingTypeDefinedParameters
            {
                // Nothing to update
            }

            /// <summary>
            /// Update the support points in <paramref name="spatiallyVaryingDataComponent"/> with the new support
            /// points defined in the toUpdate dictionary.
            /// </summary>
            /// <typeparam name="T">The forcing type.</typeparam>
            /// <param name="spatiallyVaryingDataComponent">The visited <see cref="SpatiallyVaryingDataComponent{T}"/></param>
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