using System;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers;
using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries
{
    /// <summary>
    /// <see cref="BoundaryMapFeaturesContainer"/> acts as a convenience to
    /// group the different BoundaryMapFeatureProviders that are part of
    /// the <see cref="IWaveBoundary"/> visualisation.
    ///
    /// Upon construction the different factories, and FeatureProviders are
    /// created.
    /// </summary>
    public class BoundaryMapFeaturesContainer
    {
        /// <summary>
        /// Creates a new <see cref="BoundaryMapFeaturesContainer"/>.
        /// </summary>
        /// <param name="boundaryContainer">The boundary container.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="boundaryContainer"/> is <c>null</c>.
        /// </exception>
        public BoundaryMapFeaturesContainer(IBoundaryContainer boundaryContainer,
                                            ICoordinateSystem coordinateSystem)
        {
            Ensure.NotNull(boundaryContainer, nameof(boundaryContainer));

            var parametersFactory = new BoundaryParametersFactory();
            var dataComponentFactory = new BoundaryConditionDataComponentFactory(parametersFactory);

            var waveBoundaryFactory = new WaveBoundaryFactory(boundaryContainer,
                                                              new WaveBoundaryFactoryHelper(dataComponentFactory), 
                                                              new UniqueBoundaryNameProvider(boundaryContainer));
            var geometryFactory = new WaveBoundaryGeometryFactory(boundaryContainer, boundaryContainer);

            BoundaryLineMapFeatureProvider = 
                new BoundaryLineMapFeatureProvider(boundaryContainer,
                                                   coordinateSystem,
                                                   waveBoundaryFactory,
                                                   geometryFactory);
            BoundaryEndPointMapFeatureProvider = 
                new BoundaryEndPointMapFeatureProvider(boundaryContainer, 
                                                       coordinateSystem, 
                                                       geometryFactory);

            SupportPointMapFeatureProvider =
                new BoundarySupportPointMapFeatureProvider(boundaryContainer,
                                                           coordinateSystem,
                                                           geometryFactory);
        }

        /// <summary>
        /// Gets the <see cref="BoundaryLineMapFeatureProvider"/> of this
        /// <see cref="BoundaryMapFeaturesContainer"/>.
        /// </summary>
        /// <value>
        /// The boundary line map feature provider.
        /// </value>
        public BoundaryLineMapFeatureProvider BoundaryLineMapFeatureProvider { get; }

        /// <summary>
        /// Gets the <see cref="BoundaryEndPointMapFeatureProvider"/> of this
        /// <see cref="BoundaryMapFeaturesContainer"/>.
        /// </summary>
        /// <value>
        /// The boundary end point map feature provider.
        /// </value>
        public BoundaryEndPointMapFeatureProvider BoundaryEndPointMapFeatureProvider { get; }

        /// <summary>
        /// Gets the <see cref="SupportPointMapFeatureProvider"/> of this
        /// <see cref="BoundaryMapFeaturesContainer"/>.
        /// </summary>
        /// <value>
        /// The boundary support point map feature provider.
        /// </value>
        public BoundarySupportPointMapFeatureProvider SupportPointMapFeatureProvider { get; }
    }
}