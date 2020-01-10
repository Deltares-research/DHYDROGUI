using System;
using System.Collections;
using System.Linq;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries
{
    /// <summary>
    /// <see cref="BoundaryEndPointMapFeatureProvider"/> is responsible for
    /// generating the features corresponding with the endpoints of all
    /// <see cref="IWaveBoundary"/>.
    /// </summary>
    /// <remarks>
    /// Several assumptions are made, which might be invalidated in future
    /// implementations, this will most likely require this class to be
    /// rewritten.
    ///
    /// * It is assumed that <see cref="IWaveBoundary"/> are static, once placed.
    ///   Follow up issues will most likely invalidate this invariant. In this
    ///   case, <see cref="BoundaryEndPointMapFeatureProvider"/> will need to be
    ///   extended or rewritten, to allow for operations on EndPoints.
    /// * Currently it assumed no explicit EndPoints are added, instead we assume
    ///   that once a <see cref="IWaveBoundary"/> is added through the
    ///   <see cref="BoundaryLineMapFeatureProvider"/>, a refresh is triggered,
    ///   which will generate the EndPoints anew. This is not an ideal solution,
    ///   but given the minimal amount of endpoints which will exist within a
    ///   model it is sufficient.
    /// </remarks>
    /// <seealso cref="FeatureCollection"/>
    public sealed class BoundaryEndPointMapFeatureProvider : FeatureCollection
    {
        private readonly IBoundaryContainer boundaryContainer;
        private readonly IWaveBoundaryGeometryFactory waveBoundaryGeometryFactory;

        /// <summary>
        /// Creates a new <see cref="BoundaryEndPointMapFeatureProvider"/>.
        /// </summary>
        /// <param name="boundaryContainer">The boundary container.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <param name="waveBoundaryGeometryFactory">The geometry factory.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="boundaryContainer"/> or
        /// <paramref name="waveBoundaryGeometryFactory"/> is <c>null</c>.
        /// </exception>
        public BoundaryEndPointMapFeatureProvider(IBoundaryContainer boundaryContainer,
                                                  ICoordinateSystem coordinateSystem, 
                                                  IWaveBoundaryGeometryFactory waveBoundaryGeometryFactory)
        {
            Ensure.NotNull(boundaryContainer, nameof(boundaryContainer));
            Ensure.NotNull(waveBoundaryGeometryFactory, nameof(waveBoundaryGeometryFactory));

            this.boundaryContainer = boundaryContainer;
            this.waveBoundaryGeometryFactory = waveBoundaryGeometryFactory;

            CoordinateSystem = coordinateSystem;
            FeatureType = typeof(Feature2DPoint);
        }

        public override IList Features
        {
            get => boundaryContainer.Boundaries
                                    .SelectMany(boundary => waveBoundaryGeometryFactory.ConstructBoundaryEndPoints(boundary))
                                    .Select(p => new Feature2DPoint {Geometry = p})
                                    .ToList();
            set => throw new NotSupportedException("This is currently not supported, implement when needed.");
        }

        public override IFeature Add(IGeometry geometry) => 
            throw new NotSupportedException("This is currently not supported, implement when needed.");
        public override bool Add(IFeature feature) => 
            throw new NotSupportedException("This is currently not supported, implement when needed.");
    }
}