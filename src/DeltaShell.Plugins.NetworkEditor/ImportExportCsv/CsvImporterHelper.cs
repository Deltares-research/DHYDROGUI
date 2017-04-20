using System;
using System.Data;
using System.Globalization;
using LumenWorks.Framework.IO.Csv;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    public static class CsvImporterHelper
    {
        public static bool CheckIndexInHeader(IDataReader dataReader, string fieldheader, int index, string identification)
        {
            if (fieldheader.ToUpper().StartsWith(identification.ToUpper()))
            {
                if (index != (((CsvReader)(dataReader))).GetFieldIndex(fieldheader))
                {
                    throw new ArgumentException(string.Format("Expected column header '{0}' at position {1}.",
                                                              fieldheader, index));
                }
                return true;
            }
            return false;
        }

        public static double ParseToDouble(IDataReader dataReader, int index)
        {
            var rawValue = dataReader[index];
            if (null == rawValue)
            {
                return double.NaN;
            }
            var value = rawValue.ToString();
            return value == "" ? double.NaN : double.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}