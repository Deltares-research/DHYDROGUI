using System;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    [Serializable]
    public class RelatedTable
    {
        public RelatedTable(){}

        public RelatedTable(string tableName,string foreignKeyColumnName)
        {
            TableName = tableName;
            ForeignKeyColumnName = foreignKeyColumnName;
        }

        public string TableName { get; set; }
        public string ForeignKeyColumnName { get; set; }
    }
}