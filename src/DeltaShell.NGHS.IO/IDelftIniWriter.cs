using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.DelftIniObjects;

namespace DeltaShell.NGHS.IO
{
    /// <summary>
    /// <see cref="IDelftIniWriter"/> defines the interface with which
    /// to write delft .ini files given a set of <see cref="DelftIniCategory"/>.
    /// </summary>
    public interface IDelftIniWriter
    {
        /// <summary>
        /// Creates a Delft .ini format file at target location.
        /// </summary>
        /// <param name="categories">Data to be written.</param>
        /// <param name="iniFile">File path to write to.</param>
        /// <param name="writeComment"></param>
        /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="iniFile"/> is an empty string ("") or contains the name of a system device
        /// (com1, com2, and so on).
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="iniFile"/> is null.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// The specified path is invalid (for example, it is on an unmapped drive).
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must not exceed 248 characters,
        /// and file names must not exceed 260 characters.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// path includes an incorrect or invalid syntax for file name, directory name,
        /// or volume label syntax.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        void WriteDelftIniFile(IEnumerable<DelftIniCategory> categories, string iniFile, bool writeComment = true);
    }
}