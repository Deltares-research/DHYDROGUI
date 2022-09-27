using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters
{
    /// <summary>
    /// <see cref="IBcFileWriter"/> specifies the write methods to write to bc files to disk.
    /// </summary>
    public interface IBcFileWriter
    {
        /// <summary>
        /// Writes the categories to the given <paramref name="iniFile"/>.
        /// </summary>
        /// <param name="categories">Data to be written.</param>
        /// <param name="iniFile">Location to write to.</param>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        void WriteBcFile(IEnumerable<IDelftIniCategory> categories, string iniFile);
    }
}