using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.AddBehaviours
{
    /// <summary>
    /// <see cref="BoundaryFromLineAddBehaviour"/> implements the add behaviour
    /// to construct a <see cref="IWaveBoundary"/> from a provided
    /// <see cref="ILineString"/>. When something else is provided, nothing
    /// will be constructed.
    /// </summary>
    /// <seealso cref="IAddBehaviour"/>
    public sealed class BoundaryFromLineAddBehaviour : IAddBehaviour
    {
        private readonly IBoundaryProvider boundaryProvider;
        private readonly IWaveBoundaryFactory waveBoundaryFactory;

        /// <summary>
        /// Creates a new <see cref="BoundaryFromLineAddBehaviour"/>.
        /// </summary>
        /// <param name="boundaryProvider">The boundary provider.</param>
        /// <param name="waveBoundaryFactory">The wave boundary factory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public BoundaryFromLineAddBehaviour(IBoundaryProvider boundaryProvider,
                                            IWaveBoundaryFactory waveBoundaryFactory)
        {
            Ensure.NotNull(boundaryProvider, nameof(boundaryProvider));
            Ensure.NotNull(waveBoundaryFactory, nameof(waveBoundaryFactory));

            this.boundaryProvider = boundaryProvider;
            this.waveBoundaryFactory = waveBoundaryFactory;
        }

        /// <summary>
        /// Constructs a <see cref="IWaveBoundary"/> and add it to the underlying
        /// container, given the provided <paramref name="geometry"/>.
        /// </summary>
        /// <param name="geometry">The geometry of the Feature to add.</param>
        /// <remarks>
        /// When something else than a <see cref="ILineString"/> is provided, or
        /// no valid boundary could be constructed, nothing is changed.
        /// </remarks>
        public void Execute(IGeometry geometry)
        {
            if (!(geometry is ILineString lineString))
            {
                return;
            }

            IWaveBoundary boundary = waveBoundaryFactory.ConstructWaveBoundary(lineString);

            if (boundary == null)
            {
                return;
            }

            boundaryProvider.Boundaries.Add(boundary);
        }
    }
}