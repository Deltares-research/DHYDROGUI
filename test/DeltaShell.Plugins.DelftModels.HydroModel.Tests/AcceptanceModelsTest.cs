using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.Dimr.Gui;
using DeltaShell.Gui;
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
using SharpTestsEx;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
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

        #region AcceptanceModelTests
        [TestCase("c05_Oosterschelde/Oosterschelde.zip", @"Filebased\e02.mdu")]
        [TestCase("c04_Markermeer_Veluwerandmeren/d3dfm_vrm_j10-v1.zip", @"FlowFM_uniWind.mdu")]
        [TestCase("c04_Markermeer_Veluwerandmeren/d3dfm_vrm_j10-v1.zip", @"FlowFM_varWind.mdu")]
        [TestCase("c03_Waal_40m/Waal_40m.zip", @"Waal_40m.dsproj_data\Waal_40m\Waal_40m.mdu")]
        [TestCase("c02_Maas_40m/Maas_40m.zip", @"Maas_40m.dsproj_data\Maas_j14_5-v2\Maas_j14_5-v2.mdu")]
        [TestCase("c02_Maas_40m/Maas_DIMR.zip", @"DIMR\dflowfm\Maas_j14_5-v2.mdu")]
        [TestCase("c01_Noordzeemodel/Noordzeemodel.zip", @"DeltaShell_Noordzeemodel\noordzee_2d.mdu")]
        public void Delft3DFM_AcceptanceModelTest(string relativeZipFilePath, string relativeMduFilePath)
        {
            DoInTemporaryDirectory(tempDir =>
            {
                // Step 1: Download
                var localZipFilePath = string.Empty;
                Assert.True(TryPerformAction(() => DownloadZip(Delft3DFM_AcceptanceModelsRepository, relativeZipFilePath, tempDir, out localZipFilePath)),
                    string.Format("Failed to download zip file: {0}", relativeZipFilePath));

                // Step 2: Unzip
                Assert.True(TryPerformAction(() => UnzipModel(localZipFilePath)),
                    string.Format("Failed to unzip file: {0}", localZipFilePath));

                // Step 3: using(running GUI) add correct plugins for Delft3DFM
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
                        // Step 4: Import MDU
                        var mduPath = Path.Combine(tempDir, relativeMduFilePath);
                        Assert.True(TryPerformAction(() => ImportFlowFMModelAndAddToProject(app, mduPath)),
                            string.Format("Failed to import model: {0}", mduPath));

                        // Step 5: Save Project As
                        var projectPath = Path.Combine(tempDir, @"TestProjectFolder\TestProject.dsproj");
                        Assert.True(TryPerformAction(() => app.SaveProjectAs(projectPath)),
                            string.Format("Failed to save project before running the model: {0}", projectPath));

                        // Step 6: Close Project
                        Assert.True(TryPerformAction(() => app.CloseProject()),
                            string.Format("Failed to close project after import: {0}", projectPath));

                        // Step 7: Re-Open Project
                        Assert.True(TryPerformAction(() => app.OpenProject(projectPath)),
                            string.Format("Failed to reopen project: {0}", projectPath));

                        // Step 8: Validation of the model
                        WaterFlowFMModel fmModel = null;
                        Assert.True(TryPerformAction(() => ValidateFMModel(app, out fmModel)),
                            string.Format("Failed to validate model: {0}", fmModel.Name));

                        // Step 9: Dimr Export of FM model
                        Assert.True(TryPerformAction(() => ExportDimrConfiguration<WaterFlowFMModel>(tempDir, fmModel)),
                            string.Format("Failed to export dimr configuration for model: {0}", fmModel.Name));

                        // Step 10: Adjust Time Settings (10 time steps)
                        Assert.True(TryPerformAction(() => AdjustTimeSettings(fmModel)),
                            string.Format("Failed to adjust time settings for model: {0}", fmModel.Name));

                        // Step 11: Run FM model
                        Assert.True(TryPerformAction(() => ActivityRunner.RunActivity(fmModel)),
                            string.Format("Failed to run model: {0}", fmModel.Name));

                        // Step 12: Save Project
                        Assert.True(TryPerformAction(() => app.SaveProject()),
                            string.Format("Failed to save project after running the model: {0}", projectPath));

                        // Step 13: Close Project
                        Assert.True(TryPerformAction(() => app.CloseProject()),
                            string.Format("Failed to close project after running the model: {0}", projectPath));
                    };

                    WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
                }
            });
        }
        
        [TestCase("c01_sobek-rijn-j17_5-v1/sobek-rijn-j17_5-v1.zip", @"sobek-rijn-j17_5-v1\sobek-rijn-j17_5-v1.dsproj")]
        [TestCase("c02_sobek-maas-j17_5-v1/sobek-maas-j17_5-v1.zip", @"sobek-maas-j17_5-v1\sobek-maas-j17_5-v1.dsproj")]
        [TestCase("c03_sobek-rmm-j15_5-v1/sobek-rmm-j15_5-v1.zip", @"sobek-rmm-j15_5-v1\sobek-rmm-j15_5-v1.dsproj")]
        [TestCase("c04_sobek-ovd-j14_5-v1/sobek-ovd-j14_5-v1.zip", @"sobek-ovd-j14_5-v1\sobek-ovd-j14_5-v1.dsproj")]
        [TestCase("c05_sobek-markermeer-j10_5-v1/sobek-markermeer-j10_5-v1.zip", @"sobek-markermeer-j10_5-v1\sobek-markermeer-j10_5-v1_rvw2006.dsproj")]
        [TestCase("c06_sobek-ym_ijvd-j16_05-v1/sobek-ym_ijd-j16_05-v1.zip", @"sobek-ym_ijd-j16_05-v1\sobek-ym_ijd-j16_05-v1_rvw2007.dsproj")]
        [TestCase("c07_sobek-veluwerandmeren-j10-v1/sobek-veluwerandmeren-j10_5-v1.zip", @"sobek-veluwerandmeren-j10_5-v1\sobek-vrm-j10_5-v1.dsproj")]
        [TestCase("c08_sobek-nzk_ark-j15_5-v1/sobek-nzk_ark-j15_5-v1.zip", @"sobek-nzk_ark-j15_5-v1\sobek-nzk_ark-j15_5-v1.dsproj")]
        [TestCase("c09_sobek-oosterschelde-j12_5-v1/sobek-oosterschelde-j12_5-v1.zip", @"sobek-oosterschelde-j12_5-v1\t18.dsproj" )]
        [TestCase("c10_sobek-vozo-j12_5-v1_201312stav/sobek-vozo-j12_5-v1_201312stav.zip", @"sobek-vozo-j12_5-v1_201312stav\sobek-vozo-j12_5-v1_201312stav.dsproj")]
        [TestCase("c11_sobek-mlnbk_j14_5-v1/sobek-mlnbk-j14_5-v1.zip", @"sobek-mlnbk-j14_5-v1\sobek-mlnbk-j14_5-v1.dsproj" )]
        [TestCase("c12_sobek-twentekanaal-j10_5-v1/sobek-twentekanaal-j10_5-v1.zip", @"sobek-twentekanaal-j10_5-v1\sobek-twentekanaal_j10_5-v1.dsproj")]
        [TestCase("c13_sobek-meuse-j99_5_v2/sobek-meuse-j99_5-v2.zip", @"sobek-meuse-j99_5-v2\sobek-meuse-j99_5-v2.dsproj" )]
        [TestCase("c14_sobek-duitse-vecht/Duitse_Vecht.zip", @"Vecht_Ohne_Emlichheim_validatie1.dsproj")]
        [TestCase("c15_coupled_sobek-maas-j17_5-v1_meuse-j99_5-v3/sobek-meuse-j99_5-v3_20_20_sobek-maas-j17_5-v1.zip", @"sobek-meuse-j99_5-v3.dsproj" )]
        [TestCase("c16_coupled_sobek-ovd_dv-j14_5-v1/sobek-ovd_dv-j14_5-v1.zip", @"sobek-ovd_dv-j14_5-v1.dsproj" )]

        public void SOBEK3_AcceptanceModelTest(string relativeZipFilePath, string relativeDsProjFilePath)
        {
            DoInTemporaryDirectory(tempDir =>
            {
                // Step 1: Download
                var localZipFilePath = string.Empty;
                Assert.True(TryPerformAction(() => DownloadZip(SOBEK3_AcceptanceModelsRepository, relativeZipFilePath, tempDir, out localZipFilePath)),
                    string.Format("Failed to download zip file: {0}", relativeZipFilePath));

                // Step 2: Unzip
                Assert.True(TryPerformAction(() => UnzipModel(localZipFilePath)),
                    string.Format("Failed to unzip file: {0}", localZipFilePath));

                // Step 3: using(running GUI) add correct plugins for SOBEK3
                using (var gui = new DeltaShellGui())
                {
                    //load the plugins
                    gui.Plugins.Add(new DimrGuiPlugin());
                    gui.Plugins.Add(new CommonToolsGuiPlugin());
                    gui.Plugins.Add(new FlowFMGuiPlugin());
                    gui.Plugins.Add(new HydroModelGuiPlugin());
                    gui.Plugins.Add(new NetworkEditorGuiPlugin());
                    gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                    gui.Plugins.Add(new RainfallRunoffGuiPlugin());
                    gui.Plugins.Add(new RealTimeControlGuiPlugin());
                    gui.Plugins.Add(new ScriptingGuiPlugin());
                    gui.Plugins.Add(new SharpMapGisGuiPlugin());
                    gui.Plugins.Add(new SobekImportGuiPlugin());
                    gui.Plugins.Add(new ToolboxGuiPlugin());
                    gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
                    gui.Plugins.Add(new WaterQualityModelGuiPlugin());
                    
                    
                    var app = gui.Application;
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new FewsApplicationPlugin());
                    app.Plugins.Add(new FlowFMApplicationPlugin());
                    app.Plugins.Add(new HydroModelApplicationPlugin());
                    app.Plugins.Add(new NetCdfApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new RainfallRunoffApplicationPlugin());
                    app.Plugins.Add(new RealTimeControlApplicationPlugin());
                    app.Plugins.Add(new SobekImportApplicationPlugin());
                    app.Plugins.Add(new ScriptingApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new ToolboxApplicationPlugin());
                    app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                    gui.Run();

                    Action mainWindowShown = delegate
                    {
                        // Step 4: Open Project
                        var projectPath = Path.Combine(tempDir, relativeDsProjFilePath);
                        Assert.True(TryPerformAction(() => app.OpenProject(projectPath)),
                            string.Format("Failed to open project: {0}", projectPath));

                        // Step 5: Save Project
                        Assert.True(TryPerformAction(() => app.SaveProject()),
                            string.Format("Failed to save project before running the model: {0}", projectPath));

                        // Step 6: Close Project
                        Assert.True(TryPerformAction(() => app.CloseProject()),
                            string.Format("Failed to close project after initial open: {0}", projectPath));

                        // Step 7: Re-Open Project
                        Assert.True(TryPerformAction(() => app.OpenProject(projectPath)),
                            string.Format("Failed to reopen project: {0}", projectPath));

                        // Step 8: Validation of the model
                        HydroModel hydroModel = null;
                        Assert.True(TryPerformAction(() => ValidateHydroModel(app, out hydroModel)),
                            string.Format("Failed to validate model: {0}", hydroModel.Name));

                        // Step 9: Dimr Export of HydroModel
                        Assert.True(TryPerformAction(() => ExportDimrConfiguration<HydroModel>(tempDir, hydroModel)),
                            string.Format("Failed to export dimr configuration for model: {0}", hydroModel.Name));

                        // Step 10: Adjust Time Settings (10 time steps)
                        
                        Assert.True(TryPerformAction(() => AdjustTimeSettings(hydroModel)),
                            string.Format("Failed to adjust time settings for model: {0}", hydroModel.Name));

                        // Step 11: Run HydroModel (All Sub-models workflow)
                        Assert.True(TryPerformAction(() => RunHydroModel(hydroModel)),
                            string.Format("Failed to run model: {0}", hydroModel.Name));

                        // Step 12: Save Project
                        Assert.True(TryPerformAction(() => app.SaveProject()),
                            string.Format("Failed to save project after running the model: {0}", projectPath));

                 
                        // Step 13: Close Project
                        Assert.True(TryPerformAction(() => app.CloseProject()),
                            string.Format("Failed to close project after running the model: {0}", projectPath));
                    };

                    WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
                }
            });
        }
        #endregion

        #region HelperFunctions
        /// <summary>
        /// Used to perform an Action (or series of Operations) within a temporary directory
        /// Temporary directory is deleted after Action is completed
        /// </summary>
        /// <param name="action">The Action to perform</param>
        private static void DoInTemporaryDirectory(Action<string> action)
        {
            var tempDir = FileUtils.CreateTempDirectory();
            try
            {
                action.Invoke(tempDir);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                FileUtils.DeleteIfExists(tempDir);
            }
        }

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
        #endregion

        #region CommonOperations
        private static void DownloadZip(string repositoryAddress, string relativeZipFilePath, string tempDir, out string localZipFilePath)
        {
            var zipFileName = Path.GetFileName(relativeZipFilePath);
            if(string.IsNullOrEmpty(zipFileName))
                throw new ArgumentException(string.Format("Unable to retrieve Zip File Name: {0}", relativeZipFilePath));

            localZipFilePath = Path.Combine(tempDir, zipFileName);
            using (var client = new WebClient() {Credentials = new NetworkCredential(CredentialsUser, CredentialsPwd)})
            {
                var repositoryZipFilePath = Path.Combine(repositoryAddress, relativeZipFilePath);
                client.DownloadFile(repositoryZipFilePath, localZipFilePath);
            }

            if(!File.Exists(localZipFilePath))
                throw new FileNotFoundException(string.Format("File does not exist: {0}", localZipFilePath));
        }

        private static void UnzipModel(string localZipFilePath)
        {
            var extractPath = new FileInfo(localZipFilePath).DirectoryName;
            ZipFileUtils.Extract(localZipFilePath, extractPath);
        }

        private static void ImportFlowFMModelAndAddToProject(IApplication app, string mduPath)
        {
            var fmModel = new WaterFlowFMModel(mduPath);
            app.Project.RootFolder.Items.Add(fmModel);
        }

        private static void ValidateHydroModel(IApplication app, out HydroModel hydroModel)
        {
            hydroModel = GetRootModel<HydroModel>(app);
            var report = hydroModel.Validate();
            CheckValidationErrors(report);
        }

        private static void ValidateFMModel(IApplication app, out WaterFlowFMModel fmModel)
        {
            fmModel = GetRootModel<WaterFlowFMModel>(app);
            var report = fmModel.Validate();
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

        private static void RunHydroModel(HydroModel hydroModel)
        {
            // We get the workflow that runs all models in parallel
            var workflow = hydroModel.Workflows.FirstOrDefault(w => w.Activities.Count == hydroModel.Activities.Count);
            if(workflow == null) throw new NullReferenceException("Unable to get Workflow (run all models in parallel)");

            hydroModel.CurrentWorkflow = workflow;
            
            // Run model
            ActivityRunner.RunActivity(hydroModel);
        }
        
        private static void ExportDimrConfiguration<T>(string tempDir, T model)
        {
            var dimrExportDir = Path.Combine(tempDir, "Dimr_Export");
            FileUtils.CreateDirectoryIfNotExists(dimrExportDir);

            var exporter = new DHydroConfigXmlExporter { ExportFilePath = Path.Combine(dimrExportDir, "dimr.xml") };
            if (!exporter.Export(model, null)) throw new Exception();
        }
        #endregion
    }
}
