using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator
{
    /// <summary>
    /// <see cref="StructureTimFileNameGenerator"/> generates names for tim files
    /// based on a given <see cref="TimeSeries"/> and <see cref="IStructure"/>
    /// </summary>
    public static class StructureTimFileNameGenerator
    {
        /// <summary>
        /// Generate a valid .tim file name with extension given a <see cref="structure"/> and
        /// <see cref="timeSeries"/>.
        /// </summary>
        /// <param name="structure">
        /// The <see cref="IStructure"/> to which the <paramref name="timeSeries"/> belongs.
        /// </param>
        /// <param name="timeSeries">
        /// The <see cref="ITimeSeries"/> for which the file name should be generated.
        /// </param>
        /// <returns>A valid file name with extension.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if any parameter is <c>null</c>.
        /// </exception>
        public static string Generate(IStructure structure,
                                      ITimeSeries timeSeries)
        {
            Ensure.NotNull(structure, nameof(structure));
            Ensure.NotNull(timeSeries, nameof(timeSeries));

            string ReplaceInvalidChar(string acc, char invalidChar) =>
                acc.Replace(invalidChar, '_');

            string fileName = $"{structure.Name}_{timeSeries.Name.ToLowerInvariant()}";

            fileName = Path.GetInvalidFileNameChars().Aggregate(fileName, ReplaceInvalidChar);

            fileName = fileName.Replace(' ', '_');
            return $"{fileName}{FileSuffices.TimFile}";
        }
    }
}