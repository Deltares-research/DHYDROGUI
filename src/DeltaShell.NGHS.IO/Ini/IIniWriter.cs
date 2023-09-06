namespace DeltaShell.NGHS.IO.Ini
{
    /// <summary>
    /// <see cref="IIniWriter"/> defines the interface with which
    /// to write INI files given a <see cref="IniData"/> object.
    /// </summary>
    public interface IIniWriter
    {
        /// <summary>
        /// Creates an INI format file at target location.
        /// </summary>
        /// <param name="iniData">Data to be written.</param>
        /// <param name="iniFile">File path to write to.</param>
        /// <param name="writeComment"></param>
        /// <exception cref="System.UnauthorizedAccessException">Access is denied.</exception>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="iniFile"/> is an empty string ("") or contains the name of a system device
        /// (com1, com2, and so on).
        /// </exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="iniFile"/> is null.</exception>
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
        void WriteIniFile(IniData iniData, string iniFile, bool writeComment = true);
    }
}