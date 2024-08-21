using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.Toolbox;
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
            string path = TestHelper.GetCurrentMethodName();
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
            string path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);

            try
            {
                var manager = new DataTableManager {FolderPath = path};

                // call
                manager.CreateNewDataTable("A", "Table contents", "B.usefors", "Usefor contents");

                // assert
                Assert.IsTrue(Directory.Exists(path),
                              "CreateNewDataTable should create folder if it doesn't exist yet.");
                DataTable[] dataTables = manager.DataTables.ToArray();
                Assert.AreEqual(1, dataTables.Length);
                Assert.AreEqual("A", dataTables[0].Name);

                Assert.IsFalse(dataTables[0].DataFile.ReadOnly);
                Assert.IsTrue(dataTables[0].DataFile.IsOpen);
                Assert.AreEqual("Table contents", dataTables[0].DataFile.Content);

                Assert.IsFalse(dataTables[0].SubstanceUseforFile.ReadOnly);
                Assert.IsTrue(dataTables[0].DataFile.IsOpen);
                Assert.AreEqual("Usefor contents", dataTables[0].SubstanceUseforFile.Content);

                string dataTableFilePath = Path.Combine(path, "A.tbl");
                Assert.IsTrue(File.Exists(dataTableFilePath));
                Assert.AreEqual("Table contents", File.ReadAllText(dataTableFilePath));

                string useforFilePath = Path.Combine(path, "B.usefors");
                Assert.IsTrue(File.Exists(useforFilePath));
                Assert.AreEqual("Usefor contents", File.ReadAllText(useforFilePath));
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void CreateNewDataTable_DataTableAlreadyExists_DoesNotThrowArgumentException()
        {
            // setup
            string path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);
            Directory.CreateDirectory(path);

            try
            {
                var manager = new DataTableManager {FolderPath = path};

                manager.CreateNewDataTable("A", "1", "B.usefors", "2");

                // assert
                string fullFilePath = Path.GetFullPath(manager.DataTables.First().DataFile.Path);
                Assert.DoesNotThrow(() => manager.CreateNewDataTable("A", "3", "C.usefors", "4"));

                Assert.IsTrue(File.Exists(Path.Combine(Path.GetDirectoryName(fullFilePath), "C.usefors")));
            }
            finally
            {
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void CreateNewDataTable_SubstanceUseforFileAlreadyExists_DoesNotThrowArgumentException()
        {
            // setup
            string path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);
            Directory.CreateDirectory(path);

            try
            {
                var manager = new DataTableManager {FolderPath = path};

                manager.CreateNewDataTable("A", "1", "B.usefors", "2");

                // assert
                string fullFilePath = Path.GetFullPath(manager.DataTables.First().SubstanceUseforFile.Path);
                Assert.DoesNotThrow(() => manager.CreateNewDataTable("B", "3", "B.usefors", "4"));

                Assert.IsTrue(File.Exists(Path.Combine(Path.GetDirectoryName(fullFilePath), "B.tbl")));
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
            string path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);
            Directory.CreateDirectory(path);

            try
            {
                var manager = new DataTableManager {FolderPath = path};

                manager.CreateNewDataTable("A", "1", "B.usefors", "2");

                // call
                manager.CreateNewDataTable("A", "3", "C.usefors", "4", true);

                // assert
                string fullFilePath = Path.GetFullPath(manager.DataTables.First().DataFile.Path);
                Assert.IsTrue(File.Exists(Path.Combine(Path.GetDirectoryName(fullFilePath), "C.usefors")));
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
            Assert.AreEqual("Requires FolderPath to be set to a valid filepath before calling CreateNewDataTable.",
                            exception.Message);
        }

        [Test]
        public void RemoveDataTable_FakingActionThroughBinding_RemoveCorrespondingFilesFromDisk()
        {
            // setup
            string path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);

            try
            {
                var manager = new DataTableManager {FolderPath = path};
                manager.CreateNewDataTable("A", "Table contents", "B.usefors", "Usefor contents");

                Assert.IsTrue(Directory.Exists(path),
                              "CreateNewDataTable should create folder if it doesn't exist yet.");

                string dataTableFilePath = Path.Combine(path, "A.tbl");
                Assert.IsTrue(File.Exists(dataTableFilePath));

                string useforFilePath = Path.Combine(path, "B.usefors");
                Assert.IsTrue(File.Exists(useforFilePath));

                // call
                var list = (IList<DataTable>) manager.DataTables;
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
            var dataTableManager = new DataTableManager {FolderPath = @"D:\test\"};

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

            var dataTables = new EventedList<DataTable>
            {
                dataTable1,
                dataTable2
            };

            TypeUtils.SetField(dataTableManager, "dataTables", dataTables);

            Assert.AreEqual(dataTable1, dataTableManager.DataTables.ElementAt(0));
            Assert.AreEqual(dataTable2, dataTableManager.DataTables.ElementAt(1));

            dataTableManager.MoveDataTable(dataTable2, true);

            Assert.AreEqual(dataTable2, dataTableManager.DataTables.ElementAt(0));
            Assert.AreEqual(dataTable1, dataTableManager.DataTables.ElementAt(1));
        }

        [Test] // TOOLS-22288
        [TestCase(true)]
        [TestCase(false)]
        public void MoveDataTable_WithFilesOnDisk_FilesShouldRemainOnDisk(bool moveUp)
        {
            // setup
            string path = Path.Combine(Directory.GetCurrentDirectory(), "MoveDataTable");
            FileUtils.DeleteIfExists(path);

            try
            {
                var dataTableManager = new DataTableManager {FolderPath = path};

                dataTableManager.CreateNewDataTable("A", "B", "C.d", "E");
                dataTableManager.CreateNewDataTable("F", "G", "H.i", "J");

                foreach (DataTable dataTable in dataTableManager.DataTables)
                {
                    Assert.IsTrue(File.Exists(dataTable.DataFile.Path),
                                  "Precondition: All datatable-files should remain intact.");
                    Assert.IsTrue(File.Exists(dataTable.SubstanceUseforFile.Path),
                                  "Precondition: All substance usefors-files should remain intact.");
                }

                // call
                DataTable dataTableToMove = moveUp ? dataTableManager.DataTables.Last() : dataTableManager.DataTables.First();
                dataTableManager.MoveDataTable(dataTableToMove, moveUp);

                // assert
                foreach (DataTable dataTable in dataTableManager.DataTables)
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
            Assert.IsNull(manager.FolderPath, "Test precondition: FolderPath should not be set.");

            string path = Path.Combine(Directory.GetCurrentDirectory(), "test");
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "test");
            FileUtils.DeleteIfExists(path);

            string otherPath = Path.Combine(Directory.GetCurrentDirectory(), "other");
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
                DataTable[] dataTables = manager.DataTables.ToArray();
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
            string path = Path.Combine(Directory.GetCurrentDirectory(), "test");
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
                DataTable[] dataTables = manager.DataTables.ToArray();
                Assert.IsTrue(File.Exists(dataTables[0].DataFile.Path));
                Assert.AreEqual(path, Path.GetDirectoryName(dataTables[0].DataFile.Path));
                Assert.AreEqual(dataTableContents, File.ReadAllText(dataTables[0].DataFile.Path));
                Assert.AreEqual(dataTableContents, dataTables[0].DataFile.Content);
                Assert.IsTrue(dataTables[0].DataFile.IsOpen);

                Assert.IsTrue(File.Exists(dataTables[0].SubstanceUseforFile.Path));
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
            string path = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(path);

            try
            {
                var manager = new DataTableManager {FolderPath = path};
                manager.CreateNewDataTable("A", "Table contents", "B.usefors", "Usefor contents");
                manager.CreateNewDataTable("C", "More table contents", "D.usefors", "More usefor contents");

                // call
                object[] childItems = manager.GetDirectChildren().ToArray();

                // assert
                foreach (DataTable dataTable in manager.DataTables)
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

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void Test_CreateNewDataTable_WithSameName_Twice_DataRow_Is_StillCreated_And_WarningMessage_IsThrown()
        {
            const string bacteriaCopy = "bacteria(1)";

            using (var app = CreateApplication())
            {
                app.Run();
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();

                // Initialize Project by saving it.
                string tempDirectory = FileUtils.CreateTempDirectory();
                projectService.SaveProjectAs(Path.Combine(tempDirectory, "WAQ_proj.dsproj"));

                //Initialize WAQ Model and add it to the project.
                var model = new WaterQualityModel();
                project.RootFolder.Items.Add(model);

                projectService.SaveProject();

                //Import hyd file
                string hydPath =
                    TestHelper.GetTestFilePath(
                        @"WaterQualityDataFiles\ImportHydFile\westernscheldt01.hyd");
                var hydImporter = new HydFileImporter();
                var importedItem = hydImporter.ImportItem(hydPath, model) as WaterQualityModel;
                Assert.IsNotNull(importedItem);

                //import CSV file
                string csvPath = TestHelper.GetTestFilePath(
                    @"WaterQualityDataFiles\ImportHydFile\bacteria.csv");
                var csvImporter = new DataTableImporter();
                var dataTableManager = new DataTableManager {FolderPath = tempDirectory};

                object csvFile = csvImporter.ImportItem(csvPath, dataTableManager);
                Assert.IsNotNull(csvFile);

                //Assert rowname now is bacteria1
                Assert.AreEqual("bacteria", dataTableManager.DataTables.Select(table => table.Name).Last());

                //Import CSV file again and assert that only one warning message is thrown
                TestHelper.AssertLogMessageIsGenerated(() => csvImporter.ImportItem(csvPath, dataTableManager), string.Format(
                                                           Resources.DataTableManager_WriteTableContentsToNewTextDocumentFromFile_File___0___already_exists_within_the_database__The_file_that_is_being_imported_will_be_renamed_to___1____Note_that_your_results_may_be_affected_by_the_new_import,
                                                           "bacteria", bacteriaCopy));

                //Assert rowname has been incremented by one and is now bacteria2
                Assert.AreEqual(bacteriaCopy, dataTableManager.DataTables.Select(table => table.Name).Last());
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void Test_CreateNewDataTable_ImportDifferentFile_ThenImportFirstFileAgain_DataRow_Is_StillCreated_And_WarningMessage_IsThrown()
        {
            const string bacteriaCopy = "bacteria(1)";

            using (var app = CreateApplication())
            {
                app.Run();
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();

                // Initialize Project by saving it.
                string tempDirectory = FileUtils.CreateTempDirectory();
                projectService.SaveProjectAs(Path.Combine(tempDirectory, "WAQ_proj.dsproj"));

                //Initialize WAQ Model and add it to the project.
                var model = new WaterQualityModel();
                project.RootFolder.Items.Add(model);

                projectService.SaveProject();

                //Import hyd file
                string hydPath =
                    TestHelper.GetTestFilePath(
                        @"WaterQualityDataFiles\ImportHydFile\westernscheldt01.hyd");
                var hydImporter = new HydFileImporter();
                var importedItem = hydImporter.ImportItem(hydPath, model) as WaterQualityModel;
                Assert.IsNotNull(importedItem);

                //import CSV file
                string csvPath = TestHelper.GetTestFilePath(
                    @"WaterQualityDataFiles\ImportHydFile\bacteria.csv");
                var csvImporter = new DataTableImporter();
                var dataTableManager = new DataTableManager {FolderPath = tempDirectory};

                object csvFile = csvImporter.ImportItem(csvPath, dataTableManager);
                Assert.IsNotNull(csvFile);

                //Assert rowname now is bacteria
                Assert.AreEqual("bacteria", dataTableManager.DataTables.Select(table => table.Name).Last());

                //Import a different csv file
                string diffCsvPath = TestHelper.GetTestFilePath(
                    @"WaterQualityDataFiles\ImportHydFile\bacteria2.csv");

                object diffCsvFile = csvImporter.ImportItem(diffCsvPath, dataTableManager);
                Assert.IsNotNull(diffCsvFile);

                //Import CSV file again and assert that only one warning message is thrown
                TestHelper.AssertLogMessageIsGenerated(() => csvImporter.ImportItem(csvPath, dataTableManager), string.Format(
                                                           Resources.DataTableManager_WriteTableContentsToNewTextDocumentFromFile_File___0___already_exists_within_the_database__The_file_that_is_being_imported_will_be_renamed_to___1____Note_that_your_results_may_be_affected_by_the_new_import,
                                                           "bacteria", bacteriaCopy));

                //Assert row name has been incremented by one and is now bacteria2
                Assert.AreEqual(bacteriaCopy, dataTableManager.DataTables.Select(table => table.Name).Last());
            }
        }

        private static IApplication CreateApplication()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new WaterQualityModelApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new NHibernateDaoApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetCdfApplicationPlugin(),
                new ScriptingApplicationPlugin(),
                new ToolboxApplicationPlugin(),
            };
            return new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
        }
    }
}