using System;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    public class CsvColumn
    {
        public CsvColumn(int index, string headerText, Type type)
        {
            Index = index;
            Type = type;
            HeaderText = headerText;
        }

        public int Index { get; set; }
        public Type Type { get; set; }
        public string HeaderText { get; set; }
    }
}