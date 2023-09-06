using System.IO;
using DeltaShell.NGHS.IO.Ini;

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
        /// <returns> An <see cref="IniData"/> instance containing the INI file contents. </returns>
        /// <remarks> The stream is implicitly disposed. </remarks>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="stream"/> does not support reading.
        /// </exception>
        IniData ReadDelftIniFile(Stream stream, string filePath);
    }
}