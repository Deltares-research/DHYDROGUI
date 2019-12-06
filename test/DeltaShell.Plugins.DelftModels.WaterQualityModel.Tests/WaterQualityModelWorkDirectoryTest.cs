using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.NGHS.TestUtils;
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
        [Category(TestCategory.Integration)]
        public void AddWaqModelToProject_SetsProjectDataDir()
        {
            // setup
            using (var deltaShell = GetRunningDSApplication("path", true))
            {
                var model = CreateWaqModelWithData();

                // call
                deltaShell.Project.RootFolder.Add(model);

                // assert
                StringAssert.StartsWith(deltaShell.HybridProjectRepository.ProjectDataDirectory, model.ModelDataDirectory);
            }
        }

        [TestCase(@"C:\DeltaShell.Plugins.WaterQualityModel\waq_kernel\Data\Default\proc_def")]
        [TestCase(@"C:\DeltaShell.Plugins.DelftModels.WaterQualityModel\waq_kernel\Data\Default\proc_def")]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GivenWAQModelWithDefaultProcessDefinitionFilePath_WhenOpeningTheModel_ThenTheCurrentDefaultProcessDefinitionPathIsChosen(string processDefinitionFilePath)
        {
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
                    waqModel.SubstanceProcessLibrary.ProcessDefinitionFilesPath = processDefinitionFilePath;

                    app.Project.RootFolder.Add(waqModel);
                    app.SaveProjectAs(testModelDsproj);
                    Assert.IsFalse(File.Exists(processDefinitionFilePath));

                    app.OpenProject(testModelDsproj);
                    var modelsInProject = app.GetAllModelsInProject().ToList();
                    Assert.That(modelsInProject.Count, Is.EqualTo(1));

                    waqModel = modelsInProject.FirstOrDefault() as WaterQualityModel;
                    Assert.IsNotNull(waqModel);

                    // The original ProcessDefinitionFilesPath of the WAQ-model was: "C:\\DeltaShell.Plugins.WaterQualityModel\waq_kernel\Data\Default\proc_def" 
                    //                                                                or "C:\DeltaShell.Plugins.DelftModels.WaterQualityModel\waq_kernel\Data\Default\proc_def"
                    // As the file path ends with "DeltaShell.Plugins(.DelftModels).WaterQualityModel\waq_kernel\Data\Default\proc_def", this is the default file path to the process 
                    // definition path on another PC or DeltaShell build.
                    // Here, we check that this process definition file path is set to the one that is default on the current build of DeltaShell.
                    Assert.That(waqModel.SubstanceProcessLibrary.ProcessDefinitionFilesPath, Is.EqualTo(SubstanceProcessLibrary.DefaultSobekProcessDefinitionFilesPath));                    
                }
            }
            finally
            {
                
                FileUtils.DeleteIfExists(testModelDirectory);
                FileUtils.DeleteIfExists(Directory.GetParent(testHydFilePath).FullName);
                Thread.Sleep(100); // Give system enough time to delete these files, before the next test case is being tested.
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
        public void RunModelAndThenSave_MovesOutput()
        {
            RunModelAndThenSave(true);
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
            using (var tempDirectory = new TemporaryDirectory())
            {
                string tempDirPath = tempDirectory.Path;
                string savePath = Path.Combine(tempDirPath, "RunModel_DeleteMonFile_Save", "project1.dsproj");
                using (var deltaShell = GetRunningDSApplication(tempDirPath, true))
                {
                    var dataDir = TestHelper.GetTestDataDirectory();
                    var realHydFile = Path.Combine(dataDir, "IO", "real", "uni3d.hyd");

                    var deltaShellWorkingDirectory = deltaShell.WorkDirectory;
                    var model = CreateWaqModelWithData(realHydFile, createFalseBoundaryData: false);
                    model.SetWorkingDirectoryInModelSettings(() => deltaShellWorkingDirectory);

                    deltaShell.Project.RootFolder.Add(model);

                    ActivityRunner.RunActivity(model);

                    Assert.IsTrue(Directory.Exists(deltaShell.ProjectDataDirectory),
                        "Couldn't find " + deltaShell.ProjectDataDirectory);

                    // delete the mon file, so it's incomplete
                    string monFilePath = Path.Combine(model.ModelSettings.WorkingOutputDirectory, "deltashell.mon");
                    Assert.IsTrue(File.Exists(monFilePath), "Couldn't find " + monFilePath);
                    File.Delete(monFilePath);

                    deltaShell.SaveProjectAs(savePath);
                }
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
                var dataDir = TestHelper.GetTestDataDirectory();
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
            using (var tempDirectory = new TemporaryDirectory())
            {
                string tempDirPath = tempDirectory.Path;
                string savePath = Path.Combine(tempDirPath, "RunModelInSavedFolderTest", "project1.dsproj");

                using (DeltaShellApplication deltaShell = GetRunningDSApplication(tempDirPath, createAndSaveTempProjectOnStartup))
                {
                    var model = CreateWaqModelWithData(createFalseBoundaryData:false);
                    deltaShell.Project.RootFolder.Add(model);

                    deltaShell.SaveProjectAs(savePath);

                    ActivityRunner.RunActivity(model);
                    Assert.IsTrue(model.Status == ActivityStatus.Cleaned);

                    Assert.IsTrue(Directory.Exists(model.ModelSettings.WorkingOutputDirectory),
                        "The working output directory doesn't exist.");
                    Assert.IsTrue(Directory.GetFiles(model.ModelSettings.WorkingOutputDirectory).Any(),
                        "There are no output files in the data directory.");
                }
            }
        }

        private static void RunModelAndThenSave(bool saveTempProjectOnStartup)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                string tempDirPath = tempDirectory.Path;
                string savePath = Path.Combine(tempDirPath, "RunModelAndThenSave_CopiesOutput", "project1.dsproj");
                string projectDataDir = savePath + "_data";

                using (DeltaShellApplication deltaShell = GetRunningDSApplication(tempDirPath, saveTempProjectOnStartup))
                {
                    var deltaShellWorkingDirectory = deltaShell.WorkDirectory;
                    var model = CreateWaqModelWithData(createFalseBoundaryData:false);
                    model.SetWorkingDirectoryInModelSettings(() => deltaShellWorkingDirectory);
                    deltaShell.Project.RootFolder.Add(model);

                    ActivityRunner.RunActivity(model);
                    Assert.IsTrue(model.Status == ActivityStatus.Cleaned);

                    deltaShell.SaveProjectAs(savePath);

                    StringAssert.StartsWith(deltaShellWorkingDirectory, model.ModelSettings.WorkDirectory);
                    StringAssert.StartsWith(projectDataDir, model.ModelSettings.OutputDirectory);
                    
                    Assert.IsTrue(Directory.Exists(model.ModelSettings.WorkDirectory),
                        "Waq model work directory should exist. " + model.ModelSettings.WorkDirectory);
                    Assert.AreEqual(0, Directory.GetFiles(model.ModelSettings.WorkDirectory).Length,
                        "Waq model work directory ({0}) should be empty as work directory is moved on save.", model.ModelSettings.WorkDirectory);

                    Assert.IsTrue(Directory.Exists(model.ModelSettings.OutputDirectory),
                        "The project data directory should exist. " + model.ModelSettings.OutputDirectory);
                    Assert.IsTrue(Directory.GetFiles(model.ModelSettings.OutputDirectory).Length > 0,
                        "There should be output files in the data directory.");
                }
            }
        }

        public static WaterQualityModel CreateWaqModelWithData(string hydFile = null, bool createFalseBoundaryData = true)
        {
            if (hydFile == null)
            {
                var dataDir = TestHelper.GetTestDataDirectory();
                var squareHydFile = Path.Combine(dataDir, "ValidWaqModels", "FM", "FlowFM.hyd");

                hydFile = squareHydFile;
            }

            var data = HydFileReader.ReadAll(new FileInfo(hydFile));

            var model = new WaterQualityModel();
            model.ImportHydroData(data);

            var subFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\coli_04.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

            if (createFalseBoundaryData)
            {
                model.BoundaryDataManager.CreateNewDataTable("A", "B", "C.d", "E");
                model.LoadsDataManager.CreateNewDataTable("F", "G", "H.i", "J");    
            }

            return model;
        }

        private static DeltaShellApplication GetRunningDSApplication(string tempDirectoryPath, bool createAndSaveProjectOnStartup)
        {
            string workingDirectoryPath = Path.Combine(tempDirectoryPath, "DeltaShell_Working_Directory");
            ApplicationSettingsBase userSettings = ApplicationTestHelper.GetMockedApplicationSettingsBase(workingDirectoryPath);

            var app = new DeltaShellApplication
            {
                UserSettings = userSettings,
                IsProjectCreatedInTemporaryDirectory = createAndSaveProjectOnStartup
            };

            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new WaterQualityModelApplicationPlugin());
            app.Run();

            return app;
        }

        #endregion
    }
}