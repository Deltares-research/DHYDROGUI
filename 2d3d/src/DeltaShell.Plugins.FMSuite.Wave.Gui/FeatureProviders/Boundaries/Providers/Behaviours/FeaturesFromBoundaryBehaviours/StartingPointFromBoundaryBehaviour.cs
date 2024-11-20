using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.FeaturesFromBoundaryBehaviours
{
    /// <summary>
    /// <see cref="StartingPointFromBoundaryBehaviour"/> defines the behaviour
    /// to construct the starting point corresponding with the provided
    /// <see cref="IWaveBoundary"/>.
    /// </summary>
    public sealed class StartingPointFromBoundaryBehaviour : IFeaturesFromBoundaryBehaviour
    {
        private readonly IWaveBoundaryGeometryFactory geometryFactory;

        /// <summary>
        /// Creates a new <see cref="StartingPointFromBoundaryBehaviour"/>.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="geometryFactory"/> is <c>null</c>.
        /// </exception>
        public StartingPointFromBoundaryBehaviour(IWaveBoundaryGeometryFactory geometryFactory)
        {
            Ensure.NotNull(geometryFactory, nameof(geometryFactory));
            this.geometryFactory = geometryFactory;
        }

        /// <summary>
        /// Executes this <see cref="IFeaturesFromBoundaryBehaviour"/> by constructing the starting point
        /// <see cref="IFeature"/> from the given <paramref name="waveBoundary"/>.
        /// </summary>
        /// <param name="waveBoundary">The wave boundary.</param>
        /// <returns>
        /// The collection of <see cref="IFeature"/> constructed from the given <paramref name="waveBoundary"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveBoundary"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// If no starting point could be constructed an empty list is returned.
        /// </remarks>
        public IEnumerable<IFeature> Execute(IWaveBoundary waveBoundary)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));
            IPoint point = geometryFactory.ConstructBoundaryStartPoint(waveBoundary);

            if (point == null)
            {
                yield break;
            }

            yield return new Feature2DPoint {Geometry = point};
        }
    }
}