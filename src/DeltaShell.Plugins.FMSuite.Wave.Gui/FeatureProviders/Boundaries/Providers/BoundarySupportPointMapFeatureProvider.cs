using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Features;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers
{
    /// <summary>
    /// <see cref="BoundarySupportPointMapFeatureProvider"/> is responsible for showing
    /// the appropriate boundary support points given a boundary provider.
    /// It provides the appropriate methods such that these features and their
    /// underlying data can be created through the Map.
    /// </summary>
    /// <seealso cref="SharpMap.Data.Providers.FeatureCollection"/>
    /// <remarks>
    /// This class leverages the <see cref="MultiIEventedListAdapter{TObserved,TDisplayed}"/>
    /// to create the appropriate lists. These classes are necessary to play
    /// nice with the framework, and ensure a good separation of concerns between
    /// view and data. As such, this feature provider can be seen as a view model
    /// for the support point data for the Map.
    /// </remarks>
    public class BoundarySupportPointMapFeatureProvider : Feature2DCollection
    {
        private readonly MultiIEventedListAdapter<SupportPoint, SupportPointFeature> pointFeatures;
        private readonly IBoundaryProvider boundaryProvider;
        private readonly IWaveBoundaryGeometryFactory waveBoundaryGeometryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundarySupportPointMapFeatureProvider"/> class.
        /// </summary>
        /// <param name="boundaryProvider">The boundary provider.</param>
        /// <param name="waveBoundaryGeometryFactory">The geometry factory.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter except the <paramref name="coordinateSystem"/> is <c>null</c>.
        /// </exception>
        public BoundarySupportPointMapFeatureProvider(IBoundaryProvider boundaryProvider,
                                                      ICoordinateSystem coordinateSystem,
                                                      IWaveBoundaryGeometryFactory waveBoundaryGeometryFactory)
        {
            Ensure.NotNull(boundaryProvider, nameof(boundaryProvider));
            Ensure.NotNull(waveBoundaryGeometryFactory, nameof(waveBoundaryGeometryFactory));

            CoordinateSystem = coordinateSystem;

            this.boundaryProvider = boundaryProvider;
            this.waveBoundaryGeometryFactory = waveBoundaryGeometryFactory;

            pointFeatures = new MultiIEventedListAdapter<SupportPoint, SupportPointFeature>(ObtainSupportPointFromFeature,
                                                                                            CreateSupportPointFeature);

            RegisterBoundaries(boundaryProvider.Boundaries);
            SubscribeToEventing();
        }

        public override IList Features
        {
            get => pointFeatures;
            set => throw new NotSupportedException("Setting the Features to another value is currently not supported.");
        }

        public override IFeature Add(IGeometry geometry)
        {
            throw new NotImplementedException("Should be implemented when support points can be added from the User Interface.");
        }

        public override bool Add(IFeature feature)
        {
            throw new NotSupportedException("This is currently not supported, implement when needed.");
        }

        private SupportPointFeature CreateSupportPointFeature(SupportPoint supportPoint)
        {
            return new SupportPointFeature
            {
                ObservedSupportPoint = supportPoint,
                Geometry = waveBoundaryGeometryFactory.ConstructBoundarySupportPoint(supportPoint)
            };
        }

        private static Tuple<SupportPoint, IEventedList<SupportPoint>> ObtainSupportPointFromFeature(SupportPointFeature feature)
        {
            return new Tuple<SupportPoint, IEventedList<SupportPoint>>(feature.ObservedSupportPoint,
                                                                       feature.ObservedSupportPoint.GeometricDefinition.SupportPoints);
        }

        private void SubscribeToEventing()
        {
            pointFeatures.CollectionChanged += OnFeaturesCollectionChanged;
            boundaryProvider.Boundaries.CollectionChanged += OnBoundariesCollectionChanged;
        }

        private void UnsubscribeFromEventing()
        {
            pointFeatures.CollectionChanged -= OnFeaturesCollectionChanged;
            boundaryProvider.Boundaries.CollectionChanged -= OnBoundariesCollectionChanged;
        }

        private void OnFeaturesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireFeaturesChanged();
        }

        private void OnBoundariesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(sender is IEnumerable<IWaveBoundary>))
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    RegisterBoundaries(e.NewItems.Cast<IWaveBoundary>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    DeregisterBoundaries(e.OldItems.Cast<IWaveBoundary>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException($"{e.Action.ToString()} is not supported.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        private void RegisterBoundaries(IEnumerable<IWaveBoundary> waveBoundaries)
        {
            waveBoundaries.ForEach(b => pointFeatures.RegisterList(b.GeometricDefinition.SupportPoints));
        }

        private void DeregisterBoundaries(IEnumerable<IWaveBoundary> waveBoundaries)
        {
            waveBoundaries.ForEach(b => pointFeatures.DeregisterList(b.GeometricDefinition.SupportPoints));
        }

        #region IDisposable


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                DeregisterBoundaries(boundaryProvider.Boundaries);
                UnsubscribeFromEventing();
            }
        }

        #endregion
    }
}