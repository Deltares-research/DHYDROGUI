using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData
{
    /// <summary>
    /// Manager for boundary data (Loads, model-boundaries) using files on disk in delwaq
    /// format.
    /// </summary>
    public class DataTableManager : Unique<long>, INameable, IItemContainer
    {
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

        private void DataTablesOnCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (movingDataTables || !Equals(sender, dataTables))
            {
                return;
            }

            var dataTable = (DataTable)e.Item;
            if (e.Action == NotifyCollectionChangeAction.Remove)
            {
                dataTable.DataFile.Delete();
                dataTable.SubstanceUseforFile.Delete();

                if (!DataTables.Any())
                {
                    FileUtils.DeleteIfExists(FolderPath);
                }
            }
        }

        public virtual string Name { get; set; }

        /// <summary>
        /// The data-tables managed by this manager.
        /// </summary>
        public virtual ICollection<DataTable> DataTables
        {
            get { return dataTables; }
            protected set // NHibernate
            {
                if (dataTables != null)
                {
                    dataTables.CollectionChanged -= DataTablesOnCollectionChanged;
                }
                dataTables = (IEventedList<DataTable>)value;
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

        /// <summary>
        /// Moves the datatable up or down in the <see cref="DataTables"/> order
        /// </summary>
        /// <param name="dataTable">DataTable to move</param>
        /// <param name="up">Move table up (true) or down (false)</param>
        /// <returns>New index of the datatable</returns>
        public virtual int MoveDataTable(DataTable dataTable, bool up)
        {
            var index = dataTables.IndexOf(dataTable);
            if ((index == 0 && up) || (index == dataTables.Count - 1 && !up)) return index;

            movingDataTables = true;
            var newIndex = up ? index - 1 : index + 1;
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
        /// <param name="tableContents">The data table contents.</param>
        /// <param name="useforFullFilename">The usefor filename with extension, as referred
        /// to in <paramref name="tableContents"/>.</param>
        /// <param name="useforContents">The usefor file contents.</param>
        /// <param name="createNewFileNamesIfExists">A new file name will be created if the original already exists</param>
        /// <exception cref="System.InvalidOperationException">
        /// <see cref="FolderPath"/> not set to a valid folder-path.</exception>
        /// <exception cref="System.ArgumentException">
        /// When a file already exists with the given filename.</exception>
        public virtual void CreateNewDataTable(string name, string tableContents, string useforFullFilename, string useforContents, bool createNewFileNamesIfExists = false)
        {
            if (string.IsNullOrWhiteSpace(FolderPath))
            {
                throw new InvalidOperationException("Requires FolderPath to be set to a valid filepath before calling CreateNewDataTable.");
            }
            FileUtils.CreateDirectoryIfNotExists(FolderPath);

            var dataTableFilePath = Path.Combine(FolderPath, string.Format("{0}.tbl", name));
            if (File.Exists(dataTableFilePath))
            {
                if (createNewFileNamesIfExists)
                {
                    dataTableFilePath = FileUtils.GetUniqueFileName(dataTableFilePath);
                }
                else
                {
                    var message = string.Format(
                        "A datatable named '{0}' already exists within the manager at path: {1}",
                        name, Path.GetFullPath(dataTableFilePath));
                    throw new ArgumentException(message);
                }
            }

            var useforFilePath = Path.Combine(FolderPath, useforFullFilename);
            if (File.Exists(useforFilePath))
            {
                if (createNewFileNamesIfExists)
                {
                    useforFilePath = FileUtils.GetUniqueFileName(useforFullFilename);
                }
                else
                {
                    var message = string.Format("The substance usefor file '{0}' already exists within the manager at path: {1}",
                    useforFullFilename, Path.GetFullPath(useforFilePath));
                    throw new ArgumentException(message);
                }
            }

            File.WriteAllText(dataTableFilePath, tableContents);
            var dataTableFile = new TextDocumentFromFile();
            dataTableFile.Open(dataTableFilePath);

            File.WriteAllText(useforFilePath, useforContents);
            var useforFile = new TextDocumentFromFile();
            useforFile.Open(useforFilePath);

            var newTable = new DataTable
            {
                Name = name, 
                DataFile = dataTableFile, 
                SubstanceUseforFile = useforFile
            };

            dataTables.Add(newTable);
        }

        /// <summary>
        /// Migrates this data table manager from <see cref="FolderPath"/> to a new target
        /// directory, moving all it's associated files.
        /// </summary>
        /// <param name="path">The new path.</param>
        public virtual void MigrateTo(string path)
        {
            if (Equals(FolderPath, path))
            {
                return;
            }

            if (DataTables.Any())
            {
                FileUtils.CreateDirectoryIfNotExists(path);
                foreach (var dataTable in DataTables)
                {
                    var destinationPath = Path.Combine(path, Path.GetFileName(dataTable.DataFile.Path));
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
            foreach (var dataTable in DataTables)
            {
                yield return dataTable.DataFile;
                yield return dataTable.SubstanceUseforFile;
            }
        }
    }
}