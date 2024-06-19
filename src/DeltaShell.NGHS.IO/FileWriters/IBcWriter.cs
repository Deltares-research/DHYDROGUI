using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters
{
    /// <summary>
    /// <see cref="IBcWriter"/> specifies the write methods to write to bc files to disk.
    /// </summary>
    public interface IBcWriter
    {
        /// <summary>
        /// Writes the INI sections to the given <paramref name="iniFile"/>.
        /// </summary>
        /// <param name="iniSections">Data to be written.</param>
        /// <param name="iniFile">Location to write to.</param>
        /// <param name="appendToFile"> Whether or not to append to the file. </param>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        void WriteBcFile(IEnumerable<BcIniSection> iniSections, string iniFile, bool appendToFile = false);
    }
}