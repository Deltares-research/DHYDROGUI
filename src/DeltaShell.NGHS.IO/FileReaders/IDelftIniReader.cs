using System.Collections.Generic;
using DHYDRO.Common.IO.Ini;

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
        /// <returns> A collection of all parsed Delft INI sections read from file. </returns>
        IList<IniSection> ReadDelftIniFile(string iniFile);
    }
}