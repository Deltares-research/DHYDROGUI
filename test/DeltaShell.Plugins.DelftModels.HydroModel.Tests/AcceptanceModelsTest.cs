using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr.Gui;
using DeltaShell.Gui;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui;
using DeltaShell.Plugins.Fews;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.Scripting.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.Toolbox;
using DeltaShell.Plugins.Toolbox.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [Category(NghsTestCategory.AcceptanceTests)]
    [Category(TestCategory.Slow)]
    [Category(TestCategory.WindowsForms)]
    [TestFixture]
    public class AcceptanceModelsTest
    {
        #region TestFixture
        private static string TestFixtureDirectory = string.Empty;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            TestFixtureDirectory = FileUtils.CreateTempDirectory();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            FileUtils.DeleteIfExists(TestFixtureDirectory);
        }
        #endregion

        #region AcceptanceModelTests

        [Test]
        [TestCase("c01_Noordzeemodel/Noordzeemodel.zip", @"DeltaShell_Noordzeemodel\noordzee_2d.mdu", TestName = "c01_Noordzeemodel")]
        [TestCase("c02_Maas_40m/Maas_40m.zip", @"Maas_40m.dsproj_data\Maas_j14_5-v2\Maas_j14_5-v2.mdu", TestName = "c02_Maas_40m (Maas_40m)")]
        [TestCase("c02_Maas_40m/Maas_DIMR.zip", @"DIMR\dflowfm\Maas_j14_5-v2.mdu", TestName = "c02_Maas_40m (Maas_DIMR)")]
        [TestCase("c03_Waal_40m/Waal_40m.zip", @"Waal_40m.dsproj_data\Waal_40m\Waal_40m.mdu", TestName = "c03_Waal_40m")]
        [TestCase("c04_Markermeer_Veluwerandmeren/d3dfm_vrm_j10-v1.zip", @"d3dfm_vrm_j10-v1\FlowFM_uniWind.mdu", TestName = "c04_Markermeer_Veluwerandmeren (FlowFM_uniWind)")]
        [TestCase("c04_Markermeer_Veluwerandmeren/d3dfm_vrm_j10-v1.zip", @"d3dfm_vrm_j10-v1\FlowFM_varWind.mdu", TestName = "c04_Markermeer_Veluwerandmeren (FlowFM_varWind)")]
        [TestCase("c05_Oosterschelde/Oosterschelde.zip", @"Filebased\e02.mdu", TestName = "c05_Oosterschelde")]
        public void Delft3DFM_AcceptanceModelTest(string relativeZipFilePath, string relativeMduFilePath)
        {
            // Step 1: Unzip 
            var testDataFolder = new DirectoryInfo(TestHelper.GetTestWorkingDirectory()).Parent?.Parent?.Parent?.FullName;
            var testCaseZipFilePath = Path.Combine(testDataFolder, "AcceptanceModels", "Delft3DFM", relativeZipFilePath);

            Assert.IsTrue(File.Exists(testCaseZipFilePath), "Failed to find acceptance model test-data");

            var testDirectory = string.Empty;

            Assert.True(TryPerformAction(() => UnzipModel(relativeZipFilePath, testCaseZipFilePath, out testDirectory)),
                string.Format("Failed to unzip file: {0}", testCaseZipFilePath));

            // Step 2: using(running GUI) add correct plugins for Delft3DFM
            using (var gui = new DeltaShellGui())
            {
                //load the plugins
                gui.Plugins.Add(new DimrGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Plugins.Add(new WaveGuiPlugin());
                gui.Plugins.Add(new HydroModelGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new RealTimeControlGuiPlugin());
                gui.Plugins.Add(new ScriptingGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new ToolboxGuiPlugin());
                gui.Plugins.Add(new WaterQualityModelGuiPlugin());

                var app = gui.Application;
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new WaveApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new ToolboxApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    // Step 3: Find root project path in zip folder
                    var projectRootPath = GetProjectRootInUnzippedFolder(testDirectory, relativeMduFilePath);
                    Assert.That(projectRootPath, Is.Not.Null);

                    // Step 4: Import MDU
                    var mduPath = Path.Combine(projectRootPath, relativeMduFilePath);
                    Assert.True(TryPerformAction(() => ImportFlowFMModelAndAddToProject(app, mduPath)),
                        string.Format("Failed to import model: {0}", mduPath));

                    // Step 5: Save Project As
                    var projectPath = Path.Combine(testDirectory, @"TestProjectFolder\TestProject.dsproj");
                    Assert.True(TryPerformAction(() => app.SaveProjectAs(projectPath)),
                        string.Format("Failed to save project before running the model: {0}", projectPath));

                    // Step 6: Close Project
                    Assert.True(TryPerformAction(() => app.CloseProject()),
                        string.Format("Failed to close project after import: {0}", projectPath));

                    // Step 7: Re-Open Project
                    Assert.True(TryPerformAction(() => app.OpenProject(projectPath)),
                        string.Format("Failed to reopen project: {0}", projectPath));

                    // Step 8: Validation of the model
                    ITimeDependentModel rootModel = null;
                    Assert.True(TryPerformAction(() => GetRootModelAndValidate(app, out rootModel)),
                        string.Format("Failed to validate model: {0}", rootModel.Name));

                    // Step 9: Dimr Export of FM model
                    Assert.True(TryPerformAction(() => ExportDimrConfiguration(testDirectory, rootModel)),
                        string.Format("Failed to export dimr configuration for model: {0}", rootModel.Name));

                    // Step 10: Adjust Time Settings (10 time steps)
                    Assert.True(TryPerformAction(() => AdjustTimeSettings(rootModel)),
                        string.Format("Failed to adjust time settings for model: {0}", rootModel.Name));

                    // Step 11: Run model
                    Assert.True(TryPerformAction(() => RunModel(rootModel)),
                        string.Format("Failed to run model: {0}", rootModel.Name));

                    // Step 12: Save Project
                    Assert.True(TryPerformAction(() => app.SaveProject()),
                        string.Format("Failed to save project after running the model: {0}", projectPath));

                    // Step 13: Close Project
                    Assert.True(TryPerformAction(() => app.CloseProject()),
                        string.Format("Failed to close project after running the model: {0}", projectPath));
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        #endregion

        #region HelperFunctions
        
        /// <summary>
        /// Wraps Action in a Try-Catch, returns false if Action results in an exception
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Success or Failure of Action</returns>
        private static bool TryPerformAction(Action action)
        {
            try
            {
                action.Invoke();
                return true;
            }
            catch (Exception debug)
            {
                if(!string.IsNullOrEmpty(debug.Message)) Console.WriteLine(debug.Message);
                return false;
            }
        }

        /// <summary>
        /// Get the absolute path to the folder in <paramref name="unzipFolder"/> that directly contains
        /// <paramref name="projectPath"/>. If no such path exists, null is returned.
        /// </summary>
        /// <param name="unzipFolder"> The basefolder in which to look for the projectPath. </param>
        /// <param name="projectPath"> The file path which is matched within the <paramref name="unzipFolder"/></param>
        /// <returns> The path to the parent directory of <paramref name="projectPath"/> if it exists, null otherwise. </returns>
        private static string GetProjectRootInUnzippedFolder(string unzipFolder, string projectPath)
        {
            var rootFolder = unzipFolder;
            IList<string> subFolders = new List<string>();
            while (true)
            {
                if (File.Exists(Path.Combine(rootFolder, projectPath)))
                    return rootFolder;

                subFolders.AddRange(Directory.GetDirectories(rootFolder));

                if (subFolders.Count > 0)
                {
                    rootFolder = subFolders.First();
                    subFolders.RemoveAt(0);
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region CommonOperations

        private static void UnzipModel(string relativeZipFilePath, string testCaseZipFilePath, out string testDirectory)
        {
            var zipFileName = Path.GetFileName(relativeZipFilePath);
            if (string.IsNullOrEmpty(zipFileName))
                throw new ArgumentException(string.Format("Unable to retrieve Zip File Name: {0}", relativeZipFilePath));

            var localZipFilePath = Path.Combine(TestFixtureDirectory, zipFileName);

            FileUtils.DeleteIfExists(localZipFilePath);
            FileUtils.CopyFile(testCaseZipFilePath, localZipFilePath);

            var randomFileName = Path.GetRandomFileName().Substring(0, 5);
            testDirectory = Path.Combine(TestFixtureDirectory, randomFileName);
            FileUtils.DeleteIfExists(testDirectory);
            ZipFileUtils.Extract(localZipFilePath, testDirectory);
        }

        private static void ImportFlowFMModelAndAddToProject(IApplication app, string mduPath)
        {
            var fmModel = new WaterFlowFMModel(mduPath);
            app.Project.RootFolder.Items.Add(fmModel);
        }

        private static void GetRootModelAndValidate(IApplication app, out ITimeDependentModel timeDependentModel)
        {
            timeDependentModel = GetRootModel<ITimeDependentModel>(app);

            // Unfortunately there is no IValidateable, so instead we handle this for each individual model type...
            ValidationReport report = null;

            var hydroModel = timeDependentModel as HydroModel;
            if (hydroModel != null) report = hydroModel.Validate();

            var fmModel = timeDependentModel as WaterFlowFMModel;
            if (fmModel != null) report = fmModel.Validate();
            
            var model1D = timeDependentModel as WaterFlowModel1D;
            if (model1D != null) report = model1D.Validate();
            
            if (report == null)
                throw new NotImplementedException(string.Format("Unable to Validate Root Model: {0}, did you forget to add support for this model type?", timeDependentModel.Name));

            CheckValidationErrors(report);
        }

        private static void CheckValidationErrors(ValidationReport report)
        {
            if (report.ErrorCount > 0)
            {
                var debugInfo = string.Join(Environment.NewLine, report.AllErrors.Select(er => er.Message));
                throw new Exception(debugInfo);
            }
        }

        private static T GetRootModel<T>(IApplication app)
        {
            var model = app.Project.RootFolder.Models.OfType<T>().FirstOrDefault();
            if (model == null) throw new NullReferenceException("Unable to get Model from Project Root Folder");
            return model;
        }

        private static void AdjustTimeSettings(ITimeDependentModel timeDependentModel)
        {
            timeDependentModel.StopTime = timeDependentModel.StartTime.AddHours(timeDependentModel.TimeStep.TotalHours * 10);
        }

        private static void RunModel(IActivity activity)
        {
            var hydroModel = activity as HydroModel;
            if (hydroModel != null)
            {
                // We get the workflow that runs all models in parallel
                var workflow = hydroModel.Workflows.FirstOrDefault(w => w.Activities.Count == hydroModel.Activities.Count);
                if (workflow == null) throw new NullReferenceException("Unable to get Workflow (run all models in parallel)");

                hydroModel.CurrentWorkflow = workflow;
            }

            // Run model
            ActivityRunner.RunActivity(activity);
            if(activity.Status != ActivityStatus.Cleaned) throw new AssertionException(
                string.Format("Unable to complete Model run{0}Expected status: Cleaned{0}Actual status: {1}",
                    System.Environment.NewLine, activity.Status.ToString()));
        }
        
        private static void ExportDimrConfiguration(string tempDir, IModel model)
        {
            var dimrExportDir = Path.Combine(tempDir, "Dimr_Export");
            FileUtils.CreateDirectoryIfNotExists(dimrExportDir);

            var exporter = new DHydroConfigXmlExporter { ExportFilePath = Path.Combine(dimrExportDir, "dimr.xml") };
            if (!exporter.Export(model, null)) throw new Exception();
        }
        #endregion
    }
}
