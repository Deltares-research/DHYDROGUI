using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Converter used to convert the overall boundaries defined by a spectrum file from <see cref="DelftIniCategory"/> objects
    /// to the <see cref="IBoundariesPerFile"/>.
    /// </summary>
    public static class OverallBoundaryCategoryConverter
    {
        /// <summary>
        /// Converts the overall boundaries defined by a spectrum file from the <paramref name="boundaryCategories"/>
        /// to the <paramref name="boundariesPerFile"/>.
        /// </summary>
        /// <param name="boundariesPerFile">The <see cref="IBoundariesPerFile"/> to set the data on.</param>
        /// <param name="boundaryCategories">The collection of <see cref="DelftIniCategory"/> to get the data from.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any parameter is <c>null</c>.</exception>
        public static void Convert(IBoundariesPerFile boundariesPerFile, IEnumerable<DelftIniCategory> boundaryCategories)
        {
            Ensure.NotNull(boundariesPerFile, nameof(boundariesPerFile));
            Ensure.NotNull(boundaryCategories, nameof(boundaryCategories));

            if (boundaryCategories.Count() != 1)
            {
                return;
            }

            OverallBoundaryMdwBlock boundaryBlock = BoundaryCategoryConverter.ConvertOverallBoundary(boundaryCategories.Single());

            if (boundaryBlock == null)
            {
                return;
            }

            boundariesPerFile.DefinitionPerFileUsed = true;
            boundariesPerFile.FileNameForBoundariesPerFile = boundaryBlock.OverallSpectrumFile;
        }
    }
}