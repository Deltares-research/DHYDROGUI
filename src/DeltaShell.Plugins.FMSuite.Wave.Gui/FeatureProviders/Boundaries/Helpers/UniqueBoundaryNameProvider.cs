using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers
{
    /// <summary>
    /// <see cref="IUniqueBoundaryNameProvider"/> implements the method to obtain a unique
    /// boundary name.
    /// </summary>
    /// <seealso cref="IUniqueBoundaryNameProvider" />
    public class UniqueBoundaryNameProvider : IUniqueBoundaryNameProvider
    {
        private readonly IBoundaryContainer boundaryContainer;

        /// <summary>
        /// The default boundary name
        /// </summary>
        public const string DefaultBoundaryName = "Boundary";

        /// <summary>
        /// Creates a new <see cref="UniqueBoundaryNameProvider"/> with the given
        /// <paramref name="boundaryContainer"/>.
        /// </summary>
        /// <param name="boundaryContainer">The boundary container.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryContainer"/> is <c>null</c>.
        /// </exception>
        public UniqueBoundaryNameProvider(IBoundaryContainer boundaryContainer)
        {
            Ensure.NotNull(boundaryContainer, nameof(boundaryContainer));
            this.boundaryContainer = boundaryContainer;
        }

        public string GetUniqueName() =>
            boundaryContainer.Boundaries.Any()
                ? NamingHelper.GetUniqueName("Boundary({0})", boundaryContainer.Boundaries)
                : DefaultBoundaryName;
    }
}