using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Boundary
{
    /// <summary>
    /// <see cref="IStructureBoundaryGenerator"/> provides generate methods for boundaries of structures.
    /// </summary>
    public interface IStructureBoundaryGenerator
    {
        /// <summary>
        /// Generate multiple boundaries from <paramref name="structureData"/>.
        /// </summary>
        /// <param name="structureData">Data to generate boundaries from.</param>
        /// <param name="startTime">Start time.</param>
        /// <returns>Generated boundaries</returns>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="structureData"/> is Null.</exception>
        IEnumerable<DelftBcCategory> GenerateBoundaries(IEnumerable<IStructureTimeSeries> structureData, DateTime startTime);

        /// <summary>
        /// Generate single boundary from <paramref name="structureData"/>.
        /// </summary>
        /// <param name="structureName">Name of the structure.</param>
        /// <param name="structureData">Data to generate boundaries from.</param>
        /// <param name="startTime">Start time.</param>
        /// <returns>Generated boundary</returns>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="structureName"/> or <paramref name="structureData"/> is Null.</exception>
        IEnumerable<DelftBcCategory> GenerateBoundary(string structureName, ITimeSeries structureData, DateTime startTime);
    }
}