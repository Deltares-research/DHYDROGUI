using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;

using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects.BoundaryData
{
    [TestFixture]
    public class DataTableManagerTest
    {
        [Test]
        public void DefaultConstructor_ExpectedValues()
        {
            // setup

            // call
            var manager = new DataTableManager();

            // assert
            Assert.IsInstanceOf<IUnique<long>>(manager);
            Assert.IsInstanceOf<INameable>(manager);
            Assert.IsInstanceOf<IItemContainer>(manager);
            Assert.AreEqual("Data Table Manager", manager.Name);
            Assert.IsNull(manager.FolderPath);
            CollectionAssert.IsEmpty(manager.DataTables);
            Assert.IsInstanceOf<IEventedList<DataTable>>(manager.DataTables);
        }

        [Test]
        public void FolderPath_SetValidDirectoryPath_CreatesNewDirectory()
        {
            // setup
            var manager = new DataTableManager();
            var path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);

            try
            {
                // call
                manager.FolderPath = path;

                // assert
                Assert.IsFalse(Directory.Exists(path),
                    "Setting folder path should not immediately create new directory at: {0}", Path.GetFullPath(path));
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }
        
        [Test]
        public void CreateNewTableTest()
        {
            // setup
            var path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);

            try
            {
                var manager = new DataTableManager { FolderPath = path };

                // call
                manager.CreateNewDataTable("A", "Table contents", "B.usefors", "Usefor contents");

                // assert
                Assert.IsTrue(Directory.Exists(path),
                    "CreateNewDataTable should create folder if it doesn't exist yet.");
                var dataTables = manager.DataTables.ToArray();
                Assert.AreEqual(1, dataTables.Length);
                Assert.AreEqual("A", dataTables[0].Name);

                Assert.IsFalse(dataTables[0].DataFile.ReadOnly);
                Assert.IsTrue(dataTables[0].DataFile.IsOpen);
                Assert.AreEqual("Table contents", dataTables[0].DataFile.Content);
                //Assert.AreEqual(, manager.DataTables[0].DataFile.Path); // TODO: What type of path will be returned?

                Assert.IsFalse(dataTables[0].SubstanceUseforFile.ReadOnly);
                Assert.IsTrue(dataTables[0].DataFile.IsOpen);
                Assert.AreEqual("Usefor contents", dataTables[0].SubstanceUseforFile.Content);
                //Assert.AreEqual(, manager.DataTables[0].SubstanceUseforFile.Path); // TODO: What type of path will be returned?

                var dataTableFilePath = Path.Combine(path, "A.tbl");
                Assert.IsTrue(File.Exists(dataTableFilePath));
                Assert.AreEqual("Table contents", File.ReadAllText(dataTableFilePath));

                var useforFilePath = Path.Combine(path, "B.usefors");
                Assert.IsTrue(File.Exists(useforFilePath));
                Assert.AreEqual("Usefor contents", File.ReadAllText(useforFilePath));
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void CreateNewDataTable_DataTableAlreadyExists_ThrowArgumentException()
        {
            // setup
            var path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);
            Directory.CreateDirectory(path);

            try
            {
                var manager = new DataTableManager { FolderPath = path };

                manager.CreateNewDataTable("A", "1", "B.usefors", "2");

                // call
                TestDelegate call = () => manager.CreateNewDataTable("A", "3", "C.usefors", "4");

                // assert
                var exception = Assert.Throws<ArgumentException>(call);
                var fullFilePath = Path.GetFullPath(manager.DataTables.First().DataFile.Path);
                var expectedMessage = string.Format("A datatable named 'A' already exists within the manager at path: {0}",
                    fullFilePath);
                Assert.AreEqual(expectedMessage, exception.Message);

                Assert.IsFalse(File.Exists(Path.Combine(Path.GetDirectoryName(fullFilePath), "C.usefors")));
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void CreateNewDataTable_DataTableAlreadyExists_DoesNotThrowArgumentExceptionWithCreateNew()
        {
            // setup
            var path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);
            Directory.CreateDirectory(path);

            try
            {
                var manager = new DataTableManager { FolderPath = path };

                manager.CreateNewDataTable("A", "1", "B.usefors", "2");

                // call
                manager.CreateNewDataTable("A", "3", "C.usefors", "4", true);

                // assert
                
                var fullFilePath = Path.GetFullPath(manager.DataTables.First().DataFile.Path);
                Assert.IsTrue(File.Exists(Path.Combine(Path.GetDirectoryName(fullFilePath), "C.usefors")));
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void CreateNewDataTable_SubstanceUseforFileAlreadyExists_ThrowArgumentException()
        {
            // setup
            var path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);
            Directory.CreateDirectory(path);

            try
            {
                var manager = new DataTableManager { FolderPath = path };

                manager.CreateNewDataTable("A", "1", "B.usefors", "2");

                // call
                TestDelegate call = () => manager.CreateNewDataTable("B", "3", "B.usefors", "4");

                // assert
                var exception = Assert.Throws<ArgumentException>(call);
                var fullFilePath = Path.GetFullPath(manager.DataTables.First().SubstanceUseforFile.Path);
                var expectedMessage = string.Format("The substance usefor file 'B.usefors' already exists within the manager at path: {0}",
                    fullFilePath);
                Assert.AreEqual(expectedMessage, exception.Message);

                Assert.IsFalse(File.Exists(Path.Combine(Path.GetDirectoryName(fullFilePath), "B.tbl")));
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void CreateNewDataTable_FolderPathNotSet_ThrowInvalidOperationException()
        {
            // setup
            var manager = new DataTableManager();

            // call
            TestDelegate call = () => manager.CreateNewDataTable("", "", "", "");

            // assert
            var exception = Assert.Throws<InvalidOperationException>(call);
            Assert.AreEqual("Requires FolderPath to be set to a valid filepath before calling CreateNewDataTable.", exception.Message);
        }

        [Test]
        public void RemoveDataTable_FakingActionThroughBinding_RemoveCorrespondingFilesFromDisk()
        {
            // setup
            var path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);

            try
            {
                var manager = new DataTableManager { FolderPath = path };
                manager.CreateNewDataTable("A", "Table contents", "B.usefors", "Usefor contents");

                Assert.IsTrue(Directory.Exists(path), "CreateNewDataTable should create folder if it doesn't exist yet.");

                var dataTableFilePath = Path.Combine(path, "A.tbl");
                Assert.IsTrue(File.Exists(dataTableFilePath));

                var useforFilePath = Path.Combine(path, "B.usefors");
                Assert.IsTrue(File.Exists(useforFilePath));

                // call
                var list = (IList<DataTable>)manager.DataTables;
                list.Remove(list[0]);

                // assert
                Assert.IsFalse(File.Exists(dataTableFilePath), ".tbl file should be deleted.");
                Assert.IsFalse(File.Exists(useforFilePath), ".usefors file should be deleted.");
                Assert.IsFalse(Directory.Exists(path), "Empty folder should be removed again.");
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void MoveDataTableOfDataTableManager()
        {
            var dataTableManager = new DataTableManager
            {
                FolderPath = @"D:\test\"
            };

            var mocks = new MockRepository();
            var dataFile1 = mocks.Stub<TextDocumentFromFile>();
            var dataFile2 = mocks.Stub<TextDocumentFromFile>();

            var substanceUseforFile1 = mocks.Stub<TextDocumentFromFile>();
            var substanceUseforFile2 = mocks.Stub<TextDocumentFromFile>();

            mocks.ReplayAll();

            var dataTable1 = new DataTable
                {
                    Name = "Test dataset",
                    IsEnabled = true,
                    DataFile = dataFile1,
                    SubstanceUseforFile = substanceUseforFile1
                };
            var dataTable2 = new DataTable
                {
                    Name = "Test dataset2",
                    IsEnabled = false,
                    DataFile = dataFile2,
                    SubstanceUseforFile = substanceUseforFile2
                };

            var dataTables = new EventedList<DataTable> { dataTable1, dataTable2};

            TypeUtils.SetField(dataTableManager, "dataTables", dataTables);

            Assert.AreEqual(dataTable1, dataTableManager.DataTables.ElementAt(0));
            Assert.AreEqual(dataTable2, dataTableManager.DataTables.ElementAt(1));

            dataTableManager.MoveDataTable(dataTable2, true);

            Assert.AreEqual(dataTable2, dataTableManager.DataTables.ElementAt(0));
            Assert.AreEqual(dataTable1, dataTableManager.DataTables.ElementAt(1));
        }

        [Test]
        [Category(TestCategory.Jira)] // TOOLS-22288
        [TestCase(true)]
        [TestCase(false)]
        public void MoveDataTable_WithFilesOnDisk_FilesShouldRemainOnDisk(bool moveUp)
        {
            // setup
            var path = Path.Combine(Directory.GetCurrentDirectory(), "MoveDataTable");
            FileUtils.DeleteIfExists(path);

            try
            {
                var dataTableManager = new DataTableManager { FolderPath = path };

                dataTableManager.CreateNewDataTable("A", "B", "C.d", "E");
                dataTableManager.CreateNewDataTable("F", "G", "H.i", "J");

                foreach (var dataTable in dataTableManager.DataTables)
                {
                    Assert.IsTrue(File.Exists(dataTable.DataFile.Path),
                        "Precondition: All datatable-files should remain intact.");
                    Assert.IsTrue(File.Exists(dataTable.SubstanceUseforFile.Path),
                        "Precondition: All substance usefors-files should remain intact.");
                }

                // call
                var dataTableToMove = moveUp ? dataTableManager.DataTables.Last() : dataTableManager.DataTables.First();
                dataTableManager.MoveDataTable(dataTableToMove, moveUp);

                // assert
                foreach (var dataTable in dataTableManager.DataTables)
                {
                    Assert.IsTrue(File.Exists(dataTable.DataFile.Path),
                        "All datatable-files should remain intact.");
                    Assert.IsTrue(File.Exists(dataTable.SubstanceUseforFile.Path),
                        "All substance usefors-files should remain intact.");
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }
        
        [Test]
        public void MigrateTo_NoFolderSet_DoNotCreateFolderAtTargetDestination()
        {
            // setup
            var manager = new DataTableManager();
            Assert.IsNull(manager.FolderPath,"Test precondition: FolderPath should not be set.");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "test");
            FileUtils.DeleteIfExists(path);

            try
            {
                // call
                manager.MigrateTo(path);

                // assert
                Assert.IsFalse(Directory.Exists(manager.FolderPath));
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void MigrateTo_FolderDirectorySetAndHasContents_CreateFolderArTargetDestinationAndMoveAllManagedFiles()
        {
            // setup
            var path = Path.Combine(Directory.GetCurrentDirectory(), "test");
            FileUtils.DeleteIfExists(path);

            var otherPath = Path.Combine(Directory.GetCurrentDirectory(), "other");
            FileUtils.DeleteIfExists(otherPath);

            try
            {
                var manager = new DataTableManager();
                manager.FolderPath = path;
                const string dataTableContents = "data table contents";
                const string useforsContents = "usefors contents";
                manager.CreateNewDataTable("A", dataTableContents, "A.usefors", useforsContents);

                // call
                manager.MigrateTo(otherPath);

                // assert
                Assert.IsFalse(Directory.Exists(path));

                Assert.IsTrue(Directory.Exists(otherPath));
                var dataTables = manager.DataTables.ToArray();
                Assert.IsTrue(File.Exists(dataTables[0].DataFile.Path));
                Assert.AreEqual(otherPath, Path.GetDirectoryName(dataTables[0].DataFile.Path));
                Assert.AreEqual(dataTableContents, File.ReadAllText(dataTables[0].DataFile.Path));
                Assert.AreEqual(dataTableContents, dataTables[0].DataFile.Content);
                Assert.IsTrue(dataTables[0].DataFile.IsOpen);

                Assert.IsTrue(File.Exists(dataTables[0].SubstanceUseforFile.Path));
                Assert.AreEqual(otherPath, Path.GetDirectoryName(dataTables[0].SubstanceUseforFile.Path));
                Assert.AreEqual(useforsContents, File.ReadAllText(dataTables[0].SubstanceUseforFile.Path));
                Assert.AreEqual(useforsContents, dataTables[0].SubstanceUseforFile.Content);
                Assert.IsTrue(dataTables[0].SubstanceUseforFile.IsOpen);
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
                FileUtils.DeleteIfExists(otherPath);
            }
        }

        [Test]
        public void MigrateTo_SameFolder_DoNothing()
        {
            // setup
            var path = Path.Combine(Directory.GetCurrentDirectory(), "test");
            FileUtils.DeleteIfExists(path);

            try
            {
                var manager = new DataTableManager();
                manager.FolderPath = path;
                const string dataTableContents = "data table contents";
                const string useforsContents = "usefors contents";
                manager.CreateNewDataTable("A", dataTableContents, "A.usefors", useforsContents);

                // call
                manager.MigrateTo(path);

                // assert
                Assert.IsTrue(Directory.Exists(path));
                var dataTables = manager.DataTables.ToArray();
                Assert.IsTrue(File.Exists(dataTables[0].DataFile.Path));
                Assert.AreEqual(path, Path.GetDirectoryName(dataTables[0].DataFile.Path));
                Assert.AreEqual(dataTableContents, File.ReadAllText(dataTables[0].DataFile.Path));
                Assert.AreEqual(dataTableContents, dataTables[0].DataFile.Content);
                Assert.IsTrue(dataTables[0].DataFile.IsOpen);

                Assert.IsTrue(File.Exists(dataTables[0].SubstanceUseforFile.Path));
                Assert.AreEqual(path, Path.GetDirectoryName(dataTables[0].SubstanceUseforFile.Path));
                Assert.AreEqual(useforsContents, File.ReadAllText(dataTables[0].SubstanceUseforFile.Path));
                Assert.AreEqual(useforsContents, dataTables[0].SubstanceUseforFile.Content);
                Assert.IsTrue(dataTables[0].SubstanceUseforFile.IsOpen);
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void GetDirectChildren_ManagerWithDataTables_ReturnAllDataTableTextDocuments()
        {
            // setup
            var path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);

            try
            {
                var manager = new DataTableManager { FolderPath = path };
                manager.CreateNewDataTable("A", "Table contents", "B.usefors", "Usefor contents");
                manager.CreateNewDataTable("C", "More table contents", "D.usefors", "More usefor contents");

                // call
                var childItems = manager.GetDirectChildren().ToArray();

                // assert
                foreach (var dataTable in manager.DataTables)
                {
                    CollectionAssert.Contains(childItems, dataTable.DataFile);
                    CollectionAssert.Contains(childItems, dataTable.SubstanceUseforFile);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }
    }
}