using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders
{
    /// <summary>
    /// <see cref="IDelftBcReader"/> specifies the read methods to read bc files from disk.
    /// </summary>
    public interface IDelftBcReader
    {
        /// <summary>
        /// Reads the file from <see cref="bcFile"/>.
        /// </summary>
        /// <param name="bcFile">Location to read the file from</param>
        /// <returns>
        /// The list of <see cref="DelftBcCategory"/> read from <paramref name="bcFile"/>.
        /// </returns>
        /// <exception cref="IOException"><paramref name="iniFile"/> includes an incorrect or invalid syntax for file name, directory name, or volume label.</exception>
        /// <exception cref="FormatException">When an invalid line was encountered.</exception>
        IList<DelftBcCategory> ReadDelftBcFile(string bcFile);
    }
}