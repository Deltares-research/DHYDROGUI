using System.Collections.Generic;
using DeltaShell.NGHS.Common.Logging;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public interface IBoundaryFileReader
    {
        /// <summary>
        /// Parses each lateral sources category from the specified file to a <see cref="ILateralSourceBcCategory"/>.
        /// </summary>
        /// <param name="filePath">The full file path to the boundary conditions file.</param>
        /// <param name="logHandler"> Optional parameter; the log handler to report errors. </param>
        /// <returns> A collection of parsed <see cref="ILateralSourceBcCategory"/>.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="filePath"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown when the file at <paramref name="filePath"/> does not exist.
        /// </exception>
        IEnumerable<ILateralSourceBcCategory> ReadLateralSourcesFromBcFile(string filePath, ILogHandler logHandler = null);
    }
}