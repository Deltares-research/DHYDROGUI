using System.Collections.Generic;
using System.Text;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters
{
    public sealed class BcWriter : IniWriter, IBcWriter
    {
        public void WriteBcFile(IEnumerable<BcIniSection> iniSections, string iniFile)
        {
            OpenOutputFile(iniFile,true);

            try
            {
                WriteBcSectionsToFile(iniSections);
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteBcSectionsToFile(IEnumerable<BcIniSection> iniSections)
        {
            foreach (BcIniSection bcIniSection in iniSections)
            {
                if (IsBcSectionWithoutTableData(bcIniSection))
                {
                    return;
                }

                WriteBcSectionToFile(bcIniSection);
            }
        }

        private static bool IsBcSectionWithoutTableData(BcIniSection bcIniSection)
        {
            if (IsGeneralSection(bcIniSection))
            {
                return false;
            }
            return bcIniSection.Table.Count == 0;
        }

        private void WriteBcSectionToFile(BcIniSection iniSection)
        {
            WriteLine("[" + iniSection.Section.Name + "]");

            foreach (IniProperty property in iniSection.Section.Properties)
            {
                WriteProperty(property);
            }

            if (!IsGeneralSection(iniSection))
            {
                WriteTable(iniSection.Table);
            }

            WriteLine(string.Empty);
        }

        private static bool IsGeneralSection(BcIniSection iniSection)
        {
            return iniSection.Section.IsNameEqualTo(GeneralRegion.IniHeader);
        }

        private void WriteTable(IList<IBcQuantityData> table)
        {
            StringBuilder[] tableRows = new StringBuilder[table[0].Values.Count]; // there will be as many rows as there are quantity values
            InitializeStringBuilderArray(tableRows);

            foreach (IBcQuantityData bcQuantityData in table)
            {
                WriteProperty(bcQuantityData.Quantity);
                WriteProperty(bcQuantityData.Unit);

                InsertValuesInTableRows(bcQuantityData, tableRows); // each row will have as many elements as there are quantities
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

        private static void InsertValuesInTableRows(IBcQuantityData bcQuantityData, StringBuilder[] tableRows)
        {
            for (int i = 0; i < bcQuantityData.Values.Count; i++)
            {
                tableRows[i].Append(bcQuantityData.Values[i]);
                tableRows[i].Append(" ");
            }
        }
    }
}