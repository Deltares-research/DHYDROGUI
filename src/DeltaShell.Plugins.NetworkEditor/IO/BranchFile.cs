using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Csv;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.IO
{
    public static class BranchFile
    {
        private static bool includesHeader = true;

        public enum BranchTypes
        {
            Unkown = 0, Channel = 1, SewerConnection = 2, Pipe = 3
        }

        private static int GetBranchType(IBranch branch)
        {
            if (branch is IChannel)
            {
                return (int)BranchTypes.Channel;
            }
            if (branch is IPipe)
            {
                return (int)BranchTypes.Pipe;
            }
            if (branch is ISewerConnection)
            {
                return (int)BranchTypes.SewerConnection;
            }

            return (int)BranchTypes.Unkown;
        }

        public static void Write(IEnumerable<IBranch> branches, string filePath)
        {
            var dataTable = CreateDataTable();
            
            // Create rows
            foreach (var branch in branches)
            {
                dataTable.Rows.Add(branch.Name, GetBranchType(branch));
            }

            // Write csv file
            using (var streamWriter = new StreamWriter(filePath))
            {
                var csvString = CommonCsvWriter.WriteToString(dataTable, includesHeader, false, ';');
                streamWriter.Write(csvString);
            }
        }

        public static Dictionary<string, int> Read(string filePath)
        {
            var fileContent = File.ReadAllLines(filePath).ToList();
            fileContent.RemoveAt(0);

            return fileContent.Select(line => line.Split(';'))
                .ToDictionary(lineContent => lineContent[0], lineContent => int.Parse(lineContent[1]));
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
