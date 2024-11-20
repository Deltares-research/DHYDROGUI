using System.Collections.Generic;
using Deltares.Infrastructure.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters
{
    public class IniFileWriter : IniWriter
    {
        public void WriteIniFile(IEnumerable<IniSection> iniSections, string iniFile, bool writeComments = false, bool append = false)
        {
            OpenOutputFile(iniFile, append);
            try
            {
                foreach (var iniSection in iniSections)
                {
                    WriteLine("[" + iniSection.Name + "]");
                    foreach (var property in iniSection.Properties)
                    {
                        WriteProperty(property, writeComments);
                    }
                    WriteLine(string.Empty); // (IniWriter does not do this)
                }
            }
            finally
            {
                CloseOutputFile();
            }            
        }
    }
}