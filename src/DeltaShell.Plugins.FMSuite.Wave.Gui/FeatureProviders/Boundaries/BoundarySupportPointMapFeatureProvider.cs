using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Data.Providers;
using IGeometryFactory = DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories.IGeometryFactory;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries
{
    /// <summary>
    /// <see cref="BoundarySupportPointMapFeatureProvider" /> is responsible for showing
    /// the appropriate boundary support points given a boundary container.
    /// It provides the appropriate methods such that these features and their
    /// underlying data can be created through the Map.
    /// </summary>
    /// <seealso cref="SharpMap.Data.Providers.FeatureCollection" />
    /// <remarks>
    /// This class leverages the <see cref="MultiIEventedListAdapter{TObserved,TDisplayed}" />
    /// to create the appropriate lists. These classes are necessary to play
    /// nice with the framework, and ensure a good separation of concerns between
    /// view and data. As such, this feature provider can be seen as a view model
    /// for the line data for the Map.
    /// </remarks>
    public class BoundarySupportPointMapFeatureProvider : Feature2DCollection
    {
        private readonly MultiIEventedListAdapter<SupportPoint, SupportPointFeature> pointFeatures;
        private readonly IBoundaryContainer boundaryContainer;
        private readonly IGeometryFactory geometryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundarySupportPointMapFeatureProvider"/> class.
        /// </summary>
        /// <param name="boundaryContainer">The boundary container.</param>
        /// <param name="geometryFactory">The geometry factory.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public BoundarySupportPointMapFeatureProvider(IBoundaryContainer boundaryContainer,
                                                      IGeometryFactory geometryFactory)
        {
            this.boundaryContainer = boundaryContainer ?? throw new ArgumentNullException(nameof(boundaryContainer));
            this.geometryFactory = geometryFactory ?? throw new ArgumentNullException(nameof(geometryFactory));

            pointFeatures = new MultiIEventedListAdapter<SupportPoint, SupportPointFeature>(ObtainSupportPointFromFeature,
                                                                                            CreateSupportPointFeature);

            RegisterBoundaries(boundaryContainer.Boundaries);
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
                Geometry = geometryFactory.ConstructBoundarySupportPoint(supportPoint)
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
            boundaryContainer.Boundaries.CollectionChanged += OnBoundariesCollectionChanged;
        }

        private void UnsubscribeFromEventing()
        {
            pointFeatures.CollectionChanged -= OnFeaturesCollectionChanged;
            boundaryContainer.Boundaries.CollectionChanged -= OnBoundariesCollectionChanged;
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(e.Action));
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

        public override void Dispose()
        {
            base.Dispose();
            DeregisterBoundaries(boundaryContainer.Boundaries);
            UnsubscribeFromEventing();
        }

        #endregion
    }
}
