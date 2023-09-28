using System.Collections.Generic;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.Helpers
{
    public class BcIniSection
    {
        public IniSection Section { get; }
        public IList<IBcQuantityData> Table { get; set; }

        public BcIniSection(string iniSectionName)
        {
            Table = new List<IBcQuantityData>();
            Section = new IniSection(iniSectionName);
        }
        
        public BcIniSection(IniSection iniSection)
        {
            Table = new List<IBcQuantityData>();
            Section = iniSection;
        }
    }
}