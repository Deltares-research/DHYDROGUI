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
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class HydroModelAcceptanceTest
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

        private static HydroModel ImportModel(string relativeMduPath, string localZipPath, string extractPath)
        {
            ExtractZipToCleanDirectoryAndCheckContent(localZipPath, extractPath);

            var mduPath = Path.Combine(extractPath, relativeMduPath);
            Assert.IsTrue(File.Exists(mduPath), string.Format("File does not exist: {0}", mduPath));

            var model = new HydroModel();
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

        #endregion

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

        [TestCaseSource(nameof(BasicModelsCase))]
        [Category(TestCategory.WindowsForms)]
        public void OpenModel_WithGui(string relativeZipUrl, string relativeMduPath)
        {
            var localZipPath = GetLocalZipPath(relativeZipUrl);
            Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

            var extractPath = GetZipExtractPath(localZipPath);
            var dsProjPath = OpenDsProjAndCheckNotNull(relativeMduPath, localZipPath, extractPath);

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
                        app.SaveProjectAs(dsProjPath);
                        app.OpenProject(dsProjPath);
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
        [Category(TestCategory.WindowsForms)]
        public void OpenModel_WithApplication(string relativeZipUrl, string relativeMduPath)
        {
            var localZipPath = GetLocalZipPath(relativeZipUrl);
            Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

            var extractPath = GetZipExtractPath(localZipPath);
            var dsProjPath = OpenDsProjAndCheckNotNull(relativeMduPath, localZipPath, extractPath);
            // setup
            var flow1DModel = new WaterFlowModel1D();
            var hydroModel = new HydroModel();
            hydroModel.Activities.Add(flow1DModel);

            try
            {
                using (var gui = new DeltaShellApplication())
                {
                    gui.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    gui.Plugins.Add(new CommonToolsApplicationPlugin());
                    gui.Plugins.Add(new SharpMapGisApplicationPlugin());
                    gui.Plugins.Add(new NetworkEditorApplicationPlugin());
                    gui.Plugins.Add(new RealTimeControlApplicationPlugin());
                    gui.Plugins.Add(new WaterQualityModelApplicationPlugin());
                    gui.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                    gui.Plugins.Add(new RainfallRunoffApplicationPlugin());

                    gui.Run();

                    Action mainWindowShown = delegate
                    {
                        gui.SaveProjectAs(dsProjPath);
                        gui.OpenProject(dsProjPath);
                    };

                  //  WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
                }

            }
            finally
            {
                FileUtils.DeleteIfExists(extractPath);
            }
        }

        [TestCaseSource(nameof(BasicModelsCase))]
        [Category(TestCategory.WindowsForms)]
        public void Open_Run(string relativeZipUrl, string relativeMduPath)
        {
            var localZipPath = GetLocalZipPath(relativeZipUrl);
            Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

            var extractPath = GetZipExtractPath(localZipPath);
            var dsProjPath = OpenDsProjAndCheckNotNull(relativeMduPath, localZipPath, extractPath);


            // setup
            var flow1DModel = new RainfallRunoffModel();
            var hydroModel = new HydroModel();
            hydroModel.Activities.Add(flow1DModel);

            try
            {
                using (var gui = new DeltaShellGui())
                {
                    var app = gui.Application;
                   
                    var hydroModel2 = new HydroModelApplicationPlugin();
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new RealTimeControlApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                    app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                    app.Plugins.Add(new RainfallRunoffApplicationPlugin());
                    app.Plugins.Add(hydroModel2);

                    gui.Run();

              
                    Action mainWindowShown = delegate
                    {

                  
                        var project = app.Project;
                        project.RootFolder.Add(hydroModel);
                        var rainfallRunoffModel = hydroModel.Activities.OfType<RainfallRunoffModel>().FirstOrDefault();
                        Assert.NotNull(rainfallRunoffModel);
                        app.SaveProjectAs(dsProjPath);
                        var newModel = app.Project.RootFolder.Items.OfType<RainfallRunoffModel>().FirstOrDefault();

                        app.OpenProject(dsProjPath);

                        Assert.IsNotNull(newModel);

                        //validate model

                        var report = newModel.Validate();
                        Assert.AreEqual(0, report.ErrorCount, $"Report issues: {report.AllErrors.Select(e => e.Message)}");

                        //Run model


                        ActivityRunner.RunActivity(hydroModel);

                        Assert.AreEqual(ActivityStatus.Cleaned, hydroModel.Status);
                        Assert.IsFalse(hydroModel.OutputIsEmpty);

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
        [Category(TestCategory.WindowsForms)]
        public void Open_Run_Save(string relativeZipUrl, string relativeMduPath)
        {

        }



        private static string OpenDsProjAndCheckNotNull(string relativeMduPath, string localZipPath, string extractPath)
        {
            ExtractZipToCleanDirectoryAndCheckContent(localZipPath, extractPath);

            var dsProjPath = Path.Combine(extractPath, relativeMduPath);
            Assert.IsTrue(File.Exists(dsProjPath), string.Format("File does not exist: {0}", dsProjPath));
           
            return dsProjPath;
        }


    }
}
