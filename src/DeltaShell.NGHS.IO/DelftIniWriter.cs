using System.Collections.Generic;
using DeltaShell.NGHS.IO.DelftIniObjects;

namespace DeltaShell.NGHS.IO
{
    /// <summary>
    /// <see cref="DelftIniWriter"/> implements the interface with which to write delft .ini
    /// files given a set of <see cref="DelftIniCategory"/>.
    /// </summary>
    /// <seealso cref="NGHSFileBase"/>
    public class DelftIniWriter : NGHSFileBase, IDelftIniWriter
    {
        /// <inheritdoc cref="IDelftIniWriter"/>
        /// >
        public virtual void WriteDelftIniFile(IEnumerable<DelftIniCategory> categories, string iniFile, bool writeComment = true)
        {
            OpenOutputFile(iniFile);
            try
            {
                foreach (DelftIniCategory category in categories)
                {
                    WriteLine("[" + category.Name + "]");
                    foreach (DelftIniProperty property in category.Properties)
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
            if (string.IsNullOrEmpty(property.Value))
            {
                return;
            }

            string comment = writeComment && !string.IsNullOrEmpty(property.Comment) ? string.Format("\t# {0}", property.Comment) : string.Empty;
            string line = string.Format("    {0,-22}= {1,-20}{2}", property.Name, property.Value, comment);
            WriteLine(line);
        }
    }
}