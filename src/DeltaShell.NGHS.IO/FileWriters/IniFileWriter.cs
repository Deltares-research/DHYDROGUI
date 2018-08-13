using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters
{
    public class IniFileWriter : DelftIniWriter
    {
        public void WriteIniFile(IEnumerable<DelftIniCategory> categories, string iniFile, bool writeComments = false, bool append = false)
        {
            OpenOutputFile(iniFile, append);
            try
            {
                foreach (var category in categories)
                {
                    WriteLine("[" + category.Name + "]");
                    foreach (var property in category.Properties)
                    {
                        WriteProperty(property, writeComments);
                    }
                    WriteLine(string.Empty); // (DelftIniWriter does not do this)
                }
            }
            finally
            {
                CloseOutputFile();
            }            
        }
    }
}