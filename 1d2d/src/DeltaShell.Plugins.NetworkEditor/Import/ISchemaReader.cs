using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public interface ISchemaReader: IDisposable
    {
        /// <summary>
        /// Path of dBase
        /// </summary>
        string Path{ get; set;}

        /// <summary>
        /// Database file extensions handled by the reader
        /// </summary>
        IEnumerable<string> FileExtensions { get; } 

        /// <summary>
        /// Open dBase connection
        /// </summary>
        void OpenConnection();

        /// <summary>
        /// Close dBase connection
        /// </summary>
        void CloseConnection();

        /// <summary>
        /// Get table names of dBase
        /// </summary>
        IList<string> GetTableNames{get;}

        /// <summary>
        /// Get column names
        /// </summary>
        /// <param name="tableName">Name of the table to get the columns for</param>
        /// <param name="skipBlobs">Skip columnnames for columns that contain blobs</param>
        /// <returns></returns>
        IList<string> GetColumnNames(string tableName, bool skipBlobs = false);

        /// <summary>
        /// Get distinct values of a column
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        IList<string> GetDistinctValues(string tableName, string columnName);

        /// <summary>
        /// Schema reader of relational dBase?
        /// </summary>
        bool IsRelationalDataBase { get; }

    }
}

