using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DelftTools.Utils.Csv;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.IO
{
    public static class BranchTypeFile
    {
        private static bool includesHeader = true;

        public static void Write(IEnumerable<IBranch> sewerConnections, string filePath)
        {
            var dataTable = CreateDataTable();
            
            // Create rows
            foreach (var sewerConnection in sewerConnections)
            {
                dataTable.Rows.Add(sewerConnection.Name, sewerConnection.GetType().Name);
            }

            // Write csv file
            using (var streamWriter = new StreamWriter(filePath))
            {
                var csvString = CommonCsvWriter.WriteToString(dataTable, includesHeader, false, ';');
                streamWriter.Write(csvString);
            }
        }

        public static Dictionary<string, string> Read(string filePath)
        {
            var fileContent = File.ReadAllLines(filePath).ToList();
            fileContent.RemoveAt(0);

            return fileContent.Select(line => line.Split(';'))
                .ToDictionary(lineContent => lineContent[0], lineContent => lineContent[1]);
        }

        private static DataTable CreateDataTable()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("BranchId", typeof(string));
            dataTable.Columns.Add("Type", typeof(string));
            return dataTable;
        }
    }
}
