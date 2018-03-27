using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [Category("Build.Acceptance")]
    [Category(TestCategory.Slow)]
    public class WaterFlowModel1DAcceptanceTest
    {
        /* Until we decide on using a checkout repository or not we will use the remote repository. */
        private const string AcceptanceModelsRepository = "https://repos.deltares.nl/repos/DSCTestbench/trunk/cases/e110_delft3dfm_suite/f01_acceptance_models/";
        private const string CredentialsUser = "dscbuildserver";
        private const string CredentialsPwd = "Bu1lds3rv3r";

        private string testTempDir = @"C:\D-Hydro\delta-shell\issue\DELTF3DFM-1234";

        /* Models to be downloaded, if they are not included here the test that needs to use them will fail.*/
        private static IList<string> ZipModelsList
        {
            get
            {
                return new List<string>
                {
                    "sobek-markermeer-j10_5-v1.zip",
                    "sobek-oosterschelde-j12_5-v1.zip",
                   " Duitse_Vecht.zip",
                    "sobek-maas-j17_5-v1.zip",
                    "sobek-meuse-j99_5-v2.zip",
                    "sobek-meuse-j99_5-v3_20_20_sobek-maas-j17_5-v1.zip",
                    "sobek-mlnbk-j14_5-v1.zip",
                    "sobek-nzk_ark-j15_5-v1.zip",
                    "sobek-oosterschelde-j12_5-v1.zip",
                    "sobek-ovd_dv-j14_5-v1.zip",
                    "sobek-ovd-j14_5-v1.zip",
                    "sobek-rijn-j17_5-v1.zip",
                    "sobek-rmm-j15_5-v1.zip",
                    "sobek-twentekanaal-j10_5-v1.zip",
                    "sobek-veluwerandmeren-j10_5-v1.zip",
                    "sobek-vozo-j12_5-v1_201312stav.zip",
                    "sobek-ym_ijd-j16_05-v1.zip"
                };
            }
        }

        #region SetUps / TearDowns

        [TestFixtureSetUp]
        public void Init()
        {
            //	        testTempDir = FileUtils.CreateTempDirectory();
            //	        foreach (var zipModel in ZipModelsList)
            //	        {
            //	            var remoteZipPath = GetRemoteZipPath(zipModel);
            //	            var localZipPath = GetLocalZipPath(zipModel);
            //
            //                VerifyAndDownloadZip(remoteZipPath, localZipPath);
            //	        }
        }

        [TestFixtureTearDown]
        public void CleanUp()
        {
            //	        FileUtils.DeleteIfExists(testTempDir);
        }
        #endregion

        #region Helpers
        private static WaterFlowModel1D Get1DModelFromIntegratedModel(IEnumerable<IModel> models)
        {
            //Extract 1D Model
            var hydroModel = models.OfType<HydroModel>().FirstOrDefault();
            Assert.IsNotNull(hydroModel);

            var waterFlowModel1D = hydroModel.Activities.OfType<WaterFlowModel1D>().FirstOrDefault();
            Assert.IsNotNull(waterFlowModel1D, "No WaterFlow 1D model was found.");

            return waterFlowModel1D;
        }


        private static string GetDsProjPath(string relativeMduPath, string localZipPath, string extractPath)
        {
            ExtractZipToCleanDirectoryAndCheckContent(localZipPath, extractPath);

            var dsProjPath = Path.Combine(extractPath, relativeMduPath);
            Assert.IsTrue(File.Exists(dsProjPath), string.Format("File does not exist: {0}", dsProjPath));

            return dsProjPath;
        }
        private static WaterFlowModel1D ImportModelAndCheckNotNull(string relativeMd1dPath, string localZipPath, string extractPath)
        {
            ExtractZipToCleanDirectoryAndCheckContent(localZipPath, extractPath);

            var mduPath = Path.Combine(extractPath, relativeMd1dPath);
            Assert.IsTrue(File.Exists(mduPath), string.Format("File does not exist: {0}", mduPath));

            var model = new WaterFlowModel1D(mduPath);
            Assert.IsNotNull(model);

            return model;
        }

        private static void Open1DWindow(DeltaShellGui gui, WaterFlowModel1D model)
        {
            try
            {
                gui.CommandHandler.OpenView(model, typeof(WaterFlowModel1D));
            }
            catch (Exception e)
            {
                Assert.Fail("Test failed while opening the modelview. {0}", e.Message);
            }
        }
        private static string GetRemoteZipPath(string relativeZipCasePath)
        {
            return string.Concat(AcceptanceModelsRepository, relativeZipCasePath);
        }

        private string GetLocalZipPath(string zipName)
        {
            var zipFile = Path.GetFileName(zipName);
            Assert.IsNotNull(zipFile);
            return Path.Combine(testTempDir, zipFile);
        }

        private string GetZipName(string zipUrl)
        {
            var zipExtension = ".zip";
            var splitString = zipUrl.Substring(0, zipUrl.Length - zipExtension.Length);
            return splitString;
        }

        private string GetZipExtractPath(string localZipPath)
        {
            var targetDirectory = GetZipName(localZipPath);
            var parentDirectory = Path.GetDirectoryName(localZipPath);
            Assert.IsNotNull(parentDirectory);
            return Path.Combine(parentDirectory, targetDirectory);
        }

        private static void ExtractZipToCleanDirectoryAndCheckContent(string localZipPath, string extractPath)
        {
            //We want a clean extraction.
            FileUtils.DeleteIfExists(extractPath);

            //Extracting the zip content
            ZipFileUtils.Extract(localZipPath, extractPath);

            //Make sure we have extracted files.
            Assert.IsTrue(Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories).Length > 1);
        }

        private static WaterFlowModel1D ImportModel(string relativeMduPath, string localZipPath, string extractPath)
        {
            ExtractZipToCleanDirectoryAndCheckContent(localZipPath, extractPath);

            var mduPath = Path.Combine(extractPath, relativeMduPath);
            Assert.IsTrue(File.Exists(mduPath), string.Format("File does not exist: {0}", mduPath));

            var model = new WaterFlowModel1D();
            Assert.IsNotNull(model);

            return model;
        }

        private static void VerifyAndDownloadZip(string remoteZipPath, string localZipPath)
        {
            if (File.Exists(localZipPath)) return;

            using (WebClient client = new WebClient())
            {
                // In case you need authentication...
                client.Credentials = new NetworkCredential(CredentialsUser, CredentialsPwd);
                Assert.IsFalse(client.IsBusy);
                try
                {
                    client.DownloadFile(remoteZipPath, localZipPath);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            }

            Assert.IsTrue(File.Exists(localZipPath),
                string.Format("Acceptance model {0}was not downloaded correctly.", remoteZipPath));
        }

        private static void AddPluginsToApp(DeltaShellApplication app)
        {
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new WaterQualityModelApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new RainfallRunoffApplicationPlugin());
            app.Plugins.Add(new WaveApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
        }

        #endregion

        #region TestCases

        static object[] BasicModelsCase =
        {
            new object[] { "sobek-oosterschelde-j12_5-v1.zip", @"sobek-oosterschelde-j12_5-v1\t18.dsproj" },
            //new object[] { "Duitse_Vecht.zip", @"Vecht_Ohne_Emlichheim_validatie1.dsproj" },
            //new object[] { "sobek-maas-j17_5-v1.zip", @"sobek-maas-j17_5-v1\sobek-maas-j17_5-v1.dsproj"},
            //new object[] { "sobek-meuse-j99_5-v2.zip", @"sobek-meuse-j99_5-v2\sobek-meuse-j99_5-v2.dsproj" },
            //new object[] { "sobek-markermeer-j10_5-v1.zip", @"sobek-markermeer-j10_5-v1\sobek-markermeer-j10_5-v1_rvw2006.dsproj" },
            //new object[] { "sobek-meuse-j99_5-v3_20_20_sobek-maas-j17_5-v1.zip", @"sobek-meuse-j99_5-v3.dsproj" },
            //new object[] { "sobek-mlnbk-j14_5-v1.zip", @"sobek-mlnbk-j14_5-v1\sobek-mlnbk-j14_5-v1.dsproj" },
            //new object[] {"sobek-nzk_ark-j15_5-v1.zip", @"sobek-nzk_ark-j15_5-v1\sobek-nzk_ark-j15_5-v1.dsproj"},
            //new object[] {"sobek-ovd-j14_5-v1.zip",@"sobek-ovd-j14_5-v1\sobek-ovd-j14_5-v1.dsproj"},
            //new object[] {"sobek-rijn-j17_5-v1.zip", @"sobek-rijn-j17_5-v1\sobek-rijn-j17_5-v1.dsproj"},

            //new object[] {"sobek-rmm-j15_5-v1.zip", @"sobek-rmm-j15_5-v1\sobek-rmm-j15_5-v1.dsproj"},
            //new object[] { "sobek-twentekanaal-j10_5-v1.zip", @"sobek-twentekanaal-j10_5-v1\sobek-twentekanaal_j10_5-v1.dsproj"},
            //new object[] {"sobek-veluwerandmeren-j10_5-v1.zip", @"sobek-veluwerandmeren-j10_5-v1\sobek-vrm-j10_5-v1.dsproj"},
            //new object[] {"sobek-vozo-j12_5-v1_201312stav.zip", @"sobek-vozo-j12_5-v1_201312stav\sobek-vozo-j12_5-v1_201312stav.dsproj"},
            //new object[] {"sobek-ym_ijd-j16_05-v1.zip", @"sobek-ym_ijd-j16_05-v1\sobek-ym_ijd-j16_05-v1_rvw2007.dsproj"},
        }; 
        #endregion

        #region Tests
        [TestCaseSource(nameof(BasicModelsCase))]
        [Category(TestCategory.WindowsForms)]
        public void OpenModel_WithGui(string relativeZipUrl, string relativeDsProjPath)
        {
            var localZipPath = GetLocalZipPath(relativeZipUrl);
            Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

            var extractPath = GetZipExtractPath(localZipPath);
            var dsProjPath = GetDsProjPath(relativeDsProjPath, localZipPath, extractPath);

            try
            {
                using (var gui = new DeltaShellGui())
                {
                    var app = gui.Application;
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new RealTimeControlApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                    app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                    app.Plugins.Add(new RainfallRunoffApplicationPlugin());

                    gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                    gui.Plugins.Add(new CommonToolsGuiPlugin());
                    gui.Plugins.Add(new SharpMapGisGuiPlugin());
                    gui.Plugins.Add(new NetworkEditorGuiPlugin());
                    gui.Plugins.Add(new HydroModelGuiPlugin());
                    gui.Plugins.Add(new RealTimeControlGuiPlugin());
                    gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
                    gui.Plugins.Add(new RainfallRunoffGuiPlugin());
                    gui.Plugins.Add(new FlowFMGuiPlugin());
                    gui.Plugins.Add(new WaveGuiPlugin());

                    gui.Run();

                    Action mainWindowShown = delegate
                    {
                        app.OpenProject(dsProjPath);

                        //check the app contains 1D models
                        var flow1DModel= Get1DModelFromIntegratedModel(app.Project.RootFolder.Models);
                        Assert.IsNotNull(flow1DModel);

                        //open 1D window
                        Open1DWindow(gui, flow1DModel);
                    };

                    WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(extractPath);
            }
        }

        [TestCaseSource(nameof(BasicModelsCase))]
        public void OpenModel_WithApplication(string relativeZipUrl, string relativeMduPath)
        {
            var localZipPath = GetLocalZipPath(relativeZipUrl);
            Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

            var extractPath = GetZipExtractPath(localZipPath);
            var dsProjPath = GetDsProjPath(relativeMduPath, localZipPath, extractPath);

            try
            {
                using (var app = new DeltaShellApplication() {IsProjectCreatedInTemporaryDirectory = true})
                {
                    AddPluginsToApp(app);

                    app.Run();
                    app.OpenProject(dsProjPath);
                    var waterFlowModel1D = Get1DModelFromIntegratedModel(app.Project.RootFolder.Models);
                    Assert.NotNull(waterFlowModel1D);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(extractPath);
            }
        }


        [TestCaseSource(nameof(BasicModelsCase))]
        [Category(TestCategory.VerySlow)]
        public void Open_Run_NoCustomTimeStep(string relativeZipUrl, string relativeDsProjPath)
        {
            var localZipPath = GetLocalZipPath(relativeZipUrl);
            Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

            var extractPath = GetZipExtractPath(localZipPath);
            var dsProjPath = GetDsProjPath(relativeDsProjPath, localZipPath, extractPath);
            try
            {
                using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
                {
                    AddPluginsToApp(app);

                    app.Run();

                    app.OpenProject(dsProjPath);
                    Assert.IsNotNull(app.Project?.RootFolder);

                    //Get integratedModel
                    var rootFolderModels = app.Project.RootFolder.Models.ToList();
                    Assert.That(rootFolderModels.Count, Is.EqualTo(1));

                    var waterFlowModel1D = Get1DModelFromIntegratedModel(rootFolderModels);

                    //validate model
                    var report = waterFlowModel1D.Validate();
                    Assert.AreEqual(0, report.ErrorCount, $"Report issues: {report.AllErrors.Select(e => e.Message)}");

                    //Run model
                    ActivityRunner.RunActivity(waterFlowModel1D);

                    Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
                    Assert.IsFalse(waterFlowModel1D.OutputIsEmpty);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(extractPath);
            }
        }

        [TestCaseSource(nameof(BasicModelsCase))]
        public void Open_Run_WithCustomTimeStep(string relativeZipUrl, string relativeDsProjPath)
        {
            var numTimeStepsToRun = 10;
            var localZipPath = GetLocalZipPath(relativeZipUrl);
            Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

            var extractPath = GetZipExtractPath(localZipPath);
            var dsProjPath = GetDsProjPath(relativeDsProjPath, localZipPath, extractPath);
            try
            {
                using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
                {
                    AddPluginsToApp(app);

                    app.Run();

                    app.OpenProject(dsProjPath);
                    Assert.IsNotNull(app.Project?.RootFolder);
                  
                    //Get integratedModel
                    var rootFolderModels = app.Project.RootFolder.Models.ToList();
                    Assert.That(rootFolderModels.Count, Is.EqualTo(1));

                    var waterFlowModel1D = Get1DModelFromIntegratedModel(rootFolderModels);

                    // adapt time settings
                    var totalRunTime = waterFlowModel1D.TimeStep.TotalSeconds * numTimeStepsToRun;
                    waterFlowModel1D.StopTime = waterFlowModel1D.StartTime.AddSeconds(totalRunTime);

                    //validate model
                    var report = waterFlowModel1D.Validate();
                    Assert.AreEqual(0, report.ErrorCount, $"Report issues: {report.AllErrors.Select(e => e.Message)}");

                    //Run model
                    ActivityRunner.RunActivity(waterFlowModel1D);

                    Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
                    Assert.IsFalse(waterFlowModel1D.OutputIsEmpty);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(extractPath);
            }
        }


        //todo
        //open-save
        [TestCaseSource(nameof(BasicModelsCase))]
        public void OpenModel_SaveMode(string relativeZipUrl, string relativeMduPath)
        {
            var localZipPath = GetLocalZipPath(relativeZipUrl);
            Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

            var extractPath = GetZipExtractPath(localZipPath);
            var dsProjPath = GetDsProjPath(relativeMduPath, localZipPath, extractPath);

            var dsProjSaveAsPath = Path.Combine(extractPath, "test.dsproj");
            FileUtils.DeleteIfExists(dsProjSaveAsPath);
            var dsProjDataSaveAsPath = Path.Combine(extractPath, "test.dsproj_data");
            Directory.Delete(dsProjDataSaveAsPath);

            try
            {
                using (var app = new DeltaShellApplication() { IsProjectCreatedInTemporaryDirectory = true })
                {
                    AddPluginsToApp(app);

                    app.Run();
                    app.OpenProject(dsProjPath);
                    var waterFlowModel1D = Get1DModelFromIntegratedModel(app.Project.RootFolder.Models);
                    Assert.NotNull(waterFlowModel1D);

                    //new folder test.dsproj_data should be created
                    app.SaveProjectAs(dsProjSaveAsPath);
                    Assert.IsTrue(File.Exists(dsProjSaveAsPath));
                    Assert.IsTrue(Directory.Exists(dsProjDataSaveAsPath));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(extractPath);
            }
        }

        //open-save-closeproject-open
        [TestCaseSource(nameof(BasicModelsCase))]
        public void OpenModel_SaveModel_ReOpen(string relativeZipUrl, string relativeMduPath)
        {
            var localZipPath = GetLocalZipPath(relativeZipUrl);
            Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

            var extractPath = GetZipExtractPath(localZipPath);
            var dsProjPath = GetDsProjPath(relativeMduPath, localZipPath, extractPath);

            var dsProjSaveAsPath = Path.Combine(extractPath, "test.dsproj");
            FileUtils.DeleteIfExists(dsProjSaveAsPath);
            var dsProjDataSaveAsPath = Path.Combine(extractPath, "test.dsproj_data");
            Directory.Delete(dsProjDataSaveAsPath);

            try
            {
                using (var app = new DeltaShellApplication() { IsProjectCreatedInTemporaryDirectory = true })
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new RealTimeControlApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                    app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                    app.Plugins.Add(new RainfallRunoffApplicationPlugin());

                    app.Run();
                    app.OpenProject(dsProjPath);
                    var waterFlowModel1D = Get1DModelFromIntegratedModel(app.Project.RootFolder.Models);
                    Assert.NotNull(waterFlowModel1D);

                    //new folder test.dsproj_data should be created
                    app.SaveProjectAs(dsProjSaveAsPath);
                    Assert.IsTrue(File.Exists(dsProjSaveAsPath));
                    Assert.IsTrue(Directory.Exists(dsProjDataSaveAsPath));
                    app.CloseProject();
                   
                    //check there is nothing
                    Assert.IsNull(app.Project);
                   
                    //open the project again
                    app.OpenProject(dsProjDataSaveAsPath);
                    var savedWaterFlowModel1D = Get1DModelFromIntegratedModel(app.Project.RootFolder.Models);
                    Assert.NotNull(savedWaterFlowModel1D);
                  
                    //check the flow1dmodel is there
                    var openedWaterFLowModel1D = Get1DModelFromIntegratedModel(app.Project.RootFolder.Models);
                    Assert.NotNull(openedWaterFLowModel1D);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(extractPath);
            }
        }

        [TestCaseSource(nameof(BasicModelsCase))]
        public void OpenModel_ExportDimrConfiguration(string relativeZipUrl, string relativeMduPath)
        {
            var localZipPath = GetLocalZipPath(relativeZipUrl);
            Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

            var extractPath = GetZipExtractPath(localZipPath);
            var dsProjPath = GetDsProjPath(relativeMduPath, localZipPath, extractPath);

            var exportFilePath = Path.Combine(extractPath, "dimr.xml");
            try
            {
                using (var app = new DeltaShellApplication() { IsProjectCreatedInTemporaryDirectory = true })
                {
                    AddPluginsToApp(app);

                    app.Run();
                    app.OpenProject(dsProjPath);
                    var waterFlowModel1D = Get1DModelFromIntegratedModel(app.Project.RootFolder.Models);
                    Assert.NotNull(waterFlowModel1D);
                    ExtractDimrConfigurationAndCheck(exportFilePath, waterFlowModel1D, extractPath);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(extractPath);
            }
        }

        private static void ExtractDimrConfigurationAndCheck(string exportFilePath, WaterFlowModel1D model, string extractPath)
        {
            var exporter = new DHydroConfigXmlExporter
            {
                ExportFilePath = exportFilePath
            };

            //Check if model is valid, otherwise it won't export.
            var report = model.Validate();
            Assert.IsFalse(report.AllErrors.Any());

            //Export and check dimr.xml file exists
            Assert.IsTrue(exporter.Export(model, null));
            Assert.IsTrue(File.Exists(exportFilePath));

            //Check exported model directory exists and contains files
            var exportedDimr = Path.Combine(extractPath, "dflow1d");
            Assert.IsTrue(Directory.Exists(exportedDimr));
            Assert.IsTrue(Directory.GetFiles(exportedDimr).Any());

            //Check exported model mdu exists in directory
            var exportedMd1d = Path.Combine(exportedDimr, string.Concat(model.Name, ".md1d"));
            Assert.IsTrue(File.Exists(exportedMd1d));
        }
        #endregion
    }
}
