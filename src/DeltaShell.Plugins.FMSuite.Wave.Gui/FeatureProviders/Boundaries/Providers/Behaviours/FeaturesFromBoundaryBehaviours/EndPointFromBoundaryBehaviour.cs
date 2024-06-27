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
    /// <see cref="EndPointFromBoundaryBehaviour"/> defines the behaviour
    /// to construct the end point corresponding with the provided
    /// <see cref="IWaveBoundary"/>.
    /// </summary>
    public sealed class EndPointFromBoundaryBehaviour : IFeaturesFromBoundaryBehaviour
    {
        private readonly IWaveBoundaryGeometryFactory geometryFactory;

        /// <summary>
        /// Creates a new <see cref="EndPointFromBoundaryBehaviour"/>.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="geometryFactory"/> is <c>null</c>.
        /// </exception>
        public EndPointFromBoundaryBehaviour(IWaveBoundaryGeometryFactory geometryFactory)
        {
            Ensure.NotNull(geometryFactory, nameof(geometryFactory));
            this.geometryFactory = geometryFactory;
        }

        /// <summary>
        /// Executes this <see cref="IFeaturesFromBoundaryBehaviour"/> by constructing the end point
        /// <see cref="IFeature"/> from the given <paramref name="waveBoundary"/>.
        /// </summary>
        /// <param name="waveBoundary">The wave boundary.</param>
        /// <returns>
        /// The end point <see cref="IFeature"/> constructed from the given <paramref name="waveBoundary"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveBoundary"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// If no end point could be constructed an empty list is returned.
        /// </remarks>
        public IEnumerable<IFeature> Execute(IWaveBoundary waveBoundary)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));
            IPoint point = geometryFactory.ConstructBoundaryEndPoint(waveBoundary);

            if (point == null)
            {
                yield break;
            }

            yield return new Feature2DPoint {Geometry = point};
        }
    }
}