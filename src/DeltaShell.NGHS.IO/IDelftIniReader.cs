using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;

namespace DeltaShell.NGHS.IO
{
    /// <summary>
    /// <see cref="IDelftIniReader"/> defines the interface to read delft ini
    /// files.
    /// </summary>
    public interface IDelftIniReader
    {
        /// <summary>
        /// Reads a Delft .ini format file.
        /// </summary>
        /// <param name="stream"> The <see cref="Stream"/> to read the ini file from. </param>
        /// <param name="filePath"> The path to the file location. </param>
        /// <returns> A collection of <see cref="DelftIniCategory"/> instances. </returns>
        /// <remarks> The stream is implicitly disposed. </remarks>
        IList<DelftIniCategory> ReadDelftIniFile(Stream stream, string filePath);
    }
}