using System.Collections.Generic;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.Helpers
{
    public class DelftBcCategory
    {
        public IniSection Section { get; }
        public IList<IDelftBcQuantityData> Table { get; set; }

        public DelftBcCategory(string iniSectionName)
        {
            Table = new List<IDelftBcQuantityData>();
            Section = new IniSection(iniSectionName);
        }
        
        public DelftBcCategory(IniSection iniSection)
        {
            Table = new List<IDelftBcQuantityData>();
            Section = iniSection;
        }
    }
}