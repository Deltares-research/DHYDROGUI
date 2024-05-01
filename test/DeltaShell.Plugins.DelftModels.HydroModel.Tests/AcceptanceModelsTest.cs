using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr.Gui;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
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
    [Ignore("Check if we need any of these Flow1D tests")]
    [Category("Build.Acceptance")]
    [Category(TestCategory.Slow)]
    [Category(TestCategory.WindowsForms)]
    [TestFixture]
    public class AcceptanceModelsTest
    {
        #region RepositorySettings
        private const string Delft3DFM_AcceptanceModelsRepository = "https://repos.deltares.nl/repos/DSCTestbench/trunk/cases/e110_delft3dfm_suite/f01_acceptance_models/";
        private const string SOBEK3_AcceptanceModelsRepository = "https://repos.deltares.nl/repos/DSCTestbench/trunk/cases/e111_sobek3_suite/f01_acceptance_models/";
        private const string CredentialsUser = "dscbuildserver";
        private const string CredentialsPwd = "Bu1lds3rv3r";
        #endregion

        #region TestFixture
        private static string TestFixtureDirectory = string.Empty;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            TestFixtureDirectory = FileUtils.CreateTempDirectory();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            FileUtils.DeleteIfExists(TestFixtureDirectory);
        }
        #endregion

        private static IGui CreateGui()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new DimrGuiPlugin(),
                new CommonToolsGuiPlugin(),
                new FlowFMGuiPlugin(),
                new HydroModelGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new ProjectExplorerGuiPlugin(),
                new RealTimeControlGuiPlugin(),
                new ScriptingGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new ToolboxGuiPlugin(),
                new CommonToolsApplicationPlugin(),
                new NHibernateDaoApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new HydroModelApplicationPlugin(),
                new NetCdfApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new RealTimeControlApplicationPlugin(),
                new ScriptingApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new ToolboxApplicationPlugin(),
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }
        
        #region AcceptanceModelTests
        [TestCase("c01_Noordzeemodel/Noordzeemodel.zip", @"DeltaShell_Noordzeemodel\noordzee_2d.mdu", TestName = "c01_Noordzeemodel")]
        [TestCase("c02_Maas_40m/Maas_40m.zip", @"Maas_40m.dsproj_data\Maas_j14_5-v2\Maas_j14_5-v2.mdu", TestName = "c02_Maas_40m (Maas_40m)")]
        [TestCase("c02_Maas_40m/Maas_DIMR.zip", @"DIMR\dflowfm\Maas_j14_5-v2.mdu", TestName = "c02_Maas_40m (Maas_DIMR)")]
        [TestCase("c03_Waal_40m/Waal_40m.zip", @"Waal_40m.dsproj_data\Waal_40m\Waal_40m.mdu", TestName = "c03_Waal_40m")]
        [TestCase("c04_Markermeer_Veluwerandmeren/d3dfm_vrm_j10-v1.zip", @"d3dfm_vrm_j10-v1\FlowFM_uniWind.mdu", TestName = "c04_Markermeer_Veluwerandmeren (FlowFM_uniWind)")]
        [TestCase("c04_Markermeer_Veluwerandmeren/d3dfm_vrm_j10-v1.zip", @"d3dfm_vrm_j10-v1\FlowFM_varWind.mdu", TestName = "c04_Markermeer_Veluwerandmeren (FlowFM_varWind)")]
        [TestCase("c05_Oosterschelde/Oosterschelde.zip", @"Filebased\e02.mdu", TestName = "c05_Oosterschelde")]
        public void Delft3DFM_AcceptanceModelTest(string relativeZipFilePath, string relativeMduFilePath)
        {
            // Step 1: Download
            var localZipFilePath = string.Empty;
            Assert.True(TryPerformAction(() => DownloadZip(Delft3DFM_AcceptanceModelsRepository, relativeZipFilePath, out localZipFilePath)),
                string.Format("Failed to download zip file: {0}", relativeZipFilePath));

            // Step 2: Unzip
            var localExtractedZipDir = string.Empty;
            Assert.True(TryPerformAction(() => UnzipModel(localZipFilePath, out localExtractedZipDir)),
                string.Format("Failed to unzip file: {0}", localZipFilePath));

            // Step 3: using(running GUI) add correct plugins for Delft3DFM
            using (var gui = CreateGui())
            {
                var app = gui.Application;
                gui.Run();

                Action mainWindowShown = delegate
                {
                    // Step 3.5: Find root project path in zip folder
                    var projectRootPath = GetProjectRootInUnzippedFolder(localExtractedZipDir, relativeMduFilePath);
                    Assert.That(projectRootPath, Is.Not.Null);

                    // Step 4: Import MDU
                    var mduPath = Path.Combine(projectRootPath, relativeMduFilePath);
                    Assert.True(TryPerformAction(() => ImportFlowFMModelAndAddToProject(app, mduPath)),
                        string.Format("Failed to import model: {0}", mduPath));

                    // Step 5: Save Project As
                    var projectPath = Path.Combine(localExtractedZipDir, @"TestProjectFolder\TestProject.dsproj");
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
                    Assert.True(TryPerformAction(() => ExportDimrConfiguration(localExtractedZipDir, rootModel)),
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
        
        [TestCase("c01_sobek-rijn-j17_5-v1/sobek-rijn-j17_5-v1.zip", @"sobek-rijn-j17_5-v1.dsproj", TestName = "c01_sobek-rijn-j17_5-v1")]
        [TestCase("c02_sobek-maas-j17_5-v1/sobek-maas-j17_5-v1.zip", @"sobek-maas-j17_5-v1.dsproj", TestName = "c02_sobek-maas-j17_5-v1")]
        [TestCase("c03_sobek-rmm-j15_5-v1/sobek-rmm-j15_5-v1.zip", @"sobek-rmm-j15_5-v1.dsproj", TestName = "c03_sobek-rmm-j15_5-v1")]
        [TestCase("c04_sobek-ovd-j14_5-v1/sobek-ovd-j14_5-v1.zip", @"sobek-ovd-j14_5-v1.dsproj", TestName = "c04_sobek-ovd-j14_5-v1")]
        [TestCase("c05_sobek-markermeer-j10_5-v1/sobek-markermeer-j10_5-v1.zip", @"sobek-markermeer-j10_5-v1_rvw2006.dsproj", TestName = "c05_sobek-markermeer-j10_5-v1 (sobek-markermeer-j10_5-v1_rvw2006)")]
        [TestCase("c05_sobek-markermeer-j10_5-v1/sobek-markermeer-j10_5-v1.zip", @"sobek-markermeer-j10_5-v1_rvw2007.dsproj", TestName = "c05_sobek-markermeer-j10_5-v1 (sobek-markermeer-j10_5-v1_rvw2007)")]
        [TestCase("c05_sobek-markermeer-j10_5-v1/sobek-markermeer-j10_5-v1.zip", @"sobek-markermeer-j10_5-v1_rvw2011.dsproj", TestName = "c05_sobek-markermeer-j10_5-v1 (sobek-markermeer-j10_5-v1_rvw2011)")]
        [TestCase("c05_sobek-markermeer-j10_5-v1/sobek-markermeer-j10_5-v1.zip", @"sobek-markermeer-j10_5-v1_rvw2013.dsproj", TestName = "c05_sobek-markermeer-j10_5-v1 (sobek-markermeer-j10_5-v1_rvw2013)")]
        [TestCase("c06_sobek-ym_ijvd-j16_05-v1/sobek-ym_ijd-j16_05-v1.zip", @"sobek-ym_ijd-j16_05-v1_rvw2007.dsproj", TestName = "c06_sobek-ym_ijvd-j16_05-v1 (sobek-ym_ijd-j16_05-v1_rvw2007)")]
        [TestCase("c06_sobek-ym_ijvd-j16_05-v1/sobek-ym_ijd-j16_05-v1.zip", @"sobek-ym_ijd-j16_05-v1_rvw2011.dsproj", TestName = "c06_sobek-ym_ijvd-j16_05-v1 (sobek-ym_ijd-j16_05-v1_rvw2011)")]
        [TestCase("c06_sobek-ym_ijvd-j16_05-v1/sobek-ym_ijd-j16_05-v1.zip", @"sobek-ym_ijd-j16_05-v1_rvw2013.dsproj", TestName = "c06_sobek-ym_ijvd-j16_05-v1 (sobek-ym_ijd-j16_05-v1_rvw2013)")]
        [TestCase("c07_sobek-veluwerandmeren-j10-v1/sobek-veluwerandmeren-j10_5-v1.zip", @"sobek-vrm-j10_5-v1.dsproj", TestName = "c07_sobek-veluwerandmeren-j10-v1")]
        [TestCase("c08_sobek-nzk_ark-j15_5-v1/sobek-nzk_ark-j15_5-v1.zip", @"sobek-nzk_ark-j15_5-v1.dsproj", TestName = "c08_sobek-nzk_ark-j15_5-v1")]
        [TestCase("c09_sobek-oosterschelde-j12_5-v1/sobek-oosterschelde-j12_5-v1.zip", @"t18.dsproj", TestName = "c09_sobek-oosterschelde-j12_5-v1")]
        [TestCase("c10_sobek-vozo-j12_5-v1_201312stav/sobek-vozo-j12_5-v1_201312stav.zip", @"sobek-vozo-j12_5-v1_201312stav.dsproj", TestName = "c10_sobek-vozo-j12_5-v1_201312stav")]
        [TestCase("c11_sobek-mlnbk_j14_5-v1/sobek-mlnbk-j14_5-v1.zip", @"sobek-mlnbk-j14_5-v1.dsproj", TestName = "c11_sobek-mlnbk_j14_5-v1")]
        [TestCase("c12_sobek-twentekanaal-j10_5-v1/sobek-twentekanaal-j10_5-v1.zip", @"sobek-twentekanaal_j10_5-v1.dsproj", TestName = "c12_sobek-twentekanaal-j10_5-v1")]
        [TestCase("c13_sobek-meuse-j99_5_v2/sobek-meuse-j99_5-v2.zip", @"sobek-meuse-j99_5-v2.dsproj", TestName = "c13_sobek-meuse-j99_5_v2")]
        [TestCase("c14_sobek-duitse-vecht/Duitse_Vecht.zip", @"Vecht_Ohne_Emlichheim_validatie1.dsproj", TestName = "c14_sobek-duitse-vecht")]
        [TestCase("c15_coupled_sobek-maas-j17_5-v1_meuse-j99_5-v3/sobek-meuse-j99_5-v3_20_20_sobek-maas-j17_5-v1.zip", @"sobek-maas-j17_5-v1_meuse-j99_5-v3.dsproj", TestName = "c15_coupled_sobek-maas-j17_5-v1_meuse-j99_5-v3 (sobek-maas-j17_5-v1_meuse-j99_5-v3)")]
        [TestCase("c15_coupled_sobek-maas-j17_5-v1_meuse-j99_5-v3/sobek-meuse-j99_5-v3_20_20_sobek-maas-j17_5-v1.zip", @"sobek-meuse-j99_5-v3.dsproj", TestName = "c15_coupled_sobek-maas-j17_5-v1_meuse-j99_5-v3 (sobek-meuse-j99_5-v3)")]
        [TestCase("c16_coupled_sobek-ovd_dv-j14_5-v1/sobek-ovd_dv-j14_5-v1.zip", @"sobek-ovd_dv-j14_5-v1.dsproj", TestName = "c16_coupled_sobek-ovd_dv-j14_5-v1")]
        public void SOBEK3_AcceptanceModelTest(string relativeZipFilePath, string relativeDsProjFilePath)
        {
            // Step 1: Download
            var localZipFilePath = string.Empty;
            Assert.True(TryPerformAction(() => DownloadZip(SOBEK3_AcceptanceModelsRepository, relativeZipFilePath, out localZipFilePath)),
                string.Format("Failed to download zip file: {0}", relativeZipFilePath));

            // Step 2: Unzip
            var localExtractedZipDir = string.Empty;
            Assert.True(TryPerformAction(() => UnzipModel(localZipFilePath, out localExtractedZipDir)),
                string.Format("Failed to unzip file: {0}", localZipFilePath));

            // Step 3: using(running GUI) add correct plugins for SOBEK3
            using (var gui = CreateGui())
            {
                var app = gui.Application;
                gui.Run();

                Action mainWindowShown = delegate
                {
                    // Step 3.5: Find root project path in zip folder
                    var projectRootPath = GetProjectRootInUnzippedFolder(localExtractedZipDir, relativeDsProjFilePath);
                    Assert.That(projectRootPath, Is.Not.Null);

                    // Step 4: Open Project
                    var projectPath = Path.Combine(projectRootPath, relativeDsProjFilePath);
                    Assert.True(TryPerformAction(() => app.OpenProject(projectPath)),
                        string.Format("Failed to open project: {0}", projectPath));

                    // Step 5: Save Project As
                    Assert.True(TryPerformAction(() => app.SaveProjectAs(projectPath)),
                        string.Format("Failed to save project before running the model: {0}", projectPath));

                    // Step 6: Close Project
                    Assert.True(TryPerformAction(() => app.CloseProject()),
                        string.Format("Failed to close project after initial open: {0}", projectPath));

                    // Step 7: Re-Open Project
                    Assert.True(TryPerformAction(() => app.OpenProject(projectPath)),
                        string.Format("Failed to reopen project: {0}", projectPath));

                    // Step 8: Validation of the model
                    ITimeDependentModel rootModel = null;
                    Assert.True(TryPerformAction(() => GetRootModelAndValidate(app, out rootModel)),
                        string.Format("Failed to validate model: {0}", rootModel.Name));

                    // Step 9: Dimr Export of HydroModel
                    Assert.True(TryPerformAction(() => ExportDimrConfiguration(localExtractedZipDir, rootModel)),
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
        private static void DownloadZip(string repositoryAddress, string relativeZipFilePath, out string localZipFilePath)
        {
            var zipFileName = Path.GetFileName(relativeZipFilePath);
            if(string.IsNullOrEmpty(zipFileName))
                throw new ArgumentException(string.Format("Unable to retrieve Zip File Name: {0}", relativeZipFilePath));

            localZipFilePath = Path.Combine(TestFixtureDirectory, zipFileName);

            if (!File.Exists(localZipFilePath))
            {
                using (var client = new WebClient() {Credentials = new NetworkCredential(CredentialsUser, CredentialsPwd)})
                {
                    var repositoryZipFilePath = Path.Combine(repositoryAddress, relativeZipFilePath);
                    client.DownloadFile(repositoryZipFilePath, localZipFilePath);
                }
            }

            if(!File.Exists(localZipFilePath))
                throw new FileNotFoundException(string.Format("File does not exist: {0}", localZipFilePath));
        }

        private static void UnzipModel(string localZipFilePath, out string localExtractedZipPath)
        {
            var randomFileName = Path.GetRandomFileName().Substring(0, 5);
            var zipFileInfo = new FileInfo(localZipFilePath);
            localExtractedZipPath = Path.Combine(zipFileInfo.Directory.FullName, randomFileName);
            FileUtils.DeleteIfExists(localExtractedZipPath);
            ZipFileUtils.Extract(localZipFilePath, localExtractedZipPath);
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
            // Set StopTime to StartTime + 10 TimeSteps
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
