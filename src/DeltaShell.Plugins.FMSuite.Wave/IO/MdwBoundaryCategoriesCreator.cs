using System.Collections.Generic;
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
        /// Creates and returns a <see cref="DelftIniCategory"/> from the data of a wave spatiallyVaryingDataComponent condition.
        /// </summary>
        /// <param name="boundaryContainer"> </param>
        /// <returns>The requested <see cref="DelftIniCategory"/>.</returns>
        public static IEnumerable<DelftIniCategory> CreateCategories(IBoundaryContainer boundaryContainer)
        {
            foreach (IWaveBoundary boundary in boundaryContainer.Boundaries)
            {
                var boundaryCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
                
                boundaryCategory.AddProperty(KnownWaveProperties.Name, boundary.Name);
                MdwBoundaryGeometryPropertiesCreator.AddNewProperties(boundaryCategory, boundaryContainer, boundary.GeometricDefinition.SupportPoints);
                boundaryCategory.AddProperty(KnownWaveProperties.SpectrumSpec, "parametric");
                MdwBoundaryConditionPropertiesCreator.AddNewProperties(boundaryCategory, boundary.ConditionDefinition);

                yield return boundaryCategory;
            }
        }
    }
}