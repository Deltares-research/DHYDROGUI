using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders
{
    /// <summary>
    /// Interface for the readers of Delft INI files.
    /// </summary>
    public interface IDelftIniReader
    {
        /// <summary>
        /// Reads a Delft INI format file from the specified file path.
        /// </summary>
        /// <param name="iniFile"> The file path to the Delft INI file. </param>
        /// <returns> A collection of all parsed Delft INI categories read from file. </returns>
        IList<DelftIniCategory> ReadDelftIniFile(string iniFile);
    }
}