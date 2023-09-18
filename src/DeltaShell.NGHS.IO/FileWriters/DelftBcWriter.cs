using System.Collections.Generic;
using System.Text;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters
{
    public sealed class DelftBcWriter : DelftIniWriter, IBcFileWriter
    {
        public void WriteBcFile(IEnumerable<DelftBcCategory> categories, string iniFile)
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

        private void WriteBcCategoriesToFile(IEnumerable<DelftBcCategory> categories)
        {
            foreach (DelftBcCategory bcCategory in categories)
            {
                if (IsBcCategoryWithoutTableData(bcCategory))
                {
                    return;
                }

                WriteBcCategoryToFile(bcCategory);
            }
        }

        private static bool IsBcCategoryWithoutTableData(DelftBcCategory bcCategory)
        {
            if (IsGeneralSection(bcCategory))
            {
                return false;
            }
            return bcCategory.Table.Count == 0;
        }

        private void WriteBcCategoryToFile(DelftBcCategory category)
        {
            WriteLine("[" + category.Section.Name + "]");

            foreach (IniProperty property in category.Section.Properties)
            {
                WriteProperty(property);
            }

            if (!IsGeneralSection(category))
            {
                WriteTable(category.Table);
            }

            WriteLine(string.Empty);
        }

        private static bool IsGeneralSection(DelftBcCategory category)
        {
            return category.Section.IsNameEqualTo(GeneralRegion.IniHeader);
        }

        private void WriteTable(IList<IDelftBcQuantityData> table)
        {
            StringBuilder[] tableRows = new StringBuilder[table[0].Values.Count]; // there will be as many rows as there are quantity values
            InitializeStringBuilderArray(tableRows);

            foreach (IDelftBcQuantityData delftBcQuantityData in table)
            {
                WriteProperty(delftBcQuantityData.Quantity);
                WriteProperty(delftBcQuantityData.Unit);

                InsertValuesInTableRows(delftBcQuantityData, tableRows); // each row will have as many elements as there are quantities
            }

            foreach (StringBuilder row in tableRows)
            {
                WriteLine($"    {row}");
            }
        }

        private static void InitializeStringBuilderArray(StringBuilder[] tableRows)
        {
            for (int i = 0; i < tableRows.Length; i++)
            {
                tableRows[i] = new StringBuilder();
            }
        }

        private static void InsertValuesInTableRows(IDelftBcQuantityData delftBcQuantityData, StringBuilder[] tableRows)
        {
            for (int i = 0; i < delftBcQuantityData.Values.Count; i++)
            {
                tableRows[i].Append(delftBcQuantityData.Values[i]);
                tableRows[i].Append(" ");
            }
        }
    }
}