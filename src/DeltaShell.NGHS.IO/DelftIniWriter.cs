using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.DelftIniObjects;

namespace DeltaShell.NGHS.IO
{
    public class DelftIniWriter : NGHSFileBase
    {
        /// <summary>
        /// Creates a Delft .ini format file at target location.
        /// </summary>
        /// <param name="categories">Data to be written.</param>
        /// <param name="iniFile">File path to write to.</param>
        /// <param name="writeComment"></param>
        /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="iniFile"/> is an empty string ("") or contains the name of a system device 
        ///   (com1, com2, and so on).
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="iniFile"/> is null.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        ///   The specified path is invalid (for example, it is on an unmapped drive).
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        ///   The specified path, file name, or both exceed the system-defined maximum length. 
        ///   For example, on Windows-based platforms, paths must not exceed 248 characters, 
        ///   and file names must not exceed 260 characters.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        ///   path includes an incorrect or invalid syntax for file name, directory name, 
        ///   or volume label syntax.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        public virtual void WriteDelftIniFile(IEnumerable<DelftIniCategory> categories, string iniFile, bool writeComment = true)
        {
            OpenOutputFile(iniFile);
            try
            {
                foreach (var category in categories)
                {
                    WriteLine("[" + category.Name + "]");
                    foreach (var property in category.Properties)
                    {
                        WriteProperty(property, writeComment);
                    }
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        protected virtual void WriteProperty(DelftIniProperty property, bool writeComment = false)
        {
            if (string.IsNullOrEmpty(property.Value)) return;

            string comment = writeComment && !string.IsNullOrEmpty(property.Comment) ? string.Format("\t# {0}", property.Comment) : string.Empty;
            string line = string.Format("    {0,-22}= {1,-20}{2}", property.Name, property.Value, comment);
            WriteLine(line);
        }
    }
}