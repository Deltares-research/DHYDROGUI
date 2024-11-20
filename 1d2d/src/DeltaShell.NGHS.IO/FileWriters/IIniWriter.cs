using System.Collections.Generic;
using Deltares.Infrastructure.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters
{
    /// <summary>
    /// Interface for the writers of Delft INI files.
    /// </summary>
    public interface IIniWriter
    {
        /// <summary>
        /// Creates an INI formatted file at target location.
        /// </summary>
        /// <param name="iniSections"> Data to be written. </param>
        /// <param name="iniFile"> File path to write to.</param>
        /// <param name="writeComment"> Optional; whether or not to write the comments. Defaults to <c>true</c>. </param>
        void WriteIniFile(IEnumerable<IniSection> iniSections, string iniFile, bool writeComment = true);
    }
}