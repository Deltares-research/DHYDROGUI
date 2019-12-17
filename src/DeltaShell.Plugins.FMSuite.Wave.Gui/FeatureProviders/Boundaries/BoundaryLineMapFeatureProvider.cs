using System;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
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
    public class BoundaryLineMapFeatureProvider : Feature2DCollection
    {
        private readonly MultiIEventedListAdapter<IWaveBoundary, BoundaryLineFeature> lineFeatures;
        private readonly IBoundaryContainer boundaryContainer;
        private readonly IWaveBoundaryFactory waveBoundaryFactory;

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
            };
        }

        /// <summary>
        /// Creates a new <see cref="BoundaryLineMapFeatureProvider"/>.
        /// </summary>
        /// <param name="boundaryContainer">The boundary container.</param>
        /// <param name="factory">The factory.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="factory"/> or <paramref name="boundaryContainer"/>
        /// is <c>null</c>.
        /// </exception>
        public BoundaryLineMapFeatureProvider(IBoundaryContainer boundaryContainer, 
                                              IWaveBoundaryFactory factory)
        {
            waveBoundaryFactory = factory ?? throw new ArgumentNullException(nameof(factory));

            this.boundaryContainer = boundaryContainer ?? 
                                     throw new ArgumentNullException(nameof(boundaryContainer));

            lineFeatures = new MultiIEventedListAdapter<IWaveBoundary, BoundaryLineFeature>(ObtainWaveBoundaryFromFeature, 
                                                                                            CreateBoundaryLineFeature);
            lineFeatures.RegisterList(this.boundaryContainer.Boundaries);
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
            throw new NotImplementedException("This has not been implemented yet.");
        }
    }
}