using System;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    [Serializable]
    public class MappingColumn
    {
        public MappingColumn():this(null, null){}

        public MappingColumn(string tableName, string columnName)
        {
            TableName = tableName;
            ColumnName = columnName;
        }

        public string TableName { get; set; }
        public string ColumnName { get; set; }

        public bool IsNullValue
        {
            get { return string.IsNullOrEmpty(TableName) && string.IsNullOrEmpty(ColumnName); }
        }

        public override string ToString()
        {
            if(!string.IsNullOrEmpty(TableName))
            {
                return TableName + "." + ColumnName;
            }
            return ColumnName;
        }

        public string Alias
        {
            get{
                if (IsNullValue)
                {
                    return null;
                }
                if (!string.IsNullOrEmpty(TableName))
                {
                    return TableName + "_" + ColumnName;
                }
                return ColumnName;
            }
        }
    }
}