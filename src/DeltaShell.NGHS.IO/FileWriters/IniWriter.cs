using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters
{
    public class IniWriter : NGHSFileBase, IIniWriter
    {
        /// <summary>
        /// Creates an INI formatted file at target location.
        /// </summary>
        /// <param name="iniSections">Data to be written.</param>
        /// <param name="iniFile">File path to write to.</param>
        /// <param name="writeComment"> Optional; whether or not to write the comments. Defaults to <c>true</c>. </param>
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
        public virtual void WriteIniFile(IEnumerable<IniSection> iniSections, string iniFile, bool writeComment = true)
        {
            OpenOutputFile(iniFile);
            try
            {
                var iniSectionList = iniSections.ToList();
                for (var n = 0; n < iniSectionList.Count; n++)
                {
                    var iniSection = iniSectionList[n];
                    WriteLine("[" + iniSection.Name + "]");
                    WriteProperties(writeComment, iniSection);
                    if(n != iniSectionList.Count - 1) WriteLine(string.Empty);
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteProperties(bool writeComment, IniSection iniSection)
        {
            foreach (var property in iniSection.Properties)
            {
                WriteProperty(property, writeComment);
            }
        }

        protected virtual void WriteProperty(IniProperty property, bool writeComment = false)
        {
            if (string.IsNullOrEmpty(property.Value)) return;

            var comment = writeComment && !string.IsNullOrEmpty(property.Comment) ? string.Format("# {0}", property.Comment) : "";
            var line = string.Format("    {0,-22}= {1,-20}{2}", property.Key, property.Value, comment);
            WriteLine(line);
        }
    }
}