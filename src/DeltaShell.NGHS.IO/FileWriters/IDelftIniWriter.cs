using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;

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
        /// <param name="categories"> Data to be written. </param>
        /// <param name="iniFile"> File path to write to.</param>
        /// <param name="writeComment"> Optional; whether or not to write the comments. Defaults to <c>true</c>. </param>
        void WriteDelftIniFile(IEnumerable<IDelftIniCategory> categories, string iniFile, bool writeComment = true);
    }
}