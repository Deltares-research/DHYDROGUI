using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.Toolbox;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelApplicationPluginTest
    {
        [Test]
        public void DefaultConstructorExpectedValuesTest()
        {
            // setup

            // call
            var appPlugin = new WaterQualityModelApplicationPlugin();

            // assert
            Assert.IsInstanceOf<IDataAccessListenersProvider>(appPlugin);
            Assert.AreEqual("Water quality model", appPlugin.Name,
                "Name change detected, which impacts NHibernate persistency.");
            Assert.AreEqual("Allows to simulate water quality in rivers and channels.", appPlugin.Description);
            var expectedVersionString = appPlugin.GetType().Assembly.GetName().Version.ToString();
            Assert.AreEqual(expectedVersionString, appPlugin.Version);
        }
        
        [Test]
        public void CreateDataAccessListenersTest()
        {
            // setup
            var appPlugin = new WaterQualityModelApplicationPlugin();

            // call
            var listerners = appPlugin.CreateDataAccessListeners().ToArray();

            // assert
            Assert.AreEqual(1, listerners.Count());
            Assert.IsInstanceOf<WaterQualityModelDataAccessListener>(listerners[0]);
        }

        [Test]
        public void GetFileImportersTest()
        {
            // setup
            var plugin = new WaterQualityModelApplicationPlugin();

            // call
            var importers = plugin.GetFileImporters().ToArray();

            // assert
            Assert.IsTrue(importers.Any(i => i is SubFileImporter));
            Assert.IsTrue(importers.Any(i => i is HydFileImporter));
            Assert.IsTrue(importers.Any(i => i is LoadsImporter));
            Assert.IsTrue(importers.Any(i => i is ObservationPointImporter));
            Assert.IsTrue(importers.Any(i => i is DataTableImporter));
            Assert.IsTrue(importers.Any(i => i is WaterQualityObservationAreaImporter));
        }

        [Test]
        public void GivenAModel_WhenModelIsRenamed_DataDirectoryPathIsChanged()
        {
            using (var app = new DeltaShellApplication())
            {
                var waqPlugin = new WaterQualityModelApplicationPlugin();
                var waqModel = new WaterQualityModel()
                {
                    Name = "WAQ1",
                   
                };

                app.Plugins.Add(waqPlugin);
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new ToolboxApplicationPlugin());
                app.Run();

                var tempDirectory = FileUtils.CreateTempDirectory();
                app.SaveProjectAs(Path.Combine(tempDirectory, "WAQ_proj"));

                app.Project.RootFolder.Items.Add(waqModel);
              
                var originalOutputDirectory = waqModel.ModelSettings.OutputDirectory;
                var originalDataDirectory = waqModel.ModelDataDirectory;
                Assert.That(Path.Combine(tempDirectory, "WAQ_proj_data\\WAQ1\\output"), Is.EqualTo(originalOutputDirectory));
                Assert.That(Path.Combine(tempDirectory, "WAQ_proj_data\\WAQ1"), Is.EqualTo(originalDataDirectory));

                waqModel.Name = "WAQ2";
                var newOutputDirectory = waqModel.ModelSettings.OutputDirectory;
                var newDataDirectory = waqModel.ModelDataDirectory;
                Assert.That(Path.Combine(tempDirectory, "WAQ_proj_data\\WAQ2\\output"), Is.EqualTo(newOutputDirectory));
                Assert.That(Path.Combine(tempDirectory, "WAQ_proj_data\\WAQ2"), Is.EqualTo(newDataDirectory));
            }
        }


        [Test]
        public void ImportCorrectSubFileAndThenCorruptItAndExpectExceptionMessage()
        {
            var app = new DeltaShellApplication();
            var waqModel = new WaterQualityModel();
            PerformActionOnWesternscheldWithRunningApplication(app, waqModel, () =>
            {
                var boundaryDataTableFilePath = Path.Combine(waqModel.BoundaryDataManager.FolderPath, "bacteria.tbl");
                Assert.True(File.Exists(boundaryDataTableFilePath));

                // Simulate corruption of boundary table data
                using (var sw = File.AppendText(boundaryDataTableFilePath))
                {
                    sw.WriteLine("The Corruption");
                    sw.WriteLine("Spreads in this file");
                }

                var expectedExceptionMsg =
                    string.Format(Resources.WaterQualityModel_OnInitializeCore_Failed_to_initialize_pre_processor__0_Please_look_at_the_List_file_for_more_information__0_List_file_found_in__Project_view____Output____List_file__0___1_,
                        Environment.NewLine, waqModel.ExplicitOutputDirectory);

                //Expect the exception message thrown as log message
                TestHelper.AssertAtLeastOneLogMessagesContains(() => ActivityRunner.RunActivity(waqModel), expectedExceptionMsg);
            });
        }
        
        [Test]
        [TestCase("deltashell.lst")]
        [TestCase("deltashell.lsp")]
        [TestCase("deltashell.mon")]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.DataAccess)]
        public void Check_When_RunningTwice_WaqModel_OutputFiles_AreNot_Duplicated(string outputFile)
        {
            var app = new DeltaShellApplication();
            var waqModel = new WaterQualityModel();
            PerformActionOnWesternscheldWithRunningApplication(app, waqModel, () =>
            {
                //First run
                ActivityRunner.RunActivity(waqModel);
                CheckDataItems(outputFile, waqModel);

                //Second run
                ActivityRunner.RunActivity(waqModel);
                CheckDataItems(outputFile, waqModel);
            });
        }

        [Test]
        [TestCase("deltashell.lst")]
        [TestCase("deltashell.lsp")]
        [TestCase("deltashell.mon")]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.DataAccess)]
        public void Check_When_RunningTwice_WaqModel_OutputFiles_And_Saving_TheFilesArePersisted(string outputFileName)
        {
            var app = new DeltaShellApplication();
            var waqModel = new WaterQualityModel();
            PerformActionOnWesternscheldWithRunningApplication(app, waqModel, () =>
            {
                //First run
                ActivityRunner.RunActivity(waqModel);

                //save the project
                app.SaveProject();
                //Second run
                ActivityRunner.RunActivity(waqModel);
                CheckDataItems(outputFileName, waqModel);
            });
        }

        private static void PerformActionOnWesternscheldWithRunningApplication(DeltaShellApplication app, WaterQualityModel waqModel, Action actionToPerform)
        {
            var testDir = FileUtils.CreateTempDirectory();
            var originalDir = TestHelper.GetTestFilePath("WaterQualityDataFiles");
            FileUtils.CopyAll(new DirectoryInfo(originalDir), new DirectoryInfo(testDir), string.Empty);

            var hydFilePath = Path.Combine(testDir, "flow-model", "westernscheldt01.hyd");
            Assert.True(File.Exists(hydFilePath));

            var subFilePath = Path.Combine(testDir, "waq", "sub-files", "bacteria.sub");
            Assert.True(File.Exists(subFilePath));

            var boundaryConditionsFilePath = Path.Combine(testDir, "waq", "boundary-conditions", "bacteria.csv");
            Assert.True(File.Exists(boundaryConditionsFilePath));

            try
            {
                using (app)
                {
                    //Declaring plugins.
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new NetCdfApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new ScriptingApplicationPlugin());
                    app.Plugins.Add(new ToolboxApplicationPlugin());

                    app.Run();

                    app.SaveProjectAs(Path.Combine(testDir, "WAQ_proj"));

                    app.Project.RootFolder.Items.Add(waqModel);

                    new HydFileImporter().ImportItem(hydFilePath, waqModel);
                    new SubFileImporter().Import(waqModel.SubstanceProcessLibrary, subFilePath);
                    new DataTableImporter().ImportItem(boundaryConditionsFilePath, waqModel.BoundaryDataManager);

                    actionToPerform.Invoke();
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        private static void CheckDataItems(string outputFileName, WaterQualityModel waqModel)
        {
            //Check data items
            var filePaths = GetFilePaths(waqModel);

            Assert.IsTrue(filePaths.Any( fp => Path.GetFileName(fp) == outputFileName),
                string.Format("OutputFile: {0} not found in dataItems {1}", outputFileName, string.Join(", ", filePaths)));

            //Check the file paths exist.
            foreach (var filePath in filePaths)
            {
                Assert.IsTrue(File.Exists(filePath));
            }
        }

        private static IList<string> GetFilePaths(WaterQualityModel waqModel)
        {
            var dataItems = waqModel.DataItems.Where(di =>
                di.Role == DataItemRole.Output &&
                di.ValueType == typeof(TextDocumentFromFile)).ToList();
            Assert.IsTrue(dataItems.Any());

            Assert.AreEqual(3, dataItems.Count);
            var filePaths = dataItems.Select(di => ((TextDocumentFromFile) di.Value).Path).ToList();
            return filePaths;
        }
    }
}