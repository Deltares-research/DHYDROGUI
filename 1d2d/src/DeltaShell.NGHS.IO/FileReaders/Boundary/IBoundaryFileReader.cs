using System.Collections.Generic;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.NGHS.IO.FileReaders.Boundary
{
    public interface IBoundaryFileReader
    {
        /// <summary>
        /// Parses each lateral sources section from the specified file to a <see cref="ILateralSourceBcSection"/>.
        /// </summary>
        /// <param name="filePath">The full file path to the boundary conditions file.</param>
        /// <param name="logHandler"> Optional parameter; the log handler to report errors. </param>
        /// <returns> A collection of parsed <see cref="ILateralSourceBcSection"/>.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="filePath"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown when the file at <paramref name="filePath"/> does not exist.
        /// </exception>
        IEnumerable<ILateralSourceBcSection> ReadLateralSourcesFromBcFile(string filePath, ILogHandler logHandler = null);
    }
}