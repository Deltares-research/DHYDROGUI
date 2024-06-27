using System.IO;
using Deltares.Infrastructure.IO.Ini;

namespace DeltaShell.NGHS.IO.Ini
{
    /// <summary>
    /// <see cref="IIniReader"/> defines the interface to read INI files.
    /// </summary>
    public interface IIniReader
    {
        /// <summary>
        /// Reads an INI format file.
        /// </summary>
        /// <param name="stream"> The <see cref="Stream"/> to read the ini file from. </param>
        /// <param name="filePath"> The path to the file location. </param>
        /// <returns> An <see cref="IniData"/> instance containing the INI file contents. </returns>
        /// <remarks> The stream is implicitly disposed. </remarks>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="stream"/> does not support reading.
        /// </exception>
        IniData ReadIniFile(Stream stream, string filePath);
    }
}