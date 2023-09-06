using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// Converter used to convert the domain wide boundaries defined by a spectrum file from <see cref="IniSection"/>
    /// objects
    /// to the <see cref="IBoundariesPerFile"/>.
    /// </summary>
    public static class DomainWideBoundarySectionConverter
    {
        /// <summary>
        /// Method that checks whether the <see cref="boundarySections"/> only contain a domain wide boundary section.
        /// </summary>
        /// <param name="boundarySections">The boundary sections to check.</param>
        /// <returns><c>true</c> when there is only a single domain wide boundary section defined; <c>false</c> otherwise</returns>
        public static bool IsDomainWideBoundarySection(IEnumerable<IniSection> boundarySections)
        {
            Ensure.NotNull(boundarySections, nameof(boundarySections));

            if (boundarySections.Count() != 1)
            {
                return false;
            }

            return boundarySections.First().GetEnumValue<DefinitionImportType>(KnownWaveProperties.Definition) == DefinitionImportType.SpectrumFile;
        }

        /// <summary>
        /// Converts the domain wide boundaries defined by a spectrum file from the <paramref name="boundarySections"/>
        /// to the <paramref name="boundariesPerFile"/>.
        /// </summary>
        /// <param name="boundariesPerFile">The <see cref="IBoundariesPerFile"/> to set the data on.</param>
        /// <param name="boundarySections">The collection of <see cref="IniSection"/> to get the data from.</param>
        /// <param name="mdwDirPath">The path to the directory where the .mdw file is located.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public static void Convert(IBoundariesPerFile boundariesPerFile, IEnumerable<IniSection> boundarySections, string mdwDirPath)
        {
            Ensure.NotNull(boundariesPerFile, nameof(boundariesPerFile));
            Ensure.NotNull(boundarySections, nameof(boundarySections));
            Ensure.NotNull(mdwDirPath, nameof(mdwDirPath));

            if (!IsDomainWideBoundarySection(boundarySections))
            {
                return;
            }

            DomainWideBoundaryMdwBlock domainWideBoundaryBlock = ConvertDomainWideBoundary(boundarySections.Single());

            boundariesPerFile.DefinitionPerFileUsed = true;
            boundariesPerFile.FilePathForBoundariesPerFile = !string.IsNullOrEmpty(domainWideBoundaryBlock.DomainWideSpectrumFile)
                                                                 ? Path.Combine(mdwDirPath, domainWideBoundaryBlock.DomainWideSpectrumFile)
                                                                 : string.Empty;
        }

        private static DomainWideBoundaryMdwBlock ConvertDomainWideBoundary(IniSection boundarySection)
        {
            Ensure.NotNull(boundarySection, nameof(boundarySection));

            if (boundarySection.Name != KnownWaveSections.BoundarySection)
            {
                throw new ArgumentException("Section is not an mdw boundary section.", nameof(boundarySection));
            }

            return new DomainWideBoundaryMdwBlock {DomainWideSpectrumFile = boundarySection.GetPropertyValueOrDefault(KnownWaveProperties.OverallSpecFile)};
        }

        private static T GetEnumValue<T>(this IniSection section, string propertyName) => EnumUtils.GetEnumValueByDescription<T>(section.GetPropertyValueOrDefault(propertyName));
    }
}