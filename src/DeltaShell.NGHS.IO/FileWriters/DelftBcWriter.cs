using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters
{
    public sealed class DelftBcWriter : DelftIniWriter, IBcFileWriter
    {
        public void WriteBcFile(IEnumerable<IDelftIniCategory> categories, string iniFile)
        {
            OpenOutputFile(iniFile,true);

            try
            {
                WriteBcCategoriesToFile(categories);
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteBcCategoriesToFile(IEnumerable<IDelftIniCategory> categories)
        {
            foreach (IDelftIniCategory category in categories)
            {
                var bcCategory = category as DelftBcCategory;
                if (IsBcCategoryWithoutTableData(bcCategory))
                {
                    return;
                }

                WriteBcCategoryToFile(category, bcCategory);
            }
        }

        private static bool IsBcCategoryWithoutTableData(IDelftBcCategory bcCategory)
        {
            return bcCategory != null && bcCategory.Table.Count == 0;
        }

        private void WriteBcCategoryToFile(IDelftIniCategory category, IDelftBcCategory bcCategory)
        {
            WriteLine("[" + category.Name + "]");

            foreach (DelftIniProperty property in category.Properties)
            {
                WriteProperty(property);
            }

            if (bcCategory != null)
            {
                WriteTable(bcCategory.Table);
            }

            WriteLine(string.Empty);
        }

        private void WriteTable(IList<IDelftBcQuantityData> table)
        {
            var tableRows = new string[table[0].Values.Count]; // there will be as many rows as there are quantity values

            foreach (IDelftBcQuantityData delftBcQuantityData in table)
            {
                WriteProperty(delftBcQuantityData.Quantity);
                WriteProperty(delftBcQuantityData.Unit);

                for (int i = 0; i < delftBcQuantityData.Values.Count; i++)
                {
                    tableRows[i] += delftBcQuantityData.Values[i] + " "; // each row will have as many elements as there are quantities
                }
            }

            foreach (string row in tableRows)
            {
                WriteLine($"    {row}");
            }
        }
    }
}