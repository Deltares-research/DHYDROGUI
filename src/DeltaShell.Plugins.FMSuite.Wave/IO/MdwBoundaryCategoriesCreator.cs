using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
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
        /// <param name="boundaryContainer"> Boundary container of the model definition. </param>
        /// <param name="filesManager"> The files manager. </param>
        /// <returns>A collection of <see cref="DelftIniCategory"/>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryContainer"/> or
        /// <paramref name="filesManager"/> is <c>null</c>.
        /// </exception>
        public static IEnumerable<DelftIniCategory> CreateCategories(IBoundaryContainer boundaryContainer,
                                                                     IFilesManager filesManager)
        {
            Ensure.NotNull(boundaryContainer, nameof(boundaryContainer));
            Ensure.NotNull(filesManager, nameof(filesManager));

            foreach (IWaveBoundary boundary in boundaryContainer.Boundaries)
            {
                var boundaryCategory = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);

                boundaryCategory.AddProperty(KnownWaveProperties.Name, boundary.Name);
                MdwBoundaryCategoryGeometryExtender.AddNewProperties(boundaryCategory, boundaryContainer, boundary.GeometricDefinition.SupportPoints);

                var dataComponentVisitor = new SpectrumDataComponentVisitor(boundaryCategory, filesManager);
                boundary.ConditionDefinition.DataComponent.AcceptVisitor(dataComponentVisitor);

                if (dataComponentVisitor.SpectrumType != SpectrumImportExportType.FromFile)
                {
                    MdwBoundaryCategoryConditionsExtender.AddNewProperties(boundaryCategory, boundary.ConditionDefinition);
                }

                yield return boundaryCategory;
            }
        }
    }
}