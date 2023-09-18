using System.Collections.Generic;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters
{
    /// <summary>
    /// Interface for the writers of Delft INI files.
    /// </summary>
    public interface IDelftIniWriter
    {
        /// <summary>
        /// Creates a Delft .ini format file at target location.
        /// </summary>
        /// <param name="iniSections"> Data to be written. </param>
        /// <param name="iniFile"> File path to write to.</param>
        /// <param name="writeComment"> Optional; whether or not to write the comments. Defaults to <c>true</c>. </param>
        void WriteDelftIniFile(IEnumerable<IniSection> iniSections, string iniFile, bool writeComment = true);
    }
}