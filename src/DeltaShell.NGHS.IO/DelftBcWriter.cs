using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO
{
    public sealed class DelftBcWriter : DelftIniWriter
    {
        public void WriteBcFile(IEnumerable<IDelftIniCategory> categories, string iniFile)
        {
            OpenOutputFile(iniFile,true);
            try
            {
                foreach (var category in categories)
                {
                    var bcCategory = category as DelftBcCategory;
                    if(bcCategory != null && bcCategory.Table.Count == 0) return; 
                    WriteLine("[" + category.Name + "]");
                    foreach (var property in category.Properties)
                    {
                        WriteProperty(property);
                    }
                    if(bcCategory != null) WriteTable(bcCategory.Table);
                    WriteLine(string.Empty);
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteTable(IList<IDelftBcQuantityData> table)
        {
            if (table.Count == 0) return;
            var tableRows = new string[table[0].Values.Count]; // there will be as many rows as there are quantity values
            foreach (var delftBcQuantityData in table)
            {
                WriteProperty(delftBcQuantityData.Quantity);
                WriteProperty(delftBcQuantityData.Unit);

                for (int i = 0; i < delftBcQuantityData.Values.Count; i++)
                {
                    tableRows[i] += delftBcQuantityData.Values[i] + " "; // each row will have as many elements as there are quantities
                }
            }

            foreach (var row in tableRows)
            {
                WriteLine(string.Format("    {0}", row));
            }
        }
    
    }
}