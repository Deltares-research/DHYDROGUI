using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// Schema reader using OleDbConnection
    /// </summary>
    public class OleDbSchemaReader: ISchemaReader
    {
        private OleDbConnection connection;
        private OleDbCommand command;
        private const string Provider = "Microsoft.Jet.OLEDB.4.0";

        public string Path { get; set; }

        public IEnumerable<string> FileExtensions
        {
            get { yield return ".mdb"; }
        }

        public void OpenConnection()
        {
            CloseConnection();

            var connectionString = "Provider=" + Provider + ";Data Source=" + Path;
            connection = new OleDbConnection(connectionString);
            connection.Open();
        }

        public void CloseConnection()
        {
            if (connection != null)
            {
                connection.Close();
            }
        }

        public IList<string> GetTableNames
        {
            get
            {
                var dataTable = connection.GetSchema("Tables");

                return
                    dataTable.Rows.Cast<DataRow>()
                        .Select(dataRow => dataRow["TABLE_NAME"].ToString())
                        .Where(
                            tableName =>
                                !tableName.StartsWith("GDB_", StringComparison.OrdinalIgnoreCase) &&
                                !tableName.StartsWith("MSys", StringComparison.OrdinalIgnoreCase) &&
                                !tableName.EndsWith("_SHAPE_Index", StringComparison.OrdinalIgnoreCase))
                        .ToList();
            }
        }

        public virtual IList<string> GetColumnNames(string tableName, bool skipBlobs = false)
        {
            var columnNames = new List<string>();

            command = new OleDbCommand("SELECT * FROM [" + tableName + "]", connection);
            var reader = command.ExecuteReader();
            if (reader == null) return columnNames;

            var schema = reader.GetSchemaTable();
            if (schema == null) return columnNames;

            foreach (DataRow row in schema.Rows)
            {
                // filter columns that can not be used for filtering ()
                var columnType = (Type) row[5];
                if (skipBlobs && columnType == typeof (Byte[])) continue;
                columnNames.Add((string) row[0]);
            }

            reader.Close();
            return columnNames;
        }

        public IList<string> GetDistinctValues(string tableName, string columnName)
        {
            var distinctValues = new List<string>();

            command = new OleDbCommand("SELECT DISTINCT " + tableName + "." + columnName + " FROM " + tableName, connection);
            var reader = command.ExecuteReader();
            if (reader == null) return distinctValues;
            
            while (reader.Read())
            {
                distinctValues.Add(reader.GetValue(0).ToString());
            }
            reader.Close();

            return distinctValues;
        }

        public bool IsRelationalDataBase
        {
            get { return true; }
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}
