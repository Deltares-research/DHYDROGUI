using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition
{
    public interface IStructureDefinitionReader
    {
        /// <summary>
        /// Creates an <see cref="IStructure1D"/> for an <see cref="IDelftIniCategory"/>
        /// </summary>
        /// <param name="category">Category to parse</param>
        /// <param name="crossSectionDefinitions">CrossSectionDefinitions containing profiles for the structures</param>
        /// <param name="branch">Branch for this structure</param>
        /// <returns>Structure made from category properties (can be null if not a 1D structure)</returns>
        IStructure1D ReadDefinition(IDelftIniCategory category, IList<ICrossSectionDefinition> crossSectionDefinitions,
            IBranch branch);}
}