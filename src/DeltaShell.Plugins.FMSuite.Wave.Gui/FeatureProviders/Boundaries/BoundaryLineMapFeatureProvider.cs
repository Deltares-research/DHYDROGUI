using System;
using System.Collections;
using System.Collections.Specialized;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries
{
    /// <summary>
    /// <see cref="BoundaryLineMapFeatureProvider"/> is responsible for showing
    /// the appropriate boundary line features given a boundary container.
    /// It provides the appropriate methods such that these features and their
    /// underlying data can be created through the Map.
    /// </summary>
    /// <remarks>
    /// This class leverages the <see cref="MultiIEventedListAdapter{TObserved,TDisplayed}"/>
    /// to create the appropriate lists. These classes are necessary to play
    /// nice with the framework, and ensure a good separation of concerns between
    /// view and data. As such, this feature provider can be seen as a view model
    /// for the line data for the Map.
    /// </remarks>
    public sealed class BoundaryLineMapFeatureProvider : Feature2DCollection
    {
        private readonly MultiIEventedListAdapter<IWaveBoundary, BoundaryLineFeature> lineFeatures;
        private readonly IBoundaryContainer boundaryContainer;
        private readonly IWaveBoundaryFactory waveBoundaryFactory;
        private readonly IWaveBoundaryGeometryFactory waveBoundaryGeometryFactory;

        // TODO: (MWT) move these to a helper class, so they can be easily tested?
        private Tuple<IWaveBoundary, IEventedList<IWaveBoundary>> ObtainWaveBoundaryFromFeature(BoundaryLineFeature feature)
        {
            return new Tuple<IWaveBoundary, IEventedList<IWaveBoundary>>(feature.ObservedWaveBoundary,
                                                                         boundaryContainer.Boundaries);
        }

        private BoundaryLineFeature CreateBoundaryLineFeature(IWaveBoundary waveBoundary)
        {
            return new BoundaryLineFeature()
            {
                ObservedWaveBoundary = waveBoundary,
                Geometry = waveBoundaryGeometryFactory.ConstructBoundaryLineGeometry(waveBoundary),
            };
        }

        /// <summary>
        /// Creates a new <see cref="BoundaryLineMapFeatureProvider"/>.
        /// </summary>
        /// <param name="boundaryContainer">The boundary container.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <param name="waveBoundaryFactory">The waveBoundaryFactory.</param>
        /// <param name="waveBoundaryGeometryFactory">The waveBoundaryGeometryFactory.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when ay parameter is <c>null</c>.
        /// </exception>
        public BoundaryLineMapFeatureProvider(IBoundaryContainer boundaryContainer, 
                                              ICoordinateSystem coordinateSystem, 
                                              IWaveBoundaryFactory waveBoundaryFactory,
                                              IWaveBoundaryGeometryFactory waveBoundaryGeometryFactory)
        {
            Ensure.NotNull(boundaryContainer, nameof(boundaryContainer));
            Ensure.NotNull(waveBoundaryFactory, nameof(waveBoundaryFactory));
            Ensure.NotNull(waveBoundaryGeometryFactory, nameof(waveBoundaryGeometryFactory));

            CoordinateSystem = coordinateSystem;

            this.waveBoundaryFactory = waveBoundaryFactory;
            this.waveBoundaryGeometryFactory = waveBoundaryGeometryFactory;

            this.boundaryContainer = boundaryContainer;

            lineFeatures = new MultiIEventedListAdapter<IWaveBoundary, BoundaryLineFeature>(ObtainWaveBoundaryFromFeature, 
                                                                                            CreateBoundaryLineFeature);
            lineFeatures.RegisterList(this.boundaryContainer.Boundaries);
            SubscribeToEventing();
        }

        /// <summary>
        /// Construct a new <see cref="BoundaryLineFeature"/> based upon the
        /// geometry, and add this feature to this <see cref="BoundaryLineMapFeatureProvider"/>.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns>
        /// The constructed <see cref="BoundaryLineFeature"/> if one could be constructed,
        /// otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This will add an <see cref="IWaveBoundary"/> to the underlying model.
        /// </remarks>
        public override IFeature Add(IGeometry geometry)
        {
            if (!(geometry is ILineString lineString))
            {
                return null;
            }

            IWaveBoundary boundary = waveBoundaryFactory.ConstructWaveBoundary(lineString);

            if (boundary == null)
            {
                return null;
            }

            boundaryContainer.Boundaries.Add(boundary);
            
            // We do not want to return this here, however the interface requires this (but never uses it).
            return null; 
        }

        public override bool Add(IFeature feature)
        {
            throw new NotSupportedException("This is currently not supported, implement when needed.");
        }

        private void SubscribeToEventing()
        {
            lineFeatures.CollectionChanged += OnFeaturesCollectionChanged;
        }

        private void UnsubscribeFromEventing()
        {
            lineFeatures.CollectionChanged -= OnFeaturesCollectionChanged;
        }

        private void OnFeaturesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireFeaturesChanged();
        }

        public override IList Features
        {
            get => lineFeatures;
            set => throw new NotSupportedException("Setting the Features to another value is currently not supported.");
        }

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();
            Dispose(true);

            // This has not been done in the parent classes.
            // In an attempt to do it somewhat correctly here, we suppress it here.
            GC.SuppressFinalize(this);
        }

        // Since this class is sealed, this method is private and non-virtual.
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeFromEventing();
            }
        }
        #endregion
    }
}