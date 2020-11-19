using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// Converter used to convert the domain wide boundaries defined by a spectrum file from <see cref="DelftIniCategory"/>
    /// objects
    /// to the <see cref="IBoundariesPerFile"/>.
    /// </summary>
    public static class DomainWideBoundaryCategoryConverter
    {
        /// <summary>
        /// Method that checks whether the <see cref="boundaryCategories"/> only contain a domain wide boundary category.
        /// </summary>
        /// <param name="boundaryCategories">The boundary categories to check.</param>
        /// <returns><c>true</c> when there is only a single domain wide boundary category defined; <c>false</c> otherwise</returns>
        public static bool IsDomainWideBoundaryCategory(IEnumerable<DelftIniCategory> boundaryCategories)
        {
            Ensure.NotNull(boundaryCategories, nameof(boundaryCategories));

            if (boundaryCategories.Count() != 1)
            {
                return false;
            }

            return boundaryCategories.First().GetEnumValue<DefinitionImportType>(KnownWaveProperties.Definition) == DefinitionImportType.SpectrumFile;
        }

        /// <summary>
        /// Converts the domain wide boundaries defined by a spectrum file from the <paramref name="boundaryCategories"/>
        /// to the <paramref name="boundariesPerFile"/>.
        /// </summary>
        /// <param name="boundariesPerFile">The <see cref="IBoundariesPerFile"/> to set the data on.</param>
        /// <param name="boundaryCategories">The collection of <see cref="DelftIniCategory"/> to get the data from.</param>
        /// <param name="mdwDirPath">The path to the directory where the .mdw file is located.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public static void Convert(IBoundariesPerFile boundariesPerFile, IEnumerable<DelftIniCategory> boundaryCategories, string mdwDirPath)
        {
            Ensure.NotNull(boundariesPerFile, nameof(boundariesPerFile));
            Ensure.NotNull(boundaryCategories, nameof(boundaryCategories));
            Ensure.NotNull(mdwDirPath, nameof(mdwDirPath));

            if (!IsDomainWideBoundaryCategory(boundaryCategories))
            {
                return;
            }

            DomainWideBoundaryMdwBlock domainWideBoundaryBlock = ConvertDomainWideBoundary(boundaryCategories.Single());

            boundariesPerFile.DefinitionPerFileUsed = true;
            boundariesPerFile.FilePathForBoundariesPerFile = !string.IsNullOrEmpty(domainWideBoundaryBlock.DomainWideSpectrumFile)
                                                                 ? Path.Combine(mdwDirPath, domainWideBoundaryBlock.DomainWideSpectrumFile)
                                                                 : string.Empty;
        }

        private static DomainWideBoundaryMdwBlock ConvertDomainWideBoundary(DelftIniCategory boundaryCategory)
        {
            Ensure.NotNull(boundaryCategory, nameof(boundaryCategory));

            if (boundaryCategory.Name != KnownWaveCategories.BoundaryCategory)
            {
                throw new ArgumentException("Category is not an mdw boundary category.", nameof(boundaryCategory));
            }

            return new DomainWideBoundaryMdwBlock {DomainWideSpectrumFile = boundaryCategory.GetPropertyValue(KnownWaveProperties.OverallSpecFile)};
        }

        private static T GetEnumValue<T>(this DelftIniCategory category, string propertyName) => EnumUtils.GetEnumValueByDescription<T>(category.GetPropertyValue(propertyName));
    }
}