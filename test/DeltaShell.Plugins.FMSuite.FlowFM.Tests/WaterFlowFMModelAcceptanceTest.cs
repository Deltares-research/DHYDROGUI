using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [Category("Build.Acceptance")]
    [Category(TestCategory.Slow)]
    [TestFixture]
    public class WaterFlowFMModelAcceptanceTest
	{
        /* Until we decide on using a checkout repository or not we will use the remote repository. */
	    private const string AcceptanceModelsRepository = "https://repos.deltares.nl/repos/DSCTestbench/trunk/cases/e110_delft3dfm_suite/f01_acceptance_models/";
	    private const string CredentialsUser = "dscbuildserver";
        private const string CredentialsPwd = "Bu1lds3rv3r";

	    /* @"C:\D-Hydro\delta-shell\issue\DELTF3DFM-1234\"*/
        private string testTempDir = string.Empty; //leave this string empty when using the remote repository

        /* Models to be downloaded, if they are not included here the test that needs to use them will fail.*/
	    private static IList<string> ZipModelsList
	    {
	        get
	        {
		        return new List<string>
		        {
			        "c05_Oosterschelde/Oosterschelde.zip",
                    "c04_Markermeer_Veluwerandmeren/d3dfm_vrm_j10-v1.zip",
                    "c03_Waal_40m/Waal_40m.zip",
                    "c02_Maas_40m/Maas_40m.zip",
                    "c02_Maas_40m/Maas_DIMR.zip",
                    "c01_Noordzeemodel/Noordzeemodel.zip",
                };
	        }
	    }

        #region SetUps / TearDowns

        [TestFixtureSetUp]
	    public void Init()
	    {
	        testTempDir = FileUtils.CreateTempDirectory();
	        foreach (var zipModel in ZipModelsList)
	        {
	            var remoteZipPath = GetRemoteZipPath(zipModel);
	            var localZipPath = GetLocalZipPath(zipModel);

                VerifyAndDownloadZip(remoteZipPath, localZipPath);
	        }
	    }

		[TestFixtureTearDown]
	    public void CleanUp()
	    {
	        FileUtils.DeleteIfExists(testTempDir);
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

	    private static WaterFlowFMModel ImportModelAndCheckNotNull(string relativeMduPath, string localZipPath, string extractPath)
	    {
	        ExtractZipToCleanDirectoryAndCheckContent(localZipPath, extractPath);

	        var mduPath = Path.Combine(extractPath, relativeMduPath);
	        Assert.IsTrue(File.Exists(mduPath), string.Format("File does not exist: {0}", mduPath));

	        var model = new WaterFlowFMModel(mduPath);
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
	            catch (Exception)
	            {
	                //We want to continue with the rest of downloads
	                return;
	            }
            }
	    }

	    private static IEnumerable<IWaterFlowFMModel> CheckModelExistsOrNot(Folder rootFolder, bool expected)
	    {
	        var models = rootFolder.Models.OfType<IWaterFlowFMModel>().ToList();
	        Assert.AreEqual(expected, models.Any()); //check that there are any model

	        return models;
	    }

	    private static void OpenFmWindow(DeltaShellGui gui, WaterFlowFMModel model)
	    {
	        try
	        {
	            gui.CommandHandler.OpenView(model, typeof(WaterFlowFMModelView));
	        }
	        catch (Exception e)
	        {
	            Assert.Fail("Test failed while opening the modelview. {0}", e.Message);
	        }
	    }

	    private static void AddAppAndGuiPlugins(IApplication app, DeltaShellGui gui)
	    {
	        AddAppPlugins(app);
	        gui.Plugins.Add(new CommonToolsGuiPlugin());
	        gui.Plugins.Add(new ProjectExplorerGuiPlugin());
	        gui.Plugins.Add(new NetworkEditorGuiPlugin());
	        gui.Plugins.Add(new SharpMapGisGuiPlugin());
	        gui.Plugins.Add(new FlowFMGuiPlugin());
	    }

	    private static void AddAppPlugins(IApplication app)
	    {
	        app.Plugins.Add(new NHibernateDaoApplicationPlugin());
	        app.Plugins.Add(new CommonToolsApplicationPlugin());
	        app.Plugins.Add(new FlowFMApplicationPlugin());
	        app.Plugins.Add(new NetworkEditorApplicationPlugin());
	        app.Plugins.Add(new SharpMapGisApplicationPlugin());
	    }

        #endregion

        #region Test Cases

	    static object[] BasicOperationsCases =
	    {
	        new object[] { "c05_Oosterschelde/Oosterschelde.zip", @"Filebased\e02.mdu" },
	        new object[] { "c04_Markermeer_Veluwerandmeren/d3dfm_vrm_j10-v1.zip", @"FlowFM_uniWind.mdu" },
            new object[] { "c04_Markermeer_Veluwerandmeren/d3dfm_vrm_j10-v1.zip", @"FlowFM_varWind.mdu" },
	        new object[] { "c03_Waal_40m/Waal_40m.zip", @"Waal_40m.dsproj_data\Waal_40m\Waal_40m.mdu" },
	        new object[] { "c02_Maas_40m/Maas_40m.zip", @"Maas_40m.dsproj_data\Maas_j14_5-v2\Maas_j14_5-v2.mdu" },
	        new object[] { "c02_Maas_40m/Maas_DIMR.zip", @"DIMR\dflowfm\Maas_j14_5-v2.mdu" },
	        new object[] { "c01_Noordzeemodel/Noordzeemodel.zip", @"DeltaShell_Noordzeemodel\noordzee_2d.mdu" },
	    };

        static object[] RunModelCases = 
	    {
	        /* Models that hang or pending investigation should be included in the case below instead.*/
            new object[] { "c05_Oosterschelde/Oosterschelde.zip", @"Filebased\e02.mdu" },
            new object[] { "c04_Markermeer_Veluwerandmeren/d3dfm_vrm_j10-v1.zip", @"FlowFM_uniWind.mdu" },
            new object[] { "c03_Waal_40m/Waal_40m.zip", @"Waal_40m.dsproj_data\Waal_40m\Waal_40m.mdu" },
            new object[] { "c02_Maas_40m/Maas_40m.zip", @"Maas_40m.dsproj_data\Maas_j14_5-v2\Maas_j14_5-v2.mdu" },
            new object[] { "c02_Maas_40m/Maas_DIMR.zip", @"DIMR\dflowfm\Maas_j14_5-v2.mdu" },
            new object[] { "c01_Noordzeemodel/Noordzeemodel.zip", @"DeltaShell_Noordzeemodel\noordzee_2d.mdu" },
        };

        /*Models that require investigation*/
        static object[] NotRunnableModelCases = 
	    {
	        new object[] {"c04_Markermeer_Veluwerandmeren/d3dfm_vrm_j10-v1/FlowFM_varWind", "Model hangs while running."},
	    };

        #endregion


	    #region Tests

	    [Test]
	    public void CheckIfZipModelsListExist()
	    {
            var notFoundZips = new List<string>();
	        foreach (var model in ZipModelsList)
	        {
	            var localZipPath = GetLocalZipPath(model);
	            if (!File.Exists(localZipPath))
	            {
                    notFoundZips.Add(localZipPath);
	            }
	        }

	        if (notFoundZips.Any())
	        {
	            Assert.Fail("The following files were not found {0}", notFoundZips);
            }
	    }

        [Test]
	    [TestCaseSource(nameof(BasicOperationsCases))]
		public void ImportModel(string relativeZipUrl, string relativeMduPath)
        {
            var localZipPath = GetLocalZipPath(relativeZipUrl);

            Assert.IsTrue(File.Exists(localZipPath), string.Format("The zip file {0} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.", relativeZipUrl));

            var extractPath = GetZipExtractPath(localZipPath);
			try
			{
			    ImportModelAndCheckNotNull(relativeMduPath, localZipPath, extractPath);
			}
			finally
			{
			    FileUtils.DeleteIfExists(extractPath);
            }
		}

	    #region Run Models

	    [TestCaseSource(nameof(NotRunnableModelCases))]
	    public void TestCasesThatRequireInvestigation(string testCaseName, string testCaseIssue)
	    {
	        Assert.Fail("{0} is marked as not runnable. Reason: {1}", testCaseName, testCaseIssue);
	    }

	    [TestCaseSource(nameof(RunModelCases))]
        [Category(TestCategory.VerySlow)] //very slow model, we only want to run the Acceptance + very slow during the weekends.
	    public void ImportModel_RunModel(string relativeZipUrl, string relativeMduPath)
	    {
	        var localZipPath = GetLocalZipPath(relativeZipUrl);
	        Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

	        var extractPath = GetZipExtractPath(localZipPath);
	        try
	        {
	            var model = ImportModelAndCheckNotNull(relativeMduPath, localZipPath, extractPath);

	            Assert.NotNull(model);

	            //Validate before running
	            var report = model.Validate();

	            Assert.AreEqual(0, report.ErrorCount, $"Report issues: {report.AllErrors.Select(e => e.Message)}");

	            ActivityRunner.RunActivity(model);

	            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
	        }
	        finally
	        {
	            FileUtils.DeleteIfExists(extractPath);
	        }
	    }

	    [TestCaseSource(nameof(RunModelCases))]
		public void ImportModel_RunModel_WithCustomTimeSteps(string relativeZipUrl, string relativeMduPath)
		{
			var numTimeStepsToRun = 10;
			var localZipPath = GetLocalZipPath(relativeZipUrl);
			Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

		    var extractPath = GetZipExtractPath(localZipPath);
			try
			{
			    var model = ImportModelAndCheckNotNull(relativeMduPath, localZipPath, extractPath);

			    Assert.NotNull(model);

				// adapt time settings
				var totalRunTime = model.TimeStep.TotalSeconds * numTimeStepsToRun;
				model.StopTime = model.StartTime.AddSeconds(totalRunTime);

				//Validate before running
				var report = model.Validate();

			    Assert.AreEqual(0, report.ErrorCount, $"Report issues: {report.AllErrors.Select(e => e.Message)}");

				ActivityRunner.RunActivity(model);

                Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
			}
			finally
			{
				FileUtils.DeleteIfExists(extractPath);
			}
		}

	    [TestCaseSource(nameof(RunModelCases))]
	    [Category(TestCategory.VerySlow)] //very slow model, we only want to run the Acceptance + very slow during the weekends.
        [Category(TestCategory.WindowsForms)]
	    public void ImportModel_RunModel_WithGui(string relativeZipUrl, string relativeMduPath)
	    {
	        var localZipPath = GetLocalZipPath(relativeZipUrl);
	        Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");
	        var extractPath = GetZipExtractPath(localZipPath);
	        try
	        {
	            ExtractZipToCleanDirectoryAndCheckContent(localZipPath, extractPath);

	            var mduPath = Path.Combine(extractPath, relativeMduPath);
	            Assert.IsTrue(File.Exists(mduPath), $"File does not exist: {mduPath}");
     
	            using (var gui = new DeltaShellGui())
	            {
	                //load the plugins
	                var app = gui.Application;
	                AddAppAndGuiPlugins(app, gui);
	                gui.Run();

	                Action mainWindowShown = delegate
	                {
	                    //Importing the model
	                    var model = new WaterFlowFMModel(mduPath);
	                    var project = app.Project;
	                    project.RootFolder.Add(model);

	                    Assert.IsNotNull(model);
	                    Assert.That(project.RootFolder.Models.Count(), Is.EqualTo(1));

	                    //Open FM window
	                    OpenFmWindow(gui, model);
                      
                        //validate model
	                    var report = model.Validate();
	                    Assert.AreEqual(0, report.ErrorCount, $"Report issues: {report.AllErrors.Select(e => e.Message)}");

                        ActivityRunner.RunActivity(model);

                        Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
	                    Assert.IsFalse(model.OutputIsEmpty);
	                };

	                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
	            }
	        }
	        finally
	        {
	            FileUtils.DeleteIfExists(extractPath);
	        }
	    }

	    #endregion

	    #region DIMR Configuration

	    [TestCaseSource(nameof(BasicOperationsCases))]
		public void ImportModel_ExportDimrConfiguration_CheckDimrFiles(string relativeZipUrl, string relativeMduPath)
		{
			var localZipPath = GetLocalZipPath(relativeZipUrl);

		    Assert.IsTrue(File.Exists(localZipPath), string.Format("The zip file {0} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.", relativeZipUrl));

		    var extractPath = GetZipExtractPath(localZipPath);
		    var exportFilePath = Path.Combine(extractPath, "dimr.xml");
		    try
		    {
		        var model = ImportModelAndCheckNotNull(relativeMduPath, localZipPath, extractPath);
		        Assert.NotNull(model);

		        //Validate
		        var report = model.Validate();
		        Assert.AreEqual(0, report.ErrorCount, $"Report issues: {report.AllErrors.Select(e => e.Message)}");

                ExtractDimrConfigurationAndCheck(exportFilePath, model, extractPath);
		    }
		    finally
		    {
		        FileUtils.DeleteIfExists(extractPath);
		    }
		}

	    [TestCaseSource(nameof(BasicOperationsCases))]
	    [Category(TestCategory.WindowsForms)]
	    public void ImportModel_ExportDimrConfiguration_CheckDimrFiles_WithGui(string relativeZipUrl, string relativeMduPath)
	    {
	        var localZipPath = GetLocalZipPath(relativeZipUrl);

	        Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

	        var extractPath = GetZipExtractPath(localZipPath);
	        var dsProjPath = Path.Combine(extractPath, "Project1.dsproj");
	        var exportFilePath = Path.Combine(extractPath, "dimr.xml");
            try
	        {
	            ExtractZipToCleanDirectoryAndCheckContent(localZipPath, extractPath);
	            var mduPath = Path.Combine(extractPath, relativeMduPath);

	            Assert.IsTrue(File.Exists(mduPath), $"File does not exist: {mduPath}");
        
	            using (var gui = new DeltaShellGui())
	            {
	                //load the plugins
	                var app = gui.Application;
	                AddAppAndGuiPlugins(app, gui);
	                gui.Run();

	                Action mainWindowShown = delegate
	                {
	                    //Importing the model
	                    var model = new WaterFlowFMModel(mduPath);

	                    Assert.IsNotNull(model);
                     
	                    var project = app.Project;
	                    project.RootFolder.Add(model);

	                    Assert.That(project.RootFolder.Models.Count(), Is.EqualTo(1));

	                    //Open FM window
	                    OpenFmWindow(gui, model);

	                    //Save project
	                    app.SaveProjectAs(dsProjPath);
                      
	                    //Export to dimr
	                    ExtractDimrConfigurationAndCheck(exportFilePath, model, extractPath);
                    };

	                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
	            }
	        }
	        finally
	        {
	            FileUtils.DeleteIfExists(extractPath);
	        }
	    }

	    private static void ExtractDimrConfigurationAndCheck(string exportFilePath, WaterFlowFMModel model, string extractPath)
	    {
	        var exporter = new DHydroConfigXmlExporter
	        {
	            ExportFilePath = exportFilePath
	        };

	        //Export and check dimr.xml file exists
	        Assert.IsTrue(exporter.Export(model, null));
	        Assert.IsTrue(File.Exists(exportFilePath));

	        //Check exported model directory exists and contains files
	        var exportedDimr = Path.Combine(extractPath, "dflowfm");
	        Assert.IsTrue(Directory.Exists(exportedDimr));
	        Assert.IsTrue(Directory.GetFiles(exportedDimr).Any());

	        //Check exported model mdu exists in directory
	        var exportedMdu = Path.Combine(exportedDimr, string.Concat(model.Name, ".mdu"));
	        Assert.IsTrue(File.Exists(exportedMdu));
	    }

	    #endregion

	    #region Save Model

	    private static void ImportModel_IntoProject_SaveProject_ReloadProject_CheckModel(string relativeMduPath, string localZipPath,
	        string extractPath, IApplication app, string dsProjPath, Action additionalAction = null)
	    {
	        //Import FM Model
	        var fmModel = ImportModelAndCheckNotNull(relativeMduPath, localZipPath, extractPath);
	        app.Project.RootFolder.Add(fmModel);

	        //Verify model is imported
	        var models = CheckModelExistsOrNot(app.Project.RootFolder, true).ToList(); //check that there are any model
	        Assert.AreEqual(1, models.Count); // check that there is one project
	        var firstImportedModel = models.FirstOrDefault();
	        Assert.IsNotNull(firstImportedModel);
	        Assert.AreEqual(fmModel, firstImportedModel);

	        //Do additional action
	        additionalAction?.Invoke();

	        //Validate
	        var report = fmModel.Validate();
	        Assert.AreEqual(0, report.ErrorCount, $"Report issues: {report.AllErrors.Select(e => e.Message)}");

	        //Save 
	        app.SaveProjectAs(dsProjPath);
	        Assert.IsTrue(File.Exists(dsProjPath));

	        //Close Project
	        app.CloseProject();
	        Assert.IsNull(app.Project);

	        //Reopen the project
	        app.OpenProject(dsProjPath);
            Assert.IsNotNull(app.Project);
	        models = CheckModelExistsOrNot(app.Project.RootFolder, true).ToList(); //check that there are any model
	        Assert.AreEqual(1, models.Count); // check that there is one project

	        //Check reloaded model is the same as the previous one.
	        var reopenedModel = models.FirstOrDefault();
	        Assert.IsNotNull(reopenedModel);
	        Assert.AreEqual(fmModel, reopenedModel);
	    }

	    [TestCaseSource(nameof(BasicOperationsCases))]
	    public void ImportModel_SaveModel_ReloadModel(string relativeZipUrl, string relativeMduPath)
	    {
	        var localZipPath = GetLocalZipPath(relativeZipUrl);
	        Assert.IsTrue(File.Exists(localZipPath), string.Format("The zip file {0} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.", relativeZipUrl));
	        var extractPath = GetZipExtractPath(localZipPath);
	        var mduPath = Path.Combine(extractPath, relativeMduPath);
	        var exportFilePath = Path.Combine(extractPath, Path.GetFileName(mduPath));
	        try
	        {
	            //Import model
	            var model = ImportModelAndCheckNotNull(relativeMduPath, localZipPath, extractPath);
                
	            //Export model into new location
	            Assert.IsFalse(File.Exists(exportFilePath));
	            model.ExportTo(exportFilePath);
	            Assert.IsTrue(File.Exists(exportFilePath));

	            //Load exported model
	            var exportedModel = new WaterFlowFMModel(exportFilePath);
	            Assert.IsNotNull(exportedModel);
	        }
	        finally
	        {
	            FileUtils.DeleteIfExists(exportFilePath);
	            FileUtils.DeleteIfExists(extractPath);
	        }
	    }

	    [TestCaseSource(nameof(BasicOperationsCases))]
	    [Category(TestCategory.WindowsForms)]
	    public void ImportModel_SaveAsProject_WithGui(string relativeZipUrl, string relativeMduPath)
	    {
	        var localZipPath = GetLocalZipPath(relativeZipUrl);

	        Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

	        var extractPath = GetZipExtractPath(localZipPath);
	        var dsProjPath = Path.Combine(extractPath, "Project1.dsproj");
	        try
	        {
	            ExtractZipToCleanDirectoryAndCheckContent(localZipPath, extractPath);

	            var mduPath = Path.Combine(extractPath, relativeMduPath);

	            Assert.IsTrue(File.Exists(mduPath), $"File does not exist: {mduPath}");
	     

	            using (var gui = new DeltaShellGui())
	            {
	                //load the plugins
	                var app = gui.Application;
                    AddAppAndGuiPlugins(app, gui);
	                gui.Run();

	                Action mainWindowShown = delegate
	                {
	                    CheckModelExistsOrNot(app.Project.RootFolder, false);
                        ImportModel_IntoProject_SaveProject_ReloadProject_CheckModel(relativeMduPath, localZipPath, extractPath, app, dsProjPath, () => OpenFmWindow(gui, app.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault()));
                    };

	                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
	            }
	        }
	        finally
	        {
	            FileUtils.DeleteIfExists(extractPath);
	        }
	    }

	    [Test]
	    [TestCaseSource(nameof(BasicOperationsCases))]
	    public void ImportModel_SaveAsProject_ReloadProject(string relativeZipUrl, string relativeMduPath)
	    {
	        var localZipPath = GetLocalZipPath(relativeZipUrl);

	        Assert.IsTrue(File.Exists(localZipPath), $"The zip file {relativeZipUrl} was not found, please make sure it has been included in the ZipModelsList list to be downloaded.");

	        var extractPath = GetZipExtractPath(localZipPath);
	        var dsProjPath = Path.Combine(extractPath, "Project1.dsproj");
	        try
	        {
	            using (var app = new DeltaShellApplication {IsProjectCreatedInTemporaryDirectory = true})
	            {
	                AddAppPlugins(app);
	                app.Run();

	                CheckModelExistsOrNot(app.Project.RootFolder, false);
                    ImportModel_IntoProject_SaveProject_ReloadProject_CheckModel(relativeMduPath, localZipPath, extractPath, app, dsProjPath);
	            }
	        }
	        finally
	        {
	            FileUtils.DeleteIfExists(extractPath);
	        }
	    }

	    #endregion

	    #endregion
	}
}
