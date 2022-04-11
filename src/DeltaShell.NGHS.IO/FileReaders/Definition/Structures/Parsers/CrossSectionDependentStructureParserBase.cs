using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Base class for parsers of cross-section dependent structures.
    /// </summary>
    public abstract class CrossSectionDependentStructureParserBase : StructureParserBase
    {

        /// <summary>
        /// A collection of cross-section definitions.
        /// </summary>
        protected ICollection<ICrossSectionDefinition> CrossSectionDefinitions { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="CrossSectionDependentStructureParserBase"/>.
        /// </summary>
        /// <param name="structureType">The structure type.</param>
        /// <param name="category">A structure <see cref="IDelftIniCategory"/>.</param>
        /// <param name="crossSectionDefinitions">A collection of cross-section definitions.</param>
        /// <param name="branch">The branch the structure should be imported to.</param>
        /// <param name="structuresFilename">The structures filename.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        protected CrossSectionDependentStructureParserBase(StructureType structureType,
                                                           IDelftIniCategory category,
                                                           ICollection<ICrossSectionDefinition> crossSectionDefinitions,
                                                           IBranch branch, 
                                                           string structuresFilename) 
            : base(structureType, category, branch, structuresFilename)
        {
            Ensure.NotNull(crossSectionDefinitions, nameof(crossSectionDefinitions));

            CrossSectionDefinitions = crossSectionDefinitions;
        }
    }
}