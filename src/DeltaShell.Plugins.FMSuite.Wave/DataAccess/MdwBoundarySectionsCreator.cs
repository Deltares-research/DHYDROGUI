using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// Creator for creating <see cref="IniSection"/> for wave purposes.
    /// </summary>
    public static class MdwBoundarySectionsCreator
    {
        /// <summary>
        /// Creates and returns a collection of <see cref="IniSection"/> from the wave boundaries in the specified
        /// <paramref name="boundaryContainer"/>
        /// </summary>
        /// <param name="boundaryContainer"> Boundary container of the model definition. </param>
        /// <param name="filesManager"> The files manager. </param>
        /// <returns>A collection of <see cref="IniSection"/>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryContainer"/> or
        /// <paramref name="filesManager"/> is <c>null</c>.
        /// </exception>
        public static IEnumerable<IniSection> CreateSections(IBoundaryContainer boundaryContainer,
                                                             IFilesManager filesManager)
        {
            Ensure.NotNull(boundaryContainer, nameof(boundaryContainer));
            Ensure.NotNull(filesManager, nameof(filesManager));

            if (boundaryContainer.DefinitionPerFileUsed)
            {
                yield return CreateSectionForFromSpectrumFileDefinedBoundaries(boundaryContainer, filesManager);

                yield break;
            }

            foreach (IWaveBoundary boundary in boundaryContainer.Boundaries)
            {
                var boundarySection = new IniSection(KnownWaveSections.BoundarySection);

                boundarySection.AddProperty(KnownWaveProperties.Name, boundary.Name);
                MdwBoundarySectionGeometryExtender.AddNewProperties(boundarySection, boundaryContainer, boundary.GeometricDefinition.SupportPoints);

                var dataComponentVisitor = new SpectrumDataComponentVisitor(boundarySection, filesManager);
                boundary.ConditionDefinition.DataComponent.AcceptVisitor(dataComponentVisitor);

                if (dataComponentVisitor.SpectrumType != SpectrumImportExportType.FromFile)
                {
                    MdwBoundarySectionConditionsExtender.AddNewProperties(boundarySection, boundary.ConditionDefinition);
                }

                yield return boundarySection;
            }
        }

        private static IniSection CreateSectionForFromSpectrumFileDefinedBoundaries(IBoundaryContainer boundaryContainer, IFilesManager filesManager)
        {
            var boundarySection = new IniSection(KnownWaveSections.BoundarySection);
            boundarySection.AddProperty(KnownWaveProperties.Definition, KnownWaveBoundariesFileConstants.SpectrumFileDefinitionType);

            string filePath = boundaryContainer.FilePathForBoundariesPerFile;
            if (filePath == string.Empty)
            {
                // this string should not be empty, because the IniWriter
                // only writes properties with values that are not null or empty.
                boundarySection.AddProperty(KnownWaveProperties.OverallSpecFile, " ");
            }
            else
            {
                filesManager.Add(filePath, s => boundaryContainer.FilePathForBoundariesPerFile = s);
                boundarySection.AddProperty(KnownWaveProperties.OverallSpecFile, Path.GetFileName(filePath));
            }

            return boundarySection;
        }
    }
}