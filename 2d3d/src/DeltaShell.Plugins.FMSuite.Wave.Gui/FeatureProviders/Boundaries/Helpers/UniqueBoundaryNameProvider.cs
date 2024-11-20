using System.Linq;
using DelftTools.Utils;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers
{
    /// <summary>
    /// <see cref="IUniqueBoundaryNameProvider"/> implements the method to obtain a unique
    /// boundary name.
    /// </summary>
    /// <seealso cref="IUniqueBoundaryNameProvider"/>
    public class UniqueBoundaryNameProvider : IUniqueBoundaryNameProvider
    {
        /// <summary>
        /// The default boundary name
        /// </summary>
        public const string DefaultBoundaryName = "Boundary";

        private readonly IBoundaryProvider boundaryProvider;

        /// <summary>
        /// Creates a new <see cref="UniqueBoundaryNameProvider"/> with the given
        /// <paramref name="boundaryProvider"/>.
        /// </summary>
        /// <param name="boundaryProvider">The boundary provider.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryProvider"/> is <c>null</c>.
        /// </exception>
        public UniqueBoundaryNameProvider(IBoundaryProvider boundaryProvider)
        {
            Ensure.NotNull(boundaryProvider, nameof(boundaryProvider));
            this.boundaryProvider = boundaryProvider;
        }

        public string GetUniqueName() =>
            boundaryProvider.Boundaries.Any()
                ? NamingHelper.GetUniqueName("Boundary({0})", boundaryProvider.Boundaries)
                : DefaultBoundaryName;
    }
}