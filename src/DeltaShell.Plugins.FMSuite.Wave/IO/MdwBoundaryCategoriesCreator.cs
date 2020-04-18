using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// Creator for creating <see cref="DelftIniCategory"/> for wave purposes.
    /// </summary>
    public static class MdwBoundaryCategoriesCreator
    {
        /// <summary>
        /// Creates and returns a collection of <see cref="DelftIniCategory"/> from the wave boundaries in the specified
        /// <paramref name="boundaryContainer"/>
        /// </summary>
        /// <param name="boundaryContainer"> Boundary container of the model definition </param>
        /// <returns>A collection of <see cref="DelftIniCategory"/>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryContainer"/>
        /// is <c>null</c>.
        /// </exception>
        public static IEnumerable<DelftIniCategory> CreateCategories(IBoundaryContainer boundaryContainer)
        {
            Ensure.NotNull(boundaryContainer, nameof(boundaryContainer));

            foreach (IWaveBoundary boundary in boundaryContainer.Boundaries)
            {
                var boundaryCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

                boundaryCategory.AddProperty(KnownWaveProperties.Name, boundary.Name);
                MdwBoundaryCategoryGeometryExtender.AddNewProperties(boundaryCategory, boundaryContainer, boundary.GeometricDefinition.SupportPoints);
                boundaryCategory.AddProperty(KnownWaveProperties.SpectrumSpec, "parametric");
                MdwBoundaryCategoryConditionsExtender.AddNewProperties(boundaryCategory, boundary.ConditionDefinition);

                yield return boundaryCategory;
            }
        }
    }
}