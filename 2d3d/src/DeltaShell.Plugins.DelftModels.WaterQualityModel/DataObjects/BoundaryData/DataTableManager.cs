using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData
{
    /// <summary>
    /// Manager for boundary data (Loads, model-boundaries) using files on disk in delwaq
    /// format.
    /// </summary>
    public class DataTableManager : Unique<long>, INameable, IItemContainer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DataTableManager));

        private IEventedList<DataTable> dataTables;

        /// <summary>
        /// Flag to keep track if executing <see cref="MoveDataTable"/> or not.
        /// </summary>
        private bool movingDataTables;

        public DataTableManager()
        {
            DataTables = new EventedList<DataTable>();
            Name = "Data Table Manager";
        }

        /// <summary>
        /// The data-tables managed by this manager.
        /// </summary>
        public virtual ICollection<DataTable> DataTables
        {
            get => dataTables;
            protected set // NHibernate
            {
                if (dataTables != null)
                {
                    dataTables.CollectionChanged -= DataTablesOnCollectionChanged;
                }

                dataTables = (IEventedList<DataTable>) value;
                if (dataTables != null)
                {
                    dataTables.CollectionChanged += DataTablesOnCollectionChanged;
                }
            }
        }

        /// <summary>
        /// Location where this manager keeps its files. Settings this value will create
        /// that folder if it doesn't exist yet.
        /// </summary>
        public virtual string FolderPath { get; set; }

        public virtual string Name { get; set; }

        /// <summary>
        /// Moves the datatable up or down in the <see cref="DataTables"/> order
        /// </summary>
        /// <param name="dataTable"> DataTable to move </param>
        /// <param name="up"> Move table up (true) or down (false) </param>
        /// <returns> New index of the datatable </returns>
        public virtual int MoveDataTable(DataTable dataTable, bool up)
        {
            int index = dataTables.IndexOf(dataTable);
            if (index == 0 && up || index == dataTables.Count - 1 && !up)
            {
                return index;
            }

            movingDataTables = true;
            int newIndex = up ? index - 1 : index + 1;
            dataTables.Move(index, newIndex);
            movingDataTables = false;

            return newIndex;
        }

        /// <summary>
        /// Creates the new <see cref="DataTable"/> and appends it to <see cref="DataTables"/>
        /// while creating the required files on disk.
        /// </summary>
        /// <param name="name">
        /// The name of the datatable and used to define the file name for file with contents
        /// specified for <paramref name="tableContents"/>.
        /// </param>
        /// <param name="tableContents"> The data table contents. </param>
        /// <param name="useforFullFilename">
        /// The usefor filename with extension, as referred
        /// to in <paramref name="tableContents"/>.
        /// </param>
        /// <param name="useforContents"> The usefor file contents. </param>
        /// <param name="createNewFileNamesIfExists"> A new file name will be created if the original already exists </param>
        /// <exception cref="System.InvalidOperationException">
        /// <see cref="FolderPath"/> not set to a valid folder-path.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// When a file already exists with the given filename.
        /// </exception>
        public virtual void CreateNewDataTable(string name, string tableContents, string useforFullFilename,
                                               string useforContents, bool createNewFileNamesIfExists = false)
        {
            if (string.IsNullOrWhiteSpace(FolderPath))
            {
                throw new InvalidOperationException(
                    "Requires FolderPath to be set to a valid filepath before calling CreateNewDataTable.");
            }

            FileUtils.CreateDirectoryIfNotExists(FolderPath);

            string dataTableFilePath = GetFilePath($"{name}.tbl", createNewFileNamesIfExists);
            string useforFilePath = GetFilePath(useforFullFilename, createNewFileNamesIfExists);

            var newTable = new DataTable
            {
                Name = GetUniqueName(name, dataTableFilePath),
                DataFile = OpenTextDocument(tableContents, dataTableFilePath),
                SubstanceUseforFile = OpenTextDocument(useforContents, useforFilePath)
            };

            dataTables.Add(newTable);
        }

        /// <summary>
        /// Migrates this data table manager from <see cref="FolderPath"/> to a new target
        /// directory, moving all it's associated files.
        /// </summary>
        /// <param name="path"> The new path. </param>
        public virtual void MigrateTo(string path)
        {
            if (Equals(FolderPath, path))
            {
                return;
            }

            if (DataTables.Any())
            {
                FileUtils.CreateDirectoryIfNotExists(path);
                foreach (DataTable dataTable in DataTables)
                {
                    string destinationPath = Path.Combine(path, Path.GetFileName(dataTable.DataFile.Path));
                    dataTable.DataFile.CopyTo(destinationPath);
                    dataTable.DataFile.SwitchTo(destinationPath);

                    destinationPath = Path.Combine(path, Path.GetFileName(dataTable.SubstanceUseforFile.Path));
                    dataTable.SubstanceUseforFile.CopyTo(destinationPath);
                    dataTable.SubstanceUseforFile.SwitchTo(destinationPath);
                }
            }

            FileUtils.DeleteIfExists(FolderPath);
            FolderPath = path;
        }

        public virtual IEnumerable<object> GetDirectChildren()
        {
            foreach (DataTable dataTable in DataTables)
            {
                yield return dataTable.DataFile;
                yield return dataTable.SubstanceUseforFile;
            }
        }

        /// <summary>
        /// Clears the data related to the data table when a data table is removed.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data. </param>
        private void DataTablesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (movingDataTables || !Equals(sender, dataTables))
            {
                return;
            }

            var dataTable = (DataTable) e.GetRemovedOrAddedItem();
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                dataTable.DataFile.Delete();
                dataTable.SubstanceUseforFile.Delete();

                if (!DataTables.Any())
                {
                    FileUtils.DeleteIfExists(FolderPath);
                }
            }
        }

        private string GetUniqueName(string name, string dataTableFilePath)
        {
            //We only need the unique name if the file does not yet exist. Else we just use the name passed to the method.
            string filter = string.Concat($"{name}" + "({0})");
            if (!File.Exists(dataTableFilePath))
            {
                return name;
            }

            string uniqueName = NamingHelper.GetUniqueName(filter, dataTables, typeof(DataTable));
            log.Warn(string.Format(
                         Resources
                             .DataTableManager_WriteTableContentsToNewTextDocumentFromFile_File___0___already_exists_within_the_database__The_file_that_is_being_imported_will_be_renamed_to___1____Note_that_your_results_may_be_affected_by_the_new_import,
                         name, uniqueName));

            return uniqueName;
        }

        private static TextDocumentFromFile OpenTextDocument(string useforContents, string useforFilePath)
        {
            var useforFile = new TextDocumentFromFile();
            File.WriteAllText(useforFilePath, useforContents);
            useforFile.Open(useforFilePath);
            return useforFile;
        }

        private string GetFilePath(string useforFullFilename, bool createNewFileNamesIfExists)
        {
            string useforFilePath = Path.Combine(FolderPath, useforFullFilename);
            if (!File.Exists(useforFilePath))
            {
                return useforFilePath;
            }

            if (createNewFileNamesIfExists)
            {
                useforFilePath = FileUtils.GetUniqueFileName(useforFullFilename);
            }

            return useforFilePath;
        }
    }
}