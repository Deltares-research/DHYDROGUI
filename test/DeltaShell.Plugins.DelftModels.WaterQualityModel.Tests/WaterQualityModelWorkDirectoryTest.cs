using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelWorkDirectoryTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void RunModelInTempTest()
        {
            // setup
            var model = CreateWaqModelWithData();

            var waqModelTempFolder = Path.GetDirectoryName(model.ModelSettings.WorkDirectory);

            try
            {
                // call
                ActivityRunner.RunActivity(model);

                // assert

                // Expected folder layout:
                // %temp%
                // `-- <temp folder location determined by Waq Model>
                //    |-- <waq_model_name>_output
                //    |   `-- ... model work directory, with includes and *inp file etc.
                //    |-- <waq_model_name>
                //    |  |-- boundary_data_tables
                //    |  |   `-- ... all boundary *.tbl and *.usefors files.
                //    |  |-- load_data_tables
                //    |  |   `-- ...  all load *.tbl and *.usefors files.
                //    |  |-- model_run_output
                //    |  |   `-- ... all output generated from waq run like *.lst, *.his, *.map files.

                var tempPath = Path.GetTempPath();
                StringAssert.StartsWith(tempPath, model.ModelSettings.WorkDirectory,
                    "Work directory should be located somewhere in temp folder.");
                StringAssert.StartsWith(tempPath, model.ModelSettings.OutputDirectory,
                    "Output directory should be located somewhere in temp folder.");
                StringAssert.StartsWith(tempPath, model.BoundaryDataManager.FolderPath,
                    "Boundary data directory should be located somewhere in temp folder.");
                StringAssert.StartsWith(tempPath, model.LoadsDataManager.FolderPath,
                    "Loads data directory should be located somewhere in temp folder.");

                var parentFolderOfWaqModelTempFolder = Path.GetDirectoryName(waqModelTempFolder) + Path.DirectorySeparatorChar;
                Assert.AreEqual(tempPath, parentFolderOfWaqModelTempFolder,
                    "Waq model temp folder should be located directly in temp folder.");
                var waqModelFolderName = model.Name.Replace(" ", "_");
                Assert.AreEqual(waqModelFolderName + "_output", Path.GetFileName(model.ModelSettings.WorkDirectory),
                    "Expected work directory name should be based on name of waq model and post-fixed with '_output'.");

                var waqModelDataFolder = Path.GetDirectoryName(model.ModelSettings.OutputDirectory);
                Assert.AreEqual(waqModelTempFolder, Path.GetDirectoryName(waqModelDataFolder),
                    "Expected waq model data folder to be direct child-folder of the work directory parent folder.");
                Assert.AreEqual(waqModelFolderName, Path.GetFileName(waqModelDataFolder),
                    "Expected waq data directory name should be based on name of waq model without post-fix.");
                Assert.AreEqual(waqModelDataFolder, Path.GetDirectoryName(model.BoundaryDataManager.FolderPath),
                    "Parent folder of boundary data manager should be the waq data directory.");
                Assert.AreEqual(waqModelDataFolder, Path.GetDirectoryName(model.LoadsDataManager.FolderPath),
                    "Parent folder of load data manager should be the waq data directory.");
            }
            finally
            {
                FileUtils.DeleteIfExists(waqModelTempFolder);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddWaqModelToProject_SetsProjectDataDir()
        {
            // setup
            using (var deltaShell = GetRunningDSApplication(true))
            {
                var model = CreateWaqModelWithData();

                // call
                deltaShell.Project.RootFolder.Add(model);

                // assert
                StringAssert.StartsWith(deltaShell.HybridProjectRepository.ProjectDataDirectory, model.ModelDataDirectory);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenWAQModelWithDefaultProcessDefinitionFilePath_WhenOpeningTheModel_ThenTheCurrentDefaultProcessDefinitionPathIsChosen()
        {
            const string procDefFilePath = @"C:\DeltaShell.Plugins.WaterQualityModel\waq_kernel\Data\Default\proc_def";
            var originalHydDirectory = TestHelper.GetTestFilePath(@"Models\FM_c01_waqtest_2D");
            var testHydFilePath = Path.Combine(TestHelper.CreateLocalCopy(originalHydDirectory), "waqtest.hyd");
            var testModelDirectory = FileUtils.CreateTempDirectory();
            var testModelDsproj = Path.Combine(testModelDirectory, "WAQ_Model.dsproj");

            try
            {
                using (var gui = new DeltaShellGui())
                {
                    var app = gui.Application;
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                    gui.Plugins.Add(new WaterQualityModelGuiPlugin());
                    gui.Run();

                    var waqModel = new WaterQualityModel();
                    var importer = new HydFileImporter();
                    importer.ImportItem(testHydFilePath);
                    waqModel.SubstanceProcessLibrary.ProcessDefinitionFilesPath = procDefFilePath;

                    app.Project.RootFolder.Add(waqModel);
                    app.SaveProjectAs(testModelDsproj);
                    Assert.IsFalse(File.Exists(procDefFilePath));

                    app.OpenProject(testModelDsproj);
                    var modelsInProject = app.GetAllModelsInProject().ToList();
                    Assert.That(modelsInProject.Count, Is.EqualTo(1));

                    waqModel = modelsInProject.FirstOrDefault() as WaterQualityModel;
                    Assert.IsNotNull(waqModel);

                    // The original ProcessDefinitionFilesPath of the WAQ-model was: "C:\\DeltaShell.Plugins.WaterQualityModel\waq_kernel\Data\Default\proc_def"
                    // As the file path ends with "DeltaShell.Plugins.WaterQualityModel\waq_kernel\Data\Default\proc_def", this is the default file path to the process definition path on another PC.
                    // Here, we check that this process definition file path is set to the one that is default on the current build of DeltaShell.
                    Assert.That(waqModel.SubstanceProcessLibrary.ProcessDefinitionFilesPath,
                        Is.EqualTo(SubstanceProcessLibrary.DefaultSobekProcessDefinitionFilesPath));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(testModelDirectory);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RunModelInSavedFolderTest()
        {
            RunModelInSavedFolderTestCore(true);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]
        public void RunModelInSavedFolderTest_WIP()
        {
            RunModelInSavedFolderTestCore(false);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RunModelAndThenSave_CopiesOutput()
        {
            RunModelAndThenSave(true);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]
        public void RunModelAndThenSave_CopiesOutput_NoTempProject()
        {
            RunModelAndThenSave(false);
        }

        /// <summary>
        /// This test ensures that there is no FileNotFoundException when
        /// the model didn't run completely (or a file is missing because of deletion).
        /// When you save the model, it should only look at the items that exist.
        /// TOOLS-22255
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void RunModel_DeleteMonFile_Save()
        {
            string savePath = Path.Combine(Environment.CurrentDirectory, "RunModel_DeleteMonFile_Save",
                        "project1.dsproj");
            try
            {
                using (var deltaShell = GetRunningDSApplication(true))
                {
                    var dataDir = TestHelper.GetDataDir();
                    var realHydFile = Path.Combine(dataDir, "IO", "real", "uni3d.hyd");

                    var model = CreateWaqModelWithData(realHydFile, false);
                    deltaShell.Project.RootFolder.Add(model);

                    ActivityRunner.RunActivity(model);

                    Assert.IsTrue(Directory.Exists(deltaShell.ProjectDataDirectory),
                        "Couldn't find " + deltaShell.ProjectDataDirectory);

                    // delete the mon file, so it's incomplete
                    string monFilePath = Path.Combine(model.ModelSettings.OutputDirectory, "deltashell.mon");
                    Assert.IsTrue(File.Exists(monFilePath), "Couldn't find " + monFilePath);
                    File.Delete(monFilePath);

                    deltaShell.SaveProjectAs(savePath);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(savePath);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void RunModel_ClearWorkFiles(bool useCompletingModel)
        {
            // I would like to check if the wrk files are here before running the model,
            // but I cannot find a hook before cleanup is called.

            WaterQualityModel model;

            if (useCompletingModel)
            {
                var dataDir = TestHelper.GetDataDir();
                var realHydFile = Path.Combine(dataDir, "IO", "real", "uni3d.hyd");

                model = CreateWaqModelWithData(realHydFile, false);    
            }
            else
            {
                model = CreateWaqModelWithData();
            }

            // run the model
            ActivityRunner.RunActivity(model);

            Assert.IsTrue(Directory.Exists(model.ModelSettings.WorkDirectory));

            // check that all work files are removed.
            string[] wrkFiles = Directory.GetFileSystemEntries(model.ModelSettings.WorkDirectory, "*.wrk");
            Assert.AreEqual(0, wrkFiles.Length);

            Assert.IsFalse(File.Exists(Path.Combine(model.ModelSettings.WorkDirectory, "delwaq.rtn")));
            Assert.IsFalse(File.Exists(Path.Combine(model.ModelSettings.WorkDirectory, "deltashell-initials.map")));
        }

        #region Test helpers

        private static void RunModelInSavedFolderTestCore(bool createAndSaveTempProjectOnStartup)
        {
            string savePath = Path.Combine(Environment.CurrentDirectory, "RunModelInSavedFolderTest",
                "project1.dsproj");
            var expectedProjectDataFolderPath = savePath + "_data";
            try
            {
                using (var deltaShell = GetRunningDSApplication(createAndSaveTempProjectOnStartup))
                {
                    var model = CreateWaqModelWithData();
                    deltaShell.Project.RootFolder.Add(model);

                    deltaShell.SaveProjectAs(savePath);

                    StringAssert.StartsWith(expectedProjectDataFolderPath, model.ExplicitWorkingDirectory);

                    ActivityRunner.RunActivity(model);

                    Assert.AreEqual(model.ExplicitWorkingDirectory, model.ModelSettings.WorkDirectory);
                    Assert.AreEqual(model.ExplicitOutputDirectory, model.ModelSettings.OutputDirectory);
                    Assert.IsTrue(Directory.Exists(model.ModelSettings.OutputDirectory),
                        "The project data directory doesn't exist.");
                    Assert.IsTrue(Directory.GetFiles(model.ModelSettings.OutputDirectory).Length > 0,
                        "There are no output files in the data directory.");
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(savePath));
            }
        }

        private static void RunModelAndThenSave(bool saveTempProjectOnStartup)
        {
            string savePath = Path.Combine(Environment.CurrentDirectory, "RunModelAndThenSave_CopiesOutput",
                "project1.dsproj");
            string projectDataDir = savePath + "_data";
            try
            {
                using (var deltaShell = GetRunningDSApplication(saveTempProjectOnStartup))
                {
                    var model = CreateWaqModelWithData();
                    deltaShell.Project.RootFolder.Add(model);

                    ActivityRunner.RunActivity(model);

                    deltaShell.SaveProjectAs(savePath);

                    StringAssert.StartsWith(projectDataDir, model.ModelSettings.WorkDirectory);
                    StringAssert.StartsWith(projectDataDir, model.ModelSettings.OutputDirectory);

                    Assert.AreEqual(model.ExplicitWorkingDirectory, model.ModelSettings.WorkDirectory);
                    Assert.AreEqual(model.ExplicitOutputDirectory, model.ModelSettings.OutputDirectory);

                    Assert.IsTrue(Directory.Exists(model.ModelSettings.WorkDirectory),
                        "Waq model work directory should exist. " + model.ModelSettings.WorkDirectory);
                    Assert.AreEqual(0, Directory.GetFiles(model.ModelSettings.WorkDirectory).Length,
                        "Waq model work directory ({0}) should be empty as work directory is not moved on save.", model.ModelSettings.WorkDirectory);

                    Assert.IsTrue(Directory.Exists(model.ModelSettings.OutputDirectory),
                        "The project data directory should exist. " + model.ModelSettings.OutputDirectory);
                    Assert.IsTrue(Directory.GetFiles(model.ModelSettings.OutputDirectory).Length > 0,
                        "There should be output files in the data directory.");
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(savePath));
            }
        }

        public static WaterQualityModel CreateWaqModelWithData(string hydFile = null, bool createFalseBoundaryData = true)
        {
            if (hydFile == null)
            {
                var dataDir = TestHelper.GetDataDir();
                var squareHydFile = Path.Combine(dataDir, "IO", "square", "square.hyd");

                hydFile = squareHydFile;
            }

            var data = HydFileReader.ReadAll(new FileInfo(hydFile));

            var model = new WaterQualityModel();
            model.ImportHydroData(data);

            var subFilePath = TestHelper.GetTestFilePath(@"IO\03d_Tewor2003.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

            if (createFalseBoundaryData)
            {
                model.BoundaryDataManager.CreateNewDataTable("A", "B", "C.d", "E");
                model.LoadsDataManager.CreateNewDataTable("F", "G", "H.i", "J");    
            }

            return model;
        }

        private static DeltaShellApplication GetRunningDSApplication(bool createAndSaveProjectOnStartup)
        {
            var app = new DeltaShellApplication();
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new WaterQualityModelApplicationPlugin());

            app.IsProjectCreatedInTemporaryDirectory = createAndSaveProjectOnStartup;

            app.Run();

            return app;
        }

        #endregion
    }
}