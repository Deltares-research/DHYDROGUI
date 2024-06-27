using System;
using System.Collections;
using System.Collections.Specialized;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Features;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.AddBehaviours;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers
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
        private readonly IBoundaryProvider boundaryProvider;
        private readonly IWaveBoundaryGeometryFactory waveBoundaryGeometryFactory;
        private readonly IAddBehaviour addBehaviour;

        /// <summary>
        /// Creates a new <see cref="BoundaryLineMapFeatureProvider"/>.
        /// </summary>
        /// <param name="boundaryProvider">The boundary container.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <param name="waveBoundaryGeometryFactory">The waveBoundaryGeometryFactory.</param>
        /// <param name="addBehaviour">The add behaviour.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when ay parameter except the <paramref name="coordinateSystem"/> is <c>null</c>.
        /// </exception>
        public BoundaryLineMapFeatureProvider(IBoundaryProvider boundaryProvider,
                                              ICoordinateSystem coordinateSystem,
                                              IWaveBoundaryGeometryFactory waveBoundaryGeometryFactory,
                                              IAddBehaviour addBehaviour)
        {
            Ensure.NotNull(boundaryProvider, nameof(boundaryProvider));
            Ensure.NotNull(waveBoundaryGeometryFactory, nameof(waveBoundaryGeometryFactory));
            Ensure.NotNull(addBehaviour, nameof(addBehaviour));

            CoordinateSystem = coordinateSystem;

            this.addBehaviour = addBehaviour;
            this.waveBoundaryGeometryFactory = waveBoundaryGeometryFactory;

            this.boundaryProvider = boundaryProvider;

            lineFeatures = new MultiIEventedListAdapter<IWaveBoundary, BoundaryLineFeature>(ObtainWaveBoundaryFromFeature,
                                                                                            CreateBoundaryLineFeature);
            lineFeatures.RegisterList(this.boundaryProvider.Boundaries);
            SubscribeToEventing();
        }

        public override IList Features
        {
            get => lineFeatures;
            set => throw new NotSupportedException("Setting the Features to another value is currently not supported.");
        }

        /// <summary>
        /// Execute the provided add behaviour for the given <paramref name="geometry"/>,
        /// and return null.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns>
        ///     <c>null</c>
        /// </returns>
        public override IFeature Add(IGeometry geometry)
        {
            addBehaviour.Execute(geometry);
            // We do not want to return this here, however the interface requires this (but never uses it).
            return null;
        }

        public override bool Add(IFeature feature) =>
            throw new NotSupportedException("This is currently not supported, implement when needed.");

        private Tuple<IWaveBoundary, IEventedList<IWaveBoundary>> ObtainWaveBoundaryFromFeature(BoundaryLineFeature feature) =>
            new Tuple<IWaveBoundary, IEventedList<IWaveBoundary>>(feature.ObservedWaveBoundary,
                                                                  boundaryProvider.Boundaries);

        private BoundaryLineFeature CreateBoundaryLineFeature(IWaveBoundary waveBoundary) =>
            new BoundaryLineFeature()
            {
                ObservedWaveBoundary = waveBoundary,
                Geometry = waveBoundaryGeometryFactory.ConstructBoundaryLineGeometry(waveBoundary)
            };

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

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                UnsubscribeFromEventing();
            }
        }

        #endregion
    }
}