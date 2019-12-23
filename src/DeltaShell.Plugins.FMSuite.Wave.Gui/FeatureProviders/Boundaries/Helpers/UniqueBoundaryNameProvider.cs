using System;
using System.Collections.Generic;
using System.Linq;
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
        public const string defaultBoundaryName = "Boundary";

        /// <summary>
        /// Creates a new <see cref="UniqueBoundaryNameProvider"/> with the given
        /// <paramref name="boundaryContainer"/>.
        /// </summary>
        /// <param name="boundaryContainer">The boundary container.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="boundaryContainer"/> is <c>null</c>.
        /// </exception>
        public UniqueBoundaryNameProvider(IBoundaryContainer boundaryContainer)
        {
            this.boundaryContainer = boundaryContainer ?? 
                                     throw new ArgumentNullException(nameof(boundaryContainer));
        }

        public string GetUniqueName()
        {
            // TODO: NamingHelper exists, however it makes use of a List instead of
            // TODO a HashSet, furthermore it requires the implementation of INameable.
            // TODO: Verify whether we want to adjust this.
            return boundaryContainer.Boundaries.Any() ? GenerateUniqueBoundaryName() 
                                                      : defaultBoundaryName;
        }

        private string GenerateUniqueBoundaryName()
        {
            var names = new HashSet<string>(boundaryContainer.Boundaries.Select(x => x.Name));

            if (!names.Contains(defaultBoundaryName))
            {
                return defaultBoundaryName;
            }

            const string newNameTemplate = defaultBoundaryName + " ({0})";
            var i = 1;

            string newName;
            do
            {
                newName = string.Format(newNameTemplate, i);
                i += 1;
            } while (names.Contains(newName));

            return newName;
        }
    }
}