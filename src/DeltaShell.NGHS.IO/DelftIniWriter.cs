using DeltaShell.NGHS.IO.Ini;

namespace DeltaShell.NGHS.IO
{
    /// <summary>
    /// <see cref="DelftIniWriter"/> implements the interface with which to write
    /// INI files given a set of <see cref="IniSection"/>.
    /// </summary>
    /// <seealso cref="NGHSFileBase"/>
    public class DelftIniWriter : NGHSFileBase, IDelftIniWriter
    {
        /// <inheritdoc cref="IDelftIniWriter"/>
        public virtual void WriteDelftIniFile(IniData iniData, string iniFile, bool writeComment = true)
        {
            OpenOutputFile(iniFile);
            try
            {
                foreach (IniSection section in iniData.Sections)
                {
                    WriteLine("[" + section.Name + "]");
                    foreach (IniProperty property in section.Properties)
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

        protected virtual void WriteProperty(IniProperty property, bool writeComment = false)
        {
            if (string.IsNullOrEmpty(property.Value))
            {
                return;
            }

            string comment = writeComment && !string.IsNullOrEmpty(property.Comment) ? $"\t# {property.Comment}" : string.Empty;
            string line = $"    {property.Key,-22}= {property.Value,-20}{comment}";
            WriteLine(line);
        }
    }
}