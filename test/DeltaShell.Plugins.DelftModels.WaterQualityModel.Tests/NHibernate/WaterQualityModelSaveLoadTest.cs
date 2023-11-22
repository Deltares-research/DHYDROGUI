using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.NHibernate
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class WaterQualityModelSaveLoadTest
    {
        [Test]
        public void SaveModel_DeleteDataTablePostSave_RemoveDataTableFilesFromDisk()
        {
            // setup
            string currentDirectory = Directory.GetCurrentDirectory();

            const string projectSaveName = "test.dsproj";
            string saveFolderPath = Path.Combine(currentDirectory, "A");
            FileUtils.DeleteIfExists(saveFolderPath);
            Directory.CreateDirectory(saveFolderPath);

            string saveLocation = Path.Combine(saveFolderPath, projectSaveName);

            try
            {
                using (var app = DeltaShellCoreFactory.CreateApplication())
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                    app.Run();
                    app.CreateNewProject();

                    var model = new WaterQualityModel();
                    app.Project.RootFolder.Add(model);

                    model.BoundaryDataManager.CreateNewDataTable("A", "B", "C.d", "E");
                    model.BoundaryDataManager.CreateNewDataTable("F", "G", "H.i", "J");
                    model.LoadsDataManager.CreateNewDataTable("K", "M", "n.o", "P");
                    model.LoadsDataManager.CreateNewDataTable("Q", "R", "S.t", "U");

                    app.SaveProjectAs(saveLocation);

                    Assert.IsTrue(Directory.Exists(model.BoundaryDataManager.FolderPath),
                                  "Test Precondition: The boundary data folder should exist post-save.");
                    Assert.AreEqual(4, Directory.GetFiles(model.BoundaryDataManager.FolderPath).Length,
                                    "Test precondition: The boundary data folder should contain 4 files post-save.");

                    Assert.IsTrue(Directory.Exists(model.LoadsDataManager.FolderPath),
                                  "Test Precondition: the loads data folder should exist post-save.");
                    Assert.AreEqual(4, Directory.GetFiles(model.LoadsDataManager.FolderPath).Length,
                                    "Test Precondition: the loads data folder should contain 4 files post-save.");

                    #region Call & Assert: remove 1st data table -> 1 remains for manager.

                    // call (Fake user deleting the first DataTable in both boundaries and loads in TableView)
                    ((IList<DataTable>) model.BoundaryDataManager.DataTables).RemoveAt(0);
                    ((IList<DataTable>) model.LoadsDataManager.DataTables).RemoveAt(0);

                    // assert
                    Assert.IsTrue(Directory.Exists(model.BoundaryDataManager.FolderPath),
                                  "The boundary data folder should still exist.");
                    string[] filesInBoundaryDataFolder =
                        Directory.GetFiles(model.BoundaryDataManager.FolderPath).Select(Path.GetFileName).ToArray();
                    Assert.AreEqual(2, filesInBoundaryDataFolder.Length,
                                    "The boundary data folder should contain 2 files remaining.");
                    CollectionAssert.Contains(filesInBoundaryDataFolder, "F.tbl");
                    CollectionAssert.Contains(filesInBoundaryDataFolder, "H.i");

                    Assert.IsTrue(Directory.Exists(model.LoadsDataManager.FolderPath),
                                  "The loads data folder should still exist.");
                    string[] filesInLoadsDataFolder =
                        Directory.GetFiles(model.LoadsDataManager.FolderPath).Select(Path.GetFileName).ToArray();
                    Assert.AreEqual(2, filesInLoadsDataFolder.Length,
                                    "The loads data folder should contain 2 files remaining.");
                    CollectionAssert.Contains(filesInLoadsDataFolder, "Q.tbl");
                    CollectionAssert.Contains(filesInLoadsDataFolder, "S.t");

                    #endregion

                    #region Call & Assert: remove last data table -> no data tables remain in manager.

                    // call (Fake user deleting the first DataTable in both boundaries and loads in TableView)
                    ((IList<DataTable>) model.BoundaryDataManager.DataTables).RemoveAt(0);
                    ((IList<DataTable>) model.LoadsDataManager.DataTables).RemoveAt(0);

                    // assert
                    Assert.IsFalse(Directory.Exists(model.BoundaryDataManager.FolderPath),
                                   "The boundary data folder should now be deleted as all DataTables have been removed.");
                    Assert.IsFalse(Directory.Exists(model.LoadsDataManager.FolderPath),
                                   "The loads data folder should now be deleted as all DataTables have been removed.");

                    #endregion
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(saveFolderPath);
            }
        }

        [Test]
        public void SaveModel_WithDataTables_DataTablesShouldBeMovedFromTempToSavedLocation()
        {
            // setup
            string currentDirectory = Directory.GetCurrentDirectory();

            const string projectSaveName = "test.dsproj";
            string saveFolderPath = Path.Combine(currentDirectory, "A");
            FileUtils.DeleteIfExists(saveFolderPath);
            Directory.CreateDirectory(saveFolderPath);

            string saveLocation = Path.Combine(saveFolderPath, projectSaveName);

            string workingDirectoryPath = Path.Combine(Path.GetTempPath(), "DeltaShell_Working_Directory");
            ApplicationSettingsBase userSettings =
                ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);
            try
            {
                var app = DeltaShellCoreFactory.CreateApplication();
                app.UserSettings = userSettings;
                using (app)
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                    app.Run();
                    app.CreateNewProject();

                    var model = new WaterQualityModel();
                    app.Project.RootFolder.Add(model);

                    model.BoundaryDataManager.CreateNewDataTable("A", "B", "C.d", "E");
                    model.LoadsDataManager.CreateNewDataTable("F", "G", "H.i", "j");

                    string originalBoundaryDataFolder = model.BoundaryDataManager.FolderPath;
                    string originalLoadsDataFolder = model.LoadsDataManager.FolderPath;

                    Assert.IsTrue(Directory.Exists(originalBoundaryDataFolder));
                    Assert.AreEqual(2, Directory.GetFiles(originalBoundaryDataFolder).Length);

                    Assert.IsTrue(Directory.Exists(originalLoadsDataFolder));
                    Assert.AreEqual(2, Directory.GetFiles(originalLoadsDataFolder).Length);

                    // call
                    app.SaveProjectAs(saveLocation);

                    // assert
                    string projectDataDirectory = saveLocation + "_data";
                    Assert.AreEqual(Path.Combine(projectDataDirectory, model.Name.Replace(" ", "_")), model.ModelDataDirectory);
                    Assert.AreEqual(Path.Combine(model.ModelDataDirectory, "output"), model.ModelSettings.OutputDirectory);

                    Assert.IsFalse(Directory.Exists(originalBoundaryDataFolder),
                                   "Original data table folder should have been moved to new location.");
                    Assert.IsFalse(Directory.Exists(originalLoadsDataFolder),
                                   "Original data table folder should have been moved to new location.");

                    string postSaveBoundaryDataFolder = model.BoundaryDataManager.FolderPath;
                    string postSaveLoadsDataFolder = model.LoadsDataManager.FolderPath;

                    Assert.IsTrue(Directory.Exists(postSaveBoundaryDataFolder));
                    Assert.AreEqual(2, Directory.GetFiles(postSaveBoundaryDataFolder).Length);

                    Assert.IsTrue(Directory.Exists(postSaveLoadsDataFolder));
                    Assert.AreEqual(2, Directory.GetFiles(postSaveLoadsDataFolder).Length);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(saveFolderPath);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveLoadModelTest()
        {
            // setup
            string currentDirectory = Directory.GetCurrentDirectory();

            const string projectSaveName = "test.dsproj";
            string saveFolderPath = Path.Combine(currentDirectory, "A");
            FileUtils.DeleteIfExists(saveFolderPath);
            Directory.CreateDirectory(saveFolderPath);
            string saveLocation = Path.Combine(saveFolderPath, projectSaveName);

            string workingDirectoryPath = Path.Combine(Path.GetTempPath(), "DeltaShell_Working_Directory");
            ApplicationSettingsBase userSettings =
                ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);
            try
            {
                var app = DeltaShellCoreFactory.CreateApplication();
                app.UserSettings = userSettings;
                using (app)
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                    app.Run();
                    app.CreateNewProject();

                    var model = new WaterQualityModel();
                    app.Project.RootFolder.Add(model);

                    model.BoundaryDataManager.CreateNewDataTable("A", "B", "C.d", "E");
                    model.LoadsDataManager.CreateNewDataTable("F", "G", "H.i", "j");

                    string originalBoundaryDataFolder = model.BoundaryDataManager.FolderPath;
                    Assert.IsTrue(Directory.Exists(originalBoundaryDataFolder));
                    string originalLoadDataFolder = model.LoadsDataManager.FolderPath;
                    Assert.IsTrue(Directory.Exists(originalLoadDataFolder));

                    // call
                    string savePath = Path.Combine(saveLocation);
                    app.SaveProjectAs(savePath);
                    app.CloseProject();
                    app.OpenProject(saveLocation);

                    // assert
                    List<WaterQualityModel> models = app.Project.RootFolder.Models.OfType<WaterQualityModel>().ToList();

                    Assert.AreEqual(1, models.Count);

                    WaterQualityModel retrievedModel = models[0];

                    string projectDataDir = app.HybridProjectRepository.ProjectDataDirectory;

                    StringAssert.StartsWith(saveLocation, projectDataDir);
                    StringAssert.StartsWith(projectDataDir, retrievedModel.ModelSettings.OutputDirectory);

                    StringAssert.StartsWith(projectDataDir, retrievedModel.BoundaryDataManager.FolderPath);
                    DataTable boundaryDataTable = retrievedModel.BoundaryDataManager.DataTables.First();
                    StringAssert.StartsWith(projectDataDir, boundaryDataTable.DataFile.Path);
                    StringAssert.StartsWith(projectDataDir, boundaryDataTable.SubstanceUseforFile.Path);

                    StringAssert.StartsWith(projectDataDir, retrievedModel.LoadsDataManager.FolderPath);
                    DataTable loadsDataTable = retrievedModel.LoadsDataManager.DataTables.First();
                    StringAssert.StartsWith(projectDataDir, loadsDataTable.DataFile.Path);
                    StringAssert.StartsWith(projectDataDir, loadsDataTable.SubstanceUseforFile.Path);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(saveFolderPath);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveModel_RemoveHydFile_LoadModel_FiresHydNotFound()
        {
            // copy hyd file and related files
            string dataDir = TestHelper.GetTestDataDirectory();
            string squareHydFile = Path.Combine(dataDir, "IO", "square", "square.hyd");
            string localHydFile = TestHelper.CreateLocalCopy(squareHydFile);

            try
            {
                // start deltashell
                using (var app = DeltaShellCoreFactory.CreateApplication())
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());

                    var notFoundWasFired = false;
                    var waqPlugin = new WaterQualityModelApplicationPlugin();
                    waqPlugin.HydFileNotFoundGuiHandler = delegate { notFoundWasFired = true; };
                    app.Plugins.Add(waqPlugin);

                    app.Run();
                    app.CreateNewProject();

                    // create a model
                    var model = new WaterQualityModel();

                    new HydFileImporter().ImportItem(localHydFile, model);

                    app.Project.RootFolder.Add(model);

                    // save it
                    string savePath = Path.Combine(Path.GetDirectoryName(localHydFile), "savedProject",
                                                   "project1.dsproj");
                    app.SaveProjectAs(savePath);
                    app.CloseProject();

                    // remove the hyd file
                    File.Delete(localHydFile);

                    // load the model, what should the hyd file do?
                    app.OpenProject(savePath);

                    List<WaterQualityModel> models = app.Project.RootFolder.Models.OfType<WaterQualityModel>().ToList();

                    Assert.AreEqual(1, models.Count);

                    WaterQualityModel waqModel = models[0];

                    Assert.IsTrue(waqModel.HasEverImportedHydroData);
                    Assert.IsFalse(waqModel.HasHydroDataImported);
                    Assert.IsTrue(waqModel.Grid.IsEmpty);
                    Assert.IsTrue(notFoundWasFired, "The hyd file was not there, but the handler didn't fire.");
                }
            }
            finally
            {
                // cleanup all files after test has ran.
                string dir = Path.GetDirectoryName(localHydFile);
                FileUtils.DeleteIfExists(dir);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveLoad_MoveProjectAndProjectData_FilePathsShouldBeUpdated()
        {
            // setup
            string currentDirectory = Directory.GetCurrentDirectory();

            const string projectSaveName = "test.dsproj";
            string saveFolderPath = Path.Combine(currentDirectory, "A");
            FileUtils.DeleteIfExists(saveFolderPath);
            Directory.CreateDirectory(saveFolderPath);
            string saveLocation = Path.Combine(saveFolderPath, projectSaveName);

            string moveFolderPath = Path.Combine(currentDirectory, "B");
            FileUtils.DeleteIfExists(moveFolderPath);
            Directory.CreateDirectory(moveFolderPath);
            string moveLocation = Path.Combine(moveFolderPath, projectSaveName);

            string workingDirectoryPath = Path.Combine(Path.GetTempPath(), "DeltaShell_Working_Directory");
            ApplicationSettingsBase application =
                ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);
            try
            {
                var app = DeltaShellCoreFactory.CreateApplication();
                app.UserSettings = application;
                using (app)
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                    app.Run();
                    app.CreateNewProject();

                    var model = new WaterQualityModel();
                    app.Project.RootFolder.Add(model);

                    model.BoundaryDataManager.CreateNewDataTable("A", "B", "C.d", "E");
                    model.LoadsDataManager.CreateNewDataTable("F", "G", "H.i", "j");

                    // call
                    string savePath = Path.Combine(saveLocation);
                    app.SaveProjectAs(savePath);
                    app.CloseProject();

                    FileUtils.CopyAll(new DirectoryInfo(saveFolderPath), new DirectoryInfo(moveFolderPath), null);

                    app.OpenProject(moveLocation);

                    // assert
                    List<WaterQualityModel> models = app.Project.RootFolder.Models.OfType<WaterQualityModel>().ToList();

                    Assert.AreEqual(1, models.Count);

                    WaterQualityModel retrievedModel = models[0];

                    string projectDataDir = app.HybridProjectRepository.ProjectDataDirectory;

                    StringAssert.StartsWith(moveLocation, projectDataDir);
                    StringAssert.StartsWith(projectDataDir, retrievedModel.ModelSettings.OutputDirectory);

                    StringAssert.StartsWith(projectDataDir, retrievedModel.BoundaryDataManager.FolderPath);
                    DataTable boundaryDataTable = retrievedModel.BoundaryDataManager.DataTables.First();
                    StringAssert.StartsWith(projectDataDir, boundaryDataTable.DataFile.Path);
                    StringAssert.StartsWith(projectDataDir, boundaryDataTable.SubstanceUseforFile.Path);

                    StringAssert.StartsWith(projectDataDir, retrievedModel.LoadsDataManager.FolderPath);
                    DataTable loadsDataTable = retrievedModel.LoadsDataManager.DataTables.First();
                    StringAssert.StartsWith(projectDataDir, loadsDataTable.DataFile.Path);
                    StringAssert.StartsWith(projectDataDir, loadsDataTable.SubstanceUseforFile.Path);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(saveFolderPath);
                FileUtils.DeleteIfExists(moveFolderPath);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void SaveModel_ChangeHydFileOtherData_LoadModel_OutputRemoved()
        {
            const string hydFileName = "FlowFM.hyd";
            const string hydFileDifferentTimeStep = "FlowFM_otherTimestep.hyd";
            
            // copy hyd file and related files
            string dataDir = TestHelper.GetTestDataDirectory();
            string hydFile = Path.Combine(dataDir, "ValidWaqModels", "FM", hydFileName);

            using (var tempDirectory = new TemporaryDirectory())
            {
                string workingDirectoryPath = Path.Combine(tempDirectory.Path, "DeltaShell_Working_Directory");
                ApplicationSettingsBase userSettings = ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);
                
                var app = DeltaShellCoreFactory.CreateApplication();
                app.UserSettings = userSettings;
                using (app)
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                    app.Run();
                    app.CreateNewProject();

                    string hydFileDirectoryInTemp = tempDirectory.CopyDirectoryToTempDirectory(Path.GetDirectoryName(hydFile));
                    string hydFileInTemp = Path.Combine(hydFileDirectoryInTemp, hydFileName);
                    string localHydFileOtherTimestep = Path.Combine(hydFileDirectoryInTemp, hydFileDifferentTimeStep);

                    // create a model
                    var model = new WaterQualityModel();
                    model.SetWorkingDirectoryInModelSettings(() => app.WorkDirectory);

                    new HydFileImporter().ImportItem(hydFileInTemp, model);

                    app.Project.RootFolder.Add(model);

                    new SubFileImporter().Import(model.SubstanceProcessLibrary,
                                                 Path.Combine(dataDir, "ValidWaqModels", "coli_04.sub"));

                    app.RunActivity(model);

                    Assert.IsTrue(model.Status == ActivityStatus.Cleaned, $"Actual ActivityStatus was: {model.Status}");
                    Assert.IsFalse(model.OutputIsEmpty);

                    // save it
                    string savePath = Path.Combine(hydFileDirectoryInTemp, "savedProject", "project1.dsproj");
                    app.SaveProjectAs(savePath);
                    app.CloseProject();

                    // remove the hyd file
                    File.Delete(hydFileInTemp);
                    File.Move(localHydFileOtherTimestep, hydFileInTemp);

                    // load the model, what should the hyd file do?
                    app.OpenProject(savePath);

                    List<WaterQualityModel> models = app.Project.RootFolder.Models.OfType<WaterQualityModel>().ToList();

                    Assert.AreEqual(1, models.Count);

                    WaterQualityModel waqModel = models[0];

                    Assert.IsTrue(waqModel.HasEverImportedHydroData);
                    Assert.IsTrue(waqModel.HasHydroDataImported);
                    Assert.IsFalse(waqModel.Grid.IsEmpty);

                    Assert.IsTrue(waqModel.OutputIsEmpty);
                    Assert.IsTrue(waqModel.GetOutputCoverages().All(c => c.Components[0].Values.Count == 0));
                }
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveLoadModelWithBoundaryAliases()
        {
            // copy hyd file and related files
            string dataDir = TestHelper.GetTestDataDirectory();
            string squareHydFile = Path.Combine(dataDir, "IO", "real", "uni3d.hyd");
            string localHydFile = TestHelper.CreateLocalCopy(squareHydFile);

            try
            {
                // start deltashell
                using (var app = DeltaShellCoreFactory.CreateApplication())
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                    app.Run();
                    app.CreateNewProject();

                    // create a model
                    var model = new WaterQualityModel();

                    new HydFileImporter().ImportItem(localHydFile, model);
                    const string locationAliases = "blabla";
                    model.Boundaries[0].LocationAliases = locationAliases;

                    app.Project.RootFolder.Add(model);

                    // save it
                    string savePath = Path.Combine(Path.GetDirectoryName(localHydFile), "savedProject",
                                                   "project1.dsproj");
                    app.SaveProjectAs(savePath);
                    app.CloseProject();

                    // load the model, what should the hyd file do?
                    app.OpenProject(savePath);

                    List<WaterQualityModel> models = app.Project.RootFolder.Models.OfType<WaterQualityModel>().ToList();

                    Assert.AreEqual(1, models.Count);

                    WaterQualityModel waqModel = models[0];

                    Assert.AreEqual(locationAliases, waqModel.Boundaries[0].LocationAliases);
                }
            }
            finally
            {
                // cleanup all files after test has ran.
                string dir = Path.GetDirectoryName(localHydFile);
                FileUtils.DeleteIfExists(dir);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void Check_That_Property_ImportedSubstanceFilePath_Persists_After_SaveLoad_Model()
        {
            // copy hyd file and related files
            string dataDir = TestHelper.GetTestDataDirectory();
            string squareHydFile = Path.Combine(dataDir, "IO", "real", "uni3d.hyd");
            string localHydFile = TestHelper.CreateLocalCopy(squareHydFile);

            try
            {
                // start deltashell
                using (var app = DeltaShellCoreFactory.CreateApplication())
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                    app.Run();
                    app.CreateNewProject();

                    // create a model
                    var originalWaqModel = new WaterQualityModel();

                    // add to project
                    app.Project.RootFolder.Items.Add(originalWaqModel);

                    SubstanceProcessLibrary library = originalWaqModel.SubstanceProcessLibrary;
                    library.ImportedSubstanceFilePath = null;

                    //Import 
                    new SubFileImporter().Import(library, Path.Combine(TestHelper.GetTestDataDirectory(), "ValidWaqModels", "Eutrof_simple_sobek.sub"));

                    // save it and close
                    string savePath = Path.Combine(Path.GetDirectoryName(localHydFile), "savedProject", "project1.dsproj");
                    app.SaveProjectAs(savePath);
                    app.CloseProject();

                    // load the model
                    app.OpenProject(savePath);

                    WaterQualityModel reopenedWaqModel = app.Project.RootFolder.Models.OfType<WaterQualityModel>().FirstOrDefault();
                    Assert.NotNull(reopenedWaqModel);

                    // Expect that the ImportedSubstanceFilePath is not null
                    Assert.IsNotNull(reopenedWaqModel.SubstanceProcessLibrary.ImportedSubstanceFilePath);
                }
            }
            finally
            {
                // cleanup all files after test has ran.
                string dir = Path.GetDirectoryName(localHydFile);
                FileUtils.DeleteIfExists(dir);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void OnProjectOpenedTestWaqModelWithHydFileShouldReimportThatFile()
        {
            // copy hyd file and related files
            string dataDir = TestHelper.GetTestDataDirectory();
            string squareHydFile = Path.Combine(dataDir, "IO", "real", "uni3d.hyd");
            string localHydFile = TestHelper.CreateLocalCopy(squareHydFile);

            try
            {
                // start deltashell
                using (var app = DeltaShellCoreFactory.CreateApplication())
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                    app.Run();
                    app.CreateNewProject();

                    // create a model
                    var model = new WaterQualityModel();

                    new HydFileImporter().ImportItem(localHydFile, model);
                    const string locationAliases = "blabla";
                    model.Boundaries[0].LocationAliases = locationAliases;

                    app.Project.RootFolder.Add(model);

                    // save it
                    string savePath = Path.Combine(Path.GetDirectoryName(localHydFile), "savedProject", "project1.dsproj");
                    app.SaveProjectAs(savePath);
                    app.CloseProject();

                    // load the model, what should the hyd file do?
                    app.OpenProject(savePath);

                    // check if private method ReimportHydFileForWaterQualityModel is called!

                    List<WaterQualityModel> models = app.Project.RootFolder.Models.OfType<WaterQualityModel>().ToList();

                    Assert.AreEqual(1, models.Count);

                    WaterQualityModel waqModel = models[0];

                    Assert.AreEqual(locationAliases, waqModel.Boundaries[0].LocationAliases);
                    // assert
                    UnstructuredGrid grid = waqModel.Grid;
                    Assert.IsFalse(grid.IsEmpty);
                    Assert.AreEqual(619190.8086686889d, grid.Vertices[grid.Cells[0].VertexIndices[0]].X);
                    Assert.AreEqual(4212559.096632215d, grid.Vertices[grid.Cells[0].VertexIndices[0]].Y);
                    Assert.AreEqual(63814, grid.Cells.Count);
                    Assert.AreEqual(77527, grid.Vertices.Count);

                    Assert.AreEqual(HydroDynamicModelType.Unstructured, waqModel.ModelType);
                    Assert.AreEqual(LayerType.Sigma, waqModel.LayerType);
                    Assert.AreEqual(0, waqModel.ZTop);
                    Assert.AreEqual(1, waqModel.ZBot);

                    Assert.AreEqual("uni3d.vol", waqModel.VolumesRelativeFilePath);
                    Assert.AreEqual("uni3d.are", waqModel.AreasRelativeFilePath);
                    Assert.AreEqual("uni3d.flo", waqModel.FlowsRelativeFilePath);
                    Assert.AreEqual("uni3d.poi", waqModel.PointersRelativeFilePath);
                    Assert.AreEqual("uni3d.len", waqModel.LengthsRelativeFilePath);
                    Assert.AreEqual("uni3d.sal", waqModel.SalinityRelativeFilePath);
                    Assert.AreEqual(string.Empty, waqModel.TemperatureRelativeFilePath);
                    Assert.AreEqual("uni3d.vdf", waqModel.VerticalDiffusionRelativeFilePath);
                    Assert.AreEqual("uni3d.srf", waqModel.SurfacesRelativeFilePath);
                    Assert.AreEqual("uni3d.tau", waqModel.ShearStressesRelativeFilePath);

                    Assert.AreEqual(788900, waqModel.NumberOfHorizontalExchanges);
                    Assert.AreEqual(382884, waqModel.NumberOfVerticalExchanges);
                    Assert.AreEqual(7, waqModel.NumberOfHydrodynamicLayers);
                    Assert.AreEqual(63814, waqModel.NumberOfDelwaqSegmentsPerHydrodynamicLayer);
                    Assert.AreEqual(7, waqModel.NumberOfWaqSegmentLayers);

                    IEventedList<WaterQualityBoundary> boundaries = waqModel.Boundaries;
                    Assert.AreEqual(6, boundaries.Count);
                    var expectedBoundaries = new[]
                    {
                        "sea_002.pli",
                        "sacra_001.pli",
                        "sanjoa_001.pli",
                        "yolo_001.pli",
                        "CC.pli",
                        "tracy.pli"
                    };
                    CollectionAssert.AreEqual(expectedBoundaries, boundaries.Select(b => b.Name).ToArray());

                    IDictionary<WaterQualityBoundary, int[]> boundaryNodeIds = waqModel.BoundaryNodeIds;
                    Assert.AreEqual(boundaries.Count, boundaryNodeIds.Count);
                    var expectedNumberOfBoundaryNodeIds = new[]
                    {
                        105,
                        4,
                        3,
                        24,
                        1,
                        1
                    };
                    for (var i = 0; i < boundaries.Count; i++)
                    {
                        int[] ids = boundaryNodeIds[boundaries[i]];
                        Assert.AreEqual(expectedNumberOfBoundaryNodeIds[i], ids.Length);
                    }
                }
            }
            finally
            {
                // cleanup all files after test has ran.
                string dir = Path.GetDirectoryName(localHydFile);
                FileUtils.DeleteIfExists(dir);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void OnProjectOpenedTestWaqModelWithHydFileAfterRunShouldReimportThatFileAndWaqOutputInSync()
        {
            string savePath = Path.Combine(Environment.CurrentDirectory, "OutOfSync", "project1.dsproj");
            try
            {
                // start deltashell
                using (var app = DeltaShellCoreFactory.CreateApplication())
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                    app.Run();
                    app.CreateNewProject();

                    // create a model
                    WaterQualityModel waqModel = WaterQualityModelWorkDirectoryTest.CreateWaqModelWithData();
                    app.Project.RootFolder.Add(waqModel);

                    waqModel.OutputOutOfSync = false;

                    // save it
                    app.SaveProjectAs(savePath);
                    Assert.That(waqModel.OutputOutOfSync, Is.False);
                    app.CloseProject();

                    // load the model & and check if output is in sync
                    app.OpenProject(savePath);

                    waqModel = app.Project.RootFolder.Models.OfType<WaterQualityModel>().FirstOrDefault();
                    Assert.That(waqModel, Is.Not.Null);
                    Assert.That(waqModel.OutputOutOfSync, Is.False);
                }
            }
            finally
            {
                // cleanup all files after test has ran.
                string dir = Path.GetDirectoryName(savePath);
                FileUtils.DeleteIfExists(dir);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void GivenLegacyPre350WAQModelWhenOpenAndMigrateThenModelIsMigrated()
        {
            // open arjans's legacymodel
            string projectPath = TestHelper.GetTestFilePath(@"BackwardsCompatibility\demo-test1.dsproj");
            string projectFileName = TestHelper.CopyProjectToLocalDirectory(projectPath);
            Assert.That(NrOfDataItemsInProject(projectFileName), Is.EqualTo(152), "The legacy model is changed!");
            Assert.That(DataItemNameExistInDsProj(projectFileName, "OutputSubstancesTag"), Is.False, "The legacy model is already migrated, it contains the DataItem for the substances dataitem set");
            Assert.That(DataItemNameExistInDsProj(projectFileName, "OutputParametersTag"), Is.False, "The legacy model is already migrated, it contains the DataItem for the output parameters dataitem set");
            Assert.That(WAQ350MigrationDoneInDsProj(projectFileName), Is.False, "The legacy model is already migrated");

            string currentDirectory = Directory.GetCurrentDirectory();
            const string projectSaveName = "test.dsproj";
            string saveFolderPath = Path.Combine(currentDirectory, "A");
            FileUtils.DeleteIfExists(saveFolderPath);
            Directory.CreateDirectory(saveFolderPath);
            string saveLocation = Path.Combine(saveFolderPath, projectSaveName);

            // call
            string savePath = Path.Combine(saveLocation);

            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.AddRange(GetPlugins());

                app.Run();

                app.OpenProject(projectFileName);
                Assert.That(app.Project.IsMigrated, Is.True);

                // assert
                List<WaterQualityModel> models = app.Project.RootFolder.Models.OfType<WaterQualityModel>().ToList();

                Assert.AreEqual(1, models.Count);

                app.SaveProjectAs(savePath);
                app.CloseProject();
            }

            try
            {
                Assert.That(NrOfDataItemsInProject(savePath), Is.EqualTo(151),
                            "Original legacy model contained 152 data items, we need to have at least 2 more now... " +
                            "However, restart data items were removed (-3), so it should come down to 151.");
                Assert.That(DataItemNameExistInDsProj(savePath, "OutputSubstancesTag"), Is.True,
                            "Migration from legacy WAQ to 3.5.1 has failed");
                Assert.That(DataItemNameExistInDsProj(savePath, "OutputParametersTag"), Is.True,
                            "Migration from legacy WAQ to 3.5.1 has failed");
                Assert.That(WAQ350MigrationDoneInDsProj(savePath), Is.True,
                            "Migration from legacy WAQ to 3.5.1 has failed");
            }
            finally
            {
                FileUtils.DeleteIfExists(projectFileName);
                FileUtils.DeleteIfExists(projectFileName + "_data");
                FileUtils.DeleteIfExists(saveFolderPath);
            }
        }

        private bool WAQ350MigrationDoneInDsProj(string projectFileName)
        {
            bool migrated;

            using (
                var connection =
                    new SQLiteConnection(string.Format(@"data source={0};read only=true", projectFileName)))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    try
                    {
                        var substancesDataSetIdQuery =
                            "SELECT `project_item_id` FROM `IDataItem` WHERE `role` = 'Output'  AND `Name` = 'Substances' ";
                        cmd.CommandText = substancesDataSetIdQuery;

                        object result = cmd.ExecuteScalar();
                        int substancesDataSetId = result == null || DBNull.Value == result
                                                      ? -1
                                                      : int.Parse(result.ToString());

                        var substancesMigratedQuery =
                            "SELECT COUNT(*) FROM (SELECT `_rowid_`,* FROM `IDataItem` WHERE `dataitemset_id` LIKE '{0}')";
                        substancesMigratedQuery = string.Format(substancesMigratedQuery, substancesDataSetId);

                        cmd.CommandText = substancesMigratedQuery;
                        result = cmd.ExecuteScalar();
                        bool substancesMigrated = result != null && DBNull.Value != result &&
                                                  int.Parse(result.ToString()) > 0;

                        var outputParametersDataSetIdQuery =
                            "SELECT `project_item_id` FROM `IDataItem` WHERE `role` = 'Output'  AND `Name` = 'Output parameters' ";
                        cmd.CommandText = outputParametersDataSetIdQuery;
                        result = cmd.ExecuteScalar();
                        int outputParametersDataSetId = result == null || DBNull.Value == result
                                                            ? -1
                                                            : int.Parse(result.ToString());

                        var outpurParametersMigratedQuery =
                            "SELECT COUNT(*) FROM (SELECT `_rowid_`,* FROM `IDataItem` WHERE `dataitemset_id` LIKE '{0}')";
                        outpurParametersMigratedQuery = string.Format(outpurParametersMigratedQuery,
                                                                      outputParametersDataSetId);

                        cmd.CommandText = outpurParametersMigratedQuery;
                        result = cmd.ExecuteScalar();
                        bool outputParametersMigrated = result != null && DBNull.Value != result &&
                                                        int.Parse(result.ToString()) > 0;

                        migrated = substancesMigrated && outputParametersMigrated;
                    }
                    catch
                    {
                        return false;
                    }
                }

                connection.Close();
            }

            return migrated;
        }

        private bool DataItemNameExistInDsProj(string projectFileName, string dataItemName)
        {
            bool item;
            using (
                var connection =
                    new SQLiteConnection(string.Format(@"data source={0};read only=true", projectFileName)))
            {
                connection.Open();
                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    var query = "SELECT COUNT(*) FROM (SELECT `_rowid_`,* FROM `DataItem` WHERE `Tag` LIKE '{0}' ORDER BY `_rowid_` ASC)";
                    query = string.Format(query, dataItemName);
                    cmd.CommandText = query;
                    object nrOfDataItemsInProject = cmd.ExecuteScalar();
                    item = nrOfDataItemsInProject != null && DBNull.Value != nrOfDataItemsInProject && int.Parse(nrOfDataItemsInProject.ToString()) > 0;
                }

                connection.Close();
            }

            return item;
        }

        private object NrOfDataItemsInProject(string projectFileName)
        {
            object nrOfDataItemsInProject;
            using (
                var connection =
                    new SQLiteConnection(string.Format(@"data source={0};read only=true", projectFileName)))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    var query = "SELECT COUNT(*) FROM (SELECT `_rowid_`,* FROM `DataItem`  ORDER BY `_rowid_` ASC)";
                    cmd.CommandText = query;
                    nrOfDataItemsInProject = cmd.ExecuteScalar();
                }

                connection.Close();
            }

            return nrOfDataItemsInProject;
        }

        private IEnumerable<ApplicationPlugin> GetPlugins()
        {
            return new List<ApplicationPlugin>
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new WaterQualityModelApplicationPlugin()
            };
        }
    }
}